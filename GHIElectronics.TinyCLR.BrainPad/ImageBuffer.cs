using System;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class Image {
        private int _height;
        private int _width;
        private byte[] _data;
        public int Height => this._height;
        public int Width => this._width;
        public byte[] Data => this._data;

        public Image(int width, int height, byte[] data) {
            if (width * height != data.Length)
                throw new Exception("Incorrect image data size");
            this._height = height;
            this._width = width;
            this._data = data;
        }
    }
    public enum Transform {
        FlipHorizontal,
        FlipVertical,
        Rotate90,
        Rotate180,
        Rotate270,
        None
    }
}

namespace GHIElectronics.TinyCLR.BrainPad.Internal {
    public class ImageBuffer {
        private readonly Display display;

        /// <summary>
        /// The width of the display in pixels.
        /// </summary>
        public int Width = 128;

        /// <summary>
        /// The height of the display in pixels.
        /// </summary>
        public int Height = 64;

        private byte[] vram = new byte[(128 * 64 / 8) + 1];


        //private byte[] buffer1;
        //private byte[] buffer2;
        //private byte[] buffer4;

        public ImageBuffer(Display display) => this.display = display;


        public void ShowOnScreen() =>

            //Display.Render(vram);
            this.display.i2cDevice.Write(this.vram);

        private void Point(int x, int y, bool set) {
            if (x < 0 || x > 127)
                return;
            if (y < 0 || y > 63)
                return;
            var index = (x + (y / 8) * 128) + 1;

            if (set)
                this.vram[index] |= (byte)(1 << (y % 8));
            else
                this.vram[index] &= (byte)(~(1 << (y % 8)));
        }

        /// <summary>
        /// Draws a pixel.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="color">The color to draw.</param>
        public void DrawPoint(int x, int y) => Point(x, y, true);

        /// <summary>
        /// Clears a pixel.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="color">The color to draw.</param>
        public void ClearPoint(int x, int y) => Point(x, y, false);

        /// <summary>
        /// Clears the Display.
        /// </summary>
        public void Clear() {

            Array.Clear(this.vram, 0, this.vram.Length);

            this.vram[0] = 0x40;
        }
        public void Clear(int x, int y, int width, int height) {
            if (x == 0 && y == 0 && width == 128 && height == 64) Clear();
            for (var lx = x; lx < width + x; lx++)
                for (var ly = y; ly < height + y; ly++)
                    Point(lx, ly, false);
        }

        /// <summary>
        /// Draws an image at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="image">The image to draw.</param>
        public void DrawImage(int x, int y, Image image, Transform mirror = Transform.None) {
            if (image == null) throw new ArgumentNullException("image");

            for (var xd = 0; xd < image.Width; xd++) {
                for (var yd = 0; yd < image.Height; yd++) {
                    switch (mirror) {
                        case Transform.None:
                            Point(x + xd, y + yd, image.Data[image.Width * yd + xd] == 1);
                            break;
                        case Transform.FlipHorizontal:
                            Point(x + image.Width - xd, y + yd, image.Data[image.Width * yd + xd] == 1);
                            break;
                        case Transform.FlipVertical:
                            Point(x + xd, y + image.Height - yd, image.Data[image.Width * yd + xd] == 1);
                            break;
                        case Transform.Rotate90:
                            Point(x + image.Width - yd, y + xd, image.Data[image.Width * yd + xd] == 1);
                            break;
                        case Transform.Rotate180:
                            Point(x + image.Width - xd, y + image.Height - yd, image.Data[image.Width * yd + xd] == 1);
                            break;
                        case Transform.Rotate270:
                            Point(x + yd, y + image.Height - xd, image.Data[image.Width * yd + xd] == 1);
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

            Point(x0, y0, true);
            if (dx > dy) {
                var fraction = dy - (dx >> 1);                         // same as 2*dy - dx
                while (x0 != x1) {
                    if (fraction >= 0) {
                        y0 += stepy;
                        fraction -= dx;                                // same as fraction -= 2*dx
                    }
                    x0 += stepx;
                    fraction += dy;                                    // same as fraction -= 2*dy
                    Point(x0, y0, true);
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
                    Point(x0, y0, true);
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

            DrawPoint(x, y + r);
            DrawPoint(x, y - r);
            DrawPoint(x + r, y);
            DrawPoint(x - r, y);

            while (dX < dY) {
                if (f >= 0) {
                    dY--;
                    ddFY += 2;
                    f += ddFY;
                }

                dX++;
                ddFX += 2;
                f += ddFX;

                DrawPoint(x + dX, y + dY);
                DrawPoint(x - dX, y + dY);
                DrawPoint(x + dX, y - dY);
                DrawPoint(x - dX, y - dY);

                DrawPoint(x + dY, y + dX);
                DrawPoint(x - dY, y + dX);
                DrawPoint(x + dY, y - dX);
                DrawPoint(x - dY, y - dX);
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
                DrawPoint(i, y);
                DrawPoint(i, y + height - 1);
            }

            for (var i = y; i < y + height; i++) {
                DrawPoint(x, i);
                DrawPoint(x + width - 1, i);
            }
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
                            Point(x + (h * HScale) + hs, y + (v * VScale) + vs, show);
                        }
                    }

                }
            }
            Clear(x + 5 * HScale, y, HScale, 8 * VScale);// clear the space between characters
        }
        /// <summary>
        /// Draws text at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="text">The string to draw.</param>
        public void DrawText(int x, int y, string text) => DrawText(x, y, text, 2, 2);
        /// <summary>
        /// Draws text at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="text">The string to draw.</param>
        public void DrawSmallText(int x, int y, string text) => DrawText(x, y, text, 1, 1);
        /// <summary>
        /// Draws text at the given location.
        /// </summary>
        /// <param name="x">The x coordinate to draw at.</param>
        /// <param name="y">The y coordinate to draw at.</param>
        /// <param name="text">The string to draw.</param>
        public void DrawText(int x, int y, string text, int HScale, int VScale) {
            var originalX = x;
            if (HScale == 0 || VScale == 0) return;
            if (text == null) throw new ArgumentNullException("data");

            for (var i = 0; i < text.Length; i++) {
                if (text[i] >= 32) {
                    DrawText(x, y, text[i], HScale, VScale);
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
    }
}
