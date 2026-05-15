using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    /// <summary>
    /// Reads and writes the system wall clock (separate from the
    /// <see cref="GHIElectronics.TinyCLR.Devices.Rtc.RtcController"/> chip
    /// register). Time zone is carried as a minutes-from-UTC offset.
    /// </summary>
    public static class SystemTime {
        /// <summary>Sets the system clock from raw 100 ns ticks (UTC).</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetTime(long utcTime, int timeZoneOffset);

        /// <summary>Reads the system clock as raw 100 ns ticks (UTC).</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void GetTime(out long utcTime, out int timeZoneOffset);

        /// <summary>Sets the system clock from a UTC <see cref="DateTime"/>.</summary>
        public static void SetTime(DateTime utcTime) => SystemTime.SetTime(utcTime, 0);
        /// <summary>Sets the system clock from a UTC <see cref="DateTime"/> with a time-zone offset.</summary>
        public static void SetTime(DateTime utcTime, int timeZoneOffset) => SystemTime.SetTime(utcTime.Ticks, timeZoneOffset);

        /// <summary>Reads the system clock as a UTC <see cref="DateTime"/>.</summary>
        public static DateTime GetTime() {
            SystemTime.GetTime(out DateTime utcTime, out _);

            return utcTime;
        }

        /// <summary>Reads the system clock as a UTC <see cref="DateTime"/> together with the local time-zone offset.</summary>
        public static void GetTime(out DateTime utcTime, out int timeZoneOffset) {
            SystemTime.GetTime(out long ticks, out timeZoneOffset);

            utcTime = new DateTime(ticks);
        }
    }
}
