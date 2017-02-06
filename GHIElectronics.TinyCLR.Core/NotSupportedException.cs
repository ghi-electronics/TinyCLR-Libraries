namespace System {
    [Serializable()]
    public class NotSupportedException : SystemException {
        public NotSupportedException()
            : base() {
        }

        public NotSupportedException(string message)
            : base(message) {
        }

        public NotSupportedException(string message, Exception innerException)
            : base(message, innerException) {
        }

    }
}


