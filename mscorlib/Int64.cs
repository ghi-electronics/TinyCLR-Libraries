namespace System {
    using System.Globalization;

    [Serializable]
    public struct Int64 : IFormattable {
        internal long m_value;

        public const long MaxValue = 0x7fffffffffffffffL;
        public const long MinValue = unchecked((long)0x8000000000000000L);

        public override string ToString() => Number.Format(this.m_value, true, "G", NumberFormatInfo.CurrentInfo);

        public string ToString(string format) => Number.Format(this.m_value, true, format, NumberFormatInfo.CurrentInfo);
        public string ToString(string format, IFormatProvider provider) => Number.Format(this.m_value, true, format, NumberFormatInfo.GetInstance(provider));

        public static long Parse(string s) {
            if (s == null) {
                throw new ArgumentNullException();
            }

            return Convert.ToInt64(s);
        }

        public static bool TryParse(string s, out long b) {
            b = default(long);

            try {
                b = long.Parse(s);

                return true;
            }
            catch {
                return false;
            }
        }

    }
}


