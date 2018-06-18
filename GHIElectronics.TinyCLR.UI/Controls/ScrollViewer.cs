using System;
using System.Diagnostics;
using GHIElectronics.TinyCLR.UI.Input;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class ScrollViewer : ContentControl {
        public ScrollViewer() {
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Stretch;
        }

        public event ScrollChangedEventHandler ScrollChanged {
            add {
                VerifyAccess();

                this._scrollChanged += value;
            }

            remove {
                VerifyAccess();

                this._scrollChanged -= value;
            }
        }

        public int HorizontalOffset {
            get => this._horizontalOffset;

            set {
                VerifyAccess();

                if (value < 0) {
                    value = 0;
                }
                else if ((this._flags & Flags.NeverArranged) == 0 && value > this._scrollableWidth) {
                    value = this._scrollableWidth;
                }

                if (this._horizontalOffset != value) {
                    this._horizontalOffset = value;
                    InvalidateArrange();
                }
            }
        }

        public int VerticalOffset {
            get => this._verticalOffset;

            set {
                VerifyAccess();

                if (value < 0) {
                    value = 0;
                }
                else if ((this._flags & Flags.NeverArranged) == 0 && value > this._scrollableHeight) {
                    value = this._scrollableHeight;
                }

                if (this._verticalOffset != value) {
                    this._verticalOffset = value;
                    InvalidateArrange();
                }
            }
        }

        public int ExtentHeight => this._extentHeight;

        public int ExtentWidth => this._extentWidth;

        public int LineWidth {
            get => this._lineWidth;

            set {
                VerifyAccess();

                if (value < 0) {
                    throw new System.ArgumentOutOfRangeException("LineWidth");
                }

                this._lineWidth = value;
            }
        }

        public int LineHeight {
            get => this._lineHeight;

            set {
                VerifyAccess();

                if (value < 0) {
                    throw new System.ArgumentOutOfRangeException("LineHeight");
                }

                this._lineHeight = value;
            }
        }

        public ScrollingStyle ScrollingStyle {
            get => this._scrollingStyle;

            set {
                VerifyAccess();

                if (value < ScrollingStyle.First || value > ScrollingStyle.Last) {
                    throw new ArgumentOutOfRangeException("ScrollingStyle", "Invalid Enum");
                }

                this._scrollingStyle = value;
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            var child = this.Child;
            if (child != null && child.Visibility != Visibility.Collapsed) {
                child.Measure((this.HorizontalAlignment == HorizontalAlignment.Stretch) ? Media.Constants.MaxExtent : availableWidth, (this.VerticalAlignment == VerticalAlignment.Stretch) ? Media.Constants.MaxExtent : availableHeight);
                child.GetDesiredSize(out desiredWidth, out desiredHeight);
                this._extentHeight = child._unclippedHeight;
                this._extentWidth = child._unclippedWidth;
            }
            else {
                desiredWidth = desiredHeight = 0;
                this._extentHeight = this._extentWidth = 0;
            }
        }

        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            var child = this.Child;
            if (child != null) {
                // Clip scroll-offset if necessary
                //
                this._scrollableWidth = System.Math.Max(0, this.ExtentWidth - arrangeWidth);
                this._scrollableHeight = System.Math.Max(0, this.ExtentHeight - arrangeHeight);
                this._horizontalOffset = System.Math.Min(this._horizontalOffset, this._scrollableWidth);
                this._verticalOffset = System.Math.Min(this._verticalOffset, this._scrollableHeight);

                Debug.Assert(this._horizontalOffset >= 0);
                Debug.Assert(this._verticalOffset >= 0);

                child.Arrange(-this._horizontalOffset,
                               -this._verticalOffset,
                               System.Math.Max(arrangeWidth, this.ExtentWidth),
                               System.Math.Max(arrangeHeight, this.ExtentHeight));
            }
            else {
                this._horizontalOffset = this._verticalOffset = 0;
            }

            InvalidateScrollInfo();
        }

        public void LineDown() => this.VerticalOffset += this._lineHeight;

        public void LineLeft() => this.HorizontalOffset -= this._lineWidth;

        public void LineRight() => this.HorizontalOffset += this._lineWidth;

        public void LineUp() => this.VerticalOffset -= this._lineHeight;

        public void PageDown() => this.VerticalOffset += this.ActualHeight;

        public void PageLeft() => this.HorizontalOffset -= this.ActualWidth;

        public void PageRight() => this.HorizontalOffset += this.ActualWidth;

        public void PageUp() => this.VerticalOffset -= this.ActualHeight;

        private void InvalidateScrollInfo() {
            if (this._scrollChanged != null) {
                var deltaX = this._horizontalOffset - this._previousHorizontalOffset;
                var deltaY = this._verticalOffset - this._previousVerticalOffset;
                this._scrollChanged(this, new ScrollChangedEventArgs(this._horizontalOffset, this._verticalOffset, deltaX, deltaY));
            }

            this._previousHorizontalOffset = this._horizontalOffset;
            this._previousVerticalOffset = this._verticalOffset;
        }

        protected override void OnButtonDown(GHIElectronics.TinyCLR.UI.Input.ButtonEventArgs e) {
            switch (e.Button) {
                case HardwareButton.Up:
                    if (this._scrollingStyle == ScrollingStyle.LineByLine) LineUp(); else PageUp();
                    break;
                case HardwareButton.Down:
                    if (this._scrollingStyle == ScrollingStyle.LineByLine) LineDown(); else PageDown();
                    break;
                case HardwareButton.Left:
                    if (this._scrollingStyle == ScrollingStyle.LineByLine) LineLeft(); else PageLeft();
                    break;
                case HardwareButton.Right:
                    if (this._scrollingStyle == ScrollingStyle.LineByLine) LineRight(); else PageRight();
                    break;
                default:
                    return;
            }

            if (this._previousHorizontalOffset != this._horizontalOffset || this._previousVerticalOffset != this._verticalOffset) {
                e.Handled = true;
            }
        }

        private int _previousHorizontalOffset;
        private int _previousVerticalOffset;
        private int _horizontalOffset;
        private int _verticalOffset;
        private int _extentWidth;
        private int _extentHeight;
        private int _scrollableWidth;
        private int _scrollableHeight;

        private int _lineHeight = 1;
        private int _lineWidth = 1;

        private ScrollingStyle _scrollingStyle = ScrollingStyle.LineByLine;

        private ScrollChangedEventHandler _scrollChanged;
    }
}


