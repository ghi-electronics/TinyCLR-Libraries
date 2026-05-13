using System.Collections;
using System.Collections.Generic;

namespace System.Linq {
    /// <summary>
    /// Internal IGrouping implementation backed by a List. Allocated by
    /// <see cref="Enumerable.GroupBy{TSource,TKey}"/> and friends.
    /// </summary>
    internal sealed class Grouping<TKey, TElement> : IGrouping<TKey, TElement> {
        private readonly TKey _key;
        private readonly List<TElement> _elements;

        public Grouping(TKey key) {
            this._key = key;
            this._elements = new List<TElement>();
        }

        public TKey Key => this._key;

        internal void Add(TElement item) => this._elements.Add(item);

        public IEnumerator<TElement> GetEnumerator() => this._elements.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this._elements).GetEnumerator();
    }
}
