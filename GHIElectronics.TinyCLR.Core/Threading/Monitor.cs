namespace System.Threading {
    using System.Runtime.CompilerServices;
    public static class Monitor {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void Enter(object obj);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void Exit(object obj);
    }
}


