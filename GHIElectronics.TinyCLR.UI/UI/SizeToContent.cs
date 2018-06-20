////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace GHIElectronics.TinyCLR.UI {
    /// <summary>
    /// SizeToContent
    /// </summary>
    public enum SizeToContent {
        /// <summary>
        /// Does not size to content
        /// </summary>
        Manual = 0,
        /// <summary>
        /// Sizes Width to content's Width
        /// </summary>
        Width = 1,
        /// <summary>
        /// Sizes Height to content's Height
        /// </summary>
        Height = 2,
        /// <summary>
        /// Sizes both Width and Height to content's size
        /// </summary>
        WidthAndHeight = 3,
        // Please update IsValidSizeToContent if there are name changes.
    }
}


