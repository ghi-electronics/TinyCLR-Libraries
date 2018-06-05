using System;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Display.Provider;

namespace GHIElectronics.TinyCLR.Devices.Display {
    public sealed class DisplayController {
        private IDisplayControllerProvider provider;

        internal DisplayController(IDisplayControllerProvider provider) => this.provider = provider;

        public static DisplayController GetDefault() => new DisplayController(LowLevelDevicesController.DefaultProvider?.DisplayControllerProvider ?? (Api.ParseSelector(Api.GetDefaultSelector(ApiType.DisplayProvider), out var providerId, out var idx) ? DisplayProvider.FromId(providerId).GetControllers() : null));

        public static DisplayController GetControllers(IDisplayProvider provider) => new DisplayController(provider.GetControllers());

        public void ApplySettings(DisplayControllerSettings settings) => this.provider.ApplySettings(settings);
        public void WriteString(string str) => this.provider.WriteString(str);

        public IntPtr Hdc => this.provider.Hdc;
        public DisplayInterface Interface => this.provider.Interface;
        public DisplayDataFormat[] SupportedDataFormats => this.provider.SupportedDataFormats;
    }
}
