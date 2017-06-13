namespace System {
    using System.Runtime.CompilerServices;

    [Serializable()]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public abstract class MulticastDelegate : Delegate
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
    {

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static bool operator ==(MulticastDelegate d1, MulticastDelegate d2);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static bool operator !=(MulticastDelegate d1, MulticastDelegate d2);

    }
}


