namespace System.Runtime.CompilerServices {

    using System;
    [Serializable]
    public static class RuntimeHelpers {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void InitializeArray(Array array, RuntimeFieldHandle fldHandle);

        /**
         * GetObjectValue is intended to allow value classes to be manipulated as 'Object'
         * but have aliasing behavior of a value class.  The intent is that you would use
         * this function just before an assignment to a variable of type 'Object'.  If the
         * value being assigned is a mutable value class, then a shallow copy is returned
         * (because value classes have copy semantics), but otherwise the object itself
         * is returned.
         *
         * Note: VB calls this method when they're about to assign to an Object
         * or pass it as a parameter.  The goal is to make sure that boxed
         * value types work identical to unboxed value types - ie, they get
         * cloned when you pass them around, and are always passed by value.
         * Of course, reference types are not cloned.  -- BrianGru  7/12/2001
         *
         * @param obj The object that is about to be assigned.
         * @return a shallow copy of 'obj' if it is a value class, 'obj' itself otherwise
         */
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern object GetObjectValue(object obj);

        /**
         * RunClassConstructor causes the class constructor for the given type to be triggered
         * in the current domain.  After this call returns, the class constructor is guaranteed to
         * have at least been started by some thread.  In the absence of class constructor
         * deadlock conditions, the call is further guaranteed to have completed.
         *
         * This call will generate an exception if the specified class constructor threw an
         * exception when it ran.
         */

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void RunClassConstructor(RuntimeTypeHandle type);

        extern public static int OffsetToStringData {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        // C# 8 indices/ranges: the compiler emits a call to this helper for
        // `arr[range]` where arr is a single-dimension array. Pure managed -
        // resolves the Range against the array length and copies a fresh slice.
        public static T[] GetSubArray<T>(T[] array, Range range) {
            if (array == null) throw new ArgumentNullException();
            var ol = range.GetOffsetAndLength(array.Length);
            var offset = ol.Item1;
            var length = ol.Item2;
            var dest = new T[length];
            if (length > 0) Array.Copy(array, offset, dest, 0, length);
            return dest;
        }
    }
}


