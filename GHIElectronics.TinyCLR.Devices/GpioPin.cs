using GHIElectronics.TinyCLR.Devices.Gpio.Provider;
using System;

namespace GHIElectronics.TinyCLR.Devices.Gpio {
    public delegate void GpioPinValueChangedEventHandler(GpioPin sender, GpioPinValueChangedEventArgs e);

    public sealed class GpioPin : IDisposable {
        private readonly IGpioPinProvider provider;
        private GpioPinValueChangedEventHandler callbacks;

        internal GpioPin(IGpioPinProvider provider) => this.provider = provider;

        public TimeSpan DebounceTimeout { get => this.provider.DebounceTimeout; set => this.provider.DebounceTimeout = this.SharingMode != GpioSharingMode.SharedReadOnly ? value : throw new InvalidOperationException("In shared read only mode."); }
        public int PinNumber => this.provider.PinNumber;
        public GpioSharingMode SharingMode => (GpioSharingMode)this.provider.SharingMode;

        public void Dispose() => this.provider.Dispose();
        public bool IsDriveModeSupported(GpioPinDriveMode driveMode) => this.provider.IsDriveModeSupported((ProviderGpioPinDriveMode)driveMode);
        public GpioPinDriveMode GetDriveMode() => (GpioPinDriveMode)this.provider.GetDriveMode();
        public void SetDriveMode(GpioPinDriveMode value) { if (this.SharingMode == GpioSharingMode.SharedReadOnly) throw new InvalidOperationException("In shared read only mode."); this.provider.SetDriveMode((ProviderGpioPinDriveMode)value); }
        public GpioPinValue Read() => (GpioPinValue)this.provider.Read();

        public void Write(GpioPinValue value) {
            if (this.SharingMode == GpioSharingMode.SharedReadOnly) throw new InvalidOperationException("In shared read only mode.");

            var init = this.Read();

            this.provider.Write((ProviderGpioPinValue)value);

            var same = init == value;

            if (!same)
                this.OnValueChanged(this.provider, new GpioPinProviderValueChangedEventArgs(value == GpioPinValue.High ? ProviderGpioPinEdge.RisingEdge : ProviderGpioPinEdge.FallingEdge));
        }

        public event GpioPinValueChangedEventHandler ValueChanged {
            add {
                if (this.callbacks == null)
                    this.provider.ValueChanged += this.OnValueChanged;

                this.callbacks += value;
            }
            remove {
                this.callbacks -= value;

                if (this.callbacks == null)
                    this.provider.ValueChanged -= this.OnValueChanged;
            }
        }

        private void OnValueChanged(IGpioPinProvider sender, GpioPinProviderValueChangedEventArgs e) => this.callbacks?.Invoke(this, new GpioPinValueChangedEventArgs((GpioPinEdge)e.Edge));
    }
}
