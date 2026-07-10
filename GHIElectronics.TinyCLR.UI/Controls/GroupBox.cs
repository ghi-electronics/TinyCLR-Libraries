using System;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A titled frame around a single child: draws a labelled border with the header above the content.</summary>
    public class GroupBox : ContentControl {
        private const int Pad = 4;

        private string _header = string.Empty;

        /// <summary>The title shown above the frame. Uses the inherited <see cref="Control.Font"/>.</summary>
        public string Header {
            get => this._header;
            set { this._header = value ?? string.Empty; this.InvalidateMeasure(); }
        }

        /// <summary>The colour of the frame border.</summary>
        public Color BorderColor { get; set; } = Color.FromRgb(0x80, 0x80, 0x80);

        /// <summary>The colour of the header text.</summary>
        public Color HeaderColor { get; set; } = Colors.Black;

        private int HeaderHeight => (this.Font != null && this._header.Length > 0) ? this.Font.Height + 2 : Pad;

        /// <summary>Measures the child plus the header + frame padding.</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            var hh = this.HeaderHeight;
            var child = this.Child;
            if (child != null) {
                child.Measure(availableWidth - 2 * Pad, availableHeight - hh - 2 * Pad);
                child.GetDesiredSize(out desiredWidth, out desiredHeight);
                desiredWidth += 2 * Pad;
                desiredHeight += hh + 2 * Pad;
            }
            else {
                desiredWidth = 2 * Pad;
                desiredHeight = hh + 2 * Pad;
            }
        }

        /// <summary>Arranges the child inside the frame, below the header.</summary>
        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            var hh = this.HeaderHeight;
            var child = this.Child;
            if (child != null) {
                child.Arrange(Pad, hh + Pad, arrangeWidth - 2 * Pad, arrangeHeight - hh - 2 * Pad);
            }
        }

        /// <summary>Draws the frame border and the header text.</summary>
        public override void OnRender(DrawingContext dc) {
            base.OnRender(dc); // Background + focus visual

            var w = this._renderWidth;
            var h = this._renderHeight;
            var hh = this.HeaderHeight;

            var pen = new Media.Pen(this.BorderColor, 1);
            dc.DrawLine(pen, 0, hh, w - 1, hh);       // top (below the header)
            dc.DrawLine(pen, 0, h - 1, w - 1, h - 1); // bottom
            dc.DrawLine(pen, 0, hh, 0, h - 1);        // left
            dc.DrawLine(pen, w - 1, hh, w - 1, h - 1); // right

            if (this.Font != null && this._header.Length > 0) {
                var txt = this._header;
                dc.DrawText(ref txt, this.Font, this.HeaderColor, Pad, 0, w - 2 * Pad, hh, TextAlignment.Left, TextTrimming.CharacterEllipsis);
            }
        }
    }
}
