using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.Pwm.Provider {
    public interface IPwmControllerProvider {
        int PinCount {
            get;
        }

        double MinFrequency {
            get;
        }

        double MaxFrequency {
            get;
        }

        double ActualFrequency {
            get;
        }

        double SetDesiredFrequency(double frequency);
        void AcquirePin(int pin);
        void ReleasePin(int pin);
        void EnablePin(int pin);
        void DisablePin(int pin);
        void SetPulseParameters(int pin, double dutyCycle, bool invertPolarity);
    }

    public interface IPwmProvider {
        // FUTURE: This should return "IReadOnlyList<IPwmControllerProvider>"
        IPwmControllerProvider[] GetControllers();
    }

    public class PwmProvider : IPwmProvider {
        private IPwmControllerProvider[] controllers;

        public string Name { get; }

        public IPwmControllerProvider[] GetControllers() => this.controllers;

        private PwmProvider(string name) {
            this.Name = name;
            this.controllers = new IPwmControllerProvider[DefaultPwmControllerProvider.GetControllerCount(name)];

            for (var i = 0U; i < this.controllers.Length; i++)
                this.controllers[i] = new DefaultPwmControllerProvider(name, i);
        }

        public static IPwmProvider FromId(string id) => new PwmProvider(id);
    }

    internal class DefaultPwmControllerProvider : IPwmControllerProvider {
#pragma warning disable CS0169
        private IntPtr nativeProvider;
#pragma warning restore CS0169

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern uint GetControllerCount(string providerName);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern DefaultPwmControllerProvider(string name, uint index);

        public double ActualFrequency { get; private set; }

        public extern double MaxFrequency {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        public extern double MinFrequency {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        public extern int PinCount {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void DisablePin(int pinNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void EnablePin(int pinNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void AcquirePin(int pinNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void ReleasePin(int pinNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern double SetDesiredFrequency(double frequency);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetPulseParameters(int pinNumber, double dutyCycle, bool invertPolarity);
    }
}
