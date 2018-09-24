using System;
using System.Collections;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Spi {
    public sealed class SpiController : IDisposable {
        public ISpiControllerProvider Provider { get; }

        private SpiController(ISpiControllerProvider provider) => this.Provider = provider;

        public static SpiController GetDefault() => Api.GetDefaultFromCreator(ApiType.SpiController) is SpiController c ? c : SpiController.FromName(Api.GetDefaultName(ApiType.SpiController));
        public static SpiController FromName(string name) => SpiController.FromProvider(new SpiControllerApiWrapper(Api.Find(name, ApiType.SpiController)));
        public static SpiController FromProvider(ISpiControllerProvider provider) => new SpiController(provider);

        public void Dispose() => this.Provider.Dispose();

        public SpiDevice GetDevice(SpiConnectionSettings connectionSettings) => new SpiDevice(this, connectionSettings);

        public int ChipSelectLineCount => this.Provider.ChipSelectLineCount;
        public int MinClockFrequency => this.Provider.MinClockFrequency;
        public int MaxClockFrequency => this.Provider.MaxClockFrequency;
        public int[] SupportedDataBitLengths => this.Provider.SupportedDataBitLengths;

        internal void SetActive(SpiDevice device) => this.Provider.SetActiveSettings(device.ConnectionSettings);
    }

    public sealed class SpiDevice : IDisposable {
        public SpiConnectionSettings ConnectionSettings { get; }
        public SpiController Controller { get; }

        internal SpiDevice(SpiController controller, SpiConnectionSettings connectionSettings) {
            this.ConnectionSettings = connectionSettings;
            this.Controller = controller;
        }

        public void Dispose() {

        }

        public void Read(byte[] buffer) => this.Read(buffer, 0, buffer.Length);
        public void Write(byte[] buffer) => this.Write(buffer, 0, buffer.Length);
        public void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer) => this.TransferFullDuplex(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);
        public void TransferSequential(byte[] writeBuffer, byte[] readBuffer) => this.TransferSequential(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);

        public void Read(byte[] buffer, int offset, int length) => this.WriteRead(null, 0, 0, buffer, offset, length);
        public void Write(byte[] buffer, int offset, int length) => this.WriteRead(buffer, offset, length, null, 0, 0);
        public void TransferFullDuplex(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) => this.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength);

        public void TransferSequential(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            this.WriteRead(writeBuffer, writeOffset, writeLength, null, 0, 0, false);
            this.WriteRead(null, 0, 0, readBuffer, readOffset, readLength);
        }

        private void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter = true) {
            this.Controller.SetActive(this);

            this.Controller.Provider.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, deselectAfter);
        }
    }

    public sealed class SpiConnectionSettings {
        public SpiChipSelectType ChipSelectType { get; set; } = SpiChipSelectType.None;
        public int ChipSelectLine { get; set; }
        public int ClockFrequency { get; set; } = 1_000_000;
        public int DataBitLength { get; set; } = 8;
        public SpiMode Mode { get; set; } = SpiMode.Mode0;
        public TimeSpan ChipSelectSetupTime { get; set; } = TimeSpan.FromTicks(0);
        public TimeSpan ChipSelectHoldTime { get; set; } = TimeSpan.FromTicks(0);
        public bool ChipSelectActiveState { get; set; } = false;
    }

    public enum SpiMode {
        Mode0 = 0,
        Mode1 = 1,
        Mode2 = 2,
        Mode3 = 3,
    }

    public enum SpiChipSelectType {
        None = 0,
        Controller = 1,
        Gpio = 2
    }

    namespace Provider {
        public interface ISpiControllerProvider : IDisposable {
            int ChipSelectLineCount { get; }
            int MinClockFrequency { get; }
            int MaxClockFrequency { get; }
            int[] SupportedDataBitLengths { get; }

            void SetActiveSettings(SpiConnectionSettings connectionSettings);
            void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter);
        }

        public sealed class SpiControllerApiWrapper : ISpiControllerProvider {
            private readonly IntPtr impl;

            public Api Api { get; }

            public SpiControllerApiWrapper(Api api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            public extern int ChipSelectLineCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int MinClockFrequency { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int MaxClockFrequency { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int[] SupportedDataBitLengths { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetActiveSettings(SpiConnectionSettings connectionSettings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter);
        }

        public sealed class SpiControllerSoftwareProvider : ISpiControllerProvider {
            private readonly IDictionary chipSelects;
            private readonly GpioController gpioController;
            private readonly GpioPin mosi;
            private readonly GpioPin miso;
            private readonly GpioPin sck;
            private GpioPin cs;
            private bool captureOnRisingEdge;
            private GpioPinValue clockIdleState;
            private GpioPinValue clockActiveState;

            public int ChipSelectLineCount => 0;
            public int MinClockFrequency => 0;
            public int MaxClockFrequency => 1_000_000_000;
            public int[] SupportedDataBitLengths => new[] { 8 };

            public SpiControllerSoftwareProvider(int mosiPinNumber, int misoPinNumber, int sckPinNumber) : this(GpioController.GetDefault(), mosiPinNumber, misoPinNumber, sckPinNumber) {

            }

            public SpiControllerSoftwareProvider(GpioController gpioController, int mosiPinNumber, int misoPinNumber, int sckPinNumber) {
                this.chipSelects = new Hashtable();
                this.gpioController = gpioController;

                var pins = gpioController.OpenPins(mosiPinNumber, misoPinNumber, sckPinNumber);

                this.mosi = pins[0];
                this.miso = pins[1];
                this.sck = pins[2];

                this.miso.SetDriveMode(GpioPinDriveMode.Input);

                this.mosi.Write(GpioPinValue.Low);
                this.mosi.SetDriveMode(GpioPinDriveMode.Output);

                this.sck.Write(this.clockIdleState);
                this.sck.SetDriveMode(GpioPinDriveMode.Output);
            }

            public void Dispose() {
                this.mosi.Dispose();
                this.miso.Dispose();
                this.sck.Dispose();
                this.cs?.Dispose();
            }

            public void SetActiveSettings(SpiConnectionSettings connectionSettings) {
                if (connectionSettings.DataBitLength != 8) throw new NotSupportedException();
                if (connectionSettings.ChipSelectType == SpiChipSelectType.Controller) throw new NotSupportedException();

                this.captureOnRisingEdge = ((((int)connectionSettings.Mode) & 0x01) == 0);
                this.clockActiveState = (((int)connectionSettings.Mode) & 0x02) == 0 ? GpioPinValue.High : GpioPinValue.Low;
                this.clockIdleState = this.clockActiveState == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High;

                if (connectionSettings.ChipSelectType == SpiChipSelectType.Gpio) {
                    if (!this.chipSelects.Contains(connectionSettings.ChipSelectLine)) {
                        var cs = this.gpioController.OpenPin(connectionSettings.ChipSelectLine);

                        this.chipSelects[connectionSettings.ChipSelectLine] = cs;

                        cs.Write(GpioPinValue.High);
                        cs.SetDriveMode(GpioPinDriveMode.Output);
                    }

                    this.cs = (GpioPin)this.chipSelects[connectionSettings.ChipSelectLine];
                    this.cs.Write(GpioPinValue.High);
                }
            }

            public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter) {
                if (readBuffer != null)
                    Array.Clear(readBuffer, 0, readLength);

                this.sck.Write(this.clockIdleState);
                this.cs?.Write(GpioPinValue.Low);

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
                    this.cs?.Write(GpioPinValue.High);
            }
        }
    }
}
