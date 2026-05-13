namespace System.Runtime.CompilerServices {
    // C# 11. Emitted by the compiler on members declared with `required`.
    // The compiler also requires CompilerFeatureRequiredAttribute on the
    // containing type so older compilers reject the assembly. Pure marker.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field
                  | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class RequiredMemberAttribute : Attribute {
        public RequiredMemberAttribute() { }
    }
}
