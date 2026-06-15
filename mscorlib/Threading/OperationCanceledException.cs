namespace System {
    /// <summary>
    /// Thrown when a cooperatively-cancelled operation observes its cancellation
    /// token's request. Matches the .NET BCL shape including the
    /// <see cref="CancellationToken"/> property so handlers can correlate the
    /// throw site with the source token.
    /// </summary>
    [Serializable]
    public class OperationCanceledException : SystemException {
        private readonly System.Threading.CancellationToken _token;

        public OperationCanceledException() : base() { }
        public OperationCanceledException(string message) : base(message) { }
        public OperationCanceledException(string message, Exception innerException) : base(message, innerException) { }
        public OperationCanceledException(System.Threading.CancellationToken token) {
            this._token = token;
        }
        public OperationCanceledException(string message, System.Threading.CancellationToken token) : base(message) {
            this._token = token;
        }
        public OperationCanceledException(string message, Exception innerException, System.Threading.CancellationToken token) : base(message, innerException) {
            this._token = token;
        }

        public System.Threading.CancellationToken CancellationToken => this._token;
    }
}
