namespace System.Threading {
    /// <summary>
    /// Read-only handle to a <see cref="CancellationTokenSource"/>'s cancellation
    /// state. Pollable (<see cref="IsCancellationRequested"/>) and assertable
    /// (<see cref="ThrowIfCancellationRequested"/>). The token is a struct, so
    /// passing it around is allocation-free; cancellation observation is one
    /// reference-comparison + one bool read.
    ///
    /// There is no callback (Register) support yet — consumers must poll. See
    /// CancellationTokenSource for the latency / interrupt semantics.
    /// </summary>
    public struct CancellationToken {
        private readonly CancellationTokenSource _source;

        internal CancellationToken(CancellationTokenSource source) { this._source = source; }

        // Legacy ctor — accepts a generic object so existing call sites that
        // were typed against the pre-source skeleton still compile. Anything
        // that isn't a CancellationTokenSource just yields an uncancellable
        // token (matches the old behavior).
        internal CancellationToken(object source) { this._source = source as CancellationTokenSource; }

        public static CancellationToken None => default(CancellationToken);

        public bool IsCancellationRequested => this._source != null && this._source.InternalIsCancelled;

        public bool CanBeCanceled => this._source != null;

        public void ThrowIfCancellationRequested() {
            if (this.IsCancellationRequested) throw new OperationCanceledException(this);
        }

        public override bool Equals(object obj) => obj is CancellationToken other && this._source == other._source;
        public bool Equals(CancellationToken other) => this._source == other._source;
        public override int GetHashCode() => this._source == null ? 0 : this._source.GetHashCode();

        public static bool operator ==(CancellationToken left, CancellationToken right) => left.Equals(right);
        public static bool operator !=(CancellationToken left, CancellationToken right) => !left.Equals(right);
    }
}
