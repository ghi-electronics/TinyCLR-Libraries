using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi.Provider;
using GHIElectronics.TinyCLR.Native;

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
        public static SpiController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.SpiController) is SpiController c ? c : SpiController.FromName(NativeApi.GetDefaultName(NativeApiType.SpiController));
        /// <summary>Returns an SPI controller identified by its native API name.</summary>
        /// <param name="name">Native API name.</param>
        public static SpiController FromName(string name) => SpiController.FromProvider(new SpiControllerApiWrapper(NativeApi.Find(name, NativeApiType.SpiController)));
        /// <summary>Creates a controller from a custom <see cref="ISpiControllerProvider"/> (including the software bit-bang provider).</summary>
        /// <param name="provider">Provider implementing the bus operations.</param>
        public static SpiController FromProvider(ISpiControllerProvider provider) => new SpiController(provider);

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>Creates a <see cref="SpiDevice"/> bound to this controller using the given settings.</summary>
        /// <param name="connectionSettings">Per-device clock, mode, and chip-select configuration.</param>
        public SpiDevice GetDevice(SpiConnectionSettings connectionSettings) => new SpiDevice(this, connectionSettings);

        /// <summary>Number of hardware chip-select lines exposed by this controller.</summary>
        public int ChipSelectLineCount => this.Provider.ChipSelectLineCount;
        /// <summary>Minimum SCK frequency in Hz the controller can generate.</summary>
        public int MinClockFrequency => this.Provider.MinClockFrequency;
        /// <summary>Maximum SCK frequency in Hz the controller can generate.</summary>
        public int MaxClockFrequency => this.Provider.MaxClockFrequency;
        /// <summary>Data-frame widths in bits that this controller supports.</summary>
        public int[] SupportedDataBitLengths => this.Provider.SupportedDataBitLengths;

        internal void SetActive(SpiDevice device) => this.Provider.SetActiveSettings(device.ConnectionSettings);
    }

    /// <summary>
    /// Represents a single chip on an SPI bus. Each transfer is preceded by a
    /// re-apply of <see cref="ConnectionSettings"/>, so multiple devices on the
    /// same controller can coexist without manual reconfiguration between calls.
    /// </summary>
    public class SpiDevice : IDisposable {
        /// <summary>The per-device bus settings (clock, mode, chip select, etc.).</summary>
        public SpiConnectionSettings ConnectionSettings { get; }
        /// <summary>The <see cref="SpiController"/> this device transacts over.</summary>
        public SpiController Controller { get; }

        internal SpiDevice(SpiController controller, SpiConnectionSettings connectionSettings) {
            this.ConnectionSettings = connectionSettings;
            this.Controller = controller;
        }

        /// <summary>Releases device-level resources. Does not close the underlying controller.</summary>
        public void Dispose() {

        }

        /// <summary>Reads <paramref name="buffer"/>.Length bytes; transmits zeros while reading.</summary>
        /// <param name="buffer">Destination buffer.</param>
        public void Read(byte[] buffer) => this.Read(buffer, 0, buffer.Length);
        /// <summary>Writes <paramref name="buffer"/>.Length bytes; discards received data.</summary>
        /// <param name="buffer">Source buffer.</param>
        public void Write(byte[] buffer) => this.Write(buffer, 0, buffer.Length);
        /// <summary>Writes a rectangular sub-region of a larger framebuffer. Display driver helper.</summary>
        /// <param name="buffer">Source framebuffer (RGB565 pairs of bytes).</param>
        /// <param name="xOffset">Left edge of the source rectangle within the framebuffer.</param>
        /// <param name="yOffset">Top edge of the source rectangle within the framebuffer.</param>
        /// <param name="width">Width of the source rectangle in pixels.</param>
        /// <param name="height">Height of the source rectangle in pixels.</param>
        /// <param name="originalWidth">Pixel-width of the full framebuffer.</param>
        public void Write(byte[] buffer, int xOffset, int yOffset, int width, int height, int originalWidth) => this.Write(buffer, xOffset, yOffset, width, height, originalWidth, 1, 1);
        /// <summary>Writes and reads simultaneously; both buffers must be the same length.</summary>
        /// <param name="writeBuffer">Bytes to transmit.</param>
        /// <param name="readBuffer">Receives the bytes shifted in.</param>
        public void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer) => this.TransferFullDuplex(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);
        /// <summary>Writes all of <paramref name="writeBuffer"/>, then reads all of <paramref name="readBuffer"/> in a single CS-low transaction.</summary>
        public void TransferSequential(byte[] writeBuffer, byte[] readBuffer) => this.TransferSequential(writeBuffer, 0, writeBuffer.Length, readBuffer, 0, readBuffer.Length);

        /// <summary>Reads <paramref name="length"/> bytes into <paramref name="buffer"/> starting at <paramref name="offset"/>.</summary>
        public void Read(byte[] buffer, int offset, int length) => this.WriteRead(null, 0, 0, buffer, offset, length);
        /// <summary>Writes <paramref name="length"/> bytes from <paramref name="buffer"/> starting at <paramref name="offset"/>.</summary>
        public void Write(byte[] buffer, int offset, int length) => this.WriteRead(buffer, offset, length, null, 0, 0);
        /// <summary>Full-duplex transfer with explicit slice offsets and lengths.</summary>
        public void TransferFullDuplex(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) => this.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength);

        /// <summary>Sequential write-then-read with explicit slice offsets and lengths; chip select stays asserted between the two phases.</summary>
        public void TransferSequential(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength) {
            this.WriteRead(writeBuffer, writeOffset, writeLength, null, 0, 0, false);
            this.WriteRead(null, 0, 0, readBuffer, readOffset, readLength);
        }

        private void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter = true) {
            this.Controller.SetActive(this);

            this.Controller.Provider.WriteRead(writeBuffer, writeOffset, writeLength, readBuffer, readOffset, readLength, deselectAfter);
        }

        /// <summary>
        /// Writes a rectangular framebuffer region with optional pixel replication.
        /// <paramref name="columnMultiplier"/> and <paramref name="rowMultiplier"/> let the
        /// display driver scale up small framebuffers without a CPU-side resize.
        /// </summary>
        public void Write(byte[] buffer, int x, int y, int width, int height, int originalWidth, int columnMultiplier, int rowMultiplier) {
            if (buffer == null) {
                throw new ArgumentNullException("buffer");
            }

            if ((x < 0) || (y < 0) || ((x + width) > originalWidth) || ((width * height * 2) > buffer.Length) || (columnMultiplier <= 0) || (rowMultiplier <= 0)) {
                throw new ArgumentOutOfRangeException();
            }

            var originalHeight = (buffer.Length / 2) / originalWidth;

            if (y + height > originalHeight)
                throw new ArgumentOutOfRangeException();

            this.Controller.SetActive(this);

            this.Controller.Provider.Write(buffer, x, y, width, height, originalWidth, columnMultiplier, rowMultiplier);
        }
    }

    /// <summary>Per-device SPI bus settings: clock, mode, chip select, etc.</summary>
    public class SpiConnectionSettings {
        /// <summary>How the chip-select line is driven (none or via a managed <see cref="GpioPin"/>).</summary>
        public SpiChipSelectType ChipSelectType { get; set; } = SpiChipSelectType.None;
        /// <summary>The GPIO pin that drives chip-select when <see cref="ChipSelectType"/> is <see cref="SpiChipSelectType.Gpio"/>.</summary>
        public GpioPin ChipSelectLine { get; set; } = null;
        /// <summary>SCK frequency in Hz the controller is asked to use.</summary>
        public int ClockFrequency { get; set; } = 1_000_000;
        /// <summary>Frame width in bits. Fixed at 8 in this build.</summary>
        public int DataBitLength { get; } = 8;
        /// <summary>Bit ordering on the wire.</summary>
        public SpiDataFrame DataFrameFormat { get; set; } = SpiDataFrame.MsbFirst;
        /// <summary>SPI mode (clock polarity + phase). See <see cref="SpiMode"/>.</summary>
        public SpiMode Mode { get; set; } = SpiMode.Mode0;
        /// <summary>Delay between asserting chip-select and starting the first clock.</summary>
        public TimeSpan ChipSelectSetupTime { get; set; } = TimeSpan.FromTicks(0);
        /// <summary>Delay between the last clock and deasserting chip-select.</summary>
        public TimeSpan ChipSelectHoldTime { get; set; } = TimeSpan.FromTicks(0);
        /// <summary>The level that means "selected" — false = active-low (most chips), true = active-high.</summary>
        public bool ChipSelectActiveState { get; set; } = false;
    }

    /// <summary>Bit ordering within an SPI byte.</summary>
    public enum SpiDataFrame {
        /// <summary>Most-significant bit first (the conventional default).</summary>
        MsbFirst = 0,
        /// <summary>Least-significant bit first.</summary>
        LsbFirst = 1
    }

    /// <summary>
    /// Standard SPI modes — combinations of clock polarity (CPOL) and clock phase (CPHA).
    /// Mode 0 (CPOL=0, CPHA=0) is the most common.
    /// </summary>
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
            /// <summary>Number of hardware chip-select lines exposed by this controller.</summary>
            int ChipSelectLineCount { get; }
            /// <summary>Minimum SCK frequency in Hz.</summary>
            int MinClockFrequency { get; }
            /// <summary>Maximum SCK frequency in Hz.</summary>
            int MaxClockFrequency { get; }
            /// <summary>Supported frame widths in bits.</summary>
            int[] SupportedDataBitLengths { get; }

            /// <summary>Applies the given settings before the next transfer.</summary>
            /// <param name="connectionSettings">Per-device configuration.</param>
            void SetActiveSettings(SpiConnectionSettings connectionSettings);
            /// <summary>Writes a rectangular framebuffer region with optional pixel replication.</summary>
            void Write(byte[] writeBuffer, int xOffset, int yOffset, int width, int height, int originalWidth, int columnMultiplier, int rowMultiplier);
            /// <summary>Performs a full-duplex transfer.</summary>
            /// <param name="writeBuffer">Bytes to transmit, or null for read-only.</param>
            /// <param name="writeOffset">Starting offset within <paramref name="writeBuffer"/>.</param>
            /// <param name="writeLength">Number of bytes to transmit.</param>
            /// <param name="readBuffer">Destination buffer for received bytes, or null to discard.</param>
            /// <param name="readOffset">Starting offset within <paramref name="readBuffer"/>.</param>
            /// <param name="readLength">Number of bytes to receive.</param>
            /// <param name="deselectAfter">If false, leaves chip-select asserted after the transfer (for sequential read-after-write).</param>
            void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter);
        }

        /// <summary>Concrete <see cref="ISpiControllerProvider"/> backed by the native TinyCLR SPI HAL.</summary>
        public sealed class SpiControllerApiWrapper : ISpiControllerProvider {
            private readonly IntPtr impl;

            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            /// <summary>Wraps the given native API as a provider.</summary>
            public SpiControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            /// <summary>Releases the native controller.</summary>
            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            /// <inheritdoc/>
            public extern int ChipSelectLineCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern int MinClockFrequency { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern int MaxClockFrequency { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            /// <inheritdoc/>
            public extern int[] SupportedDataBitLengths { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetActiveSettings(SpiConnectionSettings connectionSettings);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Write(byte[] writeBuffer, int xOffset, int yOffset, int width, int height, int originalWidth, int columnMultiplier, int rowMultiplier);

        }

        /// <summary>
        /// Software (bit-bang) SPI provider. Useful when no hardware SPI peripheral is
        /// available on the desired pins, or to escape pin-mux conflicts. Significantly
        /// slower than the native hardware provider.
        /// </summary>
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
            private TimeSpan chipSelectSetupTime;
            private TimeSpan chipSelectHoldTime;
            private bool chipSelectActiveState;

            /// <inheritdoc/>
            public int ChipSelectLineCount => 0;
            /// <inheritdoc/>
            public int MinClockFrequency => 0;
            /// <inheritdoc/>
            public int MaxClockFrequency => 1_000_000_000;
            /// <inheritdoc/>
            public int[] SupportedDataBitLengths => new[] { 8 };

            /// <summary>Builds a software SPI provider on the default <see cref="GpioController"/>.</summary>
            /// <param name="mosiPinNumber">Pin used as MOSI (controller output).</param>
            /// <param name="misoPinNumber">Pin used as MISO (controller input).</param>
            /// <param name="sckPinNumber">Pin used as SCK (clock).</param>
            public SpiControllerSoftwareProvider(int mosiPinNumber, int misoPinNumber, int sckPinNumber) : this(GpioController.GetDefault(), mosiPinNumber, misoPinNumber, sckPinNumber) {

            }

            /// <summary>Builds a software SPI provider on the supplied <see cref="GpioController"/>.</summary>
            /// <param name="gpioController">The GPIO controller that owns the bus pins.</param>
            /// <param name="mosiPinNumber">Pin used as MOSI.</param>
            /// <param name="misoPinNumber">Pin used as MISO.</param>
            /// <param name="sckPinNumber">Pin used as SCK.</param>
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

            /// <summary>Releases the GPIO pins held for MOSI/MISO/SCK/CS.</summary>
            public void Dispose() {
                this.mosi.Dispose();
                this.miso.Dispose();
                this.sck.Dispose();
                this.cs?.Dispose();
            }

            /// <inheritdoc/>
            public void SetActiveSettings(SpiConnectionSettings connectionSettings) {
                this.captureOnRisingEdge = ((((int)connectionSettings.Mode) & 0x01) == 0);
                this.clockActiveState = (((int)connectionSettings.Mode) & 0x02) == 0 ? GpioPinValue.High : GpioPinValue.Low;
                this.clockIdleState = this.clockActiveState == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High;
                this.chipSelectHoldTime = connectionSettings.ChipSelectHoldTime;
                this.chipSelectSetupTime = connectionSettings.ChipSelectSetupTime;
                this.chipSelectActiveState = connectionSettings.ChipSelectActiveState;

                if (connectionSettings.ChipSelectType == SpiChipSelectType.Gpio) {
                    if (!this.chipSelects.Contains(connectionSettings.ChipSelectLine)) {
                        var cs = connectionSettings.ChipSelectLine;

                        this.chipSelects[connectionSettings.ChipSelectLine] = cs;

                        cs.Write(GpioPinValue.High);
                        cs.SetDriveMode(GpioPinDriveMode.Output);
                    }

                    this.cs = (GpioPin)this.chipSelects[connectionSettings.ChipSelectLine];
                    this.cs.Write(this.chipSelectActiveState ? GpioPinValue.Low : GpioPinValue.High);
                }
            }

            /// <inheritdoc/>
            public void WriteRead(byte[] writeBuffer, int writeOffset, int writeLength, byte[] readBuffer, int readOffset, int readLength, bool deselectAfter) {
                if (readBuffer != null)
                    Array.Clear(readBuffer, 0, readLength);

                this.sck.Write(this.clockIdleState);
                this.cs?.Write(this.chipSelectActiveState ? GpioPinValue.High : GpioPinValue.Low);

                if (this.chipSelectSetupTime.TotalMilliseconds > 0)
                    Thread.Sleep((int)this.chipSelectSetupTime.TotalMilliseconds);

                for (var i = 0; i < Math.Max(readLength, writeLength); i++) {
                    byte mask = 0x80;
                    var w = i < writeLength && writeBuffer != null ? writeBuffer[i + writeOffset] : (byte)0;
                    var r = false;

                    for (var j = 0; j < 8; j++) {
                        if (this.captureOnRisingEdge) {
                            var currentTicks = DateTime.Now.Ticks;

                            this.sck.Write(this.clockIdleState);

                            this.mosi.Write((w & mask) != 0 ? GpioPinValue.High : GpioPinValue.Low);
                            r = this.miso.Read() == GpioPinValue.High;

                            var periodClockTicks = DateTime.Now.Ticks - currentTicks;

                            currentTicks = DateTime.Now.Ticks;

                            this.sck.Write(this.clockActiveState);

                            while (DateTime.Now.Ticks - currentTicks < periodClockTicks) ;
                        }
                        else {
                            var currentTicks = DateTime.Now.Ticks;

                            this.sck.Write(this.clockActiveState);

                            this.mosi.Write((w & mask) != 0 ? GpioPinValue.High : GpioPinValue.Low);
                            r = this.miso.Read() == GpioPinValue.High;

                            var periodClockTicks = DateTime.Now.Ticks - currentTicks;

                            currentTicks = DateTime.Now.Ticks;

                            this.sck.Write(this.clockIdleState);

                            while (DateTime.Now.Ticks - currentTicks < periodClockTicks) ;
                        }

                        if (i < readLength && readBuffer != null && r)
                            readBuffer[i + readOffset] |= mask;

                        mask >>= 1;
                    }
                }

                this.sck.Write(this.clockIdleState);

                if (this.chipSelectHoldTime.TotalMilliseconds > 0)
                    Thread.Sleep((int)this.chipSelectHoldTime.TotalMilliseconds);

                if (deselectAfter)
                    this.cs?.Write(this.chipSelectActiveState ? GpioPinValue.Low : GpioPinValue.High);
            }

            /// <inheritdoc/>
            public void Write(byte[] writeBuffer, int xOffset, int yOffset, int width, int height, int originalWidth, int columnMultiplier, int rowMultiplier) => throw new Exception("SoftwareSpi does not support this feature.");
        }
    }
}
