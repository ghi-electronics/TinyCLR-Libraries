using System;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi.Provider;

namespace GHIElectronics.TinyCLR.Devices.Spi {
    public class SpiSoftwareProvider : ISpiProvider {
        private readonly GpioController gpioController;
        private readonly int miso;
        private readonly int mosi;
        private readonly int sck;
        private ISpiControllerProvider[] spiController;

        public SpiSoftwareProvider(int miso, int mosi, int sck) : this(GpioController.GetDefault(), miso, mosi, sck) { }

        public SpiSoftwareProvider(GpioController controller, int miso, int mosi, int sck) {
            this.gpioController = controller;
            this.miso = miso;
            this.mosi = mosi;
            this.sck = sck;
        }

        public ISpiControllerProvider[] GetControllers() => this.spiController = (this.spiController ?? new[] { new SpiSoftwareControllerProvider(this.gpioController, this.miso, this.mosi, this.sck) });
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

        public void Read(byte[] buffer) => this.Read(buffer, 0, buffer != null ? buffer.Length : 0);
        public void Write(byte[] buffer) => this.Write(buffer, 0, buffer != null ? buffer.Length : 0);
        public void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer) => this.TransferFullDuplex(writeBuffer, 0, readBuffer, 0, writeBuffer != null ? writeBuffer.Length : 0);
        public void TransferSequential(byte[] writeBuffer, byte[] readBuffer) => this.TransferSequential(writeBuffer, 0, writeBuffer != null ? writeBuffer.Length : 0, readBuffer, 0, readBuffer != null ? readBuffer.Length : 0);

        public void Write(byte[] buffer, int offset, int length) {
            if (buffer == null) throw new ArgumentOutOfRangeException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (buffer.Length < offset + length) throw new ArgumentException(nameof(buffer));

            this.WriteRead(buffer, 0, buffer.Length, null, 0, 0, true);
        }

        public void Read(byte[] buffer, int offset, int length) {
            if (buffer == null) throw new ArgumentOutOfRangeException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (buffer.Length < offset + length) throw new ArgumentException(nameof(buffer));

            this.WriteRead(null, 0, 0, buffer, 0, buffer.Length, true);
        }

        public void TransferFullDuplex(byte[] writeBuffer, int writeOffset, byte[] readBuffer, int readOffset, int length) {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            if (writeBuffer == null) throw new ArgumentOutOfRangeException(nameof(writeBuffer));
            if (writeOffset < 0) throw new ArgumentOutOfRangeException(nameof(writeOffset));
            if (writeBuffer.Length < writeOffset + length) throw new ArgumentException(nameof(writeBuffer));

            if (readBuffer == null) throw new ArgumentOutOfRangeException(nameof(readBuffer));
            if (readOffset < 0) throw new ArgumentOutOfRangeException(nameof(writeOffset));
            if (readBuffer.Length < readOffset + length) throw new ArgumentException(nameof(readBuffer));

            this.WriteRead(writeBuffer, writeOffset, length, null, 0, 0, false);
            this.WriteRead(null, 0, 0, readBuffer, readOffset, length, true);
        }

        public void TransferSequential(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            if (writeBuffer == null) throw new ArgumentOutOfRangeException(nameof(writeBuffer));
            if (writeOffset < 0) throw new ArgumentOutOfRangeException(nameof(writeOffset));
            if (writeLength < 0) throw new ArgumentOutOfRangeException(nameof(writeLength));
            if (writeBuffer.Length < writeOffset + writeLength) throw new ArgumentException(nameof(writeBuffer));

            if (readBuffer == null) throw new ArgumentOutOfRangeException(nameof(readBuffer));
            if (readOffset < 0) throw new ArgumentOutOfRangeException(nameof(readOffset));
            if (readLength < 0) throw new ArgumentOutOfRangeException(nameof(readLength));
            if (readBuffer.Length < readOffset + readLength) throw new ArgumentException(nameof(readBuffer));

            this.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, true);
        }

        private void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter) {
            if (readBuffer != null)
                Array.Clear(readBuffer, 0, readLength);

            this.sck.Write(this.clockIdleState);
            this.cs.Write(GpioPinValue.Low);

            for (var i = 0; i < Math.Max(readLength, writeLength); i++) {
                byte mask = 0x80;
                var w = i < writeLength && writeBuffer != null ? writeBuffer[i + writeOffset] : (byte)0;
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
                        readBuffer[i + readOffset] |= mask;

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
