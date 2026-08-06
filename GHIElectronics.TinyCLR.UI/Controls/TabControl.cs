using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A tabbed container: a strip of tab headers over a body that shows the selected tab's content.
    /// Add tabs with <see cref="AddTab(string, UIElement)"/>; tapping a header selects it. A header is either a plain text label or, like
    /// WPF's <c>TabItem.Header</c>, any UIElement (an Image, a StackPanel of icon + text, …).</summary>
    public class TabControl : Control {
        private readonly ArrayList _headerStrings = new ArrayList();  // string header (null when the tab uses a UIElement header)
        private readonly ArrayList _headerElements = new ArrayList(); // UIElement header (null when the tab uses a string header)
        private readonly ArrayList _contents = new ArrayList();       // UIElement (may be null)
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

        /// <summary>Adds a tab with a plain text header and content element.</summary>
        public void AddTab(string header, UIElement content) {
            this._headerStrings.Add(header ?? string.Empty);
            this._headerElements.Add(null);
            this._contents.Add(content);
            if (content != null) {
                this.LogicalChildren.Add(content);
            }

            this.InvalidateMeasure();
        }

        /// <summary>Adds a tab whose header is an arbitrary UIElement — WPF's <c>TabItem.Header</c> model. Pass an
        /// <see cref="Image"/>, a <see cref="StackPanel"/> of icon + text, or any control; it is measured and centred
        /// in the header, and clipped to the tab. A null header shows an empty tab.</summary>
        public void AddTab(UIElement header, UIElement content) {
            this._headerStrings.Add(null);
            this._headerElements.Add(header);
            this._contents.Add(content);
            if (header != null) {
                this.LogicalChildren.Add(header);
            }

            if (content != null) {
                this.LogicalChildren.Add(content);
            }

            this.InvalidateMeasure();
        }

        /// <summary>Measures each tab's header (in its strip slot) and content (against the body area).</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            var bodyH = availableHeight - this._headerHeight;
            if (bodyH < 0) {
                bodyH = 0;
            }

            var n = this._contents.Count;
            var tabW = n > 0 ? availableWidth / n : availableWidth;
            for (var i = 0; i < n; i++) {
                ((UIElement)this._headerElements[i])?.Measure(tabW, this._headerHeight);
                ((UIElement)this._contents[i])?.Measure(availableWidth, bodyH);
            }

            desiredWidth = availableWidth < Media.Constants.MaxExtent ? availableWidth : 0;
            desiredHeight = availableHeight < Media.Constants.MaxExtent ? availableHeight : this._headerHeight;
        }

        /// <summary>Arranges each UIElement header centred in its strip slot, and the selected tab's content in the
        /// body (collapsing the rest).</summary>
        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            var bodyH = arrangeHeight - this._headerHeight;
            if (bodyH < 0) {
                bodyH = 0;
            }

            var n = this._contents.Count;
            var tabW = n > 0 ? arrangeWidth / n : arrangeWidth;
            var sel = this.SafeSelectedIndex;
            for (var i = 0; i < n; i++) {
                var h = (UIElement)this._headerElements[i];
                if (h != null) {
                    // Inset the header region by the 1px tab-outline pen so content sits INSIDE the border rather
                    // than painting over it (WPF's Border insets its child by BorderThickness; we do it explicitly).
                    const int border = 1;
                    var innerX = i * tabW + border;
                    var innerW = tabW - 2 * border;
                    var innerH = this._headerHeight - 2 * border;
                    if (innerW < 0) innerW = 0;
                    if (innerH < 0) innerH = 0;

                    h.GetDesiredSize(out var hw, out var hh);
                    if (hw > innerW) hw = innerW;
                    if (hh > innerH) hh = innerH;
                    // Centre the header content in the inset slot (like WPF's default header alignment).
                    h.Arrange(innerX + (innerW - hw) / 2, border + (innerH - hh) / 2, hw, hh);
                }

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

            base.OnTouchUp(e); // raise the public TouchUp event for user/designer handlers

            e.GetPosition(this, 0, out var x, out var y);
            var n = this._contents.Count;
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

        /// <summary>Draws the tab-header backgrounds and separators. String headers are drawn here; UIElement headers
        /// are rendered by the framework (as children) on top of the backgrounds.</summary>
        public override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            var w = this._renderWidth;
            var n = this._contents.Count;
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

                if (this._headerStrings[i] is string txt && this.Font != null) {
                    dc.DrawText(ref txt, this.Font, this.HeaderColor, x + 3, (this._headerHeight - this.Font.Height) / 2, tabW - 6, this.Font.Height, TextAlignment.Center, TextTrimming.CharacterEllipsis);
                }
            }

            dc.DrawLine(pen, 0, this._headerHeight, w - 1, this._headerHeight);
        }
    }
}
