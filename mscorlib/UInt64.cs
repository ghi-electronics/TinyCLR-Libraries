namespace System {
    using System.Globalization;

    /**
     * Wrapper for unsigned 64 bit integers.
     */
    [Serializable, CLSCompliant(false)]
    public struct UInt64 : IFormattable, IComparable, IComparable<ulong> {
        // Native storage written by the runtime, not from managed code —
        // CS0649 ("never assigned") is a false positive here.
#pragma warning disable CS0649
        private ulong m_value;
#pragma warning restore CS0649

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

        public int CompareTo(ulong value) => this.m_value < value ? -1 : (this.m_value > value ? 1 : 0);
        public int CompareTo(object obj) {
            if (obj == null) return 1;
            if (!(obj is ulong)) throw new ArgumentException();
            return this.CompareTo((ulong)obj);
        }

        public override int GetHashCode() => unchecked((int)this.m_value) ^ (int)(this.m_value >> 32);
        public override bool Equals(object obj) => obj is ulong u && u == this.m_value;
    }
}


