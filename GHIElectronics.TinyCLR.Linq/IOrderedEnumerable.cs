using System.Collections.Generic;

namespace System.Linq {
    /// <summary>
    /// Represents a sorted sequence. Used as the chain link between
    /// <c>OrderBy</c>/<c>OrderByDescending</c> and <c>ThenBy</c>/<c>ThenByDescending</c>.
    /// </summary>
    public interface IOrderedEnumerable<TElement> : IEnumerable<TElement> {
        /// <summary>Performs a subsequent ordering on the elements by a key, used to implement ThenBy and ThenByDescending.</summary>
        IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(
            Func<TElement, TKey> keySelector,
            IComparer<TKey> comparer,
            bool descending);
    }
}
