using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Update {
    public static class Application {
        public static bool IsLocked => NativeIsLocked();
        public static void Lock() => NativeLock();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeLock();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool NativeIsLocked();
    }
}
