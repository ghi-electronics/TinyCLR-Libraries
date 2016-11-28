using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.Pwm.Provider
{
    public interface IPwmControllerProvider
    {
        int PinCount
        {
            get;
        }

        double MinFrequency
        {
            get;
        }

        double MaxFrequency
        {
            get;
        }

        double ActualFrequency
        {
            get;
        }

        double SetDesiredFrequency(double frequency);
        void AcquirePin(int pin);
        void ReleasePin(int pin);
        void EnablePin(int pin);
        void DisablePin(int pin);
        void SetPulseParameters(int pin, double dutyCycle, bool invertPolarity);
    }

    public interface IPwmProvider
    {
        // FUTURE: This should return "IReadOnlyList<IPwmControllerProvider>"
        IPwmControllerProvider[] GetControllers();
    }

    internal class DefaultPwmControllerProvider : IPwmControllerProvider {
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

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void AcquirePin(int pin);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void DisablePin(int pin);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void EnablePin(int pin);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void ReleasePin(int pin);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern double SetDesiredFrequency(double frequency);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void SetPulseParameters(int pin, double dutyCycle, bool invertPolarity);
    }
}
