namespace System.Threading {
    /// <summary>
    /// Signals cancellation cooperatively. Hand out <see cref="Token"/> to long-
    /// running APIs; call <see cref="Cancel"/> from another thread (or a Timer)
    /// to request termination. Cancellation is observed by polling
    /// <see cref="CancellationToken.IsCancellationRequested"/> or calling
    /// <see cref="CancellationToken.ThrowIfCancellationRequested"/> — there is
    /// no callback fan-out yet, and there is no way to interrupt a managed
    /// <see cref="Thread.Sleep"/> in progress, so cancellation latency is
    /// bounded by the polling cadence of the consumer.
    ///
    /// Task.Delay(int, CancellationToken) polls every 50 ms, so a cancel
    /// observed there fires within ~50 ms of the Cancel() call.
    /// </summary>
    public sealed class CancellationTokenSource : IDisposable {
        // Volatile-equivalent: managed bool reads/writes are atomic on all
        // TinyCLR targets, and we don't need fences because consumers ALWAYS
        // re-read this field on every poll (no caching). One-shot latch: once
        // true, stays true forever.
        private bool _cancelled;
        private bool _disposed;
        private Timer _timer;

        public CancellationTokenSource() { }

        public CancellationTokenSource(int millisecondsDelay) {
            if (millisecondsDelay < -1) throw new ArgumentOutOfRangeException();
            if (millisecondsDelay != Timeout.Infinite) {
                this.CancelAfter(millisecondsDelay);
            }
        }

        public CancellationTokenSource(TimeSpan delay) : this((int)delay.TotalMilliseconds) { }

        public CancellationToken Token {
            get {
                if (this._disposed) throw new ObjectDisposedException(null);
                return new CancellationToken(this);
            }
        }

        public bool IsCancellationRequested => this._cancelled;

        public void Cancel() {
            if (this._disposed) throw new ObjectDisposedException(null);
            this._cancelled = true;
            // Note: real .NET fires registered callbacks here. We don't ship
            // CancellationToken.Register yet — pollers will pick this up on
            // their next check. Adding callbacks is additive when we need them.
        }

        public void Cancel(bool throwOnFirstException) => this.Cancel();

        public void CancelAfter(int millisecondsDelay) {
            if (millisecondsDelay < -1) throw new ArgumentOutOfRangeException();
            if (this._disposed) throw new ObjectDisposedException(null);
            if (this._cancelled) return;

            // One-shot Timer that fires Cancel(). Replaces any prior timer.
            // Callback is an instance method (not a capturing lambda) to avoid
            // the C# compiler emitting a `<>c__DisplayClass` — TinyCLR's
            // MetadataProcessor doesn't sanitize the `<>` characters when it
            // emits FIELD___ constants in mscorlib.h.
            if (this._timer != null) this._timer.Dispose();
            if (millisecondsDelay == Timeout.Infinite) {
                this._timer = null;
                return;
            }
            this._timer = new Timer(new TimerCallback(this.OnCancelAfterTimer), null, millisecondsDelay, Timeout.Infinite);
        }

        private void OnCancelAfterTimer(object state) {
            try { if (!this._disposed) this.Cancel(); } catch { }
        }

        public void CancelAfter(TimeSpan delay) => this.CancelAfter((int)delay.TotalMilliseconds);

        public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2) {
            var linked = new CancellationTokenSource();
            if (token1.IsCancellationRequested || token2.IsCancellationRequested) linked.Cancel();
            // Without callback support we can only sample at creation time.
            // Once Register() lands this should subscribe to both sources.
            return linked;
        }

        public void Dispose() {
            if (this._disposed) return;
            this._disposed = true;
            if (this._timer != null) this._timer.Dispose();
            this._timer = null;
        }

        // Internal hook used by CancellationToken to read the latch without
        // requiring a property lookup through reflection-style indirection.
        internal bool InternalIsCancelled => this._cancelled;
    }
}
