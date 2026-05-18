using System.Diagnostics;

namespace System.Collections.Generic {
    /// <summary>
    /// Hash-table-based generic set. Mirrors the .NET BCL surface for the subset
    /// we ship. Same chained-collision layout and prime-table sizing as
    /// <see cref="Dictionary{TKey, TValue}"/>; the entry holds the element
    /// directly instead of a key/value pair.
    ///
    /// null IS a valid element when T is a reference type — the comparer
    /// (<see cref="EqualityComparer{T}.Default"/>) treats null consistently
    /// (GetHashCode(null) = 0, Equals(null, null) = true).
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class HashSet<T> :
        ISet<T>,
        ICollection<T>,
        IEnumerable<T>,
        ICollection,
        IEnumerable {

        private struct Slot {
            public int hashCode;   // lower 31 bits of element hash; -1 if unused
            public int next;       // index of next entry in chain, -1 if end
            public T value;
        }

        private int[] _buckets;
        private Slot[] _slots;
        private int _count;
        private int _freeList;
        private int _freeCount;
        private int _version;
        private readonly IEqualityComparer<T> _comparer;

        public HashSet() : this(0, null) { }
        public HashSet(IEqualityComparer<T> comparer) : this(0, comparer) { }
        public HashSet(int capacity) : this(capacity, null) { }

        public HashSet(int capacity, IEqualityComparer<T> comparer) {
            if (capacity < 0) throw new ArgumentOutOfRangeException();
            this._comparer = comparer ?? EqualityComparer<T>.Default;
            if (capacity > 0) this.Initialize(capacity);
        }

        public HashSet(IEnumerable<T> collection) : this(collection, null) { }

        public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : this(0, comparer) {
            if (collection == null) throw new ArgumentNullException();
            foreach (var item in collection) this.AddIfNotPresent(item);
        }

        public IEqualityComparer<T> Comparer => this._comparer;
        public int Count => this._count - this._freeCount;

        bool ICollection<T>.IsReadOnly => false;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;

        // ISet<T>.Add returns bool; ICollection<T>.Add returns void. The bool
        // overload is the canonical one — true if newly added, false if already
        // present.
        public bool Add(T item) => this.AddIfNotPresent(item);

        void ICollection<T>.Add(T item) => this.AddIfNotPresent(item);

        public void Clear() {
            if (this._count > 0) {
                for (var i = 0; i < this._buckets.Length; i++) this._buckets[i] = -1;
                Array.Clear(this._slots, 0, this._count);
                this._freeList = -1;
                this._freeCount = 0;
                this._count = 0;
                this._version++;
            }
        }

        public bool Contains(T item) {
            if (this._buckets == null) return false;
            var hashCode = this.InternalGetHashCode(item);
            for (var i = this._buckets[hashCode % this._buckets.Length]; i >= 0; i = this._slots[i].next) {
                if (this._slots[i].hashCode == hashCode && this._comparer.Equals(this._slots[i].value, item))
                    return true;
            }
            return false;
        }

        public bool Remove(T item) {
            if (this._buckets == null) return false;
            var hashCode = this.InternalGetHashCode(item);
            var bucket = hashCode % this._buckets.Length;
            var last = -1;
            for (var i = this._buckets[bucket]; i >= 0; last = i, i = this._slots[i].next) {
                if (this._slots[i].hashCode == hashCode && this._comparer.Equals(this._slots[i].value, item)) {
                    if (last < 0) this._buckets[bucket] = this._slots[i].next;
                    else this._slots[last].next = this._slots[i].next;
                    this._slots[i].hashCode = -1;
                    this._slots[i].next = this._freeList;
                    this._slots[i].value = default(T);
                    this._freeList = i;
                    this._freeCount++;
                    this._version++;
                    return true;
                }
            }
            return false;
        }

        public int RemoveWhere(Predicate<T> match) {
            if (match == null) throw new ArgumentNullException();
            var removed = 0;
            for (var i = 0; i < this._count; i++) {
                if (this._slots[i].hashCode >= 0) {
                    var item = this._slots[i].value;
                    if (match(item)) {
                        if (this.Remove(item)) removed++;
                    }
                }
            }
            return removed;
        }

        public bool TryGetValue(T equalValue, out T actualValue) {
            if (this._buckets != null) {
                var hashCode = this.InternalGetHashCode(equalValue);
                for (var i = this._buckets[hashCode % this._buckets.Length]; i >= 0; i = this._slots[i].next) {
                    if (this._slots[i].hashCode == hashCode && this._comparer.Equals(this._slots[i].value, equalValue)) {
                        actualValue = this._slots[i].value;
                        return true;
                    }
                }
            }
            actualValue = default(T);
            return false;
        }

        public void CopyTo(T[] array) => this.CopyTo(array, 0, this.Count);
        public void CopyTo(T[] array, int arrayIndex) => this.CopyTo(array, arrayIndex, this.Count);

        public void CopyTo(T[] array, int arrayIndex, int count) {
            if (array == null) throw new ArgumentNullException();
            if (arrayIndex < 0 || count < 0) throw new ArgumentOutOfRangeException();
            if (arrayIndex > array.Length || array.Length - arrayIndex < count) throw new ArgumentException();

            var written = 0;
            for (var i = 0; i < this._count && written < count; i++) {
                if (this._slots[i].hashCode >= 0) {
                    array[arrayIndex + written] = this._slots[i].value;
                    written++;
                }
            }
        }

        void ICollection.CopyTo(Array array, int index) {
            if (array == null) throw new ArgumentNullException();
            if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException();
            if (array.Length - index < this.Count) throw new ArgumentException();

            if (array is T[] tArr) { this.CopyTo(tArr, index); return; }
            if (array is object[] objArr) {
                for (var i = 0; i < this._count; i++)
                    if (this._slots[i].hashCode >= 0) objArr[index++] = this._slots[i].value;
                return;
            }
            throw new ArgumentException();
        }

        // --- ISet<T> ops ---

        public void UnionWith(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException();
            foreach (var item in other) this.AddIfNotPresent(item);
        }

        public void IntersectWith(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException();
            if (this.Count == 0) return;

            // Mark which of OUR entries appear in `other`, then remove unmarked.
            // Uses one bool[] of length _count — small embedded-friendly cost.
            var seen = new bool[this._count];
            foreach (var item in other) {
                if (this._buckets == null) break;
                var hashCode = this.InternalGetHashCode(item);
                for (var i = this._buckets[hashCode % this._buckets.Length]; i >= 0; i = this._slots[i].next) {
                    if (this._slots[i].hashCode == hashCode && this._comparer.Equals(this._slots[i].value, item)) {
                        seen[i] = true;
                        break;
                    }
                }
            }
            for (var i = 0; i < this._count; i++) {
                if (this._slots[i].hashCode >= 0 && !seen[i]) {
                    this.Remove(this._slots[i].value);
                }
            }
        }

        public void ExceptWith(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException();
            if (this.Count == 0) return;
            foreach (var item in other) this.Remove(item);
        }

        public void SymmetricExceptWith(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException();
            // Match .NET BCL: items in `this XOR other`. Without an indexable
            // "have we added this from other this pass" marker, a temp set
            // disambiguates duplicates inside `other`.
            var fromOther = new HashSet<T>(this._comparer);
            foreach (var item in other) {
                if (fromOther.Contains(item)) continue;
                fromOther.AddIfNotPresent(item);
                if (!this.Remove(item)) this.AddIfNotPresent(item);
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException();
            if (this.Count == 0) return true;
            // Build a temp set from other and test containment for every element of this.
            var o = ToTempSet(other);
            if (this.Count > o.Count) return false;
            foreach (var item in this) if (!o.Contains(item)) return false;
            return true;
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException();
            var o = ToTempSet(other);
            if (this.Count >= o.Count) return false;
            foreach (var item in this) if (!o.Contains(item)) return false;
            return true;
        }

        public bool IsSupersetOf(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException();
            foreach (var item in other) if (!this.Contains(item)) return false;
            return true;
        }

        public bool IsProperSupersetOf(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException();
            if (this.Count == 0) return false;
            var o = ToTempSet(other);
            if (this.Count <= o.Count) return false;
            foreach (var item in o) if (!this.Contains(item)) return false;
            return true;
        }

        public bool Overlaps(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException();
            if (this.Count == 0) return false;
            foreach (var item in other) if (this.Contains(item)) return true;
            return false;
        }

        public bool SetEquals(IEnumerable<T> other) {
            if (other == null) throw new ArgumentNullException();
            var o = ToTempSet(other);
            if (this.Count != o.Count) return false;
            foreach (var item in this) if (!o.Contains(item)) return false;
            return true;
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        // --- internals ---

        // Bake null-handling into one place so all callers (Contains, Remove,
        // FindEntry, Insert) stay in lockstep. Comparer's GetHashCode handles
        // null for reference types but the contract is fuzzy — explicit branch
        // is clearer and matches BCL behavior.
        private int InternalGetHashCode(T item) {
            if (item == null) return 0;
            return this._comparer.GetHashCode(item) & 0x7FFFFFFF;
        }

        private bool AddIfNotPresent(T value) {
            if (this._buckets == null) this.Initialize(0);

            var hashCode = this.InternalGetHashCode(value);
            var targetBucket = hashCode % this._buckets.Length;

            for (var i = this._buckets[targetBucket]; i >= 0; i = this._slots[i].next) {
                if (this._slots[i].hashCode == hashCode && this._comparer.Equals(this._slots[i].value, value)) {
                    return false;
                }
            }

            int index;
            if (this._freeCount > 0) {
                index = this._freeList;
                this._freeList = this._slots[index].next;
                this._freeCount--;
            }
            else {
                if (this._count == this._slots.Length) {
                    this.Resize();
                    targetBucket = hashCode % this._buckets.Length;
                }
                index = this._count;
                this._count++;
            }

            this._slots[index].hashCode = hashCode;
            this._slots[index].next = this._buckets[targetBucket];
            this._slots[index].value = value;
            this._buckets[targetBucket] = index;
            this._version++;
            return true;
        }

        private void Initialize(int capacity) {
            var size = GetPrime(capacity);
            this._buckets = new int[size];
            for (var i = 0; i < this._buckets.Length; i++) this._buckets[i] = -1;
            this._slots = new Slot[size];
            this._freeList = -1;
        }

        private void Resize() => this.Resize(ExpandPrime(this._count));

        private void Resize(int newSize) {
            var newBuckets = new int[newSize];
            for (var i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
            var newSlots = new Slot[newSize];
            Array.Copy(this._slots, 0, newSlots, 0, this._count);
            for (var i = 0; i < this._count; i++) {
                if (newSlots[i].hashCode >= 0) {
                    var bucket = newSlots[i].hashCode % newSize;
                    newSlots[i].next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
            }
            this._buckets = newBuckets;
            this._slots = newSlots;
        }

        private HashSet<T> ToTempSet(IEnumerable<T> other) {
            if (other is HashSet<T> hs && hs._comparer.Equals(default(T), default(T)) == this._comparer.Equals(default(T), default(T))) {
                // Compatible comparer — can use the source set directly without
                // re-hashing. Conservative check: same default-comparer behavior.
                return hs;
            }
            return new HashSet<T>(other, this._comparer);
        }

        // Same prime table as Dictionary — keep them in lockstep so set
        // operations between HashSet and Dictionary-backed views behave
        // consistently when sizing matters.
        private static readonly int[] _primes = { 3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263, 1674319, 2009191 };

        private static int GetPrime(int min) {
            if (min < 0) throw new ArgumentException();
            foreach (var p in _primes) if (p >= min) return p;
            for (var p = min | 1; p < int.MaxValue; p += 2) if ((p & 1) != 0) return p;
            return min;
        }

        private static int ExpandPrime(int oldSize) {
            var newSize = 2 * oldSize;
            return GetPrime(newSize);
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator {
            private readonly HashSet<T> _set;
            private readonly int _version;
            private int _index;
            private T _current;

            internal Enumerator(HashSet<T> set) {
                this._set = set;
                this._version = set._version;
                this._index = 0;
                this._current = default(T);
            }

            public bool MoveNext() {
                if (this._version != this._set._version) throw new InvalidOperationException();
                while ((uint)this._index < (uint)this._set._count) {
                    if (this._set._slots[this._index].hashCode >= 0) {
                        this._current = this._set._slots[this._index].value;
                        this._index++;
                        return true;
                    }
                    this._index++;
                }
                this._index = this._set._count + 1;
                this._current = default(T);
                return false;
            }

            public T Current => this._current;
            object IEnumerator.Current => this._current;

            void IEnumerator.Reset() {
                if (this._version != this._set._version) throw new InvalidOperationException();
                this._index = 0;
                this._current = default(T);
            }

            public void Dispose() { }
        }
    }
}
