using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public static class SystemTime {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetTime(long utcTime, int timeZoneOffset);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void GetTime(out long utcTime, out int timeZoneOffset);

        public static void SetTime(DateTime utcTime) => SystemTime.SetTime(utcTime, 0);
        public static void SetTime(DateTime utcTime, int timeZoneOffset) => SystemTime.SetTime(utcTime.Ticks, timeZoneOffset);

        public static DateTime GetTime() {
            SystemTime.GetTime(out DateTime utcTime, out _);

            return utcTime;
        }

        public static void GetTime(out DateTime utcTime, out int timeZoneOffset) {
            SystemTime.GetTime(out long ticks, out timeZoneOffset);

            utcTime = new DateTime(ticks);
        }
    }
}
