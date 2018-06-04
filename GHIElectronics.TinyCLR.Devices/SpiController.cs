using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Spi.Provider;

namespace GHIElectronics.TinyCLR.Devices.Spi {
    public sealed class SpiController {
        private ISpiControllerProvider provider;

        internal SpiController(ISpiControllerProvider provider) => this.provider = provider;

        public static SpiController GetDefault() => new SpiController(LowLevelDevicesController.DefaultProvider?.SpiControllerProvider ?? (Api.ParseSelector(Api.GetDefaultSelector(ApiType.SpiProvider), out var providerId, out var idx) ? SpiProvider.FromId(providerId).GetController((int)idx) : null));

        public SpiDevice GetDevice(SpiConnectionSettings settings) => new SpiDevice(settings, this.provider.GetDeviceProvider(new ProviderSpiConnectionSettings(settings)));

        public static SpiController GetController(ISpiProvider provider) =>
            // TODO should return controller count??
            null;
    }
}
