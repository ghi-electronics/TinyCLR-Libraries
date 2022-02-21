using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;
using Color = GHIElectronics.TinyCLR.UI.Media.Color;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class Gauge : Image, IDisposable {

        #region Private Attributes       
        private float minValue = 0;
        private float maxValue = 100;
        private float threshold = 25;
        private float currentValue = 0;
        private float recommendedValue = 25;
        private int noOfDivisions = 10;
        private int noOfSubDivisions = 3;
        private string dialText = string.Empty;
        private System.Drawing.Color foreColor = System.Drawing.Color.FromArgb(255, 0, 0, 0);
        private System.Drawing.Color dialColor = System.Drawing.Color.FromArgb(255, 230, 230, 250);
        private float glossinessAlpha = 72;//25;
        private int oldWidth, oldHeight;
        int x, y, width, height;
        float fromAngle = 135F;
        float toAngle = 405F;
        private bool enableTransparentBackground = true;
        private bool requiresRedraw;
        private System.Drawing.Image backgroundImg;
        private Rectangle rectImg;
        System.Drawing.Bitmap bmp;
        private Font Font { get; }

        #endregion
        public bool EnableDigitalNumber { get; set; }
        public bool EnableThresold { get; set; }

        private struct PointF {
            public PointF(float ax, float ay) {
                this.X = ax;
                this.Y = ay;

            }
            public float X { get; set; }
            public float Y { get; set; }
        }

        public Gauge(int radius, Font font) : base() {
            this.Width = radius;
            this.Height = radius;
            this.Font = font;

            this.noOfDivisions = 10;
            this.noOfSubDivisions = 3;
            this.requiresRedraw = true;

            this.Resize();
        }


        private System.Drawing.Color backColor = System.Drawing.Color.FromArgb(255, 255, 255, 255);
        public Color BackColor {
            get => Color.FromArgb(this.backColor.A, this.backColor.R, this.backColor.G, this.backColor.B);
            set => this.backColor = System.Drawing.Color.FromArgb(value.A, value.R, value.G, value.B);
        }

        #region Public Properties
        /// <summary>
        /// Mininum value on the scale
        /// </summary>

        public float MinValue {
            get => this.minValue;
            set {
                if (value < this.maxValue) {
                    this.minValue = value;
                    if (this.currentValue < this.minValue)
                        this.currentValue = this.minValue;
                    if (this.recommendedValue < this.minValue)
                        this.recommendedValue = this.minValue;
                    this.requiresRedraw = true;

                }
            }
        }

        /// <summary>
        /// Maximum value on the scale
        /// </summary>

        public float MaxValue {
            get => this.maxValue;
            set {
                if (value > this.minValue) {
                    this.maxValue = value;
                    if (this.currentValue > this.maxValue)
                        this.currentValue = this.maxValue;
                    if (this.recommendedValue > this.maxValue)
                        this.recommendedValue = this.maxValue;
                    this.requiresRedraw = true;

                }
            }
        }

        /// <summary>
        /// Gets or Sets the Threshold area from the Recommended Value. (1-99%)
        /// </summary>
        public float ThresholdPercent {
            get => this.threshold;
            set {
                if (value > 0 && value < 100) {
                    this.threshold = value;
                    this.requiresRedraw = true;

                }
            }
        }

        /// <summary>
        /// Threshold value from which green area will be marked.
        /// </summary>
        public float RecommendedValue {
            get => this.recommendedValue;
            set {
                if (value > this.minValue && value < this.maxValue) {
                    this.recommendedValue = value;
                    this.requiresRedraw = true;

                }
            }
        }

        /// <summary>
        /// Value where the pointer will point to.
        /// </summary>
        public float Value {
            get => this.currentValue;
            set {
                if (value >= this.minValue && value <= this.maxValue) {
                    this.currentValue = value;

                }
            }
        }

        /// <summary>
        /// Background color of the dial
        /// </summary>
        public Color DialColor {
            get => Color.FromArgb(this.dialColor.A, this.dialColor.R, this.dialColor.G, this.dialColor.B);
            set {
                this.dialColor = System.Drawing.Color.FromArgb(value.A, value.R, value.G, value.B);
                this.requiresRedraw = true;

            }
        }

        /// <summary>
        /// Glossiness strength. Range: 0-100
        /// </summary>

        public float Glossiness {
            get => (this.glossinessAlpha * 100) / 220;
            set {
                var val = value;
                if (val > 100)
                    value = 100;
                if (val < 0)
                    value = 0;
                this.glossinessAlpha = (value * 220) / 100;

            }
        }

        /// <summary>
        /// Get or Sets the number of Divisions in the dial scale.
        /// </summary>
        public int NoOfDivisions {
            get => this.noOfDivisions;
            set {
                if (value > 1 && value < 25) {
                    this.noOfDivisions = value;
                    this.requiresRedraw = true;

                }
            }
        }

        /// <summary>
        /// Gets or Sets the number of Sub Divisions in the scale per Division.
        /// </summary>
        public int NoOfSubDivisions {
            get => this.noOfSubDivisions;
            set {
                if (value > 0 && value <= 10) {
                    this.noOfSubDivisions = value;
                    this.requiresRedraw = true;
                }
            }
        }

        /// <summary>
        /// Gets or Sets the Text to be displayed in the dial
        /// </summary>
        public string DialText {
            get => this.dialText;
            set {
                this.dialText = value;
                this.requiresRedraw = true;

            }
        }

        /// <summary>
        /// Enables or Disables Transparent Background color.
        /// Note: Enabling this will reduce the performance and may make the control flicker.
        /// </summary>
        public bool EnableTransparentBackground {
            get => this.enableTransparentBackground;
            set {
                this.enableTransparentBackground = value;
                this.requiresRedraw = true;

            }
        }
        #endregion

        #region Overriden Control methods
        /// <summary>
        /// Draws the pointer.
        /// </summary>
        /// <param name="e"></param>
        private System.Drawing.Bitmap GetGauge() {
            this.width = this.Width - this.x * 2;
            this.height = this.Height - this.y * 2;

            var gfx = Graphics.FromImage(this.bmp);
            gfx.Clear();

            this.PaintBackground(gfx);
            this.DrawPointer(gfx, ((this.width) / 2) + this.x, ((this.height) / 2) + this.y);
            //gfx.Flush();

            return this.bmp;
        }

        /// <summary>
        /// Draws the dial background.
        /// </summary>
        /// <param name="e"></param>
        protected void PaintBackground(Graphics gfx) {

            gfx.FillRectangle(new SolidBrush(this.backColor), 0, 0, this.Width, this.Height);
            if (this.backgroundImg == null)
                this.backgroundImg = new System.Drawing.Bitmap(this.Width, this.Height);

            if (this.requiresRedraw) {
                var g = Graphics.FromImage(this.backgroundImg);
                g.FillRectangle(new SolidBrush(this.backColor), 0, 0, this.Width, this.Height);
                //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                this.width = this.Width - this.x * 2;
                this.height = this.Height - this.y * 2;
                this.rectImg = new Rectangle(this.x, this.y, this.width, this.height);

                //Draw background color
                var backGroundBrush = new SolidBrush(this.dialColor);//new SolidBrush(Color.FromArgb(120, dialColor));
                if (this.enableTransparentBackground) {
                    float gg = this.width / 60;
                    g.FillEllipse(new SolidBrush(this.backColor), (int)-gg, (int)-gg, (int)(this.Width + gg * 2), (int)(this.Height + gg * 2));
                }
                g.FillEllipse(backGroundBrush, this.x, this.y, this.width, this.height);

                //Draw Rim
                var outlineBrush = new SolidBrush(System.Drawing.Color.FromArgb(112, 128, 144));//gray
                var outline = new System.Drawing.Pen(outlineBrush, (float)(this.width * .03));
                //g.DrawEllipse(outline, this.rectImg.X, this.rectImg.Y, this.rectImg.Width, this.rectImg.Height);
                var darkRim = new System.Drawing.Pen(System.Drawing.Color.FromArgb(112, 128, 144));//gray
                g.DrawEllipse(darkRim, this.x, this.y, this.width, this.height);

                //Draw Callibration
                this.DrawCalibration(g, this.rectImg, ((this.width) / 2) + this.x, ((this.height) / 2) + this.y);

                if (this.EnableThresold) {
                    //Draw Colored Rim
                    var colorPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(220, 220, 220), this.Width / 40);//new Pen(Color.FromArgb(190, Color.FromArgb(220, 220, 220)), this.Width / 40);
                                                                                                                         //var blackPen = new System.Drawing.Pen(System.Drawing.Color.Black, this.Width / 200);//new Pen(Color.FromArgb(250, Color.Black), this.Width / 200);
                    var gap = (int)(this.Width * 0.01F);
                    var rectg = new Rectangle(this.rectImg.X + gap, this.rectImg.Y + gap, this.rectImg.Width - gap * 2, this.rectImg.Height - gap * 2);

                    this.DrawArc(g, colorPen, rectg, 135, 270);


                    //Draw Threshold
                    colorPen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(124, 252, 0), this.Width / 50);
                    rectg = new Rectangle(this.rectImg.X + gap, this.rectImg.Y + gap, this.rectImg.Width - gap * 2, this.rectImg.Height - gap * 2);
                    var val = this.MaxValue - this.MinValue;
                    val = (100 * (this.recommendedValue - this.MinValue)) / val;
                    val = ((this.toAngle - this.fromAngle) * val) / 100;
                    val += this.fromAngle;
                    var stAngle = val - ((270 * this.threshold) / 200);
                    if (stAngle <= 135) stAngle = 135;
                    var sweepAngle = ((270 * this.threshold) / 100);
                    if (stAngle + sweepAngle > 405) sweepAngle = 405 - stAngle;
                    this.DrawArc(g, colorPen, rectg, stAngle, sweepAngle);
                }


                if (this.EnableDigitalNumber) {

                    //Draw Digital Value
                    var digiRect = new Rectangle((int)(this.Width / 2F - (int)this.width / 5F), (int)(this.height / 1.2F), (int)(this.width / 2.5F), (int)(this.Height / 9F));
                    var digiFRect = new Rectangle((int)(this.Width / 2 - this.width / 7), (int)(this.height / 1.18), (int)(this.width / 4), (int)(this.Height / 12));
                    g.FillRectangle(new SolidBrush(System.Drawing.Color.Gray), digiRect.X, digiRect.Y, digiRect.Width, digiRect.Height);
                    this.DisplayNumber(g, this.currentValue, digiFRect);
                }


                var textSize = g.MeasureString(this.dialText, this.Font);
                var digiFRectText = new RectangleF(this.Width / 2 - textSize.Width / 2, (int)(this.height / 1.5), textSize.Width, textSize.Height);
                g.DrawString(this.dialText, this.Font, new SolidBrush(this.foreColor), digiFRectText);
            }
            gfx.DrawImage(this.backgroundImg, this.rectImg.X, this.rectImg.Y);
        }

        void DrawArc(Graphics g, System.Drawing.Pen pen, Rectangle rect, double startAngle, double sweepAngle) {
            int ax, ay;
            var start_angle = ToRadians(startAngle);
            var end_angle = ToRadians(sweepAngle);
            var r = rect.Width / 2;
            for (var i = start_angle; i < end_angle; i = i + 0.05) {
                ax = rect.X + (int)(r + Math.Cos(i) * r);
                ay = rect.Y + (int)(r + Math.Sin(i) * r);
                var solid = new SolidBrush(pen.Color);
                g.FillRectangle(solid, ax, ay, (int)pen.Width, (int)pen.Width); // center point is (x = 50, y = 100)
            }
        }


        #endregion

        #region Private methods
        /// <summary>
        /// Draws the Pointer.
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        private void DrawPointer(Graphics g, int cx, int cy) {
            var radius = this.Width / 2 - (this.Width * .12F);
            var val = this.MaxValue - this.MinValue;

            val = (100 * (this.currentValue - this.MinValue)) / val;
            val = ((this.toAngle - this.fromAngle) * val) / 100;
            val += this.fromAngle;

            var angle = this.GetRadian(val);

            var pts = new PointF[5];

            pts[0].X = (float)(cx + radius * Math.Cos(angle));
            pts[0].Y = (float)(cy + radius * Math.Sin(angle));

            pts[4].X = (float)(cx + radius * Math.Cos(angle - 0.02));
            pts[4].Y = (float)(cy + radius * Math.Sin(angle - 0.02));

            angle = this.GetRadian((val + 20));
            pts[1].X = (float)(cx + (this.Width * .09F) * Math.Cos(angle));
            pts[1].Y = (float)(cy + (this.Width * .09F) * Math.Sin(angle));

            pts[2].X = cx;
            pts[2].Y = cy;

            angle = this.GetRadian((val - 20));
            pts[3].X = (float)(cx + (this.Width * .09F) * Math.Cos(angle));
            pts[3].Y = (float)(cy + (this.Width * .09F) * Math.Sin(angle));

            var pointer = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            this.FillPolygon(g, pointer, pts);

            var shinePts = new PointF[3];
            angle = this.GetRadian(val);
            shinePts[0].X = (float)(cx + radius * Math.Cos(angle));
            shinePts[0].Y = (float)(cy + radius * Math.Sin(angle));

            angle = this.GetRadian(val + 20);
            shinePts[1].X = (float)(cx + (this.Width * .09F) * Math.Cos(angle));
            shinePts[1].Y = (float)(cy + (this.Width * .09F) * Math.Sin(angle));

            shinePts[2].X = cx;
            shinePts[2].Y = cy;

            var gpointer = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(112, 128, 144)); //(shinePts[0], shinePts[2], Color.SlateGray, Color.Black);
            this.FillPolygon(g, gpointer, shinePts);

            var rect = new Rectangle(this.x, this.y, this.width, this.height);
            this.DrawCenterPoint(g, rect, ((this.width) / 2) + this.x, ((this.height) / 2) + this.y);
        }
        void FillPolygon(Graphics g, System.Drawing.Brush brush, PointF[] points) {
            if (points.Length <= 1) return;
            var pen = new System.Drawing.Pen(brush);
            for (var i = 0; i < points.Length; i++) {
                if (i + 1 < points.Length) {
                    var src = points[i];
                    var dst = points[i + 1];
                    g.DrawLine(pen, (int)src.X, (int)src.Y, (int)dst.X, (int)dst.Y);
                }
            }
        }
        /// <summary>
        /// Draws the glossiness.
        /// </summary>
        /// <param name="g"></param>
        private void DrawGloss(Graphics g) {
            var glossRect = new RectangleF(
               this.x + (float)(this.width * 0.10),
               this.y + (float)(this.height * 0.07),
               (float)(this.width * 0.80),
               (float)(this.height * 0.7));
            var gradientBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            /*
            new LinearGradientBrush(glossRect,
            Color.FromArgb((int)glossinessAlpha, Color.White),
            Color.Transparent,
            LinearGradientMode.Vertical);
            */
            g.FillEllipse(gradientBrush, (int)glossRect.X, (int)glossRect.Y, (int)glossRect.Width, (int)glossRect.Height);

            //TODO: Gradient from bottom
            glossRect = new RectangleF(
               this.x + (float)(this.width * 0.25),
               this.y + (float)(this.height * 0.77),
               (float)(this.width * 0.50),
               (float)(this.height * 0.2));
            var gloss = (int)(this.glossinessAlpha / 3);
            gradientBrush = new SolidBrush(this.backColor);
            /*
                new LinearGradientBrush(glossRect,
                Color.Transparent, Color.FromArgb(gloss, this.BackColor),
                LinearGradientMode.Vertical);
            */
            g.FillEllipse(gradientBrush, (int)glossRect.X, (int)glossRect.Y, (int)glossRect.Width, (int)glossRect.Height);
        }

        /// <summary>
        /// Draws the center point.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="rect"></param>
        /// <param name="cX"></param>
        /// <param name="cY"></param>
        private void DrawCenterPoint(Graphics g, Rectangle rect, int cX, int cY) {
            float shift = this.Width / 5;
            var rectangle = new RectangleF(cX - (shift / 2), cY - (shift / 2), shift, shift);
            var brush = new SolidBrush(this.dialColor); //LinearGradientBrush(rect, Color.Black, Color.FromArgb(100, this.dialColor), LinearGradientMode.Vertical);
            g.FillEllipse(brush, (int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);

            shift = this.Width / 7;
            rectangle = new RectangleF(cX - (shift / 2), cY - (shift / 2), shift, shift);
            brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(112, 128, 144));//new LinearGradientBrush(rect, Color.SlateGray, Color.Black, LinearGradientMode.ForwardDiagonal);
            g.FillEllipse(brush, (int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
        }

        /// <summary>
        /// Draws the Ruler
        /// </summary>
        /// <param name="g"></param>
        /// <param name="rect"></param>
        /// <param name="cX"></param>
        /// <param name="cY"></param>
        private void DrawCalibration(Graphics g, Rectangle rect, int cX, int cY) {
            var noOfParts = this.noOfDivisions + 1;
            var noOfIntermediates = this.noOfSubDivisions;
            var currentAngle = this.GetRadian(this.fromAngle);
            var gap = (int)(this.Width * 0.01F);
            float shift = this.Width / 25;
            var rectangle = new Rectangle(rect.X + gap, rect.Y + gap, rect.Width - gap, rect.Height - gap);

            int x, y, x1, y1;
            float tx, ty, radius;
            radius = rectangle.Width / 2 - gap * 5;
            var totalAngle = this.toAngle - this.fromAngle;
            var incr = this.GetRadian(((totalAngle) / ((noOfParts - 1) * (noOfIntermediates + 1))));

            var thickPen = new System.Drawing.Pen(System.Drawing.Color.Black, this.Width / 50);
            var thinPen = new System.Drawing.Pen(System.Drawing.Color.Black, this.Width / 100);
            var rulerValue = this.MinValue;
            for (var i = 0; i <= noOfParts; i++) {
                //Draw Thick Line
                x = (int)(cX + radius * Math.Cos(currentAngle));
                y = (int)(cY + radius * Math.Sin(currentAngle));
                x1 = (int)(cX + (radius - this.Width / 20) * Math.Cos(currentAngle));
                y1 = (int)(cY + (radius - this.Width / 20) * Math.Sin(currentAngle));
                g.DrawLine(thickPen, x, y, x1, y1);

                //Draw Strings
                var format = new StringFormat();
                tx = (float)(cX + (radius - this.Width / 10) * Math.Cos(currentAngle));
                ty = (float)(cY - shift + (radius - this.Width / 10) * Math.Sin(currentAngle));
                var stringPen = new System.Drawing.SolidBrush(this.foreColor);

                this.Font.ComputeTextInRect(rulerValue.ToString(), out var rulerValueWidth, out var height);

                g.DrawString(rulerValue.ToString() + "", this.Font, stringPen, tx - rulerValueWidth / 2, ty + height / 2);

                rulerValue += (float)((this.MaxValue - this.MinValue) / (noOfParts - 1));
                rulerValue = (float)Math.Round(rulerValue);

                //currentAngle += incr;
                if (i == noOfParts - 1)
                    break;
                for (var j = 0; j <= noOfIntermediates; j++) {
                    //Draw thin lines 
                    currentAngle += incr;
                    x = (int)(cX + radius * Math.Cos(currentAngle));
                    y = (int)(cY + radius * Math.Sin(currentAngle));
                    x1 = (int)(cX + (radius - this.Width / 50) * Math.Cos(currentAngle));
                    y1 = (int)(cY + (radius - this.Width / 50) * Math.Sin(currentAngle));
                    g.DrawLine(thinPen, x, y, x1, y1);
                }
            }
        }

        /// <summary>
        /// Converts the given degree to radian.
        /// </summary>
        /// <param name="theta"></param>
        /// <returns></returns>
        public float GetRadian(float theta) => theta * (float)Math.PI / 180F;

        /// <summary>
        /// Displays the given number in the 7-Segement format.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="number"></param>
        /// <param name="drect"></param>
        private void DisplayNumber(Graphics g, float number, Rectangle drect) {


            var n = number.ToString("n0");
            var num = PadLeft(n, ((int)(this.MaxValue)).ToString().Length, '0');
            float shift = 0;
            if (number < 0) {
                shift -= this.width / 17;
            }
            var drawDPS = false;
            var chars = num.ToCharArray();
            for (var i = 0; i < chars.Length; i++) {
                var c = chars[i];
                if (i < chars.Length - 1 && chars[i + 1] == '.')
                    drawDPS = true;
                else
                    drawDPS = false;
                if (c != '.') {
                    if (c == '-') {
                        this.DrawDigit(g, -1, new PointF(drect.X + shift, drect.Y), drawDPS, drect.Height);
                    }
                    else {
                        this.DrawDigit(g, short.Parse(c.ToString()), new PointF(drect.X + shift, drect.Y), drawDPS, drect.Height);
                    }
                    shift += 15 * this.width / 250;
                }
                else {
                    shift += 2 * this.width / 250;
                }
            }


        }

        /// <summary>
        /// Draws a digit in 7-Segement format.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="number"></param>
        /// <param name="position"></param>
        /// <param name="dp"></param>
        /// <param name="height"></param>
        private void DrawDigit(Graphics g, int number, PointF position, bool dp, float height) {
            float width;
            width = 10F * height / 13;

            var outline = new System.Drawing.Pen(System.Drawing.Color.Black);//new Pen(Color.FromArgb(40, this.dialColor));
            var fillPen = new System.Drawing.Pen(this.dialColor);

            #region Form Polygon Points
            //Segment A
            var segmentA = new PointF[5];
            segmentA[0] = segmentA[4] = new PointF(position.X + this.GetX(2.8F, width), position.Y + this.GetY(1F, height));
            segmentA[1] = new PointF(position.X + this.GetX(10, width), position.Y + this.GetY(1F, height));
            segmentA[2] = new PointF(position.X + this.GetX(8.8F, width), position.Y + this.GetY(2F, height));
            segmentA[3] = new PointF(position.X + this.GetX(3.8F, width), position.Y + this.GetY(2F, height));

            //Segment B
            var segmentB = new PointF[5];
            segmentB[0] = segmentB[4] = new PointF(position.X + this.GetX(10, width), position.Y + this.GetY(1.4F, height));
            segmentB[1] = new PointF(position.X + this.GetX(9.3F, width), position.Y + this.GetY(6.8F, height));
            segmentB[2] = new PointF(position.X + this.GetX(8.4F, width), position.Y + this.GetY(6.4F, height));
            segmentB[3] = new PointF(position.X + this.GetX(9F, width), position.Y + this.GetY(2.2F, height));

            //Segment C
            var segmentC = new PointF[5];
            segmentC[0] = segmentC[4] = new PointF(position.X + this.GetX(9.2F, width), position.Y + this.GetY(7.2F, height));
            segmentC[1] = new PointF(position.X + this.GetX(8.7F, width), position.Y + this.GetY(12.7F, height));
            segmentC[2] = new PointF(position.X + this.GetX(7.6F, width), position.Y + this.GetY(11.9F, height));
            segmentC[3] = new PointF(position.X + this.GetX(8.2F, width), position.Y + this.GetY(7.7F, height));

            //Segment D
            var segmentD = new PointF[5];
            segmentD[0] = segmentD[4] = new PointF(position.X + this.GetX(7.4F, width), position.Y + this.GetY(12.1F, height));
            segmentD[1] = new PointF(position.X + this.GetX(8.4F, width), position.Y + this.GetY(13F, height));
            segmentD[2] = new PointF(position.X + this.GetX(1.3F, width), position.Y + this.GetY(13F, height));
            segmentD[3] = new PointF(position.X + this.GetX(2.2F, width), position.Y + this.GetY(12.1F, height));

            //Segment E
            var segmentE = new PointF[5];
            segmentE[0] = segmentE[4] = new PointF(position.X + this.GetX(2.2F, width), position.Y + this.GetY(11.8F, height));
            segmentE[1] = new PointF(position.X + this.GetX(1F, width), position.Y + this.GetY(12.7F, height));
            segmentE[2] = new PointF(position.X + this.GetX(1.7F, width), position.Y + this.GetY(7.2F, height));
            segmentE[3] = new PointF(position.X + this.GetX(2.8F, width), position.Y + this.GetY(7.7F, height));

            //Segment F
            var segmentF = new PointF[5];
            segmentF[0] = segmentF[4] = new PointF(position.X + this.GetX(3F, width), position.Y + this.GetY(6.4F, height));
            segmentF[1] = new PointF(position.X + this.GetX(1.8F, width), position.Y + this.GetY(6.8F, height));
            segmentF[2] = new PointF(position.X + this.GetX(2.6F, width), position.Y + this.GetY(1.3F, height));
            segmentF[3] = new PointF(position.X + this.GetX(3.6F, width), position.Y + this.GetY(2.2F, height));

            //Segment G
            var segmentG = new PointF[7];
            segmentG[0] = segmentG[6] = new PointF(position.X + this.GetX(2F, width), position.Y + this.GetY(7F, height));
            segmentG[1] = new PointF(position.X + this.GetX(3.1F, width), position.Y + this.GetY(6.5F, height));
            segmentG[2] = new PointF(position.X + this.GetX(8.3F, width), position.Y + this.GetY(6.5F, height));
            segmentG[3] = new PointF(position.X + this.GetX(9F, width), position.Y + this.GetY(7F, height));
            segmentG[4] = new PointF(position.X + this.GetX(8.2F, width), position.Y + this.GetY(7.5F, height));
            segmentG[5] = new PointF(position.X + this.GetX(2.9F, width), position.Y + this.GetY(7.5F, height));

            //Segment DP
            #endregion

            #region Draw Segments Outline
            this.FillPolygon(g, outline.Brush, segmentA);
            this.FillPolygon(g, outline.Brush, segmentB);
            this.FillPolygon(g, outline.Brush, segmentC);
            this.FillPolygon(g, outline.Brush, segmentD);
            this.FillPolygon(g, outline.Brush, segmentE);
            this.FillPolygon(g, outline.Brush, segmentF);
            this.FillPolygon(g, outline.Brush, segmentG);
            #endregion

            #region Fill Segments
            //Fill SegmentA
            if (this.IsNumberAvailable(number, 0, 2, 3, 5, 6, 7, 8, 9)) {
                this.FillPolygon(g, fillPen.Brush, segmentA);
            }

            //Fill SegmentB
            if (this.IsNumberAvailable(number, 0, 1, 2, 3, 4, 7, 8, 9)) {
                this.FillPolygon(g, fillPen.Brush, segmentB);
            }

            //Fill SegmentC
            if (this.IsNumberAvailable(number, 0, 1, 3, 4, 5, 6, 7, 8, 9)) {
                this.FillPolygon(g, fillPen.Brush, segmentC);
            }

            //Fill SegmentD
            if (this.IsNumberAvailable(number, 0, 2, 3, 5, 6, 8, 9)) {
                this.FillPolygon(g, fillPen.Brush, segmentD);
            }

            //Fill SegmentE
            if (this.IsNumberAvailable(number, 0, 2, 6, 8)) {
                this.FillPolygon(g, fillPen.Brush, segmentE);
            }

            //Fill SegmentF
            if (this.IsNumberAvailable(number, 0, 4, 5, 6, 7, 8, 9)) {
                this.FillPolygon(g, fillPen.Brush, segmentF);
            }

            //Fill SegmentG
            if (this.IsNumberAvailable(number, 2, 3, 4, 5, 6, 8, 9, -1)) {
                this.FillPolygon(g, fillPen.Brush, segmentG);
            }
            #endregion

            //Draw decimal point
            if (dp) {
                g.FillEllipse(fillPen.Brush,
                (int)(position.X + this.GetX(10F, width)),
                (int)(position.Y + this.GetY(12F, height)),
                (int)(width / 7),
                (int)(width / 7));
            }
        }

        /// <summary>
        /// Gets Relative X for the given width to draw digit
        /// </summary>
        /// <param name="x"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private float GetX(float x, float width) => x * width / 12;

        /// <summary>
        /// Gets relative Y for the given height to draw digit
        /// </summary>
        /// <param name="y"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private float GetY(float y, float height) => y * height / 15;

        /// <summary>
        /// Returns true if a given number is available in the given list.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="listOfNumbers"></param>
        /// <returns></returns>
        private bool IsNumberAvailable(int number, params int[] listOfNumbers) {
            if (listOfNumbers.Length > 0) {
                foreach (var i in listOfNumbers) {
                    if (i == number)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Restricts the size to make sure the height and width are always same.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Resize() {
            if (this.oldWidth != this.Width) {
                this.Height = this.Width;
                this.oldHeight = this.Width;
            }
            if (this.oldHeight != this.Height) {
                this.Width = this.Height;
                this.oldWidth = this.Width;
            }

            this.x = 0;
            this.y = 0;
            this.width = this.Width - this.x * 2;
            this.height = this.Height - this.y * 2;

            this.bmp = new System.Drawing.Bitmap(this.Width, this.Height);


        }
        #endregion


        static double ToRadians(double val) => (Math.PI / 180) * val;

        static string PadLeft(string val, int count, char c) {
            var iter = count - val.Length;
            var add = "";
            if (iter > 0) {
                for (var i = 0; i < iter; i++)
                    add += c;
            }

            return (add + val);
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

        ~Gauge() {
            this.Dispose(false);
        }

        public override void OnRender(DrawingContext dc) {
            var x = 0;
            var y = 0;

            var uiBmp = BitmapImage.FromGraphics(Graphics.FromImage(this.GetGauge()));

            dc.DrawImage(uiBmp, x, y);
        }


    }
}
