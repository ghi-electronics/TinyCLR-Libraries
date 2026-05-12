namespace System {
    /**
     * A place holder class for boolean.
     * @author Jay Roxe (jroxe)
     * @version
     */
    [Serializable]
    public struct Boolean : IComparable, IComparable<bool> {
        public static readonly string FalseString = "False";
        public static readonly string TrueString = "True";

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private bool m_value;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        public override string ToString() => (this.m_value) ? TrueString : FalseString;

        // false < true per .NET BCL.
        public int CompareTo(bool value) => (this.m_value == value) ? 0 : (this.m_value ? 1 : -1);
        public int CompareTo(object obj) {
            if (obj == null) return 1;
            if (!(obj is bool)) throw new ArgumentException();
            return this.CompareTo((bool)obj);
        }

        // Value-based hash so Dictionary/Hashtable work with bool keys.
        public override int GetHashCode() => this.m_value ? 1 : 0;
        public override bool Equals(object obj) => obj is bool b && b == this.m_value;
    }
}


