namespace System.Collections.Generic {
    /// <summary>
    /// Generic set abstraction. Mirrors the .NET BCL <c>System.Collections.Generic.ISet&lt;T&gt;</c>
    /// shape so HashSet&lt;T&gt; and future set implementations interop cleanly with
    /// portable BCL-using code.
    /// </summary>
    public interface ISet<T> : ICollection<T> {
        new bool Add(T item);
        void UnionWith(IEnumerable<T> other);
        void IntersectWith(IEnumerable<T> other);
        void ExceptWith(IEnumerable<T> other);
        void SymmetricExceptWith(IEnumerable<T> other);
        bool IsSubsetOf(IEnumerable<T> other);
        bool IsSupersetOf(IEnumerable<T> other);
        bool IsProperSupersetOf(IEnumerable<T> other);
        bool IsProperSubsetOf(IEnumerable<T> other);
        bool Overlaps(IEnumerable<T> other);
        bool SetEquals(IEnumerable<T> other);
    }
}
