namespace System.Reflection {

    using System;
    using System.Runtime.CompilerServices;

    //This class is marked serializable, but it's really the subclasses that
    //are responsible for handling the actual work of serialization if they need it.
    [Serializable()]
    abstract public class ConstructorInfo : MethodBase {
        public override MemberTypes MemberType => System.Reflection.MemberTypes.Constructor;
        [Diagnostics.DebuggerStepThrough]
        [Diagnostics.DebuggerHidden]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern object Invoke(object[] parameters);
    }
}


