using System;
using GHIElectronics.TinyCLR.Devices.Dac.Provider;
using GHIElectronics.TinyCLR.Native;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.Dac\Dac.cs.
// Bodies on Desktop are safe no-ops:
//   * WriteValue(int)/(double) -> stored in LastWrittenValue, otherwise empty
//   * Open/CloseChannel        -> empty
// Defaults match a generic 12-bit DAC: 2 channels, 0..4095.
namespace GHIElectronics.TinyCLR.Devices.Dac {
    public class DacController : IDisposable {
        public IDacControllerProvider Provider { get; }

        private DacController(IDacControllerProvider provider) => this.Provider = provider;

        public static DacController GetDefault() => DacController.FromName("Simulator");
        public static DacController FromName(string name) => DacController.FromProvider(new DacControllerApiWrapper(NativeApi.Find(name, NativeApiType.DacController)));
        public static DacController FromProvider(IDacControllerProvider provider) => new DacController(provider);

        public int ChannelCount => this.Provider.ChannelCount;
        public int ResolutionInBits => this.Provider.ResolutionInBits;
        public int MinValue => this.Provider.MinValue;
        public int MaxValue => this.Provider.MaxValue;

        public void Dispose() => this.Provider.Dispose();

        public DacChannel OpenChannel(int channelNumber) => new DacChannel(this, channelNumber);
    }

    public class DacChannel : IDisposable {
        public int ChannelNumber { get; }
        public DacController Controller { get; }

        public int LastWrittenValue { get; private set; }

        internal DacChannel(DacController controller, int channelNumber) {
            this.ChannelNumber = channelNumber;
            this.Controller = controller;

            this.Controller.Provider.OpenChannel(channelNumber);
        }

        public void Dispose() => this.Controller.Provider.CloseChannel(this.ChannelNumber);

        public void WriteValue(int value) => this.Controller.Provider.Write(this.ChannelNumber, this.LastWrittenValue = value);
        public void WriteValue(double ratio) => this.WriteValue((int)(ratio * (this.Controller.MaxValue - this.Controller.MinValue) + this.Controller.MinValue));
    }

    namespace Provider {
        public interface IDacControllerProvider : IDisposable {
            int ChannelCount { get; }
            int ResolutionInBits { get; }
            int MinValue { get; }
            int MaxValue { get; }

            void OpenChannel(int channel);
            void CloseChannel(int channel);

            void Write(int channel, int value);
        }

        public sealed class DacControllerApiWrapper : IDacControllerProvider {
            public NativeApi Api { get; }

            public DacControllerApiWrapper(NativeApi api) => this.Api = api;

            public void Dispose() { }

            public int ChannelCount => 2;
            public int ResolutionInBits => 12;
            public int MinValue => 0;
            public int MaxValue => 4095;

            public void OpenChannel(int channel) { }
            public void CloseChannel(int channel) { }

            public void Write(int channel, int value) { }
        }
    }
}
