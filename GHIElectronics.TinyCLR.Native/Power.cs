using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public enum PowerLevel : uint {
        Active = 0,
        Idle = 1,
        Off = 2,
        Sleep1 = 3,
        Sleep2 = 4,
        Sleep3 = 5,
        Custom = 0 | 0x80000000
    }

    [Flags]
    public enum PowerWakeSource : ulong {
        Interrupt = 1,
        Gpio = 2,
        Rtc = 4,
        SystemTimer = 8,
        Timer = 16,
        Ethernet = 32,
        WiFi = 64,
        Can = 128,
        Uart = 256,
        UsbClient = 512,
        UsbHost = 1024,
        Charger = 2048,
        Custom = 0 | 0x80000000,
    }

    public static class Power {
        public static void Reset() => Power.Reset(true);

        public static void Hibernate() => SetLevel(PowerLevel.Sleep3, PowerWakeSource.Gpio, 0);

        public static void Shutdown(bool activeState) => SetLevel(PowerLevel.Off, PowerWakeSource.Gpio, activeState == false ? 0UL : 1);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Reset(bool runCoreAfter);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetLevel(PowerLevel powerLevel, PowerWakeSource wakeSource, ulong data);
    }
}
