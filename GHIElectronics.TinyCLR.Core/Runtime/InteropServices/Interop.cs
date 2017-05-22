using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices {
    public sealed class Interop {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Add(IntPtr address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Remove(IntPtr address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [CLSCompliant(false)]
        public static extern void RaiseEvent(string eventDispatcherName, string providerName, uint controllerIndex, ulong data0, ulong data1, IntPtr data2);

        public static extern IntPtr DynamicStructureAddress { [MethodImpl(MethodImplOptions.InternalCall)] get; }
    }
}
