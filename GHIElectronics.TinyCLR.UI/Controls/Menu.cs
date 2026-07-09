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

        /// <summary>Which way dropdowns open. <see cref="MenuDropDirection.Down"/> (default) suits a menu at the top
        /// of the screen; <see cref="MenuDropDirection.Up"/> suits a menu at the bottom (so the dropdown stays on
        /// screen). Up requires the Menu to sit in a Canvas (it shifts its own Canvas.Top up while open so the bar
        /// stays put) — same idea WPF handles automatically by re-placing the popup.</summary>
        public MenuDropDirection DropDirection { get; set; } = MenuDropDirection.Down;

        private int _anchorTop = int.MinValue; // the control's closed Canvas.Top; Up mode shifts up from here while open

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

        // Control-local Y of the bar. Down: at the top. Up: at the bottom of the (grown) control, so the dropdown
        // has room above it.
        private int BarTop => this.DropDirection == MenuDropDirection.Up ? (this._renderHeight - this._barHeight) : 0;

        private int OpenRowCount => (this._openIndex >= 0 && this._openIndex < this._items.Count)
            ? ((MenuEntry)this._items[this._openIndex]).Children.Count : 0;

        // Up mode only: shift the control's own Canvas.Top up by the open dropdown height so the BAR stays anchored
        // and the dropdown appears above it. dropH is 0 when closed, restoring the anchor.
        private void ApplyDropLayout() {
            if (this.DropDirection != MenuDropDirection.Up || !(this.Parent is Canvas)) {
                return;
            }

            if (this._anchorTop == int.MinValue) {
                this._anchorTop = Canvas.GetTop(this);
            }

            Canvas.SetTop(this, this._anchorTop - this.OpenRowCount * this._barHeight);
        }

        private void SetOpen(int index) {
            this._openIndex = index;
            this.ApplyDropLayout();     // reposition (Up) before the measure pass
            this.InvalidateMeasure();   // the control grows/shrinks with the dropdown
        }

        /// <summary>Opens/closes a top entry, or selects a sub-item.</summary>
        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            e.GetPosition(this, 0, out var x, out var y);

            var bh = this._barHeight;
            var barTop = this.BarTop;
            var up = this.DropDirection == MenuDropDirection.Up;

            // Bar row: toggle the tapped top-level entry.
            if (y >= barTop && y < barTop + bh) {
                var idx = x / this._itemWidth;
                if (idx >= 0 && idx < this._items.Count) {
                    this.SetOpen(this._openIndex == idx ? -1 : idx);
                    e.Handled = true;
                }

                return;
            }

            // Dropdown rows: above the bar (Up) or below it (Down).
            if (this._openIndex >= 0 && this._openIndex < this._items.Count) {
                var open = (MenuEntry)this._items[this._openIndex];
                var row = up ? (barTop - 1 - y) / bh : (y - (barTop + bh)) / bh;
                if (row >= 0 && row < open.Children.Count) {
                    var dropX = this._openIndex * this._itemWidth;
                    if (x >= dropX && x < dropX + this._itemWidth) {
                        var child = (MenuEntry)open.Children[row];
                        this.SetOpen(-1);
                        this.ItemClick?.Invoke(this, child.Header);
                        e.Handled = true;
                        return;
                    }
                }

                // Tapped outside the dropdown: close it.
                this.SetOpen(-1);
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
            var barTop = this.BarTop;
            var up = this.DropDirection == MenuDropDirection.Up;
            var pen = new Media.Pen(Color.FromRgb(0x80, 0x80, 0x86), 1);

            dc.DrawRectangle(new SolidColorBrush(this.BarColor), null, 0, barTop, w, bh);

            for (var i = 0; i < this._items.Count; i++) {
                var entry = (MenuEntry)this._items[i];
                var x = i * iw;
                if (i == this._openIndex) {
                    dc.DrawRectangle(new SolidColorBrush(this.DropColor), pen, x, barTop, iw, bh);
                }

                var txt = entry.Header;
                dc.DrawText(ref txt, this.Font, this.ForeColor, x + 4, barTop + (bh - fh) / 2, iw - 8, fh, TextAlignment.Left, TextTrimming.CharacterEllipsis);
            }

            if (this._openIndex >= 0 && this._openIndex < this._items.Count) {
                var open = (MenuEntry)this._items[this._openIndex];
                var dropX = this._openIndex * iw;
                for (var r = 0; r < open.Children.Count; r++) {
                    var child = (MenuEntry)open.Children[r];
                    // Row r sits just below the bar for Down, or just above it (stacking upward) for Up.
                    var y = up ? (barTop - (r + 1) * bh) : (barTop + bh + r * bh);
                    dc.DrawRectangle(new SolidColorBrush(this.DropColor), pen, dropX, y, iw, bh);
                    var txt = child.Header;
                    dc.DrawText(ref txt, this.Font, this.ForeColor, dropX + 4, y + (bh - fh) / 2, iw - 8, fh, TextAlignment.Left, TextTrimming.CharacterEllipsis);
                }
            }
        }
    }

    /// <summary>The direction a <see cref="Menu"/> opens its dropdowns.</summary>
    public enum MenuDropDirection {
        /// <summary>Dropdowns open downward (for a menu at the top of the screen).</summary>
        Down,
        /// <summary>Dropdowns open upward (for a menu at the bottom of the screen).</summary>
        Up
    }

    /// <summary>Handles a <see cref="Menu.ItemClick"/> event.</summary>
    public delegate void MenuItemClickHandler(object sender, string header);
}
