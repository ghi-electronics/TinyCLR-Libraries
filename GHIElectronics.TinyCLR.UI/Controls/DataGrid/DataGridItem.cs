////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

namespace GHIElectronics.TinyCLR.UI.Controls
{
    /// <summary>
    /// The DataGridItem class describes an item in a DataGrid component.
    /// </summary>
    public class DataGridItem : IComparable
    {
        /// <summary>
        /// Contains the data for each column within this row.
        /// </summary>
        public object[] Data;

        /// <summary>
        /// Contains the data type for each column within this row.
        /// </summary>
        public string[] DataType;

        /// <summary>
        /// Creates a new DataGridItem.
        /// </summary>
        /// <param name="data">Object containing data for each column.</param>
        public DataGridItem(object[] data)
        {
            Data = data;
            DataType = new string[data.Length];
            for (int i = 0; i < data.Length; i++)
                DataType[i] = data[i].GetType().FullName;
        }

        /// <summary>
        /// This is used to be compliant with IComparable.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compares this item's column to another DataGridItem's column.
        /// </summary>
        /// <param name="row">DataGridItem object.</param>
        /// <param name="columnIndex">Column index.</param>
        /// <returns>Number indicating how it should be positioned.</returns>
        public int CompareTo(DataGridItem row, int columnIndex)
        {
            switch (DataType[columnIndex])
            {
                case "System.Float":
                    float _float = (float)Data[columnIndex] - (float)row.Data[columnIndex];
                    if (_float > 0)
                        return 1;
                    else if (_float < 0)
                        return -1;
                    else
                        return 0;

                case "System.Int16":
                    return (Int16)Data[columnIndex] - (Int16)row.Data[columnIndex];

                case "System.Int32":
                    return (Int32)Data[columnIndex] - (Int32)row.Data[columnIndex];

                case "System.Int64":
                    Int64 _int64 = (Int64)Data[columnIndex] - (Int64)row.Data[columnIndex];
                    if (_int64 > 0)
                        return 1;
                    else if (_int64 < 0)
                        return -1;
                    else
                        return 0;

                case "System.String":
                    return ((string)Data[columnIndex]).CompareTo((string)row.Data[columnIndex]);

                case "System.UInt16":
                    return (UInt16)Data[columnIndex] - (UInt16)row.Data[columnIndex];

                case "System.UInt32":
                    UInt32 _uInt32 = (UInt32)Data[columnIndex] - (UInt32)row.Data[columnIndex];
                    if (_uInt32 > 0)
                        return 1;
                    else if (_uInt32 < 0)
                        return -1;
                    else
                        return 0;

                case "System.UInt64":
                    UInt64 _uInt64 = (UInt64)Data[columnIndex] - (UInt64)row.Data[columnIndex];
                    if (_uInt64 > 0)
                        return 1;
                    else if (_uInt64 < 0)
                        return -1;
                    else
                        return 0;

                default:
                    return 0;
            }
        }
    }
}
