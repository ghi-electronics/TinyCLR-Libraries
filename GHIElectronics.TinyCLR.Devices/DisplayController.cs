using GHIElectronics.TinyCLR.Devices.Display.Provider;
using System;

namespace GHIElectronics.TinyCLR.Devices.Display {
    public sealed class DisplayController {
        private IDisplayControllerProvider provider;

        internal DisplayController(IDisplayControllerProvider provider) => this.provider = provider;

        public static DisplayController GetDefault() => new DisplayController(LowLevelDevicesController.DefaultProvider?.DisplayControllerProvider ?? DefaultDisplayControllerProvider.Instance);

        public static DisplayController[] GetControllers(IDisplayProvider provider) {
            var providers = provider.GetControllers();
            var controllers = new DisplayController[providers.Length];

            for (var i = 0; i < providers.Length; i++)
                controllers[i] = new DisplayController(providers[i]);

            return controllers;
        }

        public void ApplySettings(DisplayControllerSettings settings) => this.provider.ApplySettings(settings);

        public IntPtr Hdc => this.provider.Hdc;
    }
}
