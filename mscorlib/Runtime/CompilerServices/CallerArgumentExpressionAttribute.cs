namespace System.Runtime.CompilerServices {
    // C# 10. On an optional string parameter, the compiler substitutes the
    // source-text expression of the argument named in ParameterName. Lets you
    // write helpers like ArgumentNullException.ThrowIfNull(x) and have the
    // exception's paramName auto-filled with "x".
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class CallerArgumentExpressionAttribute : Attribute {
        public CallerArgumentExpressionAttribute(string parameterName) {
            this.ParameterName = parameterName;
        }
        public string ParameterName { get; }
    }
}
