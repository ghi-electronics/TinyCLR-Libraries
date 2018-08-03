using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public enum PowerSleepLevel {
        Sleep = 0,
        Hibernate = 1,
        Off = 2,
    }

    public static class Power {
        public static void Reset() => Power.Reset(true);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Reset(bool runCoreAfter);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Sleep(PowerSleepLevel sleepLevel);
    }
}
