namespace System.Runtime.CompilerServices {
    // C# 10. Lets a method override which builder type the compiler uses for
    // its async state machine - e.g. [AsyncMethodBuilder(typeof(MyBuilder))]
    // on a method returning a custom awaitable. Pure marker; the compiler
    // reads the BuilderType property.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface
                  | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Method,
                  Inherited = false, AllowMultiple = false)]
    public sealed class AsyncMethodBuilderAttribute : Attribute {
        public AsyncMethodBuilderAttribute(Type builderType) {
            this.BuilderType = builderType;
        }
        public Type BuilderType { get; }
    }
}
