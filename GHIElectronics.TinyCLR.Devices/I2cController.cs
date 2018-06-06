using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.I2c.Provider;

namespace GHIElectronics.TinyCLR.Devices.I2c {
    public sealed class I2cController {
        private II2cControllerProvider provider;

        internal I2cController(II2cControllerProvider provider) => this.provider = provider;

        public static I2cController GetDefault() => new I2cController(LowLevelDevicesController.DefaultProvider?.I2cControllerProvider ?? (Api.ParseSelector(Api.GetDefaultSelector(ApiType.I2cProvider), out var providerId, out var idx) ? I2cProvider.FromId(providerId).GetControllers()[idx] : null));

        public I2cDevice GetDevice(I2cConnectionSettings settings) => new I2cDevice(settings, this.provider.GetDeviceProvider(new ProviderI2cConnectionSettings(settings)));

        public static I2cController[] GetController(II2cProvider provider) {
            var providers = provider.GetControllers();
            var controllers = new I2cController[providers.Length];

            for (var i = 0; i < providers.Length; ++i) {
                controllers[i] = new I2cController(providers[i]);
            }

            return controllers;
        }

    }
}
