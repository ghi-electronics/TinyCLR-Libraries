using System;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c.Provider;
using GHIElectronics.TinyCLR.Native;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.I2c\I2c.cs.
// All bodies on Desktop are safe no-ops:
//   * WriteRead -> returns FullTransfer with 0 bytes; no exception thrown
//   * Buffer sizes / timeout round-trip via fields
//   * Events accept handlers but never fire (no native source on PC)
namespace GHIElectronics.TinyCLR.Devices.I2c {
    public class I2cController : IDisposable {
        public II2cControllerProvider Provider { get; }

        public TimeSpan Timeout {
            get => this.Provider.Timeout;
            set => this.Provider.Timeout = value;
        }

        private I2cController(II2cControllerProvider provider) => this.Provider = provider;

        public static I2cController GetDefault() => I2cController.FromName("Simulator");
        public static I2cController FromName(string name) => FromProvider(new I2cControllerApiWrapper(NativeApi.Find(name, NativeApiType.I2cController)));
        public static I2cController FromName(string name, GpioPin sdaPin, GpioPin sclPin) => FromName(name, sdaPin, sclPin, false);
        public static I2cController FromName(string name, GpioPin sdaPin, GpioPin sclPin, bool usePullups) => FromProvider(new I2cControllerSoftwareProvider(sdaPin, sclPin, usePullups));
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

        public void Dispose() { }

        public void Read(byte[] buffer) => this.WriteRead(null, 0, 0, buffer, 0, buffer.Length);
        public void Write(byte[] buffer) => this.WriteRead(buffer, 0, buffer.Length, null, 0, 0);
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer) => this.WriteRead(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);
        public void Read(byte[] buffer, int offset, int length) => this.WriteRead(null, 0, 0, buffer, offset, length);
        public void Write(byte[] buffer, int offset, int length) => this.WriteRead(buffer, offset, length, null, 0, 0);

        public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            lock (ojectLocker) {
                this.Controller.SetActive(this);
                this.Controller.Provider.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out _, out _);
            }
        }

        public I2cTransferResult ReadPartial(byte[] buffer) => this.WriteReadPartial(null, 0, 0, buffer, 0, buffer.Length);
        public I2cTransferResult WritePartial(byte[] buffer) => this.WriteReadPartial(buffer, 0, buffer.Length, null, 0, 0);
        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) => this.WriteReadPartial(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);
        public I2cTransferResult ReadPartial(byte[] buffer, int offset, int length) => this.WriteReadPartial(null, 0, 0, buffer, offset, length);
        public I2cTransferResult WritePartial(byte[] buffer, int offset, int length) => this.WriteReadPartial(buffer, offset, length, null, 0, 0);

        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            lock (ojectLocker) {
                this.Controller.SetActive(this);
                var res = this.Controller.Provider.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out var written, out var read);
                return new I2cTransferResult(res, written, read);
            }
        }

        public event FrameReceivedEventHandler FrameReceived {
            add => this.frameReceivedCallbacks += value;
            remove => this.frameReceivedCallbacks -= value;
        }

        public event ErrorReceivedEventHandler ErrorReceived {
            add => this.errorReceivedCallbacks += value;
            remove => this.errorReceivedCallbacks -= value;
        }

        public void ClearWriteBuffer() => this.Controller.Provider.ClearWriteBuffer();
        public void ClearReadBuffer() => this.Controller.Provider.ClearReadBuffer();

        public int WriteBufferSize => this.Controller.Provider.WriteBufferSize;
        public int ReadBufferSize => this.Controller.Provider.ReadBufferSize;
        public int BytesToWrite => this.Controller.Provider.BytesToWrite;
        public int BytesToRead => this.Controller.Provider.BytesToRead;
    }

    public sealed class I2cConnectionSettings {
        public int SlaveAddress { get; set; }
        public I2cAddressFormat AddressFormat { get; set; }
        public uint BusSpeed { get; set; }
        public I2cMode Mode { get; set; }
        public bool EnableClockStretching { get; set; }

        public I2cConnectionSettings(int slaveAddress) : this(slaveAddress, I2cAddressFormat.SevenBit) { }
        public I2cConnectionSettings(int slaveAddress, uint busSpeed) : this(slaveAddress, I2cAddressFormat.SevenBit, busSpeed) { }
        public I2cConnectionSettings(int slaveAddress, I2cAddressFormat addressFormat, uint busSpeed = 100000) : this(slaveAddress, I2cMode.Master, addressFormat, busSpeed) { }
        public I2cConnectionSettings(int slaveAddress, I2cMode mode) : this(slaveAddress, mode, I2cAddressFormat.SevenBit) { }
        public I2cConnectionSettings(int slaveAddress, I2cMode mode, I2cAddressFormat addressFormat, uint busSpeed = 100000) : this(slaveAddress, mode, addressFormat, busSpeed, false) { }
        public I2cConnectionSettings(int slaveAddress, I2cMode mode, I2cAddressFormat addressFormat, uint busSpeed, bool enableClockStretching = false) {
            this.SlaveAddress = slaveAddress;
            this.AddressFormat = addressFormat;
            this.BusSpeed = busSpeed;
            this.Mode = mode;
            this.EnableClockStretching = enableClockStretching;
        }
    }

    public enum I2cAddressFormat { SevenBit = 0, TenBit = 1 }
    public enum I2cMode { Master = 0, Slave = 1 }
    public enum I2cTransferStatus { FullTransfer = 0, PartialTransfer = 1, SlaveAddressNotAcknowledged = 2, ClockStretchTimeout = 3 }
    public enum I2cError { Overrun = 0, Bus = 1, ArbitrationLoss = 2, BufferFull = 3 }
    public enum I2cTransaction { MasterWrite = 0, MasterRead = 1, MasterStop = 2 }

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
            public NativeApi Api { get; }
            public TimeSpan Timeout { get; set; }
            public int WriteBufferSize { get; set; }
            public int ReadBufferSize { get; set; }
            public int BytesToWrite => 0;
            public int BytesToRead => 0;

            public I2cControllerApiWrapper(NativeApi api) => this.Api = api;

            public void Dispose() { }

            public event FrameReceivedEventHandler FrameReceived { add { } remove { } }
            public event ErrorReceivedEventHandler ErrorReceived { add { } remove { } }

            public void SetActiveSettings(I2cConnectionSettings connectionSettings) { }
            public void ClearWriteBuffer() { }
            public void ClearReadBuffer() { }

            public I2cTransferStatus WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int written, out int read) {
                written = 0;
                read = 0;
                return I2cTransferStatus.FullTransfer;
            }
        }

        // Software provider on Desktop is also a no-op — we keep the GpioPin
        // refs alive but never bit-bang. Same public surface as impl so user
        // code constructing a SoftwareI2cController works on PC.
        internal sealed class I2cControllerSoftwareProvider : II2cControllerProvider {
            private readonly GpioPin sda;
            private readonly GpioPin scl;

            public TimeSpan Timeout { get; set; }
            public int WriteBufferSize { get => 0; set { } }
            public int ReadBufferSize { get => 0; set { } }
            public int BytesToWrite => 0;
            public int BytesToRead => 0;

            public event FrameReceivedEventHandler FrameReceived { add { } remove { } }
            public event ErrorReceivedEventHandler ErrorReceived { add { } remove { } }

            public I2cControllerSoftwareProvider(GpioPin sdaPin, GpioPin sclPin) : this(sdaPin, sclPin, true) { }

            public I2cControllerSoftwareProvider(GpioPin sdaPin, GpioPin sclPin, bool usePullups) {
                this.sda = sdaPin;
                this.scl = sclPin;
            }

            public void Dispose() {
                this.sda?.Dispose();
                this.scl?.Dispose();
            }

            public void SetActiveSettings(I2cConnectionSettings connectionSettings) { }
            public void ClearWriteBuffer() { }
            public void ClearReadBuffer() { }

            public I2cTransferStatus WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out int written, out int read) {
                written = 0;
                read = 0;
                return I2cTransferStatus.FullTransfer;
            }
        }
    }
}
