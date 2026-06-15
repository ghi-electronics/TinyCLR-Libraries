using System;
using System.Drawing;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;
using MediaColor = GHIElectronics.TinyCLR.UI.Media.Color;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>
    /// Analog gauge with calibrated tick marks, optional threshold arc, optional
    /// seven-segment digital readout, dial label, and pointer needle. Always
    /// square — pass the side length to the constructor.
    ///
    /// Rendering is cached: the static background (dial face, calibration,
    /// threshold, digital number, label) is drawn once into a backing bitmap;
    /// only the pointer is redrawn each paint. Any property change marks the
    /// background dirty.
    /// </summary>
    public class Gauge : Image, IDisposable {
        // TinyCLR's System.Drawing has no PointF; this is a local sub for
        // floating-point polygon vertices.
        private struct PointF {
            public float X;
            public float Y;
        }

        // --- visual constants (extracted from inline magic numbers) -----------

        private const float FromAngleDeg = 135f;
        private const float ToAngleDeg = 405f;          // 270° sweep, clockwise
        private const float PointerSpread = 20f;        // half-base spread in degrees
        private const float PointerTipOffset = 0.02f;   // radians of secondary tip used by glossy half

        private const float PointerRadiusFactor = 0.12f; // tip-of-needle distance from center as a fraction of side
        private const float PointerBaseFactor = 0.09f;   // half-base width of needle as a fraction of side

        private const int MinDivisions = 2;
        private const int MaxDivisions = 24;
        private const int MaxSubDivisions = 10;

        private const float GlossinessMaxAlpha = 220f;   // map Glossiness (0..100) → alpha (0..220)

        private static readonly MediaColor RimColor = MediaColor.FromArgb(0xFF, 112, 128, 144);   // SlateGray
        private static readonly MediaColor ThresholdRimColor = MediaColor.FromArgb(0xFF, 220, 220, 220);
        private static readonly MediaColor ThresholdMarkColor = MediaColor.FromArgb(0xFF, 124, 252, 0); // LawnGreen
        private static readonly MediaColor PointerColor = MediaColor.FromArgb(0xFF, 0, 0, 0);
        private static readonly MediaColor DigitalPanelColor = MediaColor.FromArgb(0xFF, 128, 128, 128);

        // --- public properties ------------------------------------------------

        /// <summary>Font used for the dial labels and digital readout.</summary>
        public Font Font { get; set; }
        /// <summary>When true, a seven-segment digital value is shown below the dial.</summary>
        public bool EnableDigitalNumber { get; set; }
        /// <summary>When true, the threshold arc around the recommended value is drawn.</summary>
        public bool EnableThreshold { get; set; }

        /// <summary>Background color behind the dial.</summary>
        public MediaColor BackColor {
            get => this._backColor;
            set { this._backColor = value; this.MarkDirty(); }
        }

        /// <summary>Color of the dial face.</summary>
        public MediaColor DialColor {
            get => this._dialColor;
            set { this._dialColor = value; this.MarkDirty(); }
        }

        /// <summary>Color of the tick marks, labels and dial text.</summary>
        public MediaColor ForeColor {
            get => this._foreColor;
            set { this._foreColor = value; this.MarkDirty(); }
        }

        /// <summary>Smallest value on the dial.</summary>
        public float MinValue {
            get => this._minValue;
            set {
                if (value < this._maxValue) {
                    this._minValue = value;
                    if (this._currentValue < value) this._currentValue = value;
                    if (this._recommendedValue < value) this._recommendedValue = value;
                    this.MarkDirty();
                }
            }
        }

        /// <summary>Largest value on the dial.</summary>
        public float MaxValue {
            get => this._maxValue;
            set {
                if (value > this._minValue) {
                    this._maxValue = value;
                    if (this._currentValue > value) this._currentValue = value;
                    if (this._recommendedValue > value) this._recommendedValue = value;
                    this.MarkDirty();
                }
            }
        }

        /// <summary>Threshold area around the recommended value, 1–99%.</summary>
        public float ThresholdPercent {
            get => this._threshold;
            set {
                if (value > 0 && value < 100) {
                    this._threshold = value;
                    this.MarkDirty();
                }
            }
        }

        /// <summary>Value the threshold arc is centered on.</summary>
        public float RecommendedValue {
            get => this._recommendedValue;
            set {
                if (value > this._minValue && value < this._maxValue) {
                    this._recommendedValue = value;
                    this.MarkDirty();
                }
            }
        }

        /// <summary>Current needle position. Only this property doesn't dirty
        /// the background — the pointer is repainted every frame.</summary>
        public float Value {
            get => this._currentValue;
            set {
                if (value >= this._minValue && value <= this._maxValue) {
                    this._currentValue = value;
                    this.Invalidate();
                }
            }
        }

        /// <summary>Glossiness strength 0..100 (mapped to 0..220 alpha internally).</summary>
        public float Glossiness {
            get => this._glossinessAlpha * 100f / GlossinessMaxAlpha;
            set {
                if (value < 0) value = 0;
                if (value > 100) value = 100;
                this._glossinessAlpha = value * GlossinessMaxAlpha / 100f;
            }
        }

        /// <summary>Number of major tick divisions on the dial (2-24).</summary>
        public int NoOfDivisions {
            get => this._noOfDivisions;
            set {
                if (value > MinDivisions - 1 && value < MaxDivisions + 1) {
                    this._noOfDivisions = value;
                    this.MarkDirty();
                }
            }
        }

        /// <summary>Number of minor tick marks between major divisions (1-10).</summary>
        public int NoOfSubDivisions {
            get => this._noOfSubDivisions;
            set {
                if (value > 0 && value <= MaxSubDivisions) {
                    this._noOfSubDivisions = value;
                    this.MarkDirty();
                }
            }
        }

        /// <summary>Label text drawn on the dial face.</summary>
        public string DialText {
            get => this._dialText;
            set { this._dialText = value ?? string.Empty; this.MarkDirty(); }
        }

        /// <summary>
        /// When true, the dial face is overlapped by a slightly larger ellipse
        /// in the back color so the dial appears to float. Costs an extra
        /// FillEllipse per redraw.
        /// </summary>
        public bool EnableTransparentBackground {
            get => this._enableTransparentBackground;
            set { this._enableTransparentBackground = value; this.MarkDirty(); }
        }

        // --- private state ---------------------------------------------------

        private float _minValue = 0;
        private float _maxValue = 100;
        private float _threshold = 25;
        private float _currentValue = 0;
        private float _recommendedValue = 25;
        private int _noOfDivisions = 10;
        private int _noOfSubDivisions = 3;
        private string _dialText = string.Empty;
        private MediaColor _backColor = MediaColor.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
        private MediaColor _dialColor = MediaColor.FromArgb(0xFF, 230, 230, 250);   // Lavender
        private MediaColor _foreColor = MediaColor.FromArgb(0xFF, 0, 0, 0);
        private float _glossinessAlpha = 72;
        private bool _enableTransparentBackground = true;

        // Cached System.Drawing surfaces — the composite bitmap we hand to the
        // UI layer (`_bmp` wrapped by `_cachedImage`), and a separate background
        // bitmap so we can repaint only the pointer between value changes.
        private System.Drawing.Bitmap _bmp;
        private System.Drawing.Image _backgroundImg;
        private BitmapImage _cachedImage;
        private int _cachedSide;
        private bool _backgroundDirty = true;

        private bool _disposed;

        // --- construction ----------------------------------------------------

        /// <summary>Creates a new square Gauge with the given side length in pixels.</summary>
        public Gauge(int side) : base() {
            this.Width = side;
            this.Height = side;
        }

        // --- IDisposable -----------------------------------------------------

        /// <summary>Releases the resources used by the gauge.</summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the gauge's cached drawing surfaces.</summary>
        protected virtual void Dispose(bool disposing) {
            if (this._disposed) return;

            if (disposing) {
                this.ReleaseSurfaces();
            }

            this._disposed = true;
        }

        ~Gauge() => this.Dispose(false);

        private void ReleaseSurfaces() {
            this._cachedImage?.graphics?.Dispose();
            this._cachedImage = null;
            this._bmp?.Dispose();
            this._bmp = null;
            this._backgroundImg?.Dispose();
            this._backgroundImg = null;
        }

        private void MarkDirty() {
            this._backgroundDirty = true;
            this.Invalidate();
        }

        // --- rendering -------------------------------------------------------

        /// <summary>Draws the dial face and pointer needle.</summary>
        public override void OnRender(DrawingContext dc) {
            if (this.Font == null) return;

            var w = this.ActualWidth;
            var h = this.ActualHeight;
            if (w <= 0 || h <= 0) return;
            if (!this.IsWidthSet(out _) || !this.IsHeightSet(out _)) return;

            // Gauge is always square. Use the smaller of width/height so a
            // rectangular Width/Height assignment doesn't distort the dial.
            var side = w < h ? w : h;
            this.EnsureSurfaces(side);

            using (var gfx = Graphics.FromImage(this._bmp)) {
                gfx.Clear();
                this.PaintBackground(gfx, side);
                this.PaintPointer(gfx, side / 2, side / 2, side);
            }

            if (this._cachedImage == null) {
                this._cachedImage = BitmapImage.FromGraphics(Graphics.FromImage(this._bmp));
            }
            dc.DrawImage(this._cachedImage, 0, 0);
        }

        // Allocates or grows the backing bitmaps to fit a `side`×`side` square.
        private void EnsureSurfaces(int side) {
            if (this._bmp != null && this._cachedSide == side) return;

            this.ReleaseSurfaces();
            this._bmp = new System.Drawing.Bitmap(side, side);
            this._backgroundImg = new System.Drawing.Bitmap(side, side);
            this._cachedSide = side;
            this._backgroundDirty = true;
        }

        // Paints (or reuses) the static dial face onto `gfx`. Re-renders the
        // background bitmap only when a property change marked it dirty.
        private void PaintBackground(Graphics gfx, int side) {
            using (var bg = ToSdBrush(this._backColor))
                gfx.FillRectangle(bg, 0, 0, side, side);

            if (this._backgroundDirty) {
                this.RedrawBackgroundImg(side);
                this._backgroundDirty = false;
            }

            gfx.DrawImage(this._backgroundImg, 0, 0);
        }

        private void RedrawBackgroundImg(int side) {
            using var g = Graphics.FromImage(this._backgroundImg);

            using (var bg = ToSdBrush(this._backColor))
                g.FillRectangle(bg, 0, 0, side, side);

            // Outer ring: a slightly larger transparent-color ellipse hides the
            // background corners. Optional via EnableTransparentBackground.
            if (this._enableTransparentBackground) {
                var gg = side / 60;
                using var tb = ToSdBrush(this._backColor);
                g.FillEllipse(tb, -gg, -gg, side + gg * 2, side + gg * 2);
            }

            // Dial face.
            using (var dialBrush = ToSdBrush(this._dialColor))
                g.FillEllipse(dialBrush, 0, 0, side, side);

            // Dark rim outline.
            using (var rimPen = ToSdPen(RimColor, 1))
                g.DrawEllipse(rimPen, 0, 0, side, side);

            this.DrawCalibration(g, side);

            if (this.EnableThreshold) {
                this.DrawThresholdRing(g, side);
            }

            if (this.EnableDigitalNumber) {
                this.DrawDigitalReadout(g, side);
            }

            this.DrawDialLabel(g, side);
        }

        private void DrawThresholdRing(Graphics g, int side) {
            var gap = (int)(side * 0.01f);
            var ringRect = new Rectangle(gap, gap, side - gap * 2, side - gap * 2);

            using (var rimPen = ToSdPen(ThresholdRimColor, side / 40))
                this.DrawArc(g, rimPen, ringRect, FromAngleDeg, 270);

            // Threshold band centered on the recommended value.
            var valuePct = 100 * (this._recommendedValue - this._minValue) / (this._maxValue - this._minValue);
            var valueAngle = (ToAngleDeg - FromAngleDeg) * valuePct / 100 + FromAngleDeg;
            var startAngle = valueAngle - 270 * this._threshold / 200;
            if (startAngle <= FromAngleDeg) startAngle = FromAngleDeg;
            var sweep = 270 * this._threshold / 100;
            if (startAngle + sweep > ToAngleDeg) sweep = ToAngleDeg - startAngle;

            using (var threshPen = ToSdPen(ThresholdMarkColor, side / 50))
                this.DrawArc(g, threshPen, ringRect, startAngle, sweep);
        }

        private void DrawDigitalReadout(Graphics g, int side) {
            // Rounded rectangle behind the digits. The two rects are the visual
            // panel and the slightly inset region the digits actually draw into.
            var panelRect = new Rectangle(
                (int)(side / 2f - side / 5f),
                (int)(side / 1.2f),
                (int)(side / 2.5f),
                (int)(side / 9f));
            var digitRect = new Rectangle(
                (int)(side / 2 - side / 7),
                (int)(side / 1.18),
                (int)(side / 4),
                (int)(side / 12));

            using (var panelBrush = ToSdBrush(DigitalPanelColor))
                g.FillRectangle(panelBrush, panelRect.X, panelRect.Y, panelRect.Width, panelRect.Height);

            this.DrawSevenSegmentNumber(g, this._currentValue, digitRect);
        }

        private void DrawDialLabel(Graphics g, int side) {
            if (this._dialText.Length == 0) return;

            var textSize = g.MeasureString(this._dialText, this.Font);
            var rect = new RectangleF(side / 2 - textSize.Width / 2,
                (int)(side / 1.5),
                textSize.Width, textSize.Height);

            using var brush = ToSdBrush(this._foreColor);
            g.DrawString(this._dialText, this.Font, brush, rect);
        }

        // Pointer needle + glossy half + center cap.
        private void PaintPointer(Graphics g, int cx, int cy, int side) {
            var radius = side / 2 - side * PointerRadiusFactor;
            var baseR = side * PointerBaseFactor;

            var valuePct = 100 * (this._currentValue - this._minValue) / (this._maxValue - this._minValue);
            var valueDeg = (ToAngleDeg - FromAngleDeg) * valuePct / 100 + FromAngleDeg;

            // Needle polygon: tip — right base — pivot — left base — slightly off-tip.
            var pts = new PointF[5];
            var ang = DegToRad(valueDeg);
            pts[0].X = cx + (float)(radius * System.Math.Cos(ang));
            pts[0].Y = cy + (float)(radius * System.Math.Sin(ang));
            pts[4].X = cx + (float)(radius * System.Math.Cos(ang - PointerTipOffset));
            pts[4].Y = cy + (float)(radius * System.Math.Sin(ang - PointerTipOffset));

            var rightAng = DegToRad(valueDeg + PointerSpread);
            pts[1].X = cx + (float)(baseR * System.Math.Cos(rightAng));
            pts[1].Y = cy + (float)(baseR * System.Math.Sin(rightAng));

            pts[2].X = cx;
            pts[2].Y = cy;

            var leftAng = DegToRad(valueDeg - PointerSpread);
            pts[3].X = cx + (float)(baseR * System.Math.Cos(leftAng));
            pts[3].Y = cy + (float)(baseR * System.Math.Sin(leftAng));

            using (var pointerBrush = ToSdBrush(PointerColor))
                FillPolygon(g, pointerBrush, pts);

            // Glossy half: triangle from tip to pivot via the +PointerSpread edge.
            var shine = new PointF[3];
            shine[0].X = pts[0].X;
            shine[0].Y = pts[0].Y;
            shine[1].X = pts[1].X;
            shine[1].Y = pts[1].Y;
            shine[2].X = cx;
            shine[2].Y = cy;
            using (var rimBrush = ToSdBrush(RimColor))
                FillPolygon(g, rimBrush, shine);

            this.DrawCenterCap(g, cx, cy, side);
        }

        private void DrawCenterCap(Graphics g, int cx, int cy, int side) {
            var outer = (float)side / 5;
            using (var brush = ToSdBrush(this._dialColor))
                g.FillEllipse(brush, (int)(cx - outer / 2), (int)(cy - outer / 2), (int)outer, (int)outer);

            var inner = (float)side / 7;
            using (var brush = ToSdBrush(RimColor))
                g.FillEllipse(brush, (int)(cx - inner / 2), (int)(cy - inner / 2), (int)inner, (int)inner);
        }

        // Draws ruler tick marks and numeric labels around the arc.
        private void DrawCalibration(Graphics g, int side) {
            var noOfParts = this._noOfDivisions + 1;
            var noOfIntermediates = this._noOfSubDivisions;
            var cx = side / 2;
            var cy = side / 2;
            var currentAngle = DegToRad(FromAngleDeg);
            var gap = (int)(side * 0.01f);
            var shift = (float)side / 25;
            var radius = (side - gap * 2) / 2f - gap * 5;

            var totalAngle = ToAngleDeg - FromAngleDeg;
            var incr = DegToRad(totalAngle / ((noOfParts - 1) * (noOfIntermediates + 1)));

            using var thickPen = ToSdPen(this._foreColor, side / 50);
            using var thinPen = ToSdPen(this._foreColor, side / 100);
            using var stringBrush = ToSdBrush(this._foreColor);

            var rulerValue = this._minValue;
            for (var i = 0; i <= noOfParts; i++) {
                var x0 = (int)(cx + radius * System.Math.Cos(currentAngle));
                var y0 = (int)(cy + radius * System.Math.Sin(currentAngle));
                var x1 = (int)(cx + (radius - side / 20f) * System.Math.Cos(currentAngle));
                var y1 = (int)(cy + (radius - side / 20f) * System.Math.Sin(currentAngle));
                g.DrawLine(thickPen, x0, y0, x1, y1);

                var tx = (float)(cx + (radius - side / 10f) * System.Math.Cos(currentAngle));
                var ty = (float)(cy - shift + (radius - side / 10f) * System.Math.Sin(currentAngle));

                var label = rulerValue.ToString();
                this.Font.ComputeTextInRect(label, out var labelW, out var labelH);
                g.DrawString(label, this.Font, stringBrush, tx - labelW / 2, ty + labelH / 2);

                rulerValue += (this._maxValue - this._minValue) / (noOfParts - 1);
                rulerValue = (float)System.Math.Round(rulerValue);

                if (i == noOfParts - 1) break;

                // Sub-division tick marks between major ticks.
                for (var j = 0; j <= noOfIntermediates; j++) {
                    currentAngle += incr;
                    x0 = (int)(cx + radius * System.Math.Cos(currentAngle));
                    y0 = (int)(cy + radius * System.Math.Sin(currentAngle));
                    x1 = (int)(cx + (radius - side / 50f) * System.Math.Cos(currentAngle));
                    y1 = (int)(cy + (radius - side / 50f) * System.Math.Sin(currentAngle));
                    g.DrawLine(thinPen, x0, y0, x1, y1);
                }
            }
        }

        // Pixel-spaced rasterized arc (TinyCLR Graphics doesn't expose DrawArc).
        private void DrawArc(Graphics g, System.Drawing.Pen pen, Rectangle rect, double startDeg, double sweepDeg) {
            var startRad = DegToRad((float)startDeg);
            var endRad = DegToRad((float)sweepDeg);
            var r = rect.Width / 2;
            var w = (int)pen.Width;

            using var solid = new System.Drawing.SolidBrush(pen.Color);
            for (var t = startRad; t < endRad; t += 0.05f) {
                var ax = rect.X + (int)(r + System.Math.Cos(t) * r);
                var ay = rect.Y + (int)(r + System.Math.Sin(t) * r);
                g.FillRectangle(solid, ax, ay, w, w);
            }
        }

        private static void FillPolygon(Graphics g, System.Drawing.Brush brush, PointF[] points) {
            if (points.Length <= 1) return;
            using var pen = new System.Drawing.Pen(brush);
            for (var i = 0; i < points.Length - 1; i++) {
                g.DrawLine(pen, (int)points[i].X, (int)points[i].Y, (int)points[i + 1].X, (int)points[i + 1].Y);
            }
        }

        // --- seven-segment digit rendering -----------------------------------
        //
        // Each digit is drawn as 7 segments (A=top, B=top-right, C=bottom-right,
        // D=bottom, E=bottom-left, F=top-left, G=middle). Segment shapes are
        // static in a unit grid; only the position and scale change per digit.

        // Bits: 1=A, 2=B, 4=C, 8=D, 16=E, 32=F, 64=G.
        private static readonly int[] DigitSegmentBits = new int[] {
            0b0111111, // 0: A B C D E F
            0b0000110, // 1: B C
            0b1011011, // 2: A B D E G
            0b1001111, // 3: A B C D G
            0b1100110, // 4: B C F G
            0b1101101, // 5: A C D F G
            0b1111101, // 6: A C D E F G
            0b0000111, // 7: A B C
            0b1111111, // 8: all
            0b1101111, // 9: A B C D F G
        };
        private const int MinusSignBits = 0b1000000;     // G only

        // Each segment is a closed polygon (first point == last) in unit
        // coordinates: X is 0..12, Y is 0..15. Multiplied by per-digit
        // width/12 and height/15 at draw time.
        private static readonly float[][] SegmentXY = new float[][] {
            // A (top horizontal)
            new float[] { 2.8f, 1f,  10f, 1f,  8.8f, 2f,  3.8f, 2f,  2.8f, 1f },
            // B (top right vertical)
            new float[] { 10f, 1.4f,  9.3f, 6.8f,  8.4f, 6.4f,  9f, 2.2f,  10f, 1.4f },
            // C (bottom right vertical)
            new float[] { 9.2f, 7.2f,  8.7f, 12.7f,  7.6f, 11.9f,  8.2f, 7.7f,  9.2f, 7.2f },
            // D (bottom horizontal)
            new float[] { 7.4f, 12.1f,  8.4f, 13f,  1.3f, 13f,  2.2f, 12.1f,  7.4f, 12.1f },
            // E (bottom left vertical)
            new float[] { 2.2f, 11.8f,  1f, 12.7f,  1.7f, 7.2f,  2.8f, 7.7f,  2.2f, 11.8f },
            // F (top left vertical)
            new float[] { 3f, 6.4f,  1.8f, 6.8f,  2.6f, 1.3f,  3.6f, 2.2f,  3f, 6.4f },
            // G (middle horizontal — 7 points instead of 5 because it has flat top/bottom)
            new float[] { 2f, 7f,  3.1f, 6.5f,  8.3f, 6.5f,  9f, 7f,  8.2f, 7.5f,  2.9f, 7.5f,  2f, 7f },
        };

        private void DrawSevenSegmentNumber(Graphics g, float number, Rectangle rect) {
            var formatted = number.ToString("n0");
            var padded = PadLeft(formatted, ((int)this._maxValue).ToString().Length, '0');

            var digitH = rect.Height;
            var digitW = digitH * 10 / 13;

            float xCursor = rect.X;
            if (number < 0) xCursor -= digitW / 17f;

            using var outlinePen = ToSdPen(PointerColor, 1);
            using var fillBrush = ToSdBrush(this._dialColor);

            var chars = padded.ToCharArray();
            var buffer5 = new PointF[5];
            var buffer7 = new PointF[7];
            for (var i = 0; i < chars.Length; i++) {
                var c = chars[i];
                if (c == '.') {
                    xCursor += 2 * digitW / 250f;
                    continue;
                }

                var dpFollows = i + 1 < chars.Length && chars[i + 1] == '.';
                var digitValue = c == '-' ? -1 : (int)(c - '0');

                this.DrawSingleDigit(g, outlinePen, fillBrush, digitValue, xCursor, rect.Y, digitW, digitH, dpFollows, buffer5, buffer7);
                xCursor += 15 * digitW / 250f;
            }
        }

        // Renders one 7-segment digit. `bufferA` and `bufferG` are caller-owned
        // PointF arrays we reuse for the 5-point and 7-point polygons so the
        // hot path doesn't allocate.
        private void DrawSingleDigit(Graphics g, System.Drawing.Pen outlinePen, System.Drawing.Brush fillBrush,
                                     int digit, float originX, float originY, float w, float h,
                                     bool decimalPoint, PointF[] bufferA, PointF[] bufferG) {
            var bits = digit == -1 ? MinusSignBits : DigitSegmentBits[digit];

            for (var seg = 0; seg < SegmentXY.Length; seg++) {
                var rel = SegmentXY[seg];
                var poly = (seg == 6) ? bufferG : bufferA;        // segment G has 7 points
                var pointCount = rel.Length / 2;
                for (var p = 0; p < pointCount; p++) {
                    poly[p].X = originX + rel[p * 2] * w / 12f;
                    poly[p].Y = originY + rel[p * 2 + 1] * h / 15f;
                }
                // Outline every segment so the LCD style is visible even on
                // segments the digit doesn't light up.
                FillPolygon(g, outlinePen.Brush, poly);

                if ((bits & (1 << seg)) != 0) {
                    FillPolygon(g, fillBrush, poly);
                }
            }

            if (decimalPoint) {
                g.FillEllipse(fillBrush,
                    (int)(originX + 10f * w / 12f),
                    (int)(originY + 12f * h / 15f),
                    (int)(w / 7),
                    (int)(w / 7));
            }
        }

        // --- helpers ---------------------------------------------------------

        private static float DegToRad(float deg) => deg * (float)System.Math.PI / 180f;

        // TinyCLR mscorlib doesn't have string.PadLeft.
        private static string PadLeft(string value, int totalWidth, char pad) {
            var missing = totalWidth - value.Length;
            if (missing <= 0) return value;
            var result = "";
            for (var i = 0; i < missing; i++) result += pad;
            return result + value;
        }

        private static System.Drawing.Color ToSd(MediaColor c) =>
            System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);

        private static System.Drawing.SolidBrush ToSdBrush(MediaColor c) =>
            new System.Drawing.SolidBrush(ToSd(c));

        private static System.Drawing.Pen ToSdPen(MediaColor c, int thickness) =>
            new System.Drawing.Pen(ToSd(c), thickness);
    }
}
