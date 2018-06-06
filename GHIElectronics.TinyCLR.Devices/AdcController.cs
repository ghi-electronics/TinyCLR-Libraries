using System;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Adc.Provider;

namespace GHIElectronics.TinyCLR.Devices.Adc {
    public sealed class AdcController {
        private IAdcControllerProvider m_provider;

        private int idx;

        internal AdcController(IAdcControllerProvider provider, int idx = 0) {
            this.m_provider = provider;
            this.idx = idx;
        }

        public int ChannelCount => this.m_provider.ChannelCount;

        public int ResolutionInBits => this.m_provider.ResolutionInBits;

        public int MinValue => this.m_provider.MinValue;

        public int MaxValue => this.m_provider.MaxValue;

        public AdcChannelMode ChannelMode {
            get => (AdcChannelMode)this.m_provider.ChannelMode;

            set {
                switch (value) {
                    case AdcChannelMode.Differential:
                    case AdcChannelMode.SingleEnded:
                        break;

                    default:
                        throw new ArgumentException();
                }

                this.m_provider.ChannelMode = (ProviderAdcChannelMode)value;
            }
        }

        public static AdcController GetDefault() => new AdcController(LowLevelDevicesController.DefaultProvider?.AdcControllerProvider ?? (Api.ParseSelector(Api.GetDefaultSelector(ApiType.AdcProvider), out var providerId, out var idx) ? AdcProvider.FromId(providerId).GetControllers()[idx] : null));

        public static AdcController[] GetControllers(IAdcProvider provider) {
            // FUTURE: This should return "Task<IVectorView<AdcController>>"
            var controllersProvider = provider.GetControllers();
            var controllers = new AdcController[controllersProvider.Length];

            for (var i = 0; i < controllersProvider.Length; ++i) {
                controllers[i] = new AdcController(controllersProvider[i], i);
            }

            return controllers;
        }

        public bool IsChannelModeSupported(AdcChannelMode channelMode) {
            switch (channelMode) {
                case AdcChannelMode.Differential:
                case AdcChannelMode.SingleEnded:
                    break;

                default:
                    throw new ArgumentException();
            }

            return this.m_provider.IsChannelModeSupported((ProviderAdcChannelMode)channelMode);
        }

        public AdcChannel OpenChannel(int channelNumber) {
            if ((channelNumber < 0) || (channelNumber >= this.m_provider.ChannelCount)) {
                throw new ArgumentOutOfRangeException();
            }

            return new AdcChannel(this, this.m_provider, channelNumber);
        }
    }
}
