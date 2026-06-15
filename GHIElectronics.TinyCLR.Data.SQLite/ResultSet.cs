using System;
using System.Collections;

namespace GHIElectronics.TinyCLR.Data.SQLite {
    /// <summary>
    /// Result of a SQL SELECT — rows × columns plus column-name metadata. Index by
    /// <c>this[row]</c> for an ArrayList of cells in that row, or <c>this[row, column]</c>
    /// for an individual cell.
    /// </summary>
    public class ResultSet {
        private int rowCount;
        private int columnCount;
        private string[] columnNames;
        private ArrayList data;

        /// <summary>The number of rows in the result set.</summary>
        public int RowCount => this.rowCount;
        /// <summary>The number of columns in the result set.</summary>
        public int ColumnCount => this.columnCount;
        /// <summary>The names of the columns in the result set.</summary>
        public string[] ColumnNames => this.columnNames;
        /// <summary>The rows of the result set, each an ArrayList of cell values.</summary>
        public ArrayList Data => this.data;

        /// <summary>Gets the cells of the given row as an ArrayList.</summary>
        public ArrayList this[int row] {
            get {
                if (row < 0 || row >= this.rowCount) throw new ArgumentOutOfRangeException(nameof(row));

                return (ArrayList)this.Data[row];
            }
        }

        /// <summary>Gets the value of the cell at the given row and column.</summary>
        public object this[int row, int column] {
            get {
                if (row < 0 || row >= this.rowCount) throw new ArgumentOutOfRangeException(nameof(row));
                if (column < 0 || column >= this.columnCount) throw new ArgumentOutOfRangeException(nameof(column));

                return ((ArrayList)this.Data[row])[column];
            }
        }

        internal ResultSet(string[] columnNames) {
            if (columnNames == null) throw new ArgumentNullException(nameof(columnNames));
            if (columnNames.Length == 0) throw new ArgumentException("At least one column must be provided.", nameof(columnNames));

            this.data = new ArrayList();
            this.columnNames = new string[columnNames.Length];
            this.columnCount = columnNames.Length;
            this.rowCount = 0;

            Array.Copy(columnNames, this.columnNames, columnNames.Length);
        }

        internal void AddRow(ArrayList row) {
            if (row.Count != this.columnCount) throw new ArgumentException("Row must contain exactly as many members as the number of columns in this result set.", nameof(row));

            this.rowCount++;
            this.data.Add(row);
        }
    }
}
