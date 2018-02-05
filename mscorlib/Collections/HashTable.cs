
namespace System.Collections {
    /// <summary>
    /// HashTable is an Associative Container.
    /// Created in March 2010.
    /// by Eric Harlow.
    /// </summary>
    public class Hashtable : ICloneable, IDictionary {
        Entry[] _buckets;
        int _numberOfBuckets;
        int _count;
        int _loadFactor;
        int _maxLoadFactor;
        double _growthFactor;
        const int _defaultCapacity = 4;
        const int _defaultLoadFactor = 2;

        /// <summary>
        /// Initializes a new, empty instance of the Hashtable class using the default initial capacity and load factor.
        /// </summary>
        public Hashtable() => InitializeHashTable(_defaultCapacity, _defaultLoadFactor);

        /// <summary>
        /// Initializes a new, empty instance of the Hashtable class using the specified initial capacity,
        /// and the default load factor.
        /// </summary>
        /// <param name="capacity">The initial capacity of the HashTable</param>
        public Hashtable(int capacity) => InitializeHashTable(capacity, _defaultLoadFactor);

        /// <summary>
        /// Initializes a new, empty instance of the Hashtable class using the specified initial capacity,
        /// load factor.
        /// </summary>
        /// <param name="capacity">The initial capacity of the HashTable</param>
        /// <param name="maxLoadFactor">The load factor to cause a rehash</param>
        public Hashtable(int capacity, int maxLoadFactor) => InitializeHashTable(capacity, maxLoadFactor);

        //initialize attributes
        private void InitializeHashTable(int capacity, int maxLoadFactor) {
            this._buckets = new Entry[capacity];
            this._numberOfBuckets = capacity;
            this._maxLoadFactor = maxLoadFactor;
            this._growthFactor = 2;
        }

        /// <summary>
        /// MaxLoadFactor Property is the value used to trigger a rehash.
        /// Default value is 2.
        /// A higher number can decrease lookup performance for large collections.
        /// While a value of 1 maintains a constant time complexity at the cost of increased memory requirements.
        /// </summary>
        public int MaxLoadFactor {
            get => this._maxLoadFactor; set => this._maxLoadFactor = value;
        }

        /// <summary>
        /// GrowthFactor Property is a multiplier to increase the HashTable size by during a rehash.
        /// Default value is 2.
        /// </summary>
        public double GrowthFactor {
            get => this._growthFactor; set => this._growthFactor = value;
        }

        //adding for internal purposes
        private void Add(ref Entry[] buckets, object key, object value, bool overwrite) {
            var whichBucket = Hash(key, this._numberOfBuckets);
            var match = EntryForKey(key, buckets[whichBucket]);

            if (match != null && overwrite) { //i.e. already exists in table
                match.value = value;
                return;
            }
            else if ((match != null && !overwrite)) {
                throw new ArgumentException("key exists");
            }
            else {            // insert at front
                var newOne = new Entry(key, value, ref buckets[whichBucket]);
                buckets[whichBucket] = newOne;
                this._count++;
            }

            this._loadFactor = this._count / this._numberOfBuckets;
        }

        // Hash function.
        private int Hash(object key, int numOfBuckets) {
            var hashcode = key.GetHashCode();

            if (hashcode < 0) // don't know how to mod with a negative number
                hashcode = hashcode * -1;

            return hashcode % numOfBuckets;
        }

        //looks up value in bucket
        private Entry EntryForKey(object key, Entry head) {
            for (var cur = head; cur != null; cur = cur.next)
                if (cur.key.Equals(key))
                    return cur;
            return null;
        }

        //Rehashes the table to reduce the load factor
        private void Rehash(int newSize) {
            var newTable = new Entry[newSize];
            this._numberOfBuckets = newSize;
            this._count = 0;
            for (var i = 0; i < this._buckets.Length; i++) {
                if (this._buckets[i] != null) {
                    for (var cur = this._buckets[i]; cur != null; cur = cur.next)
                        Add(ref newTable, cur.key, cur.value, false);
                }
            }
            this._buckets = newTable;
        }

        //implementation for KeyCollection and ValueCollection copyTo method
        private void CopyToCollection(Array array, int index, EnumeratorType type) {
            if (index < 0 && index > this._numberOfBuckets)
                throw new IndexOutOfRangeException("index");

            var j = 0;
            var len = array.Length;

            for (var i = index; i < this._numberOfBuckets; i++) {
                for (var cur = this._buckets[i]; cur != null && j < len; cur = cur.next) {
                    if (type == EnumeratorType.KEY)
                        ((IList)array)[j] = cur.key;
                    else
                        ((IList)array)[j] = cur.value;

                    j++;
                }
            }
        }

        #region ICloneable Members
        //shallow copy
        public object Clone() {
            var ht = new Hashtable();
            ht.InitializeHashTable(this._numberOfBuckets, this._maxLoadFactor);
            ht._count = this._count;
            ht._loadFactor = this._loadFactor;
            Array.Copy(this._buckets, ht._buckets, this._numberOfBuckets);
            return ht;
        }

        #endregion ICloneable Members

        #region IEnumerable Members

        public IEnumerator GetEnumerator() => new HashtableEnumerator(this, EnumeratorType.DE);

        #endregion IEnumerable Members

        #region ICollection Members

        public int Count => this._count;
        public bool IsSynchronized => false;
        public object SyncRoot => this;
        public void CopyTo(Array array, int index) {
            if (index < 0 && index > this._buckets.Length)
                throw new IndexOutOfRangeException("index");

            var j = 0;
            var len = array.Length;
            for (var i = index; i < this._buckets.Length; i++) {
                for (var cur = this._buckets[i]; cur != null && j < len; cur = cur.next) {
                    ((IList)array)[j] = new DictionaryEntry(cur.key, cur.value);
                    j++;
                }
            }
        }

        #endregion ICollection Members

        #region IDictionary Members

        public bool IsReadOnly => false;
        public bool IsFixedSize => false;
        public ICollection Keys => new KeyCollection(this);

        public ICollection Values => new ValueCollection(this);

        public object this[object key] {
            get {
                if (key == null)
                    throw new ArgumentNullException("key is null");
                var whichBucket = Hash(key, this._numberOfBuckets);
                var match = EntryForKey(key, this._buckets[whichBucket]);
                if (match != null)
                    return match.value;
                return null;
            }
            set {
                if (key == null)
                    throw new ArgumentNullException("key is null");

                Add(ref this._buckets, key, value, true);
                if (this._loadFactor >= this._maxLoadFactor)
                    Rehash((int)(this._numberOfBuckets * this._growthFactor));
            }
        }

        public void Add(object key, object value) {
            if (key == null)
                throw new ArgumentNullException("key is null");

            Add(ref this._buckets, key, value, false);
            if (this._loadFactor >= this._maxLoadFactor)
                Rehash((int)(this._numberOfBuckets * this._growthFactor));
        }

        public void Clear() {
            this._buckets = new Entry[_defaultCapacity];
            this._numberOfBuckets = _defaultCapacity;
            this._loadFactor = 0;
            this._count = 0;
        }

        public bool Contains(object key) {
            if (key == null)
                throw new ArgumentNullException("key is null");
            var whichBucket = Hash(key, this._numberOfBuckets);
            var match = EntryForKey(key, this._buckets[whichBucket]);

            if (match != null)
                return true;
            return false;
        }

        public void Remove(object key) {
            if (key == null)
                throw new ArgumentNullException("key is null");
            var whichBucket = Hash(key, this._numberOfBuckets);
            var match = EntryForKey(key, this._buckets[whichBucket]);

            //does entry exist?
            if (match == null)
                return;

            //is entry at front?
            if (this._buckets[whichBucket] == match) {
                this._buckets[whichBucket] = match.next;
                this._count--;
                return;
            }

            //handle entry in middle and at the end
            for (var cur = this._buckets[whichBucket]; cur != null; cur = cur.next) {
                if (cur.next == match) {
                    cur.next = match.next;
                    this._count--;
                    return;
                }
            }
        }

        #endregion IDictionary Members

        private class Entry {
            public object key;
            public object value;
            public Entry next;

            public Entry(object key, object value, ref Entry n) {
                this.key = key;
                this.value = value;
                this.next = n;
            }
        }

        private class HashtableEnumerator : IEnumerator {
            Hashtable ht;
            Entry temp;
            int index = -1;
            EnumeratorType returnType;

            public HashtableEnumerator(Hashtable hashtable, EnumeratorType type) {
                this.ht = hashtable;
                this.returnType = type;
            }

            // Return the current item.
            public object Current {
                get {
                    switch (this.returnType) {
                        case EnumeratorType.DE:
                            return new DictionaryEntry(this.temp.key, this.temp.value);

                        case EnumeratorType.KEY:
                            return this.temp.key;

                        case EnumeratorType.VALUE:
                            return this.temp.value;

                        default:
                            break;
                    }
                    return new DictionaryEntry(this.temp.key, this.temp.value);
                }
            }

            // Advance to the next item.
            public bool MoveNext() {
                startLoop:
                //iterate index or list
                if (this.temp == null) {
                    this.index++;
                    if (this.index < this.ht._numberOfBuckets)
                        this.temp = this.ht._buckets[this.index];
                    else
                        return false;
                }
                else
                    this.temp = this.temp.next;

                //null check
                if (this.temp == null)
                    goto startLoop;

                return true;
            }

            // Reset the index to restart the enumeration.
            public void Reset() => this.index = -1;
        }

        // EnumeratorType - Enum that describes which object the Enumerator's Current property will return.
        private enum EnumeratorType {
            // DictionaryEntry object.
            DE,

            // Key object.
            KEY,

            // Value object.
            VALUE
        }

        private class KeyCollection : ICollection {
            Hashtable ht;

            public KeyCollection(Hashtable hashtable) => this.ht = hashtable;

            #region ICollection Members

            public int Count => this.ht._count;

            public bool IsSynchronized => this.ht.IsSynchronized;

            public object SyncRoot => this.ht.SyncRoot;

            public void CopyTo(Array array, int index) => this.ht.CopyToCollection(array, index, EnumeratorType.KEY);

            #endregion

            #region IEnumerable Members

            public IEnumerator GetEnumerator() => new HashtableEnumerator(this.ht, EnumeratorType.KEY);

            #endregion
        }

        private class ValueCollection : ICollection {
            Hashtable ht;

            public ValueCollection(Hashtable hashtable) => this.ht = hashtable;

            #region ICollection Members

            public int Count => this.ht._count;

            public bool IsSynchronized => this.ht.IsSynchronized;

            public object SyncRoot => this.ht.SyncRoot;

            public void CopyTo(Array array, int index) => this.ht.CopyToCollection(array, index, EnumeratorType.VALUE);

            #endregion

            #region IEnumerable Members

            public IEnumerator GetEnumerator() => new HashtableEnumerator(this.ht, EnumeratorType.VALUE);

            #endregion
        }
    }
}
