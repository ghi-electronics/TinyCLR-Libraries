////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GHIElectronics.TinyCLR.UI.Media {
    public sealed class ImageBrush : Brush {
        public ImageSource ImageSource;
        public Stretch Stretch = Stretch.Fill;

        public ImageBrush(ImageSource imagesource) => this.ImageSource = imagesource;

        internal override void RenderRectangle(Bitmap bmp, Pen pen, int x, int y, int width, int height) {
            if (this.Stretch == Stretch.None) {
                bmp.DrawImage(x, y, this.ImageSource, 0, 0, this.ImageSource.Width, this.ImageSource.Height, this.Opacity);
            }
            else if (width == this.ImageSource.Width && height == this.ImageSource.Height) {
                bmp.DrawImage(x, y, this.ImageSource, 0, 0, width, height, this.Opacity);
            }
            else {
                bmp.StretchImage(x, y, this.ImageSource, width, height, this.Opacity);
            }

            if (pen != null && pen.Thickness > 0) {
                bmp.DrawRectangle(pen.Color, pen.Thickness, x, y, width, height, 0, 0,
                                      Colors.Transparent, 0, 0, Colors.Transparent, 0, 0, 0);
            }
        }
    }
}


