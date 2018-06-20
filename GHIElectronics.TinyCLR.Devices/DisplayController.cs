using System;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Display.Provider;

namespace GHIElectronics.TinyCLR.Devices.Display {
    public sealed class DisplayController {
        private IDisplayControllerProvider provider;

        internal DisplayController(IDisplayControllerProvider provider) => this.provider = provider;

        public static DisplayController GetDefault() => new DisplayController(LowLevelDevicesController.DefaultProvider?.DisplayControllerProvider ?? (Api.ParseSelector(Api.GetDefaultSelector(ApiType.DisplayProvider), out var providerId, out var idx) ? DisplayProvider.FromId(providerId).GetControllers()[idx] : null));

        public static DisplayController[] GetControllers(IDisplayProvider provider) {
            var providers = provider.GetControllers();
            var controllers = new DisplayController[providers.Length];

            for (var i = 0; i < providers.Length; ++i) {
                controllers[i] = new DisplayController(providers[i]);
            }

            return controllers;
        }

        public void ApplySettings(DisplayControllerSettings settings) {
            this.provider.ApplySettings(settings);

            this.ActiveSettings = settings;
        }

        public void WriteString(string str) => this.provider.WriteString(str);

        public DisplayControllerSettings ActiveSettings { get; private set; }

        public IntPtr Hdc => this.provider.Hdc;
        public DisplayInterface Interface => this.provider.Interface;
        public DisplayDataFormat[] SupportedDataFormats => this.provider.SupportedDataFormats;
    }
}
