namespace System.Threading {

    using System;

    [Serializable()]
    public sealed class ThreadAbortException : SystemException {
        private ThreadAbortException() { }
    }
}


