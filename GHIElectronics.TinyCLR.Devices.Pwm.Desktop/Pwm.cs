using System;
using GHIElectronics.TinyCLR.Devices.Pwm.Provider;
using GHIElectronics.TinyCLR.Native;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.Pwm\Pwm.cs.
// Bodies on Desktop are safe no-ops.
namespace GHIElectronics.TinyCLR.Devices.Pwm {
    public class PwmController : IDisposable {
        public IPwmControllerProvider Provider { get; }

        private PwmController(IPwmControllerProvider provider) => this.Provider = provider;

        public static PwmController GetDefault() => PwmController.FromName("Simulator");
        public static PwmController FromName(string name) => PwmController.FromProvider(new PwmControllerApiWrapper(NativeApi.Find(name, NativeApiType.PwmController)));
        public static PwmController FromProvider(IPwmControllerProvider provider) => new PwmController(provider);

        public double ActualFrequency { get; private set; }

        public int ChannelCount => this.Provider.ChannelCount;
        public double MinFrequency => this.Provider.MinFrequency;
        public double MaxFrequency => this.Provider.MaxFrequency;

        public void Dispose() => this.Provider.Dispose();

        public double SetDesiredFrequency(double desiredFrequency) => this.ActualFrequency = this.Provider.SetDesiredFrequency(desiredFrequency);
        public double SetDesiredFrequency(PwmChannel channel, double desiredFrequency) => this.Provider.SetDesiredFrequency(channel.ChannelNumber, desiredFrequency);

        public PwmChannel OpenChannel(int channelNumber) => new PwmChannel(this, channelNumber);
    }

    public class PwmChannel : IDisposable {
        private PwmPulsePolarity polarity;
        private double dutyCycle;

        public int ChannelNumber { get; }
        public PwmController Controller { get; }
        public bool IsStarted { get; private set; }

        internal PwmChannel(PwmController controller, int channelNumber) {
            this.ChannelNumber = channelNumber;
            this.Controller = controller;

            this.Controller.Provider.OpenChannel(channelNumber);
        }

        public void Dispose() => this.Controller.Provider.CloseChannel(this.ChannelNumber);

        public PwmPulsePolarity Polarity {
            get => this.polarity;
            set {
                this.polarity = value;
                this.Controller.Provider.SetPulseParameters(this.ChannelNumber, this.dutyCycle, this.polarity);
            }
        }

        public double GetActiveDutyCyclePercentage() => this.dutyCycle;

        public void SetActiveDutyCyclePercentage(double dutyCyclePercentage) {
            if (dutyCyclePercentage > 1.0 || dutyCyclePercentage < 0.0)
                throw new ArgumentException("dutyCyclePercentage has to be in range 0.0 to 1.0");

            this.dutyCycle = dutyCyclePercentage;
            this.Controller.Provider.SetPulseParameters(this.ChannelNumber, this.dutyCycle, this.polarity);
        }

        public void Start() {
            if (!this.IsStarted) {
                this.Controller.Provider.EnableChannel(this.ChannelNumber);
                this.IsStarted = true;
            }
        }

        public void Stop() {
            if (this.IsStarted) {
                this.Controller.Provider.DisableChannel(this.ChannelNumber);
                this.IsStarted = false;
            }
        }
    }

    public enum PwmPulsePolarity {
        ActiveHigh = 0,
        ActiveLow = 1,
    }

    namespace Provider {
        public interface IPwmControllerProvider : IDisposable {
            int ChannelCount { get; }
            double MinFrequency { get; }
            double MaxFrequency { get; }

            void OpenChannel(int channel);
            void CloseChannel(int channel);

            void EnableChannel(int channel);
            void DisableChannel(int channel);

            void SetPulseParameters(int channel, double dutyCycle, PwmPulsePolarity polarity);
            double SetDesiredFrequency(double frequency);

            double SetDesiredFrequency(int channel, double frequency);
        }

        public sealed class PwmControllerApiWrapper : IPwmControllerProvider {
            public NativeApi Api { get; }

            public PwmControllerApiWrapper(NativeApi api) => this.Api = api;

            public void Dispose() { }

            public int ChannelCount => 4;
            public double MinFrequency => 1.0;
            public double MaxFrequency => 1_000_000.0;

            public void OpenChannel(int channel) { }
            public void CloseChannel(int channel) { }
            public void EnableChannel(int channel) { }
            public void DisableChannel(int channel) { }

            public void SetPulseParameters(int channel, double dutyCycle, PwmPulsePolarity polarity) { }

            // Return the requested frequency unchanged so user code that
            // computes values based on ActualFrequency proceeds.
            public double SetDesiredFrequency(double frequency) => frequency;
            public double SetDesiredFrequency(int channel, double frequency) => frequency;
        }
    }
}
