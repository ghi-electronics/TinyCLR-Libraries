using System;

namespace GHIElectronics.TinyCLR.Data.SQLite {

    [Serializable]
    public class QueryPrepareException : System.Exception {

        internal QueryPrepareException()
            : base() {
        }

        internal QueryPrepareException(string message)
            : base(message) {
        }

        internal QueryPrepareException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}
