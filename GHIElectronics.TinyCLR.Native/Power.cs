using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public enum PowerSleepLevel : uint {
        Level0 = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4,
        Custom = 0 | 0x80000000,
    }

    [Flags]
    public enum PowerSleepWakeSource : uint {
        Gpio = 0,
        Rtc = 1,
        SystemTimer = 2,
        Timer = 4,
        Network = 8,
        Can = 16,
        Uart = 32,
        UsbClient = 64,
        UsbHost = 128,
        Custom = 0 | 0x80000000,
    }

    public static class Power {
        public static void Reset() => Power.Reset(true);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Reset(bool runCoreAfter);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Sleep(PowerSleepLevel sleepLevel, PowerSleepWakeSource wakeSource);
    }
}
