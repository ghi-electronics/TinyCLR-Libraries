using System;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A header with an expand/collapse chevron; tapping the header shows or hides the single child content.</summary>
    public class Expander : ContentControl {
        private string _header = string.Empty;
        private bool _isExpanded = true;

        /// <summary>The header text (always visible). Uses the inherited <see cref="Control.Font"/>.</summary>
        public string Header {
            get => this._header;
            set { this._header = value ?? string.Empty; this.Invalidate(); }
        }

        /// <summary>The colour of the header text and chevron.</summary>
        public Color HeaderColor { get; set; } = Colors.Black;

        /// <summary>Whether the content is shown. Tapping the header toggles this.</summary>
        public bool IsExpanded {
            get => this._isExpanded;
            set {
                if (this._isExpanded != value) {
                    this._isExpanded = value;
                    this.InvalidateMeasure();
                    this.Invalidate();
                }
            }
        }

        private int HeaderHeight => (this.Font != null) ? this.Font.Height + 6 : 20;

        /// <summary>Toggles expansion when the header row is tapped.</summary>
        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            base.OnTouchUp(e); // raise the public TouchUp event for user/designer handlers

            e.GetPosition(this, 0, out _, out var y);
            if (y >= 0 && y < this.HeaderHeight) {
                this.IsExpanded = !this.IsExpanded;
                e.Handled = true;
            }
        }

        /// <summary>Measures the header plus (when expanded) the child.</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            var hh = this.HeaderHeight;
            var child = this.Child;
            if (child != null && this._isExpanded) {
                child.Measure(availableWidth, availableHeight - hh);
                child.GetDesiredSize(out desiredWidth, out desiredHeight);
                desiredHeight += hh;
            }
            else {
                desiredWidth = 0;
                desiredHeight = hh;
            }
        }

        /// <summary>Arranges the child below the header (zero size when collapsed).</summary>
        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            var hh = this.HeaderHeight;
            var child = this.Child;
            if (child != null) {
                if (this._isExpanded) {
                    child.Arrange(0, hh, arrangeWidth, arrangeHeight - hh);
                }
                else {
                    child.Arrange(0, hh, 0, 0);
                }
            }
        }

        /// <summary>Draws the chevron + header text + a separator line under the header.</summary>
        public override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            var w = this._renderWidth;
            var hh = this.HeaderHeight;
            var pen = new Media.Pen(this.HeaderColor, 1);

            var cx = 8;
            var cy = hh / 2;
            if (this._isExpanded) {
                // Down chevron.
                dc.DrawLine(pen, cx - 3, cy - 2, cx, cy + 2);
                dc.DrawLine(pen, cx, cy + 2, cx + 3, cy - 2);
            }
            else {
                // Right chevron.
                dc.DrawLine(pen, cx - 2, cy - 3, cx + 2, cy);
                dc.DrawLine(pen, cx + 2, cy, cx - 2, cy + 3);
            }

            if (this.Font != null && this._header.Length > 0) {
                var txt = this._header;
                dc.DrawText(ref txt, this.Font, this.HeaderColor, 18, 0, w - 20, hh, TextAlignment.Left, TextTrimming.CharacterEllipsis);
            }

            dc.DrawLine(pen, 0, hh - 1, w - 1, hh - 1);
        }
    }
}
