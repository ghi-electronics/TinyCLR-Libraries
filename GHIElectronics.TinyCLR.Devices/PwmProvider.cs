using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
        private readonly static Hashtable providers = new Hashtable();

        public string Name { get; }

        public IPwmControllerProvider[] GetControllers() => this.controllers;

        private PwmProvider(string name) {
            this.Name = name;
            var api = Api.Find(this.Name, ApiType.PwmProvider);

            var controllerCount = DefaultPwmControllerProvider.GetControllerCount(api.Implementation);

            this.controllers = new IPwmControllerProvider[controllerCount];

            for (var i = 0; i < this.controllers.Length; i++)
                this.controllers[i] = new DefaultPwmControllerProvider(api.Implementation, i);

        }

        public static IPwmProvider FromId(string id) {
            if (PwmProvider.providers.Contains(id))
                return (IPwmProvider)PwmProvider.providers[id];

            var res = new PwmProvider(id);

            PwmProvider.providers[id] = res;

            return res;
        }
    }

    internal class DefaultPwmControllerProvider : IPwmControllerProvider {
#pragma warning disable CS0169
        private readonly IntPtr nativeProvider;
#pragma warning restore CS0169

        internal DefaultPwmControllerProvider(IntPtr nativeProvider, int idx) {
            this.nativeProvider = nativeProvider;
            this.idx = idx;
            this.AcquireNative();
        }

        ~DefaultPwmControllerProvider() => this.ReleaseNative();

        public double ActualFrequency { get; private set; }

        private int idx;

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
        extern private void AcquireNative();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void ReleaseNative();

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

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern static internal int GetControllerCount(IntPtr nativeProvider);
    }
}
