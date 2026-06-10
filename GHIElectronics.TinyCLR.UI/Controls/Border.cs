////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>Draws a border around its single child element.</summary>
    public class Border : ContentControl {
        /// <summary>Creates a new Border with a black, one-pixel border.</summary>
        public Border() {
            this._borderBrush = new SolidColorBrush(Colors.Black);

            this._borderLeft = this._borderTop = this._borderRight = this._borderBottom = 1;
        }

        /// <summary>The brush used to paint the border.</summary>
        public Media.Brush BorderBrush {
            get {
                VerifyAccess();

                return this._borderBrush;
            }

            set {
                VerifyAccess();

                this._borderBrush = value;
                Invalidate();
            }
        }

        /// <summary>Gets the border thickness on each side, in pixels.</summary>
        public void GetBorderThickness(out int left, out int top, out int right, out int bottom) {
            left = this._borderLeft;
            top = this._borderTop;
            right = this._borderRight;
            bottom = this._borderBottom;
        }

        /// <summary>Sets a uniform border thickness on all sides.</summary>
        public void SetBorderThickness(int length) =>
            // no need to verify access here as the next call will do it
            SetBorderThickness(length, length, length, length);

        /// <summary>Sets the border thickness for each side individually.</summary>
        public void SetBorderThickness(int left, int top, int right, int bottom) {
            VerifyAccess();

            // Negative values are not valid (same behavior as desktop WPF).
            if ((left < 0) || (right < 0) || (top < 0) || (bottom < 0)) {
                var errorMessage = "'" + left.ToString() + "," + top.ToString() + "," + right.ToString() + "," + bottom.ToString() + "' is not a valid value 'BorderThickness'";

                throw new ArgumentException(errorMessage);
            }

            this._borderLeft = left;
            this._borderTop = top;
            this._borderRight = right;
            this._borderBottom = bottom;
            InvalidateMeasure();
        }

        /// <summary>Arranges the child inside the border.</summary>
        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            var child = this.Child;
            if (child != null) {
                child.Arrange(this._borderLeft,
                              this._borderTop,
                              arrangeWidth - this._borderLeft - this._borderRight,
                              arrangeHeight - this._borderTop - this._borderBottom);
            }
        }

        /// <summary>Measures the child plus the border thickness.</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            var child = this.Child;
            if (child != null) {
                var horizontalBorder = this._borderLeft + this._borderRight;
                var verticalBorder = this._borderTop + this._borderBottom;

                child.Measure(availableWidth - horizontalBorder, availableHeight - verticalBorder);

                child.GetDesiredSize(out desiredWidth, out desiredHeight);
                desiredWidth += horizontalBorder;
                desiredHeight += verticalBorder;
            }
            else {
                desiredWidth = desiredHeight = 0;
            }
        }

        /// <summary>Draws the border and background.</summary>
        public override void OnRender(DrawingContext dc) {
            var width = this._renderWidth;
            var height = this._renderHeight;

            // Border
            //
            dc.DrawRectangle(this._borderBrush, null, 0, 0, width, height);

            // Background
            //
            if (this._background != null) {
                dc.DrawRectangle(this._background, null, this._borderLeft, this._borderTop,
                                                     width - this._borderLeft - this._borderRight,
                                                     height - this._borderTop - this._borderBottom);
            }
        }

        private Media.Brush _borderBrush;
        private int _borderLeft, _borderTop, _borderRight, _borderBottom;
    }
}


