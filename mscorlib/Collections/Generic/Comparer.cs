namespace System.Collections.Generic {
    /// <summary>
    /// Default comparer for <typeparamref name="T"/>. Falls back to
    /// <see cref="IComparable{T}"/> if implemented, then non-generic
    /// <see cref="IComparable"/>, then a stable but uninformative
    /// <c>0</c>/<c>-1</c>/<c>1</c> based on reference equality. Same boxing
    /// trade-off as <see cref="EqualityComparer{T}"/> for value types.
    /// </summary>
    public abstract class Comparer<T> : IComparer<T>, IComparer {
        private static readonly Comparer<T> _default = new DefaultComparer();

        public static Comparer<T> Default => _default;

        public abstract int Compare(T x, T y);

        int IComparer.Compare(object x, object y) {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            if (x is T tx && y is T ty) return this.Compare(tx, ty);
            // Mixed types coming in through the non-generic interface - fall
            // back to non-generic IComparable on either side.
            if (x is IComparable cx) return cx.CompareTo(y);
            if (y is IComparable cy) return -cy.CompareTo(x);
            throw new ArgumentException();
        }

        /// <summary>Creates a comparer from a delegate.</summary>
        public static Comparer<T> Create(Comparison<T> comparison) {
            if (comparison == null) throw new ArgumentNullException();
            return new ComparisonComparer(comparison);
        }

        private sealed class DefaultComparer : Comparer<T> {
            // TinyCLR mis-dispatches instance-method calls on boxed value
            // types via ANY interface (generic IComparable<T> AND non-generic
            // IComparable both fail). `this` inside e.g. Int32.CompareTo
            // reads from wrong memory. The only reliable v-table dispatch on
            // value types is for Object's own methods (GetHashCode, Equals).
            //
            // Workaround: pattern-match (which uses isinst + unbox.any, both
            // sound in TinyCLR) for known value types and do the comparison
            // inline. Reference types still go through non-generic IComparable
            // (works because the receiver is already an object reference).
            //
            // The type list covers everything in mscorlib that implements
            // IComparable<T>; extend if user types need value-type ordering
            // beyond reference types.
            public override int Compare(T x, T y) {
                if (Object.ReferenceEquals(x, y)) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                // Value-type fast paths - direct comparison after unbox via
                // pattern match.
                if (x is int xi && y is int yi) return xi < yi ? -1 : (xi > yi ? 1 : 0);
                if (x is long xl && y is long yl) return xl < yl ? -1 : (xl > yl ? 1 : 0);
                if (x is double xd && y is double yd) {
                    if (Double.IsNaN(xd)) return Double.IsNaN(yd) ? 0 : -1;
                    if (Double.IsNaN(yd)) return 1;
                    return xd < yd ? -1 : (xd > yd ? 1 : 0);
                }
                if (x is float xf && y is float yf) {
                    if (Double.IsNaN(xf)) return Double.IsNaN(yf) ? 0 : -1;
                    if (Double.IsNaN(yf)) return 1;
                    return xf < yf ? -1 : (xf > yf ? 1 : 0);
                }
                if (x is uint xu && y is uint yu) return xu < yu ? -1 : (xu > yu ? 1 : 0);
                if (x is ulong xul && y is ulong yul) return xul < yul ? -1 : (xul > yul ? 1 : 0);
                if (x is short xs && y is short ys) return xs < ys ? -1 : (xs > ys ? 1 : 0);
                if (x is ushort xus && y is ushort yus) return xus < yus ? -1 : (xus > yus ? 1 : 0);
                if (x is byte xb && y is byte yb) return xb < yb ? -1 : (xb > yb ? 1 : 0);
                if (x is sbyte xsb && y is sbyte ysb) return xsb < ysb ? -1 : (xsb > ysb ? 1 : 0);
                if (x is char xc && y is char yc) return xc < yc ? -1 : (xc > yc ? 1 : 0);
                if (x is bool xbo && y is bool ybo) return xbo == ybo ? 0 : (xbo ? 1 : -1);
                if (x is DateTime xdt && y is DateTime ydt) return DateTime.Compare(xdt, ydt);
                if (x is TimeSpan xts && y is TimeSpan yts)
                    return xts.Ticks < yts.Ticks ? -1 : (xts.Ticks > yts.Ticks ? 1 : 0);

                // Reference types: interface dispatch via non-generic
                // IComparable works because the receiver is already a real
                // object reference (not a boxed value type).
                object ox = x;
                if (ox is IComparable nc) {
                    object oy = y;
                    return nc.CompareTo(oy);
                }
                throw new ArgumentException();
            }
        }

        private sealed class ComparisonComparer : Comparer<T> {
            private readonly Comparison<T> _comparison;
            public ComparisonComparer(Comparison<T> comparison) { this._comparison = comparison; }
            public override int Compare(T x, T y) => this._comparison(x, y);
        }
    }
}
