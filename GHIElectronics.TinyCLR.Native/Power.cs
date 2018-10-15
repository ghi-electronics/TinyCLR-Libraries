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
        Interrupt = 1,
        Gpio = 2,
        Rtc = 4,
        SystemTimer = 8,
        Timer = 16,
        Ethernet = 32,
        Wifi = 64,
        Can = 128,
        Uart = 256,
        UsbClient = 512,
        UsbHost = 1024,
        Charger = 2048,
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
