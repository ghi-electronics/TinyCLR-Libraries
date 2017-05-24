namespace System {
    [Serializable()]
    public class ObjectDisposedException : SystemException {
        public ObjectDisposedException()
            : base() {
        }

        public ObjectDisposedException(string message)
            : base(message) {
        }

        public ObjectDisposedException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}


