namespace System {
    using System.Globalization;

    /**
     * Wrapper for unsigned 64 bit integers.
     */
    [Serializable, CLSCompliant(false)]
    public struct UInt64 : IFormattable {
        private ulong m_value;

        public const ulong MaxValue = (ulong)0xffffffffffffffffL;
        public const ulong MinValue = 0x0;

        public override string ToString() => Number.Format(this.m_value, true, "G", NumberFormatInfo.CurrentInfo);

        public string ToString(string format) => Number.Format(this.m_value, true, format, NumberFormatInfo.CurrentInfo);
        public string ToString(string format, IFormatProvider provider) => Number.Format(this.m_value, true, format, NumberFormatInfo.GetInstance(provider));

        [CLSCompliant(false)]
        public static ulong Parse(string s) {
            if (s == null) {
                throw new ArgumentNullException();
            }

            return Convert.ToUInt64(s);
        }

        public static bool TryParse(string s, out ulong b) {
            b = default(ulong);

            try {
                b = ulong.Parse(s);

                return true;
            }
            catch {
                return false;
            }
        }

    }
}


