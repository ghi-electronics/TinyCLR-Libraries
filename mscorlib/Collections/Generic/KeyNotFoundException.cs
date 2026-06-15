namespace System.Collections.Generic {
    [Serializable]
    public class KeyNotFoundException : SystemException {
        public KeyNotFoundException() : base("The given key was not present in the dictionary.") { }
        public KeyNotFoundException(string message) : base(message) { }
        public KeyNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
