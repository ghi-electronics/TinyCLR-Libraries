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

    internal class DefaultPwmControllerProvider : IPwmControllerProvider {
        private readonly int timer;
        private double[] dutyCycles;
        private bool[] inverts;
        private bool[] actives;

        public DefaultPwmControllerProvider(int timer) {
            if (!DefaultPwmControllerProvider.IsTimerValid(timer)) throw new ArgumentException("Invalid timer.", nameof(timer));

            this.timer = timer;
            this.dutyCycles = new double[this.PinCount];
            this.inverts = new bool[this.PinCount];
            this.actives = new bool[this.PinCount];
        }

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

        public double SetDesiredFrequency(double frequency) {
            this.ActualFrequency = this.SetDesiredFrequencyInternal(frequency);

            for (var i = 0; i < this.PinCount; i++)
                if (this.actives[i])
                    this.SetPulseParameters(i, this.dutyCycles[i], this.inverts[i]);

            return this.ActualFrequency;
        }

        public void SetPulseParameters(int pinNumber, double dutyCycle, bool invertPolarity) {
            this.SetPulseParametersInternal(pinNumber, dutyCycle, invertPolarity);

            this.dutyCycles[pinNumber] = dutyCycle;
            this.inverts[pinNumber] = invertPolarity;
        }

        public void AcquirePin(int pinNumber) {
            this.AcquirePinInternal(pinNumber);

            this.actives[pinNumber] = true;
        }

        public void ReleasePin(int pinNumber) {
            this.ReleasePinInternal(pinNumber);

            this.actives[pinNumber] = false;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void DisablePin(int pinNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void EnablePin(int pinNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void AcquirePinInternal(int pinNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void ReleasePinInternal(int pinNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern double SetDesiredFrequencyInternal(double frequency);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void SetPulseParametersInternal(int pinNumber, double dutyCycle, bool invertPolarity);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static bool IsTimerValid(int timer);
    }
}
