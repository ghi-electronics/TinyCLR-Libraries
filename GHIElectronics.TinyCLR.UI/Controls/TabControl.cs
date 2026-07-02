using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A tabbed container: a strip of tab headers over a body that shows the selected tab's content.
    /// Add tabs with <see cref="AddTab"/>; tapping a header selects it.</summary>
    public class TabControl : Control {
        private readonly ArrayList _headers = new ArrayList();  // string
        private readonly ArrayList _contents = new ArrayList(); // UIElement (may be null)
        private int _selectedIndex;
        private int _headerHeight = 24;

        /// <summary>Fill of an unselected tab header.</summary>
        public Color TabColor { get; set; } = Color.FromRgb(0xE0, 0xE0, 0xE4);
        /// <summary>Fill of the selected tab header (and the body).</summary>
        public Color SelectedTabColor { get; set; } = Colors.White;
        /// <summary>Colour of the header text and separators.</summary>
        public Color HeaderColor { get; set; } = Colors.Black;

        /// <summary>Height in pixels of the tab-header strip.</summary>
        public int HeaderHeight {
            get => this._headerHeight;
            set { if (value > 0) { this._headerHeight = value; this.InvalidateMeasure(); } }
        }

        /// <summary>The index of the visible tab. May be set before tabs are added (it is clamped when rendering).</summary>
        public int SelectedIndex {
            get => this._selectedIndex;
            set {
                if (value >= 0 && this._selectedIndex != value) {
                    this._selectedIndex = value;
                    this.InvalidateArrange();
                    this.Invalidate();
                }
            }
        }

        // The selected index clamped to the current tab count (the setter allows values set before tabs exist).
        private int SafeSelectedIndex => (this._selectedIndex < this._contents.Count) ? this._selectedIndex : this._contents.Count - 1;

        /// <summary>Adds a tab with the given header text and content element.</summary>
        public void AddTab(string header, UIElement content) {
            this._headers.Add(header ?? string.Empty);
            this._contents.Add(content);
            if (content != null) {
                this.LogicalChildren.Add(content);
            }

            this.InvalidateMeasure();
        }

        /// <summary>Measures every tab's content against the body area.</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            var bodyH = availableHeight - this._headerHeight;
            if (bodyH < 0) {
                bodyH = 0;
            }

            for (var i = 0; i < this._contents.Count; i++) {
                ((UIElement)this._contents[i])?.Measure(availableWidth, bodyH);
            }

            desiredWidth = availableWidth < Media.Constants.MaxExtent ? availableWidth : 0;
            desiredHeight = availableHeight < Media.Constants.MaxExtent ? availableHeight : this._headerHeight;
        }

        /// <summary>Arranges the selected tab's content in the body; collapses the rest.</summary>
        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            var bodyH = arrangeHeight - this._headerHeight;
            if (bodyH < 0) {
                bodyH = 0;
            }

            var sel = this.SafeSelectedIndex;
            for (var i = 0; i < this._contents.Count; i++) {
                var c = (UIElement)this._contents[i];
                if (c == null) {
                    continue;
                }

                if (i == sel) {
                    c.Arrange(0, this._headerHeight, arrangeWidth, bodyH);
                }
                else {
                    c.Arrange(0, this._headerHeight, 0, 0);
                }
            }
        }

        /// <summary>Selects the tapped tab header.</summary>
        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            e.GetPosition(this, 0, out var x, out var y);
            var n = this._headers.Count;
            if (y >= 0 && y < this._headerHeight && n > 0) {
                var tabW = this._renderWidth / n;
                if (tabW > 0) {
                    var idx = x / tabW;
                    if (idx >= 0 && idx < n) {
                        this.SelectedIndex = idx;
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>Draws the tab-header strip.</summary>
        public override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            var w = this._renderWidth;
            var n = this._headers.Count;
            if (n == 0) {
                return;
            }

            var tabW = w / n;
            if (tabW < 1) {
                tabW = 1;
            }

            var pen = new Media.Pen(Color.FromRgb(0x80, 0x80, 0x86), 1);
            var sel = this.SafeSelectedIndex;
            for (var i = 0; i < n; i++) {
                var x = i * tabW;
                var selected = i == sel;
                dc.DrawRectangle(new SolidColorBrush(selected ? this.SelectedTabColor : this.TabColor), pen, x, 0, tabW, this._headerHeight);

                if (this.Font != null) {
                    var txt = (string)this._headers[i];
                    dc.DrawText(ref txt, this.Font, this.HeaderColor, x + 3, (this._headerHeight - this.Font.Height) / 2, tabW - 6, this.Font.Height, TextAlignment.Center, TextTrimming.CharacterEllipsis);
                }
            }

            dc.DrawLine(pen, 0, this._headerHeight, w - 1, this._headerHeight);
        }
    }
}
