using GHIElectronics.TinyCLR.UI.Input;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class ListBoxItem : ContentControl {
        public bool IsSelected => (this._listBox != null && this._listBox.SelectedItem == this);

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

        protected internal virtual void OnIsSelectedChanged(bool isSelected) {
        }

        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            if (this.IsSelectable) {
                this._listBox.SelectedItem = this;
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


