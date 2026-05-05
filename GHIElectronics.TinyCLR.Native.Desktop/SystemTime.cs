using System;

namespace GHIElectronics.TinyCLR.Native {
    public static class SystemTime {
        public static void SetTime(long utcTime, int timeZoneOffset) { }

        public static void GetTime(out long utcTime, out int timeZoneOffset) {
            utcTime = 0;
            timeZoneOffset = 0;
        }

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
