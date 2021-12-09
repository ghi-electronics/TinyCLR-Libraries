////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>
    /// The DataGridColumn class describes a column in a DataGrid component.
    /// </summary>
    public class DataGridColumn {
        /// <summary>
        /// The column name to be displayed.
        /// </summary>
        public string label;

        /// <summary>
        /// The width of the column, in pixels.
        /// </summary>
        public int width;

        /// <summary>
        /// Indicates the default order of this column.
        /// </summary>
        public DataGrid.Order order;

        /// <summary>
        /// Creates a new DataGridColumn instance.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="width"></param>
        public DataGridColumn(string label, int width) {
            this.label = label;
            this.width = width;

            // Default
            this.order = DataGrid.Order.ASC;
        }

        /// <summary>
        /// Toggles the ordering of this column.
        /// </summary>
        public void ToggleOrder() {
            if (this.order == DataGrid.Order.ASC)
                this.order = DataGrid.Order.DESC;
            else
                this.order = DataGrid.Order.ASC;
        }
    }
}
