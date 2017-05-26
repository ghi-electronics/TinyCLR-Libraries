namespace System {
    using System.Globalization;
    using System.Runtime.CompilerServices;

    // Summary:
    //     Specifies whether a System.DateTime object represents a local time, a Coordinated
    //     Universal Time (UTC), or is not specified as either local time or UTC.
    [Serializable]
    public enum DateTimeKind {
        // Summary:
        //     The time represented is not specified as either local time or Coordinated
        //     Universal Time (UTC).
        //     MF does not support Unspecified type. Constructor for DateTime always creates local time.
        //     Use SpecifyKind to set Kind property to UTC or ToUniversalTime to convert local to UTC
        //Unspecified = 0,
        //
        // Summary:
        //     The time represented is UTC.
        Utc = 1,
        //
        // Summary:
        //     The time represented is local time.
        Local = 2,
    }

    /**
     * This value type represents a date and time.  Every DateTime
     * object has a private field (Ticks) of type Int64 that stores the
     * date and time as the number of 100 nanosecond intervals since
     * 12:00 AM January 1, year 1601 A.D. in the proleptic Gregorian Calendar.
     *
     * <p>For a description of various calendar issues, look at
     * <a href="http://serendipity.nofadz.com/hermetic/cal_stud.htm">
     * Calendar Studies web site</a>, at
     * http://serendipity.nofadz.com/hermetic/cal_stud.htm.
     *
     * <p>
     * <h2>Warning about 2 digit years</h2>
     * <p>As a temporary hack until we get new DateTime &lt;-&gt; String code,
     * some systems won't be able to round trip dates less than 1930.  This
     * is because we're using OleAut's string parsing routines, which look
     * at your computer's default short date string format, which uses 2 digit
     * years on most computers.  To fix this, go to Control Panel -&gt; Regional
     * Settings -&gt; Date and change the short date style to something like
     * "M/d/yyyy" (specifying four digits for the year).
     *
     */
    [Serializable()]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public struct DateTime : IFormattable
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        // Number of 100ns ticks per time unit
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 24;

        // Number of milliseconds per time unit
        private const int MillisPerSecond = 1000;
        private const int MillisPerMinute = MillisPerSecond * 60;
        private const int MillisPerHour = MillisPerMinute * 60;
        private const int MillisPerDay = MillisPerHour * 24;

        // Number of days in a non-leap year
        private const int DaysPerYear = 365;
        // Number of days in 4 years
        private const int DaysPer4Years = DaysPerYear * 4 + 1;
        // Number of days in 100 years
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;
        // Number of days in 400 years
        private const int DaysPer400Years = DaysPer100Years * 4 + 1;

        // Number of days from 1/1/0001 to 12/31/1600
        private const int DaysTo1601 = DaysPer400Years * 4;
        // Number of days from 1/1/0001 to 12/30/1899
        private const int DaysTo1899 = DaysPer400Years * 4 + DaysPer100Years * 3 - 367;
        // Number of days from 1/1/0001 to 12/31/9999
        private const int DaysTo10000 = DaysPer400Years * 25 - 366;

        private const long MinTicks = 0;
        private const long MaxTicks = 441796895990000000;
        private const long MaxMillis = (long)DaysTo10000 * MillisPerDay;

        // This is mask to extract ticks from m_ticks
        private const ulong TickMask = 0x7FFFFFFFFFFFFFFFL;
        private const ulong UTCMask = 0x8000000000000000L;

        public static readonly DateTime MinValue = new DateTime(MinTicks, true);
        public static readonly DateTime MaxValue = new DateTime(MaxTicks, true);

        private ulong m_ticks;

        private DateTime(long ticks, bool scale) {
            if (!scale) {
                ticks -= DateTime.ticksAtOrigin;

                if (((ticks & (long)TickMask) < MinTicks) || ((ticks & (long)TickMask) > MaxTicks)) {
                    throw new ArgumentOutOfRangeException("ticks", "Ticks must be between DateTime.MinValue.Ticks and DateTime.MaxValue.Ticks.");
                }

                this.m_ticks = (ulong)ticks;
            }
            else {
                this.m_ticks = (ulong)ticks;
            }
        }

        public DateTime(long ticks) : this(ticks, false) {
        }

        public DateTime(long ticks, DateTimeKind kind)
            : this(ticks) {
            if (kind == DateTimeKind.Local) {
                this.m_ticks &= ~UTCMask;
            }
            else {
                this.m_ticks |= UTCMask;
            }
        }

        public DateTime(int year, int month, int day)
            : this(year, month, day, 0, 0, 0) {
        }

        public DateTime(int year, int month, int day, int hour, int minute, int second)
            : this(year, month, day, hour, minute, second, 0) {
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond);

        public DateTime Add(TimeSpan val) => new DateTime((long)this.m_ticks + val.Ticks, true);

        private DateTime Add(double val, int scale) => new DateTime((long)((long)this.m_ticks + (long)(val * scale * TicksPerMillisecond + (val >= 0 ? 0.5 : -0.5))), true);

        public DateTime AddDays(double val) => Add(val, MillisPerDay);

        public DateTime AddHours(double val) => Add(val, MillisPerHour);

        public DateTime AddMilliseconds(double val) => Add(val, 1);

        public DateTime AddMinutes(double val) => Add(val, MillisPerMinute);

        public DateTime AddSeconds(double val) => Add(val, MillisPerSecond);

        public DateTime AddTicks(long val) => new DateTime((long)this.m_ticks + val, true);

        public static int Compare(DateTime t1, DateTime t2) {
            // Get ticks, clear UTC mask
            var t1_ticks = t1.m_ticks & TickMask;
            var t2_ticks = t2.m_ticks & TickMask;

            // Compare ticks, ignore the Kind property.
            if (t1_ticks > t2_ticks) {
                return 1;
            }

            if (t1_ticks < t2_ticks) {
                return -1;
            }

            // Values are equal
            return 0;
        }

        public int CompareTo(object val) {
            if (val == null)
                return 1;

            return DateTime.Compare(this, (DateTime)val);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static int DaysInMonth(int year, int month);

        public override bool Equals(object val) {
            if (val is DateTime) {
                // Call compare for proper comparison of 2 DateTime objects
                // Since DateTime is optimized value and internally represented by int64
                // "this" may still have type int64.
                // Convertion to object and back is a workaround.
                object o = this;
                var thisTime = (DateTime)o;
                return Compare(thisTime, (DateTime)val) == 0;
            }

            return false;
        }

        public static bool Equals(DateTime t1, DateTime t2) => Compare(t1, t2) == 0;

        public DateTime Date {
            get {
                // Need to remove UTC mask before arithmetic operations. Then set it back.
                if ((this.m_ticks & UTCMask) != 0) {
                    return new DateTime((long)(((this.m_ticks & TickMask) - (this.m_ticks & TickMask) % TicksPerDay) | UTCMask), true);
                }
                else {
                    return new DateTime((long)(this.m_ticks - this.m_ticks % TicksPerDay), true);
                }
            }
        }

        public int Day {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get => 0;
        }

        public DayOfWeek DayOfWeek {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get => DayOfWeek.Monday;
        }

        public int DayOfYear {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get => 0;
        }

        /// Reduce size by calling a single method?
        public int Hour {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get => 0;
        }

        public DateTimeKind Kind =>
                // If mask for UTC time is set - return UTC. If no maskk - return local.
                (this.m_ticks & UTCMask) != 0 ? DateTimeKind.Utc : DateTimeKind.Local;

        public static DateTime SpecifyKind(DateTime value, DateTimeKind kind) {
            var retVal = new DateTime((long)value.m_ticks, true);

            if (kind == DateTimeKind.Utc) {
                // Set UTC mask
                retVal.m_ticks = value.m_ticks | UTCMask;
            }
            else {   // Clear UTC mask
                retVal.m_ticks = value.m_ticks & ~UTCMask;
            }

            return retVal;
        }

        public int Millisecond {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get => 0;
        }

        public int Minute {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get => 0;
        }

        public int Month {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get => 0;
        }

        public static DateTime Now {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get => new DateTime();
        }

        public static DateTime UtcNow {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get => new DateTime();
        }

        public int Second {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get => 0;
        }

        /// Our origin is at 1601/01/01:00:00:00.000
        /// While desktop CLR's origin is at 0001/01/01:00:00:00.000.
        /// There are 504911232000000000 ticks between them which we are subtracting.
        /// See DeviceCode\PAL\time_decl.h for explanation of why we are taking
        /// year 1601 as origin for our HAL, PAL, and CLR.
        static long ticksAtOrigin = 504911232000000000;
        public long Ticks => (long)(this.m_ticks & TickMask) + ticksAtOrigin;

        public TimeSpan TimeOfDay => new TimeSpan((long)((this.m_ticks & TickMask) % TicksPerDay));

        public static DateTime Today {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get => new DateTime();
        }

        public int Year {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get => 0;
        }

        public TimeSpan Subtract(DateTime val) => new TimeSpan((long)(this.m_ticks & TickMask) - (long)(val.m_ticks & TickMask));

        public DateTime Subtract(TimeSpan val) => new DateTime((long)(this.m_ticks - (ulong)val.m_ticks), true);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern DateTime ToLocalTime();

        public override string ToString() => DateTimeFormat.Format(this, null, DateTimeFormatInfo.CurrentInfo);

        public string ToString(string format) => DateTimeFormat.Format(this, format, DateTimeFormatInfo.CurrentInfo);
        public string ToString(string format, IFormatProvider formatProvider) => DateTimeFormat.Format(this, format, DateTimeFormatInfo.GetInstance(formatProvider));

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern DateTime ToUniversalTime();

        public static DateTime operator +(DateTime d, TimeSpan t) => new DateTime((long)(d.m_ticks + (ulong)t.m_ticks), true);

        public static DateTime operator -(DateTime d, TimeSpan t) => new DateTime((long)(d.m_ticks - (ulong)t.m_ticks), true);

        public static TimeSpan operator -(DateTime d1, DateTime d2) => d1.Subtract(d2);

        public static bool operator ==(DateTime d1, DateTime d2) => Compare(d1, d2) == 0;

        public static bool operator !=(DateTime t1, DateTime t2) => Compare(t1, t2) != 0;

        public static bool operator <(DateTime t1, DateTime t2) => Compare(t1, t2) < 0;

        public static bool operator <=(DateTime t1, DateTime t2) => Compare(t1, t2) <= 0;

        public static bool operator >(DateTime t1, DateTime t2) => Compare(t1, t2) > 0;

        public static bool operator >=(DateTime t1, DateTime t2) => Compare(t1, t2) >= 0;
    }
}


