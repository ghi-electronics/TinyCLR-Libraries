using TinyI2c = GHIElectronics.TinyCLR.Devices.I2c;
using System;
using GHIElectronics.TinyCLR.Native;

namespace System.Device.I2c {
    /// <summary>Standard .NET-style I²C bus speed selector.</summary>
    public enum I2cBusSpeed {
        /// <summary>100 kHz standard mode.</summary>
        StandardMode = 0,
        /// <summary>400 kHz fast mode.</summary>
        FastMode = 1,
        /// <summary>1 MHz fast mode plus.</summary>
        FastModePlus = 2
    }

    /// <summary>Per-device I²C settings in the standard <c>System.Device.I2c</c> shape. TinyCLR maps these onto its native I²C driver via <see cref="I2cDevice.Create(I2cConnectionSettings)"/>.</summary>
    public sealed class I2cConnectionSettings {
        /// <summary>The bus this device is on (-1 for software I²C).</summary>
        public int BusId { get; }
        /// <summary>The 7-bit device address.</summary>
        public int DeviceAddress { get; set; }
        /// <summary>The bus clock speed. Defaults to standard mode.</summary>
        public I2cBusSpeed BusSpeed { get; set; } = I2cBusSpeed.StandardMode;

        /// <summary>Creates settings for a device at the given address on the given bus.</summary>
        public I2cConnectionSettings(int busId, int deviceAddress) {
            this.BusId = busId;
            this.DeviceAddress = deviceAddress;
        }
    }

    /// <summary>
    /// .NET-style I²C device. Standard surface (<c>Read</c> / <c>Write</c> / <c>WriteRead</c>);
    /// internally TinyCLR routes calls through <see cref="GHIElectronics.TinyCLR.Devices.I2c.I2cController"/>.
    /// </summary>
    public abstract class I2cDevice : IDisposable {
        /// <summary>The settings this device was created with.</summary>
        public abstract I2cConnectionSettings ConnectionSettings { get; }

        /// <summary>Opens an I²C device with the given settings.</summary>
        public static I2cDevice Create(I2cConnectionSettings settings) => new TinyClrI2cDevice(settings);

        /// <summary>Reads bytes from the device into the buffer.</summary>
        public abstract void Read(byte[] buffer);
        /// <summary>Writes the buffer to the device.</summary>
        public abstract void Write(byte[] buffer);
        /// <summary>Writes, then reads back in a single transaction.</summary>
        public abstract void WriteRead(byte[] writeBuffer, byte[] readBuffer);
        /// <summary>Closes the device and releases the bus.</summary>
        public abstract void Dispose();
    }

    internal sealed class TinyClrI2cDevice : I2cDevice {
        private readonly TinyI2c.I2cController controller;
        private readonly TinyI2c.I2cDevice device;
        private bool disposed;

        public override I2cConnectionSettings ConnectionSettings { get; }

        public TinyClrI2cDevice(I2cConnectionSettings settings) {
            this.ConnectionSettings = settings ?? throw new ArgumentNullException(nameof(settings));

            this.controller = ResolveController(settings.BusId);
            this.device = this.controller.GetDevice(new TinyI2c.I2cConnectionSettings(
                settings.DeviceAddress,
                TinyI2c.I2cMode.Master,
                TinyI2c.I2cAddressFormat.SevenBit,
                MapBusSpeed(settings.BusSpeed)
            ));
        }

        public override void Read(byte[] buffer) {
            this.ThrowIfDisposed();
            this.device.Read(buffer);
        }

        public override void Write(byte[] buffer) {
            this.ThrowIfDisposed();
            this.device.Write(buffer);
        }

        public override void WriteRead(byte[] writeBuffer, byte[] readBuffer) {
            this.ThrowIfDisposed();
            this.device.WriteRead(writeBuffer, readBuffer);
        }

        public override void Dispose() {
            if (this.disposed)
                return;

            this.device.Dispose();
            this.controller.Dispose();
            this.disposed = true;
        }

        private static uint MapBusSpeed(I2cBusSpeed speed) {
            switch (speed) {
                case I2cBusSpeed.StandardMode: return 100000;
                case I2cBusSpeed.FastMode: return 400000;
                case I2cBusSpeed.FastModePlus: return 1000000;
                default: throw new ArgumentOutOfRangeException(nameof(speed));
            }
        }

        private void ThrowIfDisposed() {
            if (this.disposed)
                throw new ObjectDisposedException(nameof(I2cDevice));
        }

        private static TinyI2c.I2cController ResolveController(int busId) {
            if (busId < -1)
                throw new ArgumentOutOfRangeException(nameof(busId));

            if (busId == -1)
                return TinyI2c.I2cController.FromName("GHIElectronics.TinyCLR.NativeApis.SoftwareI2cController");

            var family = DeviceInformation.DeviceFamily;
            if (string.IsNullOrEmpty(family))
                throw new InvalidOperationException("DeviceInformation.DeviceFamily is not available.");

            var controllerName = "GHIElectronics.TinyCLR.NativeApis." + family + ".I2cController\\" + busId.ToString();
            return TinyI2c.I2cController.FromName(controllerName);
        }

        private static NotSupportedException CreateTodoNotSupportedException(string feature) =>
            new NotSupportedException("TODO-Not supported: " + feature);
    }
}
