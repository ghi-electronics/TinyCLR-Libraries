////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GHIElectronics.TinyCLR.UI.Media {
    public sealed class SolidColorBrush : Brush {
        public Color Color;

        public SolidColorBrush(Color color) => this.Color = color;

        internal override void RenderRectangle(Bitmap bmp, Pen pen, int x, int y, int width, int height) {
            var outlineColor = (pen != null) ? pen.Color : Colors.Transparent;
            var outlineThickness = (ushort)0;

            if (pen != null)
                outlineThickness = pen.Thickness;

            bmp.DrawRectangle(outlineColor, outlineThickness, x, y, width, height, 0, 0,
                                      this.Color, 0, 0, this.Color, 0, 0, this.Opacity);
        }

        internal override void RenderEllipse(Bitmap bmp, Pen pen, int x, int y, int xRadius, int yRadius) {
            var outlineColor = (pen != null) ? pen.Color : Colors.Transparent;
            var outlineThickness = (ushort)0;

            if (pen != null)
                outlineThickness = pen.Thickness;

            bmp.DrawEllipse(outlineColor, outlineThickness, x, y, xRadius, yRadius,
                                      this.Color, 0, 0, this.Color, 0, 0, this.Opacity);
        }

        class LineSegment {
            public int x1;
            public int y1;
            public int x2;
            public int y2;

            /// <summary>
            /// We will use Bresenham alg for line calc.
            /// </summary>
            public int dx;
            public int dy;
            public int cx;
            public int e;
            public bool highSlope;
            public int ix;
            public int processedPts;
        }

        private void Swap(ref int a, ref int b) {
            var t = a;
            a = b;
            b = t;
        }

        private int Abs(int a) {
            if (a < 0) return -a;

            return a;
        }

        const int c_YMinBit = 0x40000000;
        const int c_XValueMask = 0x3FFFFFFF;

        /// <summary>
        /// Basic algorithm uses scan lines to fill the polygon.
        /// No multiplication or division is needed, neither is floating point calculation.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="outline"></param>
        /// <param name="pts"></param>
        internal override void RenderPolygon(Bitmap bmp, Pen outline, int[] pts) {
            var n = pts.Length / 2; /// This is number of points and number of lines (closed polygon).

            /// Polygon to fill must have at least 3 points.
            if (n < 3)
                return;

            /// Nothing to do if this is a transparent brush.
            if (this.Opacity == Bitmap.OpacityTransparent)
                return;

            var i = 0;
            var y = 0;
            var yLow = 0;
            var yHigh = 0;

            var lines = new LineSegment[n];

            var xPoints = new int[n];

            /// Initialize line segments.
            for (i = 0; i < n; i++) {
                lines[i] = new LineSegment {
                    processedPts = 0
                };
                xPoints[i] = new int();

                lines[i].x1 = pts[i * 2];
                lines[i].y1 = pts[i * 2 + 1];
                if (i < (n - 1)) {
                    lines[i].x2 = pts[(i + 1) * 2];
                    lines[i].y2 = pts[(i + 1) * 2 + 1];
                }
                else {
                    lines[i].x2 = pts[0];
                    lines[i].y2 = pts[1];
                }

                /// Reverse the points to make sure y1 <= y2 always.
                if (lines[i].y2 < lines[i].y1) {
                    Swap(ref lines[i].y2, ref lines[i].y1);
                    Swap(ref lines[i].x2, ref lines[i].x1);
                }

                /// Calculate slopes and increments.
                lines[i].dx = Abs(lines[i].x2 - lines[i].x1);
                lines[i].dy = Abs(lines[i].y2 - lines[i].y1);
                lines[i].cx = lines[i].x1;
                lines[i].e = 0;

                if (lines[i].dx < lines[i].dy) {
                    /// Angle is 45 degree or more. So y increases faster.
                    lines[i].highSlope = true;
                }
                else {
                    lines[i].highSlope = false;
                }

                /// Actual increment direction.
                if (lines[i].x2 > lines[i].x1) lines[i].ix = 1;
                else lines[i].ix = -1;

                if (i == 0) {
                    yLow = lines[i].y1;
                    yHigh = lines[i].y2;
                }
                else {
                    if (lines[i].y1 < yLow) yLow = lines[i].y1;
                    if (lines[i].y2 > yHigh) yHigh = lines[i].y2;
                }
            }

            /// Fill via scan lines between yLow and yHigh.
            for (y = yLow; y <= yHigh; y++) {
                var j = 0;
                for (i = 0; i < n; i++) {
                    xPoints[i] = int.MaxValue;
                }

                /// Find intersection points for given y.
                for (i = 0; i < n; i++) {
                    if (y < lines[i].y1) continue;
                    if (y > lines[i].y2) continue;

                    lines[i].processedPts++;

                    if (lines[i].dy != 0) {
                        if (lines[i].highSlope) {
                            /// For this y find the x, which either the same pixel
                            /// in last iteration or next one.
                            if (y == lines[i].y1) lines[i].cx = lines[i].x1;
                            else if (y == lines[i].y2) lines[i].cx = lines[i].x2;
                            else {
                                lines[i].e += lines[i].dx;
                                if ((lines[i].e << 1) >= lines[i].dy) {
                                    lines[i].cx += lines[i].ix;
                                    lines[i].e -= lines[i].dy;
                                }
                            }
                        }
                        else {
                            /// In this case for every y pixel inc, x increases more than 1 pixel.
                            if (y == lines[i].y1) lines[i].cx = lines[i].x1;
                            else if (y == lines[i].y2) lines[i].cx = lines[i].x2;
                            else {
                                for (; ; )
                                {
                                    lines[i].cx += lines[i].ix;

                                    lines[i].e += lines[i].dy;
                                    if ((lines[i].e << 1) >= lines[i].dx) {
                                        lines[i].e -= lines[i].dx;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    /// Insertion sort, do not insert back to back duplicates
                    int x1;
                    bool x1YMin;
                    var x2 = int.MaxValue;
                    var x2YMin = false;

                    ///
                    /// add both x endpoints if the line is horizontal
                    ///
                    if (lines[i].dy == 0) {
                        x1 = lines[i].x1;
                        x1YMin = true;

                        x2 = lines[i].x2;
                        x2YMin = true;

                        if (x2 < 0) x2 = 0;
                    }
                    else {
                        x1 = lines[i].cx;
                        x1YMin = lines[i].processedPts == 1;
                    }

                    // We don't need to process negative values of x
                    if (x1 < 0) x1 = 0;


                    var idx1 = -1;
                    var idx2 = x2 == int.MaxValue ? x2 : -1;
                    var offset = 0;

                    ///
                    /// First we search for the indexes of x1 and x2 (if neccessary) and then we will insert the
                    /// items, by shifting the elements in the array
                    for (j = 0; j < n; j++) {
                        var ix = (xPoints[j] & c_XValueMask);
                        var isYMin = (xPoints[j] & c_YMinBit) != 0;

                        if (idx1 == -1) {
                            ///
                            /// Only add duplicate x values if the intersection produces up (^) or down (v) angles
                            /// as opposed to right (<) or left (>) angles.
                            ///
                            if (ix == x1 && isYMin != x1YMin) { idx1 = int.MaxValue; }
                            else if (ix > x1) { idx1 = j + offset; offset++; }
                        }

                        if (idx2 == -1) {
                            ///
                            /// Only add duplicate x values if the intersection produces up (^) or down (v) angles
                            /// as opposed to right (<) or left (>) angles.
                            ///
                            if (ix == x2 && isYMin != x2YMin) { idx2 = int.MaxValue; }
                            else if (ix > x2) { idx2 = j + offset; offset++; }
                        }

                        // don't break until we have found both indexes and then end of the list
                        if (idx1 != -1 && idx2 != -1 && xPoints[j] == int.MaxValue) break;
                    }

                    var idxMin = (idx1 < idx2 ? idx1 : idx2);

                    // Because we may have two values to insert, the index to the next item to be shifted
                    // can either be n-1 or n-2.
                    offset = (idx2 == int.MaxValue || idx1 == int.MaxValue ? 1 : 2);

                    // j is already one element past the last valid data item, so increase it by one if we are
                    // inserting two elements
                    j += (offset - 1);

                    // Make sure the index does not overflow
                    if (j >= xPoints.Length) j = xPoints.Length - 1;

                    for (; j >= idxMin; j--) {
                        if (j == idx1) { xPoints[j] = x1YMin ? x1 | c_YMinBit : x1; offset--; continue; }
                        if (j == idx2) { xPoints[j] = x2YMin ? x2 | c_YMinBit : x2; offset--; continue; }

                        if (j < offset) break;

                        xPoints[j] = xPoints[j - offset];
                    }
                }

                /// Finally draw the line segments to fill.
                for (i = 0; i < xPoints.Length - 1; i += 2) {
                    var ix1 = (xPoints[i] & c_XValueMask);
                    var ix2 = (xPoints[i + 1] & c_XValueMask);

                    if ((ix1 == c_XValueMask) || (ix2 == c_XValueMask)) break;

                    bmp.DrawLine(this.Color, 1, ix1, y, ix2, y);
                }
            }
        }
    }
}


