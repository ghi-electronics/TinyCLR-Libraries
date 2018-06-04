using System;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Pwm.Provider;

namespace GHIElectronics.TinyCLR.Devices.Pwm {
    public sealed class PwmController {
        private IPwmControllerProvider m_provider;

        internal PwmController(IPwmControllerProvider provider) => this.m_provider = provider;

        public int PinCount => this.m_provider.PinCount;

        public double MinFrequency => this.m_provider.MinFrequency;

        public double MaxFrequency => this.m_provider.MaxFrequency;

        public double ActualFrequency => this.m_provider.ActualFrequency;

        public static string GetDeviceSelector() => throw new NotSupportedException();
        public static string GetDeviceSelector(string friendlyName) => throw new NotSupportedException();

        public static PwmController FromId(string deviceId) => Api.ParseSelector(deviceId, out var providerId, out var idx) ? new PwmController(PwmProvider.FromId(providerId).GetController((int)idx)) : null;

        public static PwmController GetDefault() => LowLevelDevicesController.DefaultProvider?.PwmControllerProvider != null ? new PwmController(LowLevelDevicesController.DefaultProvider?.PwmControllerProvider) : PwmController.FromId(Api.GetDefaultSelector(ApiType.PwmProvider));

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
