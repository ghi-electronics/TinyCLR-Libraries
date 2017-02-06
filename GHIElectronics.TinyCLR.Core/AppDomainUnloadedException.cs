namespace System {
    [Serializable()]
    public class AppDomainUnloadedException : SystemException {
        public AppDomainUnloadedException()
            : base() {
        }

        public AppDomainUnloadedException(string message)
            : base(message) {
        }

        public AppDomainUnloadedException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}


