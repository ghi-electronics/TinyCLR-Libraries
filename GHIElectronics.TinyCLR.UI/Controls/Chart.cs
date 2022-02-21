using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class Chart : Image, IDisposable {

        public enum ChartMode {
            LineMode,
            RectangleMode
        }

        public class DataItem {
            public double Value { get; set; }
            public string Name { get; set; }
        }

        public class Point {
            public Point() {

            }
            public Point(int ax, int ay) {
                this.X = ax;
                this.Y = ay;

            }
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class PointF {
            public PointF(float ax, float ay) {
                this.X = ax;
                this.Y = ay;

            }
            public float X { get; set; }
            public float Y { get; set; }
        }

        public class PointModel {
            public Point Point { get; set; }
            public double Value { get; set; }
        }

        public int DivisionAxisX { get; set; } = 1;
        public int DivisionAxisY { get; set; } = 1;
        public Font Font { get; set; }
        public System.Drawing.Pen AxisPen { get; set; } = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
        public System.Drawing.Pen ChartPen { get; set; } = new System.Drawing.Pen(System.Drawing.Color.Green, 1);
        public int RadiusPoint { get; set; } = 10;
        public System.Drawing.Brush EllipseColor { get; set; } = new SolidBrush(System.Drawing.Color.Black);
        public System.Drawing.Brush DivisionColor { get; set; } = new SolidBrush(System.Drawing.Color.Black);
        public System.Drawing.Brush TextColor { get; set; } = new SolidBrush(System.Drawing.Color.Black);
        public System.Drawing.Brush BackgroundColor { get; set; } = new SolidBrush(System.Drawing.Color.White);

        private int paddingLeft = 50;
        public string ChartTitle { get; set; } = "Chart1";
        public ArrayList Items { get; set; }

        private int margin = 50;
        private Point pStart;
        private Point pEnd;
        public ChartMode Mode { get; set; } = ChartMode.LineMode;

        const int SCALE_FROM_WIDTH = 800;
        const int SCALE_FROM_HEIGHT = 480;

        public Chart(int width, int height) {
            this.Width = width;
            this.Height = height;

            this.margin = (int)Scale(this.margin, SCALE_FROM_HEIGHT, height);
            this.paddingLeft = (int)Scale(this.paddingLeft, SCALE_FROM_WIDTH, width);

            this.pStart = new Point(this.margin + this.paddingLeft, height - this.margin);
            this.pEnd = new Point(width - this.margin, this.margin + (int)Scale(100, SCALE_FROM_HEIGHT, height));
        }

        private System.Drawing.Bitmap GetChart() => this.Mode switch {
            ChartMode.LineMode => this.GetLineChart(),
            ChartMode.RectangleMode => this.GetRectangleChart(),
            _ => null
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

        private System.Drawing.Bitmap GetLineChart() {
            var bitmap = new System.Drawing.Bitmap(this.Width, this.Height);

            using var graph = Graphics.FromImage(bitmap);
            //graph.SmoothingMode = SmoothingMode.AntiAlias;
            //graph.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            //graph.InterpolationMode = InterpolationMode.High;

            var imageSize = new Rectangle(0, 0, this.Width, this.Height);
            graph.FillRectangle(this.BackgroundColor, imageSize.X, imageSize.Y, imageSize.Width, imageSize.Height);

            //title
            graph.DrawString(this.ChartTitle, this.Font, this.TextColor,
                this.Width / 2 - (this.ChartTitle.Length / 2 * (int)Scale(18, SCALE_FROM_WIDTH, this.Width)), (int)Scale(30, SCALE_FROM_HEIGHT, this.Height));

            graph.DrawLine(this.AxisPen, this.margin, this.margin + (int)Scale(100, SCALE_FROM_HEIGHT, this.Height), this.margin, this.Height - this.margin);
            graph.DrawLine(this.AxisPen, this.margin, this.Height - this.margin, this.Width - this.margin, this.Height - this.margin);

            var maxValue = Math.Ceiling(this.GetMax(this.Items));
            var minValue = (int)this.GetMin(this.Items);
            var countValue = this.Items.Count;

            var chartWidth = Math.Abs(this.pEnd.X - this.pStart.X - (int)Scale(50, SCALE_FROM_WIDTH, this.Width));
            var chartHeight = Math.Abs(this.pEnd.Y - (this.pStart.Y - (int)Scale(50, SCALE_FROM_HEIGHT, this.Height)));

            var divisionHeight = chartHeight / (maxValue - minValue);
            var divisionWidth = chartWidth / countValue;

            #region Draw divisions

            var startDivX = this.pStart.X + divisionWidth;
            foreach (DataItem xx in this.Items) {
                var item = xx.Name;
                graph.FillEllipse(this.DivisionColor, startDivX - this.RadiusPoint / 2, this.pStart.Y - this.RadiusPoint / 2,
                    this.RadiusPoint,
                    this.RadiusPoint);
                graph.DrawString(item.ToString(), this.Font, this.TextColor,
                    startDivX - (int)Scale(7, SCALE_FROM_WIDTH, this.Width), this.pStart.Y + this.margin / 2 - (int)Scale(7, SCALE_FROM_HEIGHT, this.Height));
                startDivX += divisionWidth * this.DivisionAxisX;
            }

            var startDivY = this.pStart.Y - (int)Scale(25, SCALE_FROM_HEIGHT, this.Height);
            for (var i = minValue; i <= maxValue; i += this.DivisionAxisY) {
                graph.FillEllipse(this.DivisionColor, this.pStart.X - this.paddingLeft - this.RadiusPoint / 2, startDivY - this.RadiusPoint / 2,
                    this.RadiusPoint,
                    this.RadiusPoint);
                graph.DrawString(i.ToString(), this.Font, this.TextColor,
                    this.pStart.X - this.paddingLeft + this.margin / 2, startDivY - (int)Scale(10, SCALE_FROM_HEIGHT, this.Height));
                startDivY -= (int)(divisionHeight * this.DivisionAxisY);
            }

            #endregion

            #region Draw points

            var prevPoint = new Point();

            var ellipsePoints = new ArrayList(); //PointModel
            for (var i = 0; i < this.Items.Count; i++) {
                var item = (DataItem)this.Items[i];
                var pixelYValue = divisionHeight * item.Value -
                    divisionHeight * minValue + (int)Scale(25, SCALE_FROM_HEIGHT, this.Height);
                var pixelXValue = divisionWidth * (i + 1);

                if (i > 0) {
                    var currentPoint = new Point(this.pStart.X + pixelXValue, (int)(this.pStart.Y - pixelYValue));
                    graph.DrawLine(this.ChartPen, prevPoint.X, prevPoint.Y, currentPoint.X, currentPoint.Y);
                }

                ellipsePoints.Add(new PointModel() {
                    Point = new Point(this.pStart.X + pixelXValue, (int)(this.pStart.Y - pixelYValue)),
                    Value = item.Value
                });

                prevPoint = new Point(this.pStart.X + pixelXValue, (int)(this.pStart.Y - pixelYValue));
            }

            foreach (PointModel pointModel in ellipsePoints) {
                var textSize = graph.MeasureString(pointModel.Value.ToString(), this.Font);

                graph.FillEllipse(this.EllipseColor, pointModel.Point.X - this.RadiusPoint / 2,
                    pointModel.Point.Y - this.RadiusPoint / 2, this.RadiusPoint, this.RadiusPoint);
                graph.DrawString($"({pointModel.Value})", this.Font, this.TextColor,
                    pointModel.Point.X - textSize.Width / 2,
                    pointModel.Point.Y - this.Font.Height - (int)Scale(15, SCALE_FROM_HEIGHT, this.Height));
            }

            #endregion

            return bitmap;
        }

        private System.Drawing.Bitmap GetRectangleChart() {
            var bitmap = new System.Drawing.Bitmap(this.Width, this.Height);

            using var graph = Graphics.FromImage(bitmap);

            var imageSize = new Rectangle(0, 0, this.Width, this.Height);
            graph.FillRectangle(this.BackgroundColor, imageSize.X, imageSize.Y, imageSize.Width, imageSize.Height);

            //title
            graph.DrawString(this.ChartTitle, this.Font, this.TextColor,
                this.Width / 2 - (this.ChartTitle.Length / 2 * (int)Scale(18, SCALE_FROM_WIDTH, this.Width)), (int)Scale(30, SCALE_FROM_HEIGHT, this.Height));

            graph.DrawLine(this.AxisPen, this.margin, this.margin + (int)Scale(100, SCALE_FROM_HEIGHT, this.Height), this.margin, this.Height - this.margin);
            graph.DrawLine(this.AxisPen, this.margin, this.Height - this.margin, this.Width - this.margin, this.Height - this.margin);

            var maxValue = Math.Ceiling(this.GetMax(this.Items));
            var minValue = (int)this.GetMin(this.Items);
            var countValue = this.Items.Count;

            var chartWidth = Math.Abs(this.pEnd.X - this.pStart.X - (int)Scale(50, SCALE_FROM_WIDTH, this.Width));
            var chartHeight = Math.Abs(this.pEnd.Y - (this.pStart.Y - (int)Scale(50, SCALE_FROM_HEIGHT, this.Height)));

            var divisionHeight = chartHeight / (maxValue - minValue);
            var divisionWidth = chartWidth / countValue;

            #region Draw divisions

            var startDivX = this.pStart.X + divisionWidth;
            foreach (DataItem xx in this.Items) {
                var item = xx.Name;
                graph.FillEllipse(this.DivisionColor, startDivX - this.RadiusPoint / 2, this.pStart.Y - this.RadiusPoint / 2,
                    this.RadiusPoint,
                    this.RadiusPoint);
                graph.DrawString(item.ToString(), this.Font, this.TextColor,
                    startDivX - (int)Scale(7, SCALE_FROM_WIDTH, this.Width), this.pStart.Y + this.margin / 2 - (int)Scale(7, SCALE_FROM_HEIGHT, this.Height));
                startDivX += divisionWidth * this.DivisionAxisX;
            }

            var startDivY = this.pStart.Y - (int)Scale(25, SCALE_FROM_HEIGHT, this.Height);
            for (var i = minValue; i <= maxValue; i += this.DivisionAxisY) {
                graph.FillEllipse(this.DivisionColor, this.pStart.X - this.paddingLeft - this.RadiusPoint / 2, startDivY - this.RadiusPoint / 2,
                    this.RadiusPoint,
                    this.RadiusPoint);
                graph.DrawString(i.ToString(), this.Font, this.TextColor,
                    this.pStart.X - this.paddingLeft + this.margin / 2, startDivY - (int)Scale(10, SCALE_FROM_HEIGHT, this.Height));
                startDivY -= (int)(divisionHeight * this.DivisionAxisY);
            }

            #endregion

            #region Draw rectangles

            for (var i = 0; i < this.Items.Count; i++) {
                var item = (DataItem)this.Items[i];
                var itemValue = item.Value;
                var pixelYValue = divisionHeight * itemValue -
                    divisionHeight * minValue + (int)Scale(25, SCALE_FROM_HEIGHT, this.Height);
                var pixelXValue = divisionWidth * (i + 1);
                const int BorderWidth = 2;

                graph.FillRectangle(this.EllipseColor, this.pStart.X + pixelXValue - divisionWidth / 2,
                    this.pStart.Y - (int)pixelYValue,
                    divisionWidth, (int)pixelYValue - BorderWidth);

                var commonX = this.pStart.X + pixelXValue - divisionWidth / 2;
                graph.DrawLine(new System.Drawing.Pen(this.BackgroundColor, BorderWidth), commonX,
                    this.pStart.Y - BorderWidth, commonX, this.pStart.Y - (int)pixelYValue);
                graph.DrawLine(new System.Drawing.Pen(this.BackgroundColor, BorderWidth), commonX,
                    this.pStart.Y - (int)pixelYValue, commonX + divisionWidth,
                    this.pStart.Y - (int)pixelYValue);
                graph.DrawLine(new System.Drawing.Pen(this.BackgroundColor, BorderWidth),
                    commonX + divisionWidth,
                    this.pStart.Y - (int)pixelYValue, commonX + divisionWidth,
                    this.pStart.Y - BorderWidth);

                var textSize = graph.MeasureString(itemValue.ToString(), this.Font);

                graph.DrawString(itemValue.ToString(), this.Font, this.TextColor, commonX + (divisionWidth - textSize.Width)/2,
                    this.pStart.Y - (int)pixelYValue - this.Font.Height - BorderWidth);
            }
            #endregion

            return bitmap;
        }

        static int Scale(int value, int orig, int scale) {
            var v = (scale * value) / orig;

            if (v == 0)
                v = 1;
            return v;
        }

        private bool disposed;

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
            }
        }

        ~Chart() {
            this.Dispose(false);
        }

        public override void OnRender(DrawingContext dc) {
            if (this.Font == null)
                throw new ArgumentNullException("Font null!");                

            var uiBmp = BitmapImage.FromGraphics(Graphics.FromImage(this.GetChart()));

            dc.DrawImage(uiBmp, 0, 0);
        }
    }
}
