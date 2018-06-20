////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class Text : UIElement {
        public Text()
            : this(null, null) {
        }

        public Text(string content)
            : this(null, content) {
        }

        public Text(System.Drawing.Font font, string content) {
            this._text = content;
            this._font = font;
            this._foreColor = Colors.Black;
        }

        public System.Drawing.Font Font {
            get => this._font;

            set {
                VerifyAccess();

                this._font = value;
                InvalidateMeasure();
            }
        }

        public Color ForeColor {
            get => this._foreColor;

            set {
                VerifyAccess();

                this._foreColor = value;
                Invalidate();
            }
        }

        public string TextContent {
            get => this._text;

            set {
                VerifyAccess();

                if (this._text != value) {
                    this._text = value;
                    InvalidateMeasure();
                }
            }
        }

        public TextTrimming Trimming {
            get => this._trimming;

            set {
                VerifyAccess();

                this._trimming = value;
                Invalidate();
            }
        }

        public TextAlignment TextAlignment {
            get => this._alignment;

            set {
                VerifyAccess();

                this._alignment = value;
                Invalidate();
            }
        }

        public int LineHeight => (this._font != null) ? (this._font.Height + this._font.ExternalLeading) : 0;

        public bool TextWrap {
            get => this._textWrap;

            set {
                VerifyAccess();

                this._textWrap = value;
                InvalidateMeasure();
            }
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            if (this._font != null && this._text != null && this._text.Length > 0) {
                var flags = Bitmap.DT_IgnoreHeight | Bitmap.DT_WordWrap;

                switch (this._alignment) {
                    case TextAlignment.Left:
                        flags |= Bitmap.DT_AlignmentLeft;
                        break;
                    case TextAlignment.Right:
                        flags |= Bitmap.DT_AlignmentRight;
                        break;
                    case TextAlignment.Center:
                        flags |= Bitmap.DT_AlignmentCenter;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                switch (this._trimming) {
                    case TextTrimming.CharacterEllipsis:
                        flags |= Bitmap.DT_TrimmingCharacterEllipsis;
                        break;
                    case TextTrimming.WordEllipsis:
                        flags |= Bitmap.DT_TrimmingWordEllipsis;
                        break;
                }

                this._font.ComputeTextInRect(this._text, out desiredWidth, out desiredHeight, 0, 0, availableWidth, 0, flags);

                if (this._textWrap == false) desiredHeight = this._font.Height;
            }
            else {
                desiredWidth = 0;
                desiredHeight = 0;

                if (this._font != null)
                    desiredHeight = this._font.Height;
            }
        }

        public override void OnRender(DrawingContext dc) {
            if (this._font != null && this._text != null) {
                var height = this._textWrap ? this._renderHeight : this._font.Height;

                var txt = this._text;
                dc.DrawText(ref txt, this._font, this._foreColor, 0, 0, this._renderWidth, height, this._alignment, this._trimming);
            }
        }

#if TINYCLR_TRACE
        public override string ToString()
        {
            return base.ToString() + " [" + this.TextContent + "]";
        }

#endif

        protected System.Drawing.Font _font;
        private Color _foreColor;
        protected string _text;
        private bool _textWrap;
        private TextTrimming _trimming = TextTrimming.WordEllipsis;
        private TextAlignment _alignment = TextAlignment.Left;
    }
}


