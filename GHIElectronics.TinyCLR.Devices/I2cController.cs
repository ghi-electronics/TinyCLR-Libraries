using GHIElectronics.TinyCLR.Devices.I2c.Provider;
using System;

namespace GHIElectronics.TinyCLR.Devices.I2c {
    public sealed class I2cController {
        private II2cControllerProvider provider;

        internal I2cController(II2cControllerProvider provider) => this.provider = provider;

        public static I2cController GetDefault() => throw new NotSupportedException();
        public I2cDevice GetDevice(I2cConnectionSettings settings) => new I2cDevice(settings, this.provider.GetDeviceProvider(new ProviderI2cConnectionSettings(settings)));

        public static I2cController[] GetControllers(II2cProvider provider) {
            var providers = provider.GetControllers();
            var controllers = new I2cController[providers.Length];

            for (var i = 0; i < providers.Length; i++)
                controllers[i] = new I2cController(providers[i]);

            return controllers;
        }
    }
}
