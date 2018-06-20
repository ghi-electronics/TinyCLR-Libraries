////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.UI.Shapes {
    public class Ellipse : Shape {
        public Ellipse(int xRadius, int yRadius) {
            if (xRadius < 0 || yRadius < 0) {
                throw new ArgumentException();
            }

            this.Width = xRadius * 2 + 1;
            this.Height = yRadius * 2 + 1;
        }

        public override void OnRender(Media.DrawingContext dc) {
            /// Make room for cases when strokes are thick.
            var x = this._renderWidth / 2 + this.Stroke.Thickness - 1;
            var y = this._renderHeight / 2 + this.Stroke.Thickness - 1;
            var w = this._renderWidth / 2 - (this.Stroke.Thickness - 1) * 2;
            var h = this._renderHeight / 2 - (this.Stroke.Thickness - 1) * 2;

            dc.DrawEllipse(this.Fill, this.Stroke, x, y, w, h);
        }
    }
}
