////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Media {
    /// <summary>Base class for objects that paint a region.</summary>
    public abstract class Brush {
        private ushort _opacity = Bitmap.OpacityOpaque;

        /// <summary>The opacity of the brush.</summary>
        public ushort Opacity {
            get => this._opacity;
            set {
                // clip values
                if (value > Bitmap.OpacityOpaque) value = Bitmap.OpacityOpaque;

                this._opacity = value;
            }
        }

        internal abstract void RenderRectangle(Bitmap bmp, Pen outline, int x, int y, int width, int height);
        internal virtual void RenderEllipse(Bitmap bmp, Pen outline, int x, int y, int xRadius, int yRadius) => throw new NotSupportedException("RenderEllipse is not supported with this brush.");

        internal virtual void RenderPolygon(Bitmap bmp, Pen outline, int[] pts) => throw new NotSupportedException("RenderPolygon is not supported with this brush.");
    }

    /// <summary>Specifies how brush coordinates are interpreted.</summary>
    public enum BrushMappingMode {
        /// <summary>Coordinates are interpreted as absolute pixel values.</summary>
        Absolute,
        /// <summary>Coordinates are relative to the bounding box of the painted region.</summary>
        RelativeToBoundingBox
    }
}
