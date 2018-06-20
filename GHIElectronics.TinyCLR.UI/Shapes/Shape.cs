////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Shapes {
    public abstract class Shape : UIElement {
        public Media.Brush Fill {
            get {
                if (this._fill == null) {
                    this._fill = new SolidColorBrush(Colors.Black) {
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
                    this._stroke = new Media.Pen(Colors.White, 0);
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


