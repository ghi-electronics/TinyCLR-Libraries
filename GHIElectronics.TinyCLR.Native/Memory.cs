using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public static class Memory {
        public static IntPtr Allocate(long length) => Memory.Allocate((IntPtr)length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern IntPtr Allocate(IntPtr length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Free(IntPtr ptr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void GetStats(out IntPtr used, out IntPtr free);

        public static long UsedBytes { get { Memory.GetStats(out var used, out _); return used.ToInt64(); } }
        public static long FreeBytes { get { Memory.GetStats(out _, out var free); return free.ToInt64(); } }
    }
}
