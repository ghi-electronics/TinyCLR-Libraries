using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.I2c {
    public sealed class I2cController : IDisposable {
        public II2cControllerProvider Provider { get; }

        private I2cController(II2cControllerProvider provider) => this.Provider = provider;

        public static I2cController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.I2cController) is I2cController c ? c : I2cController.FromName(NativeApi.GetDefaultName(NativeApiType.I2cController));
        public static I2cController FromName(string name) => I2cController.FromProvider(new I2cControllerApiWrapper(NativeApi.Find(name, NativeApiType.I2cController)));
        public static I2cController FromProvider(II2cControllerProvider provider) => new I2cController(provider);

        public void Dispose() => this.Provider.Dispose();

        public I2cDevice GetDevice(I2cConnectionSettings connectionSettings) => new I2cDevice(this, connectionSettings);

        internal void SetActive(I2cDevice device) => this.Provider.SetActiveSettings(device.ConnectionSettings);
    }

    public sealed class I2cDevice : IDisposable {
        public I2cConnectionSettings ConnectionSettings { get; }
        public I2cController Controller { get; }

        internal I2cDevice(I2cController controller, I2cConnectionSettings connectionSettings) {
            this.ConnectionSettings = connectionSettings;
            this.Controller = controller;
        }

        public void Dispose() {

        }

        public void Read(byte[] buffer) => this.WriteRead(null, 0, 0, buffer, 0, buffer.Length);
        public void Write(byte[] buffer) => this.WriteRead(buffer, 0, buffer.Length, null, 0, 0);
        public void WriteRead(byte[] writeBuffer, byte[] readBuffer) => this.WriteRead(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);

        public void Read(byte[] buffer, int offset, int length) => this.WriteRead(null, 0, 0, buffer, offset, length);
        public void Write(byte[] buffer, int offset, int length) => this.WriteRead(buffer, offset, length, null, 0, 0);

        public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            this.Controller.SetActive(this);

            if (this.Controller.Provider.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, true, true, out _, out _) != I2cTransferStatus.FullTransfer)
                throw new InvalidOperationException();
        }

        public I2cTransferResult ReadPartial(byte[] buffer) => this.WriteReadPartial(null, 0, 0, buffer, 0, buffer.Length);
        public I2cTransferResult WritePartial(byte[] buffer) => this.WriteReadPartial(buffer, 0, buffer.Length, null, 0, 0);
        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) => this.WriteReadPartial(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);

        public I2cTransferResult ReadPartial(byte[] buffer, int offset, int length) => this.WriteReadPartial(null, 0, 0, buffer, offset, length);
        public I2cTransferResult WritePartial(byte[] buffer, int offset, int length) => this.WriteReadPartial(buffer, offset, length, null, 0, 0);

        public I2cTransferResult WriteReadPartial(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            this.Controller.SetActive(this);

            var res = this.Controller.Provider.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, true, true, out var written, out var read);

            return new I2cTransferResult(res, written, read);
        }
    }

    public sealed class I2cConnectionSettings {
        public int SlaveAddress { get; set; }
        public I2cAddressFormat AddressFormat { get; set; }
        public uint BusSpeed { get; set; }

        public I2cConnectionSettings(int slaveAddress) : this(slaveAddress, I2cAddressFormat.SevenBit) {

        }

        public I2cConnectionSettings(int slaveAddress, uint busSpeed) : this(slaveAddress, I2cAddressFormat.SevenBit, busSpeed) {

        }

        public I2cConnectionSettings(int slaveAddress, I2cAddressFormat addressFormat, uint busSpeed = 100000) {
            this.SlaveAddress = slaveAddress;
            this.AddressFormat = addressFormat;
            this.BusSpeed = busSpeed;
        }
    }

    public enum I2cAddressFormat {
        SevenBit = 0,
        TenBit = 1,
    }

    public enum I2cTransferStatus {
        FullTransfer = 0,
        PartialTransfer = 1,
        SlaveAddressNotAcknowledged = 2,
        ClockStretchTimeout = 3,
    }

    public struct I2cTransferResult {
        public I2cTransferStatus Status { get; }
        public int BytesWritten { get; }
        public int BytesRead { get; }

        public int BytesTransferred => this.BytesWritten + this.BytesRead;

        internal I2cTransferResult(I2cTransferStatus status, int bytesWritten, int bytesRead) {
            this.Status = status;
            this.BytesWritten = bytesWritten;
            this.BytesRead = bytesRead;
        }
    }

    namespace Provider {
        public interface II2cControllerProvider : IDisposable {
            void SetActiveSettings(I2cConnectionSettings connectionSettings);
            I2cTransferStatus WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool sendStartCondition, bool sendStopCondition, out int written, out int read);
        }

        public sealed class I2cControllerApiWrapper : II2cControllerProvider {
            private readonly IntPtr impl;

            public NativeApi Api { get; }

            public I2cControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetActiveSettings(I2cConnectionSettings connectionSettings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern I2cTransferStatus WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool sendStartCondition, bool sendStopCondition, out int written, out int read);
        }

        public sealed class I2cControllerSoftwareProvider : II2cControllerProvider {
            private readonly bool usePullups;
            private readonly GpioPin sda;
            private readonly GpioPin scl;
            private byte writeAddress;
            private byte readAddress;
            private bool start;

            public I2cControllerSoftwareProvider(int sdaPinNumber, int sclPinNumber) : this(sdaPinNumber, sclPinNumber, true) { }
            public I2cControllerSoftwareProvider(int sdaPinNumber, int sclPinNumber, bool usePullups) : this(GpioController.GetDefault(), sdaPinNumber, sclPinNumber, usePullups) { }
            public I2cControllerSoftwareProvider(GpioController controller, int sdaPinNumber, int sclPinNumber) : this(controller, sdaPinNumber, sclPinNumber, true) { }

            public I2cControllerSoftwareProvider(GpioController controller, int sdaPinNumber, int sclPinNumber, bool usePullups) {
                this.usePullups = usePullups;

                var pins = controller.OpenPins(sdaPinNumber, sclPinNumber);

                this.sda = pins[0];
                this.scl = pins[1];
            }

            public void Dispose() {
                this.sda.Dispose();
                this.scl.Dispose();
            }

            public void SetActiveSettings(I2cConnectionSettings connectionSettings) {
                if (connectionSettings.AddressFormat != I2cAddressFormat.SevenBit) throw new NotSupportedException();

                this.writeAddress = (byte)(connectionSettings.SlaveAddress << 1);
                this.readAddress = (byte)((connectionSettings.SlaveAddress << 1) | 1);
                this.start = false;

                this.ReleaseScl();
                this.ReleaseSda();
            }

            public I2cTransferStatus WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool sendStartCondition, bool sendStopCondition, out int written, out int read) {
                written = 0;
                read = 0;

                try {
                    var res = this.Write(writeBuffer, writeOffset, writeLength, true, readLength == 0);

                    written = res.BytesWritten;
                    read = res.BytesRead;

                    if (res.Status == I2cTransferStatus.FullTransfer && readLength != 0) {
                        res = this.Read(readBuffer, readOffset, readLength, true, true);

                        written += res.BytesWritten;
                        read += res.BytesRead;
                    }

                    this.ReleaseScl();
                    this.ReleaseSda();

                    return res.Status;
                }
                catch (I2cClockStretchTimeoutException) {
                    return I2cTransferStatus.ClockStretchTimeout;
                }
            }

            private I2cTransferResult Write(byte[] buffer, int offset, int length, bool sendStart, bool sendStop) {
                if (!this.Send(sendStart, length == 0, this.writeAddress))
                    return new I2cTransferResult(I2cTransferStatus.SlaveAddressNotAcknowledged, 0, 0);

                for (var i = 0; i < length; i++)
                    if (!this.Send(false, i == length - 1 && sendStop, buffer[i + offset]))
                        return new I2cTransferResult(I2cTransferStatus.PartialTransfer, i, 0);

                return new I2cTransferResult(I2cTransferStatus.FullTransfer, length, 0);
            }

            private I2cTransferResult Read(byte[] buffer, int offset, int length, bool sendStart, bool sendStop) {
                if (!this.Send(sendStart, length == 0, this.readAddress))
                    return new I2cTransferResult(I2cTransferStatus.SlaveAddressNotAcknowledged, 0, 0);

                for (var i = 0; i < length; i++)
                    if (!this.Receive(i < length - 1, i == length - 1 && sendStop, out buffer[i + offset]))
                        return new I2cTransferResult(I2cTransferStatus.PartialTransfer, 0, i);

                return new I2cTransferResult(I2cTransferStatus.FullTransfer, 0, length);
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
                this.scl.SetDriveMode(this.usePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
                this.ReadScl();
            }

            private void ReleaseSda() {
                this.sda.SetDriveMode(this.usePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
                this.ReadSda();
            }

            private bool ReadScl() {
                this.scl.SetDriveMode(this.usePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
                return this.scl.Read() == GpioPinValue.High;
            }

            private bool ReadSda() {
                this.sda.SetDriveMode(this.usePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
                return this.sda.Read() == GpioPinValue.High;
            }

            private void WaitForScl() {
                const long TimeoutInTicks = 1000 * 1000 * 10; // Timeout: 1 second
                const long DelayInTicks = (1000000 / 2000) * 10; // Max frequency: 2KHz

                var currentTicks = DateTime.Now.Ticks;
                var timeout = true;

                while (DateTime.Now.Ticks - currentTicks < DelayInTicks / 2) ;

                while (timeout && DateTime.Now.Ticks - currentTicks < TimeoutInTicks) {
                    if (this.ReadScl()) timeout = false;
                }

                if (timeout)
                    throw new I2cClockStretchTimeoutException();

                var periodClockInTicks = DateTime.Now.Ticks - currentTicks;

                currentTicks = DateTime.Now.Ticks;

                while (DateTime.Now.Ticks - currentTicks < periodClockInTicks) ;
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

            private bool Send(bool sendStart, bool sendStop, byte data) {
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

                return (!sendStop || this.SendStop()) && res;
            }

            private class I2cClockStretchTimeoutException : Exception {

            }
        }
    }
}
