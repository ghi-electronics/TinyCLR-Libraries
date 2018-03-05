namespace System.Threading {
    using System;
    using System.Runtime.CompilerServices;

    // deliberately not [serializable]
    public sealed class Thread {
#pragma warning disable CS0169 // The field is never used
        private Delegate m_Delegate;
        private int m_Priority;
        [System.Reflection.FieldNoReflection]
        private object m_Thread;
        [System.Reflection.FieldNoReflection]
        private object m_AppDomain;
        private int m_Id;
#pragma warning restore CS0169 // The field is never used
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public Thread(ThreadStart start);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Start();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Abort();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Suspend();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Resume();
        extern public ThreadPriority Priority {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            set;
        }

        extern public int ManagedThreadId {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public bool IsAlive {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public void Join();
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public bool Join(int millisecondsTimeout);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public bool Join(TimeSpan timeout);
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static void Sleep(int millisecondsTimeout);
        extern public static Thread CurrentThread {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        extern public ThreadState ThreadState {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static AppDomain GetDomain();

        public override int GetHashCode() => this.ManagedThreadId;
    }

    ////// declaring a local var of this enum type and passing it by ref into a function that needs to do a
    ////// stack crawl will both prevent inlining of the calle and pass an ESP point to stack crawl to
}


