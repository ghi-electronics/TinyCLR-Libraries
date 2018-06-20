////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class Control : UIElement {
        public Media.Brush Background {
            get {
                VerifyAccess();

                return this._background;
            }

            set {
                VerifyAccess();

                this._background = value;
                Invalidate();
            }
        }

        public System.Drawing.Font Font {
            get => this._font;

            set {
                VerifyAccess();

                this._font = value;
                InvalidateMeasure();
            }
        }

        public Media.Brush Foreground {
            get {
                VerifyAccess();

                return this._foreground;
            }

            set {
                VerifyAccess();

                this._foreground = value;
                Invalidate();
            }
        }

        public override void OnRender(DrawingContext dc) {
            if (this._background != null) {
                dc.DrawRectangle(this._background, null, 0, 0, this._renderWidth, this._renderHeight);
            }
        }

        protected internal Media.Brush _background = null;
        protected internal Media.Brush _foreground = new SolidColorBrush(Colors.Black);
        protected internal System.Drawing.Font _font;
    }
}


