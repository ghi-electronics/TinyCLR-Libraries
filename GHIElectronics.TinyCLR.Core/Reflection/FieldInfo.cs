namespace System.Reflection {
    using System;
    using System.Runtime.CompilerServices;

    [Serializable()]
    abstract public class FieldInfo : MemberInfo {

        /**
         * The Member type Field.
         */
        public override MemberTypes MemberType => System.Reflection.MemberTypes.Field;

        public abstract Type FieldType {
            get;
        }

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        public abstract object GetValue(object obj);
        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void SetValue(object obj, object value);
    }
}


