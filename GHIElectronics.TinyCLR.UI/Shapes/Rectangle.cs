////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Shapes {
    public class Rectangle : Shape {
        public Rectangle() {
            this.Width = 0;
            this.Height = 0;
        }

        public Rectangle(int width, int height) {
            if (width < 0 || height < 0) {
                throw new ArgumentException();
            }

            this.Width = width;
            this.Height = height;
        }

        public override void OnRender(Media.DrawingContext dc) {
            var offset = this.Stroke != null ? this.Stroke.Thickness / 2 : 0;

            dc.DrawRectangle(this.Fill, this.Stroke, offset, offset, this._renderWidth - 2 * offset, this._renderHeight - 2 * offset);
        }
    }
}
