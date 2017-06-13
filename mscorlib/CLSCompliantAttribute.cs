namespace System {
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false), Serializable]
    public sealed class CLSCompliantAttribute : Attribute {
        private bool m_compliant;

        public CLSCompliantAttribute(bool isCompliant) => this.m_compliant = isCompliant;

        public bool IsCompliant => this.m_compliant;
    }
}


