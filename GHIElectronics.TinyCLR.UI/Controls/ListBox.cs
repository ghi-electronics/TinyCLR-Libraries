////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using GHIElectronics.TinyCLR.UI.Input;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A scrollable list of selectable items.</summary>
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

                if (this._selectedIndex != value) {
                    if (value < -1) {
                        throw new ArgumentOutOfRangeException("SelectedIndex");
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

        internal ScrollViewer _scrollViewer;
        internal StackPanel _panel;
        private int _selectedIndex = -1;
        private SelectionChangedEventHandler _selectionChanged;

        private ListBoxItemCollection _items;
    }
}


