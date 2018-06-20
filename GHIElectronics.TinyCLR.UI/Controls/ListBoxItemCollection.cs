using System;
using System.Collections;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class ListBoxItemCollection : ICollection {
        UIElementCollection _items;

        public ListBoxItemCollection(ListBox listBox, UIElementCollection items) {
            this._listBox = listBox;
            this._items = items;
        }

        public int Add(ListBoxItem item) {
            var pos = this._items.Add(item);
            item.SetListBox(this._listBox);
            return pos;
        }

        public int Add(UIElement element) {
            var item = new ListBoxItem {
                Child = element
            };
            return Add(item);
        }

        public void Clear() => this._items.Clear();

        public bool Contains(ListBoxItem item) => this._items.Contains(item);

        public ListBoxItem this[int index] {
            get => (ListBoxItem)this._items[index];
            set { this._items[index] = value; value.SetListBox(this._listBox); }
        }

        public int IndexOf(ListBoxItem item) => this._items.IndexOf(item);

        public void Insert(int index, ListBoxItem item) {
            this._items.Insert(index, item);
            item.SetListBox(this._listBox);
        }

        public void Remove(ListBoxItem item) {
            this._items.Remove(item);
            item.SetListBox(null);
        }

        public void RemoveAt(int index) {
            if (index >= 0 && index < this._items.Count) {
                this[index].SetListBox(null);
            }

            this._items.RemoveAt(index);
        }

        #region ICollection Members

        public void CopyTo(Array array, int index) => this._items.CopyTo(array, index);

        public int Count => this._items.Count;

        public bool IsSynchronized => this._items.IsSynchronized;

        public object SyncRoot => this._items.SyncRoot;

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator() => ((IEnumerable)this._items).GetEnumerator();

        #endregion

        private ListBox _listBox;
    }
}


