using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    internal enum PowerLevel : uint {
        Active = 0,
        Idle = 1,
        Off = 2,
        Sleep1 = 3,
        Sleep2 = 4,
        Sleep3 = 5,
        Custom = 0 | 0x80000000
    }

    [Flags]
    internal enum PowerWakeSource : ulong {
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

    public enum ResetSource : uint {
        System,
        PowerChanged,
        Watchdog,
        Other
    }

    public static class Power {

        public static void Reset() => Power.Reset(true);

        public static void Sleep() => SetLevel(PowerLevel.Sleep3, PowerWakeSource.Gpio, 0);

        public static void Sleep(DateTime wakeupTime) {
            var wakeupSource = PowerWakeSource.Gpio;
            var time = 0UL;

            if (wakeupTime != DateTime.MaxValue) {
                wakeupSource |= PowerWakeSource.Rtc;
                time = (ulong)wakeupTime.Ticks;
            }

            SetLevel(PowerLevel.Sleep3, wakeupSource, time);
        }

        public static void Shutdown(bool wakeupPin, DateTime wakeupTime) {
            PowerWakeSource wakeupSource = 0;
            var time = 0UL;

            if (wakeupTime != DateTime.MaxValue) {
                wakeupSource |= PowerWakeSource.Rtc;
                time = (ulong)wakeupTime.Ticks;
            }

            if (wakeupPin)
                wakeupSource |= PowerWakeSource.Gpio;

            SetLevel(PowerLevel.Off, wakeupSource, time);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Reset(bool runCoreAfter);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void SetLevel(PowerLevel powerLevel, PowerWakeSource wakeSource, ulong data);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern ResetSource GetResetSource();
    }
}
