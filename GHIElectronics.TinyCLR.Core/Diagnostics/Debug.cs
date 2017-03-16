using System.Runtime.CompilerServices;

namespace System.Diagnostics {
    public static class Debug {
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void WriteLineNative(string message);

        [Conditional("DEBUG")]
        public static void WriteLine(string message) => Debug.WriteLineNative(message);

        [Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, string message) {
            if (condition)
                Debug.WriteLine(message);
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition) => Debug.Assert(condition, string.Empty, string.Empty);

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message) => Debug.Assert(condition, message, string.Empty);

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message, string detailedMessage) {
            if (!condition) {
                Debug.WriteLineIf(message != string.Empty, message);
                Debug.WriteLineIf(detailedMessage != string.Empty, detailedMessage);
                Debugger.Break();
            }
        }
    }
}
