using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections {
    [Serializable()]
    [DebuggerDisplay("Count = {Count}")]
    public class ArrayList : IList, ICloneable {
        private static object[] _emptyArray = new object[0];

        private object[] _items;
        private int _size;

        // Keep in-sync with c_DefaultCapacity in CLR_RT_HeapBlock_ArrayList in TinyCLR_Runtime__HeapBlock.h
        private const int _defaultCapacity = 4;

        public ArrayList() => this._items = new object[_defaultCapacity];

        public ArrayList(int capacity) {
            if (capacity < 0) throw new ArgumentOutOfRangeException();
            if (capacity == 0) {
                this._items = _emptyArray;
                return;
            }
            this._items = new object[capacity];
        }

        public ArrayList(ICollection c) {
            if (c == null) throw new ArgumentNullException();
            var count = c.Count;
            if (count == 0) {
                this._items = _emptyArray;
                return;
            }
            this._items = new object[count];
            c.CopyTo(this._items, 0);
            this._size = count;
        }

        public virtual int Capacity {
            get => this._items.Length; set => SetCapacity(value);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void SetCapacity(int capacity);

        public virtual int Count => this._size;
        public virtual bool IsFixedSize => false;
        public virtual bool IsReadOnly => false;
        public virtual bool IsSynchronized => false;
        public virtual object SyncRoot => this;
        public extern virtual object this[int index] {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual int Add(object value);

        public virtual void AddRange(ICollection c) => this.InsertRange(this._size, c);

        public virtual int BinarySearch(object value, IComparer comparer) => Array.BinarySearch(this._items, 0, this._size, value, comparer);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void Clear();

        public virtual object Clone() {
            var la = new ArrayList();

            if (this._size > _defaultCapacity) {
                // only re-allocate a new array if the size isn't what we need.
                // otherwise, the one allocated in the constructor will be just fine
                la._items = new object[this._size];
            }

            la._size = this._size;
            Array.Copy(this._items, 0, la._items, 0, this._size);
            return la;
        }

        public virtual bool Contains(object item) => Array.IndexOf(this._items, item, 0, this._size) >= 0;

        public virtual void CopyTo(Array array) => CopyTo(array, 0);

        public virtual void CopyTo(Array array, int arrayIndex) => Array.Copy(this._items, 0, array, arrayIndex, this._size);

        public virtual IEnumerator GetEnumerator() => new Array.SZArrayEnumerator(this._items, 0, this._size);

        public virtual int IndexOf(object value) => Array.IndexOf(this._items, value, 0, this._size);

        public virtual int IndexOf(object value, int startIndex) => Array.IndexOf(this._items, value, startIndex, this._size - startIndex);

        public virtual int IndexOf(object value, int startIndex, int count) => Array.IndexOf(this._items, value, startIndex, count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void Insert(int index, object value);

        public virtual void InsertRange(int index, ICollection c) {
            if (c == null) throw new ArgumentNullException();
            if (index < 0 || index > this._size) throw new ArgumentOutOfRangeException();
            var count = c.Count;
            if (count > 0) {
                if (this._items.Length < this._size + count) this.SetCapacity(this._size + count);
                if (index < this._size) {
                    Array.Copy(this._items, index, this._items, index + count, this._size - index);
                }

                if (this == c && index < this._size) {
                    Array.Copy(this._items, 0, this._items, index, index);
                    Array.Copy(this._items, index + count, this._items, index + index, this._size - index);
                }
                else {
                    c.CopyTo(this._items, index);
                }
                this._size += count;
            }
        }

        public virtual void Remove(object obj) {
            var index = Array.IndexOf(this._items, obj, 0, this._size);
            if (index >= 0) {
                RemoveAt(index);
            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void RemoveAt(int index);

        public virtual void RemoveRange(int index, int count) {
            if (index < 0) throw new ArgumentOutOfRangeException();
            if (count < 0) throw new ArgumentOutOfRangeException();
            if (this._size - index < count) throw new ArgumentException();
            if (count > 0) {
                var newSize = this._size - count;
                if (index < this._size) {
                    Array.Copy(this._items, index + count, this._items, index, newSize - index);
                }
                for (var i = newSize; i < this._size; i++) {
                    this._items[i] = null;
                }
                this._size = newSize;
            }
        }

        public virtual object[] ToArray() => (object[])ToArray(typeof(object));

        public virtual Array ToArray(Type type) {
            var array = Array.CreateInstance(type, this._size);

            Array.Copy(this._items, 0, array, 0, this._size);

            return array;
        }
    }
}


