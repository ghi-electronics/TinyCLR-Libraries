namespace System.Collections.Generic {
    public interface IDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>> {
        TValue this[TKey key] { get; set; }
        ICollection<TKey> Keys { get; }
        ICollection<TValue> Values { get; }
        void Add(TKey key, TValue value);
        bool ContainsKey(TKey key);
        bool Remove(TKey key);
        bool TryGetValue(TKey key, out TValue value);
    }
}
