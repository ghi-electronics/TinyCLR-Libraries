namespace System.Threading {
    using System;
    using System.Runtime.CompilerServices;

    public delegate void TimerCallback(object state);

    public sealed class Timer : MarshalByRefObject, IDisposable {
        [System.Reflection.FieldNoReflection]
#pragma warning disable CS0169 // The field is never used
        private object m_timer;
        private object m_state;
        private TimerCallback m_callback;
#pragma warning restore CS0169 // The field is never used
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public Timer(TimerCallback callback, object state, int dueTime, int period);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public Timer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public bool Change(int dueTime, int period);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public bool Change(TimeSpan dueTime, TimeSpan period);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Dispose();
    }
}


