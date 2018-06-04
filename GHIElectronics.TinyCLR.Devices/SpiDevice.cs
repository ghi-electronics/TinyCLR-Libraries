using System;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Spi.Provider;

namespace GHIElectronics.TinyCLR.Devices.Spi {
    public sealed class SpiDevice : IDisposable {
        private readonly ISpiDeviceProvider provider;

        internal SpiDevice(SpiConnectionSettings settings, ISpiDeviceProvider provider) {
            this.ConnectionSettings = settings;
            this.provider = provider;
        }

        public string DeviceId => this.provider.DeviceId;
        public SpiConnectionSettings ConnectionSettings { get; }

        public void Dispose() => this.provider.Dispose();
        public void Read(byte[] buffer) => this.provider.Read(buffer);
        public void Write(byte[] buffer) => this.provider.Write(buffer);
        public void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer) => this.provider.TransferFullDuplex(writeBuffer, readBuffer);
        public void TransferSequential(byte[] writeBuffer, byte[] readBuffer) => this.provider.TransferSequential(writeBuffer, readBuffer);

        public void Read(byte[] buffer, int offset, int length) => this.provider.Read(buffer, offset, length);
        public void Write(byte[] buffer, int offset, int length) => this.provider.Write(buffer, offset, length);
        public void TransferFullDuplex(byte[] writeBuffer, int writeOffset, byte[] readBuffer, int readOffset, int length) => this.provider.TransferFullDuplex(writeBuffer, writeOffset, readBuffer, readOffset, length);
        public void TransferSequential(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) => this.provider.TransferSequential(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength);

        public static string GetDeviceSelector() => throw new NotSupportedException();
        public static string GetDeviceSelector(string friendlyName) => throw new NotSupportedException();

        /// <summary>
        /// Retrieves the info about a certain bus.
        /// </summary>
        /// <param name="selector">The id of the bus.</param>
        /// <returns>The bus info requested.</returns>
        public static SpiBusInfo GetBusInfo(string selector) {
            if (Api.ParseSelector(selector, out var providerId, out var controllerIndex)) {
                var api = Api.Find(providerId, ApiType.SpiProvider);

                return new SpiBusInfo(api.Implementation[0], (int)controllerIndex);
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Opens a device with the connection settings provided.
        /// </summary>
        /// <param name="busId">The id of the bus.</param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static SpiDevice FromId(string busId, SpiConnectionSettings settings) {
            // FUTURE: This should be "Task<SpiDevice*> FromIdAsync(...)"
            switch (settings.Mode) {
                case SpiMode.Mode0:
                case SpiMode.Mode1:
                case SpiMode.Mode2:
                case SpiMode.Mode3:
                    break;

                default:
                    throw new ArgumentException();
            }

            switch (settings.SharingMode) {
                case SpiSharingMode.Exclusive:
                case SpiSharingMode.Shared:
                    break;

                default:
                    throw new ArgumentException();
            }

            switch (settings.DataBitLength) {
                case 8:
                case 16:
                    break;

                default:
                    throw new ArgumentException();
            }

            return Api.ParseSelector(busId, out var providerId, out var idx) ? new SpiDevice(settings, SpiProvider.FromId(providerId).GetController((int)idx).GetDeviceProvider(new ProviderSpiConnectionSettings(settings))) : null;
        }

    }
}
