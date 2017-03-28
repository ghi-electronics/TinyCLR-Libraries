
namespace System.Diagnostics {
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true), Serializable]
    public sealed class ConditionalAttribute : Attribute {
        public ConditionalAttribute(string conditionString) => this.m_conditionString = conditionString;

        public string ConditionString => this.m_conditionString;

        private string m_conditionString;
    }
}


