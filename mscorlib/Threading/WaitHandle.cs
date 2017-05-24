namespace System.Threading {
    using System;
    using System.Runtime.CompilerServices;
    public abstract class WaitHandle : MarshalByRefObject {
        public const int WaitTimeout = 0x102;
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public virtual bool WaitOne(int millisecondsTimeout, bool exitContext);
        public virtual bool WaitOne() => WaitOne(Timeout.Infinite, false);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int WaitMultiple(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext, bool WaitAll);

        public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext) => (WaitMultiple(waitHandles, millisecondsTimeout, exitContext, true /* waitall*/ ) != WaitTimeout);

        public static bool WaitAll(WaitHandle[] waitHandles) => WaitAll(waitHandles, Timeout.Infinite, true);

        public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext) => WaitMultiple(waitHandles, millisecondsTimeout, exitContext, false /* waitany*/ );

        public static int WaitAny(WaitHandle[] waitHandles) => WaitAny(waitHandles, Timeout.Infinite, true);
    }
}


