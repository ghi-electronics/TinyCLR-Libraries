namespace System {
    using System.Runtime.CompilerServices;

    /**
     * TimeSpan represents a duration of time.  A TimeSpan can be negative
     * or positive.</p>
     *
     * <p>TimeSpan is internally represented as a number of milliseconds.  While
     * this maps well into units of time such as hours and days, any
     * periods longer than that aren't representable in a nice fashion.
     * For instance, a month can be between 28 and 31 days, while a year
     * can contain 365 or 364 days.  A decade can have between 1 and 3 leapyears,
     * depending on when you map the TimeSpan into the calendar.  This is why
     * we do not provide Years() or Months().</p>
     *
     * @see System.DateTime
     */
    [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public struct TimeSpan : IFormattable
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        internal long m_ticks;

        public const long TicksPerMillisecond = 10000;
        private const double MillisecondsPerTick = 1.0 / TicksPerMillisecond;
        public const long TicksPerSecond = TicksPerMillisecond * 1000;
        private const double SecondsPerTick = 1.0 / TicksPerSecond;
        public const long TicksPerMinute = TicksPerSecond * 60;
        private const double MinutesPerTick = 1.0 / TicksPerMinute;
        public const long TicksPerHour = TicksPerMinute * 60;
        private const double HoursPerTick = 1.0 / TicksPerHour;
        public const long TicksPerDay = TicksPerHour * 24;
        private const double DaysPerTick = 1.0 / TicksPerDay;

        private const long MaxMilliSeconds = long.MaxValue / TicksPerMillisecond;
        private const long MinMilliSeconds = long.MinValue / TicksPerMillisecond;

        public static readonly TimeSpan Zero = new TimeSpan(0);

        public static readonly TimeSpan MaxValue = new TimeSpan(long.MaxValue);
        public static readonly TimeSpan MinValue = new TimeSpan(long.MinValue);

        public TimeSpan(long ticks) => this.m_ticks = ticks;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public TimeSpan(int hours, int minutes, int seconds);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public TimeSpan(int days, int hours, int minutes, int seconds);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds);

        public long Ticks => this.m_ticks;

        public int Days => (int)(this.m_ticks / TicksPerDay);

        public int Hours => (int)((this.m_ticks / TicksPerHour) % 24);

        public int Milliseconds => (int)((this.m_ticks / TicksPerMillisecond) % 1000);

        public int Minutes => (int)((this.m_ticks / TicksPerMinute) % 60);

        public int Seconds => (int)((this.m_ticks / TicksPerSecond) % 60);

        public double TotalDays => this.m_ticks * DaysPerTick;
        public double TotalHours => this.m_ticks * HoursPerTick;
        public double TotalMilliseconds {
            get {
                var temp = this.m_ticks * MillisecondsPerTick;
                if (temp > MaxMilliSeconds)
                    return MaxMilliSeconds;

                if (temp < MinMilliSeconds)
                    return MinMilliSeconds;

                return temp;
            }
        }

        public double TotalMinutes => this.m_ticks * MinutesPerTick;
        public double TotalSeconds => this.m_ticks * SecondsPerTick;
        public TimeSpan Add(TimeSpan ts) => new TimeSpan(this.m_ticks + ts.m_ticks);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static int Compare(TimeSpan t1, TimeSpan t2);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public int CompareTo(object value);

        public TimeSpan Duration() => new TimeSpan(this.m_ticks >= 0 ? this.m_ticks : -this.m_ticks);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public override bool Equals(object value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static bool Equals(TimeSpan t1, TimeSpan t2);

        public TimeSpan Negate() => new TimeSpan(-this.m_ticks);

        public TimeSpan Subtract(TimeSpan ts) => new TimeSpan(this.m_ticks - ts.m_ticks);

        public static TimeSpan FromTicks(long val) => new TimeSpan(val);
        public static TimeSpan FromMilliseconds(double milliseconds) => new TimeSpan((long)(milliseconds * TimeSpan.TicksPerMillisecond));
        public static TimeSpan FromSeconds(double seconds) => new TimeSpan((long)(seconds * TimeSpan.TicksPerSecond));
        public static TimeSpan FromMinutes(double minutes) => new TimeSpan((long)(minutes * TimeSpan.TicksPerMinute));
        public static TimeSpan FromHours(double hours) => new TimeSpan((long)(hours * TimeSpan.TicksPerHour));
        public static TimeSpan FromDays(double days) => new TimeSpan((long)(days * TimeSpan.TicksPerDay));

        public string ToString(string format, IFormatProvider formatProvider) => this.ToString();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public override string ToString();

        public static TimeSpan operator -(TimeSpan t) => new TimeSpan(-t.m_ticks);

        public static TimeSpan operator -(TimeSpan t1, TimeSpan t2) => new TimeSpan(t1.m_ticks - t2.m_ticks);

        public static TimeSpan operator +(TimeSpan t) => t;

        public static TimeSpan operator +(TimeSpan t1, TimeSpan t2) => new TimeSpan(t1.m_ticks + t2.m_ticks);

        public static bool operator ==(TimeSpan t1, TimeSpan t2) => t1.m_ticks == t2.m_ticks;

        public static bool operator !=(TimeSpan t1, TimeSpan t2) => t1.m_ticks != t2.m_ticks;

        public static bool operator <(TimeSpan t1, TimeSpan t2) => t1.m_ticks < t2.m_ticks;

        public static bool operator <=(TimeSpan t1, TimeSpan t2) => t1.m_ticks <= t2.m_ticks;

        public static bool operator >(TimeSpan t1, TimeSpan t2) => t1.m_ticks > t2.m_ticks;

        public static bool operator >=(TimeSpan t1, TimeSpan t2) => t1.m_ticks >= t2.m_ticks;

    }
}


