//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace System.Globalization.Resources
{
    
    internal partial class CultureInfo
    {
        private static System.Resources.ResourceManager manager;
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if ((CultureInfo.manager == null))
                {
                    CultureInfo.manager = new System.Resources.ResourceManager("System.Globalization.Resources.CultureInfo", typeof(CultureInfo).Assembly);
                }
                return CultureInfo.manager;
            }
        }
        internal static string GetString(CultureInfo.StringResources id)
        {
            return ((string)(ResourceManager.GetObject(((short)(id)))));
        }
        [System.SerializableAttribute()]
        internal enum StringResources : short
        {
            LongDatePattern = -30643,
            PositiveSign = -24864,
            DayNames = -21087,
            NumberDecimalSeparator = -20751,
            ShortDatePattern = -19957,
            NumberGroupSeparator = -15642,
            AMDesignator = -15501,
            ShortTimePattern = -14376,
            NegativeSign = -11738,
            PMDesignator = -6760,
            YearMonthPattern = 3936,
            MonthNames = 7086,
            AbbreviatedDayNames = 9268,
            NumberGroupSizes = 9393,
            TimeSeparator = 9689,
            MonthDayPattern = 11943,
            DateSeparator = 21845,
            AbbreviatedMonthNames = 25574,
            LongTimePattern = 30813,
        }
    }
}
