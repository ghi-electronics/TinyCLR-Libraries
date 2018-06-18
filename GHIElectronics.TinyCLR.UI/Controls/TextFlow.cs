////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Diagnostics;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class TextFlow : UIElement {
        public TextRunCollection TextRuns;

        internal class TextLine {
            public const int DefaultLineHeight = 10;

            public TextRun[] Runs;
            public int Baseline;
            public int Height;
            private int _width;

            public TextLine(ArrayList runs, int height, int baseline) {
                this.Runs = (TextRun[])runs.ToArray(typeof(TextRun));
                this.Baseline = baseline;
                this.Height = height;
            }

            // Empty line with specified height
            public TextLine(int height) {
                this.Runs = new TextRun[0];
                this.Height = height;
            }

            public int Width {
                get {
                    if (this._width == 0) {
                        var lineWidth = 0;
                        for (var i = this.Runs.Length - 1; i >= 0; i--) {
                            this.Runs[i].GetSize(out var width, out var height);
                            lineWidth += width;
                        }

                        this._width = lineWidth;
                    }

                    return this._width;
                }
            }
        }

        internal ArrayList _lineCache;
        internal TextAlignment _alignment = TextAlignment.Left;
        internal int _currentLine;

        internal ScrollingStyle _scrollingStyle = ScrollingStyle.LineByLine;

        public TextFlow() => this.TextRuns = new TextRunCollection(this);

        public ScrollingStyle ScrollingStyle {
            get => this._scrollingStyle;

            set {
                VerifyAccess();

                if (value < ScrollingStyle.First || value > ScrollingStyle.Last) {
                    throw new ArgumentOutOfRangeException("ScrollingStyle", "Invalid Enum");
                }

                this._scrollingStyle = value;
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

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            desiredWidth = availableWidth;
            desiredHeight = availableHeight;

            if (availableWidth > 0) {
                this._lineCache = SplitLines(availableWidth);

                // Compute total desired height
                //
                var totalHeight = 0;
                for (var lineNumber = this._lineCache.Count - 1; lineNumber >= 0; --lineNumber) {
                    totalHeight += ((TextLine)this._lineCache[lineNumber]).Height;
                }

                desiredHeight = totalHeight;
            }
        }

        internal bool LineScroll(bool up) {
            if (this._lineCache == null) return false;

            if (up && this._currentLine > 0) {
                this._currentLine--;
                Invalidate();
                return true;
            }
            else if (!up && this._currentLine < this._lineCache.Count - 1) {
                this._currentLine++;
                Invalidate();
                return true;
            }

            return false;
        }

        internal bool PageScroll(bool up) {
            if (this._lineCache == null) return false;

            var lineNumber = this._currentLine;
            var nLines = this._lineCache.Count;
            var pageHeight = this._renderHeight;
            var heightOfLines = 0;

            if (up) {
                // Determine first line of previous page
                //
                while (lineNumber > 0) {
                    lineNumber--;
                    var line = (TextLine)this._lineCache[lineNumber];
                    heightOfLines += line.Height;
                    if (heightOfLines > pageHeight) {
                        lineNumber++;
                        break;
                    }
                }
            }
            else {
                // Determine first line of next page
                //
                while (lineNumber < nLines) {
                    var line = (TextLine)this._lineCache[lineNumber];
                    heightOfLines += line.Height;
                    if (heightOfLines > pageHeight) {
                        break;
                    }

                    lineNumber++;
                }

                if (lineNumber == nLines) lineNumber = nLines - 1;
            }

            if (this._currentLine != lineNumber) {
                this._currentLine = lineNumber;
                Invalidate();
                return true;
            }
            else {
                return false;
            }
        }

        // Given an available width, takes the TextRuns and arranges them into
        // separate lines, breaking where possible at whitespace.
        //
        internal ArrayList SplitLines(int availableWidth) {
            Debug.Assert(availableWidth > 0);

            var lineWidth = 0;

            var remainingRuns = new ArrayList();
            for (var i = 0; i < this.TextRuns.Count; i++) {
                remainingRuns.Add(this.TextRuns[i]);
            }

            var lineCache = new ArrayList();
            var runsOnCurrentLine = new ArrayList();

            while (remainingRuns.Count > 0) {
                var newLine = false;

                var run = (TextRun)remainingRuns[0];
                remainingRuns.RemoveAt(0);

                if (run.IsEndOfLine) {
                    newLine = true;
                }
                else {
                    run.GetSize(out var runWidth, out var runHeight);
                    lineWidth += runWidth;
                    runsOnCurrentLine.Add(run);

                    // If the line length now extends beyond the available width, attempt to break the line
                    //
                    if (lineWidth > availableWidth) {
                        var onlyRunOnCurrentLine = (runsOnCurrentLine.Count == 1);

                        if (run.Text.Length > 1) {
                            runsOnCurrentLine.Remove(run);
                            if (run.Break(runWidth - (lineWidth - availableWidth), out var run1, out var run2, onlyRunOnCurrentLine)) {
                                // Break and put overflow on next line
                                //
                                if (run1 != null) {
                                    runsOnCurrentLine.Add(run1);
                                }

                                if (run2 != null) {
                                    remainingRuns.Insert(0, run2);
                                }
                            }
                            else if (!onlyRunOnCurrentLine) {
                                // No break found - put it on its own line
                                //
                                remainingRuns.Insert(0, run);
                            }
                        }
                        else // run.Text.Length == 1
                        {
                            if (!onlyRunOnCurrentLine) {
                                runsOnCurrentLine.Remove(run);
                                remainingRuns.Insert(0, run);
                            }
                        }

                        newLine = true;
                    }

                    if (lineWidth >= availableWidth || remainingRuns.Count == 0) {
                        newLine = true;
                    }
                }

                // If we're done with this line, add it to the list
                //
                if (newLine) {
                    var lineHeight = 0;
                    var baseLine = 0;
                    var nRuns = runsOnCurrentLine.Count;
                    if (nRuns > 0) {
                        // Compute line height & baseline
                        for (var i = 0; i < nRuns; i++) {
                            var font = ((TextRun)runsOnCurrentLine[i]).Font;
                            var h = font.Height + font.ExternalLeading;
                            if (h > lineHeight) {
                                lineHeight = h;
                                baseLine = font.Ascent;
                            }
                        }

                        // Add line to cache
                        lineCache.Add(new TextLine(runsOnCurrentLine, lineHeight, baseLine));
                    }
                    else {
                        // Empty line. Just borrow the height from the previous line, if any
                        lineHeight = (lineCache.Count) > 0 ?
                            ((TextLine)lineCache[lineCache.Count - 1]).Height :
                            TextLine.DefaultLineHeight;
                        lineCache.Add(new TextLine(lineHeight));
                    }

                    // Move onto next line
                    //
                    runsOnCurrentLine.Clear();
                    lineWidth = 0;
                }
            }

            return lineCache;
        }

        protected override void OnButtonDown(GHIElectronics.TinyCLR.UI.Input.ButtonEventArgs e) {
            if (e.Button == HardwareButton.Up || e.Button == HardwareButton.Down) {
                var isUp = (e.Button == HardwareButton.Up);
                switch (this._scrollingStyle) {
                    case ScrollingStyle.PageByPage:
                        e.Handled = PageScroll(isUp);
                        break;
                    case ScrollingStyle.LineByLine:
                        e.Handled = LineScroll(isUp);
                        break;
                    default:
                        Debug.Assert(false, "Unknown ScrollingStyle");
                        break;
                }
            }
        }

        public override void OnRender(Media.DrawingContext dc) {
            if (this._lineCache == null || this._lineCache.Count == 0) {
                return;
            }

            var nLines = this._lineCache.Count;
            var top = 0;
            GetRenderSize(out var width, out var height);

            // Draw each line of Text
            //
            var lineNumber = this._currentLine;
            while (lineNumber < nLines) {
                var line = (TextLine)this._lineCache[lineNumber];
                if (top + line.Height > height) {
                    break;
                }

                var runs = line.Runs;

                int x;
                switch (this._alignment) {
                    case TextAlignment.Left:
                        x = 0;
                        break;

                    case TextAlignment.Center:
                        x = (width - line.Width) >> 1; // >> 1 is the same as div by 2
                        break;

                    case TextAlignment.Right:
                        x = width - line.Width;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                for (var i = 0; i < runs.Length; i++) {
                    var run = runs[i];
                    run.GetSize(out var w, out var h);
                    var y = top + line.Baseline - run.Font.Ascent;
                    dc.DrawText(run.Text, run.Font, run.ForeColor, x, y);
                    x += w;
                }

                top += line.Height;
                lineNumber++;
            }
        }

        public int TopLine {
            get => this._currentLine;

            set {
                VerifyAccess();

                var temp = this._lineCache[value]; // Easy way to make sure _lineCache is valid and value is within range

                this._currentLine = value;
                Invalidate();
            }
        }

        public int LineCount => this._lineCache.Count; // if _lineCache is null, it'll throw a NullReferenceException
    }
}


