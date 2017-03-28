using System;
using System.Runtime.InteropServices;

namespace System {
    [ComVisible(true)]
    [Serializable]
    public class OverflowException : Exception {
        public OverflowException() : base("Overflow") { }
        public OverflowException(string message) : base(message) { }
        public OverflowException(string message, Exception innerException) : base(message, innerException) { }
    }
}
