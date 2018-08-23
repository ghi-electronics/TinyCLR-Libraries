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
    }
}
