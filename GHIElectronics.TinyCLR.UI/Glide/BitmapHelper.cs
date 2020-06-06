////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace GHIElectronics.TinyCLR.UI.Glide
{
    public class BitmapHelper : IDisposable
    {
        public readonly Graphics g;
        private readonly System.Drawing.Internal.Bitmap surface;
        public const ushort OpacityOpaque = 255;
        public const ushort OpacityTransparent = 0;
        public const uint DT_WordWrap = 1;
        public const uint DT_TruncateAtBottom = 4;
        public const uint DT_IgnoreHeight = 16;
        public const uint DT_AlignmentLeft = 0;
        public const uint DT_AlignmentCenter = 2;
        public const uint DT_AlignmentRight = 32;
        public const uint DT_TrimmingWordEllipsis = 8;
        public const uint DT_TrimmingCharacterEllipsis = 64;

        public int Height
        {
            get
            {
                return this.g.Height;
            }
        }

        public int Width
        {
            get
            {
                return this.g.Width;
            }
        }

        public BitmapHelper(Graphics g)
        {
            this.g = g;
            this.surface = BitmapHelper.Extract(g);
            
        }

        private static System.Drawing.Internal.Bitmap Extract(Graphics g)
        {
            if (g.surface is System.Drawing.Internal.Bitmap surface)
                return surface;
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            this.g.Dispose();
        }

        public void Clear()
        {
            this.g.Clear();//System.Drawing.Color.Black
        }

        public void Flush(int x, int y, int width, int height)
        {
            this.g.Flush();
        }

        internal void SetPixel(int x, int y, Media.Color color)
        {
            this.surface.SetPixel(x, y, color.ToNativeColor());
        }

        internal void SetClippingRectangle(int x, int y, int width, int height)
        {
            this.surface.SetClippingRectangle(x, y, width, height);
        }

        internal void DrawEllipse(
          Media.Color color1,
          ushort thickness,
          int v1,
          int v2,
          int xRadius,
          int yRadius,
          Media.Color color2,
          int v3,
          int v4,
          Media.Color color3,
          int v5,
          int v6,
          ushort v7)
        {
            this.surface.DrawEllipse(color1.ToNativeColor(), (int)thickness, v1, v2, xRadius, yRadius, color2.ToNativeColor(), v3, v4, color3.ToNativeColor(), v5, v6, v7);
        }

        internal void DrawRectangle(
          Media.Color outlineColor,
          ushort outlineThickness,
          int x,
          int y,
          int width,
          int height,
          int v1,
          int v2,
          Media.Color color1,
          int v3,
          int v4,
          Media.Color color2,
          int v5,
          int v6,
          ushort opacity)
        {
            this.surface.DrawRectangle(outlineColor.ToNativeColor(), (int)outlineThickness, x, y, width, height, v1, v2, color1.ToNativeColor(), v3, v4, color2.ToNativeColor(), v5, v6, opacity);
        }

        internal void DrawImage(
          int v1,
          int v2,
          System.Drawing.Bitmap source,
          int v3,
          int v4,
          int width,
          int height)
        {
            this.surface.DrawImage(v1, v2, BitmapHelper.Extract(source.data), v3, v4, width, height, (ushort)byte.MaxValue);
        }

        internal void DrawImage(
          int x,
          int y,
          System.Drawing.Bitmap bitmapSource,
          int v1,
          int v2,
          int width,
          int height,
          ushort opacity)
        {
            this.surface.DrawImage(x, y, BitmapHelper.Extract(bitmapSource.data), v1, v2, width, height, opacity);
        }

        internal void DrawLine(Media.Color color, int v, int ix1, int y1, int ix2, int y2)
        {
            this.surface.DrawLine(color.ToNativeColor(), v, ix1, y1, ix2, y2);
        }

        internal void DrawText(string text, Font font, Media.Color color, int v1, int v2)
        {
            this.surface.DrawText(text, font, color.ToNativeColor(), v1, v2);
        }

        internal bool DrawTextInRect(
          ref string text,
          ref int xRelStart,
          ref int yRelStart,
          int v1,
          int v2,
          int width,
          int height,
          uint flags,
          Media.Color color,
          Font font)
        {
            return this.surface.DrawTextInRect(ref text, ref xRelStart, ref yRelStart, v1, v2, width, height, flags, color.ToNativeColor(), font);
        }

        internal void TileImage(
          int v1,
          int v2,
          System.Drawing.Bitmap bitmap,
          int width,
          int height,
          ushort opacity)
        {
            this.surface.TileImage(v1, v2, BitmapHelper.Extract(bitmap.data), width, height, opacity);
        }

        internal void RotateImage(
          int angle,
          int v1,
          int v2,
          System.Drawing.Bitmap bitmap,
          int sourceX,
          int sourceY,
          int sourceWidth,
          int sourceHeight,
          ushort opacity)
        {
            this.surface.RotateImage(angle, v1, v2, BitmapHelper.Extract(bitmap.data), sourceX, sourceY, sourceWidth, sourceHeight, opacity);
        }

        internal void StretchImage(
          int x,
          int y,
          System.Drawing.Bitmap bitmapSource,
          int width,
          int height,
          ushort opacity)
        {
            this.surface.StretchImage(x, y, BitmapHelper.Extract(bitmapSource.data), width, height, opacity);
        }

        internal void StretchImage(
          int v1,
          int v2,
          int widthDst,
          int heightDst,
          System.Drawing.Bitmap bitmap,
          int xSrc,
          int ySrc,
          int widthSrc,
          int heightSrc,
          ushort opacity)
        {
            this.surface.StretchImage(v1, v2, widthDst, heightDst, bitmap.data.surface, xSrc, ySrc, widthSrc, heightSrc, opacity);
        }

        public void Scale9Image(
          int v1,
          int v2,
          int widthDst,
          int heightDst,
          System.Drawing.Bitmap bitmap,
          int leftBorder,
          int topBorder,
          int rightBorder,
          int bottomBorder,
          ushort opacity)
        {
            this.surface.Scale9Image(v1, v2, widthDst, heightDst, BitmapHelper.Extract(bitmap.data), leftBorder, topBorder, rightBorder, bottomBorder, opacity);
        }
    }
}
