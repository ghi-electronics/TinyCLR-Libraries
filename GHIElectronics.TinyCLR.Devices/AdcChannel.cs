using GHIElectronics.TinyCLR.Devices.Adc.Provider;
using System;

namespace GHIElectronics.TinyCLR.Devices.Adc {
    public sealed class AdcChannel : IDisposable {
        private readonly int m_channelNumber;
        private AdcController m_controller;
        private IAdcControllerProvider m_provider;
        private bool m_disposed = false;

        internal AdcChannel(AdcController controller, IAdcControllerProvider provider, int channelNumber) {
            this.m_controller = controller;
            this.m_provider = provider;
            this.m_channelNumber = channelNumber;

            this.m_provider.AcquireChannel(channelNumber);
        }

        ~AdcChannel() {
            Dispose(false);
        }

        public AdcController Controller {
            get {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                return this.m_controller;
            }
        }

        public int ReadValue() {
            if (this.m_disposed) {
                throw new ObjectDisposedException();
            }

            return this.m_provider.ReadValue(this.m_channelNumber);
        }

        public double ReadRatio() {
            if (this.m_disposed) {
                throw new ObjectDisposedException();
            }

            return ((double)this.m_provider.ReadValue(this.m_channelNumber)) / this.m_provider.MaxValue;
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
                this.m_provider.ReleaseChannel(this.m_channelNumber);
                this.m_controller = null;
                this.m_provider = null;
            }
        }
    }
}
