namespace System {
    [Serializable()]
    public class TimeoutException : SystemException {
        public TimeoutException()
            : base() {
        }

        public TimeoutException(string message)
            : base(message) {
        }

        public TimeoutException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}
