using System.Collections;
using System.Collections.Generic;

namespace System.Linq {
    /// <summary>
    /// Internal implementation that materializes the source on first
    /// enumeration, sorts it using a chain of key-comparers (one per
    /// OrderBy / ThenBy call), and yields the result. The chain is built up
    /// by <see cref="CreateOrderedEnumerable{TKey}"/>.
    /// </summary>
    internal sealed class OrderedEnumerable<TElement> : IOrderedEnumerable<TElement> {
        private readonly IEnumerable<TElement> _source;
        // SortKey chain: head is the primary sort, .Next is the secondary
        // tiebreaker, etc. Compare() walks the chain until it finds a
        // non-zero result.
        private readonly SortKey _firstKey;

        internal OrderedEnumerable(IEnumerable<TElement> source, SortKey firstKey) {
            this._source = source;
            this._firstKey = firstKey;
        }

        public IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(
                Func<TElement, TKey> keySelector,
                IComparer<TKey> comparer,
                bool descending) {
            if (keySelector == null) throw new ArgumentNullException();
            // Append to chain so the new key acts as a tiebreaker AFTER
            // existing keys.
            var newKey = new SortKey<TKey>(keySelector, comparer ?? Comparer<TKey>.Default, descending);
            this._firstKey.AppendTail(newKey);
            return this;
        }

        public IEnumerator<TElement> GetEnumerator() {
            // Materialize into an array and sort. Stable insertion sort -
            // O(n^2) but simple, no Array.Sort dependency, and good enough
            // for embedded data sizes.
            var arr = ToArray(this._source);
            this.SortInPlace(arr);
            for (var i = 0; i < arr.Length; i++) yield return arr[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        private static TElement[] ToArray(IEnumerable<TElement> source) {
            // Count + copy. Avoid leaning on List<T> to keep this file
            // self-contained.
            var temp = new List<TElement>();
            foreach (var e in source) temp.Add(e);
            return temp.ToArray();
        }

        private void SortInPlace(TElement[] arr) {
            // Stable insertion sort. Stability matters because ThenBy on a
            // matching primary key must preserve relative input order when
            // the secondary key also ties.
            for (var i = 1; i < arr.Length; i++) {
                var current = arr[i];
                var j = i - 1;
                while (j >= 0 && this._firstKey.Compare(arr[j], current) > 0) {
                    arr[j + 1] = arr[j];
                    j--;
                }
                arr[j + 1] = current;
            }
        }

        // --- Sort-key chain ---

        internal abstract class SortKey {
            internal SortKey Next;

            public void AppendTail(SortKey tail) {
                var node = this;
                while (node.Next != null) node = node.Next;
                node.Next = tail;
            }

            public int Compare(TElement x, TElement y) {
                var c = this.CompareKeys(x, y);
                if (c != 0) return c;
                return this.Next == null ? 0 : this.Next.Compare(x, y);
            }

            protected abstract int CompareKeys(TElement x, TElement y);
        }

        internal sealed class SortKey<TKey> : SortKey {
            private readonly Func<TElement, TKey> _keySelector;
            private readonly IComparer<TKey> _comparer;
            private readonly bool _descending;

            public SortKey(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending) {
                this._keySelector = keySelector;
                this._comparer = comparer;
                this._descending = descending;
            }

            protected override int CompareKeys(TElement x, TElement y) {
                var kx = this._keySelector(x);
                var ky = this._keySelector(y);
                var c = this._comparer.Compare(kx, ky);
                return this._descending ? -c : c;
            }
        }
    }
}
