using GHIElectronics.TinyCLR.Devices.Pwm.Provider;
using System;

namespace GHIElectronics.TinyCLR.Devices.Pwm {
    public sealed class PwmController {
        private static string PwmPrefix => "TIM";

        private IPwmControllerProvider m_provider;

        internal PwmController(IPwmControllerProvider provider) => this.m_provider = provider;

        public int PinCount => this.m_provider.PinCount;

        public double MinFrequency => this.m_provider.MinFrequency;

        public double MaxFrequency => this.m_provider.MaxFrequency;

        public double ActualFrequency => this.m_provider.ActualFrequency;

        public static string GetDeviceSelector() => PwmController.PwmPrefix;
        public static string GetDeviceSelector(string friendlyName) => friendlyName;

        public static PwmController FromId(string deviceId) {
            if (deviceId == null) throw new ArgumentNullException(nameof(deviceId));
            if (deviceId.Length < 4 || deviceId.IndexOf(PwmController.PwmPrefix) != 0 || !int.TryParse(deviceId.Substring(PwmController.PwmPrefix.Length), out var id) || id <= 0) throw new ArgumentException("Invalid device ID.", nameof(deviceId));

            return new PwmController(new DefaultPwmControllerProvider(id));
        }

        public static PwmController GetDefault() => throw new NotSupportedException();

        public static PwmController[] GetControllers(IPwmProvider provider) {
            // FUTURE: This should return "Task<IReadOnlyList<PwmController>>"

            var providers = provider.GetControllers();
            var controllers = new PwmController[providers.Length];

            for (var i = 0; i < providers.Length; ++i) {
                controllers[i] = new PwmController(providers[i]);
            }

            return controllers;
        }

        public double SetDesiredFrequency(double desiredFrequency) {
            if ((desiredFrequency < this.m_provider.MinFrequency) || (desiredFrequency > this.m_provider.MaxFrequency)) {
                throw new ArgumentOutOfRangeException();
            }

            return this.m_provider.SetDesiredFrequency(desiredFrequency);
        }

        public PwmPin OpenPin(int pinNumber) {
            if ((pinNumber < 0) || (pinNumber >= this.m_provider.PinCount)) {
                throw new ArgumentOutOfRangeException();
            }

            return new PwmPin(this, this.m_provider, pinNumber);
        }
    }
}
