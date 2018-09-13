#define SUPPORT_ORIGINAL_BRAINPAD

using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drawing;
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
        private Graphics screen;
        private DisplayController display;

        public Picture CreatePicture(int width, int height, byte[] data) => this.CreateScaledPicture(width, height, data, 1);
        public Picture CreateScaledPicture(int width, int height, byte[] data, int scale) => data != null ? new Picture(width, height, data, scale) : throw new Exception("Incorrect picture data size");

#if SUPPORT_ORIGINAL_BRAINPAD
        private SpiDevice spi;
        private GpioPin controlPin;
        private GpioPin resetPin;
        private GpioPin backlightPin;
#endif

        public int Width { get; } = 128;

        public int Height { get; } = 64;

        private Font font = new Font("GHIMono8x5", 8);
#if SUPPORT_ORIGINAL_BRAINPAD
        private ST7735Controller st7735;
#endif

        private sealed class ST7735DrawTarget : BufferDrawTargetRgb444 {
            private readonly DisplayController parent;

            public ST7735DrawTarget(DisplayController parent) : base(parent.ActiveConfiguration.Width, parent.ActiveConfiguration.Height) => this.parent = parent;

            public override void Flush() => this.parent.DrawBuffer(0, 0, this.Width, this.Height, this.buffer, 0);
        }

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
                    this.st7735.Enable();

                    this.st7735.SetDrawWindow(0, 0, 160, 128);
                    this.st7735.SendDrawCommand();
                    var buf = new byte[1024];
                    for (var i = 0; i < 30; i++)
                        this.st7735.SendData(buf);

                    this.st7735.SetDrawWindow((160 - this.Width) / 2, (128 - this.Height) / 2, this.Width, this.Height);

                    this.display = DisplayController.FromProvider(this.st7735);
                    this.display.SetConfiguration(new SpiDisplayControllerSettings { Width = 128, Height = 64, DataFormat = DisplayDataFormat.Rgb444 });
                    this.display.Enable();

                    this.screen = Graphics.FromHdc(GraphicsManager.RegisterDrawTarget(new ST7735DrawTarget(this.display)));

                    break;
#endif
                case BoardType.BP2:
                    this.i2cDevice = I2cController.FromName(Board.BoardType == BoardType.BP2 ? FEZCLR.I2cBus.I2c1 : G30.I2cBus.I2c1).GetDevice(SSD1306Controller.GetConnectionSettings());
                    this.ssd1306 = new SSD1306Controller(this.i2cDevice);

                    this.display = DisplayController.FromProvider(this.st7735 /*this.ssd1306*/);
                    this.display.SetConfiguration(new SpiDisplayControllerSettings { Width = 128, Height = 80, DataFormat = DisplayDataFormat.Rgb444 });
                    this.display.Enable();

                    this.screen = Graphics.FromHdc(GraphicsManager.RegisterDrawTarget(new ST7735DrawTarget(this.display)));

                    break;
            }
            this.Clear();
            this.RefreshScreen();
        }

        public void RefreshScreen() => this.screen.Flush();

        private Pen set = new Pen(Color.Blue);
        private Pen clear = new Pen(Color.Black);
        private Brush brush = new SolidBrush(Color.Blue);

        private void Point(int x, int y, bool set) => this.screen.DrawRectangle(set ? this.set : this.clear, x, y, 1, 1);

        public void DrawPoint(int x, int y) => this.Point(x, y, true);

        public void ClearPoint(int x, int y) => this.Point(x, y, false);

        public void Clear() => this.screen.Clear(Color.Black);

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

        public void DrawLine(int x0, int y0, int x1, int y1) => this.screen.DrawLine(this.set, x0, y0, x1, y1);

        public void DrawCircle(int x, int y, int r) => this.screen.DrawEllipse(this.set, x - r, y - r, r * 2  + 1, r * 2 + 1);

        public void DrawRectangle(int x, int y, int width, int height) => this.screen.DrawRectangle(this.set, x, y, width, height);

        public void DrawFilledRectangle(int x, int y, int width, int height) {
            for (var lx = x; lx < width + x; lx++)
                for (var ly = y; ly < height + y; ly++)
                    this.Point(lx, ly, true);
        }

        public void DrawText(int x, int y, string text) => this.DrawScaledText(x, y, text, 2, 2);
        public void DrawSmallText(int x, int y, string text) => this.DrawScaledText(x, y, text, 1, 1);

        public void DrawScaledText(int x, int y, string text, int HScale, int VScale) => this.screen.DrawString(text, HScale == 1 && VScale == HScale ? this.font : new Font("GHIMono8x5", HScale * 8), this.brush, x, y);

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
