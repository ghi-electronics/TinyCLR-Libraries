namespace System {
    using System.Globalization;

    [Serializable]
    public struct Int16 : IFormattable, IComparable, IComparable<short> {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        internal short m_value;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        public const short MaxValue = (short)0x7FFF;
        public const short MinValue = unchecked((short)0x8000);

        public override string ToString() => Number.Format(this.m_value, true, "G", NumberFormatInfo.CurrentInfo);

        public string ToString(string format) => Number.Format(this.m_value, true, format, NumberFormatInfo.CurrentInfo);
        public string ToString(string format, IFormatProvider provider) => Number.Format(this.m_value, true, format, NumberFormatInfo.GetInstance(provider));

        public static short Parse(string s) {
            if (s == null) {
                throw new ArgumentNullException();
            }

            return Convert.ToInt16(s);
        }

        public static bool TryParse(string s, out short b) {
            b = default(short);

            try {
                b = short.Parse(s);

                return true;
            }
            catch {
                return false;
            }
        }

        public int CompareTo(short value) => this.m_value < value ? -1 : (this.m_value > value ? 1 : 0);
        public int CompareTo(object obj) {
            if (obj == null) return 1;
            if (!(obj is short)) throw new ArgumentException();
            return this.CompareTo((short)obj);
        }

        public override int GetHashCode() => ((int)(ushort)this.m_value) | (this.m_value << 16);
        public override bool Equals(object obj) => obj is short s && s == this.m_value;
    }
}


