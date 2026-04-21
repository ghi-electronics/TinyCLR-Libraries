using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public enum GridUnitType {
        Auto,
        Pixel,
        Star,
    }

    public struct GridLength {
        public GridUnitType Unit;
        /// <summary>Pixel size, or star weight when <see cref="Unit"/> is <see cref="GridUnitType.Star"/>.</summary>
        public int Value;

        public static GridLength Auto() => new GridLength { Unit = GridUnitType.Auto, Value = 0 };

        public static GridLength Pixel(int pixels) => new GridLength { Unit = GridUnitType.Pixel, Value = pixels };

        public static GridLength Star(int weight = 1) => new GridLength { Unit = GridUnitType.Star, Value = weight < 1 ? 1 : weight };
    }

    /// <summary>
    /// Row/column layout with pixel, auto, and star sizing (WPF-style subset).
    /// Use <see cref="GetRow"/> / <see cref="SetRow"/> and <see cref="GetColumn"/> / <see cref="SetColumn"/> on children.
    /// </summary>
    public class Grid : Panel {
        private const int AutoUnset = -1;

        private static readonly Hashtable RowStore = new Hashtable();
        private static readonly Hashtable ColStore = new Hashtable();

        public readonly ArrayList RowDefinitions = new ArrayList();
        public readonly ArrayList ColumnDefinitions = new ArrayList();

        public static int GetRow(UIElement e) => RowStore.Contains(e) ? (int)RowStore[e] : 0;

        public static void SetRow(UIElement e, int row) {
            if (row < 0) {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            RowStore[e] = row;
            InvalidateParentGrid(e);
        }

        public static int GetColumn(UIElement e) => ColStore.Contains(e) ? (int)ColStore[e] : 0;

        public static void SetColumn(UIElement e, int column) {
            if (column < 0) {
                throw new ArgumentOutOfRangeException(nameof(column));
            }

            ColStore[e] = column;
            InvalidateParentGrid(e);
        }

        private static void InvalidateParentGrid(UIElement e) {
            for (var p = e.Parent; p != null; p = p.Parent) {
                if (p is Grid g) {
                    g.InvalidateMeasure();
                    return;
                }
            }
        }

        private static GridLength DefCol(int index, ArrayList defs) {
            if (defs == null || index < 0 || index >= defs.Count) {
                return GridLength.Auto();
            }

            return (GridLength)defs[index];
        }

        private static GridLength DefRow(int index, ArrayList defs) {
            if (defs == null || index < 0 || index >= defs.Count) {
                return GridLength.Auto();
            }

            return (GridLength)defs[index];
        }

        protected override void MeasureOverride(int availableWidth, int availableHeight, out int desiredWidth, out int desiredHeight) {
            var n = this.Children.Count;
            var maxCol = 0;
            var maxRow = 0;
            for (var i = 0; i < n; i++) {
                var c = this.Children[i];
                if (c.Visibility == Visibility.Collapsed) {
                    continue;
                }

                maxCol = System.Math.Max(maxCol, GetColumn(c));
                maxRow = System.Math.Max(maxRow, GetRow(c));
            }

            var colCount = System.Math.Max(maxCol + 1, System.Math.Max(1, this.ColumnDefinitions.Count));
            var rowCount = System.Math.Max(maxRow + 1, System.Math.Max(1, this.RowDefinitions.Count));

            var colWidths = new int[colCount];
            var colStar = new int[colCount];
            var colUnit = new GridUnitType[colCount];
            var rowHeights = new int[rowCount];
            var rowStar = new int[rowCount];
            var rowUnit = new GridUnitType[rowCount];

            for (var c = 0; c < colCount; c++) {
                var gl = DefCol(c, this.ColumnDefinitions);
                colUnit[c] = gl.Unit;
                if (gl.Unit == GridUnitType.Pixel) {
                    colWidths[c] = gl.Value;
                }
                else if (gl.Unit == GridUnitType.Star) {
                    colWidths[c] = 0;
                    colStar[c] = gl.Value;
                }
                else {
                    colWidths[c] = AutoUnset;
                }
            }

            for (var r = 0; r < rowCount; r++) {
                var gl = DefRow(r, this.RowDefinitions);
                rowUnit[r] = gl.Unit;
                if (gl.Unit == GridUnitType.Pixel) {
                    rowHeights[r] = gl.Value;
                }
                else if (gl.Unit == GridUnitType.Star) {
                    rowHeights[r] = 0;
                    rowStar[r] = gl.Value;
                }
                else {
                    rowHeights[r] = AutoUnset;
                }
            }

            for (var i = 0; i < n; i++) {
                var child = this.Children[i];
                if (child.Visibility == Visibility.Collapsed) {
                    continue;
                }

                var cc = GetColumn(child);
                var rr = GetRow(child);
                child.Measure(Media.Constants.MaxExtent, Media.Constants.MaxExtent);
                child.GetDesiredSize(out var cw, out var ch);
                if (cc >= 0 && cc < colCount && colUnit[cc] == GridUnitType.Auto) {
                    if (colWidths[cc] == AutoUnset) {
                        colWidths[cc] = cw;
                    }
                    else {
                        colWidths[cc] = System.Math.Max(colWidths[cc], cw);
                    }
                }

                if (rr >= 0 && rr < rowCount && rowUnit[rr] == GridUnitType.Auto) {
                    if (rowHeights[rr] == AutoUnset) {
                        rowHeights[rr] = ch;
                    }
                    else {
                        rowHeights[rr] = System.Math.Max(rowHeights[rr], ch);
                    }
                }
            }

            for (var c = 0; c < colCount; c++) {
                if (colUnit[c] == GridUnitType.Auto && colWidths[c] == AutoUnset) {
                    colWidths[c] = 0;
                }
            }

            for (var r = 0; r < rowCount; r++) {
                if (rowUnit[r] == GridUnitType.Auto && rowHeights[r] == AutoUnset) {
                    rowHeights[r] = 0;
                }
            }

            var fixedCol = 0;
            var starColWeight = 0;
            for (var c = 0; c < colCount; c++) {
                if (colUnit[c] == GridUnitType.Star && colStar[c] > 0) {
                    starColWeight += colStar[c];
                }
                else {
                    fixedCol += colWidths[c];
                }
            }

            var availW = availableWidth > 0 && availableWidth < Media.Constants.MaxExtent ? availableWidth : Media.Constants.MaxExtent;
            var remainingCol = availW - fixedCol;
            if (remainingCol < 0) {
                remainingCol = 0;
            }

            if (starColWeight > 0) {
                for (var c = 0; c < colCount; c++) {
                    if (colUnit[c] == GridUnitType.Star && colStar[c] > 0) {
                        colWidths[c] = remainingCol * colStar[c] / starColWeight;
                    }
                }
            }

            var fixedRow = 0;
            var starRowWeight = 0;
            for (var r = 0; r < rowCount; r++) {
                if (rowUnit[r] == GridUnitType.Star && rowStar[r] > 0) {
                    starRowWeight += rowStar[r];
                }
                else {
                    fixedRow += rowHeights[r];
                }
            }

            var availH = availableHeight > 0 && availableHeight < Media.Constants.MaxExtent ? availableHeight : Media.Constants.MaxExtent;
            var remainingRow = availH - fixedRow;
            if (remainingRow < 0) {
                remainingRow = 0;
            }

            if (starRowWeight > 0) {
                for (var r = 0; r < rowCount; r++) {
                    if (rowUnit[r] == GridUnitType.Star && rowStar[r] > 0) {
                        rowHeights[r] = remainingRow * rowStar[r] / starRowWeight;
                    }
                }
            }

            for (var i = 0; i < n; i++) {
                var child = this.Children[i];
                if (child.Visibility == Visibility.Collapsed) {
                    continue;
                }

                var cc = GetColumn(child);
                var rr = GetRow(child);
                if (cc < 0 || cc >= colCount || rr < 0 || rr >= rowCount) {
                    continue;
                }

                child.Measure(colWidths[cc], rowHeights[rr]);
            }

            desiredWidth = 0;
            for (var c = 0; c < colCount; c++) {
                desiredWidth += colWidths[c];
            }

            desiredHeight = 0;
            for (var r = 0; r < rowCount; r++) {
                desiredHeight += rowHeights[r];
            }

            this._colWidths = colWidths;
            this._rowHeights = rowHeights;
        }

        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            if (this._colWidths == null || this._rowHeights == null) {
                return;
            }

            var colOffsets = new int[this._colWidths.Length];
            for (var c = 1; c < this._colWidths.Length; c++) {
                colOffsets[c] = colOffsets[c - 1] + this._colWidths[c - 1];
            }

            var rowOffsets = new int[this._rowHeights.Length];
            for (var r = 1; r < this._rowHeights.Length; r++) {
                rowOffsets[r] = rowOffsets[r - 1] + this._rowHeights[r - 1];
            }

            var n = this.Children.Count;
            for (var i = 0; i < n; i++) {
                var child = this.Children[i];
                if (child.Visibility == Visibility.Collapsed) {
                    continue;
                }

                var cc = GetColumn(child);
                var rr = GetRow(child);
                if (cc < 0 || cc >= this._colWidths.Length || rr < 0 || rr >= this._rowHeights.Length) {
                    continue;
                }

                var w = this._colWidths[cc];
                var h = this._rowHeights[rr];
                child.Arrange(colOffsets[cc], rowOffsets[rr], w, h);
            }
        }

        private int[] _colWidths;
        private int[] _rowHeights;
    }
}
