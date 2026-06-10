using System;
using System.Collections;
using System.Drawing;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Native;

namespace System.Drawing
{
    internal interface IGraphics : IDisposable
    {
        int Width { get; }
        int Height { get; }

        void Clear();
        void Flush(IntPtr hdc);
        void Flush(IntPtr hdc, int x, int y, int width, int height);

        uint GetPixel(int x, int y);
        void SetPixel(int x, int y, uint color);
        byte[] GetBitmap();
        byte[] GetBitmap(int x, int y, int width, int height);

        void DrawLine(uint color, int thickness, int x0, int y0, int x1, int y1);
        void DrawRectangle(uint colorOutline, int thicknessOutline, int x, int y, int width, int height, int xCornerRadius, int yCornerRadius, uint colorGradientStart, int xGradientStart, int yGradientStart, uint colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity);
        void DrawEllipse(uint colorOutline, int thicknessOutline, int x, int y, int xRadius, int yRadius, uint colorGradientStart, int xGradientStart, int yGradientStart, uint colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity);
        void DrawText(string text, Font font, uint color, int x, int y);
        void DrawTextInRect(string text, int x, int y, int width, int height, uint dtFlags, Color color, Font font);
        void StretchImage(int xDst, int yDst, int widthDst, int heightDst, IGraphics image, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity);
        void DrawImage(int xDst, int yDst, IGraphics image, int xSrc, int ySrc, int width, int height, ushort opacity);
        void SetClippingRectangle(int x, int y, int width, int height);
        bool DrawTextInRect(ref string text, ref int xRelStart, ref int yRelStart, int x, int y, int width, int height, uint dtFlags, uint color, Font font);
        void RotateImage(int angle, int xDst, int yDst, IGraphics image, int xSrc, int ySrc, int width, int height, ushort opacity);
        void MakeTransparent(uint color);
        void TileImage(int xDst, int yDst, IGraphics image, int width, int height, ushort opacity);
        void Scale9Image(int xDst, int yDst, int widthDst, int heightDst, IGraphics image, int leftBorder, int topBorder, int rightBorder, int bottomBorder, ushort opacity);
    }

    /// <summary>
    /// 2-D drawing surface backed by a <see cref="Bitmap"/>. Issue line, rectangle,
    /// ellipse, text, and image draws against this surface, then push the result to
    /// the display via <see cref="Flush(IntPtr)"/>.
    /// </summary>
    public class Graphics : MarshalByRefObject, IDisposable
    {
        /// <summary>Gets the width of the drawing surface in pixels.</summary>
        public int Width => this.surface.Width;
        /// <summary>Gets the height of the drawing surface in pixels.</summary>
        public int Height => this.surface.Height;

        internal IGraphics surface;
        private bool disposed;
        internal bool callFromImage;
        private IntPtr hdc;

        /// <summary>Gets the unit of measure for this surface, which is always pixels.</summary>
        public GraphicsUnit PageUnit { get; } = GraphicsUnit.Pixel;

        /// <summary>Gets the ARGB color of the pixel at the given coordinates.</summary>
        public uint GetPixel(int x, int y) => this.surface.GetPixel(x, y);
        /// <summary>Sets the color of the pixel at the given coordinates.</summary>
        public void SetPixel(int x, int y, Color color) => this.surface.SetPixel(x, y, (uint)color.ToArgb());
        /// <summary>Gets the raw pixel data of the drawing surface.</summary>
        public byte[] GetBitmap() => this.surface.GetBitmap();
        /// <summary>Gets the raw pixel data for a rectangular region of the drawing surface.</summary>
        public byte[] GetBitmap(int x, int y, int width, int height) => this.surface.GetBitmap(x, y, width, height);

        private static IGraphics CreateSurface(byte[] buffer) => CreateSurface(buffer, BitmapImageType.Bmp);
        private static IGraphics CreateSurface(byte[] buffer, int width, int height)
        {
            if (buffer == null)
                throw new ArgumentNullException();

            return new Internal.Bitmap(buffer, width, height);
        }

        private static IGraphics CreateSurface(byte[] buffer, BitmapImageType type)
        {
            if (buffer == null)
                throw new ArgumentNullException();

            return new Internal.Bitmap(buffer, type);
        }

        private static IGraphics CreateSurface(byte[] buffer, int offset, int count, BitmapImageType type)
        {
            if (buffer == null)
                throw new ArgumentNullException();

            return new Internal.Bitmap(buffer, offset, count, type);
        }

        private static IGraphics CreateSurface(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new IndexOutOfRangeException();

            return new Internal.Bitmap(width, height);
        }

        internal Graphics(byte[] buffer) : this(Graphics.CreateSurface(buffer), IntPtr.Zero) { }
        internal Graphics(byte[] buffer, BitmapImageType type) : this(Graphics.CreateSurface(buffer, type), IntPtr.Zero) { }
        internal Graphics(byte[] buffer, int offset, int count, BitmapImageType type) : this(Graphics.CreateSurface(buffer, offset, count, type), IntPtr.Zero) { }
        internal Graphics(int width, int height) : this(width, height, IntPtr.Zero) { }
        internal Graphics(int width, int height, IntPtr hdc) : this(Graphics.CreateSurface(width, height), hdc) { }
        internal Graphics(byte[] buffer, int width, int height) : this(buffer, width, height, IntPtr.Zero) { }
        internal Graphics(byte[] buffer, int width, int height, IntPtr hdc) : this(Graphics.CreateSurface(buffer, width, height), hdc) { }

        internal Graphics(IGraphics bmp, IntPtr hdc)
        {
            this.surface = bmp;
            this.hdc = hdc;
        }

        /// <summary>Releases the resources used by this drawing surface.</summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed && !this.callFromImage)
            {
                this.surface?.Dispose();
                this.surface = null;

                this.disposed = true;
            }
        }

        private uint ToFlags(StringFormat format, float height, bool ignoreHeight, bool truncateAtBottom)
        {
            var flags = 0U;

            if (ignoreHeight || height == 0.0) flags |= (uint)System.Drawing.Graphics.DrawTextAlignment.IgnoreHeight;
            if (truncateAtBottom) flags |= (uint)System.Drawing.Graphics.DrawTextAlignment.TruncateAtBottom;

            if (format.FormatFlags != 0) throw new NotSupportedException();

            switch (format.Alignment)
            {
                case StringAlignment.Center: flags |= (uint)System.Drawing.Graphics.DrawTextAlignment.AlignmentCenter; break;
                case StringAlignment.Far: flags |= (uint)System.Drawing.Graphics.DrawTextAlignment.AlignmentRight; break;
                case StringAlignment.Near: flags |= (uint)System.Drawing.Graphics.DrawTextAlignment.AlignmentLeft; break;
                default: throw new ArgumentException();
            }

            switch (format.Trimming)
            {
                case StringTrimming.EllipsisCharacter: flags |= (uint)System.Drawing.Graphics.DrawTextAlignment.TrimmingCharacterEllipsis; break;
                case StringTrimming.EllipsisWord: flags |= (uint)System.Drawing.Graphics.DrawTextAlignment.WordWrap | (uint)System.Drawing.Graphics.DrawTextAlignment.TrimmingWordEllipsis; break;
                case StringTrimming.None:
                    break;

                case StringTrimming.EllipsisPath:
                case StringTrimming.Character:
                case StringTrimming.Word:
                    throw new NotSupportedException();

                default:
                    throw new ArgumentException();
            }

            return flags;
        }

        ~Graphics() => this.Dispose(false);

        /// <summary>Measures the pixel size needed to render the given text in the given font.</summary>
        public SizeF MeasureString(string text, Font font)
        {
            font.ComputeExtent(text, out var width, out var height);

            return new SizeF(width, height);
        }

        /// <summary>Measures the pixel size needed to render the given text within a layout area using the given format.</summary>
        public SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat)
        {
            font.ComputeTextInRect(text, out var width, out var height, 0, 0, (int)layoutArea.Width, (int)layoutArea.Height, this.ToFlags(stringFormat, layoutArea.Height, false, false));

            return new SizeF(width, height);
        }

        /// <summary>Clears the entire drawing surface to its default color.</summary>
        public void Clear() => this.surface.Clear();

        /// <summary>Creates a drawing surface for the display referenced by the given device context handle.</summary>
        public static Graphics FromHdc(IntPtr hdc)
        {
            if (hdc == IntPtr.Zero) throw new ArgumentNullException(nameof(hdc));

            var res = Internal.Bitmap.GetSizeForLcdFromHdc(hdc, out var width, out var height);

            if (!res || width == 0 || height == 0) throw new InvalidOperationException("No screen configured.");

            return new Graphics(width, height, hdc);
        }

        /// <summary>Gets the drawing surface backing the given image.</summary>
        public static Graphics FromImage(Image image)
        {
            image.data.callFromImage = true;

            return image.data;
        }

        /// <summary>Represents the method that handles the flush event for a drawing surface.</summary>
        public delegate void OnFlushHandler(Graphics sender, byte[] data, int x, int y, int width, int height, int originalWidth);

        /// <summary>Occurs when the drawing surface is flushed.</summary>
        static public event OnFlushHandler OnFlushEvent;

        /// <summary>Pushes the contents of the drawing surface to the display.</summary>
        public void Flush()
        {
            if (this.hdc != IntPtr.Zero)
            {
                this.surface.Flush(this.hdc, 0, 0, this.surface.Width, this.surface.Height);
            }

            OnFlushEvent?.Invoke(this, this.surface.GetBitmap(), 0, 0, this.surface.Width, this.surface.Height, this.surface.Width);
        }

        /// <summary>Draws a portion of an image at the specified location.</summary>
        //Draws a portion of an image at a specified location.
        public void DrawImage(Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit) => this.surface.StretchImage(x, y, srcRect.Width, srcRect.Height, image.data.surface, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, 0xFF);

        /// <summary>Draws an image at the specified location, scaled to the given size.</summary>
        //Draws the specified Image at the specified location and with the specified size.
        public void DrawImage(Image image, int x, int y, int width, int height) => this.surface.StretchImage(x, y, width, height, image.data.surface, 0, 0, image.Width, image.Height, 0xFF);

        /// <summary>Draws an image at the specified location using its original size.</summary>
        //Draws the specified image, using its original physical size, at the location specified by a coordinate pair.
        public void DrawImage(Image image, int x, int y) => this.surface.StretchImage(x, y, image.Width, image.Height, image.data.surface, 0, 0, image.Width, image.Height, 0xFF);

        /// <summary>Draws a portion of an image into the destination rectangle, scaling as needed.</summary>
        //Draws the specified portion of the specified Image at the specified location and with the specified size.
        public void DrawImage(Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit) => this.surface.StretchImage(destRect.X, destRect.Y, destRect.Width, destRect.Height, image.data.surface, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, 0xFF);

        /// <summary>Draws a line between two points using the given pen.</summary>
        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            if (pen.Color.A != 0xFF) throw new NotSupportedException("Alpha not supported.");

            this.surface.DrawLine((uint)(pen.Color.value & 0x00FFFFFF), (int)pen.Width, x1, y1, x2, y2);
        }

        /// <summary>Draws text at the given location using the specified font and brush.</summary>
        public void DrawString(string s, Font font, Brush brush, float x, float y)
        {
            if (brush is SolidBrush b)
            {
                if (b.Color.A != 0xFF) throw new NotSupportedException("Alpha not supported.");

                this.surface.DrawText(s, font, (uint)(b.Color.value & 0x00FFFFFF), (int)x, (int)y);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>Draws text wrapped within the given layout rectangle.</summary>
        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle) => this.DrawString(s, font, brush, layoutRectangle, new StringFormat
        {
            Trimming = StringTrimming.EllipsisWord,
            Alignment = StringAlignment.Near
        });

        /// <summary>Draws text within the given layout rectangle using the specified format.</summary>
        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
        {
            if (brush is SolidBrush b)
            {
                if (b.Color.A != 0xFF) throw new NotSupportedException("Alpha not supported.");

                this.surface.DrawTextInRect(s, (int)layoutRectangle.X, (int)layoutRectangle.Y, (int)layoutRectangle.Width, (int)layoutRectangle.Height, this.ToFlags(format, layoutRectangle.Height, false, false), b.Color, font);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>Draws the outline of an ellipse bounded by the given rectangle using the specified pen.</summary>
        public void DrawEllipse(Pen pen, int x, int y, int width, int height)
        {
            if (pen.Color.A != 0xFF) throw new NotSupportedException("Alpha not supported.");

            var rgb = (uint)(pen.Color.ToArgb() & 0x00FFFFFF);

            width = (width - 1) / 2;
            height = (height - 1) / 2;

            x += width;
            y += height;

            this.surface.DrawEllipse(rgb, (int)pen.Width, x, y, width, height, (uint)Color.Transparent.value, x, y, (uint)Color.Transparent.value, x + width * 2, y + height * 2, 0x00);
        }

        /// <summary>Draws the outline of a rectangle using the specified pen.</summary>
        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            if (pen.Color.A != 0xFF) throw new NotSupportedException("Alpha not supported.");

            var rgb = (uint)(pen.Color.ToArgb() & 0x00FFFFFF);

            this.surface.DrawRectangle(rgb, (int)pen.Width, x, y, width, height, 0, 0, (uint)Color.Transparent.value, x, y, (uint)Color.Transparent.value, x + width, y + height, 0x00);
        }

        /// <summary>Fills the interior of an ellipse bounded by the given rectangle using the specified brush.</summary>
        public void FillEllipse(Brush brush, int x, int y, int width, int height)
        {
            if (brush is SolidBrush b)
            {
                var rgb = (uint)(b.Color.ToArgb() & 0x00FFFFFF);

                width = (width - 1) / 2;
                height = (height - 1) / 2;

                x += width;
                y += height;

                this.surface.DrawEllipse(rgb, 0, x, y, width, height, rgb, x, y, rgb, x + width * 2, y + height * 2, b.Color.A);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>Fills the interior of a rectangle using the specified brush.</summary>
        public void FillRectangle(Brush brush, int x, int y, int width, int height)
        {
            if (brush is SolidBrush b)
            {
                var rgb = (uint)(b.Color.ToArgb() & 0x00FFFFFF);

                this.surface.DrawRectangle(rgb, 0, x, y, width, height, 0, 0, rgb, x, y, rgb, x + width, y + height, b.Color.A);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>Draws a region of an image at the destination location with the given opacity.</summary>
        public void DrawImage(int xDst, int yDst, Image image, int xSrc, int ySrc, int width, int height, ushort opacity) => this.surface.DrawImage(xDst, yDst, image.data.surface, xSrc, ySrc, width, height, opacity);

        /// <summary>Pushes a rectangular region of the drawing surface to the display.</summary>
        public void Flush(int x, int y, int width, int height)
        {
            if (this.hdc != IntPtr.Zero)
            {
                this.surface.Flush(this.hdc, x, y, width, height);
            }

           // Note:
           // Proper way is this.surface.GetBitmap(x,y, width, height) but it will create a new buffer.
           // Keep same buffer for now.
           OnFlushEvent?.Invoke(this, this.surface.GetBitmap(), x, y, width, height, this.surface.Width);
        }

        /// <summary>Restricts drawing to the given rectangular region.</summary>
        public void SetClippingRectangle(int x, int y, int width, int height) => this.surface.SetClippingRectangle(x, y, width, height);
        /// <summary>Draws text within the given rectangle using the specified alignment flags.</summary>
        public void DrawTextInRect(string text, int x, int y, int width, int height, DrawTextAlignment dtFlags, Color color, Font font) => this.surface.DrawTextInRect(text, x, y, width, height, (uint)dtFlags, color, font);
        /// <summary>Draws text within the given rectangle, updating the relative start position, and returns whether more text remains.</summary>
        public bool DrawTextInRect(ref string text, ref int xRelStart, ref int yRelStart, int x, int y, int width, int height, DrawTextAlignment dtFlags, Color color, Font font) => this.surface.DrawTextInRect(ref text, ref xRelStart, ref yRelStart, x, y, width, height, (uint)dtFlags, (uint)color.ToArgb(), font);
        /// <summary>Draws a region of an image rotated by the given angle at the destination location.</summary>
        public void RotateImage(int angle, int xDst, int yDst, Image image, int xSrc, int ySrc, int width, int height, ushort opacity)
        {
            if (image == null) throw new ArgumentNullException("image null.");

            if ((xSrc + width > image.Width) || (ySrc + height > image.Height))
                throw new ArgumentOutOfRangeException();

            this.surface.RotateImage(angle, xDst, yDst, image.data.surface, xSrc, ySrc, width, height, opacity);
        }
        /// <summary>Makes the given color transparent on the drawing surface.</summary>
        public void MakeTransparent(Color color) => this.surface.MakeTransparent((uint)color.ToArgb());
        /// <summary>Draws a region of an image scaled to fill the destination rectangle.</summary>
        public void StretchImage(int xDst, int yDst, int widthDst, int heightDst, Image image, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity) => this.surface.StretchImage(xDst, yDst, widthDst, heightDst, image.data.surface, xSrc, ySrc, widthSrc, heightSrc, opacity);
        /// <summary>Tiles an image to fill the given area at the destination location.</summary>
        public void TileImage(int xDst, int yDst, Image image, int width, int height, ushort opacity) => this.surface.TileImage(xDst, yDst, image.data.surface, width, height, opacity);
        /// <summary>Draws an image using nine-slice scaling so its borders are not stretched.</summary>
        public void Scale9Image(int xDst, int yDst, int widthDst, int heightDst, Image image, int leftBorder, int topBorder, int rightBorder, int bottomBorder, ushort opacity) => this.surface.Scale9Image(xDst, yDst, widthDst, heightDst, image.data.surface, leftBorder, topBorder, rightBorder, bottomBorder, opacity);

        /// <summary>Bit flags that control how text is aligned, wrapped, and trimmed within a rectangle.</summary>
        //
        // These have to be kept in sync with the CLR_GFX_Bitmap::c_DrawText_ flags.
        //
        public enum DrawTextAlignment : uint
        {
            /// <summary>No special alignment behavior.</summary>
            None = 0x00000000,
            /// <summary>Text wraps to the next line when it does not fit.</summary>
            WordWrap = 0x00000001,
            /// <summary>Text that overflows the bottom of the rectangle is truncated.</summary>
            TruncateAtBottom = 0x00000004,
            /// <summary>Overflowing text is replaced with an ellipsis.</summary>
            Ellipsis = 0x00000008,
            /// <summary>The available height is ignored during layout.</summary>
            IgnoreHeight = 0x00000010,
            /// <summary>Text is aligned to the left edge.</summary>
            AlignmentLeft = 0x00000000,
            /// <summary>Text is centered horizontally.</summary>
            AlignmentCenter = 0x00000002,
            /// <summary>Text is aligned to the right edge.</summary>
            AlignmentRight = 0x00000020,
            /// <summary>Bit mask for the horizontal alignment flags.</summary>
            AlignmentMask = 0x00000022,
            /// <summary>Text is not trimmed.</summary>
            TrimmingNone = 0x00000000,
            /// <summary>Text is trimmed at a word boundary with an ellipsis.</summary>
            TrimmingWordEllipsis = 0x00000008,
            /// <summary>Text is trimmed at a character boundary with an ellipsis.</summary>
            TrimmingCharacterEllipsis = 0x00000040,
            /// <summary>Bit mask for the trimming flags.</summary>
            TrimmingMask = 0x00000048,
        }

    }

    namespace Internal
    {
        //The name and namespace of this must match the definition in c_TypeIndexLookup in TypeSystem.cpp and ResourceManager.GetObject
        internal class Bitmap : MarshalByRefObject, IDisposable, IGraphics
        {
#pragma warning disable CS0169 // The field is never used
            IntPtr implPtr;
#pragma warning restore CS0169 // The field is never used

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~Bitmap() => this.Dispose(false);

            public extern int Width { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int Height { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public byte[] GetBitmap(int x, int y, int width, int height)
            {
                if ((x < 0) || (y < 0) || (x + width > this.Width) || (y + height > this.Height))
                    throw new ArgumentOutOfRangeException();

                return this.NativeGetBitmap(x, y, width, height, this.Width);
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public static extern bool GetSizeForLcdFromHdc(IntPtr hdc, out int width, out int height);

            [MethodImplAttribute(MethodImplOptions.InternalCall)]
            private extern void CreateInstantFromResources(uint buffer, uint size, uint assembly);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern Bitmap(byte[] imageData, BitmapImageType type);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern Bitmap(byte[] imageData, int offset, int count, BitmapImageType type);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern Bitmap(int width, int height);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern Bitmap(byte[] data, int width, int height);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Clear();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Dispose(bool disposing);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Flush(IntPtr hdc);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DrawText(string text, Font font, uint color, int x, int y);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DrawImage(int xDst, int yDst, Bitmap bitmap, int xSrc, int ySrc, int width, int height, ushort opacity);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DrawEllipse(uint colorOutline, int thicknessOutline, int x, int y, int xRadius, int yRadius, uint colorGradientStart, int xGradientStart, int yGradientStart, uint colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DrawLine(uint color, int thickness, int x0, int y0, int x1, int y1);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void DrawRectangle(uint colorOutline, int thicknessOutline, int x, int y, int width, int height, int xCornerRadius, int yCornerRadius, uint colorGradientStart, int xGradientStart, int yGradientStart, uint colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity);

            public void DrawTextInRect(string text, int x, int y, int width, int height, uint dtFlags, Color color, Font font)
            {
                var xRelStart = 0;
                var yRelStart = 0;

                this.DrawTextInRect(ref text, ref xRelStart, ref yRelStart, x, y, width, height, dtFlags, (uint)(color.value & 0x00FFFFFF), font);
            }

            //public void DrawEllipse(Color colorOutline, int x, int y, int xRadius, int yRadius) => DrawEllipse(colorOutline, 1, x, y, xRadius, yRadius, Color.Black, 0, 0, Color.Black, 0, 0, OpacityOpaque);
            //
            //public void DrawImage(int xDst, int yDst, Graphics bitmap, int xSrc, int ySrc, int width, int height) => DrawImage(xDst, yDst, bitmap, xSrc, ySrc, width, height, OpacityOpaque);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Flush(IntPtr hdc, int x, int y, int width, int height);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetClippingRectangle(int x, int y, int width, int height);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool DrawTextInRect(ref string text, ref int xRelStart, ref int yRelStart, int x, int y, int width, int height, uint dtFlags, uint color, Font font);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void RotateImage(int angle, int xDst, int yDst, Bitmap bitmap, int xSrc, int ySrc, int width, int height, ushort opacity);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void MakeTransparent(uint color);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void StretchImage(int xDst, int yDst, Bitmap bitmap, int width, int height, ushort opacity);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetPixel(int xPos, int yPos, uint color);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern uint GetPixel(int xPos, int yPos);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern byte[] GetBitmap();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern byte[] NativeGetBitmap(int x, int y, int width, int height, int originalWidth);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void StretchImage(int xDst, int yDst, int widthDst, int heightDst, Bitmap bitmap, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity);

            public void StretchImage(int xDst, int yDst, int widthDst, int heightDst, IGraphics bitmap, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity)
            {
                if (bitmap is Bitmap b)
                    this.StretchImage(xDst, yDst, widthDst, heightDst, b, xSrc, ySrc, widthSrc, heightSrc, opacity);
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void TileImage(int xDst, int yDst, Bitmap bitmap, int width, int height, ushort opacity);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Scale9Image(int xDst, int yDst, int widthDst, int heightDst, Bitmap bitmap, int leftBorder, int topBorder, int rightBorder, int bottomBorder, ushort opacity);

            public void DrawImage(int xDst, int yDst, IGraphics bitmap, int xSrc, int ySrc, int width, int height, ushort opacity)
            {
                if (bitmap is Bitmap b)
                    this.DrawImage(xDst, yDst, b, xSrc, ySrc, width, height, opacity);
            }

            public void RotateImage(int angle, int xDst, int yDst, IGraphics bitmap, int xSrc, int ySrc, int width, int height, ushort opacity)
            {
                if (bitmap is Bitmap b)
                    this.RotateImage(angle, xDst, yDst, b, xSrc, ySrc, width, height, opacity);
            }

            public void TileImage(int xDst, int yDst, IGraphics bitmap, int width, int height, ushort opacity)
            {
                if (bitmap is Bitmap b)
                    this.TileImage(xDst, yDst, b, width, height, opacity);
            }

            public void Scale9Image(int xDst, int yDst, int widthDst, int heightDst, IGraphics bitmap, int leftBorder, int topBorder, int rightBorder, int bottomBorder, ushort opacity)
            {
                if (bitmap is Bitmap b)
                    this.Scale9Image(xDst, yDst, widthDst, heightDst, b, leftBorder, topBorder, rightBorder, bottomBorder, opacity);
            }
        }
    }
}
