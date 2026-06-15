////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace GHIElectronics.TinyCLR.UI.Media {
    /// <summary>A brush that paints a region with a linear color gradient.</summary>
    public sealed class LinearGradientBrush : Brush {
        /// <summary>The color at the start of the gradient.</summary>
        public Color StartColor;
        /// <summary>The color at the end of the gradient.</summary>
        public Color EndColor;

        /// <summary>How the gradient start and end coordinates are interpreted.</summary>
        public BrushMappingMode MappingMode = BrushMappingMode.RelativeToBoundingBox;

        /// <summary>The start coordinates of the gradient.</summary>
        public int StartX, StartY;
        /// <summary>The end coordinates of the gradient.</summary>
        public int EndX, EndY;

        /// <summary>The coordinate range used for relative gradient mapping.</summary>
        public const int RelativeBoundingBoxSize = 1000;

        /// <summary>Creates a gradient brush spanning the painted region.</summary>
        public LinearGradientBrush(Color startColor, Color endColor)
            : this(startColor, endColor, 0, 0, RelativeBoundingBoxSize, RelativeBoundingBoxSize) { }

        /// <summary>Creates a gradient brush with the given start and end coordinates.</summary>
        public LinearGradientBrush(Color startColor, Color endColor, int startX, int startY, int endX, int endY) {
            this.StartColor = startColor;
            this.EndColor = endColor;
            this.StartX = startX;
            this.StartY = startY;
            this.EndX = endX;
            this.EndY = endY;
        }

        internal override void RenderRectangle(Bitmap bmp, Pen pen, int x, int y, int width, int height) {
            var outlineColor = (pen != null) ? pen.Color : Colors.Transparent;
            var outlineThickness = (ushort)0;

            if (pen != null)
                outlineThickness = pen.Thickness;

            int x1, y1;
            int x2, y2;

            switch (this.MappingMode) {
                case BrushMappingMode.RelativeToBoundingBox:
                    x1 = x + (int)((long)(width - 1) * this.StartX / RelativeBoundingBoxSize);
                    y1 = y + (int)((long)(height - 1) * this.StartY / RelativeBoundingBoxSize);
                    x2 = x + (int)((long)(width - 1) * this.EndX / RelativeBoundingBoxSize);
                    y2 = y + (int)((long)(height - 1) * this.EndY / RelativeBoundingBoxSize);
                    break;
                default: //case BrushMappingMode.Absolute:
                    x1 = this.StartX;
                    y1 = this.StartY;
                    x2 = this.EndX;
                    y2 = this.EndY;
                    break;
            }

            bmp.DrawRectangle(outlineColor, outlineThickness, x, y, width, height, 0, 0,
                                          this.StartColor, x1, y1, this.EndColor, x2, y2, this.Opacity);
        }
    }
}


