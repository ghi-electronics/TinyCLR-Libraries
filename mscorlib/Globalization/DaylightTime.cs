namespace System.Globalization {

    using System;
    /**
     * This class represents a starting/ending time for a period of daylight saving time.
     */
    [Serializable]
    public class DaylightTime {
        internal DateTime m_start;
        internal DateTime m_end;
        internal TimeSpan m_delta;

        private DaylightTime() {
        }

        public DaylightTime(DateTime start, DateTime end, TimeSpan delta) {
            this.m_start = start;
            this.m_end = end;
            this.m_delta = delta;
        }

        /**
         * The start date of a daylight saving period.
         */
        public DateTime Start => this.m_start;

        /**
         * The end date of a daylight saving period.
         */
        public DateTime End => this.m_end;

        /**
         * Delta to stardard offset in ticks.
         */
        public TimeSpan Delta => this.m_delta;
    }
}


