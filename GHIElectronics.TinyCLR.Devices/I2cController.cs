using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.I2c.Provider;

namespace GHIElectronics.TinyCLR.Devices.I2c {
    public sealed class I2cController {
        private II2cControllerProvider provider;

        internal I2cController(II2cControllerProvider provider) => this.provider = provider;

        public static I2cController GetDefault() => new I2cController(LowLevelDevicesController.DefaultProvider?.I2cControllerProvider ?? (Api.ParseSelector(Api.GetDefaultSelector(ApiType.I2cProvider), out var providerId, out var idx) ? I2cProvider.FromId(providerId).GetController((int)idx) : null));

        public I2cDevice GetDevice(I2cConnectionSettings settings) => new I2cDevice(settings, this.provider.GetDeviceProvider(new ProviderI2cConnectionSettings(settings)));

    }
}
