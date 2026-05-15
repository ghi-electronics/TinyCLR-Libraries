using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.I2c {
    /// <summary>
    /// Represents an I²C bus controller. Open a peer with <see cref="GetDevice(I2cConnectionSettings)"/>
    /// to transact with a specific slave address. The same controller can serve
    /// multiple slaves — settings are re-applied per transfer.
    /// </summary>
    public class I2cController : IDisposable {
        /// <summary>The low-level provider backing this controller.</summary>
        public II2cControllerProvider Provider { get; }

        /// <summary>Maximum time the controller will block on a single transfer before giving up.</summary>
        public TimeSpan Timeout {
            get => this.Provider.Timeout;
            set => this.Provider.Timeout = value;
        }

        private I2cController(II2cControllerProvider provider) => this.Provider = provider;

        /// <summary>Returns the default I²C controller for this device.</summary>
        public static I2cController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.I2cController) is I2cController c ? c : I2cController.FromName(NativeApi.GetDefaultName(NativeApiType.I2cController));
        /// <summary>Returns an I²C controller identified by its native API name.</summary>
        public static I2cController FromName(string name) => FromProvider(new I2cControllerApiWrapper(NativeApi.Find(name, NativeApiType.I2cController)));
        /// <summary>Returns a software (bit-bang) I²C controller using the supplied SDA/SCL pins.</summary>
        public static I2cController FromName(string name, GpioPin sdaPin, GpioPin sclPin) => FromName(name, sdaPin, sclPin, false);
        /// <summary>Returns a software I²C controller, optionally engaging the internal pull-ups.</summary>
        /// <param name="name">Must be the SoftwareI2cController native API name.</param>
        /// <param name="sdaPin">Pin used as SDA.</param>
        /// <param name="sclPin">Pin used as SCL.</param>
        /// <param name="usePullups">When true, configures SDA/SCL as inputs with internal pull-ups.</param>
        public static I2cController FromName(string name, GpioPin sdaPin, GpioPin sclPin, bool usePullups) {
            if (name.CompareTo("GHIElectronics.TinyCLR.NativeApis.SoftwareI2cController") != 0)
                throw new ArgumentException("Invalid controller.");

            return FromProvider(new I2cControllerSoftwareProvider(sdaPin, sclPin, usePullups));
        }
        /// <summary>Creates a controller from a custom <see cref="II2cControllerProvider"/>.</summary>
        public static I2cController FromProvider(II2cControllerProvider provider) {
            var c = new I2cController(provider) {
                Timeout = TimeSpan.FromSeconds(2)
            };

            return c;
        }

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>Creates an <see cref="I2cDevice"/> bound to this controller using the supplied settings.</summary>
        /// <param name="connectionSettings">Slave address, bus speed, and master/slave mode.</param>
        public I2cDevice GetDevice(I2cConnectionSettings connectionSettings) {
            var device = new I2cDevice(this, connectionSettings);

            if (connectionSettings.Mode == I2cMode.Slave) {

                this.Provider.SetActiveSettings(device.ConnectionSettings);

                if (this.Provider.ReadBufferSize == 0)
                    this.Provider.ReadBufferSize = 256;
                if (this.Provider.WriteBufferSize == 0)
                    this.Provider.WriteBufferSize = 256;
            }

            return device;
        }

        internal void SetActive(I2cDevice device) => this.Provider.SetActiveSettings(device.ConnectionSettings);

        /// <summary>Slave-mode only: empties the controller's outgoing buffer.</summary>
        public void ClearWriteBuffer() => this.Provider.ClearWriteBuffer();
        /// <summary>Slave-mode only: empties the controller's incoming buffer.</summary>
        public void ClearReadBuffer() => this.Provider.ClearReadBuffer();

        /// <summary>Slave-mode only: size in bytes of the controller's outgoing buffer.</summary>
        public int WriteBufferSize { get => this.Provider.WriteBufferSize; set => this.Provider.WriteBufferSize = value; }
        /// <summary>Slave-mode only: size in bytes of the controller's incoming buffer.</summary>
        public int ReadBufferSize { get => this.Provider.ReadBufferSize; set => this.Provider.ReadBufferSize = value; }
        /// <summary>Slave-mode only: bytes currently queued to transmit.</summary>
        public int BytesToWrite => this.Provider.BytesToWrite;
        /// <summary>Slave-mode only: bytes currently available to read.</summary>
        public int BytesToRead => this.Provider.BytesToRead;


        internal static string MasterNotSupported = "Not supported in master mode.";
    }

    /// <summary>
    /// Represents a single slave on the I²C bus. Master-mode devices use the
    /// blocking <see cref="Read(byte[])"/> / <see cref="Write(byte[])"/> / <see cref="WriteRead(byte[],byte[])"/>
    /// family; slave-mode devices additionally expose <see cref="FrameReceived"/>
    /// and <see cref="ErrorReceived"/> events.
    /// </summary>
    public class I2cDevice : IDisposable {
        private static object ojectLocker = new object();
        /// <summary>The per-device connection settings.</summary>
        public I2cConnectionSettings ConnectionSettings { get; }
        /// <summary>The <see cref="I2cController"/> that owns this device.</summary>
        public I2cController Controller { get; }

        private FrameReceivedEventHandler frameReceivedCallbacks;
        private ErrorReceivedEventHandler errorReceivedCallbacks;

        internal I2cDevice(I2cController controller, I2cConnectionSettings connectionSettings) {
            this.ConnectionSettings = connectionSettings;
            this.Controller = controller;
        }

        /// <summary>Releases device-level resources.</summary>
        public void Dispose() {

        }

        /// <summary>Reads <paramref name="buffer"/>.Length bytes from the slave.</summary>
        public void Read(byte[] buffer) => this.WriteRead(null, 0, 0, buffer, 0, buffer.Length);
        /// <summary>Writes <paramref name="buffer"/>.Length bytes to the slave.</summary>
        public void Write(byte[] buffer) => this.WriteRead(buffer, 0, buffer.Length, null, 0, 0);
        /// <summary>Performs a register-style write-then-read transaction.</summary>
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer) => this.WriteRead(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);
        /// <summary>Reads <paramref name="length"/> bytes into <paramref name="buffer"/> starting at <paramref name="offset"/>.</summary>
        public void Read(byte[] buffer, int offset, int length) => this.WriteRead(null, 0, 0, buffer, offset, length);
        /// <summary>Writes <paramref name="length"/> bytes from <paramref name="buffer"/> starting at <paramref name="offset"/>.</summary>
        public void Write(byte[] buffer, int offset, int length) => this.WriteRead(buffer, offset, length, null, 0, 0);

        /// <summary>
        /// Performs a write-then-read transaction with explicit slice offsets and lengths.
        /// Throws when the slave NACKs or the transfer is otherwise incomplete (master mode only).
        /// </summary>
        public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            lock (ojectLocker) {
                this.Controller.SetActive(this);

                if (this.Controller.Provider.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out _, out _) != I2cTransferStatus.FullTransfer)
                    if (this.ConnectionSettings.Mode != I2cMode.Slave)
                        throw new InvalidOperationException();
            }
        }

        /// <summary>Like <see cref="Read(byte[])"/> but returns a status + count instead of throwing on a partial transfer.</summary>
        public I2cTransferResult ReadPartial(byte[] buffer) =>
            // GHI Changed: 5/5/2022 ???
            // if (this.ConnectionSettings.Mode != I2cMode.Slave)
            //    throw new NotSupportedException(I2cController.MasterNotSupported);

            this.WriteReadPartial(null, 0, 0, buffer, 0, buffer.Length);
        /// <summary>Like <see cref="Write(byte[])"/> but returns a status + count instead of throwing on a partial transfer.</summary>
        public I2cTransferResult WritePartial(byte[] buffer) =>
            this.WriteReadPartial(buffer, 0, buffer.Length, null, 0, 0);
        /// <summary>Like <see cref="WriteRead(byte[],byte[])"/> but returns a status + counts.</summary>
        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) =>
            this.WriteReadPartial(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);

        /// <summary>Partial read with explicit slice offsets and lengths.</summary>
        public I2cTransferResult ReadPartial(byte[] buffer, int offset, int length) =>
            this.WriteReadPartial(null, 0, 0, buffer, offset, length);
        /// <summary>Partial write with explicit slice offsets and lengths.</summary>
        public I2cTransferResult WritePartial(byte[] buffer, int offset, int length) =>
            this.WriteReadPartial(buffer, offset, length, null, 0, 0);

        /// <summary>Partial write-then-read with explicit slice offsets and lengths.</summary>
        /// <returns>A <see cref="I2cTransferResult"/> with the transfer status and byte counts.</returns>
        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            lock (ojectLocker) {
                this.Controller.SetActive(this);

                var res = this.Controller.Provider.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out var written, out var read);

                return new I2cTransferResult(res, written, read);
            }
        }

        private void OnFrameReceived(I2cDevice sender, FrameEventArgs e) {
            if (e.Address == this.ConnectionSettings.SlaveAddress)
                this.frameReceivedCallbacks?.Invoke(this, e);
        }
        private void OnErrorReceived(I2cDevice sender, ErrorReceivedEventArgs e) {
            if (e.Address == this.ConnectionSettings.SlaveAddress)
                this.errorReceivedCallbacks?.Invoke(this, e);
        }

        /// <summary>Slave-mode only: raised when a master start/stop or data frame is observed addressed to this slave.</summary>
        public event FrameReceivedEventHandler FrameReceived {
            add {
                if (this.ConnectionSettings.Mode != I2cMode.Slave)
                    throw new NotSupportedException(I2cController.MasterNotSupported);

                if (this.frameReceivedCallbacks == null)
                    this.Controller.Provider.FrameReceived += this.OnFrameReceived;

                this.frameReceivedCallbacks += value;
            }
            remove {
                if (this.ConnectionSettings.Mode != I2cMode.Slave)
                    throw new NotSupportedException(I2cController.MasterNotSupported);

                this.frameReceivedCallbacks -= value;

                if (this.frameReceivedCallbacks == null)
                    this.Controller.Provider.FrameReceived -= this.OnFrameReceived;
            }
        }

        /// <summary>Slave-mode only: raised when the controller detects a bus error (overrun, arbitration loss, etc.).</summary>
        public event ErrorReceivedEventHandler ErrorReceived {
            add {
                if (this.ConnectionSettings.Mode != I2cMode.Slave)
                    throw new NotSupportedException(I2cController.MasterNotSupported);

                if (this.errorReceivedCallbacks == null)
                    this.Controller.Provider.ErrorReceived += this.OnErrorReceived;

                this.errorReceivedCallbacks += value;
            }
            remove {
                if (this.ConnectionSettings.Mode != I2cMode.Slave)
                    throw new NotSupportedException(I2cController.MasterNotSupported);

                this.errorReceivedCallbacks -= value;

                if (this.errorReceivedCallbacks == null)
                    this.Controller.Provider.ErrorReceived -= this.OnErrorReceived;
            }
        }

        /// <summary>Slave-mode only: empties the controller's outgoing buffer.</summary>
        public void ClearWriteBuffer() {
            if (this.ConnectionSettings.Mode != I2cMode.Slave)
                throw new NotSupportedException(I2cController.MasterNotSupported);

            this.Controller.Provider.ClearWriteBuffer();
        }
        /// <summary>Slave-mode only: empties the controller's incoming buffer.</summary>
        public void ClearReadBuffer() {
            if (this.ConnectionSettings.Mode != I2cMode.Slave)
                throw new NotSupportedException(I2cController.MasterNotSupported);

            this.Controller.Provider.ClearReadBuffer();
        }

        /// <summary>Slave-mode only: size in bytes of the controller's outgoing buffer.</summary>
        public int WriteBufferSize {
            get {
                if (this.ConnectionSettings.Mode != I2cMode.Slave)
                    throw new NotSupportedException(I2cController.MasterNotSupported);

                return this.Controller.Provider.WriteBufferSize;
            }
        }
        /// <summary>Slave-mode only: size in bytes of the controller's incoming buffer.</summary>
        public int ReadBufferSize {
            get {
                if (this.ConnectionSettings.Mode != I2cMode.Slave)
                    throw new NotSupportedException(I2cController.MasterNotSupported);

                return this.Controller.Provider.ReadBufferSize;
            }
        }
        /// <summary>Slave-mode only: bytes currently queued to transmit.</summary>
        public int BytesToWrite {
            get {
                if (this.ConnectionSettings.Mode != I2cMode.Slave)
                    throw new NotSupportedException(I2cController.MasterNotSupported);

                return this.Controller.Provider.BytesToWrite;
            }
        }

        /// <summary>Slave-mode only: bytes currently available to read.</summary>
        public int BytesToRead {
            get {
                if (this.ConnectionSettings.Mode != I2cMode.Slave)
                    throw new NotSupportedException(I2cController.MasterNotSupported);

                return this.Controller.Provider.BytesToRead;
            }
        }
    }

    /// <summary>Per-device I²C connection settings: address, bus speed, and master/slave role.</summary>
    public sealed class I2cConnectionSettings {
        /// <summary>The peer's slave address.</summary>
        public int SlaveAddress { get; set; }
        /// <summary>7-bit or 10-bit address format.</summary>
        public I2cAddressFormat AddressFormat { get; set; }
        /// <summary>Bus speed in Hz (e.g. 100_000 for standard, 400_000 for fast mode).</summary>
        public uint BusSpeed { get; set; }
        /// <summary>Whether this device acts as master or slave.</summary>
        public I2cMode Mode { get; set; }

        /// <summary>Slave-mode only: allow the controller to stretch the clock while it's not ready.</summary>
        public bool EnableClockStretching { get; set; }

        /// <summary>Builds a 7-bit master-mode settings object at standard speed.</summary>
        public I2cConnectionSettings(int slaveAddress) : this(slaveAddress, I2cAddressFormat.SevenBit) {

        }

        /// <summary>Builds a 7-bit master-mode settings object at the given bus speed.</summary>
        public I2cConnectionSettings(int slaveAddress, uint busSpeed) : this(slaveAddress, I2cAddressFormat.SevenBit, busSpeed) {

        }

        /// <summary>Builds a master-mode settings object with explicit address format.</summary>
        public I2cConnectionSettings(int slaveAddress, I2cAddressFormat addressFormat, uint busSpeed = 100000) : this(slaveAddress, I2cMode.Master, addressFormat, busSpeed) {

        }

        /// <summary>Builds a 7-bit settings object with the given role (master/slave).</summary>
        public I2cConnectionSettings(int slaveAddress, I2cMode mode) : this(slaveAddress, mode, I2cAddressFormat.SevenBit) {

        }

        /// <summary>Builds a settings object with explicit role and address format.</summary>
        public I2cConnectionSettings(int slaveAddress, I2cMode mode, I2cAddressFormat addressFormat, uint busSpeed = 100000) : this(slaveAddress, mode, addressFormat, busSpeed, false) {

        }

        /// <summary>Builds a settings object with full control over every field.</summary>
        public I2cConnectionSettings(int slaveAddress, I2cMode mode, I2cAddressFormat addressFormat, uint busSpeed, bool enableClockStretching = false) {
            this.SlaveAddress = slaveAddress;
            this.AddressFormat = addressFormat;
            this.BusSpeed = busSpeed;
            this.Mode = mode;
            this.EnableClockStretching = enableClockStretching;
        }
    }

    /// <summary>I²C address width.</summary>
    public enum I2cAddressFormat {
        /// <summary>Standard 7-bit slave address.</summary>
        SevenBit = 0,
        /// <summary>Extended 10-bit slave address.</summary>
        TenBit = 1,
    }

    /// <summary>Bus role for an <see cref="I2cDevice"/>.</summary>
    public enum I2cMode {
        /// <summary>This endpoint is the master (originates clock and transactions).</summary>
        Master = 0,
        /// <summary>This endpoint is the slave (responds to addressed transactions).</summary>
        Slave = 1
    }

    /// <summary>Outcome of an I²C transfer.</summary>
    public enum I2cTransferStatus {
        /// <summary>All requested bytes were transferred successfully.</summary>
        FullTransfer = 0,
        /// <summary>Some, but not all, requested bytes were transferred.</summary>
        PartialTransfer = 1,
        /// <summary>The slave did not acknowledge its address.</summary>
        SlaveAddressNotAcknowledged = 2,
        /// <summary>The slave held the clock low past the configured timeout.</summary>
        ClockStretchTimeout = 3,
    }

    /// <summary>Bus errors reported via <see cref="I2cDevice.ErrorReceived"/>.</summary>
    public enum I2cError {
        /// <summary>Receive overrun.</summary>
        Overrun = 0,
        /// <summary>Generic bus error.</summary>
        Bus = 1,
        /// <summary>Lost arbitration to another master.</summary>
        ArbitrationLoss = 2,
        /// <summary>Internal buffer full.</summary>
        BufferFull = 3
    }

    /// <summary>Master-initiated transaction kind observed by a slave.</summary>
    public enum I2cTransaction {
        /// <summary>Master is writing to this slave.</summary>
        MasterWrite = 0,
        /// <summary>Master is reading from this slave.</summary>
        MasterRead = 1,
        /// <summary>Master issued a stop condition.</summary>
        MasterStop = 2
    }

    /// <summary>Arguments for <see cref="I2cDevice.FrameReceived"/>.</summary>
    public sealed class FrameEventArgs {
        /// <summary>Driver-captured time of the frame.</summary>
        public DateTime Timestamp { get; }
        /// <summary>Number of bytes associated with this frame.</summary>
        public uint DataCount { get; }

        /// <summary>The address that was acknowledged.</summary>
        public uint Address { get; }

        /// <summary>The kind of master-initiated transaction observed.</summary>
        public I2cTransaction Event { get; }

        internal FrameEventArgs(I2cTransaction e, uint address, uint count, DateTime timestamp) {
            this.Address = address;
            this.DataCount = count;
            this.Timestamp = timestamp;
            this.Event = e;
        }
    }

    /// <summary>Arguments for <see cref="I2cDevice.ErrorReceived"/>.</summary>
    public sealed class ErrorReceivedEventArgs {
        /// <summary>Driver-captured time of the error.</summary>
        public DateTime Timestamp { get; }
        /// <summary>The kind of error detected.</summary>
        public I2cError Error { get; }

        /// <summary>The address (if any) the error was associated with.</summary>
        public uint Address { get; }
        internal ErrorReceivedEventArgs(uint address, I2cError error, DateTime timestamp) {
            this.Address = address;
            this.Error = error;
            this.Timestamp = timestamp;
        }
    }

    /// <summary>Outcome of a partial-transfer call (<see cref="I2cDevice.ReadPartial(byte[])"/> and friends).</summary>
    public struct I2cTransferResult {
        /// <summary>Whether the transfer was full, partial, NAK'd, or timed out.</summary>
        public I2cTransferStatus Status { get; }
        /// <summary>Bytes actually written.</summary>
        public int BytesWritten { get; }
        /// <summary>Bytes actually read.</summary>
        public int BytesRead { get; }

        /// <summary>Sum of <see cref="BytesWritten"/> and <see cref="BytesRead"/>.</summary>
        public int BytesTransferred => this.BytesWritten + this.BytesRead;

        internal I2cTransferResult(I2cTransferStatus status, int bytesWritten, int bytesRead) {
            this.Status = status;
            this.BytesWritten = bytesWritten;
            this.BytesRead = bytesRead;
        }
    }

    /// <summary>Handler signature for <see cref="I2cDevice.FrameReceived"/>.</summary>
    public delegate void FrameReceivedEventHandler(I2cDevice sender, FrameEventArgs e);
    /// <summary>Handler signature for <see cref="I2cDevice.ErrorReceived"/>.</summary>
    public delegate void ErrorReceivedEventHandler(I2cDevice sender, ErrorReceivedEventArgs e);

    namespace Provider {
        /// <summary>Provider contract for an I²C controller.</summary>
        public interface II2cControllerProvider : IDisposable {
            /// <summary>Slave-mode only: size in bytes of the controller's outgoing buffer.</summary>
            int WriteBufferSize { get; set; }
            /// <summary>Slave-mode only: size in bytes of the controller's incoming buffer.</summary>
            int ReadBufferSize { get; set; }
            /// <summary>Slave-mode only: bytes currently queued to transmit.</summary>
            int BytesToWrite { get; }
            /// <summary>Slave-mode only: bytes currently available to read.</summary>
            int BytesToRead { get; }
            /// <summary>Slave-mode only: empties the controller's outgoing buffer.</summary>
            void ClearWriteBuffer();
            /// <summary>Slave-mode only: empties the controller's incoming buffer.</summary>
            void ClearReadBuffer();
            /// <summary>Applies the given settings before the next transfer.</summary>
            void SetActiveSettings(I2cConnectionSettings connectionSettings);
            /// <summary>Performs a write-then-read transaction.</summary>
            I2cTransferStatus WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int written, out int read);

            /// <summary>Slave-mode only: raised when a master frame addressed to this slave is observed.</summary>
            event FrameReceivedEventHandler FrameReceived;
            /// <summary>Slave-mode only: raised when the controller detects a bus error.</summary>
            event ErrorReceivedEventHandler ErrorReceived;

            /// <summary>Maximum time the controller will block on a single transfer.</summary>
            TimeSpan Timeout { get; set; }
        }

        /// <summary>Concrete <see cref="II2cControllerProvider"/> backed by the native TinyCLR I²C HAL.</summary>
        public sealed class I2cControllerApiWrapper : II2cControllerProvider {
            private readonly IntPtr impl;

            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            private FrameReceivedEventHandler frameReceivedCallbacks;
            private ErrorReceivedEventHandler errorReceivedCallbacks;

            private readonly NativeEventDispatcher frameReceivedDispatcher;
            private readonly NativeEventDispatcher errorReceivedDispatcher;

            /// <summary>Wraps the given native API as a provider.</summary>
            public I2cControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.frameReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.I2c.FrameReceived");
                this.errorReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.I2c.ErrorReceived");

                this.frameReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.frameReceivedCallbacks?.Invoke(null, new FrameEventArgs((I2cTransaction)d0, (uint)d1, (uint)d2, ts)); };
                this.errorReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.errorReceivedCallbacks?.Invoke(null, new ErrorReceivedEventArgs((uint)d0, (I2cError)d1, ts)); };
            }

            /// <summary>Releases the native controller.</summary>
            public void Dispose() => this.Release();

            /// <inheritdoc/>
            public event FrameReceivedEventHandler FrameReceived {
                add {
                    if (this.frameReceivedCallbacks == null)
                        this.SetFrameReceivedEventEnabled(true);

                    this.frameReceivedCallbacks += value;
                }
                remove {
                    this.frameReceivedCallbacks -= value;

                    if (this.frameReceivedCallbacks == null)
                        this.SetFrameReceivedEventEnabled(false);
                }
            }

            /// <inheritdoc/>
            public event ErrorReceivedEventHandler ErrorReceived {
                add {
                    if (this.errorReceivedCallbacks == null)
                        this.SetErrorReceivedEventEnabled(true);

                    this.errorReceivedCallbacks += value;
                }
                remove {
                    this.errorReceivedCallbacks -= value;

                    if (this.errorReceivedCallbacks == null)
                        this.SetErrorReceivedEventEnabled(false);
                }
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetActiveSettings(I2cConnectionSettings connectionSettings);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern I2cTransferStatus WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int written, out int read);

            /// <inheritdoc/>
            public extern int WriteBufferSize { [MethodImpl(MethodImplOptions.InternalCall)]  get; [MethodImpl(MethodImplOptions.InternalCall)]  set; }

            /// <inheritdoc/>
            public extern int ReadBufferSize { [MethodImpl(MethodImplOptions.InternalCall)]  get; [MethodImpl(MethodImplOptions.InternalCall)]  set; }

            /// <inheritdoc/>
            public extern int BytesToWrite { [MethodImpl(MethodImplOptions.InternalCall)]  get; }

            /// <inheritdoc/>
            public extern int BytesToRead { [MethodImpl(MethodImplOptions.InternalCall)]  get; }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearWriteBuffer();
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearReadBuffer();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetFrameReceivedEventEnabled(bool enabled);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetErrorReceivedEventEnabled(bool enabled);

            /// <inheritdoc/>
            public TimeSpan Timeout { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

        }

        /// <summary>
        /// Software (bit-bang) I²C provider. Used when no hardware I²C peripheral is
        /// available on the desired SDA/SCL pins. Master mode only.
        /// </summary>
        internal sealed class I2cControllerSoftwareProvider : II2cControllerProvider {
            private readonly bool usePullups;
            private readonly GpioPin sda;
            private readonly GpioPin scl;
            private byte writeAddress;
            private byte readAddress;
            private bool start;

            /// <inheritdoc/>
            public event ErrorReceivedEventHandler ErrorReceived {
                add {
                    throw new NotSupportedException(I2cController.MasterNotSupported);
                }
                remove {
                    throw new NotSupportedException(I2cController.MasterNotSupported);
                }
            }
            /// <inheritdoc/>
            public event FrameReceivedEventHandler FrameReceived {
                add {
                    throw new NotSupportedException(I2cController.MasterNotSupported);
                }
                remove {
                    throw new NotSupportedException(I2cController.MasterNotSupported);
                }
            }

            public I2cControllerSoftwareProvider(GpioPin sdaPin, GpioPin sclPin) : this(sdaPin, sclPin, true) { }

            public I2cControllerSoftwareProvider(GpioPin sdaPin, GpioPin sclPin, bool usePullups) {
                this.usePullups = usePullups;

                this.sda = sdaPin;
                this.scl = sclPin;
            }

            public void Dispose() {
                this.sda.Dispose();
                this.scl.Dispose();
            }

            public void SetActiveSettings(I2cConnectionSettings connectionSettings) {
                if (connectionSettings.AddressFormat != I2cAddressFormat.SevenBit) throw new NotSupportedException();
                if (connectionSettings.Mode == I2cMode.Slave) throw new NotSupportedException();

                this.writeAddress = (byte)(connectionSettings.SlaveAddress << 1);
                this.readAddress = (byte)((connectionSettings.SlaveAddress << 1) | 1);
                this.start = false;

                this.ReleaseScl();
                this.ReleaseSda();
            }

            public I2cTransferStatus WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int written, out int read) {
                written = 0;
                read = 0;

                try {
                    var res = this.Write(writeBuffer, writeOffset, writeLength, true, readLength == 0);

                    written = res.BytesWritten;
                    read = res.BytesRead;

                    if (res.Status == I2cTransferStatus.FullTransfer && readLength != 0) {
                        res = this.Read(readBuffer, readOffset, readLength, true, true);

                        written += res.BytesWritten;
                        read += res.BytesRead;
                    }

                    this.ReleaseScl();
                    this.ReleaseSda();

                    return res.Status;

                }
                catch (I2cClockStretchTimeoutException) {
                    return I2cTransferStatus.ClockStretchTimeout;
                }
            }

            private I2cTransferResult Write(byte[] buffer, int offset, int length, bool sendStart, bool sendStop) {
                if (!this.Send(sendStart, length == 0, this.writeAddress))
                    return new I2cTransferResult(I2cTransferStatus.SlaveAddressNotAcknowledged, 0, 0);

                for (var i = 0; i < length; i++)
                    if (!this.Send(false, i == length - 1 && sendStop, buffer[i + offset]))
                        return new I2cTransferResult(I2cTransferStatus.PartialTransfer, i, 0);

                return new I2cTransferResult(I2cTransferStatus.FullTransfer, length, 0);
            }

            private I2cTransferResult Read(byte[] buffer, int offset, int length, bool sendStart, bool sendStop) {
                if (!this.Send(sendStart, length == 0, this.readAddress))
                    return new I2cTransferResult(I2cTransferStatus.SlaveAddressNotAcknowledged, 0, 0);

                for (var i = 0; i < length; i++)
                    if (!this.Receive(i < length - 1, i == length - 1 && sendStop, out buffer[i + offset]))
                        return new I2cTransferResult(I2cTransferStatus.PartialTransfer, 0, i);

                return new I2cTransferResult(I2cTransferStatus.FullTransfer, 0, length);
            }

            private void ClearScl() {
                this.scl.SetDriveMode(GpioPinDriveMode.Output);
                this.scl.Write(GpioPinValue.Low);
            }

            private void ClearSda() {
                this.sda.SetDriveMode(GpioPinDriveMode.Output);
                this.sda.Write(GpioPinValue.Low);
            }

            private void ReleaseScl() {
                this.scl.SetDriveMode(this.usePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
                this.ReadScl();
            }

            private void ReleaseSda() {
                this.sda.SetDriveMode(this.usePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
                this.ReadSda();
            }

            private bool ReadScl() {
                this.scl.SetDriveMode(this.usePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
                return this.scl.Read() == GpioPinValue.High;
            }

            private bool ReadSda() {
                this.sda.SetDriveMode(this.usePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
                return this.sda.Read() == GpioPinValue.High;
            }

            private void WaitForScl() {
                const long TimeoutInTicks = 1000 * 1000 * 10; // Timeout: 1 second
                const long DelayInTicks = (1000000 / 2000) * 10; // Max frequency: 2KHz

                var currentTicks = DateTime.Now.Ticks;
                var timeout = true;

                while (DateTime.Now.Ticks - currentTicks < DelayInTicks / 2) ;

                while (timeout && DateTime.Now.Ticks - currentTicks < TimeoutInTicks) {
                    if (this.ReadScl()) timeout = false;
                }

                if (timeout)
                    throw new I2cClockStretchTimeoutException();

                var periodClockInTicks = DateTime.Now.Ticks - currentTicks;

                currentTicks = DateTime.Now.Ticks;

                while (DateTime.Now.Ticks - currentTicks < periodClockInTicks) ;
            }

            private bool WriteBit(bool bit) {
                if (bit)
                    this.ReleaseSda();
                else
                    this.ClearSda();

                this.WaitForScl();

                if (bit && !this.ReadSda())
                    return false;

                this.ClearScl();

                return true;
            }

            private bool ReadBit() {
                this.ReleaseSda();

                this.WaitForScl();

                var bit = this.ReadSda();

                this.ClearScl();

                return bit;
            }

            private bool SendStart() {
                if (this.start) {
                    this.ReleaseSda();

                    this.WaitForScl();
                }

                if (!this.ReadSda())
                    return false;

                this.ClearSda();

                this.ClearScl();

                this.start = true;

                return true;
            }

            private bool SendStop() {
                this.ClearSda();

                this.WaitForScl();

                if (!this.ReadSda())
                    return false;

                this.start = false;

                return true;
            }

            private bool Send(bool sendStart, bool sendStop, byte data) {
                if (sendStart)
                    this.SendStart();

                for (var bit = 0; bit < 8; bit++) {
                    this.WriteBit((data & 0x80) != 0);

                    data <<= 1;
                }

                var nack = this.ReadBit();

                if (sendStop)
                    this.SendStop();

                return !nack;
            }

            private bool Receive(bool sendAck, bool sendStop, out byte data) {
                data = 0;

                for (var bit = 0; bit < 8; bit++)
                    data = (byte)((data << 1) | (this.ReadBit() ? 1 : 0));

                var res = this.WriteBit(!sendAck);

                return (!sendStop || this.SendStop()) && res;
            }

            private class I2cClockStretchTimeoutException : Exception {

            }

            public int WriteBufferSize { get => throw new NotSupportedException(I2cController.MasterNotSupported); set => throw new NotSupportedException(I2cController.MasterNotSupported); }
            public int ReadBufferSize { get => throw new NotSupportedException(I2cController.MasterNotSupported); set => throw new NotSupportedException(I2cController.MasterNotSupported); }
            public int BytesToWrite => throw new NotSupportedException(I2cController.MasterNotSupported);
            public int BytesToRead => throw new NotSupportedException(I2cController.MasterNotSupported);

            public TimeSpan Timeout { get ; set ; }

            public void ClearWriteBuffer() => throw new NotSupportedException(I2cController.MasterNotSupported);
            public void ClearReadBuffer() => throw new NotSupportedException(I2cController.MasterNotSupported);
        }
    }
}
