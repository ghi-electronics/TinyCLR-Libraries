namespace System {
    [Serializable]
    public class FormatException : SystemException {
        public FormatException() { }
        public FormatException(string message) : base(message) { }
        public FormatException(string message, Exception innerException) : base(message, innerException) { }
    }
}
