using System.Collections;
using System.Runtime.CompilerServices;

namespace System {
    [Serializable]
    public abstract class Array : ICloneable, IList {
        internal const int MaxByteArrayLength = 0x7FFFFFC7;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern Array CreateInstance(Type elementType, int length);

        public static void Copy(Array sourceArray, Array destinationArray, int length) => Copy(sourceArray, 0, destinationArray, 0, length);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void Clear(Array array, int index, int length);

        public object GetValue(int index) => ((IList)this)[index];

        public extern int Length {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        int ICollection.Count => this.Length;

        public object SyncRoot => this;
        public bool IsReadOnly => false;
        public bool IsFixedSize => true;
        public bool IsSynchronized => false;
        extern object IList.this[int index] {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        int IList.Add(object value) => throw new NotSupportedException();

        bool IList.Contains(object value) => Array.IndexOf(this, value) >= 0;

        void IList.Clear() => Array.Clear(this, 0, this.Length);

        int IList.IndexOf(object value) => Array.IndexOf(this, value);

        void IList.Insert(int index, object value) => throw new NotSupportedException();

        void IList.Remove(object value) => throw new NotSupportedException();

        void IList.RemoveAt(int index) => throw new NotSupportedException();

        public object Clone() {
            var length = this.Length;
            var destArray = Array.CreateInstance(this.GetType().GetElementType(), length);
            Array.Copy(this, destArray, length);

            return destArray;
        }

        public static int BinarySearch(Array array, object value, IComparer comparer) => BinarySearch(array, 0, array.Length, value, comparer);

        public static int BinarySearch(Array array, int index, int length, object value, IComparer comparer) {
            var lo = index;
            var hi = index + length - 1;
            while (lo <= hi) {
                var i = (lo + hi) >> 1;

                int c;
                if (comparer == null) {
                    try {
                        var elementComparer = array.GetValue(i) as IComparable;
                        c = elementComparer.CompareTo(value);
                    }
                    catch (Exception e) {
                        throw new InvalidOperationException("Failed to compare two elements in the array", e);
                    }
                }
                else {
                    c = comparer.Compare(array.GetValue(i), value);
                }

                if (c == 0)
                    return i;
                if (c < 0) {
                    lo = i + 1;
                }
                else {
                    hi = i - 1;
                }
            }

            return ~lo;
        }

        public void CopyTo(Array array, int index) => Array.Copy(this, 0, array, index, this.Length);

        public IEnumerator GetEnumerator() => new SZArrayEnumerator(this);

        public static int IndexOf(Array array, object value) => IndexOf(array, value, 0, array.Length);

        public static int IndexOf(Array array, object value, int startIndex) => IndexOf(array, value, startIndex, array.Length - startIndex);

        public static int IndexOf(Array array, object value, int startIndex, int count) {
            // Try calling a quick native method to handle primitive types.

            if (TrySZIndexOf(array, startIndex, count, value, out var retVal)) {
                return retVal;
            }

            var endIndex = startIndex + count;

            for (var i = startIndex; i < endIndex; i++) {
                var obj = array.GetValue(i);

                if (Object.Equals(obj, value))
                    return i;
            }

            return -1;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern bool TrySZIndexOf(Array sourceArray, int sourceIndex, int count, object value, out int retVal);

        // This is the underlying Enumerator for all of our array-based data structures (Array, ArrayList, Stack, and Queue)
        // It supports enumerating over an array, a part of an array, and also will wrap around when the endIndex
        // specified is larger than the size of the array (to support Queue's internal circular array)
        internal class SZArrayEnumerator : IEnumerator {
            private Array _array;
            private int _index;
            private int _endIndex;
            private int _startIndex;
            private int _arrayLength;

            internal SZArrayEnumerator(Array array) {
                this._array = array;
                this._arrayLength = this._array.Length;
                this._endIndex = this._arrayLength;
                this._startIndex = 0;
                this._index = -1;
            }

            // By specifying the startIndex and endIndex, the enumerator will enumerate
            // only a subset of the array. Note that startIndex is inclusive, while
            // endIndex is NOT inclusive.
            // For example, if array is of size 5,
            // new SZArrayEnumerator(array, 0, 3) will enumerate through
            // array[0], array[1], array[2]
            //
            // This also supports an array acting as a circular data structure.
            // For example, if array is of size 5,
            // new SZArrayEnumerator(array, 4, 7) will enumerate through
            // array[4], array[0], array[1]
            internal SZArrayEnumerator(Array array, int startIndex, int endIndex) {
                this._array = array;
                this._arrayLength = this._array.Length;
                this._endIndex = endIndex;
                this._startIndex = startIndex;
                this._index = this._startIndex - 1;
            }

            public bool MoveNext() {
                if (this._index < this._endIndex) {
                    this._index++;
                    return (this._index < this._endIndex);
                }

                return false;
            }

            public object Current => this._array.GetValue(this._index % this._arrayLength);

            public void Reset() => this._index = this._startIndex - 1;
        }
    }
}


