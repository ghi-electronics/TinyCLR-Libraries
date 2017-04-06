using System.Runtime.CompilerServices;

namespace System.Diagnostics {
    public static class Debugger {
        public static extern bool IsAttached { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Break();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Log(int level, string category, string message);
    }
}


