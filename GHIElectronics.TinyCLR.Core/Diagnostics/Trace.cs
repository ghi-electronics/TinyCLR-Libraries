namespace System.Diagnostics {
    public static class Trace {
        [Conditional("TRACE")]
        public static void WriteLine(string message) => Debugger.Log(0, string.Empty, message + "\r\n");

        [Conditional("TRACE")]
        public static void WriteLineIf(bool condition, string message) {
            if (condition)
                Trace.WriteLine(message);
        }

        [Conditional("TRACE")]
        public static void Assert(bool condition) => Trace.Assert(condition, string.Empty, string.Empty);

        [Conditional("TRACE")]
        public static void Assert(bool condition, string message) => Trace.Assert(condition, message, string.Empty);

        [Conditional("TRACE")]
        public static void Assert(bool condition, string message, string detailedMessage) {
            if (!condition) {
                Trace.WriteLineIf(message != string.Empty, message);
                Trace.WriteLineIf(detailedMessage != string.Empty, detailedMessage);
                Debugger.Break();
            }
        }
    }
}
