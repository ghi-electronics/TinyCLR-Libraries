namespace System {
    using System.Globalization;

    /**
     * A place holder class for signed bytes.
     * @author Jay Roxe (jroxe)
     * @version
     */
    [Serializable, CLSCompliant(false)]
    public struct SByte : IFormattable {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private sbyte m_value;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        /**
         * The maximum value that a <code>Byte</code> may represent: 127.
         */
        public const sbyte MaxValue = (sbyte)0x7F;

        /**
         * The minimum value that a <code>Byte</code> may represent: -128.
         */
        public const sbyte MinValue = unchecked((sbyte)0x80);

        public override string ToString() => Number.Format(this.m_value, true, "G", NumberFormatInfo.CurrentInfo);

        public string ToString(string format) => Number.Format(this.m_value, true, format, NumberFormatInfo.CurrentInfo);
        public string ToString(string format, IFormatProvider provider) => Number.Format(this.m_value, true, format, NumberFormatInfo.GetInstance(provider));

        [CLSCompliant(false)]
        public static sbyte Parse(string s) {
            if (s == null) {
                throw new ArgumentNullException();
            }

            return Convert.ToSByte(s);
        }

        public static bool TryParse(string s, out sbyte b) {
            b = default(sbyte);

            try {
                b = sbyte.Parse(s);

                return true;
            }
            catch {
                return false;
            }
        }

    }
}


