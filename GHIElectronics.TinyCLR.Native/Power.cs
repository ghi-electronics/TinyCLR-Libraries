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

    /// <summary>Reasons reported by <see cref="Power.GetResetSource"/>.</summary>
    public enum ResetSource : uint {
        /// <summary>Reason not categorized.</summary>
        Other = 0,
        /// <summary>Cold start (rail came up from zero).</summary>
        PowerOn = 1,
        /// <summary>Hardware NRST line was asserted.</summary>
        ResetPin = 2,
        /// <summary>Supply rail dipped below the brown-out threshold.</summary>
        BrownoutReset = 4,
        /// <summary>Firmware-initiated soft reset (e.g. <see cref="Power.Reset"/>).</summary>
        SystemReset = 8,
        /// <summary>Watchdog timer expired without being kicked.</summary>
        WatchdogReset = 16,
        /// <summary>Resumed from low-power state by the RTC alarm.</summary>
        LowPowerRtc = 32,
        /// <summary>Resumed from low-power state by the wake-up pin.</summary>
        LowPowerWakeupPin = 64
    }

    /// <summary>Core-clock profile.</summary>
    public enum SystemClock : uint {
        /// <summary>Standard high-speed clock.</summary>
        High = 0,
        /// <summary>Reduced clock for lower power draw.</summary>
        Low = 1,
        /// <summary>Overclocked profile (may exceed datasheet spec).</summary>
        Overclock = 2,
    }

    /// <summary>Polarity of the wake-up pin.</summary>
    public enum WakeupEdge : uint {
        /// <summary>Wake on falling edge.</summary>
        Falling = 0,
        /// <summary>Wake on rising edge.</summary>
        Rising = 1,
    }

    /// <summary>
    /// Power-management entry points: <see cref="Reset"/> for a soft reset,
    /// <see cref="Sleep()"/> for low-power stop with wake-up, <see cref="Shutdown"/>
    /// for power-off with RTC/pin wake-up, and <see cref="SetSystemClock"/> for
    /// dynamic clock scaling.
    /// </summary>
    public static class Power {
        /// <summary>Polarity used by <see cref="Shutdown"/> when waking from a pin event. Applies to every pin set in the wake-up bit mask.</summary>
        public static WakeupEdge WakeupEdge;

        /// <summary>Soft-resets the device, re-running the app afterward.</summary>
        public static void Reset() => Power.Reset(true);

        /// <summary>Enters Sleep3 (deepest sleep). Wakes on any GPIO EXTI interrupt.</summary>
        public static void Sleep() => SetLevel(PowerLevel.Sleep3, PowerWakeSource.Gpio, 0, 0, 0);

        /// <summary>Enters Sleep3 with an optional wake time. Pass <see cref="DateTime.MaxValue"/> for "pin only".</summary>
        public static void Sleep(DateTime wakeupTime) {
            var wakeupSource = PowerWakeSource.Gpio;
            var time = 0UL;

            if (wakeupTime != DateTime.MaxValue) {
                wakeupSource |= PowerWakeSource.Rtc;
                time = (ulong)wakeupTime.Ticks;
            }

            SetLevel(PowerLevel.Sleep3, wakeupSource, time, 0, 0);
        }

        /// <summary>
        /// Powers off until either one of the selected wake-up pins asserts or the
        /// RTC alarm fires.
        /// </summary>
        /// <param name="wakeupPins">
        /// OR-combined wake-up pin bit mask. Each SoC's pin-package exposes a
        /// <c>WakeupPin</c> class (e.g. <c>SC20260.WakeupPin.PA0</c>) with one
        /// <c>int</c> constant per pin its hardware can wake on; OR them together
        /// to allow any of those pins to wake the device. Pass <c>0</c> for
        /// RTC-only wake. A bit not routed to this SoC's wake-up peripheral
        /// throws <see cref="ArgumentException"/>.
        /// <para>
        /// Example: <c>Power.Shutdown(SC20260.WakeupPin.PA0 | SC20260.WakeupPin.PA2, t)</c>.
        /// </para>
        /// </param>
        /// <param name="wakeupTime">
        /// Wall-clock time at which the RTC should wake the device, or
        /// <see cref="DateTime.MaxValue"/> for "pin only".
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="wakeupPins"/> is <c>0</c> AND <paramref name="wakeupTime"/>
        /// is <see cref="DateTime.MaxValue"/> (no wake source — the device would never
        /// come back), or when any bit in <paramref name="wakeupPins"/> isn't routed to
        /// this SoC's hardware wake-up peripheral.
        /// </exception>
        public static void Shutdown(int wakeupPins, DateTime wakeupTime) {
            if (wakeupPins == 0 && wakeupTime == DateTime.MaxValue)
                throw new ArgumentException("Shutdown requires at least one wake source: pass one or more WakeupPin bits or an RTC wakeupTime (or both).");

            PowerWakeSource wakeupSource = 0;
            var time = 0UL;

            if (wakeupTime != DateTime.MaxValue) {
                wakeupSource |= PowerWakeSource.Rtc;
                time = (ulong)wakeupTime.Ticks;
            }

            if (wakeupPins != 0)
                wakeupSource |= PowerWakeSource.Gpio;

            // Firmware validates each bit in wakeupPins against the SoC's hardware
            // wake-up routing when PowerWakeSource.Gpio is set — any unsupported
            // bit returns ArgumentInvalid which surfaces here as ArgumentException.
            SetLevel(PowerLevel.Off, wakeupSource, time, WakeupEdge, wakeupPins);
        }

        /// <summary>Switches the core-clock profile. <paramref name="persist"/> stores the choice across resets.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetSystemClock(SystemClock clock, bool persist);

        /// <summary>Returns the current core-clock profile.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern SystemClock GetSystemClock();

        /// <summary>Soft-resets the device. When <paramref name="runCoreAfter"/> is true, the app re-runs after reset; otherwise the device boots to bootloader.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Reset(bool runCoreAfter);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void SetLevel(PowerLevel powerLevel, PowerWakeSource wakeSource, ulong rtcTime, WakeupEdge wakeupEdge, int wakeupPins);

        /// <summary>Returns the reason for the most recent reset.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern ResetSource GetResetSource();
    }
}
