using System;
using System.Drawing;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>
    /// Horizontal or vertical value slider with optional tick marks and snap-to
    /// intervals. The knob is rendered with the shared Scale9 Button bitmaps so
    /// it picks up the theme's surface styling automatically.
    /// </summary>
    public class Slider : ContentControl, IDisposable {
        // --- visual constants ------------------------------------------------

        /// <summary>
        /// Knob thickness across the track = trackBreadth / KnobBreadthRatio.
        /// Slightly less than 1 leaves a thin visible track on either side.
        /// </summary>
        private const double KnobBreadthRatio = 1.2;

        /// <summary>Tick mark length as a fraction of the perpendicular dimension.</summary>
        private const double TickLengthRatio = 0.05;

        // --- public API ------------------------------------------------------

        /// <summary>Represents the method that handles the slider's value-changed event.</summary>
        public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs args);

        /// <summary>Provides data for the slider's value-changed event.</summary>
        public sealed class ValueChangedEventArgs : EventArgs {
            /// <summary>Initializes a new instance of the <see cref="ValueChangedEventArgs"/> class.</summary>
            public ValueChangedEventArgs(double value) => this.Value = value;
            /// <summary>The new slider value.</summary>
            public double Value { get; }
        }

        /// <summary>Raised when the slider value changes.</summary>
        public event ValueChangedEventHandler ValueChanged;

        /// <summary>The opacity applied when rendering the knob.</summary>
        public ushort Alpha { get; set; } = Theme.DefaultAlpha;
        /// <summary>Corner radius in pixels for the Scale9-rendered knob.</summary>
        public int RadiusBorder { get; set; } = Theme.DefaultRadiusBorder;

        /// <summary>Initializes a new slider with default size.</summary>
        public Slider() : this(0, 0) { }

        /// <summary>Initializes a new slider with the given width and height.</summary>
        public Slider(int width, int height) {
            if (width > 0) this.Width = width;
            if (height > 0) this.Height = height;

            this._knobSize = 20;
            this._snapInterval = 10;
            this._tickInterval = 10;
            this._min = 0;
            this._max = 100;
            this._value = 0;

            this._bitmapKnobUp = Resources.LoadBitmapImage(Resources.BitmapResources.Button_Up);
            this._bitmapKnobDown = Resources.LoadBitmapImage(Resources.BitmapResources.Button_Down);
        }

        /// <summary>Whether the slider is laid out horizontally or vertically.</summary>
        public Orientation Orientation {
            get => this._orientation;
            set {
                if (this._orientation == value) return;
                this._orientation = value;
                this._metricsDirty = true;
                this.Invalidate();
            }
        }

        /// <summary>The minimum value the slider can represent.</summary>
        public double Minimum {
            get => this._min;
            set {
                if (this._min == value) return;
                this._min = value;
                this._metricsDirty = true;
                this.ClampValueAndInvalidate();
            }
        }

        /// <summary>The maximum value the slider can represent.</summary>
        public double Maximum {
            get => this._max;
            set {
                if (this._max == value) return;
                this._max = value;
                this._metricsDirty = true;
                this.ClampValueAndInvalidate();
            }
        }

        /// <summary>The current slider value, clamped between <see cref="Minimum"/> and <see cref="Maximum"/>.</summary>
        public double Value {
            get => this._value;
            set {
                if (value < this._min) value = this._min;
                if (value > this._max) value = this._max;
                if (this._value == value) return;

                this._value = value;
                this.ValueChanged?.Invoke(this, new ValueChangedEventArgs(value));
                this.Invalidate();
            }
        }

        /// <summary>Knob size along the slide axis (px).</summary>
        public int KnobSize {
            get => this._knobSize;
            set {
                if (this._knobSize == value) return;
                this._knobSize = value;
                this._metricsDirty = true;
                this.Invalidate();
            }
        }

        /// <summary>Number of tick mark intervals along the track. 0 disables ticks.</summary>
        public int TickInterval {
            get => this._tickInterval;
            set {
                if (value < 0) value = 0;
                if (this._tickInterval == value) return;
                this._tickInterval = value;
                this._metricsDirty = true;
                this.Invalidate();
            }
        }

        /// <summary>Number of snap stops along the track. 0 disables snap (continuous).</summary>
        public int SnapInterval {
            get => this._snapInterval;
            set {
                if (value < 0) value = 0;
                if (this._snapInterval == value) return;
                this._snapInterval = value;
                this._metricsDirty = true;
                this.Invalidate();
            }
        }

        /// <summary>Color used for the track line and tick marks.</summary>
        public Media.Color TrackColor {
            get => this._trackColor;
            set {
                this._trackColor = value;
                this._trackPen = new Media.Pen(value);
                this.Invalidate();
            }
        }

        // --- internal state --------------------------------------------------

        private readonly BitmapImage _bitmapKnobUp;
        private readonly BitmapImage _bitmapKnobDown;

        private Orientation _orientation = Orientation.Horizontal;
        private bool _dragging;
        private int _dragOriginX, _dragOriginY;
        private UIElement _previousCapture;

        private int _knobSize;
        private int _tickInterval;
        private int _snapInterval;
        private double _min;
        private double _max;
        private double _value;

        private Media.Color _trackColor = Colors.Black;
        private Media.Pen _trackPen = new Media.Pen(Colors.Black);

        // Cached metrics, recomputed lazily when _metricsDirty is set by a
        // property setter or layout change.
        private bool _metricsDirty = true;
        private int _cachedTrackLength;
        private int _cachedKnobBreadth;
        private double _cachedSnapSize;
        private double _cachedTickSize;
        private double _cachedPixelsPerValue;

        private bool _disposed;

        // --- rendering -------------------------------------------------------

        /// <summary>Renders the track, tick marks, and knob for the slider.</summary>
        public override void OnRender(DrawingContext dc) {
            var w = this.ActualWidth;
            var h = this.ActualHeight;
            if (w <= 0 || h <= 0) return;

            // Paint Background and focus ring from Control base.
            base.OnRender(dc);

            this.EnsureMetrics(w, h);

            if (this._orientation == Orientation.Horizontal) {
                this.RenderHorizontal(dc, w, h);
            }
            else {
                this.RenderVertical(dc, w, h);
            }
        }

        private void RenderHorizontal(DrawingContext dc, int w, int h) {
            var trackY = h / 2;
            var startX = this._cachedKnobBreadth / 2;
            var endX = startX + this._cachedTrackLength;

            dc.DrawLine(this._trackPen, startX, trackY, endX, trackY);

            if (this._tickInterval > 1) {
                var tickLen = (int)System.Math.Ceiling(h * TickLengthRatio);
                for (var i = 0; i <= this._tickInterval; i++) {
                    var tx = startX + (int)(i * this._cachedTickSize);
                    dc.DrawLine(this._trackPen, tx, 0, tx, tickLen);
                }
            }

            var knobX = this.ValueToPixel();
            var knobY = h - this.KnobThickness(h);
            var knobImg = this._dragging ? this._bitmapKnobDown : this._bitmapKnobUp;
            dc.Scale9Image(knobX, knobY, this._knobSize, this.KnobThickness(h),
                knobImg, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.Alpha);
        }

        private void RenderVertical(DrawingContext dc, int w, int h) {
            var trackX = w / 2;
            var startY = this._cachedKnobBreadth / 2;
            var endY = startY + this._cachedTrackLength;

            dc.DrawLine(this._trackPen, trackX, startY, trackX, endY);

            if (this._tickInterval > 1) {
                var tickLen = (int)System.Math.Ceiling(w * TickLengthRatio);
                for (var i = 0; i <= this._tickInterval; i++) {
                    var ty = startY + (int)(i * this._cachedTickSize);
                    dc.DrawLine(this._trackPen, 0, ty, tickLen, ty);
                }
            }

            var knobY = this.ValueToPixel();
            var knobX = w - this.KnobThickness(w);
            var knobImg = this._dragging ? this._bitmapKnobDown : this._bitmapKnobUp;
            dc.Scale9Image(knobX, knobY, this.KnobThickness(w), this._knobSize,
                knobImg, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.Alpha);
        }

        private int KnobThickness(int perpendicular) => (int)(perpendicular / KnobBreadthRatio);

        // Maps the current Value to the leading pixel of the knob on the slide axis.
        // Vertical orientation inverts (top = max, bottom = min) to match WPF / common
        // convention.
        private int ValueToPixel() {
            var range = this._max - this._min;
            if (range <= 0 || this._cachedPixelsPerValue == 0) return 0;

            var normalized = this._value - this._min;
            if (this._orientation == Orientation.Horizontal) {
                return (int)(normalized * this._cachedPixelsPerValue);
            }
            return (int)((range - normalized) * this._cachedPixelsPerValue);
        }

        // --- metrics caching -------------------------------------------------

        private void EnsureMetrics(int w, int h) {
            if (!this._metricsDirty && this._cachedTrackLength > 0) return;

            // Total length along the slide axis minus knob length so the knob
            // never overshoots either end.
            var axisLength = this._orientation == Orientation.Horizontal ? w : h;
            this._cachedTrackLength = axisLength - this._knobSize;
            if (this._cachedTrackLength < 1) this._cachedTrackLength = 1;

            this._cachedKnobBreadth = this._knobSize;

            this._cachedSnapSize = this._snapInterval > 0
                ? (double)this._cachedTrackLength / this._snapInterval
                : 0;
            this._cachedTickSize = this._tickInterval > 0
                ? (double)this._cachedTrackLength / this._tickInterval
                : 0;

            var range = this._max - this._min;
            this._cachedPixelsPerValue = range > 0 ? this._cachedTrackLength / range : 0;

            this._metricsDirty = false;
        }

        private void ClampValueAndInvalidate() {
            var v = this._value;
            if (v < this._min) v = this._min;
            if (v > this._max) v = this._max;
            if (v != this._value) {
                this._value = v;
                this.ValueChanged?.Invoke(this, new ValueChangedEventArgs(v));
            }
            this.Invalidate();
        }

        // --- input -----------------------------------------------------------

        /// <summary>Begins dragging the knob when the slider is touched.</summary>
        protected override void OnTouchDown(TouchEventArgs e) {
            if (!this.IsEnabled) return;

            this._dragOriginX = 0;
            this._dragOriginY = 0;
            this.PointToScreen(ref this._dragOriginX, ref this._dragOriginY);
            this._dragging = true;

            // Capture touch to this slider so the knob keeps following the finger for the whole gesture. Without
            // capture each move event hit-tests to whatever is under the finger, so the drag is lost the moment the
            // finger strays off the (thin) track — the routed touch events only bubble UP from the hit element, they
            // are never re-dispatched down to us. Remember whatever held capture before so we can restore it on release.
            this._previousCapture = TouchCapture.Captured;
            try {
                TouchCapture.Capture(this);
            }
            catch {
                this._previousCapture = null; // not attached to the window subtree — fall back to hit-test routing
            }

            // Tap-to-set: move the value to the touch point (this also seeds the drag from where the finger landed).
            this.UpdateValueFromTouch(e.Touches[0].X - this._dragOriginX, e.Touches[0].Y - this._dragOriginY);

            e.Handled = true;
            this.Invalidate();
        }

        /// <summary>Ends knob dragging when the touch is released.</summary>
        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this._dragging) return;
            this._dragging = false;

            // Release capture back to whoever held it before the drag (typically nothing / the window).
            if (TouchCapture.Captured == this) {
                try {
                    if (this._previousCapture != null) {
                        TouchCapture.Capture(this._previousCapture);
                    }
                    else {
                        TouchCapture.Capture(null, CaptureMode.None);
                    }
                }
                catch {
                    TouchCapture.Capture(null, CaptureMode.None);
                }
            }

            this._previousCapture = null;
            this.Invalidate();
        }

        /// <summary>Updates the value as the knob is dragged.</summary>
        protected override void OnTouchMove(TouchEventArgs e) {
            if (!this._dragging) return;

            var localX = e.Touches[0].X - this._dragOriginX;
            var localY = e.Touches[0].Y - this._dragOriginY;
            this.UpdateValueFromTouch(localX, localY);
        }

        private void UpdateValueFromTouch(int localX, int localY) {
            var w = this.ActualWidth;
            var h = this.ActualHeight;
            if (w <= 0 || h <= 0) return;
            this.EnsureMetrics(w, h);
            if (this._cachedPixelsPerValue == 0) return;

            int pos;
            if (this._orientation == Orientation.Horizontal) {
                pos = localX - this._cachedKnobBreadth / 2;
                pos = Clamp(pos, 0, this._cachedTrackLength);
                pos = this.ApplySnap(pos);
                this.Value = this._min + pos / this._cachedPixelsPerValue;
            }
            else {
                pos = localY - this._cachedKnobBreadth / 2;
                pos = Clamp(pos, 0, this._cachedTrackLength);
                pos = this.ApplySnap(pos);
                this.Value = this._max - pos / this._cachedPixelsPerValue;
            }
        }

        private int ApplySnap(int pixel) {
            if (this._cachedSnapSize <= 0) return pixel;
            var slot = (int)System.Math.Round(pixel / this._cachedSnapSize);
            return (int)(slot * this._cachedSnapSize);
        }

        private static int Clamp(int v, int lo, int hi) =>
            v < lo ? lo : v > hi ? hi : v;

        /// <summary>
        /// Hardware button support: Left/Right step a horizontal slider,
        /// Up/Down step a vertical slider. Step size is one snap interval, or
        /// 1% of the range when SnapInterval is 0.
        /// </summary>
        protected override void OnButtonDown(ButtonEventArgs e) {
            if (!this.IsEnabled) return;

            var step = this.GetKeyboardStep();
            var delta = 0.0;

            if (this._orientation == Orientation.Horizontal) {
                if (e.Button == HardwareButton.Right) delta = step;
                else if (e.Button == HardwareButton.Left) delta = -step;
                else return;
            }
            else {
                // Vertical: visually larger value is toward the top.
                if (e.Button == HardwareButton.Up) delta = step;
                else if (e.Button == HardwareButton.Down) delta = -step;
                else return;
            }

            this.Value = this._value + delta;
            e.Handled = true;
        }

        private double GetKeyboardStep() {
            var steps = this._snapInterval > 0 ? this._snapInterval : 100;
            var range = this._max - this._min;
            return range > 0 ? range / steps : 0;
        }

        // --- IDisposable -----------------------------------------------------

        /// <summary>Releases the knob bitmap resources used by the slider.</summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the knob bitmap resources used by the slider.</summary>
        protected virtual void Dispose(bool disposing) {
            if (this._disposed) return;

            if (disposing) {
                this._bitmapKnobUp?.graphics?.Dispose();
                this._bitmapKnobDown?.graphics?.Dispose();
            }

            this._disposed = true;
        }

        ~Slider() => this.Dispose(false);
    }
}
