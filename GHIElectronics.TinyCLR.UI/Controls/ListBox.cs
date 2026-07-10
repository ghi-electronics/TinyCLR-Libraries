////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>
    /// A scrollable list of selectable items. Two content modes:
    /// <list type="bullet">
    /// <item><b>Items</b> — add explicit <see cref="ListBoxItem"/> rows via <see cref="Items"/> (fully realized).</item>
    /// <item><b>ItemsSource</b> — bind a large <see cref="IList"/> of data to <see cref="ItemsSource"/>; the list then
    /// virtualizes, recycling a small pool of rows (each item shown via <c>ToString()</c>). This replaces the former
    /// separate <c>VirtualizingListBox</c>.</item>
    /// </list>
    /// The two modes are mutually exclusive; setting <see cref="ItemsSource"/> switches the list into virtualized mode.
    /// </summary>
    public class ListBox : ContentControl {
        // Cached once per AppDomain so each commit doesn't allocate a fresh RoutedEvent.
        private static readonly RoutedEvent ClickRoutedEvent =
            new RoutedEvent("ListBoxClickEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));

        /// <summary>Creates a new ListBox.</summary>
        public ListBox() {
            this._panel = new StackPanel();
            this._scrollViewer = new ScrollViewer {
                Child = this._panel
            };
            this.LogicalChildren.Add(this._scrollViewer);
        }

        /// <summary>The collection of items in the list.</summary>
        public ListBoxItemCollection Items {
            get {
                VerifyAccess();

                if (this._items == null) {
                    this._items = new ListBoxItemCollection(this, this._panel.Children);
                }

                return this._items;
            }
        }

        /// <summary>
        /// Fires when the user commits the current selection — either by tapping a
        /// ListBoxItem or by pressing <see cref="HardwareButton.Select"/> while the
        /// ListBox has focus. The event source is the committed <see cref="ListBoxItem"/>.
        /// </summary>
        public event RoutedEventHandler Click;

        /// <summary>Raised when the selected item changes.</summary>
        public event SelectionChangedEventHandler SelectionChanged {
            add {
                VerifyAccess();
                this._selectionChanged += value;
            }

            remove {
                VerifyAccess();
                this._selectionChanged -= value;
            }
        }

        /// <summary>Index of the selected item, or -1 if none is selected.</summary>
        public int SelectedIndex {
            get => this._selectedIndex;

            set {
                VerifyAccess();

                if (this._selectedIndex == value) {
                    return;
                }

                if (value < -1) {
                    throw new ArgumentOutOfRangeException("SelectedIndex");
                }

                // Virtualized (ItemsSource) mode: selection is purely index-based; there is no ListBoxItem container.
                if (this._itemsSource != null) {
                    var prev = this._selectedIndex;
                    this._selectedIndex = value;
                    this.SyncRows();
                    this._selectionChanged?.Invoke(this, new SelectionChangedEventArgs(prev, value));
                    return;
                }

                var item = (this._items != null && value >= 0 && value < this._items.Count) ? this._items[value] : null;

                if (item != null && !item.IsSelectable) {
                    throw new InvalidOperationException("Item is not selectable");
                }

                var previousItem = this.SelectedItem;
                if (previousItem != null) {
                    previousItem.OnIsSelectedChanged(false);
                }

                var args = new SelectionChangedEventArgs(this._selectedIndex, value);
                this._selectedIndex = value;

                if (item != null) {
                    item.OnIsSelectedChanged(true);
                }

                this._selectionChanged?.Invoke(this, args);
            }
        }

        /// <summary>The currently selected item, or null if none is selected.</summary>
        public ListBoxItem SelectedItem {
            get {
                if (this._items != null && this._selectedIndex >= 0 && this._selectedIndex < this._items.Count) {
                    return this._items[this._selectedIndex];
                }

                return null;
            }

            set {
                VerifyAccess();

                var index = this.Items.IndexOf(value);
                if (index != -1) {
                    this.SelectedIndex = index;
                }
            }
        }

        /// <summary>Scrolls the list so the given item is visible.</summary>
        public void ScrollIntoView(ListBoxItem item) {
            VerifyAccess();

            if (!this.Items.Contains(item)) return;
            this._panel.GetLayoutOffset(out var panelX, out var panelY);
            item.GetLayoutOffset(out var x, out var y);

            var top = y + panelY;
            var bottom = top + item._renderHeight;

            // Make sure bottom of item is in view
            //
            if (bottom > this._scrollViewer._renderHeight) {
                this._scrollViewer.VerticalOffset -= (this._scrollViewer._renderHeight - bottom);
            }

            // Make sure top of item is in view
            //
            if (top < 0) {
                this._scrollViewer.VerticalOffset += top;
            }
        }

        /// <summary>Handles Up/Down navigation and Select activation via hardware buttons.</summary>
        protected override void OnButtonDown(GHIElectronics.TinyCLR.UI.Input.ButtonEventArgs e) {
            if (e.Button == HardwareButton.Down && this._selectedIndex < this.Items.Count - 1) {
                var newIndex = this._selectedIndex + 1;
                while (newIndex < this.Items.Count && !this.Items[newIndex].IsSelectable) newIndex++;

                if (newIndex < this.Items.Count) {
                    this.SelectedIndex = newIndex;
                    ScrollIntoView(this.SelectedItem);
                    e.Handled = true;
                }
            }
            else if (e.Button == HardwareButton.Up && this._selectedIndex > 0) {
                var newIndex = this._selectedIndex - 1;
                while (newIndex >= 0 && !this.Items[newIndex].IsSelectable) newIndex--;

                if (newIndex >= 0) {
                    this.SelectedIndex = newIndex;
                    ScrollIntoView(this.SelectedItem);
                    e.Handled = true;
                }
            }
            else if (e.Button == HardwareButton.Select) {
                var item = this.SelectedItem;
                if (item != null) {
                    e.Handled = this.RaiseClick(item);
                }
            }
        }

        // Raises Click with the supplied ListBoxItem as the event source.
        // Used by ListBoxItem.OnTouchUp and by Select-button activation.
        // Exceptions from user handlers propagate.
        internal bool RaiseClick(ListBoxItem source) {
            var args = new RoutedEventArgs(ClickRoutedEvent, source);
            this.Click?.Invoke(this, args);
            return args.Handled;
        }

        //
        // Scrolling events re-exposed from the ScrollViewer
        //

        /// <summary>
        /// Event handler if the scroll changes.
        /// </summary>
        public event ScrollChangedEventHandler ScrollChanged {
            add { this._scrollViewer.ScrollChanged += value; }
            remove { this._scrollViewer.ScrollChanged -= value; }
        }

        /// <summary>
        /// Horizontal offset of the scroll.
        /// </summary>
        public int HorizontalOffset {
            get => this._scrollViewer.HorizontalOffset;

            set => this._scrollViewer.HorizontalOffset = value;
        }

        /// <summary>
        /// Vertical offset of the scroll.
        /// </summary>
        public int VerticalOffset {
            get => this._scrollViewer.VerticalOffset;

            set => this._scrollViewer.VerticalOffset = value;
        }

        /// <summary>
        /// Extent height of the scroll area.
        /// </summary>
        public int ExtentHeight => this._scrollViewer.ExtentHeight;

        /// <summary>
        /// Extent width of the scroll area.
        /// </summary>
        public int ExtentWidth => this._scrollViewer.ExtentWidth;

        /// <summary>
        /// The scrolling style.
        /// </summary>
        public ScrollingStyle ScrollingStyle {
            get => this._scrollViewer.ScrollingStyle;

            set => this._scrollViewer.ScrollingStyle = value;
        }

        // ---- ItemsSource (virtualized) mode: recycles a small pool of rows for a large IList of data. ----

        private const int VirtualPoolSize = 12;

        /// <summary>
        /// Binds a large data list to the list box; each item is shown via its <c>ToString()</c>. Setting this switches
        /// the list into virtualized mode (a small pool of rows is recycled while scrolling), for big lists that would
        /// be too heavy as explicit <see cref="Items"/>. Leave null to use the <see cref="Items"/> collection. This
        /// replaces the former separate <c>VirtualizingListBox</c> control.
        /// </summary>
        public IList ItemsSource {
            get => this._itemsSource;

            set {
                VerifyAccess();
                this._itemsSource = value;
                this._selectedIndex = -1;
                this._firstVisible = 0;

                if (value != null) {
                    this.EnsureVirtualPanel();
                    if (this._scrollViewer.Child != this._virtualPanel) {
                        this._scrollViewer.Child = this._virtualPanel;
                    }

                    this._scrollViewer.VerticalOffset = 0;
                    this._virtualPanel.InvalidateMeasure();
                    this.SyncRows();
                }
                else if (this._scrollViewer.Child != this._panel) {
                    this._scrollViewer.Child = this._panel;
                }
            }
        }

        /// <summary>Fixed row height in pixels used for the virtualization math in <see cref="ItemsSource"/> mode.</summary>
        public int ItemHeight {
            get => this._itemHeight;

            set {
                if (value < 4) {
                    throw new ArgumentOutOfRangeException("ItemHeight");
                }

                VerifyAccess();
                this._itemHeight = value;
                this._virtualPanel?.InvalidateMeasure();
            }
        }

        private int ItemCount => this._itemsSource == null ? 0 : this._itemsSource.Count;

        private void EnsureVirtualPanel() {
            if (this._virtualPanel != null) {
                return;
            }

            this._virtualPanel = new VirtualPanel(this);
            for (var i = 0; i < VirtualPoolSize; i++) {
                var text = new Text { TextContent = string.Empty };
                var border = new Border { Child = text, Background = Theme.TextBoxFillBrush };
                border.SetBorderThickness(0, 0, 0, 1);
                border.TouchUp += this.VirtualRow_TouchUp;
                this._virtualPanel.Children.Add(border);
            }

            this._scrollViewer.ScrollChanged += this.Virtual_ScrollChanged;
        }

        private void Virtual_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            if (this._itemsSource == null) {
                return;
            }

            this._firstVisible = this._scrollViewer.VerticalOffset / this._itemHeight;
            if (this._firstVisible < 0) {
                this._firstVisible = 0;
            }

            this.SyncRows();
        }

        private void VirtualRow_TouchUp(object sender, TouchEventArgs e) {
            if (!this.IsEnabled || this._itemsSource == null) {
                return;
            }

            var slot = this._virtualPanel.Children.IndexOf((Border)sender);
            if (slot < 0) {
                return;
            }

            var idx = this._firstVisible + slot;
            if (idx >= 0 && idx < this.ItemCount) {
                this.SelectedIndex = idx;
            }
        }

        private void SyncRows() {
            if (this._virtualPanel == null) {
                return;
            }

            var count = this.ItemCount;
            for (var i = 0; i < this._virtualPanel.Children.Count; i++) {
                var border = (Border)this._virtualPanel.Children[i];
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

        // Virtualizing panel: reports the full scroll extent (ItemCount × ItemHeight) but only lays out the recycled
        // pool of rows at their virtual scroll positions. (Ported from the former VirtualizingListBox.)
        private sealed class VirtualPanel : Panel {
            private readonly ListBox owner;

            public VirtualPanel(ListBox owner) => this.owner = owner;

            protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
                var ih = this.owner._itemHeight;
                desiredHeight = this.owner.ItemCount * ih;
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
                    this.Children[i].Arrange(0, (fv + i) * ih, arrangeWidth, ih);
                }
            }
        }

        internal ScrollViewer _scrollViewer;
        internal StackPanel _panel;
        private int _selectedIndex = -1;
        private SelectionChangedEventHandler _selectionChanged;

        private ListBoxItemCollection _items;
        private IList _itemsSource;
        private int _itemHeight = 28;
        private int _firstVisible;
        private VirtualPanel _virtualPanel;
    }
}


