namespace System.Collections.Generic {
    /// <summary>
    /// Generic set abstraction. Mirrors the .NET BCL <c>System.Collections.Generic.ISet&lt;T&gt;</c>
    /// shape so HashSet&lt;T&gt; and future set implementations interop cleanly with
    /// portable BCL-using code.
    /// </summary>
    public interface ISet<T> : ICollection<T> {
        /// <summary>Adds an element to the set and returns whether it was newly added.</summary>
        new bool Add(T item);
        /// <summary>Modifies the set to contain all elements that are present in itself, the specified collection, or both.</summary>
        void UnionWith(IEnumerable<T> other);
        /// <summary>Modifies the set to contain only elements that are also present in the specified collection.</summary>
        void IntersectWith(IEnumerable<T> other);
        /// <summary>Removes all elements in the specified collection from the set.</summary>
        void ExceptWith(IEnumerable<T> other);
        /// <summary>Modifies the set to contain only elements that are present either in itself or in the specified collection, but not both.</summary>
        void SymmetricExceptWith(IEnumerable<T> other);
        /// <summary>Determines whether the set is a subset of the specified collection.</summary>
        bool IsSubsetOf(IEnumerable<T> other);
        /// <summary>Determines whether the set is a superset of the specified collection.</summary>
        bool IsSupersetOf(IEnumerable<T> other);
        /// <summary>Determines whether the set is a proper superset of the specified collection.</summary>
        bool IsProperSupersetOf(IEnumerable<T> other);
        /// <summary>Determines whether the set is a proper subset of the specified collection.</summary>
        bool IsProperSubsetOf(IEnumerable<T> other);
        /// <summary>Determines whether the set and the specified collection share any common elements.</summary>
        bool Overlaps(IEnumerable<T> other);
        /// <summary>Determines whether the set and the specified collection contain the same elements.</summary>
        bool SetEquals(IEnumerable<T> other);
    }
}
