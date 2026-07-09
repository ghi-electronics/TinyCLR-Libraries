using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>An entry in a <see cref="Menu"/>: a header plus optional sub-items.</summary>
    public sealed class MenuEntry {
        /// <summary>The text shown for this entry.</summary>
        public string Header;
        /// <summary>The sub-items shown when this top-level entry is opened.</summary>
        public readonly ArrayList Children = new ArrayList();

        /// <summary>Creates an entry with the given header.</summary>
        public MenuEntry(string header) => this.Header = header ?? string.Empty;

        /// <summary>Adds a sub-item.</summary>
        public MenuEntry Add(MenuEntry child) {
            this.Children.Add(child);
            return child;
        }
    }

    /// <summary>A simple two-level menu bar: a row of top-level entries; tapping one opens its sub-items as an inline
    /// dropdown drawn within the control's bounds. (Embedded UIs have no popup layer, so size the Menu tall enough to
    /// show the open dropdown.) Tapping a sub-item raises <see cref="ItemClick"/>.</summary>
    public class Menu : Control {
        private readonly ArrayList _items = new ArrayList(); // MenuEntry
        private int _openIndex = -1;
        private int _barHeight = 24;
        private int _itemWidth = 64;

        /// <summary>Fill of the menu bar.</summary>
        public Color BarColor { get; set; } = Color.FromRgb(0xE0, 0xE0, 0xE4);
        /// <summary>Fill of an open dropdown.</summary>
        public Color DropColor { get; set; } = Colors.White;
        /// <summary>Colour of the entry text.</summary>
        public Color ForeColor { get; set; } = Colors.Black;

        /// <summary>Width in pixels of each top-level bar entry.</summary>
        public int ItemWidth {
            get => this._itemWidth;
            set { if (value > 0) { this._itemWidth = value; this.Invalidate(); } }
        }

        /// <summary>Raised when a sub-item is tapped; the argument is its header text.</summary>
        public event MenuItemClickHandler ItemClick;

        /// <summary>Adds a top-level entry.</summary>
        public void AddItem(MenuEntry item) {
            if (item != null) {
                this._items.Add(item);
                this.InvalidateMeasure();
            }
        }

        /// <summary>Reports the bar height, growing to include the open dropdown so it is both visible AND
        /// hit-testable. When closed the control is only the bar tall, so it doesn't block touches to whatever
        /// sits beneath it (hit-testing is bounds-based).</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            desiredWidth = availableWidth < Media.Constants.MaxExtent ? availableWidth : this._items.Count * this._itemWidth;
            desiredHeight = this._barHeight;

            if (this._openIndex >= 0 && this._openIndex < this._items.Count) {
                var open = (MenuEntry)this._items[this._openIndex];
                desiredHeight += open.Children.Count * this._barHeight;
            }
        }

        /// <summary>Opens/closes a top entry, or selects a sub-item.</summary>
        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            e.GetPosition(this, 0, out var x, out var y);

            if (y >= 0 && y < this._barHeight) {
                var idx = x / this._itemWidth;
                if (idx >= 0 && idx < this._items.Count) {
                    this._openIndex = (this._openIndex == idx) ? -1 : idx;
                    this.InvalidateMeasure(); // re-measure: the control grows/shrinks with the dropdown
                    e.Handled = true;
                }

                return;
            }

            if (this._openIndex >= 0 && this._openIndex < this._items.Count) {
                var open = (MenuEntry)this._items[this._openIndex];
                var row = (y - this._barHeight) / this._barHeight;
                if (row >= 0 && row < open.Children.Count) {
                    var dropX = this._openIndex * this._itemWidth;
                    if (x >= dropX && x < dropX + this._itemWidth) {
                        var child = (MenuEntry)open.Children[row];
                        this._openIndex = -1;
                        this.InvalidateMeasure();
                        this.ItemClick?.Invoke(this, child.Header);
                        e.Handled = true;
                        return;
                    }
                }

                // Tapped outside the dropdown: close it.
                this._openIndex = -1;
                this.InvalidateMeasure();
            }
        }

        /// <summary>Draws the bar and (if open) the dropdown.</summary>
        public override void OnRender(DrawingContext dc) {
            base.OnRender(dc);

            if (this.Font == null) {
                return;
            }

            var w = this._renderWidth;
            var bh = this._barHeight;
            var iw = this._itemWidth;
            var fh = this.Font.Height;
            var pen = new Media.Pen(Color.FromRgb(0x80, 0x80, 0x86), 1);

            dc.DrawRectangle(new SolidColorBrush(this.BarColor), null, 0, 0, w, bh);

            for (var i = 0; i < this._items.Count; i++) {
                var entry = (MenuEntry)this._items[i];
                var x = i * iw;
                if (i == this._openIndex) {
                    dc.DrawRectangle(new SolidColorBrush(this.DropColor), pen, x, 0, iw, bh);
                }

                var txt = entry.Header;
                dc.DrawText(ref txt, this.Font, this.ForeColor, x + 4, (bh - fh) / 2, iw - 8, fh, TextAlignment.Left, TextTrimming.CharacterEllipsis);
            }

            if (this._openIndex >= 0 && this._openIndex < this._items.Count) {
                var open = (MenuEntry)this._items[this._openIndex];
                var dropX = this._openIndex * iw;
                for (var r = 0; r < open.Children.Count; r++) {
                    var child = (MenuEntry)open.Children[r];
                    var y = bh + r * bh;
                    dc.DrawRectangle(new SolidColorBrush(this.DropColor), pen, dropX, y, iw, bh);
                    var txt = child.Header;
                    dc.DrawText(ref txt, this.Font, this.ForeColor, dropX + 4, y + (bh - fh) / 2, iw - 8, fh, TextAlignment.Left, TextTrimming.CharacterEllipsis);
                }
            }
        }
    }

    /// <summary>Handles a <see cref="Menu.ItemClick"/> event.</summary>
    public delegate void MenuItemClickHandler(object sender, string header);
}
