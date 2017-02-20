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
        private double[] dutyCycles;
        private bool[] inverts;
        private bool[] actives;

        public DefaultPwmControllerProvider() {
            this.dutyCycles = new double[this.PinCount];
            this.inverts = new bool[this.PinCount];
            this.actives = new bool[this.PinCount];
        }

        public extern double ActualFrequency {
            [MethodImpl(MethodImplOptions.InternalCall)]
            get;
        }

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
            var result = this.SetDesiredFrequencyInternal(frequency);

            for (var i = 0; i < this.PinCount; i++)
                if (this.actives[i])
                    this.SetPulseParameters(i, this.dutyCycles[i], this.inverts[i]);

            return result;
        }

        public void SetPulseParameters(int pin, double dutyCycle, bool invertPolarity) {
            this.SetPulseParametersInternal(pin, dutyCycle, invertPolarity);

            this.dutyCycles[pin] = dutyCycle;
            this.inverts[pin] = invertPolarity;
        }

        public void AcquirePin(int pin) {
            this.AcquirePinInternal(pin);

            this.actives[pin] = true;
        }

        public void ReleasePin(int pin) {
            this.ReleasePinInternal(pin);

            this.actives[pin] = false;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void DisablePin(int pin);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void EnablePin(int pin);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void AcquirePinInternal(int pin);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void ReleasePinInternal(int pin);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern double SetDesiredFrequencyInternal(double frequency);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void SetPulseParametersInternal(int pin, double dutyCycle, bool invertPolarity);
    }
}
