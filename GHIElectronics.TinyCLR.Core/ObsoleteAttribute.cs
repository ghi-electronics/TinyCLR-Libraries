namespace System {
    /**
     * This attribute is attached to members that are not to be used any longer.
     * Message is some human readable explanation of what to use
     * Error indicates if the compiler should treat usage of such a method as an
     *   error. (this would be used if the actual implementation of the obsolete
     *   method's implementation had changed).
     *
     * Issue: do we need to be able to localize this message string?
     *
     */
    [Serializable(), AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum |
        AttributeTargets.Interface | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Delegate
        , Inherited = false)]
    public sealed class ObsoleteAttribute : Attribute {
        private string _message;
        private bool _error;

        public ObsoleteAttribute() {
            this._message = null;
            this._error = false;
        }

        public ObsoleteAttribute(string message) {
            this._message = message;
            this._error = false;
        }

        public ObsoleteAttribute(string message, bool error) {
            this._message = message;
            this._error = error;
        }

        public string Message => this._message;
        public bool IsError => this._error;
    }
}


