namespace System.Diagnostics {
    public static class Trace {
        private static TraceListenerCollection listeners;
        private static object syncRoot = new object();

        public static TraceListenerCollection Listeners {
            get {
                if (Trace.listeners == null) {
                    lock (Trace.syncRoot) {
                        if (Trace.listeners == null) {
                            Trace.listeners = new TraceListenerCollection { new DefaultTraceListener() };
                        }
                    }
                }

                return Trace.listeners;
            }
        }

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
