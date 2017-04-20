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
        internal static string PwmPrefix => "CON";

        private readonly string deviceId;
        private readonly int controller;

        public static IPwmControllerProvider[] Instances { get; }

        static DefaultPwmControllerProvider() {
            var deviceIds = DefaultPwmControllerProvider.GetDeviceIds();

            DefaultPwmControllerProvider.Instances = new IPwmControllerProvider[deviceIds.Length];

            for (var i = 0; i < deviceIds.Length; i++)
                DefaultPwmControllerProvider.Instances[i] = new DefaultPwmControllerProvider("CON" + deviceIds[i].ToString());
        }

        public static IPwmControllerProvider FindById(string deviceId) {
            for (var i = 0; i < DefaultPwmControllerProvider.Instances.Length; i++) {
                var inst = (DefaultPwmControllerProvider)DefaultPwmControllerProvider.Instances[i];

                if (inst.deviceId == deviceId)
                    return inst;
            }

            return null;
        }

        private DefaultPwmControllerProvider(string deviceId) {
            if (deviceId == null) throw new ArgumentNullException(nameof(deviceId));
            if (deviceId.Length < 4 || deviceId.IndexOf(DefaultPwmControllerProvider.PwmPrefix) != 0 || !int.TryParse(deviceId.Substring(DefaultPwmControllerProvider.PwmPrefix.Length), out var controller) || controller < 0) throw new ArgumentException("Invalid device ID.", nameof(deviceId));
            if (!DefaultPwmControllerProvider.IsTimerValid(controller)) throw new ArgumentException("Invalid controller.", nameof(DefaultPwmControllerProvider.controller));

            this.deviceId = deviceId;
            this.controller = controller;
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
        private extern static bool IsTimerValid(int timer);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int[] GetDeviceIds();
    }
}
