using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public static class Flash {
        public static void EnableExternalFlash() => NativeEnableExternalFlash();

        public static bool IsEnabledExternalFlash() => NativeIsEnabledExternalFlash();

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern void NativeEnableExternalFlash();

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern bool NativeIsEnabledExternalFlash();
    }
}
