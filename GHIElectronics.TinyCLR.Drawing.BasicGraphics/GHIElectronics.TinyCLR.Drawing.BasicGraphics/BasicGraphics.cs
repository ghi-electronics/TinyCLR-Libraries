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

        public void FillRectangle(Brush brush, int x, int y, int width, int height) => throw new NotSupportedException();

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

        public void FillEllipse(Brush brush, int x, int y, int width, int height) => throw new NotSupportedException();
    }
}
