namespace System {
    using System.Globalization;

    /**
     * A place holder class for signed bytes.
     * @author Jay Roxe (jroxe)
     * @version
     */
    [Serializable]
    public struct Byte : IFormattable, IComparable, IComparable<byte> {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private byte m_value;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        /**
         * The maximum value that a <code>Byte</code> may represent: 127.
         */
        public const byte MaxValue = (byte)0xFF;

        /**
         * The minimum value that a <code>Byte</code> may represent: -128.
         */
        public const byte MinValue = 0;

        public override string ToString() => Number.Format(this.m_value, true, "G", NumberFormatInfo.CurrentInfo);

        public string ToString(string format) => Number.Format(this.m_value, true, format, NumberFormatInfo.CurrentInfo);
        public string ToString(string format, IFormatProvider provider) => Number.Format(this.m_value, true, format, NumberFormatInfo.GetInstance(provider));

        [CLSCompliant(false)]
        public static byte Parse(string s) {
            if (s == null) {
                throw new ArgumentNullException();
            }

            return Convert.ToByte(s);
        }

        public static bool TryParse(string s, out byte b) {
            b = default(byte);

            try {
                b = byte.Parse(s);

                return true;
            }
            catch {
                return false;
            }
        }

        public int CompareTo(byte value) => this.m_value < value ? -1 : (this.m_value > value ? 1 : 0);
        public int CompareTo(object obj) {
            if (obj == null) return 1;
            if (!(obj is byte)) throw new ArgumentException();
            return this.CompareTo((byte)obj);
        }

        public override int GetHashCode() => this.m_value;
        public override bool Equals(object obj) => obj is byte b && b == this.m_value;
    }
}


