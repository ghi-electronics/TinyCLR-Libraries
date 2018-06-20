////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Media {
    public abstract class Brush {
        private ushort _opacity = Bitmap.OpacityOpaque;

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

    public enum BrushMappingMode {
        Absolute,
        RelativeToBoundingBox
    }
}
