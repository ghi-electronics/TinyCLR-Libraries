////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>
    /// The DataGridItem class describes an item in a DataGrid component.
    /// </summary>
    public class DataGridItem : IComparable {
        /// <summary>
        /// Contains the data for each column within this row.
        /// </summary>
        public object[] data;

        /// <summary>
        /// Contains the data type for each column within this row.
        /// </summary>
        public string[] dataType;

        /// <summary>
        /// Creates a new DataGridItem.
        /// </summary>
        /// <param name="data">Object containing data for each column.</param>
        public DataGridItem(object[] data) {
            this.data = data;
            this.dataType = new string[data.Length];
            for (var i = 0; i < data.Length; i++)
                this.dataType[i] = data[i].GetType().FullName;
        }

        /// <summary>
        /// This is used to be compliant with IComparable.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj) => throw new NotImplementedException();

        /// <summary>
        /// Compares this item's column to another DataGridItem's column.
        /// </summary>
        /// <param name="row">DataGridItem object.</param>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Number indicating how it should be positioned.</returns>
        public int CompareTo(DataGridItem row, int columnIndex) {
            switch (this.dataType[columnIndex]) {
                case "System.Float":
                    var float_ = (float)this.data[columnIndex] - (float)row.data[columnIndex];
                    if (float_ > 0)
                        return 1;
                    else if (float_ < 0)
                        return -1;
                    else
                        return 0;

                case "System.Int16":
                    return (short)this.data[columnIndex] - (short)row.data[columnIndex];

                case "System.Int32":
                    return (int)this.data[columnIndex] - (int)row.data[columnIndex];

                case "System.Int64":
                    var int64_ = (long)this.data[columnIndex] - (long)row.data[columnIndex];
                    if (int64_ > 0)
                        return 1;
                    else if (int64_ < 0)
                        return -1;
                    else
                        return 0;

                case "System.String":
                    return ((string)this.data[columnIndex]).CompareTo((string)row.data[columnIndex]);

                case "System.UInt16":
                    return (ushort)this.data[columnIndex] - (ushort)row.data[columnIndex];

                case "System.UInt32":
                    var uInt32_ = (uint)this.data[columnIndex] - (uint)row.data[columnIndex];
                    if (uInt32_ > 0)
                        return 1;
                    else if (uInt32_ < 0)
                        return -1;
                    else
                        return 0;

                case "System.UInt64":
                    var uInt64_ = (ulong)this.data[columnIndex] - (ulong)row.data[columnIndex];
                    if (uInt64_ > 0)
                        return 1;
                    else if (uInt64_ < 0)
                        return -1;
                    else
                        return 0;

                default:
                    return 0;
            }
        }
    }
}
