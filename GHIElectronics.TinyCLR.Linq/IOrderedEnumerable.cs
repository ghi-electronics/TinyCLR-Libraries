using System.Collections.Generic;

namespace System.Linq {
    /// <summary>
    /// Represents a sorted sequence. Used as the chain link between
    /// <c>OrderBy</c>/<c>OrderByDescending</c> and <c>ThenBy</c>/<c>ThenByDescending</c>.
    /// </summary>
    public interface IOrderedEnumerable<TElement> : IEnumerable<TElement> {
        IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(
            Func<TElement, TKey> keySelector,
            IComparer<TKey> comparer,
            bool descending);
    }
}
