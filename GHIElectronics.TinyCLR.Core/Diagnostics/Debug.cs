namespace System.Diagnostics {
    public static class Debug {
        [Conditional("DEBUG")]
        public static void WriteLine(string message) => Debugger.Log(0, string.Empty, message + "\r\n");

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
