using GHIElectronics.TinyCLR.UI.Input;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A single selectable item within a <see cref="ListBox"/>.</summary>
    public class ListBoxItem : ContentControl {
        /// <summary>True when this item is the list's selected item.</summary>
        public bool IsSelected => (this._listBox != null && this._listBox.SelectedItem == this);

        /// <summary>Whether this item can be selected.</summary>
        public bool IsSelectable {
            get => this._isSelectable;

            set {
                VerifyAccess();

                if (this._isSelectable != value) {
                    this._isSelectable = value;
                    if (!value && this.IsSelected) {
                        this._listBox.SelectedIndex = -1;
                    }
                }
            }
        }

        /// <summary>Called when this item's selected state changes.</summary>
        protected internal virtual void OnIsSelectedChanged(bool isSelected) {
        }

        /// <summary>Handles touch release; selects this item and raises the list's Click.</summary>
        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled || this._listBox == null) {
                return;
            }

            if (this.IsSelectable) {
                this._listBox.SelectedItem = this;
                e.Handled = this._listBox.RaiseClick(this);
            }
        }

        internal void SetListBox(ListBox listbox) {
            this._listBox = listbox;
            if (this.IsSelected && !this.IsSelectable) {
                this._listBox.SelectedIndex = -1;
            }
        }

        private bool _isSelectable = true;
        private ListBox _listBox;
    }
}


