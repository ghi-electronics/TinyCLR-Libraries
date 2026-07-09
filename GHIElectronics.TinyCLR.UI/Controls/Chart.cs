using System;
using System.Collections;
using System.Drawing;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>
    /// Simple line / bar chart. The rendered surface is cached and only rebuilt
    /// when <see cref="Refresh"/> is called or the control's render size
    /// changes — so calling Invalidate on the parent each frame does not redo
    /// the chart math.
    /// </summary>
    public class Chart : Image, IDisposable {

        /// <summary>How the chart series is drawn.</summary>
        public enum ChartMode {
            /// <summary>Draws the series as a connected line.</summary>
            LineMode,
            /// <summary>Draws the series as vertical bars.</summary>
            RectangleMode,
            /// <summary>Draws the series as a line with the area beneath it filled (uses <see cref="AreaColor"/>).</summary>
            AreaMode
        }

        /// <summary>A single data point in the chart.</summary>
        public class DataItem {
            /// <summary>The point's value.</summary>
            public double Value { get; set; }
            /// <summary>The point's axis label.</summary>
            public string Name { get; set; }
        }

        // Named "ChartPoint" rather than "Point" so it doesn't visually collide
        // with System.Drawing.Point inside this file (both are in scope via
        // `using System.Drawing`).
        /// <summary>An x/y pixel coordinate used by the chart.</summary>
        public class ChartPoint {
            /// <summary>Creates a point at the origin.</summary>
            public ChartPoint() { }
            /// <summary>Creates a point at the given coordinates.</summary>
            public ChartPoint(int ax, int ay) { this.X = ax; this.Y = ay; }
            /// <summary>The X coordinate in pixels.</summary>
            public int X { get; set; }
            /// <summary>The Y coordinate in pixels.</summary>
            public int Y { get; set; }
        }

        /// <summary>Pairs a plotted point with its source value.</summary>
        public class ChartPointModel {
            /// <summary>The point's pixel location.</summary>
            public ChartPoint Point { get; set; }
            /// <summary>The value the point represents.</summary>
            public double Value { get; set; }
        }

        /// <summary>Spacing factor between labeled divisions on the X axis.</summary>
        public int DivisionAxisX { get; set; } = 1;
        /// <summary>Spacing factor between labeled divisions on the Y axis.</summary>
        public int DivisionAxisY { get; set; } = 1;

        /// <summary>Font used for chart text.</summary>
        public Font Font { get; set; }

        // Public styling — Media types, like every other control. Internally
        // converted to System.Drawing at rebuild time.
        /// <summary>Pen used to draw the axes.</summary>
        public Media.Pen AxisPen { get; set; } = new Media.Pen(Colors.Black, 1);
        /// <summary>Pen used to draw the chart series.</summary>
        public Media.Pen ChartPen { get; set; } = new Media.Pen(Colors.Green, 1);
        /// <summary>Brush used to fill the data point markers.</summary>
        public Media.SolidColorBrush EllipseColor { get; set; } = new Media.SolidColorBrush(Colors.Black);
        /// <summary>Brush used to draw the axis division markers.</summary>
        public Media.SolidColorBrush DivisionColor { get; set; } = new Media.SolidColorBrush(Colors.Black);
        /// <summary>Brush used to draw chart text.</summary>
        public Media.SolidColorBrush TextColor { get; set; } = new Media.SolidColorBrush(Colors.Black);
        /// <summary>Brush used to fill the chart background.</summary>
        public Media.SolidColorBrush BackgroundColor { get; set; } = new Media.SolidColorBrush(Colors.White);
        /// <summary>Fill under the series when <see cref="Mode"/> is <see cref="ChartMode.AreaMode"/>.</summary>
        public Media.SolidColorBrush AreaColor { get; set; } = new Media.SolidColorBrush(Media.Color.FromRgb(0xBB, 0xDE, 0xFB));

        /// <summary>Radius in pixels of the data point markers.</summary>
        public int RadiusPoint { get; set; } = 10;
        /// <summary>Title text shown above the chart.</summary>
        public string ChartTitle { get; set; } = "Chart1";
        /// <summary>The data points to plot.</summary>
        public ArrayList Items { get; set; }
        /// <summary>Whether the series is drawn as a line or as bars.</summary>
        public ChartMode Mode { get; set; } = ChartMode.LineMode;

        private int paddingLeft = 50;
        private int margin = 50;
        private ChartPoint pStart;
        private ChartPoint pEnd;

        const int SCALE_FROM_WIDTH = 800;
        const int SCALE_FROM_HEIGHT = 480;

        // Cached rendered surface. Reused across paints until Refresh() or a
        // size change marks it dirty.
        private System.Drawing.Bitmap _cachedBmp;
        private BitmapImage _cachedImage;
        private int _cachedW;
        private int _cachedH;
        private bool _dirty = true;

        /// <summary>Creates a new Chart with the given pixel size.</summary>
        public Chart(int width, int height) {
            this.Width = width;
            this.Height = height;

            this.margin = (int)Scale(this.margin, SCALE_FROM_HEIGHT, height);
            this.paddingLeft = (int)Scale(this.paddingLeft, SCALE_FROM_WIDTH, width);

            this.pStart = new ChartPoint(this.margin + this.paddingLeft, height - this.margin);
            this.pEnd = new ChartPoint(width - this.margin, this.margin + (int)Scale(100, SCALE_FROM_HEIGHT, height));
        }

        /// <summary>
        /// Marks the cached chart surface stale. Call after mutating <see cref="Items"/>
        /// or any styling property to force a re-render on the next paint.
        /// </summary>
        public void Refresh() {
            this._dirty = true;
            this.Invalidate();
        }

        /// <summary>Draws the cached chart surface.</summary>
        public override void OnRender(DrawingContext dc) {
            if (this.Font == null) return;
            if (this.Items == null || this.Items.Count == 0) return;

            var w = this.ActualWidth;
            var h = this.ActualHeight;
            if (w <= 0 || h <= 0) return;

            this.EnsureCache(w, h);
            if (this._cachedImage == null) return;

            dc.DrawImage(this._cachedImage, 0, 0);
        }

        // Rebuilds the cached bitmap if dirty or if the render size changed
        // since the last paint. Disposes the previous cache before replacing.
        private void EnsureCache(int w, int h) {
            if (!this._dirty && this._cachedImage != null && this._cachedW == w && this._cachedH == h) {
                return;
            }

            this.DisposeCache();

            this._cachedBmp = this.RenderChart();
            this._cachedImage = BitmapImage.FromGraphics(Graphics.FromImage(this._cachedBmp));
            this._cachedW = w;
            this._cachedH = h;
            this._dirty = false;
        }

        private void DisposeCache() {
            this._cachedImage?.graphics?.Dispose();
            this._cachedImage = null;
            this._cachedBmp?.Dispose();
            this._cachedBmp = null;
        }

        private System.Drawing.Bitmap RenderChart() => this.Mode switch {
            ChartMode.LineMode => this.GetLineChart(),
            ChartMode.AreaMode => this.GetLineChart(fillArea: true),
            ChartMode.RectangleMode => this.GetRectangleChart(),
            _ => null,
        };

        double GetMax(ArrayList data) {
            double max = 0;
            foreach (DataItem item in data) {
                if (item.Value > max) max = item.Value;
            }
            return max;
        }

        double GetMin(ArrayList data) {
            var min = double.MaxValue;
            foreach (DataItem item in data) {
                if (item.Value < min) min = item.Value;
            }
            return min;
        }

        // --- conversion helpers ----------------------------------------------
        // The actual surface is a System.Drawing.Bitmap so we still need
        // System.Drawing.Pen/Brush instances for the heavy drawing calls.
        // These are allocated once per rebuild (not per paint).

        private static System.Drawing.Color ToSd(Media.Color c) =>
            System.Drawing.Color.FromArgb(c.R, c.G, c.B);

        private static System.Drawing.Pen ToSdPen(Media.Pen p) =>
            new System.Drawing.Pen(ToSd(p.Color), p.Thickness);

        private static System.Drawing.SolidBrush ToSdBrush(Media.SolidColorBrush b) =>
            new System.Drawing.SolidBrush(ToSd(b.Color));

        private System.Drawing.Bitmap GetLineChart(bool fillArea = false) {
            var bitmap = new System.Drawing.Bitmap(this.Width, this.Height);

            using var graph = Graphics.FromImage(bitmap);
            using var axisPen = ToSdPen(this.AxisPen);
            using var chartPen = ToSdPen(this.ChartPen);
            using var bgBrush = ToSdBrush(this.BackgroundColor);
            using var ellipseBrush = ToSdBrush(this.EllipseColor);
            using var divisionBrush = ToSdBrush(this.DivisionColor);
            using var textBrush = ToSdBrush(this.TextColor);

            graph.FillRectangle(bgBrush, 0, 0, this.Width, this.Height);

            graph.DrawString(this.ChartTitle, this.Font, textBrush,
                this.Width / 2 - (this.ChartTitle.Length / 2 * (int)Scale(18, SCALE_FROM_WIDTH, this.Width)),
                (int)Scale(30, SCALE_FROM_HEIGHT, this.Height));

            graph.DrawLine(axisPen, this.margin, this.margin + (int)Scale(100, SCALE_FROM_HEIGHT, this.Height), this.margin, this.Height - this.margin);
            graph.DrawLine(axisPen, this.margin, this.Height - this.margin, this.Width - this.margin, this.Height - this.margin);

            var maxValue = Math.Ceiling(this.GetMax(this.Items));
            var minValue = (int)this.GetMin(this.Items);
            var countValue = this.Items.Count;

            var chartWidth = Math.Abs(this.pEnd.X - this.pStart.X - (int)Scale(50, SCALE_FROM_WIDTH, this.Width));
            var chartHeight = Math.Abs(this.pEnd.Y - (this.pStart.Y - (int)Scale(50, SCALE_FROM_HEIGHT, this.Height)));

            // Guard against flat datasets and empty collections.
            var range = maxValue - minValue;
            var divisionHeight = (range != 0) ? chartHeight / range : 0;
            var divisionWidth = (countValue > 0) ? chartWidth / countValue : 0;

            var startDivX = this.pStart.X + divisionWidth;
            foreach (DataItem xx in this.Items) {
                graph.FillEllipse(divisionBrush, startDivX - this.RadiusPoint / 2, this.pStart.Y - this.RadiusPoint / 2,
                    this.RadiusPoint, this.RadiusPoint);
                graph.DrawString(xx.Name, this.Font, textBrush,
                    startDivX - (int)Scale(7, SCALE_FROM_WIDTH, this.Width),
                    this.pStart.Y + this.margin / 2 - (int)Scale(7, SCALE_FROM_HEIGHT, this.Height));
                startDivX += divisionWidth * this.DivisionAxisX;
            }

            var startDivY = this.pStart.Y - (int)Scale(25, SCALE_FROM_HEIGHT, this.Height);
            for (var i = minValue; i <= maxValue; i += this.DivisionAxisY) {
                graph.FillEllipse(divisionBrush, this.pStart.X - this.paddingLeft - this.RadiusPoint / 2, startDivY - this.RadiusPoint / 2,
                    this.RadiusPoint, this.RadiusPoint);
                graph.DrawString(i.ToString(), this.Font, textBrush,
                    this.pStart.X - this.paddingLeft + this.margin / 2,
                    startDivY - (int)Scale(10, SCALE_FROM_HEIGHT, this.Height));
                startDivY -= (int)(divisionHeight * this.DivisionAxisY);
            }

            // Pass 1: compute all plotted points (no drawing yet, so an area fill can go UNDER the line).
            var ellipsePoints = new ArrayList(); // ChartPointModel
            for (var i = 0; i < this.Items.Count; i++) {
                var item = (DataItem)this.Items[i];
                var pixelYValue = divisionHeight * item.Value -
                    divisionHeight * minValue + (int)Scale(25, SCALE_FROM_HEIGHT, this.Height);
                var pixelXValue = divisionWidth * (i + 1);

                ellipsePoints.Add(new ChartPointModel {
                    Point = new ChartPoint(this.pStart.X + pixelXValue, (int)(this.pStart.Y - pixelYValue)),
                    Value = item.Value,
                });
            }

            // Optional area fill (AreaMode): fill from the baseline up to the interpolated line, one 1px strip
            // per column. Done at rebuild time (cached), so it costs nothing per frame.
            if (fillArea && ellipsePoints.Count > 1) {
                using var areaBrush = ToSdBrush(this.AreaColor);
                var baseline = this.pStart.Y;
                for (var s = 0; s < ellipsePoints.Count - 1; s++) {
                    var a = ((ChartPointModel)ellipsePoints[s]).Point;
                    var b = ((ChartPointModel)ellipsePoints[s + 1]).Point;
                    var dx = Math.Max(1, b.X - a.X);
                    for (var px = a.X; px <= b.X; px++) {
                        var y = a.Y + (b.Y - a.Y) * (px - a.X) / dx;
                        if (baseline > y) graph.FillRectangle(areaBrush, px, y, 1, baseline - y);
                    }
                }
            }

            // Pass 2: the connecting line, on top of any fill.
            for (var s = 1; s < ellipsePoints.Count; s++) {
                var a = ((ChartPointModel)ellipsePoints[s - 1]).Point;
                var b = ((ChartPointModel)ellipsePoints[s]).Point;
                graph.DrawLine(chartPen, a.X, a.Y, b.X, b.Y);
            }

            foreach (ChartPointModel pm in ellipsePoints) {
                var textSize = graph.MeasureString(pm.Value.ToString(), this.Font);

                graph.FillEllipse(ellipseBrush, pm.Point.X - this.RadiusPoint / 2,
                    pm.Point.Y - this.RadiusPoint / 2, this.RadiusPoint, this.RadiusPoint);
                graph.DrawString($"({pm.Value})", this.Font, textBrush,
                    pm.Point.X - textSize.Width / 2,
                    pm.Point.Y - this.Font.Height - (int)Scale(15, SCALE_FROM_HEIGHT, this.Height));
            }

            return bitmap;
        }

        private System.Drawing.Bitmap GetRectangleChart() {
            var bitmap = new System.Drawing.Bitmap(this.Width, this.Height);

            using var graph = Graphics.FromImage(bitmap);
            using var axisPen = ToSdPen(this.AxisPen);
            using var bgBrush = ToSdBrush(this.BackgroundColor);
            using var ellipseBrush = ToSdBrush(this.EllipseColor);
            using var divisionBrush = ToSdBrush(this.DivisionColor);
            using var textBrush = ToSdBrush(this.TextColor);
            // The bar outline strokes use the chart's background color (gives
            // the bars a "card cut from the background" look). One pen, reused.
            using var barOutlinePen = new System.Drawing.Pen(ToSd(this.BackgroundColor.Color), 2);

            graph.FillRectangle(bgBrush, 0, 0, this.Width, this.Height);

            graph.DrawString(this.ChartTitle, this.Font, textBrush,
                this.Width / 2 - (this.ChartTitle.Length / 2 * (int)Scale(18, SCALE_FROM_WIDTH, this.Width)),
                (int)Scale(30, SCALE_FROM_HEIGHT, this.Height));

            graph.DrawLine(axisPen, this.margin, this.margin + (int)Scale(100, SCALE_FROM_HEIGHT, this.Height), this.margin, this.Height - this.margin);
            graph.DrawLine(axisPen, this.margin, this.Height - this.margin, this.Width - this.margin, this.Height - this.margin);

            var maxValue = Math.Ceiling(this.GetMax(this.Items));
            var minValue = (int)this.GetMin(this.Items);
            var countValue = this.Items.Count;

            var chartWidth = Math.Abs(this.pEnd.X - this.pStart.X - (int)Scale(50, SCALE_FROM_WIDTH, this.Width));
            var chartHeight = Math.Abs(this.pEnd.Y - (this.pStart.Y - (int)Scale(50, SCALE_FROM_HEIGHT, this.Height)));

            var range = maxValue - minValue;
            var divisionHeight = (range != 0) ? chartHeight / range : 0;
            var divisionWidth = (countValue > 0) ? chartWidth / countValue : 0;

            var startDivX = this.pStart.X + divisionWidth;
            foreach (DataItem xx in this.Items) {
                graph.FillEllipse(divisionBrush, startDivX - this.RadiusPoint / 2, this.pStart.Y - this.RadiusPoint / 2,
                    this.RadiusPoint, this.RadiusPoint);
                graph.DrawString(xx.Name, this.Font, textBrush,
                    startDivX - (int)Scale(7, SCALE_FROM_WIDTH, this.Width),
                    this.pStart.Y + this.margin / 2 - (int)Scale(7, SCALE_FROM_HEIGHT, this.Height));
                startDivX += divisionWidth * this.DivisionAxisX;
            }

            var startDivY = this.pStart.Y - (int)Scale(25, SCALE_FROM_HEIGHT, this.Height);
            for (var i = minValue; i <= maxValue; i += this.DivisionAxisY) {
                graph.FillEllipse(divisionBrush, this.pStart.X - this.paddingLeft - this.RadiusPoint / 2, startDivY - this.RadiusPoint / 2,
                    this.RadiusPoint, this.RadiusPoint);
                graph.DrawString(i.ToString(), this.Font, textBrush,
                    this.pStart.X - this.paddingLeft + this.margin / 2,
                    startDivY - (int)Scale(10, SCALE_FROM_HEIGHT, this.Height));
                startDivY -= (int)(divisionHeight * this.DivisionAxisY);
            }

            for (var i = 0; i < this.Items.Count; i++) {
                var item = (DataItem)this.Items[i];
                var pixelYValue = divisionHeight * item.Value -
                    divisionHeight * minValue + (int)Scale(25, SCALE_FROM_HEIGHT, this.Height);
                var pixelXValue = divisionWidth * (i + 1);
                const int BorderWidth = 2;

                graph.FillRectangle(ellipseBrush, this.pStart.X + pixelXValue - divisionWidth / 2,
                    this.pStart.Y - (int)pixelYValue, divisionWidth, (int)pixelYValue - BorderWidth);

                var commonX = this.pStart.X + pixelXValue - divisionWidth / 2;
                graph.DrawLine(barOutlinePen, commonX, this.pStart.Y - BorderWidth, commonX, this.pStart.Y - (int)pixelYValue);
                graph.DrawLine(barOutlinePen, commonX, this.pStart.Y - (int)pixelYValue, commonX + divisionWidth, this.pStart.Y - (int)pixelYValue);
                graph.DrawLine(barOutlinePen, commonX + divisionWidth, this.pStart.Y - (int)pixelYValue, commonX + divisionWidth, this.pStart.Y - BorderWidth);

                var textSize = graph.MeasureString(item.Value.ToString(), this.Font);

                graph.DrawString(item.Value.ToString(), this.Font, textBrush,
                    commonX + (divisionWidth - textSize.Width) / 2,
                    this.pStart.Y - (int)pixelYValue - this.Font.Height - BorderWidth);
            }

            return bitmap;
        }

        static int Scale(int value, int orig, int scale) {
            var v = (scale * value) / orig;
            if (v == 0) v = 1;
            return v;
        }

        // --- IDisposable -----------------------------------------------------

        private bool disposed;

        /// <summary>Releases the resources used by the chart.</summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the cached chart bitmap.</summary>
        protected virtual void Dispose(bool disposing) {
            if (this.disposed) return;

            if (disposing) {
                this.DisposeCache();
            }

            this.disposed = true;
        }

        ~Chart() {
            this.Dispose(false);
        }
    }
}
