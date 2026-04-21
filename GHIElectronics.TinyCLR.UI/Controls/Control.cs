////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class Control : UIElement {
        /// <summary>Lower values are visited first by <see cref="Input.FocusNavigator"/>.</summary>
        public int TabIndex { get; set; }

        /// <summary>When false, this control is skipped for keyboard focus navigation.</summary>
        public bool IsTabStop { get; set; } = true;

        /// <summary>When true and the control has focus, a focus rectangle is drawn.</summary>
        public bool ShowFocusVisual { get; set; } = true;

        /// <summary>Optional data context for lightweight binding (e.g. <see cref="TextBox.SetTextBinding"/>).</summary>
        public object DataContext { get; set; }

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

            if (this.ShowFocusVisual && this.IsFocused && this._renderWidth > 2 && this._renderHeight > 2) {
                var pen = new Pen(Theme.FocusRing, 2);
                var t = pen.Thickness;
                if (t < this._renderWidth && t < this._renderHeight) {
                    dc.DrawRectangle(null, pen, t / 2, t / 2, this._renderWidth - t, this._renderHeight - t);
                }
            }
        }

        protected internal Media.Brush _background = null;
        protected internal Media.Brush _foreground = new SolidColorBrush(Colors.Black);
        protected internal System.Drawing.Font _font;
    }
}


