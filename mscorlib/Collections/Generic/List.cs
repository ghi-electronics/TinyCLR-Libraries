using System.Collections;
using System.Diagnostics;

namespace System.Collections.Generic {
    public interface IEnumerable<out T> : IEnumerable {
        new IEnumerator<T> GetEnumerator();
    }

    public interface IEnumerator<out T> : IDisposable, IEnumerator {
        new T Current { get; }
    }

    public interface ICollection<T> : IEnumerable<T> {
        int Count { get; }
        bool IsReadOnly { get; }
        void Add(T item);
        void Clear();
        bool Contains(T item);
        void CopyTo(T[] array, int arrayIndex);
        bool Remove(T item);
    }

    public interface IList<T> : ICollection<T> {
        T this[int index] { get; set; }
        int IndexOf(T item);
        void Insert(int index, T item);
        void RemoveAt(int index);
    }

    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class List<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable {
        private const int _defaultCapacity = 4;

        private static readonly T[] s_emptyArray = new T[0];

        private T[] _items;
        private int _size;
        private int _version;

        public List() {
            this._items = new T[_defaultCapacity];
        }

        public List(int capacity) {
            if (capacity < 0) {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if (capacity == 0) {
                this._items = s_emptyArray;
            }
            else {
                this._items = new T[capacity];
            }
        }

        public List(IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (collection is ICollection<T> c) {
                int count = c.Count;
                if (count == 0) {
                    this._items = s_emptyArray;
                }
                else {
                    this._items = new T[count];
                    c.CopyTo(this._items, 0);
                    this._size = count;
                }
            }
            else {
                this._items = new T[_defaultCapacity];
                foreach (T item in collection) {
                    this.Add(item);
                }
            }
        }

        public int Capacity {
            get => this._items.Length;
            set {
                if (value < this._size) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (value == this._items.Length) {
                    return;
                }

                if (value == 0) {
                    this._items = s_emptyArray;
                    return;
                }

                var dest = new T[value];
                if (this._size > 0) {
                    Array.Copy(this._items, 0, dest, 0, this._size);
                }

                this._items = dest;
            }
        }

        public int Count => this._size;

        bool ICollection<T>.IsReadOnly => false;

        public T this[int index] {
            get {
                if ((uint)index >= (uint)this._size) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return this._items[index];
            }
            set {
                if ((uint)index >= (uint)this._size) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                this._items[index] = value;
                this._version++;
            }
        }

        public void Add(T item) {
            if (this._size == this._items.Length) {
                this.EnsureCapacity(this._size + 1);
            }

            this._items[this._size++] = item;
            this._version++;
        }

        public void AddRange(IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }

            if (collection is ICollection<T> c) {
                int n = c.Count;
                if (n > 0) {
                    this.EnsureCapacity(this._size + n);
                    c.CopyTo(this._items, this._size);
                    this._size += n;
                    this._version++;
                }
            }
            else {
                foreach (T item in collection) {
                    this.Add(item);
                }
            }
        }

        public void Clear() {
            if (this._size > 0) {
                Array.Clear(this._items, 0, this._size);
                this._size = 0;
            }

            this._version++;
        }

        public bool Contains(T item) {
            for (int i = 0; i < this._size; i++) {
                if (object.Equals(this._items[i], item)) {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex > array.Length || array.Length - arrayIndex < this._size) {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            Array.Copy(this._items, 0, array, arrayIndex, this._size);
        }

        public int IndexOf(T item) {
            for (int i = 0; i < this._size; i++) {
                if (object.Equals(this._items[i], item)) {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, T item) {
            if ((uint)index > (uint)this._size) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (this._size == this._items.Length) {
                this.EnsureCapacity(this._size + 1);
            }

            if (index < this._size) {
                Array.Copy(this._items, index, this._items, index + 1, this._size - index);
            }

            this._items[index] = item;
            this._size++;
            this._version++;
        }

        public bool Remove(T item) {
            int index = this.IndexOf(item);
            if (index >= 0) {
                this.RemoveAt(index);
                return true;
            }

            return false;
        }

        public void RemoveAt(int index) {
            if ((uint)index >= (uint)this._size) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            this._size--;
            if (index < this._size) {
                Array.Copy(this._items, index + 1, this._items, index, this._size - index);
            }

            this._items[this._size] = default(T);
            this._version++;
        }

        public T[] ToArray() {
            var r = new T[this._size];
            Array.Copy(this._items, 0, r, 0, this._size);
            return r;
        }

        public void TrimExcess() {
            int threshold = (int)(this._items.Length * 0.9);
            if (this._size < threshold) {
                this.Capacity = this._size;
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        private void EnsureCapacity(int min) {
            if (this._items.Length >= min) {
                return;
            }

            int newCapacity = this._items.Length == 0 ? _defaultCapacity : this._items.Length * 2;
            if ((uint)newCapacity > 0x7FFFFFC7u) {
                newCapacity = 0x7FFFFFC7;
            }

            if (newCapacity < min) {
                newCapacity = min;
            }

            this.Capacity = newCapacity;
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable {
            private readonly List<T> _list;
            private int _index;
            private readonly int _version;
            private T _current;

            internal Enumerator(List<T> list) {
                this._list = list;
                this._index = -1;
                this._version = list._version;
                this._current = default(T);
            }

            public void Dispose() {
            }

            public bool MoveNext() {
                if (this._version != this._list._version) {
                    throw new InvalidOperationException();
                }

                this._index++;
                if (this._index < this._list._size) {
                    this._current = this._list._items[this._index];
                    return true;
                }

                this._index = this._list._size;
                this._current = default(T);
                return false;
            }

            public T Current => this._current;

            object IEnumerator.Current {
                get {
                    if (this._index < 0 || this._index >= this._list._size) {
                        throw new InvalidOperationException();
                    }

                    return this._current;
                }
            }

            void IEnumerator.Reset() {
                throw new NotSupportedException();
            }
        }
    }
}
