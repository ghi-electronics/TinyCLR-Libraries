////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Shapes {
    /// <summary>Draws a rectangle.</summary>
    public class Rectangle : Shape {
        /// <summary>Creates a rectangle with no size.</summary>
        public Rectangle() {
            this.Width = 0;
            this.Height = 0;
        }

        /// <summary>Creates a rectangle of the given width and height.</summary>
        public Rectangle(int width, int height) {
            if (width < 0 || height < 0) {
                throw new ArgumentException();
            }

            this.Width = width;
            this.Height = height;
        }

        /// <summary>Renders the rectangle to the drawing context.</summary>
        public override void OnRender(Media.DrawingContext dc) {
            var offset = this.Stroke != null ? this.Stroke.Thickness / 2 : 0;

            dc.DrawRectangle(this.Fill, this.Stroke, offset, offset, this._renderWidth - 2 * offset, this._renderHeight - 2 * offset);
        }
    }
}
