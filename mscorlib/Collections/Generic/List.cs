using System.Diagnostics;

namespace System.Collections.Generic {
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class List<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable {
        private static readonly T[] _emptyArray = new T[0];

        private T[] _items;
        private int _size;

        // Keep in-sync with c_DefaultCapacity in CLR_RT_HeapBlock_ArrayList in TinyCLR_Runtime__HeapBlock.h
        private const int _defaultCapacity = 4;

        public List() => _items = new T[_defaultCapacity];

        public List(int capacity) {
            if (capacity < 0) throw new ArgumentOutOfRangeException();
            _items = capacity == 0 ? _emptyArray : new T[capacity];
        }

        public List(IEnumerable<T> collection) {
            if (collection == null) throw new ArgumentNullException();
            _items = new T[_defaultCapacity];
            var en = collection.GetEnumerator();
            while (en.MoveNext())
                Add(en.Current);
        }

        public int Capacity {
            get => _items.Length;
            set {
                if (value < _size) throw new ArgumentOutOfRangeException();
                if (value == _items.Length) return;
                if (value == 0) {
                    _items = _emptyArray;
                    return;
                }
                var newItems = new T[value];
                if (_size > 0) Array.Copy(_items, 0, newItems, 0, _size);
                _items = newItems;
            }
        }

        public int Count => _size;
        public bool IsReadOnly => false;

        public T this[int index] {
            get {
                if ((uint)index >= (uint)_size) throw new ArgumentOutOfRangeException();
                return _items[index];
            }
            set {
                if ((uint)index >= (uint)_size) throw new ArgumentOutOfRangeException();
                _items[index] = value;
            }
        }

        public void Add(T item) {
            if (_size == _items.Length) EnsureCapacity(_size + 1);
            _items[_size++] = item;
        }

        public void AddRange(IEnumerable<T> collection) => InsertRange(_size, collection);

        public void Clear() {
            if (_size > 0) {
                Array.Clear(_items, 0, _size);
                _size = 0;
            }
        }

        public bool Contains(T item) => IndexOf(item) >= 0;

        public void CopyTo(T[] array, int arrayIndex) {
            if (array == null) throw new ArgumentNullException();
            Array.Copy(_items, 0, array, arrayIndex, _size);
        }

        public IEnumerator<T> GetEnumerator() => new Enumerator(this);

        public int IndexOf(T item) {
            for (var i = 0; i < _size; i++) {
                if (Object.Equals(_items[i], item)) return i;
            }
            return -1;
        }

        public void Insert(int index, T item) {
            if ((uint)index > (uint)_size) throw new ArgumentOutOfRangeException();
            if (_size == _items.Length) EnsureCapacity(_size + 1);
            if (index < _size) Array.Copy(_items, index, _items, index + 1, _size - index);
            _items[index] = item;
            _size++;
        }

        public void InsertRange(int index, IEnumerable<T> collection) {
            if (collection == null) throw new ArgumentNullException();
            if ((uint)index > (uint)_size) throw new ArgumentOutOfRangeException();
            var en = collection.GetEnumerator();
            while (en.MoveNext())
                Insert(index++, en.Current);
        }

        public bool Remove(T item) {
            var index = IndexOf(item);
            if (index >= 0) {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index) {
            if ((uint)index >= (uint)_size) throw new ArgumentOutOfRangeException();
            _size--;
            if (index < _size) Array.Copy(_items, index + 1, _items, index, _size - index);
            _items[_size] = default(T);
        }

        public void RemoveRange(int index, int count) {
            if (index < 0) throw new ArgumentOutOfRangeException();
            if (count < 0) throw new ArgumentOutOfRangeException();
            if (_size - index < count) throw new ArgumentException();
            if (count > 0) {
                var newSize = _size - count;
                if (index < newSize) Array.Copy(_items, index + count, _items, index, newSize - index);
                Array.Clear(_items, newSize, count);
                _size = newSize;
            }
        }

        public T[] ToArray() {
            var array = new T[_size];
            Array.Copy(_items, 0, array, 0, _size);
            return array;
        }

        private void EnsureCapacity(int min) {
            if (_items.Length < min) {
                var newCapacity = _items.Length == 0 ? _defaultCapacity : _items.Length * 2;
                if (newCapacity < min) newCapacity = min;
                Capacity = newCapacity;
            }
        }

        // --- IList (non-generic) explicit implementation ---

        bool IList.IsFixedSize => false;
        bool IList.IsReadOnly => false;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;

        object IList.this[int index] {
            get => this[index];
            set => this[index] = (T)value;
        }

        int IList.Add(object value) {
            Add((T)value);
            return _size - 1;
        }

        bool IList.Contains(object value) => value is T t && Contains(t);
        void IList.Clear() => Clear();
        int IList.IndexOf(object value) => value is T t ? IndexOf(t) : -1;
        void IList.Insert(int index, object value) => Insert(index, (T)value);
        void IList.Remove(object value) { if (value is T t) Remove(t); }
        void IList.RemoveAt(int index) => RemoveAt(index);
        void ICollection.CopyTo(Array array, int index) => Array.Copy(_items, 0, array, index, _size);

        // --- IEnumerable (non-generic) explicit implementation ---

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        // --- Inner Enumerator ---

        private class Enumerator : IEnumerator<T>, IEnumerator, IDisposable {
            private readonly List<T> _list;
            private int _index;
            private T _current;

            internal Enumerator(List<T> list) {
                _list = list;
                _index = -1;
                _current = default(T);
            }

            public bool MoveNext() {
                _index++;
                if (_index < _list._size) {
                    _current = _list._items[_index];
                    return true;
                }
                _current = default(T);
                return false;
            }

            public T Current => _current;

            object IEnumerator.Current => _current;

            public void Reset() {
                _index = -1;
                _current = default(T);
            }

            public void Dispose() { }
        }
    }
}
