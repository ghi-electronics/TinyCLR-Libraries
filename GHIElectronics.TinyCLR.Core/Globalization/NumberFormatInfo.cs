namespace System.Globalization {
    using System;
    //
    // Property             Default Description
    // PositiveSign           '+'   Character used to indicate positive values.
    // NegativeSign           '-'   Character used to indicate negative values.
    // NumberDecimalSeparator '.'   The character used as the decimal separator.
    // NumberGroupSeparator   ','   The character used to separate groups of
    //                              digits to the left of the decimal point.
    // NumberDecimalDigits    2     The default number of decimal places.
    // NumberGroupSizes       3     The number of digits in each group to the
    //                              left of the decimal point.
    // NaNSymbol             "NaN"  The string used to represent NaN values.
    // PositiveInfinitySymbol"Infinity" The string used to represent positive
    //                              infinities.
    // NegativeInfinitySymbol"-Infinity" The string used to represent negative
    //                              infinities.
    //
    //
    //
    // Property                  Default  Description
    // CurrencyDecimalSeparator  '.'      The character used as the decimal
    //                                    separator.
    // CurrencyGroupSeparator    ','      The character used to separate groups
    //                                    of digits to the left of the decimal
    //                                    point.
    // CurrencyDecimalDigits     2        The default number of decimal places.
    // CurrencyGroupSizes        3        The number of digits in each group to
    //                                    the left of the decimal point.
    // CurrencyPositivePattern   0        The format of positive values.
    // CurrencyNegativePattern   0        The format of negative values.
    // CurrencySymbol            "$"      String used as local monetary symbol.
    //

    [Serializable]
    sealed public class NumberFormatInfo : IFormatProvider /* ICloneable */
    {
        internal int[] numberGroupSizes = null;//new int[] { 3 };
        internal string positiveSign = null;//"+";
        internal string negativeSign = null;//"-";
        internal string numberDecimalSeparator = null;//".";
        internal string numberGroupSeparator = null;//",";
        private CultureInfo m_cultureInfo;
        internal NumberFormatInfo(CultureInfo cultureInfo) => this.m_cultureInfo = cultureInfo;

        public int[] NumberGroupSizes {
            get {
                if (this.numberGroupSizes == null) {
                    string sizesStr = null;

                    this.m_cultureInfo.EnsureStringResource(ref sizesStr, System.Globalization.Resources.CultureInfo.StringResources.NumberGroupSizes);

                    var sizesLen = sizesStr.Length;
                    this.numberGroupSizes = new int[sizesLen];

                    int size;
                    for (var i = 0; i < sizesLen; i++) {
                        size = sizesStr[i] - '0';
                        if (size > 9 || size < 0) {
                            this.numberGroupSizes = null;
                            throw new InvalidOperationException();
                        }

                        this.numberGroupSizes[i] = size;
                    }
                }

                return ((int[])this.numberGroupSizes.Clone());
            }
        }

        public static NumberFormatInfo GetInstance(IFormatProvider formatProvider) {
            // Fast case for a regular CultureInfo
            NumberFormatInfo info;
            if (formatProvider is CultureInfo cultureProvider) {
                info = cultureProvider.numInfo;
                if (info != null) {
                    return info;
                }
                else {
                    return cultureProvider.NumberFormat;
                }
            }
            // Fast case for an NFI;
            info = formatProvider as NumberFormatInfo;
            if (info != null) {
                return info;
            }
            if (formatProvider != null) {
                info = formatProvider.GetFormat(typeof(NumberFormatInfo)) as NumberFormatInfo;
                if (info != null) {
                    return info;
                }
            }
            return CurrentInfo;
        }

        public object GetFormat(Type formatType) => formatType == typeof(NumberFormatInfo) ? this : null;

        public static NumberFormatInfo CurrentInfo => CultureInfo.CurrentUICulture.NumberFormat;

        public string NegativeSign => this.m_cultureInfo.EnsureStringResource(ref this.negativeSign, System.Globalization.Resources.CultureInfo.StringResources.NegativeSign);

        public string NumberDecimalSeparator => this.m_cultureInfo.EnsureStringResource(ref this.numberDecimalSeparator, System.Globalization.Resources.CultureInfo.StringResources.NumberDecimalSeparator);

        public string NumberGroupSeparator => this.m_cultureInfo.EnsureStringResource(ref this.numberGroupSeparator, System.Globalization.Resources.CultureInfo.StringResources.NumberGroupSeparator);

        public string PositiveSign => this.m_cultureInfo.EnsureStringResource(ref this.positiveSign, System.Globalization.Resources.CultureInfo.StringResources.PositiveSign);
    } // NumberFormatInfo
}


