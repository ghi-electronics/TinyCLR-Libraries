////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Threading;

namespace GHIElectronics.TinyCLR.UI.Media {
    /// <summary>
    /// Drawing Context.
    /// </summary>
    public class DrawingContext : DispatcherObject, IDisposable {
        internal DrawingContext(Bitmap bmp) => this._bitmap = bmp;

        /// <summary>Offsets the origin of subsequent drawing operations.</summary>
        public void Translate(int dx, int dy) {
            VerifyAccess();

            this._x += dx;
            this._y += dy;
        }

        /// <summary>Gets the current drawing origin offset.</summary>
        public void GetTranslation(out int x, out int y) {
            VerifyAccess();

            x = this._x;
            y = this._y;
        }

        /// <summary>Clears the drawing surface.</summary>
        public void Clear() {
            VerifyAccess();

            this._bitmap.Clear();
        }

        internal void Close() => this._bitmap = null;

        /// <summary>Draws a polygon defined by the given points.</summary>
        public void DrawPolygon(Brush brush, Pen pen, int[] pts) {
            VerifyAccess();

            brush.RenderPolygon(this._bitmap, pen, pts);

            var nPts = pts.Length / 2;

            for (var i = 0; i < nPts - 1; i++) {
                DrawLine(pen, pts[i * 2], pts[i * 2 + 1], pts[i * 2 + 2], pts[i * 2 + 3]);
            }

            if (nPts > 2) {
                DrawLine(pen, pts[nPts * 2 - 2], pts[nPts * 2 - 1], pts[0], pts[1]);
            }
        }

        /// <summary>Sets a single pixel to the given color.</summary>
        public void SetPixel(Color color, int x, int y) {
            VerifyAccess();

            this._bitmap.SetPixel(this._x + x, this._y + y, color);
        }

        /// <summary>Draws a line between two points.</summary>
        public void DrawLine(Pen pen, int x0, int y0, int x1, int y1) {
            VerifyAccess();

            if (pen != null) {
                this._bitmap.DrawLine(pen.Color, pen.Thickness, this._x + x0, this._y + y0, this._x + x1, this._y + y1);
            }
        }

        /// <summary>Draws an ellipse with the given fill and outline.</summary>
        public void DrawEllipse(Brush brush, Pen pen, int x, int y, int xRadius, int yRadius) {
            VerifyAccess();

            // Fill
            //
            if (brush != null) {
                brush.RenderEllipse(this._bitmap, pen, this._x + x, this._y + y, xRadius, yRadius);
            }

            // Pen
            else if (pen != null && pen.Thickness > 0) {
                this._bitmap.DrawEllipse(pen.Color, pen.Thickness, this._x + x, this._y + y, xRadius, yRadius,
                    Colors.Transparent, 0, 0, Colors.Transparent, 0, 0, 0);
            }

        }

        /// <summary>Draws an image at the given position.</summary>
        public void DrawImage(ImageSource source, int x, int y) {
            VerifyAccess();

            this._bitmap.DrawImage(this._x + x, this._y + y, source, 0, 0, source.Width, source.Height);
        }

        /// <summary>Draws a region of an image at the given position.</summary>
        public void DrawImage(ImageSource source, int destinationX, int destinationY, int sourceX, int sourceY, int sourceWidth, int sourceHeight) {
            VerifyAccess();

            this._bitmap.DrawImage(this._x + destinationX, this._y + destinationY, source, sourceX, sourceY, sourceWidth, sourceHeight);
        }

        /// <summary>Draws a region of an image blended with the given opacity.</summary>
        public void BlendImage(ImageSource source, int destinationX, int destinationY, int sourceX, int sourceY, int sourceWidth, int sourceHeight, ushort opacity) {
            VerifyAccess();

            this._bitmap.DrawImage(this._x + destinationX, this._y + destinationY, source, sourceX, sourceY, sourceWidth, sourceHeight, opacity);
        }

        /// <summary>Draws a region of an image rotated by the given angle.</summary>
        public void RotateImage(int angle, int destinationX, int destinationY, ImageSource bitmap, int sourceX, int sourceY, int sourceWidth, int sourceHeight, ushort opacity) {
            VerifyAccess();

            this._bitmap.RotateImage(angle, this._x + destinationX, this._y + destinationY, bitmap, sourceX, sourceY, sourceWidth, sourceHeight, opacity);
        }

        /// <summary>Draws a region of an image stretched to the given size.</summary>
        public void StretchImage(int xDst, int yDst, int widthDst, int heightDst, ImageSource bitmap, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity) {
            VerifyAccess();

            this._bitmap.StretchImage(this._x + xDst, this._y + yDst, widthDst, heightDst, bitmap, xSrc, ySrc, widthSrc, heightSrc, opacity);
        }

        /// <summary>Draws an image tiled across the given area.</summary>
        public void TileImage(int xDst, int yDst, ImageSource bitmap, int width, int height, ushort opacity) {
            VerifyAccess();

            this._bitmap.TileImage(this._x + xDst, this._y + yDst, bitmap, width, height, opacity);
        }

        /// <summary>Draws an image using nine-slice scaling with the given borders.</summary>
        public void Scale9Image(int xDst, int yDst, int widthDst, int heightDst, ImageSource bitmap, int leftBorder, int topBorder, int rightBorder, int bottomBorder, ushort opacity) {
            VerifyAccess();

            this._bitmap.Scale9Image(this._x + xDst, this._y + yDst, widthDst, heightDst, bitmap, leftBorder, topBorder, rightBorder, bottomBorder, opacity);
        }

        /// <summary>Draws text at the given position.</summary>
        public void DrawText(string text, System.Drawing.Font font, Color color, int x, int y) {
            VerifyAccess();

            this._bitmap.DrawText(text, font, color, this._x + x, this._y + y);
        }

        /// <summary>Draws text within a rectangle using the given alignment and trimming.</summary>
        public bool DrawText(ref string text, System.Drawing.Font font, Color color, int x, int y, int width, int height,
                             TextAlignment alignment, TextTrimming trimming) {
            VerifyAccess();

            var flags = Bitmap.DT_WordWrap;

            // Text alignment
            switch (alignment) {
                case TextAlignment.Left:
                    //flags |= Bitmap.DT_AlignmentLeft;
                    break;
                case TextAlignment.Center:
                    flags |= Bitmap.DT_AlignmentCenter;
                    break;
                case TextAlignment.Right:
                    flags |= Bitmap.DT_AlignmentRight;
                    break;
                default:
                    throw new NotSupportedException("TextAlignment value " + alignment + " is not supported.");
            }

            // Trimming
            switch (trimming) {
                case TextTrimming.CharacterEllipsis:
                    flags |= Bitmap.DT_TrimmingCharacterEllipsis;
                    break;
                case TextTrimming.WordEllipsis:
                    flags |= Bitmap.DT_TrimmingWordEllipsis;
                    break;
            }

            var xRelStart = 0;
            var yRelStart = 0;
            return this._bitmap.DrawTextInRect(ref text, ref xRelStart, ref yRelStart, this._x + x, this._y + y,
                                           width, height, flags, color, font);
        }

        /// <summary>Gets the current clipping rectangle.</summary>
        public void GetClippingRectangle(out int x, out int y, out int width, out int height) {
            if (this._clipDepth == 0) {
                x = 0;
                y = 0;
                width = this._bitmap.Width - this._x;
                height = this._bitmap.Height - this._y;
            }
            else {
                var rect = this._clippingRectangles[this._clipDepth - 1];
                x = rect.X - this._x;
                y = rect.Y - this._y;
                width = rect.Width;
                height = rect.Height;
            }
        }

        /// <summary>Pushes a clipping rectangle onto the clip stack.</summary>
        public void PushClippingRectangle(int x, int y, int width, int height) {
            VerifyAccess();

            if (width < 0 || height < 0) {
                throw new ArgumentException();
            }

            ClipRectangle rect;
            rect.X = this._x + x;
            rect.Y = this._y + y;
            rect.Width = width;
            rect.Height = height;

            if (this._clipDepth > 0) {
                // Intersect with the existing clip bounds
                var previousRect = this._clippingRectangles[this._clipDepth - 1];
                //need to evaluate performance differences of inlining Min & Max.
                var x1 = System.Math.Max(rect.X, previousRect.X);
                var x2 = System.Math.Min(rect.X + rect.Width, previousRect.X + previousRect.Width);
                var y1 = System.Math.Max(rect.Y, previousRect.Y);
                var y2 = System.Math.Min(rect.Y + rect.Height, previousRect.Y + previousRect.Height);

                rect.X = x1;
                rect.Y = y1;
                rect.Width = x2 - x1;
                rect.Height = y2 - y1;
            }

            if (this._clipDepth == this._clippingRectangles.Length) {
                // Grow on demand. Depth follows tree depth, not screen size.
                var grown = new ClipRectangle[this._clippingRectangles.Length * 2];
                Array.Copy(this._clippingRectangles, grown, this._clippingRectangles.Length);
                this._clippingRectangles = grown;
            }

            this._clippingRectangles[this._clipDepth++] = rect;

            ApplyNativeClip(rect.X, rect.Y, rect.Width, rect.Height);
            this.EmptyClipRect = (rect.Width <= 0 || rect.Height <= 0);
        }

        /// <summary>Pops the most recently pushed clipping rectangle.</summary>
        public void PopClippingRectangle() {
            VerifyAccess();

            var n = this._clipDepth;

            if (n > 0) {
                this._clipDepth--;

                ClipRectangle rect;

                if (n == 1) // in this case, at this point the stack is empty
                {
                    rect.X = 0;
                    rect.Y = 0;
                    rect.Width = this._bitmap.Width;
                    rect.Height = this._bitmap.Height;
                }
                else {
                    rect = this._clippingRectangles[this._clipDepth - 1];
                }

                ApplyNativeClip(rect.X, rect.Y, rect.Width, rect.Height);

                this.EmptyClipRect = (rect.Width == 0 && rect.Height == 0);
            }
        }

        // O3: only push the clip to the native surface when it actually changes,
        // collapsing redundant SetClippingRectangle interop calls.
        private void ApplyNativeClip(int x, int y, int width, int height) {
            if (this._hasLastClip && x == this._lastClipX && y == this._lastClipY && width == this._lastClipW && height == this._lastClipH) {
                return;
            }

            this._bitmap.SetClippingRectangle(x, y, width, height);

            this._lastClipX = x;
            this._lastClipY = y;
            this._lastClipW = width;
            this._lastClipH = height;
            this._hasLastClip = true;
        }

        // O1 helper: test (in the current translated coordinate space, the same
        // space GetClippingRectangle reports) whether a rectangle can produce any
        // visible pixel under the current clip. Lets RenderRecursive skip whole
        // off-screen subtrees. Resolution-independent: pure runtime bounds math.
        internal bool IntersectsClip(int x, int y, int width, int height) {
            if (width <= 0 || height <= 0) {
                return false;
            }

            GetClippingRectangle(out var cx, out var cy, out var cw, out var ch);

            if (cw <= 0 || ch <= 0) {
                return false;
            }

            if (x >= cx + cw || x + width <= cx) {
                return false;
            }

            if (y >= cy + ch || y + height <= cy) {
                return false;
            }

            return true;
        }

        // Reuse one DrawingContext across frames (see MediaContext) instead of
        // allocating a context + clip stack every frame. Clears per-frame state.
        internal void Reset() {
            this._x = 0;
            this._y = 0;
            this._clipDepth = 0;
            this.EmptyClipRect = false;
            this._hasLastClip = false; // don't assume the surface clip survived between frames
        }

        /// <summary>Draws a rectangle with the given fill and outline.</summary>
        public void DrawRectangle(Brush brush, Pen pen, int x, int y, int width, int height) {
            VerifyAccess();

            // Fill
            //
            if (brush != null) {
                brush.RenderRectangle(this._bitmap, pen, this._x + x, this._y + y, width, height);
            }

            // Pen
            else if (pen != null && pen.Thickness > 0) {
                this._bitmap.DrawRectangle(pen.Color, pen.Thickness, this._x + x, this._y + y, width, height, 0, 0,
                                      Colors.Transparent, 0, 0, Colors.Transparent, 0, 0, 0);
            }
        }

        /// <summary>The width of the drawing surface in pixels.</summary>
        public int Width => this._bitmap.Width;

        /// <summary>The height of the drawing surface in pixels.</summary>
        public int Height => this._bitmap.Height;

        // Value type so the clip stack (an array) holds no heap references and
        // pushing costs no allocation.
        private struct ClipRectangle {
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }

        internal bool EmptyClipRect = false;

        private Bitmap _bitmap;
        internal int _x;
        internal int _y;

        // Array-backed clip stack of value types. Avoids the per-push heap
        // allocation (and boxing) of the old Stack + ClipRectangle-class, which
        // showed up as GC stutter during animation/scrolling. _clipDepth is the
        // live count; the array grows on demand (depth ~ tree depth, not screen).
        private ClipRectangle[] _clippingRectangles = new ClipRectangle[16];
        private int _clipDepth;

        // O3: last rectangle actually pushed to the native surface.
        private int _lastClipX, _lastClipY, _lastClipW, _lastClipH;
        private bool _hasLastClip;

        /// <summary>Releases the resources used by the drawing context.</summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the resources used by the drawing context.</summary>
        protected virtual void Dispose(bool disposing) => this._bitmap = null;

    }
}


