using System.ComponentModel;

namespace System {
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct Decimal : IFormattable, IComparable, IComparable<decimal> {
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

        // Decimal is a placeholder type in TinyCLR (all members no-op). These
        // stubs satisfy IComparable<T> shape so generic code targeting decimal
        // compiles; they always return 0.
        public int CompareTo(decimal value) => 0;
        public int CompareTo(object value) => value == null ? 1 : 0;
    }
}
