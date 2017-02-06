namespace System.Reflection {
    using System;
    using System.Runtime.CompilerServices;

    [Serializable()]
    abstract public class PropertyInfo : MemberInfo {
        public abstract Type PropertyType {
            get;
        }

        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual object GetValue(object obj, object[] index);
        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern virtual void SetValue(object obj, object value, object[] index);
    }
}


