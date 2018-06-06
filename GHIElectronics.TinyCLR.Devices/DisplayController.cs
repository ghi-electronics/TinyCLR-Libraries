using System;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Display.Provider;

namespace GHIElectronics.TinyCLR.Devices.Display {
    public sealed class DisplayController {
        private IDisplayControllerProvider provider;
        private int idx;

        internal DisplayController(IDisplayControllerProvider provider, int idx = 0) {
            this.provider = provider;
            this.idx = idx;
        }

        public static DisplayController GetDefault() => new DisplayController(LowLevelDevicesController.DefaultProvider?.DisplayControllerProvider ?? (Api.ParseSelector(Api.GetDefaultSelector(ApiType.DisplayProvider), out var providerId, out var idx) ? DisplayProvider.FromId(providerId).GetControllers()[idx] : null));

        public static DisplayController[] GetControllers(IDisplayProvider provider) {
            var providerControllers = provider.GetControllers();
            var controllers = new DisplayController[providerControllers.Length];

            for (var i = 0; i < providerControllers.Length; ++i) {
                controllers[i] = new DisplayController(providerControllers[i], i);
            }

            return controllers;
        }

        public void ApplySettings(DisplayControllerSettings settings) => this.provider.ApplySettings(settings);
        public void WriteString(string str) => this.provider.WriteString(str);

        public IntPtr Hdc => this.provider.Hdc;
        public DisplayInterface Interface => this.provider.Interface;
        public DisplayDataFormat[] SupportedDataFormats => this.provider.SupportedDataFormats;
    }
}
