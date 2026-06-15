////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace GHIElectronics.TinyCLR.UI.Media {
    /// <summary>Specifies how content is stretched to fill a region.</summary>
    public enum Stretch {
        /// <summary>The content is drawn at its original size without stretching.</summary>
        None,
        /// <summary>The content is stretched to fill the region exactly.</summary>
        Fill,
        /// <summary>The content is scaled uniformly to fit within the region.</summary>
        Uniform,
        /// <summary>The content is scaled uniformly to completely fill the region.</summary>
        UniformToFill,
    }
}


