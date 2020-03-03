using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Pwm.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Pwm {
    public sealed class PwmController : IDisposable {
        public IPwmControllerProvider Provider { get; }

        private PwmController(IPwmControllerProvider provider) => this.Provider = provider;

        public static PwmController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.PwmController) is PwmController c ? c : PwmController.FromName(NativeApi.GetDefaultName(NativeApiType.PwmController));
        public static PwmController FromName(string name) => PwmController.FromProvider(new PwmControllerApiWrapper(NativeApi.Find(name, NativeApiType.PwmController)));
        public static PwmController FromProvider(IPwmControllerProvider provider) => new PwmController(provider);

        public double ActualFrequency { get; private set; }

        public int ChannelCount => this.Provider.ChannelCount;
        public double MinFrequency => this.Provider.MinFrequency;
        public double MaxFrequency => this.Provider.MaxFrequency;

        public void Dispose() => this.Provider.Dispose();

        public double SetDesiredFrequency(double desiredFrequency) => this.ActualFrequency = this.Provider.SetDesiredFrequency(desiredFrequency);

        public PwmChannel OpenChannel(int channelNumber) => new PwmChannel(this, channelNumber);
    }

    public sealed class PwmChannel : IDisposable {
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
        }

        public sealed class PwmControllerApiWrapper : IPwmControllerProvider {
            private readonly IntPtr impl;

            public NativeApi Api { get; }

            public PwmControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            public extern int ChannelCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern double MinFrequency { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern double MaxFrequency { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void OpenChannel(int channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void CloseChannel(int channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void EnableChannel(int channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DisableChannel(int channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetPulseParameters(int channel, double dutyCycle, PwmPulsePolarity polarity);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern double SetDesiredFrequency(double frequency);
        }
    }
}
