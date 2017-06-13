#define TRACE

namespace System.Diagnostics {
    public static class Trace {
        public static TraceListenerCollection Listeners { get; } = new TraceListenerCollection { new DefaultTraceListener() };

        [Conditional("TRACE")]
        public static void WriteLine(string message) {
            foreach (var listener in Trace.Listeners)
                ((TraceListener)listener).WriteLine(message);
        }

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
