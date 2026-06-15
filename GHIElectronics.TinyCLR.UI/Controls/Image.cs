////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>
    /// Summary description for Image.
    /// </summary>
    public class Image : UIElement {
        /// <summary>How the image is scaled to fill the control.</summary>
        public Stretch Stretch { get; set; } = Stretch.None;

        /// <summary>The image to display.</summary>
        public ImageSource Source {
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

        /// <summary>Measures the desired size based on the image and stretch mode.</summary>
        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            desiredWidth = desiredHeight = 0;
            if (this._bitmap != null) {
                switch (this.Stretch) {
                    case Stretch.None:
                        desiredWidth = this._bitmap.Width;
                        desiredHeight = this._bitmap.Height;
                        break;

                    case Stretch.Fill:
                        // Width/Height are optional with Stretch.Fill — when the
                        // caller didn't specify, fall back to the bitmap's
                        // natural size instead of throwing "width not set" and
                        // tearing down the parent's paint pass.
                        desiredWidth = this.IsWidthSet(out var fillW) ? fillW : this._bitmap.Width;
                        desiredHeight = this.IsHeightSet(out var fillH) ? fillH : this._bitmap.Height;
                        break;

                    default: throw new NotSupportedException("Stretch value " + this.Stretch + " is not supported. Use Stretch.None or Stretch.Fill.");
                }
            }
        }

        /// <summary>Draws the image using the current stretch mode.</summary>
        public override void OnRender(DrawingContext dc) {
            var bmp = this._bitmap;
            if (bmp != null) {
                switch (this.Stretch) {
                    case Stretch.None:
                        dc.DrawImage(this._bitmap, 0, 0);
                        break;

                    case Stretch.Fill:
                        dc.StretchImage(0, 0, this._renderWidth, this._renderHeight, this._bitmap, 0, 0, this._bitmap.Width, this._bitmap.Height, Bitmap.OpacityOpaque);
                        break;
                }
            }
        }

        private ImageSource _bitmap;
    }
}


