using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.I2c.Provider;

namespace GHIElectronics.TinyCLR.Devices.I2c {
    public sealed class I2cController : IDisposable {
        private I2cDevice active;

        public II2cControllerProvider Provider { get; }

        private I2cController(II2cControllerProvider provider) => this.Provider = provider;

        public static I2cController GetDefault() => Api.GetDefaultFromCreator(ApiType.I2cController) is I2cController c ? c : I2cController.FromName(Api.GetDefaultName(ApiType.I2cController));
        public static I2cController FromName(string name) => I2cController.FromProvider(new I2cControllerApiWrapper(Api.Find(name, ApiType.I2cController)));
        public static I2cController FromProvider(II2cControllerProvider provider) => new I2cController(provider);

        public void Dispose() => this.Provider.Dispose();

        public I2cDevice GetDevice(I2cConnectionSettings connectionSettings) => new I2cDevice(this, connectionSettings);

        internal void SetActive(I2cDevice device) {
            if (this.active != device) {
                this.active = device;

                this.Provider.SetActiveSettings(device.ConnectionSettings.SlaveAddress, device.ConnectionSettings.BusSpeed);
            }
        }
    }

    public sealed class I2cDevice : IDisposable {
        public I2cConnectionSettings ConnectionSettings { get; }
        public I2cController Controller { get; }

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
            this.Controller.SetActive(this);

            if (this.Controller.Provider.WriteRead(writeBuffer, (uint)writeOffset, (uint)writeLength, readBuffer, (uint)readOffset, (uint)readLength, true, out _, out _) != I2cTransferStatus.FullTransfer)
                throw new InvalidOperationException();
        }

        public I2cTransferResult ReadPartial(byte[] buffer) => this.WriteReadPartial(null, 0, 0, buffer, 0, buffer.Length);
        public I2cTransferResult WritePartial(byte[] buffer) => this.WriteReadPartial(buffer, 0, buffer.Length, null, 0, 0);
        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) => this.WriteReadPartial(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);

        public I2cTransferResult ReadPartial(byte[] buffer, int offset, int length) => this.WriteReadPartial(null, 0, 0, buffer, offset, length);
        public I2cTransferResult WritePartial(byte[] buffer, int offset, int length) => this.WriteReadPartial(buffer, offset, length, null, 0, 0);

        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            this.Controller.SetActive(this);

            var res = this.Controller.Provider.WriteRead(writeBuffer, (uint)writeOffset, (uint)writeLength, readBuffer, (uint)readOffset, (uint)readLength, true, out var written, out var read);

            return new I2cTransferResult(res, written, read);
        }
    }

    public sealed class I2cConnectionSettings {
        public uint SlaveAddress { get; set; }
        public I2cBusSpeed BusSpeed { get; set; }

        public I2cConnectionSettings(uint slaveAddress) : this(slaveAddress, I2cBusSpeed.StandardMode) {

        }

        public I2cConnectionSettings(uint slaveAddress, I2cBusSpeed busSpeed) {
            this.SlaveAddress = slaveAddress;
            this.BusSpeed = busSpeed;
        }
    }

    public enum I2cBusSpeed {
        StandardMode = 0,
        FastMode = 1,
    }

    public enum I2cTransferStatus {
        FullTransfer = 0,
        PartialTransfer = 1,
        SlaveAddressNotAcknowledged = 2,
        ClockStretchTimeout = 3,
    }

    public struct I2cTransferResult {
        public I2cTransferStatus Status { get; }
        public uint BytesWritten { get; }
        public uint BytesRead { get; }

        public uint BytesTransferred => this.BytesWritten + this.BytesRead;

        internal I2cTransferResult(I2cTransferStatus status, uint bytesWritten, uint bytesRead) {
            this.Status = status;
            this.BytesWritten = bytesWritten;
            this.BytesRead = bytesRead;
        }
    }

    namespace Provider {
        public interface II2cControllerProvider : IDisposable {
            void SetActiveSettings(uint slaveAddress, I2cBusSpeed speed);
            I2cTransferStatus WriteRead(byte[] writeBuffer, uint writeOffset, uint writeLength, byte[] readBuffer, uint readOffset, uint readLength, bool sendStopAfter, out uint written, out uint read);
        }

        public sealed class I2cControllerApiWrapper : II2cControllerProvider {
            private readonly IntPtr impl;

            public Api Api { get; }

            public I2cControllerApiWrapper(Api api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetActiveSettings(uint slaveAddress, I2cBusSpeed speed);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern I2cTransferStatus WriteRead(byte[] writeBuffer, uint writeOffset, uint writeLength, byte[] readBuffer, uint readOffset, uint readLength, bool sendStopAfter, out uint written, out uint read);
        }
    }
}
