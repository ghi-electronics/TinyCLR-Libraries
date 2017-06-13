namespace System {
    using System.Globalization;

    /**
     * * Wrapper for unsigned 32 bit integers.
     */
    [Serializable, CLSCompliant(false)]
    public struct UInt32 : IFormattable {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private uint m_value;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        public const uint MaxValue = (uint)0xffffffff;
        public const uint MinValue = 0U;

        public override string ToString() => Number.Format(this.m_value, true, "G", NumberFormatInfo.CurrentInfo);

        public string ToString(string format) => Number.Format(this.m_value, true, format, NumberFormatInfo.CurrentInfo);
        public string ToString(string format, IFormatProvider provider) => Number.Format(this.m_value, true, format, NumberFormatInfo.GetInstance(provider));

        [CLSCompliant(false)]
        public static uint Parse(string s) {
            if (s == null) {
                throw new ArgumentNullException();
            }

            return Convert.ToUInt32(s);
        }

        public static bool TryParse(string s, out uint b) {
            b = default(uint);

            try {
                b = uint.Parse(s);

                return true;
            }
            catch {
                return false;
            }
        }

    }
}


