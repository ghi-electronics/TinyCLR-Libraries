using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections {
    /// <summary>
    /// An array implementation of a stack. Push can be O(n). Pop is O(1).
    /// </summary>
    [Serializable()]
    [DebuggerDisplay("Count = {Count}")]
    public class Stack : ICollection, ICloneable {
        private object[] _array;     // Storage for stack elements
        private int _size;           // Number of items in the stack.

        // Keep in-sync with c_DefaultCapacity in CLR_RT_HeapBlock_Stack in TinyCLR_Runtime__HeapBlock.h
        private const int _defaultCapacity = 4;

        /// <summary>
        /// Initializes a new instance of the Stack class that is empty and has the default initial capacity.
        /// </summary>
        public Stack() {
            this._array = new object[_defaultCapacity];
            this._size = 0;
        }

        /// <summary>
        /// Size of the stack
        /// </summary>
        public virtual int Count => this._size;
        /// <summary>
        /// Returns whether the current stack is synchornized. Always return false.
        /// </summary>
        public virtual bool IsSynchronized => false;
        /// <summary>
        /// Gets an object that can be used to synchronize access to the Stack.
        /// </summary>
        public virtual object SyncRoot => this;
        /// <summary>
        /// Removes all Objects from the Stack.
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void Clear();

        /// <summary>
        /// Creates a shallow copy of the Stack.
        /// </summary>
        /// <returns>A shallow copy of the Stack.</returns>
        public virtual object Clone() {
            var s = new Stack();
            var capacity = _defaultCapacity;

            if (this._size > _defaultCapacity) {
                // only re-allocate a new array if the size isn't what we need.
                // otherwise, the one allocated in the constructor will be just fine
                s._array = new object[this._size];
                capacity = this._size;
            }

            s._size = this._size;
            Array.Copy(this._array, this._array.Length - this._size, s._array, capacity - this._size, this._size);
            return s;
        }

        /// <summary>
        /// Determines whether an element is in the Stack.
        /// </summary>
        /// <param name="obj">The Object to locate in the Stack.</param>
        /// <returns>true, if obj is found in the Stack; otherwise, false</returns>
        public virtual bool Contains(object obj) => Array.IndexOf(this._array, obj, this._array.Length - this._size, this._size) >= 0;

        /// <summary>
        /// Copies the Stack to an existing one-dimensional Array, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination of the elements copied from Stack.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public virtual void CopyTo(Array array, int index) => Array.Copy(this._array, this._array.Length - this._size, array, index, this._size);

        /// <summary>
        /// Returns an IEnumerator for this Stack.
        /// </summary>
        /// <returns>An IEnumerator for the Stack.</returns>
        public virtual IEnumerator GetEnumerator() {
            var capacity = this._array.Length;
            return new Array.SZArrayEnumerator(this._array, capacity - this._size, capacity);
        }

        /// <summary>
        /// Returns the object at the top of the Stack without removing it.
        /// </summary>
        /// <returns>The Object at the top of the Stack.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual object Peek();

        /// <summary>
        /// Removes and returns the object at the top of the Stack.
        /// </summary>
        /// <returns>The Object removed from the top of the Stack.</returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual object Pop();

        /// <summary>
        /// Inserts an object at the top of the Stack.
        /// </summary>
        /// <param name="obj">The Object to push onto the Stack.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void Push(object obj);

        /// <summary>
        /// Copies the Stack to a new array, in the same order Pop would return the items.
        /// </summary>
        /// <returns>A new array containing copies of the elements of the Stack.</returns>
        public virtual object[] ToArray() {
            var objArray = new object[this._size];

            Array.Copy(this._array, this._array.Length - this._size, objArray, 0, this._size);

            return objArray;
        }
    }
}


