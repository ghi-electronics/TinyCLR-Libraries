using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public sealed class Interop {
        private Interop() { }

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static extern void Add(IntPtr address);

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static extern void Remove(IntPtr address);

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static extern void RaiseEvent(string eventDispatcherName, string apiName, ulong data0, ulong data1, ulong data2, IntPtr data3, DateTime timestamp);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern Interop[] FindAll();

        public string Name { get; }
        public uint Checksum { get; }
        public IntPtr Methods { get; }
    }
}
