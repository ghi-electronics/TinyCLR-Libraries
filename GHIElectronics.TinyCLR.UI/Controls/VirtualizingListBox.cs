using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>
    /// Large homogeneous lists: recycles a small pool of rows while preserving full scroll extent.
    /// Set <see cref="ItemsSource"/> to an <see cref="IList"/> (e.g. <see cref="ArrayList"/>); each item is shown via <c>ToString()</c>.
    /// </summary>
    public class VirtualizingListBox : Control {
        private const int DefaultPoolSize = 12;

        private readonly ScrollViewer scroll;
        private readonly VirtualPanel host;
        private IList _itemsSource;
        private int _itemHeight = 28;
        private int _firstVisible;
        private int _selectedIndex = -1;
        private SelectionChangedEventHandler _selectionChanged;

        public VirtualizingListBox() {
            this.host = new VirtualPanel(this);
            for (var i = 0; i < DefaultPoolSize; i++) {
                var text = new Text { TextContent = string.Empty };
                var border = new Border {
                    Child = text,
                    Background = Theme.TextBoxFillBrush,
                };
                border.SetBorderThickness(0, 0, 0, 1);
                border.TouchUp += this.Row_TouchUp;
                this.host.Children.Add(border);
            }

            this.scroll = new ScrollViewer {
                Child = this.host,
            };
            this.scroll.ScrollChanged += this.Scroll_ScrollChanged;
            this.LogicalChildren.Add(this.scroll);
        }

        public IList ItemsSource {
            get => this._itemsSource;

            set {
                VerifyAccess();
                this._itemsSource = value;
                this._firstVisible = 0;
                this._selectedIndex = -1;
                this.scroll.VerticalOffset = 0;
                this.host.InvalidateMeasure();
                this.SyncRows();
            }
        }

        /// <summary>Fixed row height in pixels (all rows use this for virtualization math).</summary>
        public int ItemHeight {
            get => this._itemHeight;

            set {
                if (value < 4) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                VerifyAccess();
                this._itemHeight = value;
                this.host.InvalidateMeasure();
            }
        }

        public int SelectedIndex {
            get => this._selectedIndex;

            set {
                VerifyAccess();
                var prev = this._selectedIndex;
                if (prev == value) {
                    return;
                }

                this._selectedIndex = value;
                this.SyncRows();
                this._selectionChanged?.Invoke(this, new SelectionChangedEventArgs(prev, value));
            }
        }

        public event SelectionChangedEventHandler SelectionChanged {
            add => this._selectionChanged += value;
            remove => this._selectionChanged -= value;
        }

        public int HorizontalOffset {
            get => this.scroll.HorizontalOffset;
            set => this.scroll.HorizontalOffset = value;
        }

        public int VerticalOffset {
            get => this.scroll.VerticalOffset;
            set => this.scroll.VerticalOffset = value;
        }

        private int ItemCount => this._itemsSource == null ? 0 : this._itemsSource.Count;

        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            this._firstVisible = this.scroll.VerticalOffset / this._itemHeight;
            if (this._firstVisible < 0) {
                this._firstVisible = 0;
            }

            this.SyncRows();
        }

        private void Row_TouchUp(object sender, TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            var border = (Border)sender;
            var slot = this.host.Children.IndexOf(border);
            if (slot < 0) {
                return;
            }

            var idx = this._firstVisible + slot;
            if (idx >= 0 && idx < this.ItemCount) {
                this.SelectedIndex = idx;
            }
        }

        private void SyncRows() {
            var count = this.ItemCount;
            var ih = this._itemHeight;
            for (var i = 0; i < this.host.Children.Count; i++) {
                var border = (Border)this.host.Children[i];
                var text = (Text)border.Child;
                var idx = this._firstVisible + i;
                if (idx >= count) {
                    border.Visibility = Visibility.Collapsed;
                    continue;
                }

                border.Visibility = Visibility.Visible;
                var item = this._itemsSource[idx];
                text.TextContent = item == null ? string.Empty : item.ToString();
                border.Background = idx == this._selectedIndex ? Theme.SelectionBrush : Theme.TextBoxFillBrush;
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            this.scroll.Measure(availableWidth, availableHeight);
            this.scroll.GetDesiredSize(out desiredWidth, out desiredHeight);
        }

        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            this.scroll.Arrange(0, 0, arrangeWidth, arrangeHeight);
        }

        private sealed class VirtualPanel : Panel {
            private readonly VirtualizingListBox owner;

            public VirtualPanel(VirtualizingListBox owner) => this.owner = owner;

            protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
                var cnt = this.owner.ItemCount;
                var ih = this.owner._itemHeight;
                desiredHeight = cnt * ih;
                desiredWidth = 0;
                var n = this.Children.Count;
                for (var i = 0; i < n; i++) {
                    this.Children[i].Measure(availableWidth, ih);
                    this.Children[i].GetDesiredSize(out var w, out _);
                    desiredWidth = System.Math.Max(desiredWidth, w);
                }

                if (desiredWidth == 0 && availableWidth > 0 && availableWidth < Media.Constants.MaxExtent) {
                    desiredWidth = availableWidth;
                }
            }

            protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
                var ih = this.owner._itemHeight;
                var fv = this.owner._firstVisible;
                var n = this.Children.Count;
                for (var i = 0; i < n; i++) {
                    var y = (fv + i) * ih;
                    this.Children[i].Arrange(0, y, arrangeWidth, ih);
                }
            }
        }
    }
}
