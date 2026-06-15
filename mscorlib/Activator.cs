using System.Reflection;

namespace System {
    // Bare-minimum Activator. The C# compiler lowers `new T()` (under
    // `where T : new()`) to `Activator.CreateInstance<T>()`. Without this
    // method in mscorlib, any code that uses the new() constraint fails to
    // compile with CS0656 "Missing compiler required member
    // 'System.Activator.CreateInstance'".
    //
    // Implementation is pure managed: typeof(T) → no-arg ConstructorInfo →
    // Invoke. ConstructorInfo.Invoke is already an InternalCall in TinyCLR,
    // so this path avoids any new native interop.
    //
    // Limitations vs BCL:
    //   * Only the parameterless generic overload is implemented. The
    //     non-generic Activator.CreateInstance(Type) and the
    //     Activator.CreateInstance(Type, object[]) family are not provided
    //     — TinyCLR user code rarely needs them and the C# compiler does
    //     not depend on them.
    //   * No Activator.CreateInstance(string assemblyName, string typeName)
    //     for late-bound construction; same reasoning.
    public static class Activator {
        public static T CreateInstance<T>() {
            var t = typeof(T);
            var ctor = t.GetConstructor(new Type[0]);
            // Defensive only — `where T : new()` is enforced by the C# compiler,
            // so by the time we run, T is guaranteed to have a parameterless
            // ctor. (BCL throws MissingMethodException here; TinyCLR mscorlib
            // doesn't declare that type, so we use InvalidOperationException.)
            if (ctor == null)
                throw new InvalidOperationException("No parameterless constructor for type " + t.FullName);
            return (T)ctor.Invoke(null);
        }
    }
}
