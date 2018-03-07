using System;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Rtc.Provider;

namespace GHIElectronics.TinyCLR.Devices.Rtc {
    public sealed class RtcController {
        private IRtcControllerProvider provider;

        internal RtcController(IRtcControllerProvider provider) => this.provider = provider;

        public static RtcController GetDefault() => new RtcController((Api.ParseSelector(Api.GetDefaultSelector(ApiType.RtcProvider), out var providerId, out var idx) ? RtcControllerProvider.FromId(providerId) : throw new InvalidOperationException()));

        public static RtcController GetController(IRtcControllerProvider provider) => new RtcController(provider);

        public DateTime Now {
            get => this.provider.Now;
            set => this.provider.Now = value;
        }
    }
}
