using GHIElectronics.TinyCLR.Devices.I2c.Provider;
using System;
using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.Devices.I2c {
    public struct I2cTransferResult {
        public I2cTransferStatus Status;
        public uint BytesTransferred;

        internal I2cTransferResult(ProviderI2cTransferResult providerI2cTransferResult) {
            this.Status = (I2cTransferStatus)providerI2cTransferResult.Status;
            this.BytesTransferred = providerI2cTransferResult.BytesTransferred;
        }
    }

    public sealed class I2cDevice : IDisposable {
        private readonly II2cDeviceProvider provider;

        internal I2cDevice(I2cConnectionSettings settings, II2cDeviceProvider provider) {
            this.ConnectionSettings = settings;
            this.provider = provider;
        }

        public static I2cDevice FromId(string deviceId, I2cConnectionSettings settings) {
            if (settings.SlaveAddress < 0 || settings.SlaveAddress > 127) throw new ArgumentOutOfRangeException();
            if (deviceId == null) throw new ArgumentNullException(nameof(deviceId));

            return Api.ParseSelector(deviceId, out var providerId, out var idx) ? new I2cDevice(settings, I2cProvider.FromId(providerId).GetControllers()[idx].GetDeviceProvider(new ProviderI2cConnectionSettings(settings))) : null;
        }

        public static string GetDeviceSelector() => throw new NotSupportedException();
        public static string GetDeviceSelector(string friendlyName) => throw new NotSupportedException();

        public string DeviceId => this.provider.DeviceId;
        public I2cConnectionSettings ConnectionSettings { get; }

        public void Dispose() => this.provider.Dispose();
        public void Read(byte[] buffer) => this.provider.Read(buffer);
        public I2cTransferResult ReadPartial(byte[] buffer) => new I2cTransferResult(this.provider.ReadPartial(buffer));
        public void Write(byte[] buffer) => this.provider.Write(buffer);
        public I2cTransferResult WritePartial(byte[] buffer) => new I2cTransferResult(this.provider.WritePartial(buffer));
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer) => this.provider.WriteRead(writeBuffer, readBuffer);
        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) => new I2cTransferResult(this.provider.WriteReadPartial(writeBuffer, readBuffer));

        public void Read(byte[] buffer, int offset, int length) => this.provider.Read(buffer, offset, length);
        public I2cTransferResult ReadPartial(byte[] buffer, int offset, int length) => new I2cTransferResult(this.provider.ReadPartial(buffer, offset, length));
        public void Write(byte[] buffer, int offset, int length) => this.provider.Write(buffer, offset, length);
        public I2cTransferResult WritePartial(byte[] buffer, int offset, int length) => new I2cTransferResult(this.provider.WritePartial(buffer, offset, length));
        public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) => this.provider.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength);
        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) => new I2cTransferResult(this.provider.WriteReadPartial(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength));
    }
}
