using TinySpi = GHIElectronics.TinyCLR.Devices.Spi;
using System;
using GHIElectronics.TinyCLR.Native;

namespace System.Device.Spi {
    /// <summary>Bit ordering within an SPI frame.</summary>
    public enum DataFlow {
        /// <summary>Most-significant bit first.</summary>
        MsbFirst = 0,
        /// <summary>Least-significant bit first.</summary>
        LsbFirst = 1
    }

    /// <summary>Standard SPI mode — combinations of clock polarity and phase.</summary>
    public enum SpiMode {
        /// <summary>CPOL=0, CPHA=0.</summary>
        Mode0 = 0,
        /// <summary>CPOL=0, CPHA=1.</summary>
        Mode1 = 1,
        /// <summary>CPOL=1, CPHA=0.</summary>
        Mode2 = 2,
        /// <summary>CPOL=1, CPHA=1.</summary>
        Mode3 = 3
    }

    /// <summary>Per-device SPI settings in the standard <c>System.Device.Spi</c> shape. TinyCLR maps these onto its native SPI driver via <see cref="SpiDevice.Create(SpiConnectionSettings)"/>.</summary>
    public sealed class SpiConnectionSettings {
        public int BusId { get; }
        public int ChipSelectLine { get; set; }
        public int ClockFrequency { get; set; } = 500000;
        public int DataBitLength { get; set; } = 8;
        public DataFlow DataFlow { get; set; } = DataFlow.MsbFirst;
        public SpiMode Mode { get; set; } = SpiMode.Mode0;

        public SpiConnectionSettings(int busId, int chipSelectLine) {
            this.BusId = busId;
            this.ChipSelectLine = chipSelectLine;
        }
    }

    /// <summary>
    /// .NET-style SPI device. Standard surface (<c>Read</c> / <c>Write</c> / <c>TransferFullDuplex</c>);
    /// internally TinyCLR routes calls through <see cref="GHIElectronics.TinyCLR.Devices.Spi.SpiController"/>.
    /// </summary>
    public abstract class SpiDevice : IDisposable {
        public abstract SpiConnectionSettings ConnectionSettings { get; }

        public static SpiDevice Create(SpiConnectionSettings settings) => new TinyClrSpiDevice(settings);

        public abstract void Read(byte[] buffer);
        public abstract void Write(byte[] buffer);
        public abstract void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer);
        public abstract void Dispose();
    }

    internal sealed class TinyClrSpiDevice : SpiDevice {
        private readonly TinySpi.SpiController controller;
        private readonly TinySpi.SpiDevice device;
        private bool disposed;

        public override SpiConnectionSettings ConnectionSettings { get; }

        public TinyClrSpiDevice(SpiConnectionSettings settings) {
            this.ConnectionSettings = settings ?? throw new ArgumentNullException(nameof(settings));

            //if (settings.ChipSelectLine >= 0)
            //    throw CreateTodoNotSupportedException("SPI ChipSelectLine integer mapping to TinyCLR GpioPin.");

            //if (settings.DataBitLength != 8)
            //    throw CreateTodoNotSupportedException("SPI DataBitLength other than 8.");

            this.controller = ResolveController(settings.BusId);
            this.device = this.controller.GetDevice(new TinySpi.SpiConnectionSettings {
                ChipSelectType = TinySpi.SpiChipSelectType.None,
                ClockFrequency = settings.ClockFrequency,
                DataFrameFormat = settings.DataFlow == DataFlow.MsbFirst ? TinySpi.SpiDataFrame.MsbFirst : TinySpi.SpiDataFrame.LsbFirst,
                Mode = (TinySpi.SpiMode)settings.Mode
            });
        }

        public override void Read(byte[] buffer) {
            this.ThrowIfDisposed();
            this.device.Read(buffer);
        }

        public override void Write(byte[] buffer) {
            this.ThrowIfDisposed();
            this.device.Write(buffer);
        }

        public override void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer) {
            this.ThrowIfDisposed();
            this.device.TransferFullDuplex(writeBuffer, readBuffer);
        }

        public override void Dispose() {
            if (this.disposed)
                return;

            this.device.Dispose();
            this.controller.Dispose();
            this.disposed = true;
        }

        private void ThrowIfDisposed() {
            if (this.disposed)
                throw new ObjectDisposedException(nameof(SpiDevice));
        }

        private static TinySpi.SpiController ResolveController(int busId) {
            if (busId < -1)
                throw new ArgumentOutOfRangeException(nameof(busId));

            if (busId == -1)
                return TinySpi.SpiController.FromName("GHIElectronics.TinyCLR.NativeApis.SoftwareSpiController");

            var deviceName = DeviceInformation.DeviceName;
            if (string.IsNullOrEmpty(deviceName))
                throw new InvalidOperationException("DeviceInformation.DeviceName is not available.");

            var apiFamily = IsSc13Device(deviceName) ? "STM32L4" : "STM32H7";
            var controllerName = "GHIElectronics.TinyCLR.NativeApis." + apiFamily + ".SpiController\\" + busId.ToString();
            return TinySpi.SpiController.FromName(controllerName);
        }

        private static bool IsSc13Device(string deviceName) =>
            deviceName.Length >= 4 &&
            deviceName[0] == 'S' &&
            deviceName[1] == 'C' &&
            deviceName[2] == '1' &&
            deviceName[3] == '3';

        private static NotSupportedException CreateTodoNotSupportedException(string feature) =>
            new NotSupportedException("TODO-Not supported: " + feature);
    }
}
