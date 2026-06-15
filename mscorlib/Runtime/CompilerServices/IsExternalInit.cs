namespace System.Runtime.CompilerServices {
    // The C# 9 compiler emits a `modreq(IsExternalInit)` on the set accessor of
    // init-only properties so that only object-initializer / record / `with`
    // construction can call it. The type itself has no runtime behavior - its
    // mere presence in mscorlib is what makes the compiler accept the syntax.
    // Without this type the compiler errors with CS0518.
    //
    // Unlocks: records, record structs, init-only properties, `with` expressions.
    public static class IsExternalInit { }
}
