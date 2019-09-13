using System;

namespace GHIElectronics.TinyCLR.Data.SQLite {
    [Serializable]
    public class OpenException : System.Exception {
        internal OpenException() : base() {
        }

        internal OpenException(string message) : base(message) {
        }

        internal OpenException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
