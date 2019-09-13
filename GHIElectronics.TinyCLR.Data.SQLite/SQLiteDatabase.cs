using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Data.SQLite {
    public class SQLiteDatabase : IDisposable {
        private const int SQLITE_OK = 0;
        private const int SQLITE_ROW = 100;
        private const int SQLITE_DONE = 101;
        private const int SQLITE_INTEGER = 1;
        private const int SQLITE_FLOAT = 2;
        private const int SQLITE_TEXT = 3;
        private const int SQLITE_BLOB = 4;
        private const int SQLITE_NULL = 5;
        private bool disposed;

#pragma warning disable 0414
        private int nativePointer;
#pragma warning restore 0414

        public SQLiteDatabase() {
            this.nativePointer = 0;
            this.disposed = false;

            if (this.NativeOpen(":memory:") != SQLITE_OK) throw new OpenException();
        }

        public SQLiteDatabase(string file) {
            this.nativePointer = 0;
            this.disposed = false;

            file = Path.GetFullPath(file);

            if (file == null) throw new ArgumentException("You must provide a valid file.", nameof(file));

            if (this.NativeOpen(file) != SQLITE_OK)
                throw new OpenException();
        }

        ~SQLiteDatabase() {
            this.Dispose(false);
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void ExecuteNonQuery(string query) {
            if (this.disposed) throw new ObjectDisposedException("Object disposed.");
            if (query == null) throw new ArgumentNullException(nameof(query));

            var handle = this.PrepareSqlStatement(query);

            if (this.NativeStep(handle) != SQLITE_DONE)
                throw new QueryExecutionException(this.NativeErrorMessage());

            this.FinalizeSqlStatment(handle);
        }

        public ResultSet ExecuteQuery(string query) {
            if (this.disposed) throw new ObjectDisposedException("Object disposed.");
            if (query == null) throw new ArgumentNullException(nameof(query));

            var handle = this.PrepareSqlStatement(query);
            var columnCount = this.NativeColumnCount(handle);
            var columnNames = new string[columnCount];

            for (var i = 0; i < columnCount; i++)
                columnNames[i] = this.NativeColumnName(handle, i);

            var results = new ResultSet(columnNames);

            while (this.NativeStep(handle) == SQLiteDatabase.SQLITE_ROW) {
                var row = new ArrayList();

                for (var i = 0; i < columnCount; i++) {
                    switch (this.NativeColumnType(handle, i)) {
                        case SQLiteDatabase.SQLITE_INTEGER: row.Add(this.NativeColumnLong(handle, i)); break;
                        case SQLiteDatabase.SQLITE_TEXT: row.Add(this.NativeColumnText(handle, i)); break;
                        case SQLiteDatabase.SQLITE_FLOAT: row.Add(this.NativeColumnDouble(handle, i)); break;
                        case SQLiteDatabase.SQLITE_NULL: row.Add(null); break;
                        case SQLiteDatabase.SQLITE_BLOB:
                            var length = this.NativeColumnBlobLength(handle, i);

                            if (length == 0) {
                                row.Add(null);

                                break;
                            }

                            var buffer = new byte[length];
                            this.NativeColumnBlobData(handle, i, buffer);
                            row.Add(buffer);

                            break;
                    }
                }

                results.AddRow(row);
            }

            this.FinalizeSqlStatment(handle);

            return results;
        }

        protected virtual void Dispose(bool disposing) {
            if (this.disposed)
                return;

            this.NativeClose();

            this.disposed = true;
        }

        private int PrepareSqlStatement(string query) {
            if (this.disposed) throw new ObjectDisposedException("Object disposed.");

            if (this.NativePrepare(query, query.Length, out var handle) != SQLITE_OK)
                throw new QueryPrepareException(this.NativeErrorMessage());

            return handle;
        }

        private void FinalizeSqlStatment(int handle) {
            if (this.disposed) throw new ObjectDisposedException("Object disposed.");

            if (this.NativeFinalize(handle) != SQLITE_OK)
                throw new QueryFinalizationException(this.NativeErrorMessage());
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeOpen(string filename);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeClose();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativePrepare(string query, int queryLength, out int handle);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern string NativeErrorMessage();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeStep(int handle);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeFinalize(int handle);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeColumnCount(int handle);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern string NativeColumnName(int handle, int column);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeColumnType(int handle, int column);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern long NativeColumnLong(int handle, int column);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern string NativeColumnText(int handle, int column);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern double NativeColumnDouble(int handle, int column);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int NativeColumnBlobLength(int handle, int column);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeColumnBlobData(int handle, int column, byte[] buffer);
    }
}
