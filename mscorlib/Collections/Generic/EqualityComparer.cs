namespace System.Collections.Generic {
    /// <summary>
    /// Default equality comparer for <typeparamref name="T"/>. Falls back to
    /// <c>object.Equals</c> / <c>object.GetHashCode</c>, which means value-type
    /// arguments are boxed at the call site - acceptable cost for this BCL
    /// subset (no per-primitive specializations).
    /// </summary>
    public abstract class EqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer {
        private static readonly EqualityComparer<T> _default = new DefaultEqualityComparer();

        public static EqualityComparer<T> Default => _default;

        public abstract bool Equals(T x, T y);
        public abstract int GetHashCode(T obj);

        bool IEqualityComparer.Equals(object x, object y) {
            if (x == y) return true;
            if (x == null || y == null) return false;
            if (x is T tx && y is T ty) return this.Equals(tx, ty);
            // Cross-type compare: defer to object.Equals so callers get sensible
            // behavior when handing in mismatched runtime types via the
            // non-generic interface.
            return x.Equals(y);
        }

        int IEqualityComparer.GetHashCode(object obj) {
            if (obj == null) return 0;
            if (obj is T t) return this.GetHashCode(t);
            return obj.GetHashCode();
        }

        private sealed class DefaultEqualityComparer : EqualityComparer<T> {
            // Important: do NOT call obj.Equals / obj.GetHashCode directly on
            // a generic T. Roslyn emits `ldarga.s + constrained.callvirt` for
            // that pattern, and TinyCLR's runtime mis-handles `constrained.`
            // for reference types - the native handler receives an address
            // rather than the reference, and InternalCalls like
            // String::get_Length see a non-string receiver and NRE.
            //
            // Casting to (object) forces Roslyn to emit `box !T + callvirt`
            // instead. For reference types, `box` is a no-op pass-through and
            // the resulting callvirt uses the normal v-table path that
            // String.Equals / String.GetHashCode rely on. For value types,
            // it's a real box (one allocation per call - acceptable cost for
            // this BCL subset).
            public override bool Equals(T x, T y) {
                if (x == null) return y == null;
                if (y == null) return false;
                object ox = x;
                object oy = y;
                return ox.Equals(oy);
            }

            public override int GetHashCode(T obj) {
                if (obj == null) return 0;
                object o = obj;
                return o.GetHashCode();
            }
        }
    }
}
