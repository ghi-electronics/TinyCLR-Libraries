using System.Runtime.CompilerServices;
using System.Threading;

namespace System {
    public static class GC {
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool AnyPendingFinalizers();

        public static void WaitForPendingFinalizers() {
            while (GC.AnyPendingFinalizers())
                Thread.Sleep(10);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SuppressFinalize(object obj);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void ReRegisterForFinalize(object obj);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Collect();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern long GetTotalMemory(bool forceFullCollection);
    }
}
