#define SUPPORT_ORIGINAL_BRAINPAD

using System;
using System.ComponentModel;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.Sitronix.ST7735;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class Picture {
        public int Height { get; }
        public int Width { get; }
        internal byte[] Data { get; }

        internal Picture(int width, int height, byte[] data, int scale) {
            if (width * height != data.Length) throw new Exception("Incorrect picture data size");
            if (scale <= 0) throw new Exception("Scale can't be zero or negative.");

            this.Height = height * scale;
            this.Width = width * scale;

            if (scale == 1) {
                this.Data = data;
            }
            else {
                this.Data = new byte[this.Width * this.Height];

                for (var x = 0; x < this.Width; x++)
                    for (var y = 0; y < this.Height; y++)
                        this.Data[y * this.Width + x] = data[y / scale * width + x / scale];
            }
        }
    }

    public class SSD1306Controller {
        private readonly byte[] vram = new byte[128 * 64 / 8 + 1];
        private readonly byte[] buffer2 = new byte[2];
        private readonly I2cDevice i2c;

        public DisplayDataFormat DataFormat => (DisplayDataFormat)(-1);
        public int Width => 128;
        public int Height => 64;

        public int MaxWidth => 128;
        public int MaxHeight => 64;

        public static I2cConnectionSettings GetConnectionSettings() => new I2cConnectionSettings(0x3C) {
            AddressFormat = I2cAddressFormat.SevenBit,
            BusSpeed = I2cBusSpeed.FastMode,
        };

        public SSD1306Controller(I2cDevice i2c) {
            this.vram[0] = 0x40;
            this.i2c = i2c;

            this.Initialize();
        }

        private void Initialize() {
            this.SendCommand(0xae);// turn off oled panel
            this.SendCommand(0x00);// set low column address
            this.SendCommand(0x10);// set high column address
            this.SendCommand(0x40);// set start line address
            this.SendCommand(0x81);// set contrast control register
            this.SendCommand(0xcf);
            this.SendCommand(0xa1);// set segment re-map 95 to 0
            this.SendCommand(0xa6);// set normal display
            this.SendCommand(0xa8);// set multiplex ratio(1 to 64)
            this.SendCommand(0x3f);// 1/64 duty
            this.SendCommand(0xd3);// set display offset
            this.SendCommand(0x00);// not offset
            this.SendCommand(0xd5);// set display clock divide ratio/oscillator frequency
            this.SendCommand(0x80);// set divide ratio
            this.SendCommand(0xd9);// set pre-charge period
            this.SendCommand(0xf1);
            this.SendCommand(0xda);// set com pins hardware configuration
            this.SendCommand(0x12);
            this.SendCommand(0xdb);//--set vcomh
            this.SendCommand(0x40);//--set startline 0x0
            this.SendCommand(0x8d);//--set Charge Pump enable/disable
            this.SendCommand(0x14);//--set(0x10) disable
            this.SendCommand(0xaf);//--turn on oled panel
            this.SendCommand(0xc8);// mirror the screen

            // Mapping
            this.SendCommand(0x20);
            this.SendCommand(0x00);
            this.SendCommand(0x21);
            this.SendCommand(0);
            this.SendCommand(128 - 1);
            this.SendCommand(0x22);
            this.SendCommand(0);
            this.SendCommand(7);
        }

        public void Dispose() => this.i2c.Dispose();

        public void SendCommand(byte cmd) {
            this.buffer2[1] = cmd;
            this.i2c.Write(this.buffer2);
        }

        public void SetColorFormat(bool invert) => this.SendCommand((byte)(invert ? 0xa7 : 0xa6));

        public void DrawBuffer(byte[] buffer) => this.DrawBuffer(buffer, 0);

        public void DrawBuffer(byte[] buffer, int offset) {
            Array.Copy(buffer, offset, this.vram, 1, this.vram.Length - 1);

            this.i2c.Write(this.vram);
        }
    }

    public class Display {
        private enum Transform {
            FlipHorizontal,
            FlipVertical,
            Rotate90,
            Rotate180,
            Rotate270,
            None
        }

        private I2cDevice i2cDevice;
        private SSD1306Controller ssd1306;

        public Picture CreatePicture(int width, int height, byte[] data) => this.CreateScaledPicture(width, height, data, 1);
        public Picture CreateScaledPicture(int width, int height, byte[] data, int scale) => data != null ? new Picture(width, height, data, scale) : throw new Exception("Incorrect picture data size");

#if SUPPORT_ORIGINAL_BRAINPAD
        private const int ORIGINAL_VRAMS = 12;

        private SpiDevice spi;
        private GpioPin controlPin;
        private GpioPin resetPin;
        private GpioPin backlightPin;
#endif

        /// <summary>
        /// The width of the display in pixels.
        /// </summary>
        public int Width { get; } = 128;

        /// <summary>
        /// The height of the display in pixels.
        /// </summary>
        public int Height { get; } = 64;

        private byte[][] vrams;
        private byte[] vram;
#if SUPPORT_ORIGINAL_BRAINPAD
        private ST7735Controller st7735;
#endif

        public Display() {

            switch (Board.BoardType) {
#if SUPPORT_ORIGINAL_BRAINPAD
                case BoardType.Original:
                    // we will not allocate memory unless the display is used
                    //this.vram= new byte[128 * 64 * 2];

                    var GPIO = GpioController.GetDefault();
                    this.controlPin = GPIO.OpenPin(G30.GpioPin.PC5);
                    this.resetPin = GPIO.OpenPin(G30.GpioPin.PC4);
                    this.backlightPin = GPIO.OpenPin(G30.GpioPin.PA4);

                    this.controlPin.SetDriveMode(GpioPinDriveMode.Output);
                    this.resetPin.SetDriveMode(GpioPinDriveMode.Output);
                    this.backlightPin.SetDriveMode(GpioPinDriveMode.Output);
                    this.backlightPin.Write(GpioPinValue.High);

                    var settings = new SpiConnectionSettings {
                        Mode = SpiMode.Mode3,
                        ClockFrequency = 12000000,
                        DataBitLength = 8,
                        ChipSelectType = SpiChipSelectType.Gpio,
                        ChipSelectLine = G30.GpioPin.PB12
                    };
                    this.spi = SpiController.FromName(G30.SpiBus.Spi2).GetDevice(settings);

                    this.st7735 = new ST7735Controller(this.spi, this.controlPin, this.resetPin);
                    this.st7735.SetDataFormat(Devices.Display.DisplayDataFormat.Rgb444);
                    this.st7735.SetDataAccessControl(true, true, false, false);
                    this.st7735.SetDrawWindow((160 - this.Width) / 2, (128 - this.Height) / 2, this.Width, this.Height);
                    this.st7735.Enable();

                    break;
#endif
                case BoardType.BP2:
                    this.vram = new byte[(128 * 64 / 8)];

                    this.i2cDevice = I2cController.FromName(Board.BoardType == BoardType.BP2 ? FEZCLR.I2cBus.I2c1 : G30.I2cBus.I2c1).GetDevice(SSD1306Controller.GetConnectionSettings());
                    this.ssd1306 = new SSD1306Controller(this.i2cDevice);

                    break;
            }
            this.Clear();
            this.RefreshScreen();
        }

        public void RefreshScreen() {

            switch (Board.BoardType) {
#if SUPPORT_ORIGINAL_BRAINPAD
                case BoardType.Original:
                    if (this.vrams == null)
                        return;

                    this.st7735.SendDrawCommand();
                    for (var i = 0; i < this.vrams.Length; i++)
                        this.st7735.SendData(this.vrams[i]);

                    break;
#endif
                case BoardType.BP2:
                    this.ssd1306.DrawBuffer(this.vram);
                    break;
            }
        }

        private void Point(int x, int y, bool set) {
            if (x < 0 || x > 127)
                return;
            if (y < 0 || y > 63)
                return;

            switch (Board.BoardType) {
#if SUPPORT_ORIGINAL_BRAINPAD
                case BoardType.Original:
                    var total = (int)(this.Width * this.Height * 1.5);
                    var each = total / Display.ORIGINAL_VRAMS;

                    if (this.vrams == null) {
                        //We have many small arrays to reduce chance of our of memory because of fragmentation. Make sure total is evenly divisible by vrams count
                        GC.Collect();

                        this.vrams = new byte[Display.ORIGINAL_VRAMS][];

                        for (var i = 0; i < this.vrams.Length; i++)
                            this.vrams[i] = new byte[each];
                    }

                    var pixel = y * this.Width + x;
                    var addr = 0;
                    var mask = 0;

                    if ((pixel % 2) == 0) {
                        addr = (int)(pixel * 1.5 + 1);
                        mask = 0xF0;
                    }
                    else {
                        addr = (int)((pixel - 1) * 1.5 + 2);
                        mask = 0x0F;
                    }

                    var arr = addr / each;

                    addr %= each;

                    if (set) {
                        this.vrams[arr][addr] |= (byte)mask;
                    }
                    else {
                        this.vrams[arr][addr] &= (byte)~mask;
                    }
                    break;
#endif
                case BoardType.BP2:
                    var index = x + (y / 8) * 128;

                    if (set)
                        this.vram[index] |= (byte)(1 << (y % 8));
                    else
                        this.vram[index] &= (byte)(~(1 << (y % 8)));
                    break;
            }

        }

        /// <summary>
        /// Draws a pixel.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="color">The color to draw.</param>
        public void DrawPoint(int x, int y) => this.Point(x, y, true);

        /// <summary>
        /// Clears a pixel.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="color">The color to draw.</param>
        public void ClearPoint(int x, int y) => this.Point(x, y, false);

        /// <summary>
        /// Clears the Display.
        /// </summary>
        public void Clear() {
            switch (Board.BoardType) {
#if SUPPORT_ORIGINAL_BRAINPAD
                case BoardType.Original:
                    if (this.vrams == null)
                        return;

                    for (var i = 0; i < this.vrams.Length; i++)
                        Array.Clear(this.vrams[i], 0, this.vrams[i].Length);

                    break;
#endif
                case BoardType.BP2:
                    if (this.vram == null)
                        return;
                    Array.Clear(this.vram, 0, this.vram.Length);
                    break;
            }
        }

        public void ClearPart(int x, int y, int width, int height) {
            if (x == 0 && y == 0 && width == 128 && height == 64) this.Clear();
            for (var lx = x; lx < width + x; lx++)
                for (var ly = y; ly < height + y; ly++)
                    this.Point(lx, ly, false);
        }

        /// <summary>
        /// Draws an picture at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="picture">The picture to draw.</param>
        public void DrawPicture(int x, int y, Picture picture) => this.DrawRotatedPicture(x, y, picture, Transform.None);
        public void DrawPictureRotated90Degrees(int x, int y, Picture picture) => this.DrawRotatedPicture(x, y, picture, Transform.Rotate90);
        public void DrawPictureRotated180Degrees(int x, int y, Picture picture) => this.DrawRotatedPicture(x, y, picture, Transform.Rotate180);
        public void DrawPictureRotated270Degrees(int x, int y, Picture picture) => this.DrawRotatedPicture(x, y, picture, Transform.Rotate270);
        public void DrawPictureFlippedHorizontally(int x, int y, Picture picture) => this.DrawRotatedPicture(x, y, picture, Transform.FlipHorizontal);
        public void DrawPictureFlippedVertically(int x, int y, Picture picture) => this.DrawRotatedPicture(x, y, picture, Transform.FlipVertical);

        private void DrawRotatedPicture(int x, int y, Picture picture, Transform mirror) {
            if (picture == null) throw new ArgumentNullException("picture");

            for (var xd = 0; xd < picture.Width; xd++) {
                for (var yd = 0; yd < picture.Height; yd++) {
                    switch (mirror) {
                        case Transform.None:
                            this.Point(x + xd, y + yd, picture.Data[picture.Width * yd + xd] == 1);
                            break;
                        case Transform.FlipHorizontal:
                            this.Point(x + picture.Width - xd, y + yd, picture.Data[picture.Width * yd + xd] == 1);
                            break;
                        case Transform.FlipVertical:
                            this.Point(x + xd, y + picture.Height - yd, picture.Data[picture.Width * yd + xd] == 1);
                            break;
                        case Transform.Rotate90:
                            this.Point(x + picture.Width - yd, y + xd, picture.Data[picture.Width * yd + xd] == 1);
                            break;
                        case Transform.Rotate180:
                            this.Point(x + picture.Width - xd, y + picture.Height - yd, picture.Data[picture.Width * yd + xd] == 1);
                            break;
                        case Transform.Rotate270:
                            this.Point(x + yd, y + picture.Height - xd, picture.Data[picture.Width * yd + xd] == 1);
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="x">The x coordinate to start drawing at.</param>
        /// <param name="y">The y coordinate to start drawing at.</param>
        /// <param name="x1">The ending x coordinate.</param>
        /// <param name="y1">The ending y coordinate.</param>
        /// <param name="color">The color to draw.</param>
        public void DrawLine(int x0, int y0, int x1, int y1) {

            var dy = y1 - y0;
            var dx = x1 - x0;
            int stepx, stepy;

            if (dy < 0) { dy = -dy; stepy = -1; } else { stepy = 1; }
            if (dx < 0) { dx = -dx; stepx = -1; } else { stepx = 1; }
            dy <<= 1;                                                  // dy is now 2*dy
            dx <<= 1;                                                  // dx is now 2*dx

            this.Point(x0, y0, true);
            if (dx > dy) {
                var fraction = dy - (dx >> 1);                         // same as 2*dy - dx
                while (x0 != x1) {
                    if (fraction >= 0) {
                        y0 += stepy;
                        fraction -= dx;                                // same as fraction -= 2*dx
                    }
                    x0 += stepx;
                    fraction += dy;                                    // same as fraction -= 2*dy
                    this.Point(x0, y0, true);
                }
            }
            else {
                var fraction = dx - (dy >> 1);
                while (y0 != y1) {
                    if (fraction >= 0) {
                        x0 += stepx;
                        fraction -= dy;
                    }
                    y0 += stepy;
                    fraction += dx;
                    this.Point(x0, y0, true);
                }
            }

            /*var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            int t, dX, dY, yStep, error;

            if (steep) {
                t = x0;
                x0 = y0;
                y0 = t;
                t = x1;
                x1 = y1;
                y1 = t;
            }

            if (x0 > x1) {
                t = x0;
                x0 = x1;
                x1 = t;

                t = y0;
                y0 = y1;
                y1 = t;
            }

            dX = x1 - x0;
            dY = System.Math.Abs(y1 - y0);

            error = (dX / 2);

            if (y0 < y1) {
                yStep = 1;
            }
            else {
                yStep = -1;
            }

            for (; x0 < x1; x0++) {
                if (steep) {
                    DrawPoint(y0, x0, color);
                }
                else {
                    DrawPoint(x0, y0, color);
                }

                error -= dY;

                if (error < 0) {
                    y0 += (byte)yStep;
                    error += dX;
                }
            }*/

        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="r">The radius of the circle.</param>
        /// <param name="color">The color to draw.</param>
        public void DrawCircle(int x, int y, int r) {

            if (r <= 0) return;

            var f = 1 - r;
            var ddFX = 1;
            var ddFY = -2 * r;
            var dX = 0;
            var dY = r;

            this.DrawPoint(x, y + r);
            this.DrawPoint(x, y - r);
            this.DrawPoint(x + r, y);
            this.DrawPoint(x - r, y);

            while (dX < dY) {
                if (f >= 0) {
                    dY--;
                    ddFY += 2;
                    f += ddFY;
                }

                dX++;
                ddFX += 2;
                f += ddFX;

                this.DrawPoint(x + dX, y + dY);
                this.DrawPoint(x - dX, y + dY);
                this.DrawPoint(x + dX, y - dY);
                this.DrawPoint(x - dX, y - dY);

                this.DrawPoint(x + dY, y + dX);
                this.DrawPoint(x - dY, y + dX);
                this.DrawPoint(x + dY, y - dX);
                this.DrawPoint(x - dY, y - dX);
            }
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <param name="color">The color to use.</param>
        public void DrawRectangle(int x, int y, int width, int height) {
            if (width < 0) return;
            if (height < 0) return;

            for (var i = x; i < x + width; i++) {
                this.DrawPoint(i, y);
                this.DrawPoint(i, y + height - 1);
            }

            for (var i = y; i < y + height; i++) {
                this.DrawPoint(x, i);
                this.DrawPoint(x + width - 1, i);
            }
        }

        public void DrawFilledRectangle(int x, int y, int width, int height) {
            for (var lx = x; lx < width + x; lx++)
                for (var ly = y; ly < height + y; ly++)
                    this.Point(lx, ly, true);
        }

        byte[] font = new byte[95 * 5] {
            0x00, 0x00, 0x00, 0x00, 0x00, /* Space	0x20 */
            0x00, 0x00, 0x4f, 0x00, 0x00, /* ! */
            0x00, 0x07, 0x00, 0x07, 0x00, /* " */
            0x14, 0x7f, 0x14, 0x7f, 0x14, /* # */
            0x24, 0x2a, 0x7f, 0x2a, 0x12, /* $ */
            0x23, 0x13, 0x08, 0x64, 0x62, /* % */
            0x36, 0x49, 0x55, 0x22, 0x20, /* & */
            0x00, 0x05, 0x03, 0x00, 0x00, /* ' */
            0x00, 0x1c, 0x22, 0x41, 0x00, /* ( */
            0x00, 0x41, 0x22, 0x1c, 0x00, /* ) */
            0x14, 0x08, 0x3e, 0x08, 0x14, /* // */
            0x08, 0x08, 0x3e, 0x08, 0x08, /* + */
            0x50, 0x30, 0x00, 0x00, 0x00, /* , */
            0x08, 0x08, 0x08, 0x08, 0x08, /* - */
            0x00, 0x60, 0x60, 0x00, 0x00, /* . */
            0x20, 0x10, 0x08, 0x04, 0x02, /* / */
            0x3e, 0x51, 0x49, 0x45, 0x3e, /* 0		0x30 */
            0x00, 0x42, 0x7f, 0x40, 0x00, /* 1 */
            0x42, 0x61, 0x51, 0x49, 0x46, /* 2 */
            0x21, 0x41, 0x45, 0x4b, 0x31, /* 3 */
            0x18, 0x14, 0x12, 0x7f, 0x10, /* 4 */
            0x27, 0x45, 0x45, 0x45, 0x39, /* 5 */
            0x3c, 0x4a, 0x49, 0x49, 0x30, /* 6 */
            0x01, 0x71, 0x09, 0x05, 0x03, /* 7 */
            0x36, 0x49, 0x49, 0x49, 0x36, /* 8 */
            0x06, 0x49, 0x49, 0x29, 0x1e, /* 9 */
            0x00, 0x36, 0x36, 0x00, 0x00, /* : */
            0x00, 0x56, 0x36, 0x00, 0x00, /* ; */
            0x08, 0x14, 0x22, 0x41, 0x00, /* < */
            0x14, 0x14, 0x14, 0x14, 0x14, /* = */
            0x00, 0x41, 0x22, 0x14, 0x08, /* > */
            0x02, 0x01, 0x51, 0x09, 0x06, /* ? */
            0x3e, 0x41, 0x5d, 0x55, 0x1e, /* @		0x40 */
            0x7e, 0x11, 0x11, 0x11, 0x7e, /* A */
            0x7f, 0x49, 0x49, 0x49, 0x36, /* B */
            0x3e, 0x41, 0x41, 0x41, 0x22, /* C */
            0x7f, 0x41, 0x41, 0x22, 0x1c, /* D */
            0x7f, 0x49, 0x49, 0x49, 0x41, /* E */
            0x7f, 0x09, 0x09, 0x09, 0x01, /* F */
            0x3e, 0x41, 0x49, 0x49, 0x7a, /* G */
            0x7f, 0x08, 0x08, 0x08, 0x7f, /* H */
            0x00, 0x41, 0x7f, 0x41, 0x00, /* I */
            0x20, 0x40, 0x41, 0x3f, 0x01, /* J */
            0x7f, 0x08, 0x14, 0x22, 0x41, /* K */
            0x7f, 0x40, 0x40, 0x40, 0x40, /* L */
            0x7f, 0x02, 0x0c, 0x02, 0x7f, /* M */
            0x7f, 0x04, 0x08, 0x10, 0x7f, /* N */
            0x3e, 0x41, 0x41, 0x41, 0x3e, /* O */
            0x7f, 0x09, 0x09, 0x09, 0x06, /* P		0x50 */
            0x3e, 0x41, 0x51, 0x21, 0x5e, /* Q */
            0x7f, 0x09, 0x19, 0x29, 0x46, /* R */
            0x26, 0x49, 0x49, 0x49, 0x32, /* S */
            0x01, 0x01, 0x7f, 0x01, 0x01, /* T */
            0x3f, 0x40, 0x40, 0x40, 0x3f, /* U */
            0x1f, 0x20, 0x40, 0x20, 0x1f, /* V */
            0x3f, 0x40, 0x38, 0x40, 0x3f, /* W */
            0x63, 0x14, 0x08, 0x14, 0x63, /* X */
            0x07, 0x08, 0x70, 0x08, 0x07, /* Y */
            0x61, 0x51, 0x49, 0x45, 0x43, /* Z */
            0x00, 0x7f, 0x41, 0x41, 0x00, /* [ */
            0x02, 0x04, 0x08, 0x10, 0x20, /* \ */
            0x00, 0x41, 0x41, 0x7f, 0x00, /* ] */
            0x04, 0x02, 0x01, 0x02, 0x04, /* ^ */
            0x40, 0x40, 0x40, 0x40, 0x40, /* _ */
            0x00, 0x00, 0x03, 0x05, 0x00, /* `		0x60 */
            0x20, 0x54, 0x54, 0x54, 0x78, /* a */
            0x7F, 0x44, 0x44, 0x44, 0x38, /* b */
            0x38, 0x44, 0x44, 0x44, 0x44, /* c */
            0x38, 0x44, 0x44, 0x44, 0x7f, /* d */
            0x38, 0x54, 0x54, 0x54, 0x18, /* e */
            0x04, 0x04, 0x7e, 0x05, 0x05, /* f */
            0x08, 0x54, 0x54, 0x54, 0x3c, /* g */
            0x7f, 0x08, 0x04, 0x04, 0x78, /* h */
            0x00, 0x44, 0x7d, 0x40, 0x00, /* i */
            0x20, 0x40, 0x44, 0x3d, 0x00, /* j */
            0x7f, 0x10, 0x28, 0x44, 0x00, /* k */
            0x00, 0x41, 0x7f, 0x40, 0x00, /* l */
            0x7c, 0x04, 0x7c, 0x04, 0x78, /* m */
            0x7c, 0x08, 0x04, 0x04, 0x78, /* n */
            0x38, 0x44, 0x44, 0x44, 0x38, /* o */
            0x7c, 0x14, 0x14, 0x14, 0x08, /* p		0x70 */
            0x08, 0x14, 0x14, 0x14, 0x7c, /* q */
            0x7c, 0x08, 0x04, 0x04, 0x00, /* r */
            0x48, 0x54, 0x54, 0x54, 0x24, /* s */
            0x04, 0x04, 0x3f, 0x44, 0x44, /* t */
            0x3c, 0x40, 0x40, 0x20, 0x7c, /* u */
            0x1c, 0x20, 0x40, 0x20, 0x1c, /* v */
            0x3c, 0x40, 0x30, 0x40, 0x3c, /* w */
            0x44, 0x28, 0x10, 0x28, 0x44, /* x */
            0x0c, 0x50, 0x50, 0x50, 0x3c, /* y */
            0x44, 0x64, 0x54, 0x4c, 0x44, /* z */
            0x08, 0x36, 0x41, 0x41, 0x00, /* { */
            0x00, 0x00, 0x77, 0x00, 0x00, /* | */
            0x00, 0x41, 0x41, 0x36, 0x08, /* } */
            0x08, 0x08, 0x2a, 0x1c, 0x08  /* ~ */
        };

        private void DrawText(int x, int y, char letter, int HScale, int VScale) {
            var index = 5 * (letter - 32);


            for (var h = 0; h < 5; h++) {
                for (var hs = 0; hs < HScale; hs++) {
                    for (var v = 0; v < 8; v++) {
                        var show = (this.font[index + h] & (1 << v)) != 0;
                        for (var vs = 0; vs < VScale; vs++) {
                            this.Point(x + (h * HScale) + hs, y + (v * VScale) + vs, show);
                        }
                    }

                }
            }
            this.ClearPart(x + 5 * HScale, y, HScale, 8 * VScale);// clear the space between characters
        }

        /// <summary>
        /// Draws text at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="text">The string to draw.</param>
        public void DrawText(int x, int y, string text) => this.DrawScaledText(x, y, text, 2, 2);

        /// <summary>
        /// Draws text at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="text">The string to draw.</param>
        public void DrawSmallText(int x, int y, string text) => this.DrawScaledText(x, y, text, 1, 1);

        /// <summary>
        /// Draws text at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="text">The string to draw.</param>
        public void DrawScaledText(int x, int y, string text, int HScale, int VScale) {
            var originalX = x;
            if (HScale == 0 || VScale == 0) return;
            if (text == null) throw new ArgumentNullException("data");

            for (var i = 0; i < text.Length; i++) {
                if (text[i] >= 32) {
                    this.DrawText(x, y, text[i], HScale, VScale);
                    x += (6 * HScale);

                }
                else {
                    if (text[i] == '\n') {
                        y += (9 * VScale);
                    }
                    if (text[i] == '\r') {
                        x = originalX;
                    }
                }
            }
        }

        public void DrawNumber(int x, int y, double number) => this.DrawText(x, y, number.ToString("N2"));
        public void DrawSmallNumber(int x, int y, double number) => this.DrawSmallText(x, y, number.ToString("N2"));
        public void DrawNumber(int x, int y, long number) => this.DrawText(x, y, number.ToString("N0"));
        public void DrawSmallNumber(int x, int y, long number) => this.DrawSmallText(x, y, number.ToString("N0"));

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType() => base.GetType();
    }
}
