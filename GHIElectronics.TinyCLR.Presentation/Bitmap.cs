using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;

namespace Microsoft.SPOT {
    public enum Button {
        None,
        VK_DOWN,
        VK_UP,
        VK_LEFT,
        VK_RIGHT,
        LastSystemDefinedButton
    }

    public class Bitmap : IDisposable {
        public void Dispose() {

        }

        public void Flush(int x, int y, int width, int height) {

        }

        public const ushort OpacityOpaque = 0xFF;
        public const ushort OpacityTransparent = 0;

        public const uint DT_WordWrap = 0x00000001;
        public const uint DT_TruncateAtBottom = 0x00000004;
        public const uint DT_IgnoreHeight = 0x00000010;

        public const uint DT_AlignmentLeft = 0x00000000;
        public const uint DT_AlignmentCenter = 0x00000002;
        public const uint DT_AlignmentRight = 0x00000020;

        public const uint DT_TrimmingWordEllipsis = 0x00000008;
        public const uint DT_TrimmingCharacterEllipsis = 0x00000040;

        public Bitmap(int screenWidth, int screenHeight) {

        }

        public int Height { get; }
        public int Width { get; }

        internal void DrawEllipse(Color color1, ushort thickness, int v1, int v2, int xRadius, int yRadius, Color color2, int v3, int v4, Color color3, int v5, int v6, int v7) => throw new NotImplementedException();
        internal void DrawImage(int v1, int v2, Bitmap source, int v3, int v4, int width, int height) => throw new NotImplementedException();
        internal void Clear() => throw new NotImplementedException();
        internal void DrawImage(int x, int y, Bitmap bitmapSource, int v1, int v2, int width, int height, ushort opacity) => throw new NotImplementedException();
        internal void DrawRectangle(Color outlineColor, ushort outlineThickness, int x, int y, int width, int height, int v1, int v2, Color color1, int v3, int v4, Color color2, int v5, int v6, ushort opacity) => throw new NotImplementedException();
        internal void StretchImage(int x, int y, Bitmap bitmapSource, int width, int height, ushort opacity) => throw new NotImplementedException();
        internal void DrawRectangle(Color color1, ushort thickness, int x, int y, int width, int height, int v1, int v2, Color color2, int v3, int v4, Color color3, int v5, int v6, int v7) => throw new NotImplementedException();
        internal void DrawLine(Color color, int v, int ix1, int y1, int ix2, int y2) => throw new NotImplementedException();
        internal void SetPixel(int v1, int v2, Color color) => throw new NotImplementedException();
        internal bool DrawTextInRect(ref string text, ref int xRelStart, ref int yRelStart, int v1, int v2, int width, int height, uint flags, Color color, Font font) => throw new NotImplementedException();
        internal void DrawText(string text, Font font, Color color, int v1, int v2) => throw new NotImplementedException();
        internal void TileImage(int v1, int v2, Bitmap bitmap, int width, int height, ushort opacity) => throw new NotImplementedException();
        internal void StretchImage(int v1, int v2, int widthDst, int heightDst, Bitmap bitmap, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity) => throw new NotImplementedException();
        internal void Scale9Image(int v1, int v2, int widthDst, int heightDst, Bitmap bitmap, int leftBorder, int topBorder, int rightBorder, int bottomBorder, ushort opacity) => throw new NotImplementedException();
        internal void RotateImage(int angle, int v1, int v2, Bitmap bitmap, int sourceX, int sourceY, int sourceWidth, int sourceHeight, ushort opacity) => throw new NotImplementedException();
        internal void SetClippingRectangle(int x, int y, int width, int height) => throw new NotImplementedException();
    }
}
