using System;
using GHIElectronics.TinyCLR.Devices.Adc.Provider;
using GHIElectronics.TinyCLR.Native;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.Adc\Adc.cs.
// Bodies on Desktop are safe no-ops:
//   * ReadValue()          -> 0   (ReadRatio() -> 0.0 since MinValue==0)
//   * Open/CloseChannel    -> empty
//   * IsChannelModeSupported -> true
//   * Channel mode is stored in a field so Get/Set round-trips
// Defaults match a generic 12-bit ADC: 8 channels, 0..4095, single-ended.
namespace GHIElectronics.TinyCLR.Devices.Adc {
    public enum AdcChannelMode {
        SingleEnded = 0,
        Differential = 1
    }

    public class AdcController : IDisposable {
        public IAdcControllerProvider Provider { get; }

        private AdcController(IAdcControllerProvider provider) => this.Provider = provider;

        // GetDefault() routes through FromName("Simulator") on Desktop. Reads
        // honestly in user code/debugger — and gives us a future Phase 2 hook
        // (users could register a real simulator under that name).
        public static AdcController GetDefault() => AdcController.FromName("Simulator");
        public static AdcController FromName(string name) => AdcController.FromProvider(new AdcControllerApiWrapper(NativeApi.Find(name, NativeApiType.AdcController)));
        public static AdcController FromProvider(IAdcControllerProvider provider) => new AdcController(provider);

        public int ChannelCount => this.Provider.ChannelCount;
        public int ResolutionInBits => this.Provider.ResolutionInBits;
        public int MinValue => this.Provider.MinValue;
        public int MaxValue => this.Provider.MaxValue;

        public AdcChannelMode ChannelMode {
            get => this.Provider.GetChannelMode();
            set => this.Provider.SetChannelMode(value);
        }

        public bool IsChannelModeSupported(AdcChannelMode mode) => this.Provider.IsChannelModeSupported(mode);

        public void Dispose() => this.Provider.Dispose();

        public AdcChannel OpenChannel(int channelNumber) => new AdcChannel(this, channelNumber);
    }

    public class AdcChannel : IDisposable {
        public int ChannelNumber { get; }
        public AdcController Controller { get; }
        public TimeSpan SamplingTime { get; set; } = TimeSpan.FromTicks(1);

        internal AdcChannel(AdcController controller, int channelNumber) {
            this.ChannelNumber = channelNumber;
            this.Controller = controller;

            this.Controller.Provider.OpenChannel(channelNumber);
        }

        public void Dispose() => this.Controller.Provider.CloseChannel(this.ChannelNumber);

        public int ReadValue() => this.Controller.Provider.Read(this.ChannelNumber, this.SamplingTime);
        public double ReadRatio() => (this.ReadValue() - this.Controller.MinValue) / (double)(this.Controller.MaxValue - this.Controller.MinValue);
    }

    namespace Provider {
        public interface IAdcControllerProvider : IDisposable {
            int ChannelCount { get; }
            int ResolutionInBits { get; }
            int MinValue { get; }
            int MaxValue { get; }

            bool IsChannelModeSupported(AdcChannelMode mode);
            AdcChannelMode GetChannelMode();
            void SetChannelMode(AdcChannelMode value);

            void OpenChannel(int channel);
            void CloseChannel(int channel);

            int Read(int channel, TimeSpan samplingTime);
        }

        // Public surface mirrors the impl's AdcControllerApiWrapper. Same
        // public ctor signature (NativeApi), same Api property, same methods.
        // Bodies are no-ops (no [MethodImpl(InternalCall)]).
        public sealed class AdcControllerApiWrapper : IAdcControllerProvider {
            private AdcChannelMode channelMode = AdcChannelMode.SingleEnded;

            public NativeApi Api { get; }

            public AdcControllerApiWrapper(NativeApi api) => this.Api = api;

            public void Dispose() { }

            // Generic 12-bit ADC defaults so user code that reads these
            // properties to size buffers / scale values gets sensible numbers.
            public int ChannelCount => 8;
            public int ResolutionInBits => 12;
            public int MinValue => 0;
            public int MaxValue => 4095;

            public bool IsChannelModeSupported(AdcChannelMode mode) => true;
            public AdcChannelMode GetChannelMode() => this.channelMode;
            public void SetChannelMode(AdcChannelMode value) => this.channelMode = value;

            public void OpenChannel(int channel) { }
            public void CloseChannel(int channel) { }

            public int Read(int channel, TimeSpan samplingTime) => 0;
        }
    }
}
