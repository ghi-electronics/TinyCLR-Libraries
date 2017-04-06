using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi.Provider;
using System;

namespace GHIElectronics.TinyCLR.Devices.Spi {
    public class SpiSoftwareProvider : ISpiProvider {
        private readonly GpioController controller;
        private readonly int miso;
        private readonly int mosi;
        private readonly int sck;
        private ISpiControllerProvider[] controllers;

        public SpiSoftwareProvider(int miso, int mosi, int sck) : this(GpioController.GetDefault(), miso, mosi, sck) { }

        public SpiSoftwareProvider(GpioController controller, int miso, int mosi, int sck) {
            this.controller = controller;
            this.miso = miso;
            this.mosi = mosi;
            this.sck = sck;
        }

        public ISpiControllerProvider[] GetControllers() => this.controllers = (this.controllers ?? new[] { new SpiSoftwareControllerProvider(this.controller, this.miso, this.mosi, this.sck) });
    }

    internal class SpiSoftwareControllerProvider : ISpiControllerProvider {
        private readonly GpioController controller;
        private readonly int miso;
        private readonly int mosi;
        private readonly int sck;

        public SpiSoftwareControllerProvider(GpioController controller, int miso, int mosi, int sck) {
            this.controller = controller;
            this.miso = miso;
            this.mosi = mosi;
            this.sck = sck;
        }

        public ISpiDeviceProvider GetDeviceProvider(ProviderSpiConnectionSettings settings) {
            GpioPin mosi = null, sck = null;

            if (this.controller.TryOpenPin(this.miso, GpioSharingMode.Exclusive, out var miso, out _) && this.controller.TryOpenPin(this.mosi, GpioSharingMode.Exclusive, out mosi, out _) && this.controller.TryOpenPin(this.sck, GpioSharingMode.Exclusive, out sck, out _) && this.controller.TryOpenPin(settings.ChipSelectionLine, GpioSharingMode.Exclusive, out var cs, out _))
                return new SpiManagedSoftwareDeviceProvider(settings, miso, mosi, sck, cs);

            miso?.Dispose();
            mosi?.Dispose();
            sck?.Dispose();

            throw new InvalidOperationException();
        }
    }

    internal class SpiManagedSoftwareDeviceProvider : ISpiDeviceProvider {
        private readonly GpioPin miso;
        private readonly GpioPin mosi;
        private readonly GpioPin sck;
        private readonly GpioPin cs;
        private readonly bool captureOnRisingEdge;
        private readonly GpioPinValue clockIdleState;
        private readonly GpioPinValue clockActiveState;
        private bool disposed;

        public SpiManagedSoftwareDeviceProvider(ProviderSpiConnectionSettings settings, GpioPin miso, GpioPin mosi, GpioPin sck, GpioPin cs) {
            if (settings.DataBitLength != 8) throw new NotSupportedException("Must use 8 bit mode.");
            if (settings.SharingMode != ProviderSpiSharingMode.Exclusive) throw new NotSupportedException("Must use exclusive mode.");

            this.ConnectionSettings = settings;
            this.miso = miso;
            this.mosi = mosi;
            this.sck = sck;
            this.cs = cs;
            this.captureOnRisingEdge = ((((int)settings.Mode) & 0x01) == 0);
            this.clockActiveState = (((int)settings.Mode) & 0x02) == 0 ? GpioPinValue.High : GpioPinValue.Low;
            this.clockIdleState = this.clockActiveState == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High;
            this.disposed = false;

            this.sck.SetDriveMode(GpioPinDriveMode.Output);
            this.sck.Write(this.clockIdleState);

            this.miso.SetDriveMode(GpioPinDriveMode.Input);
            this.miso.Read();

            this.mosi.SetDriveMode(GpioPinDriveMode.Output);
            this.mosi.Write(GpioPinValue.High);

            this.cs.SetDriveMode(GpioPinDriveMode.Output);
            this.cs.Write(GpioPinValue.High);
        }

        public ProviderSpiConnectionSettings ConnectionSettings { get; }
        public string DeviceId => $"SPI-SWM-{this.miso.PinNumber}-{this.mosi.PinNumber}-{this.sck.PinNumber}";

        public void Write(byte[] buffer) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.WriteRead(buffer, null, true);
        }

        public void Read(byte[] buffer) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.WriteRead(null, buffer, true);
        }

        public void TransferSequential(byte[] writeBuffer, byte[] readBuffer) {
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));

            this.WriteRead(writeBuffer, null, false);
            this.WriteRead(null, readBuffer, true);
        }

        public void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer) {
            if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));
            if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));

            this.WriteRead(writeBuffer, readBuffer, true);
        }

        private void WriteRead(byte[] writeBuffer, byte[] readBuffer, bool deselectAfter) {
            var writeLength = writeBuffer != null ? writeBuffer.Length : 0;
            var readLength = readBuffer != null ? readBuffer.Length : 0;

            if (readBuffer != null)
                Array.Clear(readBuffer, 0, readLength);

            this.sck.Write(this.clockIdleState);
            this.cs.Write(GpioPinValue.Low);

            for (var i = 0; i < Math.Max(readLength, writeLength); i++) {
                byte mask = 0x80;
                var w = i < writeLength && writeBuffer != null ? writeBuffer[i] : (byte)0;
                var r = false;

                for (var j = 0; j < 8; j++) {
                    if (this.captureOnRisingEdge) {
                        this.sck.Write(this.clockIdleState);

                        this.mosi.Write((w & mask) != 0 ? GpioPinValue.High : GpioPinValue.Low);
                        r = this.miso.Read() == GpioPinValue.High;

                        this.sck.Write(this.clockActiveState);
                    }
                    else {
                        this.sck.Write(this.clockActiveState);

                        this.mosi.Write((w & mask) != 0 ? GpioPinValue.High : GpioPinValue.Low);
                        r = this.miso.Read() == GpioPinValue.High;

                        this.sck.Write(this.clockIdleState);
                    }

                    if (i < readLength && readBuffer != null && r)
                        readBuffer[i] |= mask;

                    mask >>= 1;
                }
            }

            this.sck.Write(this.clockIdleState);

            if (deselectAfter)
                this.cs.Write(GpioPinValue.High);
        }

        public void Dispose() {
            if (!this.disposed) {
                this.miso.Dispose();
                this.mosi.Dispose();
                this.sck.Dispose();
                this.cs.Dispose();
                this.disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        ~SpiManagedSoftwareDeviceProvider() => this.Dispose();
    }
}
