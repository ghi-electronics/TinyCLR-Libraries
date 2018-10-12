using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public enum PowerLevel : uint {
        Active = 0,
        Idle = 1,
        Sleep1 = 2,
        Sleep2 = 3,
        Sleep3 = 4,
        Off = 5,
        Custom = 0 | 0x80000000
    }

    [Flags]
    public enum PowerSleepWakeSource : ulong {
        Interrupt = 0,
        Gpio = 1,
        Rtc = 2,
        SystemTimer = 4,
        Timer = 8,
        Ethernet = 16,
        Wifi = 32,
        Can = 64,
        Uart = 128,
        UsbClient = 256,
        UsbHost = 512,
        Charger = 1024,
        Custom = 0 | 0x80000000,
    }

    public static class Power {
        public static void Reset() => Power.Reset(true);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Reset(bool runCoreAfter);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetLevel(PowerLevel powerLevel, PowerSleepWakeSource wakeSource, ulong data);
    }
}
