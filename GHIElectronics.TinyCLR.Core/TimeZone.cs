namespace System {
    using System.Globalization;
    using System.Runtime.CompilerServices;

    [Serializable]
    public abstract class TimeZone {
#pragma warning disable 0649
        internal int m_id;
#pragma warning restore

        protected TimeZone() { }

        public static TimeZone CurrentTimeZone => new CurrentSystemTimeZone(GetTimeZoneOffset());

        public abstract string StandardName {
            get;
        }

        public abstract string DaylightName {
            get;
        }

        public abstract TimeSpan GetUtcOffset(DateTime time);

        public virtual DateTime ToUniversalTime(DateTime time) {
            if (time.Kind == DateTimeKind.Utc)
                return time;

            return new DateTime(time.Ticks - GetTimeZoneOffset(), DateTimeKind.Utc);
        }

        public virtual DateTime ToLocalTime(DateTime time) {
            if (time.Kind == DateTimeKind.Local)
                return time;

            return new DateTime(time.Ticks + GetTimeZoneOffset(), DateTimeKind.Local);
        }

        public abstract DaylightTime GetDaylightChanges(int year);

        public virtual bool IsDaylightSavingTime(DateTime time) => false;

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static long GetTimeZoneOffset();
    }

    [Serializable]
    internal class CurrentSystemTimeZone : TimeZone {
        protected long m_ticksOffset = 0;

        internal CurrentSystemTimeZone() {
        }

        internal CurrentSystemTimeZone(long ticksOffset) => this.m_ticksOffset = ticksOffset;

        public override string StandardName => throw new NotImplementedException();

        public override string DaylightName => throw new NotImplementedException();

        public override DaylightTime GetDaylightChanges(int year) => throw new NotImplementedException();

        public override TimeSpan GetUtcOffset(DateTime time) {
            if (time.Kind == DateTimeKind.Utc)
                return TimeSpan.Zero;

            return new TimeSpan(this.m_ticksOffset);
        }
    }
}


