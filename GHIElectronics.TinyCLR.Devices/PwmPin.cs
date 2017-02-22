using GHIElectronics.TinyCLR.Devices.Pwm.Provider;
using System;

namespace GHIElectronics.TinyCLR.Devices.Pwm {
    public sealed class PwmPin : IDisposable {
        private readonly int m_pinNumber;
        private PwmController m_controller;
        private IPwmControllerProvider m_provider;
        private bool m_started = false;
        private bool m_disposed = false;
        private double m_dutyCycle = 0;
        private PwmPulsePolarity m_polarity = PwmPulsePolarity.ActiveHigh;

        internal PwmPin(PwmController controller, IPwmControllerProvider provider, int pinNumber) {
            this.m_controller = controller;
            this.m_provider = provider;
            this.m_pinNumber = pinNumber;

            this.m_provider.AcquirePin(pinNumber);
        }

        ~PwmPin() {
            Dispose(false);
        }

        public PwmController Controller {
            get {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                return this.m_controller;
            }
        }

        public PwmPulsePolarity Polarity {
            get {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                return this.m_polarity;
            }

            set {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                switch (value) {
                    case PwmPulsePolarity.ActiveHigh:
                    case PwmPulsePolarity.ActiveLow:
                        break;

                    default:
                        throw new ArgumentException();
                }

                this.m_polarity = value;

                if (this.m_started) {
                    this.m_provider.SetPulseParameters(this.m_pinNumber, this.m_dutyCycle, value == PwmPulsePolarity.ActiveLow);
                }
            }
        }

        public bool IsStarted {
            get {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                return this.m_started;
            }
        }

        public void Start() {
            if (this.m_disposed) {
                throw new ObjectDisposedException();
            }

            if (!this.m_started) {
                this.m_provider.EnablePin(this.m_pinNumber);
                this.m_provider.SetPulseParameters(this.m_pinNumber, this.m_dutyCycle, this.m_polarity == PwmPulsePolarity.ActiveLow);
                this.m_started = true;
            }
        }

        public void Stop() {
            if (this.m_disposed) {
                throw new ObjectDisposedException();
            }

            if (this.m_started) {
                this.m_provider.DisablePin(this.m_pinNumber);
                this.m_started = false;
            }
        }

        public double GetActiveDutyCyclePercentage() {
            if (this.m_disposed) {
                throw new ObjectDisposedException();
            }

            return this.m_dutyCycle;
        }

        public void SetActiveDutyCyclePercentage(double dutyCyclePercentage) {
            if (this.m_disposed) {
                throw new ObjectDisposedException();
            }

            if ((dutyCyclePercentage < 0) || (dutyCyclePercentage > 1)) {
                throw new ArgumentOutOfRangeException();
            }

            this.m_dutyCycle = dutyCyclePercentage;

            if (this.m_started) {
                this.m_provider.SetPulseParameters(this.m_pinNumber, this.m_dutyCycle, this.m_polarity == PwmPulsePolarity.ActiveLow);
            }
        }

        public void Dispose() {
            if (!this.m_disposed) {
                Dispose(true);
                GC.SuppressFinalize(this);
                this.m_disposed = true;
            }
        }

        private void Dispose(bool disposing) {
            if (disposing) {
                this.m_provider.ReleasePin(this.m_pinNumber);
                this.m_controller = null;
                this.m_provider = null;
            }
        }
    }
}
