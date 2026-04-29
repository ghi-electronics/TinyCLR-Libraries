using TinyI2c = GHIElectronics.TinyCLR.Devices.I2c;
using System;
using GHIElectronics.TinyCLR.Native;

namespace System.Device.I2c {
    public enum I2cBusSpeed {
        StandardMode = 0,
        FastMode = 1,
        FastModePlus = 2
    }

    public sealed class I2cConnectionSettings {
        public int BusId { get; }
        public int DeviceAddress { get; set; }
        public I2cBusSpeed BusSpeed { get; set; } = I2cBusSpeed.StandardMode;

        public I2cConnectionSettings(int busId, int deviceAddress) {
            this.BusId = busId;
            this.DeviceAddress = deviceAddress;
        }
    }

    public abstract class I2cDevice : IDisposable {
        public abstract I2cConnectionSettings ConnectionSettings { get; }

        public static I2cDevice Create(I2cConnectionSettings settings) => new TinyClrI2cDevice(settings);

        public abstract void Read(byte[] buffer);
        public abstract void Write(byte[] buffer);
        public abstract void WriteRead(byte[] writeBuffer, byte[] readBuffer);
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

            var deviceName = DeviceInformation.DeviceName;
            if (string.IsNullOrEmpty(deviceName))
                throw new InvalidOperationException("DeviceInformation.DeviceName is not available.");

            var apiFamily = IsSc13Device(deviceName) ? "STM32L4" : "STM32H7";
            var controllerName = "GHIElectronics.TinyCLR.NativeApis." + apiFamily + ".I2cController\\" + busId.ToString();
            return TinyI2c.I2cController.FromName(controllerName);
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
