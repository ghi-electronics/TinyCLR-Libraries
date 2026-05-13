using System.Diagnostics;

namespace System.Collections.Generic {
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class List<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable {
        private static readonly T[] _emptyArray = new T[0];

        private T[] _items;
        private int _size;
        // Bumped on every structural mutation so live enumerators can detect
        // "collection modified during enumeration" and throw rather than
        // silently corrupting. Matches the .NET BCL pattern.
        private int _version;

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
                _version++;
            }
        }

        public void Add(T item) {
            if (_size == _items.Length) EnsureCapacity(_size + 1);
            _items[_size++] = item;
            _version++;
        }

        public void AddRange(IEnumerable<T> collection) => InsertRange(_size, collection);

        public void Clear() {
            if (_size > 0) {
                Array.Clear(_items, 0, _size);
                _size = 0;
            }
            _version++;
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
            InsertCore(index, item);
            _version++;
        }

        // Insert without bumping _version. Used by InsertRange / AddRange so a
        // bulk op increments the version exactly once at the end (matching .NET
        // BCL semantics — N inserts within one bulk call should look like ONE
        // structural mutation to live enumerators, not N).
        private void InsertCore(int index, T item) {
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
            var any = false;
            while (en.MoveNext()) {
                InsertCore(index++, en.Current);
                any = true;
            }
            if (any) _version++;
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
            _version++;
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
                _version++;
            }
        }

        public T[] ToArray() {
            var array = new T[_size];
            Array.Copy(_items, 0, array, 0, _size);
            return array;
        }

        // --- Predicate-based query (Find / Exists / TrueForAll family) ---

        public T Find(Predicate<T> match) {
            if (match == null) throw new ArgumentNullException();
            for (var i = 0; i < _size; i++) if (match(_items[i])) return _items[i];
            return default(T);
        }

        public List<T> FindAll(Predicate<T> match) {
            if (match == null) throw new ArgumentNullException();
            var list = new List<T>();
            for (var i = 0; i < _size; i++) if (match(_items[i])) list.Add(_items[i]);
            return list;
        }

        public int FindIndex(Predicate<T> match) => FindIndex(0, _size, match);
        public int FindIndex(int startIndex, Predicate<T> match) => FindIndex(startIndex, _size - startIndex, match);
        public int FindIndex(int startIndex, int count, Predicate<T> match) {
            if ((uint)startIndex > (uint)_size) throw new ArgumentOutOfRangeException();
            if (count < 0 || startIndex > _size - count) throw new ArgumentOutOfRangeException();
            if (match == null) throw new ArgumentNullException();
            var end = startIndex + count;
            for (var i = startIndex; i < end; i++) if (match(_items[i])) return i;
            return -1;
        }

        public T FindLast(Predicate<T> match) {
            if (match == null) throw new ArgumentNullException();
            for (var i = _size - 1; i >= 0; i--) if (match(_items[i])) return _items[i];
            return default(T);
        }

        public int FindLastIndex(Predicate<T> match) => FindLastIndex(_size - 1, _size, match);
        public int FindLastIndex(int startIndex, Predicate<T> match) => FindLastIndex(startIndex, startIndex + 1, match);
        public int FindLastIndex(int startIndex, int count, Predicate<T> match) {
            if (match == null) throw new ArgumentNullException();
            if (_size == 0) {
                if (startIndex != -1) throw new ArgumentOutOfRangeException();
            }
            else if ((uint)startIndex >= (uint)_size) throw new ArgumentOutOfRangeException();
            if (count < 0 || startIndex - count + 1 < 0) throw new ArgumentOutOfRangeException();
            var end = startIndex - count;
            for (var i = startIndex; i > end; i--) if (match(_items[i])) return i;
            return -1;
        }

        public bool Exists(Predicate<T> match) => FindIndex(match) >= 0;

        public bool TrueForAll(Predicate<T> match) {
            if (match == null) throw new ArgumentNullException();
            for (var i = 0; i < _size; i++) if (!match(_items[i])) return false;
            return true;
        }

        // --- LastIndexOf ---

        public int LastIndexOf(T item) => _size == 0 ? -1 : LastIndexOf(item, _size - 1, _size);
        public int LastIndexOf(T item, int index) => LastIndexOf(item, index, index + 1);
        public int LastIndexOf(T item, int index, int count) {
            if (_size == 0) return -1;
            if ((uint)index >= (uint)_size) throw new ArgumentOutOfRangeException();
            if (count < 0 || count > index + 1) throw new ArgumentOutOfRangeException();
            var end = index - count;
            for (var i = index; i > end; i--) if (Object.Equals(_items[i], item)) return i;
            return -1;
        }

        // --- Sort ---
        // In-place quicksort with insertion-sort fallback for small partitions.
        // Embedded target: arr sizes are modest, no need for full introsort.

        public void Sort() => Sort(0, _size, null);
        public void Sort(Comparison<T> comparison) {
            if (comparison == null) throw new ArgumentNullException();
            Sort(0, _size, Comparer<T>.Create(comparison));
        }
        public void Sort(IComparer<T> comparer) => Sort(0, _size, comparer);
        public void Sort(int index, int count, IComparer<T> comparer) {
            if (index < 0 || count < 0) throw new ArgumentOutOfRangeException();
            if (_size - index < count) throw new ArgumentException();
            if (comparer == null) comparer = Comparer<T>.Default;
            if (count > 1) QuickSort(index, index + count - 1, comparer);
            _version++;
        }

        private void QuickSort(int left, int right, IComparer<T> comparer) {
            while (left < right) {
                if (right - left < 8) { InsertionSort(left, right, comparer); return; }
                var pivot = _items[(left + right) / 2];
                int i = left, j = right;
                while (i <= j) {
                    while (comparer.Compare(_items[i], pivot) < 0) i++;
                    while (comparer.Compare(_items[j], pivot) > 0) j--;
                    if (i <= j) {
                        var tmp = _items[i]; _items[i] = _items[j]; _items[j] = tmp;
                        i++; j--;
                    }
                }
                // Recurse on smaller partition, iterate larger - keeps stack O(log n).
                if (j - left < right - i) {
                    if (left < j) QuickSort(left, j, comparer);
                    left = i;
                }
                else {
                    if (i < right) QuickSort(i, right, comparer);
                    right = j;
                }
            }
        }

        private void InsertionSort(int left, int right, IComparer<T> comparer) {
            for (var i = left + 1; i <= right; i++) {
                var item = _items[i];
                var j = i - 1;
                while (j >= left && comparer.Compare(_items[j], item) > 0) {
                    _items[j + 1] = _items[j];
                    j--;
                }
                _items[j + 1] = item;
            }
        }

        // --- Reverse ---

        public void Reverse() => Reverse(0, _size);
        public void Reverse(int index, int count) {
            if (index < 0 || count < 0) throw new ArgumentOutOfRangeException();
            if (_size - index < count) throw new ArgumentException();
            var i = index;
            var j = index + count - 1;
            while (i < j) {
                var tmp = _items[i]; _items[i] = _items[j]; _items[j] = tmp;
                i++; j--;
            }
            _version++;
        }

        // --- Functional / bulk ---

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) {
            if (converter == null) throw new ArgumentNullException();
            var list = new List<TOutput>(_size);
            for (var i = 0; i < _size; i++) list.Add(converter(_items[i]));
            return list;
        }

        public void ForEach(Action<T> action) {
            if (action == null) throw new ArgumentNullException();
            var version = _version;
            for (var i = 0; i < _size; i++) {
                if (version != _version) throw new InvalidOperationException();
                action(_items[i]);
            }
        }

        public int RemoveAll(Predicate<T> match) {
            if (match == null) throw new ArgumentNullException();
            // Two-finger compaction: keep elements that don't match, in original order.
            var free = 0;
            while (free < _size && !match(_items[free])) free++;
            if (free >= _size) return 0;
            var current = free + 1;
            while (current < _size) {
                while (current < _size && match(_items[current])) current++;
                if (current < _size) _items[free++] = _items[current++];
            }
            var removed = _size - free;
            Array.Clear(_items, free, removed);
            _size = free;
            _version++;
            return removed;
        }

        // --- BinarySearch (requires the list to be sorted by `comparer`) ---

        public int BinarySearch(T item) => BinarySearch(0, _size, item, null);
        public int BinarySearch(T item, IComparer<T> comparer) => BinarySearch(0, _size, item, comparer);
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer) {
            if (index < 0 || count < 0) throw new ArgumentOutOfRangeException();
            if (_size - index < count) throw new ArgumentException();
            if (comparer == null) comparer = Comparer<T>.Default;
            int lo = index, hi = index + count - 1;
            while (lo <= hi) {
                var mid = lo + ((hi - lo) >> 1);
                var c = comparer.Compare(_items[mid], item);
                if (c == 0) return mid;
                if (c < 0) lo = mid + 1;
                else hi = mid - 1;
            }
            return ~lo;
        }

        // --- Range / capacity ---

        public List<T> GetRange(int index, int count) {
            if (index < 0 || count < 0) throw new ArgumentOutOfRangeException();
            if (_size - index < count) throw new ArgumentException();
            var list = new List<T>(count);
            Array.Copy(_items, index, list._items, 0, count);
            list._size = count;
            return list;
        }

        public void TrimExcess() {
            var threshold = (int)(_items.Length * 0.9);
            if (_size < threshold) Capacity = _size;
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
            private readonly int _version;
            private int _index;
            private T _current;

            internal Enumerator(List<T> list) {
                _list = list;
                _version = list._version;
                _index = -1;
                _current = default(T);
            }

            public bool MoveNext() {
                // Fail-fast if the list mutated since this enumerator was
                // created. Previously the enumerator would silently keep going
                // on a moving target and return wrong elements.
                if (_version != _list._version) throw new InvalidOperationException();
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
                if (_version != _list._version) throw new InvalidOperationException();
                _index = -1;
                _current = default(T);
            }

            public void Dispose() { }
        }
    }
}
