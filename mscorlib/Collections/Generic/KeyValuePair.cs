namespace System.Collections.Generic {
    [Serializable]
    public struct KeyValuePair<TKey, TValue> {
        public TKey Key { get; }
        public TValue Value { get; }

        public KeyValuePair(TKey key, TValue value) {
            this.Key = key;
            this.Value = value;
        }

        public override string ToString() {
            // Matches the .NET BCL format: "[key, value]"
            var keyStr = this.Key == null ? "" : this.Key.ToString();
            var valStr = this.Value == null ? "" : this.Value.ToString();
            return "[" + keyStr + ", " + valStr + "]";
        }
    }
}
