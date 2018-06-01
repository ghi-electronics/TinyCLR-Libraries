using System;
using System.Runtime.CompilerServices;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Gpio.Provider;
using GHIElectronics.TinyCLR.Devices.I2c.Provider;

namespace GHIElectronics.TinyCLR.Devices.I2c {
    public class I2cSoftwareProvider : II2cProvider {
        private readonly GpioController controller;
        private readonly int sda;
        private readonly int scl;
        private readonly bool useSoftwarePullups;
        private II2cControllerProvider controllers;

        public I2cSoftwareProvider(int sda, int scl) : this(sda, scl, true) { }
        public I2cSoftwareProvider(int sda, int scl, bool useSoftwarePullups) : this(GpioController.GetDefault(), sda, scl, useSoftwarePullups) { }
        public I2cSoftwareProvider(GpioController controller, int sda, int scl) : this(controller, sda, scl, true) { }

        public I2cSoftwareProvider(GpioController controller, int sda, int scl, bool useSoftwarePullups) {
            this.controller = controller;
            this.sda = sda;
            this.scl = scl;
            this.useSoftwarePullups = useSoftwarePullups;
        }

        public II2cControllerProvider GetController(int idx) => this.controllers = (this.controllers ?? new I2cSoftwareControllerProvider(this.controller, this.sda, this.scl, this.useSoftwarePullups));
    }

    internal class I2cSoftwareControllerProvider : II2cControllerProvider {
        private readonly GpioController controller;
        private readonly int sda;
        private readonly int scl;
        private readonly bool useSoftwarePullups;

        public I2cSoftwareControllerProvider(GpioController controller, int sda, int scl, bool useSoftwarePullups) {
            this.controller = controller;
            this.sda = sda;
            this.scl = scl;
            this.useSoftwarePullups = useSoftwarePullups;
        }

        public II2cDeviceProvider GetDeviceProvider(ProviderI2cConnectionSettings settings) {
            if (this.controller.provider is DefaultGpioControllerProvider) {
                return new I2cNativeSoftwareDeviceProvider(settings, this.sda, this.scl, this.useSoftwarePullups);
            }
            else {
                if (this.controller.TryOpenPin(this.sda, GpioSharingMode.Exclusive, out var sda, out _) && this.controller.TryOpenPin(this.scl, GpioSharingMode.Exclusive, out var scl, out _))
                    return new I2cManagedSoftwareDeviceProvider(settings, sda, scl, this.useSoftwarePullups);

                sda?.Dispose();

                throw new InvalidOperationException();
            }
        }
    }

    internal class I2cManagedSoftwareDeviceProvider : II2cDeviceProvider {
        private readonly byte writeAddress;
        private readonly byte readAddress;
        private readonly GpioPin sda;
        private readonly GpioPin scl;
        private readonly bool useSoftwarePullups;
        private bool start;
        private bool disposed;

        public I2cManagedSoftwareDeviceProvider(ProviderI2cConnectionSettings settings, GpioPin sda, GpioPin scl, bool useSoftwarePullups) {
            if (settings.SlaveAddress > 0x7F) throw new ArgumentOutOfRangeException(nameof(settings), "Slave address must be no more than 0x7F.");
            if (settings.BusSpeed != ProviderI2cBusSpeed.StandardMode) throw new NotSupportedException("Must use standard mode.");
            if (settings.SharingMode != ProviderI2cSharingMode.Exclusive) throw new NotSupportedException("Must use exclusive mode.");

            this.writeAddress = (byte)(settings.SlaveAddress << 1);
            this.readAddress = (byte)((settings.SlaveAddress << 1) | 1);
            this.sda = sda;
            this.scl = scl;
            this.useSoftwarePullups = useSoftwarePullups;
            this.start = false;
            this.disposed = false;
        }

        public string DeviceId => $"I2C-SWM-{this.sda.PinNumber}-{this.scl.PinNumber}";

        public void Read(byte[] buffer) => this.Read(buffer, 0, buffer != null ? buffer.Length : 0);
        public ProviderI2cTransferResult ReadPartial(byte[] buffer) => this.ReadPartial(buffer, 0, buffer != null ? buffer.Length : 0);
        public void Write(byte[] buffer) => this.Write(buffer, 0, buffer != null ? buffer.Length : 0);
        public ProviderI2cTransferResult WritePartial(byte[] buffer) => this.WritePartial(buffer, 0, buffer != null ? buffer.Length : 0);
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer) => this.WriteRead(writeBuffer, 0, writeBuffer != null ? writeBuffer.Length : 0, readBuffer, 0, readBuffer != null ? readBuffer.Length : 0);
        public ProviderI2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) => this.WriteReadPartial(writeBuffer, 0, writeBuffer != null ? writeBuffer.Length : 0, readBuffer, 0, readBuffer != null ? readBuffer.Length : 0);

        public void Read(byte[] buffer, int offset, int length) => this.ReadPartial(buffer, offset, length);
        public void Write(byte[] buffer, int offset, int length) => this.WritePartial(buffer, offset, length);
        public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) => this.WriteReadPartial(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength);

        public ProviderI2cTransferResult ReadPartial(byte[] buffer, int offset, int length) {
            if (buffer == null) throw new ArgumentOutOfRangeException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (buffer.Length < offset + length) throw new ArgumentException(nameof(buffer));

            var res = this.Read(buffer, offset, length, true, true);

            this.ReleaseScl();
            this.ReleaseSda();

            return res;
        }

        public ProviderI2cTransferResult WritePartial(byte[] buffer, int offset, int length) {
            if (buffer == null) throw new ArgumentOutOfRangeException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (buffer.Length < offset + length) throw new ArgumentException(nameof(buffer));

            var res = this.Write(buffer, offset, length, true, true);

            this.ReleaseScl();
            this.ReleaseSda();

            return res;
        }

        public ProviderI2cTransferResult WriteReadPartial(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            if (writeBuffer == null) throw new ArgumentOutOfRangeException(nameof(writeBuffer));
            if (writeOffset < 0) throw new ArgumentOutOfRangeException(nameof(writeOffset));
            if (writeLength < 0) throw new ArgumentOutOfRangeException(nameof(writeLength));
            if (writeBuffer.Length < writeOffset + writeLength) throw new ArgumentException(nameof(writeBuffer));

            if (readBuffer == null) throw new ArgumentOutOfRangeException(nameof(readBuffer));
            if (readOffset < 0) throw new ArgumentOutOfRangeException(nameof(readOffset));
            if (readLength < 0) throw new ArgumentOutOfRangeException(nameof(readLength));
            if (readBuffer.Length < readOffset + readLength) throw new ArgumentException(nameof(readBuffer));

            var res = this.Write(writeBuffer, writeOffset, writeLength, true, false);

            if (res.Status == ProviderI2cTransferStatus.FullTransfer) {
                var soFar = res.BytesTransferred;

                res = this.Read(readBuffer, readOffset, readLength, true, true);

                this.ReleaseScl();
                this.ReleaseSda();

                return new ProviderI2cTransferResult { Status = res.Status == ProviderI2cTransferStatus.SlaveAddressNotAcknowledged ? ProviderI2cTransferStatus.PartialTransfer : res.Status, BytesTransferred = res.BytesTransferred + soFar };
            }
            else {
                this.ReleaseScl();
                this.ReleaseSda();

                return res;
            }
        }

        private void ClearScl() {
            this.scl.SetDriveMode(GpioPinDriveMode.Output);
            this.scl.Write(GpioPinValue.Low);
        }

        private void ClearSda() {
            this.sda.SetDriveMode(GpioPinDriveMode.Output);
            this.sda.Write(GpioPinValue.Low);
        }

        private void ReleaseScl() {
            this.scl.SetDriveMode(this.useSoftwarePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
            this.ReadScl();
        }

        private void ReleaseSda() {
            this.sda.SetDriveMode(this.useSoftwarePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
            this.ReadSda();
        }

        private bool ReadScl() {
            this.scl.SetDriveMode(this.useSoftwarePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
            return this.scl.Read() == GpioPinValue.High;
        }
        private bool ReadSda() {
            this.sda.SetDriveMode(this.useSoftwarePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
            return this.sda.Read() == GpioPinValue.High;
        }

        private void WaitForScl() {
            var i = 0;
            while (!this.ReadScl() && i++ < 100)
                Thread.Sleep(1);
        }

        private bool WriteBit(bool bit) {
            if (bit)
                this.ReleaseSda();
            else
                this.ClearSda();

            this.WaitForScl();

            if (bit && !this.ReadSda())
                return false;

            this.ClearScl();

            return true;
        }

        private bool ReadBit() {
            this.ReleaseSda();

            this.WaitForScl();

            var bit = this.ReadSda();

            this.ClearScl();

            return bit;
        }

        private bool SendStart() {
            if (this.start) {
                this.ReleaseSda();

                this.WaitForScl();
            }

            if (!this.ReadSda())
                return false;

            this.ClearSda();

            this.ClearScl();

            this.start = true;

            return true;
        }

        private bool SendStop() {
            this.ClearSda();

            this.WaitForScl();

            if (!this.ReadSda())
                return false;

            this.start = false;

            return true;
        }

        private bool Transmit(bool sendStart, bool sendStop, byte data) {
            if (sendStart)
                this.SendStart();

            for (var bit = 0; bit < 8; bit++) {
                this.WriteBit((data & 0x80) != 0);

                data <<= 1;
            }

            var nack = this.ReadBit();

            if (sendStop)
                this.SendStop();

            return !nack;
        }

        private bool Receive(bool sendAck, bool sendStop, out byte data) {
            data = 0;

            for (var bit = 0; bit < 8; bit++)
                data = (byte)((data << 1) | (this.ReadBit() ? 1 : 0));

            var res = this.WriteBit(!sendAck);

            return (sendStop ? this.SendStop() : true) && res;
        }

        private ProviderI2cTransferResult Write(byte[] buffer, int offset, int length, bool sendStart, bool sendStop) {
            if (!this.Transmit(sendStart, length == 0, this.writeAddress))
                return new ProviderI2cTransferResult { BytesTransferred = 0, Status = ProviderI2cTransferStatus.SlaveAddressNotAcknowledged };

            for (var i = 0; i < length; i++)
                if (!this.Transmit(false, i == length - 1 ? sendStop : false, buffer[i + offset]))
                    return new ProviderI2cTransferResult { BytesTransferred = (uint)i, Status = ProviderI2cTransferStatus.PartialTransfer };

            return new ProviderI2cTransferResult { BytesTransferred = (uint)length, Status = ProviderI2cTransferStatus.FullTransfer };
        }

        private ProviderI2cTransferResult Read(byte[] buffer, int offset, int length, bool sendStart, bool sendStop) {
            if (!this.Transmit(sendStart, length == 0, this.readAddress))
                return new ProviderI2cTransferResult { BytesTransferred = 0, Status = ProviderI2cTransferStatus.SlaveAddressNotAcknowledged };

            for (var i = 0; i < length; i++)
                if (!this.Receive(i < length - 1, i == length - 1 ? sendStop : false, out buffer[i + offset]))
                    return new ProviderI2cTransferResult { BytesTransferred = (uint)i, Status = ProviderI2cTransferStatus.PartialTransfer };

            return new ProviderI2cTransferResult { BytesTransferred = (uint)length, Status = ProviderI2cTransferStatus.FullTransfer };
        }

        public void Dispose() {
            if (!this.disposed) {
                this.sda.Dispose();
                this.scl.Dispose();
                this.disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        ~I2cManagedSoftwareDeviceProvider() => this.Dispose();
    }

    internal class I2cNativeSoftwareDeviceProvider : II2cDeviceProvider {
        private readonly byte address;
        private readonly int sda;
        private readonly int scl;
        private readonly bool useSoftwarePullups;
        private bool disposed;

        public I2cNativeSoftwareDeviceProvider(ProviderI2cConnectionSettings settings, int sda, int scl, bool useSoftwarePullups) {
            if (settings.SlaveAddress > 0x7F) throw new ArgumentOutOfRangeException(nameof(settings), "Slave address must be no more than 0x7F.");
            if (settings.BusSpeed != ProviderI2cBusSpeed.StandardMode) throw new NotSupportedException("Must use standard mode.");
            if (settings.SharingMode != ProviderI2cSharingMode.Exclusive) throw new NotSupportedException("Must use exclusive mode.");

            this.address = (byte)settings.SlaveAddress;
            this.sda = sda;
            this.scl = scl;
            this.useSoftwarePullups = useSoftwarePullups;
            this.disposed = false;
        }

        public string DeviceId => $"I2C-SWN-{this.sda}-{this.scl}";

        private ProviderI2cTransferResult GetResult(uint total, int expected) => new ProviderI2cTransferResult { BytesTransferred = total, Status = total == expected ? ProviderI2cTransferStatus.FullTransfer : (total == 0 ? ProviderI2cTransferStatus.SlaveAddressNotAcknowledged : ProviderI2cTransferStatus.PartialTransfer) };

        public void Read(byte[] buffer) => this.Read(buffer, 0, buffer != null ? buffer.Length : 0);
        public ProviderI2cTransferResult ReadPartial(byte[] buffer) => this.ReadPartial(buffer, 0, buffer != null ? buffer.Length : 0);
        public void Write(byte[] buffer) => this.Write(buffer, 0, buffer != null ? buffer.Length : 0);
        public ProviderI2cTransferResult WritePartial(byte[] buffer) => this.WritePartial(buffer, 0, buffer != null ? buffer.Length : 0);
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer) => this.WriteRead(writeBuffer, 0, writeBuffer != null ? writeBuffer.Length : 0, readBuffer, 0, readBuffer != null ? readBuffer.Length : 0);
        public ProviderI2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) => this.WriteReadPartial(writeBuffer, 0, writeBuffer != null ? writeBuffer.Length : 0, readBuffer, 0, readBuffer != null ? readBuffer.Length : 0);

        public void Read(byte[] buffer, int offset, int length) => this.ReadPartial(buffer, offset, length);
        public void Write(byte[] buffer, int offset, int length) => this.WritePartial(buffer, offset, length);
        public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) => this.WriteReadPartial(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength);

        public ProviderI2cTransferResult ReadPartial(byte[] buffer, int offset, int length) {
            if (buffer == null) throw new ArgumentOutOfRangeException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (buffer.Length < offset + length) throw new ArgumentException(nameof(buffer));

            I2cNativeSoftwareDeviceProvider.NativeWriteRead(this.scl, this.sda, this.address, this.useSoftwarePullups, null, 0, 0, buffer, offset, length, out _, out var total);

            return this.GetResult(total, length);
        }

        public ProviderI2cTransferResult WritePartial(byte[] buffer, int offset, int length) {
            if (buffer == null) throw new ArgumentOutOfRangeException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (buffer.Length < offset + length) throw new ArgumentException(nameof(buffer));

            I2cNativeSoftwareDeviceProvider.NativeWriteRead(this.scl, this.sda, this.address, this.useSoftwarePullups, buffer, offset, length, null, 0, 0, out var total, out _);

            return this.GetResult(total, length);
        }

        public ProviderI2cTransferResult WriteReadPartial(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            if (writeBuffer == null) throw new ArgumentOutOfRangeException(nameof(writeBuffer));
            if (writeOffset < 0) throw new ArgumentOutOfRangeException(nameof(writeOffset));
            if (writeLength < 0) throw new ArgumentOutOfRangeException(nameof(writeLength));
            if (writeBuffer.Length < writeOffset + writeLength) throw new ArgumentException(nameof(writeBuffer));

            if (readBuffer == null) throw new ArgumentOutOfRangeException(nameof(readBuffer));
            if (readOffset < 0) throw new ArgumentOutOfRangeException(nameof(readOffset));
            if (readLength < 0) throw new ArgumentOutOfRangeException(nameof(readLength));
            if (readBuffer.Length < readOffset + readLength) throw new ArgumentException(nameof(readBuffer));

            I2cNativeSoftwareDeviceProvider.NativeWriteRead(this.scl, this.sda, this.address, this.useSoftwarePullups, writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, out var written, out var read);

            return this.GetResult(written + read, writeLength + readLength);
        }

        public void Dispose() {
            if (!this.disposed) {
                this.disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        ~I2cNativeSoftwareDeviceProvider() => this.Dispose();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern static bool NativeWriteRead(int scl, int sda, byte address, bool useSoftwarePullups, byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, out uint written, out uint read);
    }
}
