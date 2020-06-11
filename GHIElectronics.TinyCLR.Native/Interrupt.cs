using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Native {
    public static class Interrupt {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Enable();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Disable();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsDisabled();

    }
}
