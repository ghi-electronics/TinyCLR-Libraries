////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class TextRun {
        public readonly string Text;
        public readonly System.Drawing.Font Font;
        public readonly Color ForeColor;

        internal bool IsEndOfLine;

        protected int _width;
        protected int _height;

        private TextRun() {
        }

        public TextRun(string text, System.Drawing.Font font, Color foreColor) {
            if (text == null || text.Length == 0) {
                throw new ArgumentNullException("Text must be non-null and non-empty");
            }

            this.Text = text;
            this.Font = font ?? throw new ArgumentNullException("font must be non-null");
            this.ForeColor = foreColor;
        }

        public static TextRun EndOfLine {
            get {
                var eol = new TextRun {
                    IsEndOfLine = true
                };
                return eol;
            }
        }

        private int EmergencyBreak(int width) {
            var index = this.Text.Length;
            int w;
            do {
                this.Font.ComputeExtent(this.Text.Substring(0, --index), out w, out var h);
            }

            while (w >= width && index > 1);

            return index;
        }

        internal bool Break(int availableWidth, out TextRun run1, out TextRun run2, bool emergencyBreak) {
            Debug.Assert(availableWidth > 0);
            Debug.Assert(availableWidth < this._width);
            Debug.Assert(this.Text.Length > 1);

            var leftBreak = -1;
            var rightBreak = -1;

            // Try to find a candidate position for breaking
            //
            var foundBreak = false;
            while (!foundBreak) {
                // Try adding a word
                //
                var indexOfNextSpace = this.Text.IndexOf(' ', leftBreak + 1);

                foundBreak = (indexOfNextSpace == -1);

                if (!foundBreak) {
                    this.Font.ComputeExtent(this.Text.Substring(0, indexOfNextSpace), out var w, out var h);
                    foundBreak = (w >= availableWidth);
                    if (w == availableWidth) {
                        leftBreak = indexOfNextSpace;
                    }
                }

                if (foundBreak) {
                    if (leftBreak >= 0) {
                        rightBreak = leftBreak + 1;
                    }
                    else if (emergencyBreak) {
                        leftBreak = EmergencyBreak(availableWidth);
                        rightBreak = leftBreak;
                    }
                    else {
                        run1 = run2 = null;
                        return false;
                    }
                }
                else {
                    leftBreak = indexOfNextSpace;
                }
            }

            var first = this.Text.Substring(0, leftBreak).TrimEnd(' ');

            // Split the text run
            //
            run1 = null;
            if (first.Length > 0) {
                run1 = new TextRun(first, this.Font, this.ForeColor);
            }

            run2 = null;
            if (rightBreak < this.Text.Length) {
                var run2String = this.Text.Substring(rightBreak).TrimStart(' ');

                // if run2 is all spaces (length == 0 after trim), we'll leave run2 as null
                if (run2String.Length > 0) {
                    run2 = new TextRun(run2String, this.Font, this.ForeColor);
                }
            }

            return true;
        }

        public void GetSize(out int width, out int height) {
            if (this._width == 0) {
                this.Font.ComputeExtent(this.Text, out this._width, out this._height);
            }

            width = this._width;
            height = this._height;
        }
    }
}


