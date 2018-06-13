using System;
using System.Drawing;

namespace Microsoft.SPOT {
    public enum Button {
        None,
        VK_DOWN,
        VK_UP,
        VK_LEFT,
        VK_RIGHT,
        VK_SELECT,
        LastSystemDefinedButton
    }

    public class Bitmap : IDisposable {
        private readonly Graphics g;

        public const ushort OpacityOpaque = 0xFF;
        public const ushort OpacityTransparent = 0;

        public const uint DT_WordWrap = System.Drawing.Internal.Bitmap.DT_WordWrap;
        public const uint DT_TruncateAtBottom = System.Drawing.Internal.Bitmap.DT_TruncateAtBottom;
        public const uint DT_IgnoreHeight = System.Drawing.Internal.Bitmap.DT_IgnoreHeight;

        public const uint DT_AlignmentLeft = System.Drawing.Internal.Bitmap.DT_AlignmentLeft;
        public const uint DT_AlignmentCenter = System.Drawing.Internal.Bitmap.DT_AlignmentCenter;
        public const uint DT_AlignmentRight = System.Drawing.Internal.Bitmap.DT_AlignmentRight;

        public const uint DT_TrimmingWordEllipsis = System.Drawing.Internal.Bitmap.DT_TrimmingWordEllipsis;
        public const uint DT_TrimmingCharacterEllipsis = System.Drawing.Internal.Bitmap.DT_TrimmingCharacterEllipsis;

        public int Height => this.g.Height;
        public int Width => this.g.Width;

        public Bitmap(Graphics g) => this.g = g;
        public void Dispose() => this.g.Dispose();

        public void Clear() => this.g.Clear(Color.Black);
        public void Flush(int x, int y, int width, int height) => this.g.Flush();

        internal void SetPixel(int x, int y, Color color) => this.g.surface.SetPixel(x, y, (uint)color.ToRgb());
        internal void SetClippingRectangle(int x, int y, int width, int height) => this.g.surface.SetClippingRectangle(x, y, width, height);

        internal void DrawEllipse(Color color1, ushort thickness, int v1, int v2, int xRadius, int yRadius, Color color2, int v3, int v4, Color color3, int v5, int v6, ushort v7) => this.g.surface.DrawEllipse((uint)color1.ToRgb(), thickness, v1, v2, xRadius, yRadius, (uint)color2.ToRgb(), v3, v4, (uint)color3.ToRgb(), v5, v6, v7);
        internal void DrawRectangle(Color outlineColor, ushort outlineThickness, int x, int y, int width, int height, int v1, int v2, Color color1, int v3, int v4, Color color2, int v5, int v6, ushort opacity) => this.g.surface.DrawRectangle((uint)outlineColor.ToRgb(), outlineThickness, x, y, width, height, v1, v2, (uint)color1.ToRgb(), v3, v4, (uint)color2.ToRgb(), v5, v6, opacity);
        internal void DrawImage(int v1, int v2, Bitmap source, int v3, int v4, int width, int height) => this.g.surface.DrawImage(v1, v2, source.g.surface, v3, v4, width, height, OpacityOpaque);
        internal void DrawImage(int x, int y, Bitmap bitmapSource, int v1, int v2, int width, int height, ushort opacity) => this.g.surface.DrawImage(x, y, bitmapSource.g.surface, v1, v2, width, height, opacity);
        internal void DrawLine(Color color, int v, int ix1, int y1, int ix2, int y2) => this.g.surface.DrawLine((uint)color.ToRgb(), v, ix1, y1, ix2, y2);
        internal void DrawText(string text, Font font, Color color, int v1, int v2) => this.g.surface.DrawText(text, font, (uint)color.ToRgb(), v1, v2);
        internal bool DrawTextInRect(ref string text, ref int xRelStart, ref int yRelStart, int v1, int v2, int width, int height, uint flags, Color color, Font font) => this.g.surface.DrawTextInRect(ref text, ref xRelStart, ref yRelStart, v1, v2, width, height, flags, (uint)color.ToRgb(), font);

        internal void TileImage(int v1, int v2, Bitmap bitmap, int width, int height, ushort opacity) => this.g.surface.TileImage(v1, v2, bitmap.g.surface, width, height, opacity);
        internal void RotateImage(int angle, int v1, int v2, Bitmap bitmap, int sourceX, int sourceY, int sourceWidth, int sourceHeight, ushort opacity) => this.g.surface.RotateImage(angle, v1, v2, bitmap.g.surface, sourceX, sourceY, sourceWidth, sourceHeight, opacity);
        internal void StretchImage(int x, int y, Bitmap bitmapSource, int width, int height, ushort opacity) => this.g.surface.StretchImage(x, y, bitmapSource.g.surface, width, height, opacity);
        internal void StretchImage(int v1, int v2, int widthDst, int heightDst, Bitmap bitmap, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity) => this.g.surface.StretchImage(v1, v2, widthDst, heightDst, bitmap.g.surface, xSrc, ySrc, widthSrc, heightSrc, opacity);
        internal void Scale9Image(int v1, int v2, int widthDst, int heightDst, Bitmap bitmap, int leftBorder, int topBorder, int rightBorder, int bottomBorder, ushort opacity) => this.g.surface.Scale9Image(v1, v2, widthDst, heightDst, bitmap.g.surface, leftBorder, topBorder, rightBorder, bottomBorder, opacity);
    }
}
