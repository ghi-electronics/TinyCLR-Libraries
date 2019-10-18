using System;

namespace GHIElectronics.TinyCLR.Data.SQLite {
    [Serializable]
    public class OpenException : Exception {
        internal OpenException() : base() {

        }

        internal OpenException(string message) : base(message) {

        }

        internal OpenException(string message, Exception innerException) : base(message, innerException) {

        }
    }

    [Serializable]
    public class QueryExecutionException : Exception {
        internal QueryExecutionException() : base() {

        }

        internal QueryExecutionException(string message) : base(message) {

        }

        internal QueryExecutionException(string message, Exception innerException) : base(message, innerException) {

        }
    }
    [Serializable]
    public class QueryFinalizationException : Exception {
        internal QueryFinalizationException() : base() {

        }

        internal QueryFinalizationException(string message) : base(message) {

        }

        internal QueryFinalizationException(string message, Exception innerException) : base(message, innerException) {

        }
    }
    [Serializable]
    public class QueryPrepareException : Exception {
        internal QueryPrepareException() : base() {

        }

        internal QueryPrepareException(string message) : base(message) {

        }

        internal QueryPrepareException(string message, Exception innerException) : base(message, innerException) {

        }
    }
}
