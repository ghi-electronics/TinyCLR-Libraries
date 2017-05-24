namespace System {
    [Serializable()]
    public class NotImplementedException : SystemException {
        public NotImplementedException()
            : base() {
        }

        public NotImplementedException(string message)
            : base(message) {
        }

        public NotImplementedException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}


