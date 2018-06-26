using System;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.SdCard.Provider;

namespace GHIElectronics.TinyCLR.Devices.SdCard {
    public sealed class SdCardController {
        private ISdCardControllerProvider provider;

        internal SdCardController(ISdCardControllerProvider provider) => this.provider = provider;

        public static SdCardController GetDefault() => new SdCardController(Api.ParseSelector(Api.GetDefaultSelector(ApiType.SdCardProvider), out var providerId, out var idx) ? SdCardProvider.FromId(providerId).GetControllers()[idx] : null);

        public static SdCardController[] GetControllers(ISdCardProvider provider) {

            var providers = provider.GetControllers();

            var controllers = new SdCardController[providers.Length];

            for (var i = 0; i < providers.Length; ++i) {
                controllers[i] = new SdCardController(providers[i]);
            }

            return controllers;

        }

        public IntPtr Hdc => this.provider.Hdc;

        public int ControllerIndex => this.provider.ControllerIndex;

        public ISdCardControllerProvider Provider => this.provider;
    }
}
