using System.Runtime.CompilerServices;

namespace System.Diagnostics {
    public static class Debug {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [Conditional("DEBUG")]
        public static extern void WriteLine(string message);
    }
}
