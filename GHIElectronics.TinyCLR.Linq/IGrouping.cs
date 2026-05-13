using System.Collections.Generic;

namespace System.Linq {
    /// <summary>
    /// Represents a collection of objects that have a common key. Returned by <c>GroupBy</c>.
    /// </summary>
    public interface IGrouping<TKey, TElement> : IEnumerable<TElement> {
        TKey Key { get; }
    }
}
