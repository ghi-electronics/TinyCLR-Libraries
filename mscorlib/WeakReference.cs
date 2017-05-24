namespace System {
    using System.Runtime.CompilerServices;
    [Serializable()]
    public class WeakReference {

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern WeakReference(object target);

        public extern virtual bool IsAlive {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public extern virtual object Target {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

    }
}


