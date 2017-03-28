using GHIElectronics.TinyCLR.Graphics.Display;
using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Drawing {
    public sealed class Graphics : MarshalByRefObject, IDisposable {
        private static bool screenCreated;
        private static object screenLock = new object();

        internal int Width => this.surface.Width;
        internal int Height => this.surface.Height;

        private Internal.Bitmap surface;
        private bool forScreen;
        private bool disposed;

        internal Graphics(byte[] buffer) {
            this.surface = new Internal.Bitmap(buffer, Internal.Bitmap.BitmapImageType.Bmp);
            this.forScreen = false;
        }

        internal Graphics(int width, int height) : this(width, height, false) { }

        private Graphics(int width, int height, bool forScreen) {
            this.surface = new Internal.Bitmap(width, height);
            this.forScreen = forScreen;
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (!this.disposed) {
                this.surface?.Dispose();
                this.surface = null;

                this.disposed = true;
            }
        }

        ~Graphics() => this.Dispose(false);

        public void Clear(Color color) {
            if (color != Color.Black) throw new NotSupportedException();

            this.surface.Clear();
        }

        public static Graphics FromHdc(IntPtr hdc) {
            var config = DisplayConfiguration.GetForCurrentDisplay();

            if (config == null || config.Width == 0 || config.Height == 0) throw new InvalidOperationException("No screen configured.");
            if (hdc != config.Hdc) throw new ArgumentException("Invalid handle.", nameof(hdc));

            lock (Graphics.screenLock) {
                if (Graphics.screenCreated)
                    throw new InvalidOperationException("Graphics already created for screen.");

                Graphics.screenCreated = true;

                return new Graphics((int)config.Width, (int)config.Height, true);
            }
        }

        public static Graphics FromImage(Image image) => image.data;

        public void Flush() {
            if (!this.forScreen) throw new InvalidOperationException("Graphics not for screen.");

            this.surface.Flush();
        }

        public void DrawImage(Image image, int x, int y) => this.surface.DrawImage(x, y, image.data.surface, 0, 0, image.Width, image.Height, 0xFF);

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

        public void DrawEllipse(Pen pen, int x, int y, int width, int height) {
            var rgb = (uint)(pen.Color.ToArgb() & 0x00FFFFFF);

            width = (width - 1) / 2;
            height = (height - 1) / 2;

            x -= width;
            y -= height;

            this.surface.DrawEllipse(rgb, (int)pen.Width, x, y, width, height, (uint)Color.Transparent.value, x, y, (uint)Color.Transparent.value, x + width * 2, y + height * 2, pen.Color.A);
        }

        public void DrawRectangle(Pen pen, int x, int y, int width, int height) {
            var rgb = (uint)(pen.Color.ToArgb() & 0x00FFFFFF);

            this.surface.DrawRectangle(rgb, (int)pen.Width, x, y, width, height, 0, 0, (uint)Color.Transparent.value, x, y, (uint)Color.Transparent.value, x + width, y + height, pen.Color.A);
        }

        public void FillEllipse(Brush brush, int x, int y, int width, int height) {
            if (brush is SolidBrush b) {
                var rgb = (uint)(b.Color.ToArgb() & 0x00FFFFFF);

                width = (width - 1) / 2;
                height = (height - 1) / 2;

                x -= width;
                y -= height;

                this.surface.DrawEllipse(rgb, 1, x, y, width, height, rgb, x, y, rgb, x + width * 2, y + height * 2, b.Color.A);
            }
            else {
                throw new NotSupportedException();
            }
        }

        public void FillRectangle(Brush brush, int x, int y, int width, int height) {
            if (brush is SolidBrush b) {
                var rgb = (uint)(b.Color.ToArgb() & 0x00FFFFFF);

                this.surface.DrawRectangle(rgb, 1, x, y, width, height, 0, 0, rgb, x, y, rgb, x + width, y + height, b.Color.A);
            }
            else {
                throw new NotSupportedException();
            }
        }
    }

    namespace Internal {
        internal class Bitmap : MarshalByRefObject, IDisposable {
            private object m_bitmap;

            public void Dispose() {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~Bitmap() => this.Dispose(false);

            public extern int Width { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern int Height { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern Bitmap(byte[] imageData, BitmapImageType type);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern Bitmap(int width, int height);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Clear();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Dispose(bool disposing);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Flush();

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

            public static readonly int MaxWidth;//      = 220;
            public static readonly int MaxHeight;// = 176;
            public static readonly int CenterX;// = (MaxWidth - 1) / 2;
            public static readonly int CenterY;// = (MaxHeight - 1) / 2;

            static Bitmap() {
                var config = DisplayConfiguration.GetForCurrentDisplay();

                if (config == null || config.Width == 0 || config.Height == 0) return;

                MaxWidth = (int)config.Width;
                MaxHeight = (int)config.Height;

                CenterX = (MaxWidth - 1) / 2;
                CenterY = (MaxHeight - 1) / 2;
            }

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

            //Note that these values have to match the c_Type* consts in CLR_GFX_BitmapDescription
            public enum BitmapImageType : byte {
                TinyCLRBitmap = 0,
                Gif = 1,
                Jpeg = 2,
                Bmp = 3 // The windows .bmp format
            }

            public void DrawTextInRect(string text, int x, int y, int width, int height, uint dtFlags, Color color, Font font) {
                var xRelStart = 0;
                var yRelStart = 0;

                DrawTextInRect(ref text, ref xRelStart, ref yRelStart, x, y, width, height, dtFlags, color, font);
            }

            //public void DrawEllipse(Color colorOutline, int x, int y, int xRadius, int yRadius) => DrawEllipse(colorOutline, 1, x, y, xRadius, yRadius, Color.Black, 0, 0, Color.Black, 0, 0, OpacityOpaque);
            //
            //public void DrawImage(int xDst, int yDst, Graphics bitmap, int xSrc, int ySrc, int width, int height) => DrawImage(xDst, yDst, bitmap, xSrc, ySrc, width, height, OpacityOpaque);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Flush(int x, int y, int width, int height);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetClippingRectangle(int x, int y, int width, int height);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool DrawTextInRect(ref string text, ref int xRelStart, ref int yRelStart, int x, int y, int width, int height, uint dtFlags, Color color, Font font);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void RotateImage(int angle, int xDst, int yDst, Graphics bitmap, int xSrc, int ySrc, int width, int height, ushort opacity);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void MakeTransparent(Color color);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void StretchImage(int xDst, int yDst, Graphics bitmap, int width, int height, ushort opacity);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetPixel(int xPos, int yPos, Color color);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern Color GetPixel(int xPos, int yPos);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern byte[] GetBitmap();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void StretchImage(int xDst, int yDst, int widthDst, int heightDst, Graphics bitmap, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void TileImage(int xDst, int yDst, Graphics bitmap, int width, int height, ushort opacity);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Scale9Image(int xDst, int yDst, int widthDst, int heightDst, Graphics bitmap, int leftBorder, int topBorder, int rightBorder, int bottomBorder, ushort opacity);
        }
    }
}
