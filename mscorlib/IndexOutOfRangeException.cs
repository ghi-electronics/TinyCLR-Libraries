namespace System {
    [Serializable()]
    public class IndexOutOfRangeException : SystemException {
        public IndexOutOfRangeException()
            : base() {
        }

        public IndexOutOfRangeException(string message)
            : base(message) {
        }

        public IndexOutOfRangeException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}


