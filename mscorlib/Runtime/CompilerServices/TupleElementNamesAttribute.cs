namespace System.Runtime.CompilerServices {
    // The C# compiler emits this attribute on parameters / fields / locals of
    // tuple type whenever the developer named the elements, e.g.
    //   (int n, string s) p = (1, "x");
    // Without this type defined in mscorlib the compiler errors CS8137.
    // We do NOT use the element names at runtime; the attribute exists purely
    // to satisfy the predefined-types contract.
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property
                  | AttributeTargets.ReturnValue | AttributeTargets.Class | AttributeTargets.Struct
                  | AttributeTargets.Event,
                  AllowMultiple = false, Inherited = false)]
    public sealed class TupleElementNamesAttribute : Attribute {
        private readonly string[] _transformNames;
        public TupleElementNamesAttribute(string[] transformNames) {
            this._transformNames = transformNames;
        }
        public System.Collections.Generic.IList<string> TransformNames => this._transformNames;
    }
}
