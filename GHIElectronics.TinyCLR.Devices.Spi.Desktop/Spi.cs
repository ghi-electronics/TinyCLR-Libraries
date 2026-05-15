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
    /// <summary>
    /// Represents an SPI bus controller. Obtain one via <see cref="GetDefault"/>
    /// or <see cref="FromName(string)"/>, then create a <see cref="SpiDevice"/> for
    /// each chip on the bus via <see cref="GetDevice(SpiConnectionSettings)"/>.
    /// </summary>
    public class SpiController : IDisposable {
        /// <summary>The low-level provider backing this controller.</summary>
        public ISpiControllerProvider Provider { get; }

        private SpiController(ISpiControllerProvider provider) => this.Provider = provider;

        /// <summary>Returns the default SPI controller for this device.</summary>
        public static SpiController GetDefault() => SpiController.FromName("Simulator");
        /// <summary>Returns an SPI controller identified by its native API name.</summary>
        public static SpiController FromName(string name) => SpiController.FromProvider(new SpiControllerApiWrapper(NativeApi.Find(name, NativeApiType.SpiController)));
        /// <summary>Creates a controller from a custom <see cref="ISpiControllerProvider"/>.</summary>
        public static SpiController FromProvider(ISpiControllerProvider provider) => new SpiController(provider);

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>Creates a <see cref="SpiDevice"/> bound to this controller using the given settings.</summary>
        public SpiDevice GetDevice(SpiConnectionSettings connectionSettings) => new SpiDevice(this, connectionSettings);

        /// <summary>Number of hardware chip-select lines exposed by this controller.</summary>
        public int ChipSelectLineCount => this.Provider.ChipSelectLineCount;
        /// <summary>Minimum SCK frequency in Hz.</summary>
        public int MinClockFrequency => this.Provider.MinClockFrequency;
        /// <summary>Maximum SCK frequency in Hz.</summary>
        public int MaxClockFrequency => this.Provider.MaxClockFrequency;
        /// <summary>Supported frame widths in bits.</summary>
        public int[] SupportedDataBitLengths => this.Provider.SupportedDataBitLengths;

        internal void SetActive(SpiDevice device) => this.Provider.SetActiveSettings(device.ConnectionSettings);
    }

    /// <summary>Represents a single chip on an SPI bus.</summary>
    public class SpiDevice : IDisposable {
        /// <summary>The per-device bus settings.</summary>
        public SpiConnectionSettings ConnectionSettings { get; }
        /// <summary>The <see cref="SpiController"/> this device transacts over.</summary>
        public SpiController Controller { get; }

        internal SpiDevice(SpiController controller, SpiConnectionSettings connectionSettings) {
            this.ConnectionSettings = connectionSettings;
            this.Controller = controller;
        }

        /// <summary>Releases device-level resources.</summary>
        public void Dispose() { }

        /// <summary>Reads <paramref name="buffer"/>.Length bytes.</summary>
        public void Read(byte[] buffer) => this.Read(buffer, 0, buffer.Length);
        /// <summary>Writes <paramref name="buffer"/>.Length bytes.</summary>
        public void Write(byte[] buffer) => this.Write(buffer, 0, buffer.Length);
        /// <summary>Writes a rectangular sub-region of a larger framebuffer.</summary>
        public void Write(byte[] buffer, int xOffset, int yOffset, int width, int height, int originalWidth) => this.Write(buffer, xOffset, yOffset, width, height, originalWidth, 1, 1);
        /// <summary>Writes and reads simultaneously; both buffers must be the same length.</summary>
        public void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer) => this.TransferFullDuplex(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);
        /// <summary>Writes all of <paramref name="writeBuffer"/>, then reads all of <paramref name="readBuffer"/> in a single CS-low transaction.</summary>
        public void TransferSequential(byte[] writeBuffer, byte[] readBuffer) => this.TransferSequential(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);

        /// <summary>Reads <paramref name="length"/> bytes into <paramref name="buffer"/> starting at <paramref name="offset"/>.</summary>
        public void Read(byte[] buffer, int offset, int length) => this.WriteRead(null, 0, 0, buffer, offset, length);
        /// <summary>Writes <paramref name="length"/> bytes from <paramref name="buffer"/> starting at <paramref name="offset"/>.</summary>
        public void Write(byte[] buffer, int offset, int length) => this.WriteRead(buffer, offset, length, null, 0, 0);
        /// <summary>Full-duplex transfer with explicit slice offsets and lengths.</summary>
        public void TransferFullDuplex(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) => this.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength);

        /// <summary>Sequential write-then-read with explicit slice offsets and lengths.</summary>
        public void TransferSequential(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            this.WriteRead(writeBuffer, writeOffset, writeLength, null, 0, 0, false);
            this.WriteRead(null, 0, 0, readBuffer, readOffset, readLength);
        }

        private void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter = true) {
            this.Controller.SetActive(this);
            this.Controller.Provider.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, deselectAfter);
        }

        /// <summary>Writes a rectangular framebuffer region with optional pixel replication.</summary>
        public void Write(byte[] buffer, int x, int y, int width, int height, int originalWidth, int columnMultiplier, int rowMultiplier) {
            this.Controller.SetActive(this);
            this.Controller.Provider.Write(buffer, x, y, width, height, originalWidth, columnMultiplier, rowMultiplier);
        }
    }

    /// <summary>Per-device SPI bus settings.</summary>
    public class SpiConnectionSettings {
        /// <summary>How chip-select is driven.</summary>
        public SpiChipSelectType ChipSelectType { get; set; } = SpiChipSelectType.None;
        /// <summary>The GPIO pin that drives chip-select.</summary>
        public GpioPin ChipSelectLine { get; set; } = null;
        /// <summary>SCK frequency in Hz.</summary>
        public int ClockFrequency { get; set; } = 1_000_000;
        /// <summary>Frame width in bits. Fixed at 8 in this build.</summary>
        public int DataBitLength { get; } = 8;
        /// <summary>Bit ordering on the wire.</summary>
        public SpiDataFrame DataFrameFormat { get; set; } = SpiDataFrame.MsbFirst;
        /// <summary>SPI mode (clock polarity + phase).</summary>
        public SpiMode Mode { get; set; } = SpiMode.Mode0;
        /// <summary>Delay between asserting chip-select and starting the first clock.</summary>
        public TimeSpan ChipSelectSetupTime { get; set; } = TimeSpan.FromTicks(0);
        /// <summary>Delay between the last clock and deasserting chip-select.</summary>
        public TimeSpan ChipSelectHoldTime { get; set; } = TimeSpan.FromTicks(0);
        /// <summary>The level that means "selected" — false = active-low, true = active-high.</summary>
        public bool ChipSelectActiveState { get; set; } = false;
    }

    /// <summary>Bit ordering within an SPI byte.</summary>
    public enum SpiDataFrame {
        /// <summary>Most-significant bit first.</summary>
        MsbFirst = 0,
        /// <summary>Least-significant bit first.</summary>
        LsbFirst = 1
    }

    /// <summary>Standard SPI modes — combinations of clock polarity (CPOL) and clock phase (CPHA).</summary>
    public enum SpiMode {
        /// <summary>CPOL=0, CPHA=0.</summary>
        Mode0 = 0,
        /// <summary>CPOL=0, CPHA=1.</summary>
        Mode1 = 1,
        /// <summary>CPOL=1, CPHA=0.</summary>
        Mode2 = 2,
        /// <summary>CPOL=1, CPHA=1.</summary>
        Mode3 = 3,
    }

    /// <summary>How chip-select is driven.</summary>
    public enum SpiChipSelectType {
        /// <summary>No chip-select line is asserted by the controller.</summary>
        None = 0,
        /// <summary>Chip-select is driven by a user-supplied <see cref="GpioPin"/>.</summary>
        Gpio = 1
    }

    namespace Provider {
        /// <summary>Provider contract for an SPI controller.</summary>
        public interface ISpiControllerProvider : IDisposable {
            /// <summary>Number of hardware chip-select lines.</summary>
            int ChipSelectLineCount { get; }
            /// <summary>Minimum SCK frequency in Hz.</summary>
            int MinClockFrequency { get; }
            /// <summary>Maximum SCK frequency in Hz.</summary>
            int MaxClockFrequency { get; }
            /// <summary>Supported frame widths in bits.</summary>
            int[] SupportedDataBitLengths { get; }

            /// <summary>Applies the given settings before the next transfer.</summary>
            void SetActiveSettings(SpiConnectionSettings connectionSettings);
            /// <summary>Writes a rectangular framebuffer region with optional pixel replication.</summary>
            void Write(byte[] writeBuffer, int xOffset, int yOffset, int width, int height, int originalWidth, int columnMultiplier, int rowMultiplier);
            /// <summary>Performs a full-duplex transfer.</summary>
            void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter);
        }

        // No-op provider. Read buffers are zeroed; write inputs ignored.
        /// <summary>Concrete <see cref="ISpiControllerProvider"/> backed by the native TinyCLR SPI HAL.</summary>
        public sealed class SpiControllerApiWrapper : ISpiControllerProvider {
            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            /// <summary>Wraps the given native API as a provider.</summary>
            public SpiControllerApiWrapper(NativeApi api) => this.Api = api;

            /// <summary>Releases the native controller.</summary>
            public void Dispose() { }

            /// <inheritdoc/>
            public int ChipSelectLineCount => 1;
            /// <inheritdoc/>
            public int MinClockFrequency => 0;
            /// <inheritdoc/>
            public int MaxClockFrequency => 1_000_000_000;
            /// <inheritdoc/>
            public int[] SupportedDataBitLengths => new[] { 8 };

            /// <inheritdoc/>
            public void SetActiveSettings(SpiConnectionSettings connectionSettings) { }

            /// <inheritdoc/>
            public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter) {
                if (readBuffer != null && readLength > 0)
                    Array.Clear(readBuffer, readOffset, readLength);
            }

            /// <inheritdoc/>
            public void Write(byte[] writeBuffer, int xOffset, int yOffset, int width, int height, int originalWidth, int columnMultiplier, int rowMultiplier) { }
        }

        // Software provider on Desktop is also a no-op. The impl uses GPIO
        // bit-banging; here we just record the configuration and return
        // zeros on read. Constructors mirror the impl's so user code that
        // creates one compiles and runs.
        /// <summary>Software (bit-bang) SPI provider — Desktop stub.</summary>
        public sealed class SpiControllerSoftwareProvider : ISpiControllerProvider {
            private readonly IDictionary chipSelects;
            private readonly GpioController gpioController;

            /// <inheritdoc/>
            public int ChipSelectLineCount => 0;
            /// <inheritdoc/>
            public int MinClockFrequency => 0;
            /// <inheritdoc/>
            public int MaxClockFrequency => 1_000_000_000;
            /// <inheritdoc/>
            public int[] SupportedDataBitLengths => new[] { 8 };

            /// <summary>Builds a software SPI provider on the default <see cref="GpioController"/>.</summary>
            public SpiControllerSoftwareProvider(int mosiPinNumber, int misoPinNumber, int sckPinNumber)
                : this(GpioController.GetDefault(), mosiPinNumber, misoPinNumber, sckPinNumber) { }

            /// <summary>Builds a software SPI provider on the supplied <see cref="GpioController"/>.</summary>
            public SpiControllerSoftwareProvider(GpioController gpioController, int mosiPinNumber, int misoPinNumber, int sckPinNumber) {
                this.chipSelects = new Hashtable();
                this.gpioController = gpioController;
                // Open pins via the no-op Gpio.Desktop provider. Safe.
                gpioController.OpenPins(mosiPinNumber, misoPinNumber, sckPinNumber);
            }

            /// <summary>Releases the GPIO pins held for MOSI/MISO/SCK/CS.</summary>
            public void Dispose() { }

            /// <inheritdoc/>
            public void SetActiveSettings(SpiConnectionSettings connectionSettings) { }

            /// <inheritdoc/>
            public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter) {
                if (readBuffer != null && readLength > 0)
                    Array.Clear(readBuffer, readOffset, readLength);
            }

            /// <inheritdoc/>
            public void Write(byte[] writeBuffer, int xOffset, int yOffset, int width, int height, int originalWidth, int columnMultiplier, int rowMultiplier) { }
        }
    }
}
