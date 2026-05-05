using System;
using System.Collections;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi.Provider;
using GHIElectronics.TinyCLR.Native;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.Spi\Spi.cs.
// Bodies on Desktop are safe no-ops:
//   * Read buffers are zero-filled.
//   * Write/TransferFullDuplex/TransferSequential discard input.
//   * GetDefault() routes through FromName("Simulator") for clean factory chain.
//   * SoftwareProvider opens GPIO pins via Gpio.Desktop (also no-op) — safe.
namespace GHIElectronics.TinyCLR.Devices.Spi {
    public class SpiController : IDisposable {
        public ISpiControllerProvider Provider { get; }

        private SpiController(ISpiControllerProvider provider) => this.Provider = provider;

        public static SpiController GetDefault() => SpiController.FromName("Simulator");
        public static SpiController FromName(string name) => SpiController.FromProvider(new SpiControllerApiWrapper(NativeApi.Find(name, NativeApiType.SpiController)));
        public static SpiController FromProvider(ISpiControllerProvider provider) => new SpiController(provider);

        public void Dispose() => this.Provider.Dispose();

        public SpiDevice GetDevice(SpiConnectionSettings connectionSettings) => new SpiDevice(this, connectionSettings);

        public int ChipSelectLineCount => this.Provider.ChipSelectLineCount;
        public int MinClockFrequency => this.Provider.MinClockFrequency;
        public int MaxClockFrequency => this.Provider.MaxClockFrequency;
        public int[] SupportedDataBitLengths => this.Provider.SupportedDataBitLengths;

        internal void SetActive(SpiDevice device) => this.Provider.SetActiveSettings(device.ConnectionSettings);
    }

    public class SpiDevice : IDisposable {
        public SpiConnectionSettings ConnectionSettings { get; }
        public SpiController Controller { get; }

        internal SpiDevice(SpiController controller, SpiConnectionSettings connectionSettings) {
            this.ConnectionSettings = connectionSettings;
            this.Controller = controller;
        }

        public void Dispose() { }

        public void Read(byte[] buffer) => this.Read(buffer, 0, buffer.Length);
        public void Write(byte[] buffer) => this.Write(buffer, 0, buffer.Length);
        public void Write(byte[] buffer, int xOffset, int yOffset, int width, int height, int originalWidth) => this.Write(buffer, xOffset, yOffset, width, height, originalWidth, 1, 1);
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

        public void Write(byte[] buffer, int x, int y, int width, int height, int originalWidth, int columnMultiplier, int rowMultiplier) {
            this.Controller.SetActive(this);
            this.Controller.Provider.Write(buffer, x, y, width, height, originalWidth, columnMultiplier, rowMultiplier);
        }
    }

    public class SpiConnectionSettings {
        public SpiChipSelectType ChipSelectType { get; set; } = SpiChipSelectType.None;
        public GpioPin ChipSelectLine { get; set; } = null;
        public int ClockFrequency { get; set; } = 1_000_000;
        public int DataBitLength { get; } = 8;
        public SpiDataFrame DataFrameFormat { get; set; } = SpiDataFrame.MsbFirst;
        public SpiMode Mode { get; set; } = SpiMode.Mode0;
        public TimeSpan ChipSelectSetupTime { get; set; } = TimeSpan.FromTicks(0);
        public TimeSpan ChipSelectHoldTime { get; set; } = TimeSpan.FromTicks(0);
        public bool ChipSelectActiveState { get; set; } = false;
    }

    public enum SpiDataFrame {
        MsbFirst = 0,
        LsbFirst = 1
    }

    public enum SpiMode {
        Mode0 = 0,
        Mode1 = 1,
        Mode2 = 2,
        Mode3 = 3,
    }

    public enum SpiChipSelectType {
        None = 0,
        Gpio = 1
    }

    namespace Provider {
        public interface ISpiControllerProvider : IDisposable {
            int ChipSelectLineCount { get; }
            int MinClockFrequency { get; }
            int MaxClockFrequency { get; }
            int[] SupportedDataBitLengths { get; }

            void SetActiveSettings(SpiConnectionSettings connectionSettings);
            void Write(byte[] writeBuffer, int xOffset, int yOffset, int width, int height, int originalWidth, int columnMultiplier, int rowMultiplier);
            void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter);
        }

        // No-op provider. Read buffers are zeroed; write inputs ignored.
        public sealed class SpiControllerApiWrapper : ISpiControllerProvider {
            public NativeApi Api { get; }

            public SpiControllerApiWrapper(NativeApi api) => this.Api = api;

            public void Dispose() { }

            public int ChipSelectLineCount => 1;
            public int MinClockFrequency => 0;
            public int MaxClockFrequency => 1_000_000_000;
            public int[] SupportedDataBitLengths => new[] { 8 };

            public void SetActiveSettings(SpiConnectionSettings connectionSettings) { }

            public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter) {
                if (readBuffer != null && readLength > 0)
                    Array.Clear(readBuffer, readOffset, readLength);
            }

            public void Write(byte[] writeBuffer, int xOffset, int yOffset, int width, int height, int originalWidth, int columnMultiplier, int rowMultiplier) { }
        }

        // Software provider on Desktop is also a no-op. The impl uses GPIO
        // bit-banging; here we just record the configuration and return
        // zeros on read. Constructors mirror the impl's so user code that
        // creates one compiles and runs.
        public sealed class SpiControllerSoftwareProvider : ISpiControllerProvider {
            private readonly IDictionary chipSelects;
            private readonly GpioController gpioController;

            public int ChipSelectLineCount => 0;
            public int MinClockFrequency => 0;
            public int MaxClockFrequency => 1_000_000_000;
            public int[] SupportedDataBitLengths => new[] { 8 };

            public SpiControllerSoftwareProvider(int mosiPinNumber, int misoPinNumber, int sckPinNumber)
                : this(GpioController.GetDefault(), mosiPinNumber, misoPinNumber, sckPinNumber) { }

            public SpiControllerSoftwareProvider(GpioController gpioController, int mosiPinNumber, int misoPinNumber, int sckPinNumber) {
                this.chipSelects = new Hashtable();
                this.gpioController = gpioController;
                // Open pins via the no-op Gpio.Desktop provider. Safe.
                gpioController.OpenPins(mosiPinNumber, misoPinNumber, sckPinNumber);
            }

            public void Dispose() { }

            public void SetActiveSettings(SpiConnectionSettings connectionSettings) { }

            public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter) {
                if (readBuffer != null && readLength > 0)
                    Array.Clear(readBuffer, readOffset, readLength);
            }

            public void Write(byte[] writeBuffer, int xOffset, int yOffset, int width, int height, int originalWidth, int columnMultiplier, int rowMultiplier) { }
        }
    }
}
