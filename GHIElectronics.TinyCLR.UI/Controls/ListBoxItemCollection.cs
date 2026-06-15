using System;
using System.Collections;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>The collection of items belonging to a <see cref="ListBox"/>.</summary>
    public class ListBoxItemCollection : ICollection {
        UIElementCollection _items;

        /// <summary>Creates a collection backed by the given list box and element collection.</summary>
        public ListBoxItemCollection(ListBox listBox, UIElementCollection items) {
            this._listBox = listBox;
            this._items = items;
        }

        /// <summary>Adds an item to the list and returns its index.</summary>
        public int Add(ListBoxItem item) {
            var pos = this._items.Add(item);
            item.SetListBox(this._listBox);
            return pos;
        }

        /// <summary>Wraps an element in a new ListBoxItem, adds it, and returns its index.</summary>
        public int Add(UIElement element) {
            var item = new ListBoxItem {
                Child = element
            };
            return Add(item);
        }

        /// <summary>Removes all items from the list.</summary>
        public void Clear() => this._items.Clear();

        /// <summary>Returns true if the list contains the given item.</summary>
        public bool Contains(ListBoxItem item) => this._items.Contains(item);

        /// <summary>Gets or sets the item at the given index.</summary>
        public ListBoxItem this[int index] {
            get => (ListBoxItem)this._items[index];
            set { this._items[index] = value; value.SetListBox(this._listBox); }
        }

        /// <summary>Returns the index of the given item, or -1 if not found.</summary>
        public int IndexOf(ListBoxItem item) => this._items.IndexOf(item);

        /// <summary>Inserts an item at the given index.</summary>
        public void Insert(int index, ListBoxItem item) {
            this._items.Insert(index, item);
            item.SetListBox(this._listBox);
        }

        /// <summary>Removes the given item from the list.</summary>
        public void Remove(ListBoxItem item) {
            this._items.Remove(item);
            item.SetListBox(null);
        }

        /// <summary>Removes the item at the given index.</summary>
        public void RemoveAt(int index) {
            if (index >= 0 && index < this._items.Count) {
                this[index].SetListBox(null);
            }

            this._items.RemoveAt(index);
        }

        #region ICollection Members

        /// <summary>Copies the items to an array starting at the given index.</summary>
        public void CopyTo(Array array, int index) => this._items.CopyTo(array, index);

        /// <summary>Number of items in the collection.</summary>
        public int Count => this._items.Count;

        /// <summary>Whether access to the collection is synchronized.</summary>
        public bool IsSynchronized => this._items.IsSynchronized;

        /// <summary>Object used to synchronize access to the collection.</summary>
        public object SyncRoot => this._items.SyncRoot;

        #endregion

        #region IEnumerable Members

        /// <summary>Returns an enumerator over the items.</summary>
        public IEnumerator GetEnumerator() => ((IEnumerable)this._items).GetEnumerator();

        #endregion

        private ListBox _listBox;
    }
}


