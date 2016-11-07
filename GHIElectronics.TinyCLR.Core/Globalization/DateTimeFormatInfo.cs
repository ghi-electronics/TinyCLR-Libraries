////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////#define ENABLE_CROSS_APPDOMAIN
#define ENABLE_CROSS_APPDOMAIN
namespace System.Globalization
{
    using System;
    using System.Collections;
    public sealed class DateTimeFormatInfo /*: ICloneable, IFormatProvider*/
    {
        internal String amDesignator = null;
        internal String pmDesignator = null;
        internal String dateSeparator = null;
        internal String longTimePattern = null;
        internal String shortTimePattern = null;
        internal String generalShortTimePattern = null;
        internal String generalLongTimePattern = null;
        internal String timeSeparator = null;
        internal String monthDayPattern = null;
        internal const String rfc1123Pattern = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";
        internal const String sortableDateTimePattern = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";
        internal const String universalSortableDateTimePattern = "yyyy'-'MM'-'dd HH':'mm':'ss'Z'";
        internal String fullDateTimePattern = null;
        internal String longDatePattern = null;
        internal String shortDatePattern = null;
        internal String yearMonthPattern = null;
        internal String[] abbreviatedDayNames = null;
        internal String[] dayNames = null;
        internal String[] abbreviatedMonthNames = null;
        internal String[] monthNames = null;
        CultureInfo m_cultureInfo;
        internal DateTimeFormatInfo(CultureInfo cultureInfo)
        {
            m_cultureInfo = cultureInfo;
        }

        public static DateTimeFormatInfo CurrentInfo
        {
            get
            {
                return CultureInfo.CurrentUICulture.DateTimeFormat;
            }
        }

        public String AMDesignator
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref this.amDesignator, "AM");
            }
        }

        public String DateSeparator
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref this.dateSeparator, "/");
            }
        }

        public String FullDateTimePattern
        {
            get
            {
                if (fullDateTimePattern == null)
                {
                    fullDateTimePattern = LongDatePattern + " " + LongTimePattern;
                }

                return (fullDateTimePattern);
            }
        }

        public String LongDatePattern
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref longDatePattern, "dddd, dd MMMM yyyy");
            }
        }

        public String LongTimePattern
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref this.longTimePattern, "HH:mm:ss");
            }
        }

        public String MonthDayPattern
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref this.monthDayPattern, "MMMM dd");
            }
        }

        public String PMDesignator
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref this.pmDesignator, "PM");
            }
        }

        public String RFC1123Pattern
        {
            get
            {
                return (rfc1123Pattern);
            }
        }

        public String ShortDatePattern
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref this.shortDatePattern, "MM/dd/yyyy");
            }
        }

        public String ShortTimePattern
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref this.shortTimePattern, "HH:mm");
            }
        }

        public String SortableDateTimePattern
        {
            get
            {
                return (sortableDateTimePattern);
            }
        }

        internal String GeneralShortTimePattern
        {
            get
            {
                if (generalShortTimePattern == null)
                {
                    generalShortTimePattern = ShortDatePattern + " " + ShortTimePattern;
                }

                return (generalShortTimePattern);
            }
        }

        /*=================================GeneralLongTimePattern=====================
        **Property: Return the pattern for 'g' general format: shortDate + Long time
        **Note: This is used by DateTimeFormat.cs to get the pattern for 'g'
        **      We put this internal property here so that we can avoid doing the
        **      concatation every time somebody asks for the general format.
        ==============================================================================*/
        internal String GeneralLongTimePattern
        {
            get
            {
                if (generalLongTimePattern == null)
                {
                    generalLongTimePattern = ShortDatePattern + " " + LongTimePattern;
                }

                return (generalLongTimePattern);
            }
        }

        public String TimeSeparator
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref this.timeSeparator, ":");
            }
        }

        public String UniversalSortableDateTimePattern
        {
            get
            {
                return (universalSortableDateTimePattern);
            }
        }

        public String YearMonthPattern
        {
            get
            {
                return m_cultureInfo.EnsureStringResource(ref this.yearMonthPattern, "yyyy MMMM");
            }
        }

        public String[] AbbreviatedDayNames
        {
            get
            {
                return m_cultureInfo.EnsureStringArrayResource(ref abbreviatedDayNames, "Sun|Mon|Tue|Wed|Thu|Fri|Sat");
            }
        }

        public String[] DayNames
        {
            get
            {
                return m_cultureInfo.EnsureStringArrayResource(ref dayNames, "Sunday|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday");
            }
        }

        public String[] AbbreviatedMonthNames
        {
            get
            {
                return m_cultureInfo.EnsureStringArrayResource(ref abbreviatedMonthNames, "Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec");
            }
        }

        public String[] MonthNames
        {
            get
            {
                return m_cultureInfo.EnsureStringArrayResource(ref monthNames, "January|February|March|April|May|June|July|August|September|October|November|December");
            }
        }
    }
}


