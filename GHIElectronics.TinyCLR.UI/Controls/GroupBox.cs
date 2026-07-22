using System;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A titled frame around a single child: draws a labelled border with the header sitting ON the top
    /// border line (WinForms/WPF "fieldset" style — the border is broken by a gap around the header text).</summary>
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

        /// <summary>Draws the fieldset frame: the top border runs through the vertical middle of the header text,
        /// broken by a gap around it (WinForms/WPF style).</summary>
        public override void OnRender(DrawingContext dc) {
            base.OnRender(dc); // Background + focus visual

            var w = this._renderWidth;
            var h = this._renderHeight;
            var hasHeader = this.Font != null && this._header.Length > 0;
            var lineY = hasHeader ? this.Font.Height / 2 : 0; // top border passes through the header's vertical middle

            var pen = new Media.Pen(this.BorderColor, 1);
            dc.DrawLine(pen, 0, h - 1, w - 1, h - 1);  // bottom
            dc.DrawLine(pen, 0, lineY, 0, h - 1);      // left
            dc.DrawLine(pen, w - 1, lineY, w - 1, h - 1); // right

            if (hasHeader) {
                // Top border with a GAP around the header text: a short stub, the text, then the line resumes.
                var inset = Pad + 4;                                   // header text x-offset
                this.Font.ComputeExtent(this._header, out var textW, out _);
                var gapStart = inset - 3;
                var gapEnd = inset + textW + 3;
                if (gapStart > 1) dc.DrawLine(pen, 0, lineY, gapStart, lineY);        // left stub
                if (gapEnd < w - 1) dc.DrawLine(pen, gapEnd, lineY, w - 1, lineY);    // resume to the right edge

                var txt = this._header;
                dc.DrawText(ref txt, this.Font, this.HeaderColor, inset, 0, w - inset - Pad, this.Font.Height, TextAlignment.Left, TextTrimming.CharacterEllipsis);
            }
            else {
                dc.DrawLine(pen, 0, lineY, w - 1, lineY); // no header -> full top line
            }
        }
    }
}
