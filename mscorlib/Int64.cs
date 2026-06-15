namespace System {
    using System.Globalization;

    [Serializable]
    public struct Int64 : IFormattable, IComparable, IComparable<long> {
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

        public int CompareTo(long value) => this.m_value < value ? -1 : (this.m_value > value ? 1 : 0);
        public int CompareTo(object obj) {
            if (obj == null) return 1;
            if (!(obj is long)) throw new ArgumentException();
            return this.CompareTo((long)obj);
        }

        public override int GetHashCode() => unchecked((int)this.m_value) ^ (int)(this.m_value >> 32);
        public override bool Equals(object obj) => obj is long l && l == this.m_value;
    }
}


