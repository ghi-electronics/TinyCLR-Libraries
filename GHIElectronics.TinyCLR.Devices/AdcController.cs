using GHIElectronics.TinyCLR.Devices.Adc.Provider;
using System;

namespace GHIElectronics.TinyCLR.Devices.Adc {
    public sealed class AdcController {
        private IAdcControllerProvider m_provider;

        internal AdcController(IAdcControllerProvider provider) => this.m_provider = provider;

        public int ChannelCount => this.m_provider.ChannelCount;

        public int ResolutionInBits => this.m_provider.ResolutionInBits;

        public int MinValue => this.m_provider.MinValue;

        public int MaxValue => this.m_provider.MaxValue;

        public AdcChannelMode ChannelMode {
            get => (AdcChannelMode)m_provider.ChannelMode;

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

        public static AdcController GetDefault() => new AdcController(LowLevelDevicesController.DefaultProvider?.AdcControllerProvider ?? DefaultAdcControllerProvider.Instance);

        public static AdcController[] GetControllers(IAdcProvider provider) {
            // FUTURE: This should return "Task<IVectorView<AdcController>>"

            var providers = provider.GetControllers();
            var controllers = new AdcController[providers.Length];

            for (var i = 0; i < providers.Length; ++i) {
                controllers[i] = new AdcController(providers[i]);
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
