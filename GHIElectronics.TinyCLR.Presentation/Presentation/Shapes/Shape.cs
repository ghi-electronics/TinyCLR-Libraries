////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Drawing;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.SPOT.Presentation.Shapes {
    public abstract class Shape : UIElement {
        public Media.Brush Fill {
            get {
                if (this._fill == null) {
                    this._fill = new SolidColorBrush(Color.Black) {
                        Opacity = Bitmap.OpacityTransparent
                    };
                }

                return this._fill;
            }

            set {
                this._fill = value;
                Invalidate();
            }
        }

        public Media.Pen Stroke {
            get {
                if (this._stroke == null) {
                    this._stroke = new Media.Pen(Color.White, 0);
                }

                return this._stroke;
            }

            set {
                this._stroke = value;
                Invalidate();
            }
        }

        private Media.Brush _fill;
        private Media.Pen _stroke;
    }
}


