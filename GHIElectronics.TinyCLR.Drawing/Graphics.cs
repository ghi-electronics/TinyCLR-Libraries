using System;
using System.Collections;
using System.Drawing;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Drawing;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Drawing {
    public interface IDrawTarget : IDisposable {
        int Width { get; }
        int Height { get; }

        void Clear(Color color);
        void Flush();

        Color GetPixel(int x, int y);
        void SetPixel(int x, int y, Color color);
        byte[] GetData();
    }

    public static class GraphicsManager {
        public static IntPtr RegisterDrawTarget(IDrawTarget target) => System.Drawing.Graphics.RegisterDrawTarget(target);
    }
}

namespace System.Drawing {
    internal interface IGraphics : IDisposable {
        int Width { get; }
        int Height { get; }

        void Clear();
        void Flush(IntPtr hdc);

        uint GetPixel(int x, int y);
        void SetPixel(int x, int y, uint color);
        byte[] GetBitmap();

        void DrawLine(uint color, int thickness, int x0, int y0, int x1, int y1);
        void DrawRectangle(uint colorOutline, int thicknessOutline, int x, int y, int width, int height, int xCornerRadius, int yCornerRadius, uint colorGradientStart, int xGradientStart, int yGradientStart, uint colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity);
        void DrawEllipse(uint colorOutline, int thicknessOutline, int x, int y, int xRadius, int yRadius, uint colorGradientStart, int xGradientStart, int yGradientStart, uint colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity);
        void DrawText(string text, Font font, uint color, int x, int y);
        void DrawTextInRect(string text, int x, int y, int width, int height, uint dtFlags, Color color, Font font);
        void StretchImage(int xDst, int yDst, int widthDst, int heightDst, IGraphics image, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity);
    }

    public sealed class Graphics : MarshalByRefObject, IDisposable {
        internal int Width => this.surface.Width;
        internal int Height => this.surface.Height;

        internal IGraphics surface;
        private bool disposed;
        internal bool callFromImage;
        private IntPtr hdc;

        public GraphicsUnit PageUnit { get; } = GraphicsUnit.Pixel;

        private static Hashtable drawTargets = new Hashtable();
        private static IntPtr nextHdc = IntPtr.Zero;

        internal static IntPtr RegisterDrawTarget(IDrawTarget target) {
            Graphics.nextHdc = IntPtr.Add(Graphics.nextHdc, 1);
            Graphics.drawTargets.Add(Graphics.nextHdc, target);

            return Graphics.nextHdc;
        }

        public uint GetPixel(int x, int y) => this.surface.GetPixel(x, y);
        public void SetPixel(int x, int y, uint color) => this.surface.SetPixel(x, y, color);
        public byte[] GetBitmap() => this.surface.GetBitmap();

        private static bool HasDrawing() {
            foreach (var i in Interop.FindAll())
                if (i.Name == "GHIElectronics.TinyCLR.Drawing")
                    return true;

            return false;
        }

        private static IGraphics CreateSurface(byte[] buffer) => CreateSurface(buffer, BitmapImageType.Bmp);

        private static IGraphics CreateSurface(byte[] buffer, BitmapImageType type) {
            if (!Graphics.HasDrawing())
                throw new NotSupportedException();

            return new Internal.Bitmap(buffer, type);
        }

        private static IGraphics CreateSurface(byte[] buffer, int offset, int count, BitmapImageType type) {
            if (!Graphics.HasDrawing())
                throw new NotSupportedException();

            return new Internal.Bitmap(buffer, offset, count, type);
        }

        private static IGraphics CreateSurface(int width, int height) {
            if (!Graphics.HasDrawing()) {

                throw new ArgumentException("Not supported");
            }

            return new Internal.Bitmap(width, height);
        }

        internal Graphics(byte[] buffer) : this(Graphics.CreateSurface(buffer), IntPtr.Zero) { }
        internal Graphics(byte[] buffer, BitmapImageType type) : this(Graphics.CreateSurface(buffer, type), IntPtr.Zero) { }
        internal Graphics(byte[] buffer,int offset, int count, BitmapImageType type) : this(Graphics.CreateSurface(buffer, offset, count, type), IntPtr.Zero) { }
        internal Graphics(int width, int height) : this(width, height, IntPtr.Zero) { }
        private Graphics(int width, int height, IntPtr hdc) : this(Graphics.CreateSurface(width, height), hdc) { }

        internal Graphics(IGraphics bmp, IntPtr hdc) {
            this.surface = bmp;
            this.hdc = hdc;
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (!this.disposed && !this.callFromImage) {
                this.surface?.Dispose();
                this.surface = null;

                this.disposed = true;
            }
        }

        private uint ToFlags(StringFormat format, float height, bool ignoreHeight, bool truncateAtBottom) {
            var flags = 0U;

            if (ignoreHeight || height == 0.0) flags |= Internal.Bitmap.DT_IgnoreHeight;
            if (truncateAtBottom) flags |= Internal.Bitmap.DT_TruncateAtBottom;

            if (format.FormatFlags != 0) throw new NotSupportedException();

            switch (format.Alignment) {
                case StringAlignment.Center: flags |= Internal.Bitmap.DT_AlignmentCenter; break;
                case StringAlignment.Far: flags |= Internal.Bitmap.DT_AlignmentRight; break;
                case StringAlignment.Near: flags |= Internal.Bitmap.DT_AlignmentLeft; break;
                default: throw new ArgumentException();
            }

            switch (format.Trimming) {
                case StringTrimming.EllipsisCharacter: flags |= Internal.Bitmap.DT_TrimmingCharacterEllipsis; break;
                case StringTrimming.EllipsisWord: flags |= Internal.Bitmap.DT_WordWrap | Internal.Bitmap.DT_TrimmingWordEllipsis; break;
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

        public SizeF MeasureString(string text, Font font) {
            font.ComputeExtent(text, out var width, out var height);

            return new SizeF(width, height);
        }

        public SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat) {
            font.ComputeTextInRect(text, out var width, out var height, 0, 0, (int)layoutArea.Width, (int)layoutArea.Height, this.ToFlags(stringFormat, layoutArea.Height, false, false));

            return new SizeF(width, height);
        }

        public void Clear() => this.surface.Clear();

        public static Graphics FromHdc(IntPtr hdc) {
            if (hdc == IntPtr.Zero) throw new ArgumentNullException(nameof(hdc));

            if (!Graphics.drawTargets.Contains(hdc)) {
                var res = Internal.Bitmap.GetSizeForLcdFromHdc(hdc, out var width, out var height);

                if (!res || width == 0 || height == 0) throw new InvalidOperationException("No screen configured.");

                return new Graphics(width, height, hdc);
            }
            else {
                throw new ArgumentException("Not supported");
            }
        }

        public static Graphics FromImage(Image image) {
            image.data.callFromImage = true;

            return image.data;
        }

        public delegate void OnFlushHandler(IntPtr hdc, byte[] data);

        static public event OnFlushHandler OnFlushEvent;

        public void Flush() {
            if (this.hdc != IntPtr.Zero) {
                this.surface.Flush(this.hdc);
            }

            OnFlushEvent?.Invoke(this.hdc, this.surface.GetBitmap());
        }

        //Draws a portion of an image at a specified location.
        public void DrawImage(Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit) => this.surface.StretchImage(x, y, srcRect.Width, srcRect.Height, image.data.surface, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, 0xFF);

        //Draws the specified Image at the specified location and with the specified size.
        public void DrawImage(Image image, int x, int y, int width, int height) => this.surface.StretchImage(x, y, width, height, image.data.surface, 0, 0, image.Width, image.Height, 0xFF);

        //Draws the specified image, using its original physical size, at the location specified by a coordinate pair.
        public void DrawImage(Image image, int x, int y) => this.surface.StretchImage(x, y, image.Width, image.Height, image.data.surface, 0, 0, image.Width, image.Height, 0xFF);

        //Draws the specified portion of the specified Image at the specified location and with the specified size.
        public void DrawImage(Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit) => this.surface.StretchImage(destRect.X, destRect.Y, destRect.Width, destRect.Height, image.data.surface, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, 0xFF);

        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2) {
            if (pen.Color.A != 0xFF) throw new NotSupportedException("Alpha not supported.");

            this.surface.DrawLine((uint)(pen.Color.value & 0x00FFFFFF), (int)pen.Width, x1, y1, x2, y2);
        }

        public void DrawString(string s, Font font, Brush brush, float x, float y) {
            if (brush is SolidBrush b) {
                if (b.Color.A != 0xFF) throw new NotSupportedException("Alpha not supported.");

                this.surface.DrawText(s, font, (uint)(b.Color.value & 0x00FFFFFF), (int)x, (int)y);
            }
            else {
                throw new NotSupportedException();
            }
        }

        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle) => this.DrawString(s, font, brush, layoutRectangle, new StringFormat {
            Trimming = StringTrimming.EllipsisWord,
            Alignment = StringAlignment.Near
        });

        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format) {
            if (brush is SolidBrush b) {
                if (b.Color.A != 0xFF) throw new NotSupportedException("Alpha not supported.");

                this.surface.DrawTextInRect(s, (int)layoutRectangle.X, (int)layoutRectangle.Y, (int)layoutRectangle.Width, (int)layoutRectangle.Height, this.ToFlags(format, layoutRectangle.Height, false, false), b.Color, font);
            }
            else {
                throw new NotSupportedException();
            }
        }

        public void DrawEllipse(Pen pen, int x, int y, int width, int height) {
            if (pen.Color.A != 0xFF) throw new NotSupportedException("Alpha not supported.");

            var rgb = (uint)(pen.Color.ToArgb() & 0x00FFFFFF);

            width = (width - 1) / 2;
            height = (height - 1) / 2;

            x += width;
            y += height;

            this.surface.DrawEllipse(rgb, (int)pen.Width, x, y, width, height, (uint)Color.Transparent.value, x, y, (uint)Color.Transparent.value, x + width * 2, y + height * 2, 0x00);
        }

        public void DrawRectangle(Pen pen, int x, int y, int width, int height) {
            if (pen.Color.A != 0xFF) throw new NotSupportedException("Alpha not supported.");

            var rgb = (uint)(pen.Color.ToArgb() & 0x00FFFFFF);

            this.surface.DrawRectangle(rgb, (int)pen.Width, x, y, width, height, 0, 0, (uint)Color.Transparent.value, x, y, (uint)Color.Transparent.value, x + width, y + height, 0x00);
        }

        public void FillEllipse(Brush brush, int x, int y, int width, int height) {
            if (brush is SolidBrush b) {
                var rgb = (uint)(b.Color.ToArgb() & 0x00FFFFFF);

                width = (width - 1) / 2;
                height = (height - 1) / 2;

                x += width;
                y += height;

                this.surface.DrawEllipse(rgb, 0, x, y, width, height, rgb, x, y, rgb, x + width * 2, y + height * 2, b.Color.A);
            }
            else {
                throw new NotSupportedException();
            }
        }

        public void FillRectangle(Brush brush, int x, int y, int width, int height) {
            if (brush is SolidBrush b) {
                var rgb = (uint)(b.Color.ToArgb() & 0x00FFFFFF);

                this.surface.DrawRectangle(rgb, 0, x, y, width, height, 0, 0, rgb, x, y, rgb, x + width, y + height, b.Color.A);
            }
            else {
                throw new NotSupportedException();
            }
        }
    }

    namespace Internal {
        //The name and namespace of this must match the definition in c_TypeIndexLookup in TypeSystem.cpp and ResourceManager.GetObject
        internal class Bitmap : MarshalByRefObject, IDisposable, IGraphics {
#pragma warning disable CS0169 // The field is never used
            IntPtr implPtr;
#pragma warning restore CS0169 // The field is never used

            public void Dispose() {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~Bitmap() => this.Dispose(false);

            public extern int Width { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int Height { [MethodImpl(MethodImplOptions.InternalCall)] get; }

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

            public const ushort OpacityOpaque = 0xFF;
            public const ushort OpacityTransparent = 0;

            public const int SRCCOPY = 0x00000001;
            public const int PATINVERT = 0x00000002;
            public const int DSTINVERT = 0x00000003;
            public const int BLACKNESS = 0x00000004;
            public const int WHITENESS = 0x00000005;
            public const int DSTGRAY = 0x00000006;
            public const int DSTLTGRAY = 0x00000007;
            public const int DSTDKGRAY = 0x00000008;
            public const int SINGLEPIXEL = 0x00000009;
            public const int RANDOM = 0x0000000a;

            //
            // These have to be kept in sync with the CLR_GFX_Bitmap::c_DrawText_ flags.
            //
            public const uint DT_None = 0x00000000;
            public const uint DT_WordWrap = 0x00000001;
            public const uint DT_TruncateAtBottom = 0x00000004;
            [Obsolete("Use DT_TrimmingWordEllipsis or DT_TrimmingCharacterEllipsis to specify the type of trimming needed.", false)]
            public const uint DT_Ellipsis = 0x00000008;
            public const uint DT_IgnoreHeight = 0x00000010;
            public const uint DT_AlignmentLeft = 0x00000000;
            public const uint DT_AlignmentCenter = 0x00000002;
            public const uint DT_AlignmentRight = 0x00000020;
            public const uint DT_AlignmentMask = 0x00000022;

            public const uint DT_TrimmingNone = 0x00000000;
            public const uint DT_TrimmingWordEllipsis = 0x00000008;
            public const uint DT_TrimmingCharacterEllipsis = 0x00000040;
            public const uint DT_TrimmingMask = 0x00000048;

            public void DrawTextInRect(string text, int x, int y, int width, int height, uint dtFlags, Color color, Font font) {
                var xRelStart = 0;
                var yRelStart = 0;

                this.DrawTextInRect(ref text, ref xRelStart, ref yRelStart, x, y, width, height, dtFlags, (uint)(color.value & 0x00FFFFFF), font);
            }

            //public void DrawEllipse(Color colorOutline, int x, int y, int xRadius, int yRadius) => DrawEllipse(colorOutline, 1, x, y, xRadius, yRadius, Color.Black, 0, 0, Color.Black, 0, 0, OpacityOpaque);
            //
            //public void DrawImage(int xDst, int yDst, Graphics bitmap, int xSrc, int ySrc, int width, int height) => DrawImage(xDst, yDst, bitmap, xSrc, ySrc, width, height, OpacityOpaque);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Flush(int x, int y, int width, int height);

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
            public extern void StretchImage(int xDst, int yDst, int widthDst, int heightDst, Bitmap bitmap, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity);

            public void StretchImage(int xDst, int yDst, int widthDst, int heightDst, IGraphics bitmap, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity) {
                if (bitmap is Bitmap b)
                    this.StretchImage(xDst, yDst, widthDst, heightDst, b, xSrc, ySrc, widthSrc, heightSrc, opacity);
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void TileImage(int xDst, int yDst, Bitmap bitmap, int width, int height, ushort opacity);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Scale9Image(int xDst, int yDst, int widthDst, int heightDst, Bitmap bitmap, int leftBorder, int topBorder, int rightBorder, int bottomBorder, ushort opacity);
        }
    }
}
