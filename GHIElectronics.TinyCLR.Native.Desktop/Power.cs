using System;

namespace GHIElectronics.TinyCLR.Native {
    public enum ResetSource : uint {
        Other = 0,
        PowerOn = 1,
        ResetPin = 2,
        BrownoutReset = 4,
        SystemReset = 8,
        WatchdogReset = 16,
        LowPowerRtc = 32,
        LowPowerWakeupPin = 64
    }

    public enum SystemClock : uint {
        High = 0,
        Low = 1,
        Overclock = 2,
    }

    public enum WakeupEdge : uint {
        Falling = 0,
        Rising = 1,
    }

    // Public surface mirrors the impl. Reset() / Sleep() / Shutdown() do
    // NOT actually reset/sleep the Desktop process — they're no-ops so the
    // user's app keeps running. SystemClock/ResetSource return safe defaults.
    public static class Power {
        public static WakeupEdge WakeupEdge;

        public static void Reset() { }
        public static void Reset(bool runCoreAfter) { }
        public static void Sleep() { }
        public static void Sleep(DateTime wakeupTime) { }
        public static void Shutdown(bool wakeupPin, DateTime wakeupTime) { }

        public static void SetSystemClock(SystemClock clock, bool persist) { }
        public static SystemClock GetSystemClock() => SystemClock.High;

        public static ResetSource GetResetSource() => ResetSource.PowerOn;
    }
}
