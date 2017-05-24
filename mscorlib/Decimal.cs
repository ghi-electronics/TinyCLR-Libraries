using System.ComponentModel;

namespace System {
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct Decimal : IFormattable {
        [CLSCompliant(false)]
        public Decimal(uint value) { }

        [CLSCompliant(false)]
        public Decimal(ulong value) { }

        public Decimal(int value) { }
        public Decimal(long value) { }
        public Decimal(float value) { }
        public Decimal(double value) { }
        public Decimal(int[] bits) { }
        public Decimal(int lo, int mid, int hi, bool isNegative, byte scale) { }

        public string ToString(string format) => string.Empty;
        public string ToString(string format, IFormatProvider formatProvider) => string.Empty;
    }
}
