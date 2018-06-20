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

        public void Translate(int dx, int dy) {
            VerifyAccess();

            this._x += dx;
            this._y += dy;
        }

        public void GetTranslation(out int x, out int y) {
            VerifyAccess();

            x = this._x;
            y = this._y;
        }

        public void Clear() {
            VerifyAccess();

            this._bitmap.Clear();
        }

        internal void Close() => this._bitmap = null;

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

        public void SetPixel(Color color, int x, int y) {
            VerifyAccess();

            this._bitmap.SetPixel(this._x + x, this._y + y, color);
        }

        public void DrawLine(Pen pen, int x0, int y0, int x1, int y1) {
            VerifyAccess();

            if (pen != null) {
                this._bitmap.DrawLine(pen.Color, pen.Thickness, this._x + x0, this._y + y0, this._x + x1, this._y + y1);
            }
        }

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

        public void DrawImage(ImageSource source, int x, int y) {
            VerifyAccess();

            this._bitmap.DrawImage(this._x + x, this._y + y, source, 0, 0, source.Width, source.Height);
        }

        public void DrawImage(ImageSource source, int destinationX, int destinationY, int sourceX, int sourceY, int sourceWidth, int sourceHeight) {
            VerifyAccess();

            this._bitmap.DrawImage(this._x + destinationX, this._y + destinationY, source, sourceX, sourceY, sourceWidth, sourceHeight);
        }

        public void BlendImage(ImageSource source, int destinationX, int destinationY, int sourceX, int sourceY, int sourceWidth, int sourceHeight, ushort opacity) {
            VerifyAccess();

            this._bitmap.DrawImage(this._x + destinationX, this._y + destinationY, source, sourceX, sourceY, sourceWidth, sourceHeight, opacity);
        }

        public void RotateImage(int angle, int destinationX, int destinationY, ImageSource bitmap, int sourceX, int sourceY, int sourceWidth, int sourceHeight, ushort opacity) {
            VerifyAccess();

            this._bitmap.RotateImage(angle, this._x + destinationX, this._y + destinationY, bitmap, sourceX, sourceY, sourceWidth, sourceHeight, opacity);
        }

        public void StretchImage(int xDst, int yDst, int widthDst, int heightDst, ImageSource bitmap, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity) {
            VerifyAccess();

            this._bitmap.StretchImage(this._x + xDst, this._y + yDst, widthDst, heightDst, bitmap, xSrc, ySrc, widthSrc, heightSrc, opacity);
        }

        public void TileImage(int xDst, int yDst, ImageSource bitmap, int width, int height, ushort opacity) {
            VerifyAccess();

            this._bitmap.TileImage(this._x + xDst, this._y + yDst, bitmap, width, height, opacity);
        }

        public void Scale9Image(int xDst, int yDst, int widthDst, int heightDst, ImageSource bitmap, int leftBorder, int topBorder, int rightBorder, int bottomBorder, ushort opacity) {
            VerifyAccess();

            this._bitmap.Scale9Image(this._x + xDst, this._y + yDst, widthDst, heightDst, bitmap, leftBorder, topBorder, rightBorder, bottomBorder, opacity);
        }

        public void DrawText(string text, System.Drawing.Font font, Color color, int x, int y) {
            VerifyAccess();

            this._bitmap.DrawText(text, font, color, this._x + x, this._y + y);
        }

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
                    throw new NotSupportedException();
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

        public void GetClippingRectangle(out int x, out int y, out int width, out int height) {
            if (this._clippingRectangles.Count == 0) {
                x = 0;
                y = 0;
                width = this._bitmap.Width - this._x;
                height = this._bitmap.Height - this._y;
            }
            else {
                var rect = (ClipRectangle)this._clippingRectangles.Peek();
                x = rect.X - this._x;
                y = rect.Y - this._y;
                width = rect.Width;
                height = rect.Height;
            }
        }

        public void PushClippingRectangle(int x, int y, int width, int height) {
            VerifyAccess();

            if (width < 0 || height < 0) {
                throw new ArgumentException();
            }

            var rect = new ClipRectangle(this._x + x, this._y + y, width, height);

            if (this._clippingRectangles.Count > 0) {
                // Intersect with the existing clip bounds
                var previousRect = (ClipRectangle)this._clippingRectangles.Peek();
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

            this._clippingRectangles.Push(rect);

            this._bitmap.SetClippingRectangle(rect.X, rect.Y, rect.Width, rect.Height);
            this.EmptyClipRect = (rect.Width <= 0 || rect.Height <= 0);
        }

        public void PopClippingRectangle() {
            VerifyAccess();

            var n = this._clippingRectangles.Count;

            if (n > 0) {
                this._clippingRectangles.Pop();

                ClipRectangle rect;

                if (n == 1) // in this case, at this point the stack is empty
                {
                    rect = new ClipRectangle(0, 0, this._bitmap.Width, this._bitmap.Height);
                }
                else {
                    rect = (ClipRectangle)this._clippingRectangles.Peek();
                }

                this._bitmap.SetClippingRectangle(rect.X, rect.Y, rect.Width, rect.Height);

                this.EmptyClipRect = (rect.Width == 0 && rect.Height == 0);
            }
        }

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

        public int Width => this._bitmap.Width;

        public int Height => this._bitmap.Height;

        private class ClipRectangle {
            public ClipRectangle(int x, int y, int width, int height) {
                this.X = x;
                this.Y = y;
                this.Width = width;
                this.Height = height;
            }

            public int X;
            public int Y;
            public int Width;
            public int Height;
        }

        internal bool EmptyClipRect = false;

        private Bitmap _bitmap;
        internal int _x;
        internal int _y;
        private Stack _clippingRectangles = new Stack();

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => this._bitmap = null;

    }
}


