namespace System {
    using System.Runtime.CompilerServices;

    [Serializable()]
    public class Exception {
        private string _message;
        private Exception m_innerException;
#pragma warning disable CS0169 // The field is never used
        private object m_stackTrace;
#pragma warning restore CS0169 // The field is never used
        protected int m_HResult;

        public Exception() {
        }

        public Exception(string message) => this._message = message;

        public Exception(string message, Exception innerException) {
            this._message = message;
            this.m_innerException = innerException;
        }

        public virtual string Message {
            get {
                if (this._message == null) {
                    return "Exception was thrown: " + this.GetType().FullName;
                }
                else {
                    return this._message;
                }
            }
        }

        public Exception InnerException => this.m_innerException;
        public extern virtual string StackTrace {
            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            get;
        }

        public override string ToString() {
            var message = this.Message;
            var s = base.ToString();

            if (message != null && message.Length > 0) {
                s += ": " + message;
            }

            return s;
        }

    }

}


