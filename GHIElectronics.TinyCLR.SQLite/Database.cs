using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.SQLite {
    /// <summary>Represents the results of SQLite query.</summary>
    public class ResultSet {
        private int rowCount;
        private int columnCount;
        private string[] columnNames;
        private ArrayList data;

        /// <summary>The number of rows in the result set.</summary>
        public int RowCount => this.rowCount;

        /// <summary>The number of columns in the result set.</summary>
        public int ColumnCount => this.columnCount;

        /// <summary>The names of the columns.</summary>
        public string[] ColumnNames => this.columnNames;

        /// <summary>
        /// The result data. Each entry in the array list represents one row as an ArrayList where each entry represents a cell in that row.
        /// </summary>
        public ArrayList Data => this.data;

        /// <summary>The row data for the row with the given index.</summary>
        /// <param name="row">The row to access.</param>
        /// <returns>The ArrayList for that row.</returns>
        public ArrayList this[int row] {
            get {
                if (row < 0 || row >= this.rowCount) throw new ArgumentOutOfRangeException("row");

                return (ArrayList)this.Data[row];
            }
        }

        /// <summary>Accesses the data at the given row and column.</summary>
        /// <param name="row">The row to access.</param>
        /// <param name="column">The column to access.</param>
        /// <returns>The object at that row and column.</returns>
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
    /// <summary>A SQLite database. See https://www.ghielectronics.com/docs/135/ for more information.</summary>
    /// <remarks>
    /// Supported SQLite database version 3.7.13. This class exposes simple methods to open, close and process SQL queries. Currently, this version
    /// supports INTEGER, DOUBLE and TEXT record types.
    /// </remarks>
    public class Database : IDisposable {
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

        /// <summary>Creates a SQLite database in memory</summary>
        public Database() {
            this.nativePointer = 0;
            this.disposed = false;

            if (this.NativeOpen(":memory:") != SQLITE_OK) throw new OpenException();
        }

        /// <summary>Opens or creates a SQLite database with the specified path to a file, such as "\\SD\\Database.dat"</summary>
        /// <param name="file">The path to the file.</param>
        public Database(string file) {
            this.nativePointer = 0;
            this.disposed = false;

            file = Path.GetFullPath(file);

            if (file == null) throw new ArgumentException("You must provide a valid file.", "file");

            if (this.NativeOpen(file) != SQLITE_OK)
                throw new OpenException();
        }

        /// <summary>The finalizer.</summary>
        ~Database() {
            this.Dispose(false);
        }

        /// <summary>Closes the database.</summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Executes a query that returns no results</summary>
        /// <param name="query">The SQL query to execute</param>
        public void ExecuteNonQuery(string query) {
            if (this.disposed) throw new ObjectDisposedException();
            if (query == null) throw new ArgumentNullException("query");

            var handle = this.PrepareSqlStatement(query);

            if (this.NativeStep(handle) != SQLITE_DONE)
                throw new QueryExecutionException(this.NativeErrorMessage());

            this.FinalizeSqlStatment(handle);
        }

        /// <summary>Executes a query and returns the results.</summary>
        /// <param name="query">The SQL query to execute</param>
        /// <returns>The results of the query.</returns>
        public ResultSet ExecuteQuery(string query) {
            if (this.disposed) throw new ObjectDisposedException();
            if (query == null) throw new ArgumentNullException("query");

            var handle = this.PrepareSqlStatement(query);
            var columnCount = this.NativeColumnCount(handle);
            var columnNames = new string[columnCount];

            for (var i = 0; i < columnCount; i++)
                columnNames[i] = this.NativeColumnName(handle, i);

            var results = new ResultSet(columnNames);

            while (this.NativeStep(handle) == Database.SQLITE_ROW) {
                var row = new ArrayList();

                for (var i = 0; i < columnCount; i++) {
                    switch (this.NativeColumnType(handle, i)) {
                        case Database.SQLITE_INTEGER: row.Add(this.NativeColumnLong(handle, i)); break;
                        case Database.SQLITE_TEXT: row.Add(this.NativeColumnText(handle, i)); break;
                        case Database.SQLITE_FLOAT: row.Add(this.NativeColumnDouble(handle, i)); break;
                        case Database.SQLITE_NULL: row.Add(null); break;
                        case Database.SQLITE_BLOB:
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

        /// <summary>Closes the database.</summary>
        /// <param name="disposing">Whether or not this is called from Dispose.</param>
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
        /// <summary>The exception thrown when the database fails to execute a query.</summary>
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

        /// <summary>The exception thrown when the database fails to finalize a query.</summary>
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

        /// <summary>The exception thrown when the database fails to prepare the query.</summary>
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

        /// <summary>The exception thrown when the database fails to open.</summary>
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
