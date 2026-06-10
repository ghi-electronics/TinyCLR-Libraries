////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>
    /// Column descriptor for <see cref="DataGrid"/>.
    /// </summary>
    public class DataGridColumn {
        /// <summary>Display label for the column header.</summary>
        public string Label { get; set; }

        /// <summary>Column width in pixels.</summary>
        public int Width { get; set; }

        /// <summary>Default sort order applied when the column header is tapped.</summary>
        public DataGrid.Order Order { get; set; }

        /// <summary>Creates a new column with the given label and pixel width.</summary>
        public DataGridColumn(string label, int width) {
            this.Label = label;
            this.Width = width;
            this.Order = DataGrid.Order.ASC;
        }

        /// <summary>Toggles between ASC and DESC.</summary>
        public void ToggleOrder() => this.Order = (this.Order == DataGrid.Order.ASC) ? DataGrid.Order.DESC : DataGrid.Order.ASC;
    }
}
