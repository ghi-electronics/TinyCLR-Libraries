namespace System.Threading {
    using System.Runtime.CompilerServices;
    public sealed class ManualResetEvent : WaitHandle {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public ManualResetEvent(bool initialState);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public bool Reset();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public bool Set();
    }
}


