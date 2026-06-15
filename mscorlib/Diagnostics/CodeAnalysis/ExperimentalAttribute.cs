namespace System.Diagnostics.CodeAnalysis {
    // C# 12. Marks an API as experimental; references emit a diagnostic with
    // the configured DiagnosticId so users must opt in with a SuppressWarning
    // or #pragma. Pure marker.
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class
                  | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor
                  | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field
                  | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate,
                  Inherited = false)]
    public sealed class ExperimentalAttribute : Attribute {
        public ExperimentalAttribute(string diagnosticId) {
            this.DiagnosticId = diagnosticId;
        }
        public string DiagnosticId { get; }
        public string UrlFormat { get; set; }
    }
}
