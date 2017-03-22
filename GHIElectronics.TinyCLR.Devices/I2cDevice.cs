using GHIElectronics.TinyCLR.Devices.I2c.Provider;
using System;

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

        internal I2cDevice(II2cDeviceProvider provider) => this.provider = provider;

        public static I2cDevice FromId(string deviceId, I2cConnectionSettings settings) {
            if (settings.SlaveAddress < 0 || settings.SlaveAddress > 127) throw new ArgumentOutOfRangeException();
            if (deviceId == null) throw new ArgumentNullException(nameof(deviceId));
            if (deviceId != DefaultI2cDeviceProvider.I2cPrefix + "1") throw new InvalidOperationException();

            return new I2cDevice(new DefaultI2cDeviceProvider(deviceId, settings));
        }

        public static string GetDeviceSelector() => DefaultI2cDeviceProvider.I2cPrefix;
        public static string GetDeviceSelector(string friendlyName) => friendlyName;

        public void Dispose() => this.provider.Dispose();
        public void Read(byte[] buffer) => this.provider.Read(buffer);
        public I2cTransferResult ReadPartial(byte[] buffer) => new I2cTransferResult(this.provider.ReadPartial(buffer));
        public void Write(byte[] buffer) => this.provider.Write(buffer);
        public I2cTransferResult WritePartial(byte[] buffer) => new I2cTransferResult(this.provider.WritePartial(buffer));
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer) => this.provider.WriteRead(writeBuffer, readBuffer);
        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) => new I2cTransferResult(this.provider.WriteReadPartial(writeBuffer, readBuffer));
    }
}
