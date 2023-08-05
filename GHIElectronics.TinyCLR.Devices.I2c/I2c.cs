using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.I2c {
    public class I2cController : IDisposable {
        public II2cControllerProvider Provider { get; }

        public TimeSpan Timeout {
            get => this.Provider.Timeout;
            set => this.Provider.Timeout = value;
        }

        private I2cController(II2cControllerProvider provider) => this.Provider = provider;

        public static I2cController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.I2cController) is I2cController c ? c : I2cController.FromName(NativeApi.GetDefaultName(NativeApiType.I2cController));
        public static I2cController FromName(string name) => FromProvider(new I2cControllerApiWrapper(NativeApi.Find(name, NativeApiType.I2cController)));
        public static I2cController FromName(string name, GpioPin sdaPin, GpioPin sclPin) => FromName(name, sdaPin, sclPin, false);
        public static I2cController FromName(string name, GpioPin sdaPin, GpioPin sclPin, bool usePullups) {
            if (name.CompareTo("GHIElectronics.TinyCLR.NativeApis.SoftwareI2cController") != 0)
                throw new ArgumentException("Invalid controller.");

            return FromProvider(new I2cControllerSoftwareProvider(sdaPin, sclPin, usePullups));
        }
        public static I2cController FromProvider(II2cControllerProvider provider) {
            var c = new I2cController(provider) {
                Timeout = TimeSpan.FromSeconds(2)
            };

            return c;
        }

        public void Dispose() => this.Provider.Dispose();

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

        public void ClearWriteBuffer() => this.Provider.ClearWriteBuffer();
        public void ClearReadBuffer() => this.Provider.ClearReadBuffer();

        public int WriteBufferSize { get => this.Provider.WriteBufferSize; set => this.Provider.WriteBufferSize = value; }
        public int ReadBufferSize { get => this.Provider.ReadBufferSize; set => this.Provider.ReadBufferSize = value; }
        public int BytesToWrite => this.Provider.BytesToWrite;
        public int BytesToRead => this.Provider.BytesToRead;


        internal static string MasterNotSupported = "Not supported in master mode.";
    }

    public class I2cDevice : IDisposable {
        private static object ojectLocker = new object();
        public I2cConnectionSettings ConnectionSettings { get; }
        public I2cController Controller { get; }

        private FrameReceivedEventHandler frameReceivedCallbacks;
        private ErrorReceivedEventHandler errorReceivedCallbacks;

        internal I2cDevice(I2cController controller, I2cConnectionSettings connectionSettings) {
            this.ConnectionSettings = connectionSettings;
            this.Controller = controller;
        }

        public void Dispose() {

        }

        public void Read(byte[] buffer) => this.WriteRead(null, 0, 0, buffer, 0, buffer.Length);
        public void Write(byte[] buffer) => this.WriteRead(buffer, 0, buffer.Length, null, 0, 0);
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer) => this.WriteRead(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);
        public void Read(byte[] buffer, int offset, int length) => this.WriteRead(null, 0, 0, buffer, offset, length);
        public void Write(byte[] buffer, int offset, int length) => this.WriteRead(buffer, offset, length, null, 0, 0);

        public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            lock (ojectLocker) {
                this.Controller.SetActive(this);

                if (this.Controller.Provider.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out _, out _) != I2cTransferStatus.FullTransfer)
                    if (this.ConnectionSettings.Mode != I2cMode.Slave)
                        throw new InvalidOperationException();
            }
        }

        public I2cTransferResult ReadPartial(byte[] buffer) =>
            // GHI Changed: 5/5/2022 ???
            // if (this.ConnectionSettings.Mode != I2cMode.Slave)
            //    throw new NotSupportedException(I2cController.MasterNotSupported);

            this.WriteReadPartial(null, 0, 0, buffer, 0, buffer.Length);
        public I2cTransferResult WritePartial(byte[] buffer) =>
            // GHI Changed: 5/5/2022 ???
            // if (this.ConnectionSettings.Mode != I2cMode.Slave)
            //    throw new NotSupportedException(I2cController.MasterNotSupported);

            this.WriteReadPartial(buffer, 0, buffer.Length, null, 0, 0);
        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) =>
            // GHI Changed: 5/5/2022 ???
            // if (this.ConnectionSettings.Mode != I2cMode.Slave)
            //    throw new NotSupportedException(I2cController.MasterNotSupported);

            this.WriteReadPartial(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);

        public I2cTransferResult ReadPartial(byte[] buffer, int offset, int length) =>
            // GHI Changed: 5/5/2022 ???
            // if (this.ConnectionSettings.Mode != I2cMode.Slave)
            //    throw new NotSupportedException(I2cController.MasterNotSupported);

            this.WriteReadPartial(null, 0, 0, buffer, offset, length);
        public I2cTransferResult WritePartial(byte[] buffer, int offset, int length) =>
            // GHI Changed: 5/5/2022 ???
            // if (this.ConnectionSettings.Mode != I2cMode.Slave)
            //    throw new NotSupportedException(I2cController.MasterNotSupported);

            this.WriteReadPartial(buffer, offset, length, null, 0, 0);

        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            // GHI Changed: 5/5/2022 ???
            // if (this.ConnectionSettings.Mode != I2cMode.Slave)
            //    throw new NotSupportedException(I2cController.MasterNotSupported);

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

        public void ClearWriteBuffer() {
            if (this.ConnectionSettings.Mode != I2cMode.Slave)
                throw new NotSupportedException(I2cController.MasterNotSupported);

            this.Controller.Provider.ClearWriteBuffer();
        }
        public void ClearReadBuffer() {
            if (this.ConnectionSettings.Mode != I2cMode.Slave)
                throw new NotSupportedException(I2cController.MasterNotSupported);

            this.Controller.Provider.ClearReadBuffer();
        }

        public int WriteBufferSize {
            get {
                if (this.ConnectionSettings.Mode != I2cMode.Slave)
                    throw new NotSupportedException(I2cController.MasterNotSupported);

                return this.Controller.Provider.WriteBufferSize;
            }
        }
        public int ReadBufferSize {
            get {
                if (this.ConnectionSettings.Mode != I2cMode.Slave)
                    throw new NotSupportedException(I2cController.MasterNotSupported);

                return this.Controller.Provider.ReadBufferSize;
            }
        }
        public int BytesToWrite {
            get {
                if (this.ConnectionSettings.Mode != I2cMode.Slave)
                    throw new NotSupportedException(I2cController.MasterNotSupported);

                return this.Controller.Provider.BytesToWrite;
            }
        }

        public int BytesToRead {
            get {
                if (this.ConnectionSettings.Mode != I2cMode.Slave)
                    throw new NotSupportedException(I2cController.MasterNotSupported);

                return this.Controller.Provider.BytesToRead;
            }
        }
    }

    public sealed class I2cConnectionSettings {
        public int SlaveAddress { get; set; }
        public I2cAddressFormat AddressFormat { get; set; }
        public uint BusSpeed { get; set; }
        public I2cMode Mode { get; set; }

        public bool EnableClockStretching { get; set; }

        public I2cConnectionSettings(int slaveAddress) : this(slaveAddress, I2cAddressFormat.SevenBit) {

        }

        public I2cConnectionSettings(int slaveAddress, uint busSpeed) : this(slaveAddress, I2cAddressFormat.SevenBit, busSpeed) {

        }

        public I2cConnectionSettings(int slaveAddress, I2cAddressFormat addressFormat, uint busSpeed = 100000) : this(slaveAddress, I2cMode.Master, addressFormat, busSpeed) {

        }

        public I2cConnectionSettings(int slaveAddress, I2cMode mode) : this(slaveAddress, mode, I2cAddressFormat.SevenBit) {

        }

        public I2cConnectionSettings(int slaveAddress, I2cMode mode, I2cAddressFormat addressFormat, uint busSpeed = 100000) : this(slaveAddress, mode, addressFormat, busSpeed, false) {

        }

        public I2cConnectionSettings(int slaveAddress, I2cMode mode, I2cAddressFormat addressFormat, uint busSpeed, bool enableClockStretching = false) {
            this.SlaveAddress = slaveAddress;
            this.AddressFormat = addressFormat;
            this.BusSpeed = busSpeed;
            this.Mode = mode;
            this.EnableClockStretching = enableClockStretching;
        }
    }

    public enum I2cAddressFormat {
        SevenBit = 0,
        TenBit = 1,
    }

    public enum I2cMode {
        Master = 0,
        Slave = 1
    }

    public enum I2cTransferStatus {
        FullTransfer = 0,
        PartialTransfer = 1,
        SlaveAddressNotAcknowledged = 2,
        ClockStretchTimeout = 3,
    }

    public enum I2cError {
        Overrun = 0,
        Bus = 1,
        ArbitrationLoss = 2,
        BufferFull = 3
    }

    public enum I2cTransaction {
        MasterWrite = 0,
        MasterRead = 1,
        MasterStop = 2
    }

    public sealed class FrameEventArgs {
        public DateTime Timestamp { get; }
        public uint DataCount { get; }

        public uint Address { get; }

        public I2cTransaction Event { get; }

        internal FrameEventArgs(I2cTransaction e, uint address, uint count, DateTime timestamp) {
            this.Address = address;
            this.DataCount = count;
            this.Timestamp = timestamp;
            this.Event = e;
        }
    }

    public sealed class ErrorReceivedEventArgs {
        public DateTime Timestamp { get; }
        public I2cError Error { get; }

        public uint Address { get; }
        internal ErrorReceivedEventArgs(uint address, I2cError error, DateTime timestamp) {
            this.Address = address;
            this.Error = error;
            this.Timestamp = timestamp;
        }
    }

    public struct I2cTransferResult {
        public I2cTransferStatus Status { get; }
        public int BytesWritten { get; }
        public int BytesRead { get; }

        public int BytesTransferred => this.BytesWritten + this.BytesRead;

        internal I2cTransferResult(I2cTransferStatus status, int bytesWritten, int bytesRead) {
            this.Status = status;
            this.BytesWritten = bytesWritten;
            this.BytesRead = bytesRead;
        }
    }

    public delegate void FrameReceivedEventHandler(I2cDevice sender, FrameEventArgs e);
    public delegate void ErrorReceivedEventHandler(I2cDevice sender, ErrorReceivedEventArgs e);

    namespace Provider {
        public interface II2cControllerProvider : IDisposable {
            int WriteBufferSize { get; set; }
            int ReadBufferSize { get; set; }
            int BytesToWrite { get; }
            int BytesToRead { get; }
            void ClearWriteBuffer();
            void ClearReadBuffer();
            void SetActiveSettings(I2cConnectionSettings connectionSettings);
            I2cTransferStatus WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int written, out int read);

            event FrameReceivedEventHandler FrameReceived;
            event ErrorReceivedEventHandler ErrorReceived;

            TimeSpan Timeout { get; set; }
        }

        public sealed class I2cControllerApiWrapper : II2cControllerProvider {
            private readonly IntPtr impl;

            public NativeApi Api { get; }

            private FrameReceivedEventHandler frameReceivedCallbacks;
            private ErrorReceivedEventHandler errorReceivedCallbacks;

            private readonly NativeEventDispatcher frameReceivedDispatcher;
            private readonly NativeEventDispatcher errorReceivedDispatcher;

            public I2cControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.frameReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.I2c.FrameReceived");
                this.errorReceivedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.I2c.ErrorReceived");

                this.frameReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.frameReceivedCallbacks?.Invoke(null, new FrameEventArgs((I2cTransaction)d0, (uint)d1, (uint)d2, ts)); };
                this.errorReceivedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.errorReceivedCallbacks?.Invoke(null, new ErrorReceivedEventArgs((uint)d0, (I2cError)d1, ts)); };
            }

            public void Dispose() => this.Release();

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

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetActiveSettings(I2cConnectionSettings connectionSettings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern I2cTransferStatus WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int written, out int read);

            public extern int WriteBufferSize { [MethodImpl(MethodImplOptions.InternalCall)]  get; [MethodImpl(MethodImplOptions.InternalCall)]  set; }

            public extern int ReadBufferSize { [MethodImpl(MethodImplOptions.InternalCall)]  get; [MethodImpl(MethodImplOptions.InternalCall)]  set; }

            public extern int BytesToWrite { [MethodImpl(MethodImplOptions.InternalCall)]  get; }

            public extern int BytesToRead { [MethodImpl(MethodImplOptions.InternalCall)]  get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearWriteBuffer();
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClearReadBuffer();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetFrameReceivedEventEnabled(bool enabled);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetErrorReceivedEventEnabled(bool enabled);

            public TimeSpan Timeout { [MethodImpl(MethodImplOptions.InternalCall)] get; [MethodImpl(MethodImplOptions.InternalCall)] set; }

        }

        internal sealed class I2cControllerSoftwareProvider : II2cControllerProvider {
            private readonly bool usePullups;
            private readonly GpioPin sda;
            private readonly GpioPin scl;
            private byte writeAddress;
            private byte readAddress;
            private bool start;

            public event ErrorReceivedEventHandler ErrorReceived {
                add {
                    throw new NotSupportedException(I2cController.MasterNotSupported);
                }
                remove {
                    throw new NotSupportedException(I2cController.MasterNotSupported);
                }
            }
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

            public TimeSpan Timeout { get; set; }

            public void ClearWriteBuffer() => throw new NotSupportedException(I2cController.MasterNotSupported);
            public void ClearReadBuffer() => throw new NotSupportedException(I2cController.MasterNotSupported);
        }
    }
}
