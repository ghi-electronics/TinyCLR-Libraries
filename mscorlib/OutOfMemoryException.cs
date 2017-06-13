namespace System {
    [Serializable()]
    public class OutOfMemoryException : SystemException {
        public OutOfMemoryException()
            : base() {
        }

        public OutOfMemoryException(string message)
            : base(message) {
        }

        public OutOfMemoryException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}


