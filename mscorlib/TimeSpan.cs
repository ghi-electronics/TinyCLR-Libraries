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
    public struct TimeSpan : IFormattable, IComparable, IComparable<TimeSpan>
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        internal long m_ticks;

        // Strongly-typed companion for IComparable<TimeSpan>; delegates to the
        // existing tick-based comparison since TimeSpan is a thin wrapper
        // around a long.
        public int CompareTo(TimeSpan value) =>
            this.m_ticks < value.m_ticks ? -1 : (this.m_ticks > value.m_ticks ? 1 : 0);

        public override int GetHashCode() =>
            unchecked((int)this.m_ticks) ^ (int)(this.m_ticks >> 32);

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

        // Forward-direction double constants. Used by the FromX(double) factory
        // methods below so the multiplication is unambiguously `double * double`
        // in IL — `ldc.r8 <literal>; mul`. Previously the FromX methods used
        // `(double)TicksPerX` (cast of a const long), expecting the C# compiler
        // to fold it to a double literal at compile time. In Debug builds it
        // does; in Release builds Roslyn sometimes emits
        // `ldc.i8 N; conv.r8; mul` instead, which re-introduces the broken
        // `conv.r8 + mul` interpreter pattern. Defining the double-typed
        // constants explicitly removes the ambiguity.
        private const double TicksPerMillisecondAsDouble = 10000.0;
        private const double TicksPerSecondAsDouble      = 10000000.0;
        private const double TicksPerMinuteAsDouble      = 600000000.0;
        private const double TicksPerHourAsDouble        = 36000000000.0;
        private const double TicksPerDayAsDouble         = 864000000000.0;

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

        // Total* properties go through DivToDouble to avoid `long * double` in
        // the IL, which the TinyCLR interpreter mis-executes on Release-built
        // mscorlib (Total* would return the clamp bound for any nonzero
        // TimeSpan). (double)whole is exact for any TimeSpan up to ~285,000
        // years (53-bit mantissa).
        public double TotalDays => DivToDouble(this.m_ticks, TicksPerDay);
        public double TotalHours => DivToDouble(this.m_ticks, TicksPerHour);

        // Must be a single ternary expression, not multi-`if`-return. Roslyn's
        // Release-mode optimizer emits IL for the if/return form that the
        // TinyCLR interpreter mis-executes inside mscorlib (returns the clamp
        // bound for legitimate in-range inputs). The ternary form produces
        // single-expression IL that survives the optimizer.
        public double TotalMilliseconds {
            get {
                var r = DivToDouble(this.m_ticks, TicksPerMillisecond);
                return r > MaxMilliSeconds ? (double)MaxMilliSeconds
                     : r < MinMilliSeconds ? (double)MinMilliSeconds
                     : r;
            }
        }

        public double TotalMinutes => DivToDouble(this.m_ticks, TicksPerMinute);
        public double TotalSeconds => DivToDouble(this.m_ticks, TicksPerSecond);

        // Computes `ticks / divisor` as a double without going through the
        // broken long*double multiplication path. `divisor` must be > 0.
        private static double DivToDouble(long ticks, long divisor) {
            var whole = ticks / divisor;
            var remainder = ticks - whole * divisor;
            return (double)whole + (double)remainder / (double)divisor;
        }
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

        // Use the explicit-double constants (defined near the top of this
        // class) rather than `(double)TicksPerX`. The cast-of-const-long form
        // worked in Debug builds but Release builds re-introduced the broken
        // `conv.r8 + mul` IL pattern. Using a const that is already a double
        // means the IL is unambiguously `ldc.r8 <literal>; mul` (pure
        // double * double) regardless of optimization level.
        public static TimeSpan FromMilliseconds(double milliseconds) => new TimeSpan((long)(milliseconds * TicksPerMillisecondAsDouble));
        public static TimeSpan FromSeconds(double seconds)           => new TimeSpan((long)(seconds      * TicksPerSecondAsDouble));
        public static TimeSpan FromMinutes(double minutes)           => new TimeSpan((long)(minutes      * TicksPerMinuteAsDouble));
        public static TimeSpan FromHours(double hours)               => new TimeSpan((long)(hours        * TicksPerHourAsDouble));
        public static TimeSpan FromDays(double days)                 => new TimeSpan((long)(days         * TicksPerDayAsDouble));

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


