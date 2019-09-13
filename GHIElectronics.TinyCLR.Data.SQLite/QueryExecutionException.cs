using System;

namespace GHIElectronics.TinyCLR.Data.SQLite {
    [Serializable]
    public class QueryExecutionException : System.Exception {
        internal QueryExecutionException() : base() {
        }

        internal QueryExecutionException(string message) : base(message) {
        }

        internal QueryExecutionException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
