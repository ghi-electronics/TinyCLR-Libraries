using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Pwm.Provider;

namespace GHIElectronics.TinyCLR.Devices.Pwm {
    public sealed class PwmController : IDisposable {
        public IPwmControllerProvider Provider { get; }

        private PwmController(IPwmControllerProvider provider) => this.Provider = provider;

        public static PwmController GetDefault() => Api.GetDefaultFromCreator(ApiType.PwmController) is PwmController c ? c : PwmController.FromName(Api.GetDefaultName(ApiType.PwmController));
        public static PwmController FromName(string name) => PwmController.FromProvider(new PwmControllerApiWrapper(Api.Find(name, ApiType.PwmController)));
        public static PwmController FromProvider(IPwmControllerProvider provider) => new PwmController(provider);

        public double ActualFrequency { get; private set; }

        public uint ChannelCount => this.Provider.ChannelCount;
        public double MinFrequency => this.Provider.MinFrequency;
        public double MaxFrequency => this.Provider.MaxFrequency;

        public void Dispose() => this.Provider.Dispose();

        public double SetDesiredFrequency(double desiredFrequency) => this.ActualFrequency = this.Provider.SetDesiredFrequency(desiredFrequency);

        public PwmChannel OpenChannel(uint channelNumber) => new PwmChannel(this, channelNumber);
    }

    public sealed class PwmChannel : IDisposable {
        private PwmPulsePolarity polarity;
        private double dutyCycle;

        public uint ChannelNumber { get; }
        public PwmController Controller { get; }
        public bool IsStarted { get; private set; }

        internal PwmChannel(PwmController controller, uint channelNumber) {
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
            uint ChannelCount { get; }
            double MinFrequency { get; }
            double MaxFrequency { get; }

            void OpenChannel(uint channel);
            void CloseChannel(uint channel);

            void EnableChannel(uint channel);
            void DisableChannel(uint channel);

            void SetPulseParameters(uint channel, double dutyCycle, PwmPulsePolarity polarity);
            double SetDesiredFrequency(double frequency);
        }

        public sealed class PwmControllerApiWrapper : IPwmControllerProvider {
            private readonly IntPtr impl;

            public Api Api { get; }

            public PwmControllerApiWrapper(Api api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            public extern uint ChannelCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern double MinFrequency { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern double MaxFrequency { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void OpenChannel(uint channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void CloseChannel(uint channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void EnableChannel(uint channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DisableChannel(uint channel);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetPulseParameters(uint channel, double dutyCycle, PwmPulsePolarity polarity);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern double SetDesiredFrequency(double frequency);
        }
    }
}
