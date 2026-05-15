using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    /// <summary>
    /// Represents an interop module — a native C library whose methods are callable
    /// from managed code via <c>[MethodImpl(InternalCall)]</c>. Use <see cref="FindAll"/>
    /// to enumerate every interop module the firmware exposes.
    /// </summary>
    public sealed class Interop {
        private Interop() { }

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static extern void Add(IntPtr address);

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static extern void Remove(IntPtr address);

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static extern void RaiseEvent(string eventDispatcherName, string apiName, ulong data0, ulong data1, ulong data2, IntPtr data3, DateTime timestamp);

        /// <summary>Returns every interop module registered with the runtime.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern Interop[] FindAll();

        /// <summary>The interop module's assembly name.</summary>
        public string Name { get; }
        /// <summary>CRC of the module's interop table — must match the managed assembly.</summary>
        public uint Checksum { get; }
        /// <summary>Pointer to the module's method-dispatch table.</summary>
        public IntPtr Methods { get; }
    }
}
