using GHIElectronics.TinyCLR.Devices.Spi.Provider;
using System;

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

        public static string GetDeviceSelector() => DefaultSpiDeviceProvider.s_SpiPrefix;
        public static string GetDeviceSelector(string friendlyName) => friendlyName;

        /// <summary>
        /// Retrieves the info about a certain bus.
        /// </summary>
        /// <param name="busId">The id of the bus.</param>
        /// <returns>The bus info requested.</returns>
        public static SpiBusInfo GetBusInfo(string busId) {
            var busNum = GetBusNum(busId);
            return new SpiBusInfo(busNum);
        }

        internal static int GetBusNum(string deviceId) {
            if (deviceId == null) throw new ArgumentNullException(nameof(deviceId));
            if (deviceId.Length < 4 || deviceId.IndexOf("SPI") != 0 || !int.TryParse(deviceId.Substring(3), out var id) || id <= 0) throw new ArgumentException("Invalid SPI bus", nameof(deviceId));

            return id - 1;
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

            return new SpiDevice(settings, DefaultSpiControllerProvider.FindById(busId).GetDeviceProvider(new ProviderSpiConnectionSettings(settings)));
        }

    }
}
