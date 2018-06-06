using System;
using GHIElectronics.TinyCLR.Devices.Dac.Provider;

namespace GHIElectronics.TinyCLR.Devices.Dac {
    public sealed class DacChannel : IDisposable {
        private readonly int channel;
        private bool disposed;
        private DacController controller;
        private IDacControllerProvider provider;

        internal DacChannel(IDacControllerProvider provider, DacController controller, int channel) {
            this.channel = channel;
            this.disposed = false;
            this.controller = controller;
            this.provider = provider;

            this.provider.AcquireChannel(channel);
        }

        ~DacChannel() => this.Dispose(false);

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (this.disposed)
                return;

            if (disposing) {
                this.provider.ReleaseChannel(this.channel);

                this.provider = null;
                this.controller = null;
            }

            this.disposed = true;
        }

        public DacController Controller => this.controller;

        public int LastWrittenValue { get; private set; }

        public void WriteValue(int value) {
            if (this.disposed)
                throw new ObjectDisposedException();
            if (value < this.provider.MinValue || value > this.provider.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value));

            this.LastWrittenValue = value;

            this.provider.WriteValue(this.channel, this.LastWrittenValue);
        }

        public void WriteValue(double ratio) {
            if (this.disposed)
                throw new ObjectDisposedException();
            if (ratio < 0.0 || ratio > 1.0)
                throw new ArgumentOutOfRangeException(nameof(ratio));

            this.WriteValue((int)(ratio * (this.provider.MaxValue - this.provider.MinValue)) + this.provider.MinValue);
        }
    }
}
