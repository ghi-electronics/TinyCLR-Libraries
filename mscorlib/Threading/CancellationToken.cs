namespace System.Threading {
    // Skeleton. The struct's storage is reserved for a future CancellationTokenSource
    // reference; current state is "never cancellable" so most APIs that accept a
    // CancellationToken parameter type-check correctly and behave as if no token
    // were passed. ThrowIfCancellationRequested is a no-op because there is no
    // source to fire cancellation yet.
    public struct CancellationToken {
        private readonly object _source;

        internal CancellationToken(object source) { this._source = source; }

        public static CancellationToken None => default(CancellationToken);

        public bool IsCancellationRequested => false;

        public bool CanBeCanceled => false;

        public void ThrowIfCancellationRequested() {
            // No-op until CancellationTokenSource is added. When a source exists,
            // this should throw OperationCanceledException if requested.
        }

        public override bool Equals(object obj) => obj is CancellationToken other && this._source == other._source;
        public bool Equals(CancellationToken other) => this._source == other._source;
        public override int GetHashCode() => this._source == null ? 0 : this._source.GetHashCode();

        public static bool operator ==(CancellationToken left, CancellationToken right) => left.Equals(right);
        public static bool operator !=(CancellationToken left, CancellationToken right) => !left.Equals(right);
    }
}
