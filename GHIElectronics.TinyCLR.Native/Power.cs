using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public enum PowerSleepLevel {
        Level0 = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4,
    }

    public static class Power {
        public static void Reset() => Power.Reset(true);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Reset(bool runCoreAfter);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Sleep(PowerSleepLevel sleepLevel);
    }
}
