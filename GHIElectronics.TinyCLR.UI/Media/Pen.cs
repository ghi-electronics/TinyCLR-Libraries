////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Drawing;

namespace GHIElectronics.TinyCLR.UI.Media {
    /// <summary>Describes the color and thickness used to draw outlines.</summary>
    public class Pen {
        /// <summary>The color of the pen.</summary>
        public Color Color;
        /// <summary>The thickness of the pen in pixels.</summary>
        public ushort Thickness;

        /// <summary>Creates a pen of the given color with a thickness of one.</summary>
        public Pen(Color color)
            : this(color, 1) {
        }

        /// <summary>Creates a pen of the given color and thickness.</summary>
        public Pen(Color color, ushort thickness) {
            this.Color = color;
            this.Thickness = thickness;
        }
    }
}


