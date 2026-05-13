namespace System.Runtime.CompilerServices {
    // C# 11. Emitted by the compiler to signal that older compilers must
    // reject the assembly because some feature (e.g. RequiredMembers,
    // RefStructs) was used. Pure marker; well-known feature names are
    // exposed as const strings so the compiler can reference them.
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public sealed class CompilerFeatureRequiredAttribute : Attribute {
        public CompilerFeatureRequiredAttribute(string featureName) {
            this.FeatureName = featureName;
        }
        public string FeatureName { get; }
        public bool IsOptional { get; set; }

        public const string RefStructs = nameof(RefStructs);
        public const string RequiredMembers = nameof(RequiredMembers);
    }
}
