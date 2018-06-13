////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Drawing;

namespace Microsoft.SPOT.Presentation.Media {
    public sealed class ImageBrush : Brush {
        public Bitmap BitmapSource;
        public Stretch Stretch = Stretch.Fill;

        public ImageBrush(Bitmap bmp) => this.BitmapSource = bmp;

        protected internal override void RenderRectangle(Bitmap bmp, Pen pen, int x, int y, int width, int height) {
            if (this.Stretch == Stretch.None) {
                bmp.DrawImage(x, y, this.BitmapSource, 0, 0, this.BitmapSource.Width, this.BitmapSource.Height, this.Opacity);
            }
            else if (width == this.BitmapSource.Width && height == this.BitmapSource.Height) {
                bmp.DrawImage(x, y, this.BitmapSource, 0, 0, width, height, this.Opacity);
            }
            else {
                bmp.StretchImage(x, y, this.BitmapSource, width, height, this.Opacity);
            }

            if (pen != null && pen.Thickness > 0) {
                bmp.DrawRectangle(pen.Color, pen.Thickness, x, y, width, height, 0, 0,
                                      Color.Transparent, 0, 0, Color.Transparent, 0, 0, 0);
            }
        }
    }
}


