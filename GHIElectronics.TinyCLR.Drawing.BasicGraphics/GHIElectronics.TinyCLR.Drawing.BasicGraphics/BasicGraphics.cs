using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Drawing.BasicGraphics {
    public abstract class BasicGraphics {
        public abstract void DrawPixel(int x, int y, Pen pen);

        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2) {
            var xLength = x2 - x1;
            var yLength = y2 - y1;
            int stepx, stepy;

            if (yLength < 0) { yLength = -yLength; stepy = -1; } else { stepy = 1; }
            if (xLength < 0) { xLength = -xLength; stepx = -1; } else { stepx = 1; }
            yLength <<= 1;                                  // yLength is now 2 * yLength
            xLength <<= 1;                                  // xLength is now 2 * xLength

            this.DrawPixel(x1, y1, pen);
            if (xLength > yLength) {
                var fraction = yLength - (xLength >> 1);    // same as 2 * yLength - xLength
                while (x1 != x2) {
                    if (fraction >= 0) {
                        y1 += stepy;
                        fraction -= xLength;                // same as fraction -= 2 * xLength
                    }
                    x1 += stepx;
                    fraction += yLength;                    // same as fraction -= 2 * yLength
                    this.DrawPixel(x1, y1, pen);
                }
            }
            else {
                var fraction = xLength - (yLength >> 1);
                while (y1 != y2) {
                    if (fraction >= 0) {
                        x1 += stepx;
                        fraction -= yLength;
                    }
                    y1 += stepy;
                    fraction += xLength;
                    this.DrawPixel(x1, y1, pen);
                }
            }
        }

        public void DrawRectangle(Pen pen, int x, int y, int width, int height) {
            if (width < 0) return;
            if (height < 0) return;

            for (var i = x; i < x + width; i++) {
                this.DrawPixel(i, y, pen);
                this.DrawPixel(i, y + height - 1, pen);
            }

            for (var i = y; i < y + height; i++) {
                this.DrawPixel(x, i, pen);
                this.DrawPixel(x + width - 1, i, pen);
            }
        }

        public void FillRectangle(Brush brush, int x, int y, int width, int height) => throw new NotSupportedException("FillRectangle currently not supported.");

        /// <summary>
        /// Draws an ellipse.
        /// </summary>
        /// <param name="pen">The Pen object that determines the color, width, and style of the ellipse.</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
        /// <param name="width">The Width of the bounding rectangle that defines the ellipse.</param>
        /// <param name="height">Height of the bounding rectangle that defines the ellipse.</param>
        public void DrawEllipse(Pen pen, int x, int y, int width, int height) {
            if (width != height)
                throw new Exception("width and height must be equal!");

            var radius = width / 2;
            if (radius <= 0) return;

            var centerX = x + radius;
            var centerY = y + radius;

            var f = 1 - radius;
            var ddFX = 1;
            var ddFY = -2 * radius;
            var dX = 0;
            var dY = radius;

            this.DrawPixel(centerX, centerY + radius, pen);
            this.DrawPixel(centerX, centerY - radius, pen);
            this.DrawPixel(centerX + radius, centerY, pen);
            this.DrawPixel(centerX - radius, centerY, pen);

            while (dX < dY) {
                if (f >= 0) {
                    dY--;
                    ddFY += 2;
                    f += ddFY;
                }

                dX++;
                ddFX += 2;
                f += ddFX;

                this.DrawPixel(centerX + dX, centerY + dY, pen);
                this.DrawPixel(centerX - dX, centerY + dY, pen);
                this.DrawPixel(centerX + dX, centerY - dY, pen);
                this.DrawPixel(centerX - dX, centerY - dY, pen);

                this.DrawPixel(centerX + dY, centerY + dX, pen);
                this.DrawPixel(centerX - dY, centerY + dX, pen);
                this.DrawPixel(centerX + dY, centerY - dX, pen);
                this.DrawPixel(centerX - dY, centerY - dX, pen);
            }
        }

        public void FillEllipse(Brush brush, int x, int y, int width, int height) => throw new NotSupportedException("FillEllipse currently not supported.");

        public void DrawString(string s, Font font, Brush brush, float x, float y) {
            var bgFontWidth = 5;
            var bgFontHeight = 8;
            var layoutRectangle = new RectangleF(x, y, s.Length * (bgFontWidth + 1), bgFontHeight);
            this.DrawString(s, font, brush, layoutRectangle);
        }

        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle) => this.DrawString(s, font, brush, layoutRectangle, null);

        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format) {
            Pen fontColor;

            if (font != null || format != null)
                throw new NotSupportedException("Neither Font nor StringFormat is supported at this time.");

            if (brush is SolidBrush fontBrush) {
                fontColor = new Pen(fontBrush.Color);
            }
            else {
                throw new NotSupportedException();
            }

            var originalX = (int)layoutRectangle.X;

            var HScale = 1;
            var VScale = 1;
            //if (HScale < 1 || VScale < 1) return;

            if (s == null) throw new ArgumentNullException("No string data found.");

            for (var i = 0; i < s.Length; i++) {
                if (s[i] >= 32) {
                    this.DrawLetter((int)layoutRectangle.X, (int)layoutRectangle.Y, s[i], fontColor, HScale, VScale);
                    layoutRectangle.X += (6 * HScale);
                }
                else {
                    if (s[i] == '\n') {
                        layoutRectangle.Y += (9 * VScale);
                        layoutRectangle.X = originalX;
                    }
                    if (s[i] == '\r')
                        layoutRectangle.X = originalX;
                }
            }
        }

        private void DrawLetter(int x, int y, char letter, Pen pen, int HScale, int VScale) {
            var index = 5 * (letter - 32);

            for (var horizontalFontSize = 0; horizontalFontSize < 5; horizontalFontSize++) {
                for (var hs = 0; hs < HScale; hs++) {
                    for (var verticleFoneSize = 0; verticleFoneSize < 8; verticleFoneSize++) {
                        for (var vs = 0; vs < VScale; vs++) {
                            if ((this.font[index + horizontalFontSize] & (1 << verticleFoneSize)) != 0)
                                this.DrawPixel(x + (horizontalFontSize * HScale) + hs, y + (verticleFoneSize * VScale) + vs, pen);
                        }
                    }
                }
            }
        }

        readonly byte[] font = new byte[95 * 5] {
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
            0x7c, 0x08, 0x04, 0x04, 0x08, /* r */
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

        //Draws a portion of an image at a specified location.
        public void DrawImage(Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit) => throw new NotSupportedException("Not supported at this time.");

        //Draws the specified Image at the specified location and with the specified size.
        public void DrawImage(Image image, int x, int y, int width, int height) => throw new NotSupportedException("Not supported at this time.");

        //Draws the specified image, using its original physical size, at the location specified by a coordinate pair.
        public void DrawImage(Image image, int x, int y) => throw new NotSupportedException("Not supported at this time.");

        //Draws the specified portion of the specified Image at the specified location and with the specified size.
        public void DrawImage(Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit) => throw new NotSupportedException("Not supported at this time.");

    }
}
