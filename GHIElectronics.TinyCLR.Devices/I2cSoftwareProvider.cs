using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c.Provider;
using GHIElectronics.TinyCLR.Devices.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.I2c {
    public class I2cSoftwareProvider : II2cProvider {
        private readonly GpioController controller;
        private readonly int sda;
        private readonly int scl;
        private readonly bool useSoftwarePullups;
        private II2cControllerProvider[] controllers;

        public I2cSoftwareProvider(int sda, int scl) : this(sda, scl, true) { }
        public I2cSoftwareProvider(int sda, int scl, bool useSoftwarePullups) : this(GpioController.GetDefault(), sda, scl, useSoftwarePullups) { }
        public I2cSoftwareProvider(GpioController controller, int sda, int scl) : this(controller, sda, scl, true) { }

        public I2cSoftwareProvider(GpioController controller, int sda, int scl, bool useSoftwarePullups) {
            this.controller = controller;
            this.sda = sda;
            this.scl = scl;
            this.useSoftwarePullups = useSoftwarePullups;
        }

        public II2cControllerProvider[] GetControllers() => this.controllers = (this.controllers ?? new[] { new I2cSoftwareControllerProvider(this.controller, this.sda, this.scl, this.useSoftwarePullups) });

        private class I2cSoftwareControllerProvider : II2cControllerProvider {
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
                if (object.ReferenceEquals(this.controller, GpioController.GetDefault())) {
                    return new I2cNativeSoftwareDeviceProvider(settings, this.sda, this.scl, this.useSoftwarePullups);
                }
                else {
                    if (this.controller.TryOpenPin(this.sda, GpioSharingMode.Exclusive, out var sda, out _) && this.controller.TryOpenPin(this.scl, GpioSharingMode.Exclusive, out var scl, out _))
                        return new I2cManagedSoftwareDeviceProvider(settings, sda, scl, this.useSoftwarePullups);

                    sda?.Dispose();

                    throw new InvalidOperationException();
                }
            }

            private class I2cManagedSoftwareDeviceProvider : II2cDeviceProvider {
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

                public void Read(byte[] buffer) => this.ReadPartial(buffer);
                public void Write(byte[] buffer) => this.WritePartial(buffer);
                public void WriteRead(byte[] writeBuffer, byte[] readBuffer) => this.WriteReadPartial(writeBuffer, readBuffer);

                public ProviderI2cTransferResult ReadPartial(byte[] buffer) {
                    if (buffer == null) throw new ArgumentNullException(nameof(buffer));

                    var res = this.Read(buffer, true, true);

                    this.ReleaseScl();
                    this.ReleaseSda();

                    return res;
                }

                public ProviderI2cTransferResult WritePartial(byte[] buffer) {
                    if (buffer == null) throw new ArgumentNullException(nameof(buffer));

                    var res = this.Write(buffer, true, true);

                    this.ReleaseScl();
                    this.ReleaseSda();

                    return res;
                }

                public ProviderI2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) {
                    if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));
                    if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));

                    var res = this.Write(writeBuffer, true, false);

                    if (res.Status == ProviderI2cTransferStatus.FullTransfer) {
                        var soFar = res.BytesTransferred;

                        res = this.Read(readBuffer, true, true);

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
                    this.scl.SetDriveMode(GpioPinDriveMode.Output);
                    this.sda.Write(GpioPinValue.Low);
                }

                private void ReleaseScl() {
                    this.scl.SetDriveMode(this.useSoftwarePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
                    this.ReadScl();
                }

                private void ReleaseSda() {
                    this.scl.SetDriveMode(this.useSoftwarePullups ? GpioPinDriveMode.InputPullUp : GpioPinDriveMode.Input);
                    this.ReadSda();
                }

                private bool ReadScl() => this.scl.Read() == GpioPinValue.High;
                private bool ReadSda() => this.sda.Read() == GpioPinValue.High;

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

                private ProviderI2cTransferResult Write(byte[] buffer, bool sendStart, bool sendStop) {
                    if (!this.Transmit(sendStart, buffer.Length == 0, this.writeAddress))
                        return new ProviderI2cTransferResult { BytesTransferred = 0, Status = ProviderI2cTransferStatus.SlaveAddressNotAcknowledged };

                    for (var i = 0U; i < buffer.Length; i++)
                        if (!this.Transmit(false, i == buffer.Length - 1 ? sendStop : false, buffer[i]))
                            return new ProviderI2cTransferResult { BytesTransferred = i, Status = ProviderI2cTransferStatus.PartialTransfer };

                    return new ProviderI2cTransferResult { BytesTransferred = (uint)buffer.Length, Status = ProviderI2cTransferStatus.FullTransfer };
                }

                private ProviderI2cTransferResult Read(byte[] buffer, bool sendStart, bool sendStop) {
                    if (!this.Transmit(sendStart, buffer.Length == 0, this.readAddress))
                        return new ProviderI2cTransferResult { BytesTransferred = 0, Status = ProviderI2cTransferStatus.SlaveAddressNotAcknowledged };

                    for (var i = 0U; i < buffer.Length; i++)
                        if (!this.Receive(i < buffer.Length - 1, i == buffer.Length - 1 ? sendStop : false, out buffer[i]))
                            return new ProviderI2cTransferResult { BytesTransferred = i, Status = ProviderI2cTransferStatus.PartialTransfer };

                    return new ProviderI2cTransferResult { BytesTransferred = (uint)buffer.Length, Status = ProviderI2cTransferStatus.FullTransfer };
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

            private class I2cNativeSoftwareDeviceProvider : II2cDeviceProvider {
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

                    var clock = Port.ReservePin((Cpu.Pin)this.sda, true);
                    var data = Port.ReservePin((Cpu.Pin)this.scl, true);

                    if (clock && data)
                        return;

                    if (data)
                        Port.ReservePin((Cpu.Pin)this.scl, false);

                    if (clock)
                        Port.ReservePin((Cpu.Pin)this.sda, false);

                    throw new InvalidOperationException("The given pins are in use.");
                }

                public string DeviceId => $"I2C-SWN-{this.sda}-{this.scl}";

                public void Read(byte[] buffer) => this.ReadPartial(buffer);
                public void Write(byte[] buffer) => this.WritePartial(buffer);
                public void WriteRead(byte[] writeBuffer, byte[] readBuffer) => this.WriteReadPartial(writeBuffer, readBuffer);

                private ProviderI2cTransferResult GetResult(uint total, int expected) => new ProviderI2cTransferResult { BytesTransferred = total, Status = total == expected ? ProviderI2cTransferStatus.FullTransfer : (total == 0 ? ProviderI2cTransferStatus.SlaveAddressNotAcknowledged : ProviderI2cTransferStatus.PartialTransfer) };

                public ProviderI2cTransferResult ReadPartial(byte[] buffer) {
                    if (buffer == null) throw new ArgumentNullException(nameof(buffer));

                    I2cNativeSoftwareDeviceProvider.NativeWriteRead(this.scl, this.sda, this.address, this.useSoftwarePullups, null, buffer, out _, out var total);

                    return this.GetResult(total, buffer.Length);
                }

                public ProviderI2cTransferResult WritePartial(byte[] buffer) {
                    if (buffer == null) throw new ArgumentNullException(nameof(buffer));

                    I2cNativeSoftwareDeviceProvider.NativeWriteRead(this.scl, this.sda, this.address, this.useSoftwarePullups, buffer, null, out var total, out _);

                    return this.GetResult(total, buffer.Length);
                }

                public ProviderI2cTransferResult WriteReadPartial(byte[] writeBuffer, byte[] readBuffer) {
                    if (readBuffer == null) throw new ArgumentNullException(nameof(readBuffer));
                    if (writeBuffer == null) throw new ArgumentNullException(nameof(writeBuffer));

                    I2cNativeSoftwareDeviceProvider.NativeWriteRead(this.scl, this.sda, this.address, this.useSoftwarePullups, writeBuffer, readBuffer, out var written, out var read);

                    return this.GetResult(written + read, writeBuffer.Length + readBuffer.Length);
                }

                public void Dispose() {
                    if (!this.disposed) {
                        Port.ReservePin((Cpu.Pin)this.sda, false);
                        Port.ReservePin((Cpu.Pin)this.scl, false);
                        this.disposed = true;
                    }

                    GC.SuppressFinalize(this);
                }

                ~I2cNativeSoftwareDeviceProvider() => this.Dispose();

                [MethodImpl(MethodImplOptions.InternalCall)]
                private extern static bool NativeWriteRead(int scl, int sda, byte address, bool useSoftwarePullups, byte[] writeBuffer, byte[] readBuffer, out uint written, out uint read);
            }
        }
    }
}
