namespace System.Runtime.CompilerServices {
    // C# 12 (preview). Source-generator-emitted attribute that redirects a
    // call site at the given (file, line, character) to the annotated method.
    // Used heavily by ASP.NET Core / EF Core minimal-API generators. Pure marker.
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class InterceptsLocationAttribute : Attribute {
        public InterceptsLocationAttribute(string filePath, int line, int character) {
            this.FilePath = filePath;
            this.Line = line;
            this.Character = character;
        }
        public string FilePath { get; }
        public int Line { get; }
        public int Character { get; }
    }
}
