using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.SQLite {

    public class ResultSet {
        private int rowCount;
        private int columnCount;
        private string[] columnNames;
        private ArrayList data;

        public int RowCount => this.rowCount;
        public int ColumnCount => this.columnCount;
        public string[] ColumnNames => this.columnNames;
        public ArrayList Data => this.data;

        public ArrayList this[int row] {
            get {
                if (row < 0 || row >= this.rowCount) throw new ArgumentOutOfRangeException("row");

                return (ArrayList)this.Data[row];
            }
        }

        public object this[int row, int column] {
            get {
                if (row < 0 || row >= this.rowCount) throw new ArgumentOutOfRangeException("row");
                if (column < 0 || column >= this.columnCount) throw new ArgumentOutOfRangeException("column");

                return ((ArrayList)this.Data[row])[column];
            }
        }

        internal ResultSet(string[] columnNames) {
            if (columnNames == null) throw new ArgumentNullException("columnNames");
            if (columnNames.Length == 0) throw new ArgumentException("At least one column must be provided.", "columnNames");

            this.data = new ArrayList();
            this.columnNames = new string[columnNames.Length];
            this.columnCount = columnNames.Length;
            this.rowCount = 0;

            Array.Copy(columnNames, this.columnNames, columnNames.Length);
        }

        internal void AddRow(ArrayList row) {
            if (row.Count != this.columnCount) throw new ArgumentException("Row must contain exactly as many members as the number of columns in this result set.", "row");

            this.rowCount++;
            this.data.Add(row);
        }
    }

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

            if (file == null) throw new ArgumentException("You must provide a valid file.", "file");

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
            if (this.disposed) throw new ObjectDisposedException();
            if (query == null) throw new ArgumentNullException("query");

            var handle = this.PrepareSqlStatement(query);

            if (this.NativeStep(handle) != SQLITE_DONE)
                throw new QueryExecutionException(this.NativeErrorMessage());

            this.FinalizeSqlStatment(handle);
        }

        public ResultSet ExecuteQuery(string query) {
            if (this.disposed) throw new ObjectDisposedException();
            if (query == null) throw new ArgumentNullException("query");

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
            if (this.disposed) throw new ObjectDisposedException();

            if (this.NativePrepare(query, query.Length, out var handle) != SQLITE_OK)
                throw new QueryPrepareException(this.NativeErrorMessage());

            return handle;
        }

        private void FinalizeSqlStatment(int handle) {
            if (this.disposed) throw new ObjectDisposedException();

            if (this.NativeFinalize(handle) != SQLITE_OK)
                throw new QueryFinalizationException(this.NativeErrorMessage());
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeOpen(string filename);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeClose();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativePrepare(string query, int queryLength, out int handle);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private string NativeErrorMessage();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeStep(int handle);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeFinalize(int handle);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeColumnCount(int handle);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private string NativeColumnName(int handle, int column);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeColumnType(int handle, int column);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private long NativeColumnLong(int handle, int column);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private string NativeColumnText(int handle, int column);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private double NativeColumnDouble(int handle, int column);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private int NativeColumnBlobLength(int handle, int column);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void NativeColumnBlobData(int handle, int column, byte[] buffer);

        [Serializable]
        public class QueryExecutionException : System.Exception {

            internal QueryExecutionException()
                : base() {
            }

            internal QueryExecutionException(string message)
                : base(message) {
            }

            internal QueryExecutionException(string message, Exception innerException)
                : base(message, innerException) {
            }
        }

        [Serializable]
        public class QueryFinalizationException : System.Exception {

            internal QueryFinalizationException()
                : base() {
            }

            internal QueryFinalizationException(string message)
                : base(message) {
            }

            internal QueryFinalizationException(string message, Exception innerException)
                : base(message, innerException) {
            }
        }

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

        [Serializable]
        public class OpenException : System.Exception {

            internal OpenException()
                : base() {
            }

            internal OpenException(string message)
                : base(message) {
            }

            internal OpenException(string message, Exception innerException)
                : base(message, innerException) {
            }
        }
#pragma warning disable 0414
#pragma warning restore 0414
    }
}
