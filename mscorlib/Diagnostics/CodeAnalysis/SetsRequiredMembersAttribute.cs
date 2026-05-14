namespace System.Diagnostics.CodeAnalysis {
    // C# 11 companion to [Required] members. Marks a constructor as setting
    // all required members itself, so callers don't have to use the
    // object-initializer syntax. Pure marker.
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public sealed class SetsRequiredMembersAttribute : Attribute {
        public SetsRequiredMembersAttribute() { }
    }
}
