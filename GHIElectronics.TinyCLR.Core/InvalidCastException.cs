namespace System {
    [Serializable()]
    public class InvalidCastException : SystemException {
        public InvalidCastException()
            : base() {
        }

        public InvalidCastException(string message)
            : base(message) {
        }

        public InvalidCastException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}


