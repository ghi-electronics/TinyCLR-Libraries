namespace System {
    using System.Reflection;
    using System.Runtime.CompilerServices;
    [Serializable()]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public abstract class Delegate
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public override extern bool Equals(object obj);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern Delegate Combine(Delegate a, Delegate b);

        extern public MethodInfo Method {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public object Target {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern Delegate Remove(Delegate source, Delegate value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern bool operator ==(Delegate d1, Delegate d2);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern bool operator !=(Delegate d1, Delegate d2);

    }
}


