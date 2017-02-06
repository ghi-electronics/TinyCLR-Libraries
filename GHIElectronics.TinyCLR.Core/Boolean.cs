namespace System {
    /**
     * A place holder class for boolean.
     * @author Jay Roxe (jroxe)
     * @version
     */
    [Serializable]
    public struct Boolean {
        public static readonly string FalseString = "False";
        public static readonly string TrueString = "True";

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private bool m_value;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

        public override string ToString() => (this.m_value) ? TrueString : FalseString;

    }
}


