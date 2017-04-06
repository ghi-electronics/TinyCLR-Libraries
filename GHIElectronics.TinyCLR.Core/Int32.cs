namespace System {
    using System.Globalization;

    [Serializable]
    public struct Int32 : IFormattable {
        internal int m_value;

        public const int MaxValue = 0x7fffffff;
        public const int MinValue = unchecked((int)0x80000000);

        public override string ToString() => Number.Format(this.m_value, true, "G", NumberFormatInfo.CurrentInfo);

        public string ToString(string format) => Number.Format(this.m_value, true, format, NumberFormatInfo.CurrentInfo);
        public string ToString(string format, IFormatProvider provider) => Number.Format(this.m_value, true, format, NumberFormatInfo.GetInstance(provider));

        public static int Parse(string s) {
            if (s == null) {
                throw new ArgumentNullException();
            }

            return Convert.ToInt32(s);
        }

        public static bool TryParse(string s, out int b) {
            b = default(int);

            try {
                b = int.Parse(s);

                return true;
            }
            catch {
                return false;
            }
        }

    }
}


