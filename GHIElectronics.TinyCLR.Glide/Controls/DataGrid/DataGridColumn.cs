////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GHIElectronics.TinyCLR.UI.Controls
{
    /// <summary>
    /// The DataGridColumn class describes a column in a DataGrid component.
    /// </summary>
    public class DataGridColumn
    {
        /// <summary>
        /// The column name to be displayed.
        /// </summary>
        public string Label;

        /// <summary>
        /// The width of the column, in pixels.
        /// </summary>
        public int Width;

        /// <summary>
        /// Indicates the default order of this column.
        /// </summary>
        public DataGrid.Order Order;

        /// <summary>
        /// Creates a new DataGridColumn instance.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="width"></param>
        public DataGridColumn(string label, int width)
        {
            Label = label;
            Width = width;

            // Default
            Order = DataGrid.Order.ASC;
        }

        /// <summary>
        /// Toggles the ordering of this column.
        /// </summary>
        public void ToggleOrder()
        {
            if (Order == DataGrid.Order.ASC)
                Order = DataGrid.Order.DESC;
            else
                Order = DataGrid.Order.ASC;
        }
    }
}
