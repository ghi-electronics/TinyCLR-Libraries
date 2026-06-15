namespace System {
    // Minimum-viable AggregateException for dual-mode signature parity with BCL.
    //
    // BCL's Task.Exception returns System.AggregateException (not Exception).
    // If TinyCLR's Task.Exception returns Exception instead, the IL typeref
    // baked into user code reads `[mscorlib]Task::get_Exception() : Exception`.
    // On Desktop the runtime resolves typerefs against BCL mscorlib and finds
    // no method with that exact signature (BCL's returns AggregateException),
    // raising MissingMethodException.
    //
    // Surface kept minimal: ctors that wrap a single inner exception or an
    // Exception[]. InnerExceptions / Flatten / Handle deliberately omitted —
    // they require ReadOnlyCollection<T> which TinyCLR doesn't ship. Callers
    // can still reach the wrapped exception through the inherited
    // Exception.InnerException property. If a user wants to enumerate all
    // wrapped exceptions on device, fall back to InnerException-chained
    // walking; on Desktop the BCL surface exposes the full set.
    [Serializable()]
    public class AggregateException : Exception {
        private readonly Exception[] _innerExceptions;

        public AggregateException()
            : base() {
            this._innerExceptions = new Exception[0];
        }

        public AggregateException(string message)
            : base(message) {
            this._innerExceptions = new Exception[0];
        }

        public AggregateException(string message, Exception innerException)
            : base(message, innerException) {
            this._innerExceptions = new Exception[] { innerException };
        }

        public AggregateException(Exception innerException)
            : base(innerException != null ? innerException.Message : null, innerException) {
            this._innerExceptions = new Exception[] { innerException };
        }

        public AggregateException(params Exception[] innerExceptions)
            : base(BuildMessage(innerExceptions), FirstOrNull(innerExceptions)) {
            this._innerExceptions = innerExceptions == null
                ? new Exception[0]
                : (Exception[])innerExceptions.Clone();
        }

        public AggregateException(string message, params Exception[] innerExceptions)
            : base(message, FirstOrNull(innerExceptions)) {
            this._innerExceptions = innerExceptions == null
                ? new Exception[0]
                : (Exception[])innerExceptions.Clone();
        }

        // Walk the wrapped exceptions in registration order. Returns a fresh
        // copy so callers can't mutate the internal buffer.
        public Exception[] GetInnerExceptions() => (Exception[])this._innerExceptions.Clone();

        private static Exception FirstOrNull(Exception[] arr) =>
            (arr == null || arr.Length == 0) ? null : arr[0];

        private static string BuildMessage(Exception[] arr) {
            if (arr == null || arr.Length == 0) return null;
            return arr[0] != null ? arr[0].Message : null;
        }
    }
}
