namespace System.Runtime.CompilerServices {
    // Marks a `static void M()` method as a module initializer - run once when
    // the containing assembly's <Module> type initializer fires (i.e. at first
    // touch of any type in the assembly). C# 9. Pure marker.
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ModuleInitializerAttribute : Attribute {
        public ModuleInitializerAttribute() { }
    }
}
