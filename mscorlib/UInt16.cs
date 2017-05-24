namespace System {
    using System.Globalization;

    /**
     * Wrapper for unsigned 16 bit integers.
     */
    [Serializable, CLSCompliant(false)]
    public struct UInt16 : IFormattable {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private ushort m_value;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        public const ushort MaxValue = (ushort)0xFFFF;
        public const ushort MinValue = 0;

        public override string ToString() => Number.Format(this.m_value, true, "G", NumberFormatInfo.CurrentInfo);

        public string ToString(string format) => Number.Format(this.m_value, true, format, NumberFormatInfo.CurrentInfo);
        public string ToString(string format, IFormatProvider provider) => Number.Format(this.m_value, true, format, NumberFormatInfo.GetInstance(provider));

        [CLSCompliant(false)]
        public static ushort Parse(string s) {
            if (s == null) {
                throw new ArgumentNullException();
            }

            return Convert.ToUInt16(s);
        }

        public static bool TryParse(string s, out ushort b) {
            b = default(ushort);

            try {
                b = ushort.Parse(s);

                return true;
            }
            catch {
                return false;
            }
        }

    }
}


