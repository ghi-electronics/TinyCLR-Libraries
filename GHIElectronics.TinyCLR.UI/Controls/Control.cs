////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>Base class for focusable UI controls with background, foreground and font.</summary>
    public class Control : UIElement {
        /// <summary>Lower values are visited first by <see cref="Input.FocusNavigator"/>.</summary>
        public int TabIndex { get; set; }

        /// <summary>When false, this control is skipped for keyboard focus navigation.</summary>
        public bool IsTabStop { get; set; } = true;

        /// <summary>When true and the control has focus, a focus rectangle is drawn.</summary>
        public bool ShowFocusVisual { get; set; } = true;

        /// <summary>Optional data context for lightweight binding (e.g. <see cref="TextBox.SetTextBinding"/>).</summary>
        public object DataContext { get; set; }

        /// <summary>The brush used to paint the control's background.</summary>
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

        /// <summary>The font used by the control's text.</summary>
        public System.Drawing.Font Font {
            get => this._font;

            set {
                VerifyAccess();

                this._font = value;
                InvalidateMeasure();
            }
        }

        /// <summary>The brush used to paint the control's foreground content.</summary>
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

        /// <summary>Draws the background and, when focused, the focus rectangle.</summary>
        public override void OnRender(DrawingContext dc) {
            if (this._background != null) {
                dc.DrawRectangle(this._background, null, 0, 0, this._renderWidth, this._renderHeight);
            }

            if (this.ShowFocusVisual && this.IsFocused && this._renderWidth > 2 && this._renderHeight > 2) {
                var pen = GetFocusPen();
                var t = pen.Thickness;
                if (t < this._renderWidth && t < this._renderHeight) {
                    dc.DrawRectangle(null, pen, t / 2, t / 2, this._renderWidth - t, this._renderHeight - t);
                }
            }
        }

        // Cached focus-ring pen, rebuilt only when Theme.FocusRing changes.
        // Previously a fresh Pen was allocated on every paint of every focused
        // control — measurable GC churn under animations.
        private static Pen s_focusPen;
        private static Color s_focusPenColor;
        private static Pen GetFocusPen() {
            var current = Theme.FocusRing;
            var cached = s_focusPen;
            if (cached == null || !ColorEquals(s_focusPenColor, current)) {
                cached = new Pen(current, 2);
                s_focusPen = cached;
                s_focusPenColor = current;
            }
            return cached;
        }

        private static bool ColorEquals(Color a, Color b) =>
            a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;

        /// <summary>Backing field for the control's background brush.</summary>
        protected internal Media.Brush _background = null;
        /// <summary>Backing field for the control's foreground brush.</summary>
        protected internal Media.Brush _foreground = new SolidColorBrush(Colors.Black);
        /// <summary>Backing field for the control's font.</summary>
        protected internal System.Drawing.Font _font;
    }
}


