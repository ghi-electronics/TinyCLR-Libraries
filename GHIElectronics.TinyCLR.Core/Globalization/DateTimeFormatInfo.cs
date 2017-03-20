#define ENABLE_CROSS_APPDOMAIN
namespace System.Globalization {
    public sealed class DateTimeFormatInfo : IFormatProvider /* ICloneable */
    {
        internal string amDesignator = null;
        internal string pmDesignator = null;
        internal string dateSeparator = null;
        internal string longTimePattern = null;
        internal string shortTimePattern = null;
        internal string generalShortTimePattern = null;
        internal string generalLongTimePattern = null;
        internal string timeSeparator = null;
        internal string monthDayPattern = null;
        internal const string rfc1123Pattern = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";
        internal const string sortableDateTimePattern = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";
        internal const string universalSortableDateTimePattern = "yyyy'-'MM'-'dd HH':'mm':'ss'Z'";
        internal string fullDateTimePattern = null;
        internal string longDatePattern = null;
        internal string shortDatePattern = null;
        internal string yearMonthPattern = null;
        internal string[] abbreviatedDayNames = null;
        internal string[] dayNames = null;
        internal string[] abbreviatedMonthNames = null;
        internal string[] monthNames = null;
        CultureInfo m_cultureInfo;
        internal DateTimeFormatInfo(CultureInfo cultureInfo) => this.m_cultureInfo = cultureInfo;

        public static DateTimeFormatInfo CurrentInfo => CultureInfo.CurrentUICulture.DateTimeFormat;

        public string AMDesignator => this.m_cultureInfo.EnsureStringResource(ref this.amDesignator, System.Globalization.Resources.CultureInfo.StringResources.AMDesignator);

        public string DateSeparator => this.m_cultureInfo.EnsureStringResource(ref this.dateSeparator, System.Globalization.Resources.CultureInfo.StringResources.DateSeparator);

        public string FullDateTimePattern {
            get {
                if (this.fullDateTimePattern == null) {
                    this.fullDateTimePattern = this.LongDatePattern + " " + this.LongTimePattern;
                }

                return (this.fullDateTimePattern);
            }
        }

        public string LongDatePattern => this.m_cultureInfo.EnsureStringResource(ref this.longDatePattern, System.Globalization.Resources.CultureInfo.StringResources.LongDatePattern);

        public string LongTimePattern => this.m_cultureInfo.EnsureStringResource(ref this.longTimePattern, System.Globalization.Resources.CultureInfo.StringResources.LongTimePattern);

        public string MonthDayPattern => this.m_cultureInfo.EnsureStringResource(ref this.monthDayPattern, System.Globalization.Resources.CultureInfo.StringResources.MonthDayPattern);

        public string PMDesignator => this.m_cultureInfo.EnsureStringResource(ref this.pmDesignator, System.Globalization.Resources.CultureInfo.StringResources.PMDesignator);

        public string RFC1123Pattern => (rfc1123Pattern);

        public string ShortDatePattern => this.m_cultureInfo.EnsureStringResource(ref this.shortDatePattern, System.Globalization.Resources.CultureInfo.StringResources.ShortDatePattern);

        public string ShortTimePattern => this.m_cultureInfo.EnsureStringResource(ref this.shortTimePattern, System.Globalization.Resources.CultureInfo.StringResources.ShortTimePattern);

        public string SortableDateTimePattern => (sortableDateTimePattern);

        internal string GeneralShortTimePattern {
            get {
                if (this.generalShortTimePattern == null) {
                    this.generalShortTimePattern = this.ShortDatePattern + " " + this.ShortTimePattern;
                }

                return (this.generalShortTimePattern);
            }
        }

        /*=================================GeneralLongTimePattern=====================
        **Property: Return the pattern for 'g' general format: shortDate + Long time
        **Note: This is used by DateTimeFormat.cs to get the pattern for 'g'
        **      We put this internal property here so that we can avoid doing the
        **      concatation every time somebody asks for the general format.
        ==============================================================================*/
        internal string GeneralLongTimePattern {
            get {
                if (this.generalLongTimePattern == null) {
                    this.generalLongTimePattern = this.ShortDatePattern + " " + this.LongTimePattern;
                }

                return (this.generalLongTimePattern);
            }
        }

        public string TimeSeparator => this.m_cultureInfo.EnsureStringResource(ref this.timeSeparator, System.Globalization.Resources.CultureInfo.StringResources.TimeSeparator);

        public string UniversalSortableDateTimePattern => (universalSortableDateTimePattern);

        public string YearMonthPattern => this.m_cultureInfo.EnsureStringResource(ref this.yearMonthPattern, System.Globalization.Resources.CultureInfo.StringResources.YearMonthPattern);

        public string[] AbbreviatedDayNames => this.m_cultureInfo.EnsureStringArrayResource(ref this.abbreviatedDayNames, System.Globalization.Resources.CultureInfo.StringResources.AbbreviatedDayNames);

        public string[] DayNames => this.m_cultureInfo.EnsureStringArrayResource(ref this.dayNames, System.Globalization.Resources.CultureInfo.StringResources.DayNames);

        public string[] AbbreviatedMonthNames => this.m_cultureInfo.EnsureStringArrayResource(ref this.abbreviatedMonthNames, System.Globalization.Resources.CultureInfo.StringResources.AbbreviatedMonthNames);

        public string[] MonthNames => this.m_cultureInfo.EnsureStringArrayResource(ref this.monthNames, System.Globalization.Resources.CultureInfo.StringResources.MonthNames);

        public static DateTimeFormatInfo GetInstance(IFormatProvider provider) {
            // Fast case for a regular CultureInfo
            DateTimeFormatInfo info;
            if (provider is CultureInfo cultureProvider) {
                return cultureProvider.DateTimeFormat;
            }
            // Fast case for a DTFI;
            info = provider as DateTimeFormatInfo;
            if (info != null) {
                return info;
            }
            // Wasn't cultureInfo or DTFI, do it the slower way
            if (provider != null) {
                info = provider.GetFormat(typeof(DateTimeFormatInfo)) as DateTimeFormatInfo;
                if (info != null) {
                    return info;
                }
            }
            // Couldn't get anything, just use currentInfo as fallback
            return CurrentInfo;
        }


        public object GetFormat(Type formatType) => formatType == typeof(DateTimeFormatInfo) ? this : null;
    }
}
