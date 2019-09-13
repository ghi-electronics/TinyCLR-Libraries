using System;

namespace GHIElectronics.TinyCLR.Data.SQLite {
    [Serializable]
    public class QueryFinalizationException : System.Exception {
        internal QueryFinalizationException() : base() {
        }

        internal QueryFinalizationException(string message) : base(message) {
        }

        internal QueryFinalizationException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
