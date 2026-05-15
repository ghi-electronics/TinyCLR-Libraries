using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    /// <summary>
    /// Controls how the firmware uses the device's flash storage. Most apps don't
    /// need this — call <see cref="EnableExtendDeployment"/> only on devices with
    /// external flash that should be used to extend the deployment region.
    /// </summary>
    public static class Flash {
        /// <summary>Permanently enables external flash as an extension of the deployment region.</summary>
        public static void EnableExtendDeployment() => NativeEnableExternalFlash();

        /// <summary>True when external-flash deployment extension is enabled.</summary>
        public static bool IsEnabledExtendDeployment => NativeIsEnabledExternalFlash();

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern void NativeEnableExternalFlash();

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern bool NativeIsEnabledExternalFlash();
    }
}
