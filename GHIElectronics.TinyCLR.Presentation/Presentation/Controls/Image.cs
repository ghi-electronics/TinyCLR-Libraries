////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Controls {
    /// <summary>
    /// Summary description for Image.
    /// </summary>
    public class Image : UIElement {
        public Image() {
        }

        public Image(Bitmap bmp)
            : this() => this._bitmap = bmp;

        public Bitmap Bitmap {
            get {
                VerifyAccess();

                return this._bitmap;
            }

            set {
                VerifyAccess();

                this._bitmap = value;
                InvalidateMeasure();
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            desiredWidth = desiredHeight = 0;
            if (this._bitmap != null) {
                desiredWidth = this._bitmap.Width;
                desiredHeight = this._bitmap.Height;
            }
        }

        public override void OnRender(DrawingContext dc) {
            var bmp = this._bitmap;
            if (bmp != null) {
                dc.DrawImage(this._bitmap, 0, 0);
            }
        }

        private Bitmap _bitmap;
    }
}


