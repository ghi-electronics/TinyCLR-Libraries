////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A lightweight element that draws a single string in a given font.</summary>
    public class Text : UIElement {
        /// <summary>Initializes a new empty text element.</summary>
        public Text()
            : this(null, null) {
        }

        /// <summary>Initializes a new text element with the given content and no font.</summary>
        public Text(string content)
            : this(null, content) {
        }

        /// <summary>Initializes a new text element with the given font and content.</summary>
        public Text(System.Drawing.Font font, string content) {
            this._text = content;
            this._font = font;
            this._foreColor = Colors.Black;
        }

        /// <summary>The font used to draw the text.</summary>
        public System.Drawing.Font Font {
            get => this._font;

            set {
                VerifyAccess();

                this._font = value;
                InvalidateMeasure();
            }
        }

        /// <summary>The color used to draw the text.</summary>
        public Color ForeColor {
            get => this._foreColor;

            set {
                VerifyAccess();

                this._foreColor = value;
                Invalidate();
            }
        }

        /// <summary>The string of text to display.</summary>
        public string TextContent {
            get => this._text;

            set {
                VerifyAccess();

                if (this._text != value) {
                    this._text = value;

                    // A fixed-width, single-line Text (e.g. a clock updated every second) keeps the same footprint
                    // when its content changes, so just repaint it. InvalidateMeasure() would propagate up to the
                    // Window and trigger a full layout+repaint pass that visibly stalls other animations (a moving
                    // gauge, etc.). Only re-measure when the element's size can actually change.
                    if (this.IsWidthSet(out _) && !this._textWrap) {
                        Invalidate();
                    }
                    else {
                        InvalidateMeasure();
                    }
                }
            }
        }

        /// <summary>How text that does not fit is trimmed (for example, with an ellipsis).</summary>
        public TextTrimming Trimming {
            get => this._trimming;

            set {
                VerifyAccess();

                this._trimming = value;
                Invalidate();
            }
        }

        /// <summary>The horizontal alignment of the text.</summary>
        public TextAlignment TextAlignment {
            get => this._alignment;

            set {
                VerifyAccess();

                this._alignment = value;
                Invalidate();
            }
        }

        /// <summary>The height of a single line of text, including external leading.</summary>
        public int LineHeight => (this._font != null) ? (this._font.Height + this._font.ExternalLeading) : 0;

        /// <summary>Whether text wraps onto multiple lines when it exceeds the available width.</summary>
        public bool TextWrap {
            get => this._textWrap;

            set {
                VerifyAccess();

                this._textWrap = value;
                InvalidateMeasure();
            }
        }

        /// <summary>Measures the size needed to draw the text in the available width.</summary>
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
                        throw new NotSupportedException("TextAlignment value " + this._alignment + " is not supported.");
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

        /// <summary>Draws the text using the current font, color, alignment, and trimming.</summary>
        public override void OnRender(DrawingContext dc) {
            if (this._font != null && this._text != null) {
                var height = this._textWrap ? this._renderHeight : this._font.Height;

                var txt = this._text;
                dc.DrawText(ref txt, this._font, this._foreColor, 0, 0, this._renderWidth, height, this._alignment, this._trimming);
            }
        }

#if TINYCLR_TRACE
        /// <summary>Returns a string representation of the element including its text content.</summary>
        public override string ToString()
        {
            return base.ToString() + " [" + this.TextContent + "]";
        }

#endif

        /// <summary>The font used to render the text.</summary>
        protected System.Drawing.Font _font;
        private Color _foreColor;
        /// <summary>The text content to render.</summary>
        protected string _text;
        private bool _textWrap;
        private TextTrimming _trimming = TextTrimming.WordEllipsis;
        private TextAlignment _alignment = TextAlignment.Left;
    }
}


