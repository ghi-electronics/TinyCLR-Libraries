using System.Diagnostics;

namespace System.Collections.Generic {
    /// <summary>
    /// Hash-table-based generic dictionary. Mirrors the .NET BCL surface for
    /// the operations we ship in this subset. Chained collisions; capacity
    /// grows to roughly 2x on overflow.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class Dictionary<TKey, TValue> :
        IDictionary<TKey, TValue>,
        ICollection<KeyValuePair<TKey, TValue>>,
        IEnumerable<KeyValuePair<TKey, TValue>>,
        IDictionary,
        ICollection,
        IEnumerable {

        private struct Entry {
            public int hashCode;   // lower 31 bits of key hash; -1 if unused
            public int next;       // index of next entry in chain, -1 if end
            public TKey key;
            public TValue value;
        }

        private int[] _buckets;            // bucket index -> first entry index (+ 1, so 0 = "empty")
        private Entry[] _entries;
        private int _count;
        private int _freeList;             // head of free-entry chain
        private int _freeCount;
        private int _version;
        private readonly IEqualityComparer<TKey> _comparer;
        private KeyCollection _keys;
        private ValueCollection _values;

        public Dictionary() : this(0, null) { }
        public Dictionary(int capacity) : this(capacity, null) { }
        public Dictionary(IEqualityComparer<TKey> comparer) : this(0, comparer) { }

        public Dictionary(int capacity, IEqualityComparer<TKey> comparer) {
            if (capacity < 0) throw new ArgumentOutOfRangeException();
            this._comparer = comparer ?? EqualityComparer<TKey>.Default;
            if (capacity > 0) this.Initialize(capacity);
        }

        public Dictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

        public Dictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : this(dictionary == null ? 0 : dictionary.Count, comparer) {
            if (dictionary == null) throw new ArgumentNullException();
            foreach (var kv in dictionary) this.Add(kv.Key, kv.Value);
        }

        public IEqualityComparer<TKey> Comparer => this._comparer;
        public int Count => this._count - this._freeCount;

        public KeyCollection Keys => this._keys ?? (this._keys = new KeyCollection(this));
        public ValueCollection Values => this._values ?? (this._values = new ValueCollection(this));

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => this.Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => this.Values;

        public TValue this[TKey key] {
            get {
                var i = this.FindEntry(key);
                if (i < 0) throw new KeyNotFoundException();
                return this._entries[i].value;
            }
            set => this.Insert(key, value, false);
        }

        public void Add(TKey key, TValue value) => this.Insert(key, value, true);

        public void Clear() {
            if (this._count > 0) {
                for (var i = 0; i < this._buckets.Length; i++) this._buckets[i] = -1;
                Array.Clear(this._entries, 0, this._count);
                this._freeList = -1;
                this._freeCount = 0;
                this._count = 0;
                this._version++;
            }
        }

        public bool ContainsKey(TKey key) => this.FindEntry(key) >= 0;

        public bool ContainsValue(TValue value) {
            if (value == null) {
                for (var i = 0; i < this._count; i++)
                    if (this._entries[i].hashCode >= 0 && this._entries[i].value == null) return true;
            }
            else {
                var cmp = EqualityComparer<TValue>.Default;
                for (var i = 0; i < this._count; i++)
                    if (this._entries[i].hashCode >= 0 && cmp.Equals(this._entries[i].value, value)) return true;
            }
            return false;
        }

        public bool Remove(TKey key) {
            if (key == null) throw new ArgumentNullException();
            if (this._buckets != null) {
                var hashCode = this._comparer.GetHashCode(key) & 0x7FFFFFFF;
                var bucket = hashCode % this._buckets.Length;
                var last = -1;
                for (var i = this._buckets[bucket]; i >= 0; last = i, i = this._entries[i].next) {
                    if (this._entries[i].hashCode == hashCode && this._comparer.Equals(this._entries[i].key, key)) {
                        if (last < 0) this._buckets[bucket] = this._entries[i].next;
                        else this._entries[last].next = this._entries[i].next;
                        this._entries[i].hashCode = -1;
                        this._entries[i].next = this._freeList;
                        this._entries[i].key = default(TKey);
                        this._entries[i].value = default(TValue);
                        this._freeList = i;
                        this._freeCount++;
                        this._version++;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            var i = this.FindEntry(key);
            if (i >= 0) { value = this._entries[i].value; return true; }
            value = default(TValue);
            return false;
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        // --- ICollection<KVP> ---

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => this.Add(item.Key, item.Value);

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) {
            var i = this.FindEntry(item.Key);
            return i >= 0 && EqualityComparer<TValue>.Default.Equals(this._entries[i].value, item.Value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index) {
            if (array == null) throw new ArgumentNullException();
            if ((uint)index > (uint)array.Length) throw new ArgumentOutOfRangeException();
            if (array.Length - index < this.Count) throw new ArgumentException();
            for (var i = 0; i < this._count; i++) {
                if (this._entries[i].hashCode >= 0)
                    array[index++] = new KeyValuePair<TKey, TValue>(this._entries[i].key, this._entries[i].value);
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
            var i = this.FindEntry(item.Key);
            if (i >= 0 && EqualityComparer<TValue>.Default.Equals(this._entries[i].value, item.Value))
                return this.Remove(item.Key);
            return false;
        }

        // --- Non-generic IDictionary / ICollection ---

        bool IDictionary.IsFixedSize => false;
        bool IDictionary.IsReadOnly => false;
        ICollection IDictionary.Keys => this.Keys;
        ICollection IDictionary.Values => this.Values;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;

        object IDictionary.this[object key] {
            get {
                if (IsCompatibleKey(key)) {
                    var i = this.FindEntry((TKey)key);
                    if (i >= 0) return this._entries[i].value;
                }
                return null;
            }
            set {
                if (key == null) throw new ArgumentNullException();
                try {
                    var tk = (TKey)key;
                    try { this[tk] = (TValue)value; }
                    catch (InvalidCastException) { throw new ArgumentException(); }
                }
                catch (InvalidCastException) { throw new ArgumentException(); }
            }
        }

        void IDictionary.Add(object key, object value) {
            if (key == null) throw new ArgumentNullException();
            try {
                var tk = (TKey)key;
                try { this.Add(tk, (TValue)value); }
                catch (InvalidCastException) { throw new ArgumentException(); }
            }
            catch (InvalidCastException) { throw new ArgumentException(); }
        }

        bool IDictionary.Contains(object key) => IsCompatibleKey(key) && this.ContainsKey((TKey)key);
        void IDictionary.Remove(object key) { if (IsCompatibleKey(key)) this.Remove((TKey)key); }

        void ICollection.CopyTo(Array array, int index) {
            if (array == null) throw new ArgumentNullException();
            if ((uint)index > (uint)array.Length) throw new ArgumentOutOfRangeException();
            if (array.Length - index < this.Count) throw new ArgumentException();

            if (array is KeyValuePair<TKey, TValue>[] pairs) {
                ((ICollection<KeyValuePair<TKey, TValue>>)this).CopyTo(pairs, index);
            }
            else if (array is object[] objects) {
                for (var i = 0; i < this._count; i++) {
                    if (this._entries[i].hashCode >= 0)
                        objects[index++] = new KeyValuePair<TKey, TValue>(this._entries[i].key, this._entries[i].value);
                }
            }
            else {
                throw new ArgumentException();
            }
        }

        private static bool IsCompatibleKey(object key) {
            if (key == null) throw new ArgumentNullException();
            return key is TKey;
        }

        // --- internals ---

        private int FindEntry(TKey key) {
            if (key == null) throw new ArgumentNullException();
            if (this._buckets != null) {
                var hashCode = this._comparer.GetHashCode(key) & 0x7FFFFFFF;
                for (var i = this._buckets[hashCode % this._buckets.Length]; i >= 0; i = this._entries[i].next) {
                    if (this._entries[i].hashCode == hashCode && this._comparer.Equals(this._entries[i].key, key)) return i;
                }
            }
            return -1;
        }

        private void Initialize(int capacity) {
            var size = GetPrime(capacity);
            this._buckets = new int[size];
            for (var i = 0; i < this._buckets.Length; i++) this._buckets[i] = -1;
            this._entries = new Entry[size];
            this._freeList = -1;
        }

        private void Insert(TKey key, TValue value, bool add) {
            if (key == null) throw new ArgumentNullException();
            if (this._buckets == null) this.Initialize(0);

            var hashCode = this._comparer.GetHashCode(key) & 0x7FFFFFFF;
            var targetBucket = hashCode % this._buckets.Length;

            for (var i = this._buckets[targetBucket]; i >= 0; i = this._entries[i].next) {
                if (this._entries[i].hashCode == hashCode && this._comparer.Equals(this._entries[i].key, key)) {
                    if (add) throw new ArgumentException(); // duplicate key
                    this._entries[i].value = value;
                    this._version++;
                    return;
                }
            }

            int index;
            if (this._freeCount > 0) {
                index = this._freeList;
                this._freeList = this._entries[index].next;
                this._freeCount--;
            }
            else {
                if (this._count == this._entries.Length) {
                    this.Resize();
                    targetBucket = hashCode % this._buckets.Length;
                }
                index = this._count;
                this._count++;
            }

            this._entries[index].hashCode = hashCode;
            this._entries[index].next = this._buckets[targetBucket];
            this._entries[index].key = key;
            this._entries[index].value = value;
            this._buckets[targetBucket] = index;
            this._version++;
        }

        private void Resize() => this.Resize(ExpandPrime(this._count));

        private void Resize(int newSize) {
            var newBuckets = new int[newSize];
            for (var i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
            var newEntries = new Entry[newSize];
            Array.Copy(this._entries, 0, newEntries, 0, this._count);
            for (var i = 0; i < this._count; i++) {
                if (newEntries[i].hashCode >= 0) {
                    var bucket = newEntries[i].hashCode % newSize;
                    newEntries[i].next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
            }
            this._buckets = newBuckets;
            this._entries = newEntries;
        }

        // Small prime table; "good enough" for sub-Mb embedded dicts. Anything
        // larger just doubles.
        private static readonly int[] _primes = { 3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263, 1674319, 2009191 };

        private static int GetPrime(int min) {
            if (min < 0) throw new ArgumentException();
            foreach (var p in _primes) if (p >= min) return p;
            // Past the table: just bump up by 2x and pretend - any int will do
            // (collision rate suffers but correctness is preserved).
            for (var p = min | 1; p < int.MaxValue; p += 2) if ((p & 1) != 0) return p;
            return min;
        }

        private static int ExpandPrime(int oldSize) {
            var newSize = 2 * oldSize;
            return GetPrime(newSize);
        }

        // --- Enumerator + key/value views ---

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDictionaryEnumerator {
            private readonly Dictionary<TKey, TValue> _dictionary;
            private readonly int _version;
            private int _index;
            private KeyValuePair<TKey, TValue> _current;

            internal Enumerator(Dictionary<TKey, TValue> dictionary) {
                this._dictionary = dictionary;
                this._version = dictionary._version;
                this._index = 0;
                this._current = new KeyValuePair<TKey, TValue>();
            }

            public bool MoveNext() {
                if (this._version != this._dictionary._version) throw new InvalidOperationException();
                while ((uint)this._index < (uint)this._dictionary._count) {
                    if (this._dictionary._entries[this._index].hashCode >= 0) {
                        this._current = new KeyValuePair<TKey, TValue>(
                            this._dictionary._entries[this._index].key,
                            this._dictionary._entries[this._index].value);
                        this._index++;
                        return true;
                    }
                    this._index++;
                }
                this._index = this._dictionary._count + 1;
                this._current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            public KeyValuePair<TKey, TValue> Current => this._current;
            object IEnumerator.Current => this._current;
            DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(this._current.Key, this._current.Value);
            object IDictionaryEnumerator.Key => this._current.Key;
            object IDictionaryEnumerator.Value => this._current.Value;

            void IEnumerator.Reset() {
                if (this._version != this._dictionary._version) throw new InvalidOperationException();
                this._index = 0;
                this._current = new KeyValuePair<TKey, TValue>();
            }

            public void Dispose() { }
        }

        public sealed class KeyCollection : ICollection<TKey>, ICollection, IEnumerable<TKey>, IEnumerable {
            private readonly Dictionary<TKey, TValue> _dictionary;
            internal KeyCollection(Dictionary<TKey, TValue> dictionary) {
                if (dictionary == null) throw new ArgumentNullException();
                this._dictionary = dictionary;
            }
            public int Count => this._dictionary.Count;
            public bool IsReadOnly => true;
            bool ICollection.IsSynchronized => false;
            object ICollection.SyncRoot => ((ICollection)this._dictionary).SyncRoot;

            public void CopyTo(TKey[] array, int index) {
                if (array == null) throw new ArgumentNullException();
                if ((uint)index > (uint)array.Length) throw new ArgumentOutOfRangeException();
                if (array.Length - index < this._dictionary.Count) throw new ArgumentException();
                var entries = this._dictionary._entries;
                for (var i = 0; i < this._dictionary._count; i++)
                    if (entries[i].hashCode >= 0) array[index++] = entries[i].key;
            }

            public bool Contains(TKey item) => this._dictionary.ContainsKey(item);

            void ICollection<TKey>.Add(TKey item) => throw new NotSupportedException();
            void ICollection<TKey>.Clear() => throw new NotSupportedException();
            bool ICollection<TKey>.Remove(TKey item) => throw new NotSupportedException();

            void ICollection.CopyTo(Array array, int index) {
                if (array is TKey[] tks) { this.CopyTo(tks, index); return; }
                if (array is object[] objs) {
                    var entries = this._dictionary._entries;
                    for (var i = 0; i < this._dictionary._count; i++)
                        if (entries[i].hashCode >= 0) objs[index++] = entries[i].key;
                    return;
                }
                throw new ArgumentException();
            }

            public Enumerator GetEnumerator() => new Enumerator(this._dictionary);
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => new Enumerator(this._dictionary);
            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this._dictionary);

            public struct Enumerator : IEnumerator<TKey>, IEnumerator {
                private readonly Dictionary<TKey, TValue> _dictionary;
                private readonly int _version;
                private int _index;
                private TKey _current;

                internal Enumerator(Dictionary<TKey, TValue> dictionary) {
                    this._dictionary = dictionary;
                    this._version = dictionary._version;
                    this._index = 0;
                    this._current = default(TKey);
                }

                public bool MoveNext() {
                    if (this._version != this._dictionary._version) throw new InvalidOperationException();
                    while ((uint)this._index < (uint)this._dictionary._count) {
                        if (this._dictionary._entries[this._index].hashCode >= 0) {
                            this._current = this._dictionary._entries[this._index].key;
                            this._index++;
                            return true;
                        }
                        this._index++;
                    }
                    this._index = this._dictionary._count + 1;
                    this._current = default(TKey);
                    return false;
                }

                public TKey Current => this._current;
                object IEnumerator.Current => this._current;
                void IEnumerator.Reset() {
                    if (this._version != this._dictionary._version) throw new InvalidOperationException();
                    this._index = 0;
                    this._current = default(TKey);
                }
                public void Dispose() { }
            }
        }

        public sealed class ValueCollection : ICollection<TValue>, ICollection, IEnumerable<TValue>, IEnumerable {
            private readonly Dictionary<TKey, TValue> _dictionary;
            internal ValueCollection(Dictionary<TKey, TValue> dictionary) {
                if (dictionary == null) throw new ArgumentNullException();
                this._dictionary = dictionary;
            }
            public int Count => this._dictionary.Count;
            public bool IsReadOnly => true;
            bool ICollection.IsSynchronized => false;
            object ICollection.SyncRoot => ((ICollection)this._dictionary).SyncRoot;

            public void CopyTo(TValue[] array, int index) {
                if (array == null) throw new ArgumentNullException();
                if ((uint)index > (uint)array.Length) throw new ArgumentOutOfRangeException();
                if (array.Length - index < this._dictionary.Count) throw new ArgumentException();
                var entries = this._dictionary._entries;
                for (var i = 0; i < this._dictionary._count; i++)
                    if (entries[i].hashCode >= 0) array[index++] = entries[i].value;
            }

            public bool Contains(TValue item) => this._dictionary.ContainsValue(item);

            void ICollection<TValue>.Add(TValue item) => throw new NotSupportedException();
            void ICollection<TValue>.Clear() => throw new NotSupportedException();
            bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException();

            void ICollection.CopyTo(Array array, int index) {
                if (array is TValue[] tvs) { this.CopyTo(tvs, index); return; }
                if (array is object[] objs) {
                    var entries = this._dictionary._entries;
                    for (var i = 0; i < this._dictionary._count; i++)
                        if (entries[i].hashCode >= 0) objs[index++] = entries[i].value;
                    return;
                }
                throw new ArgumentException();
            }

            public Enumerator GetEnumerator() => new Enumerator(this._dictionary);
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => new Enumerator(this._dictionary);
            IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this._dictionary);

            public struct Enumerator : IEnumerator<TValue>, IEnumerator {
                private readonly Dictionary<TKey, TValue> _dictionary;
                private readonly int _version;
                private int _index;
                private TValue _current;

                internal Enumerator(Dictionary<TKey, TValue> dictionary) {
                    this._dictionary = dictionary;
                    this._version = dictionary._version;
                    this._index = 0;
                    this._current = default(TValue);
                }

                public bool MoveNext() {
                    if (this._version != this._dictionary._version) throw new InvalidOperationException();
                    while ((uint)this._index < (uint)this._dictionary._count) {
                        if (this._dictionary._entries[this._index].hashCode >= 0) {
                            this._current = this._dictionary._entries[this._index].value;
                            this._index++;
                            return true;
                        }
                        this._index++;
                    }
                    this._index = this._dictionary._count + 1;
                    this._current = default(TValue);
                    return false;
                }

                public TValue Current => this._current;
                object IEnumerator.Current => this._current;
                void IEnumerator.Reset() {
                    if (this._version != this._dictionary._version) throw new InvalidOperationException();
                    this._index = 0;
                    this._current = default(TValue);
                }
                public void Dispose() { }
            }
        }
    }
}
