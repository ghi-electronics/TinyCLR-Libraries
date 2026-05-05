using System;

namespace GHIElectronics.TinyCLR.Data.SQLite {
    // Desktop sister: TinyCLR's Data.SQLite uses [MethodImpl(InternalCall)]
    // to bind to native sqlite3, which only exists on the device. BCL has no
    // built-in SQLite, so on Desktop we provide a safe no-op stub: the class
    // constructs successfully, ExecuteNonQuery accepts queries silently, and
    // ExecuteQuery returns an empty ResultSet. This lets dual-mode apps boot
    // and exercise non-DB code paths on PC without crashing. Actual data
    // round-trip on Desktop requires a real BCL SQLite (e.g. via NuGet) - swap
    // this stub if you need that.
    public class SQLiteDatabase : IDisposable {
        private bool disposed;

        public SQLiteDatabase() {
        }

        public SQLiteDatabase(string file) {
        }

        ~SQLiteDatabase() => this.Dispose(false);

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void ExecuteNonQuery(string query) {
            if (this.disposed) throw new ObjectDisposedException("Object disposed.");
            if (query == null) throw new ArgumentNullException(nameof(query));
            // No-op on Desktop.
        }

        public ResultSet ExecuteQuery(string query) {
            if (this.disposed) throw new ObjectDisposedException("Object disposed.");
            if (query == null) throw new ArgumentNullException(nameof(query));
            // Return an empty ResultSet with a single placeholder column.
            // ResultSet's internal ctor requires at least one column name.
            return new ResultSet(new[] { "_" });
        }

        protected virtual void Dispose(bool disposing) => this.disposed = true;
    }
}
