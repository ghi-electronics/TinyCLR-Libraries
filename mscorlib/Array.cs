using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System {
    [Serializable]
    // IList<object> is declared (not IList<T> for actual element T) because TinyCLR
    // uses full generic-type erasure: the MMP linker maps every generic VAR/MVAR to
    // DATATYPE_OBJECT, and the runtime's GENERICINST signature parser resolves to
    // the open TypeDef ignoring type args. So at runtime dispatch, IList<int>,
    // IList<string>, etc. all resolve to the same erased IList<T>, which IList<object>'s
    // metadata matches. The C# language spec separately guarantees SZ arrays of T are
    // assignable to IList<T>/IEnumerable<T>/etc. without checking the metadata, so the
    // compiler accepts `IEnumerable<int> e = new int[5];` regardless of what Array
    // declares here. This change makes the runtime dispatch find a matching method
    // for generic-interface callvirts on arrays, which is what was previously failing
    // with CLR_E_WRONG_TYPE.
    public abstract class Array : ICloneable, IList, IList<object> {
        internal const int MaxByteArrayLength = 0x7FFFFFC7;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern Array CreateInstance(Type elementType, int length);

        public static void Copy(Array sourceArray, Array destinationArray, int length) => Copy(sourceArray, 0, destinationArray, 0, length);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void Copy2D(Array sourceArray, Array destinationArray, int x, int y, int width, int height, int originalWidth, int elementGroupSize);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void Clear(Array array, int index, int length);

        public object GetValue(int index) => ((IList)this)[index];

        public extern int Length {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        // Members that are signature-identical between non-generic IList/ICollection
        // and IList<object>/ICollection<object> are declared as PUBLIC (implicit interface
        // implementation). Explicit-interface impls have qualified method names in
        // metadata that TinyCLR's runtime virtual-dispatch lookup doesn't match against
        // the erased open generic form, so callvirt on `IList<T>::get_Item` etc. would
        // fail. Public-implicit names ("get_Item", "Count", ...) match both interfaces
        // after erasure. See [[tinyclr-generic-erasure]] memory.
        //
        // Members with differing signatures (Add: int vs void, Remove: void vs bool,
        // CopyTo: Array vs object[]) stay split between explicit IList.X and explicit
        // ICollection<object>.X / IList<object>.X impls.

        public int Count => this.Length;

        public object SyncRoot => this;
        public bool IsReadOnly => false;
        public bool IsFixedSize => true;
        public bool IsSynchronized => false;

        public extern object this[int index] {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        int IList.Add(object value) => throw new NotSupportedException();

        public bool Contains(object value) => Array.IndexOf(this, value) >= 0;

        public void Clear() => Array.Clear(this, 0, this.Length);

        public int IndexOf(object value) => Array.IndexOf(this, value);

        public void Insert(int index, object value) => throw new NotSupportedException();

        void IList.Remove(object value) => throw new NotSupportedException();

        public void RemoveAt(int index) => throw new NotSupportedException();

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

        // Public method returns IEnumerator<object>, so plain-name vtable lookup
        // on Array finds a signature-matching GetEnumerator for both
        // IEnumerable.GetEnumerator (return type contravariance: IEnumerator<object>
        // IS an IEnumerator) AND IEnumerable<T>::GetEnumerator (erased return type
        // matches IEnumerator<object>'s erased return). Explicit interface impls
        // get qualified method names in metadata, which the runtime's plain-name
        // lookup doesn't find, so we need this implicit form for the LINQ-on-arrays
        // dispatch path.
        public IEnumerator<object> GetEnumerator() => new SZArrayEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        // --- ICollection<object> / IList<object> members with signatures that differ
        //     from the non-generic IList equivalents (Add returns void vs int, Remove
        //     returns bool vs void, CopyTo takes object[] vs Array). These must stay
        //     as explicit interface impls. ---

        void ICollection<object>.Add(object item) => throw new NotSupportedException();

        void ICollection<object>.CopyTo(object[] array, int arrayIndex) => Array.Copy(this, 0, array, arrayIndex, this.Length);

        bool ICollection<object>.Remove(object item) => throw new NotSupportedException();

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
        // Implements IEnumerator<object> too: after MMP erasure that satisfies
        // IEnumerator<T> for any T returned by Array's IEnumerable<object> impl
        // above. Value-type elements are returned boxed via Array.GetValue, and
        // user code's `unbox.any T` (emitted by the C# compiler after `e.Current`)
        // unboxes back to the static element type.
        internal class SZArrayEnumerator : IEnumerator<object> {
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

            public void Dispose() { }
        }
    }
}


