////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace GHIElectronics.TinyCLR.UI.Glide
{
    /// <summary>
    /// The Graphics class contains a set of methods you can use to draw on itself.
    /// </summary>
    public sealed class GlideGraphics: IDisposable
    {
        
        private System.Drawing.Bitmap _realBitmap;
        private System.Drawing.Graphics _bitmap;
        /// <summary>
        /// Creates a new Graphics object.
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public GlideGraphics(int width, int height)
        {
            _realBitmap = new System.Drawing.Bitmap(width, height);
            _bitmap = System.Drawing.Graphics.FromImage(_realBitmap);
           
        }

        public GlideGraphics(System.Drawing.Bitmap bmp)
        {
            _realBitmap = bmp;
            _bitmap = System.Drawing.Graphics.FromImage(_realBitmap);

        }

        public Graphics GetGraphic()
        {
            return _bitmap;
        }

        /// <summary>
        /// Clears the Graphics object.
        /// </summary>
        public void Clear()
        {
            _bitmap.Clear();//System.Drawing.Color.Black
        }

        /// <summary>
        /// Disposes of the Graphics object.
        /// </summary>
        public void Dispose()
        {
            _bitmap.Dispose();
        }

        /// <summary>
        /// Draws an ellipse.
        /// </summary>
        /// <param name="colorOutline">Color of the outline.</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="xRadius">X Radius</param>
        /// <param name="yRadius">Y Radius</param>
        public void DrawEllipse(System.Drawing.Color colorOutline, int x, int y, int xRadius, int yRadius)
        {
            var width = 2 * xRadius;
            var height = 2 * yRadius;
            var pen = new Pen(new SolidBrush(colorOutline));
            _bitmap.DrawEllipse(pen,x,y,width,height);
        }

        /// <summary>
        /// Draws an ellipse.
        /// </summary>
        /// <param name="colorOutline">Color of the outline.</param>
        /// <param name="thicknessOutline">Thickness of the outline.</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="xRadius">X Radius</param>
        /// <param name="yRadius">Y Radius</param>
        /// <param name="colorGradientStart">Starting gradient color.</param>
        /// <param name="xGradientStart">Starting gradient X.</param>
        /// <param name="yGradientStart">Starting gradient Y.</param>
        /// <param name="colorGradientEnd">Ending gradient color.</param>
        /// <param name="xGradientEnd">Ending gradient X.</param>
        /// <param name="yGradientEnd">Ending gradient Y.</param>
        /// <param name="opacity">Opacity</param>
        public void DrawEllipse(System.Drawing.Color colorOutline, int thicknessOutline, int x, int y, int xRadius, int yRadius, System.Drawing.Color colorGradientStart, int xGradientStart, int yGradientStart, System.Drawing.Color colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity)
        {
            var width = 2 * xRadius;
            var height = 2 * yRadius;
            var pen = new Pen(new SolidBrush(colorOutline));
            _bitmap.DrawEllipse(pen, x, y, width, height);
            //_bitmap.DrawEllipse(colorOutline.ToNativeColor(), thicknessOutline, x, y, xRadius, yRadius, colorGradientStart.ToNativeColor(), xGradientStart, yGradientStart, colorGradientEnd.ToNativeColor(), xGradientEnd, yGradientEnd, opacity);
        }

        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="xDst">Destination X.</param>
        /// <param name="yDst">Destination Y.</param>
        /// <param name="bitmap">Bitmap</param>
        /// <param name="xSrc">Source X.</param>
        /// <param name="ySrc">Source Y.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void DrawImage(int xDst, int yDst, System.Drawing.Bitmap bitmap, int xSrc, int ySrc, int width, int height)
        {
            _bitmap.DrawImage(bitmap, xDst, yDst, new System.Drawing.Rectangle(xSrc, ySrc, width, height), GraphicsUnit.Pixel);
            //_bitmap.DrawImage(xDst, yDst, bitmap, xSrc, ySrc, width, height, 0xff);
        }

        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="xDst">Destination X.</param>
        /// <param name="yDst">Destination Y.</param>
        /// <param name="bitmap">Bitmap</param>
        /// <param name="xSrc">Source X.</param>
        /// <param name="ySrc">Source Y.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="opacity">Opacity</param>
        public void DrawImage(int xDst, int yDst, System.Drawing.Bitmap bitmap, int xSrc, int ySrc, int width, int height, ushort opacity)
        {
            _bitmap.DrawImage(bitmap, xDst, yDst, new System.Drawing.Rectangle(xSrc, ySrc, width, height), GraphicsUnit.Pixel);
            //_bitmap.DrawImage(xDst, yDst, bitmap, xSrc, ySrc, width, height, opacity);
        }

        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="thickness">Thickness</param>
        /// <param name="x0">Starting X.</param>
        /// <param name="y0">Starting Y.</param>
        /// <param name="x1">Ending X.</param>
        /// <param name="y1">Ending Y.</param>
        public void DrawLine(System.Drawing.Color color, int thickness, int x0, int y0, int x1, int y1)
        {
            
            var pen = new Pen(new SolidBrush(color),thickness);
           
            _bitmap.DrawLine(pen, x0, y0, x1, y1);
            //_bitmap.DrawLine(color.ToNativeColor(), thickness, x0, y0, x1, y1);
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="colorOutline">Color of the outline.</param>
        /// <param name="thicknessOutline">Thickness of the outline.</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="xCornerRadius">X corner radius.</param>
        /// <param name="yCornerRadius">Y corner radius.</param>
        /// <param name="colorGradientStart">Starting gradient color.</param>
        /// <param name="xGradientStart">Starting gradient X.</param>
        /// <param name="yGradientStart">Starting gradient Y.</param>
        /// <param name="colorGradientEnd">Ending gradient color.</param>
        /// <param name="xGradientEnd">Ending gradient X.</param>
        /// <param name="yGradientEnd">Ending gradient Y.</param>
        /// <param name="opacity">Opacity</param>
        public void DrawRectangle(System.Drawing.Color colorOutline, int thicknessOutline, int x, int y, int width, int height, int xCornerRadius, int yCornerRadius, System.Drawing.Color colorGradientStart, int xGradientStart, int yGradientStart, System.Drawing.Color colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity)
        {
            var pen = new Pen(new SolidBrush(colorOutline),thicknessOutline);
            _bitmap.DrawRectangle(pen, x, y, width, height);
            //_bitmap.DrawRectangle(colorOutline.ToNativeColor(), thicknessOutline, x, y, width, height, xCornerRadius, yCornerRadius, colorGradientStart.ToNativeColor(), xGradientStart, yGradientStart, colorGradientEnd.ToNativeColor(), xGradientEnd, yGradientEnd, opacity);

        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="color">Color</param>
        /// <param name="opacity">Opacity</param>
        public void DrawRectangle(Geom.Rectangle rect, System.Drawing.Color color, ushort opacity)
        {
            var brush = new SolidBrush(color);
            //var pen = new Pen(color);
            _bitmap.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
            //_bitmap.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            //_bitmap.DrawRectangle(0, 0, rect.X, rect.Y, rect.Width, rect.Height, 0, 0, color.ToNativeColor(), 0, 0, 0, 0, 0, opacity);
        }

        /// <summary>
        /// Draws text.
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="font">Font</param>
        /// <param name="color">Color</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        public void DrawText(string text, System.Drawing.Font font, System.Drawing.Color color, int x, int y)
        {
            SolidBrush brush = new SolidBrush(color);
            _bitmap.DrawString(text, font, brush, x, y);
            //_bitmap.DrawText(text, font, color.ToNativeColor(), x, y);
        }

        /// <summary>
        /// Draws text in a rectangle.
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="dtFlags">Flags found in Bitmap.</param>
        /// <param name="color">Color</param>
        /// <param name="font">Font</param>
        public void DrawTextInRect(string text, int x, int y, int width, int height, StringFormat format, System.Drawing.Color color, System.Drawing.Font font)
        {
            //uint dtFlags
            SolidBrush brush = new SolidBrush(color);
            _bitmap.DrawString(text, font, brush,new RectangleF( x, y,width,height),format);
            //_bitmap.DrawTextInRect(text, x, y, width, height, dtFlags, color, font);
        }

        /// <summary>
        /// Draws text in a rectangle.
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="xRelStart"></param>
        /// <param name="yRelStart"></param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="dtFlags">Flags found in Bitmap.</param>
        /// <param name="color">Color</param>
        /// <param name="font">Font</param>
        /// <returns></returns>
        public bool DrawTextInRect(ref string text, ref int xRelStart, ref int yRelStart, int x, int y, int width, int height, StringFormat format, System.Drawing.Color color, System.Drawing.Font font)
        {
            //uint dtFlags
            SolidBrush brush = new SolidBrush(color);
            _bitmap.DrawString(text, font, brush, new RectangleF(x, y, width, height), format);
            return true;
            //return _bitmap.DrawTextInRect(ref text, ref xRelStart, ref yRelStart, x, y, width, height, dtFlags, color.ToNativeColor(), font);
        }

        /// <summary>
        /// Draws text in a rectangle.
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="rect">Rectangle</param>
        /// <param name="dtFlags">Flags found in Bitmap.</param>
        /// <param name="color">Color</param>
        /// <param name="font">Font</param>
        public void DrawTextInRect(string text, Geom.Rectangle rect, StringFormat format, System.Drawing.Color color, System.Drawing.Font font)
        {
            //uint dtFlags
            SolidBrush brush = new SolidBrush(color);
            _bitmap.DrawString(text, font, brush, new RectangleF(rect.X, rect.Y,rect.Width,rect.Height),format);
            //_bitmap.DrawTextInRect(text, rect.X, rect.Y, rect.Width, rect.Height, dtFlags, color, font);
        }

        /// <summary>
        /// Flushes the Graphics object to the screen.
        /// </summary>
        public void Flush()
        {
            _bitmap.Flush();
            //_bitmap.Flush(0,0,_bitmap.Width,_bitmap.Height);
        }

        /// <summary>
        /// Flushes the Graphics object to the screen.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void Flush(int x, int y, int width, int height)
        {
            _bitmap.Flush();
            //_bitmap.Flush(x, y, width, height);
        }

        /// <summary>
        /// Bitmap used by the Graphics object.
        /// </summary>
        /// <returns></returns>
        public System.Drawing.Bitmap GetBitmap()
        {
            return _realBitmap;
        }

        //public Color GetPixel(int xPos, int yPos);
        //public void MakeTransparent(Color color);
        //public void RotateImage(int angle, int xDst, int yDst, Bitmap bitmap, int xSrc, int ySrc, int width, int height, ushort opacity);

        /// <summary>
        /// Resizes images without distortion.
        /// </summary>
        /// <param name="xDst">Destination X.</param>
        /// <param name="yDst">Destination Y.</param>
        /// <param name="widthDst">Width</param>
        /// <param name="heightDst">Height</param>
        /// <param name="bitmap">Bitmap</param>
        /// <param name="leftBorder">Left border.</param>
        /// <param name="topBorder">Top border.</param>
        /// <param name="rightBorder">Right border.</param>
        /// <param name="bottomBorder">Bottom border.</param>
        /// <param name="opacity">Opacity</param>
        public void Scale9Image(int xDst, int yDst, int widthDst, int heightDst, System.Drawing.Bitmap bitmap, int leftBorder, int topBorder, int rightBorder, int bottomBorder, ushort opacity)
        {
            //_bitmap.DrawImage(bitmap, new System.Drawing.Rectangle(xDst, yDst, widthDst, heightDst), new System.Drawing.Rectangle(leftBorder, topBorder, bitmap.Width-rightBorder, bitmap.Height-bottomBorder), GraphicsUnit.Pixel);
            BitmapHelper helper = new BitmapHelper(_bitmap);
            helper.Scale9Image(xDst, yDst, widthDst, heightDst, bitmap, leftBorder, topBorder, rightBorder, bottomBorder, opacity);
            //_bitmap.Scale9Image(xDst, yDst, widthDst, heightDst, bitmap, leftBorder, topBorder, rightBorder, bottomBorder, opacity);
        }

        /// <summary>
        /// Resizes images without distortion.
        /// </summary>
        /// <param name="xDst">Destination X.</param>
        /// <param name="yDst">Destination Y.</param>
        /// <param name="widthDst">Width</param>
        /// <param name="heightDst">Height</param>
        /// <param name="bitmap">Bitmap</param>
        /// <param name="border">Border</param>
        /// <param name="opacity">Opacity</param>
        public void Scale9Image(int xDst, int yDst, int widthDst, int heightDst, System.Drawing.Bitmap bitmap, int border, ushort opacity)
        {
            //_bitmap.DrawImage(bitmap, new System.Drawing.Rectangle(xDst, yDst, widthDst, heightDst), new System.Drawing.Rectangle(border, border, bitmap.Width-border, bitmap.Height-border), GraphicsUnit.Pixel);
            BitmapHelper helper = new BitmapHelper(_bitmap);
            helper.Scale9Image(xDst, yDst, widthDst, heightDst, bitmap, border, border, border, border, opacity);
            //_bitmap.Scale9Image(xDst, yDst, widthDst, heightDst, bitmap, border, border, border, border, opacity);
        }

        /// <summary>
        /// Sets the clipping rectangle.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void SetClippingRectangle(int x, int y, int width, int height)
        {
            //need implementation here
            BitmapHelper helper = new BitmapHelper(_bitmap);
            helper.SetClippingRectangle(x, y, width, height);
            //_bitmap.SetClippingRectangle(x, y, width, height);
        }

        //public void SetPixel(int xPos, int yPos, Color color);

        /// <summary>
        /// Stretch an image.
        /// </summary>
        /// <param name="xDst">Destination X.</param>
        /// <param name="yDst">Destination Y.</param>
        /// <param name="bitmap">Bitmap</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="opacity">Opacity</param>
        public void StretchImage(int xDst, int yDst, System.Drawing.Bitmap bitmap, int width, int height, ushort opacity)
        {
            BitmapHelper helper = new BitmapHelper(_bitmap);
            helper.StretchImage(xDst, yDst, bitmap, width, height, opacity);
            //_bitmap.DrawImage(bitmap, new System.Drawing.Rectangle(xDst, yDst, width, height), new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
            //_bitmap.StretchImage(xDst, yDst, bitmap, width, height, opacity);
        }

        //public void StretchImage(int xDst, int yDst, int widthDst, int heightDst, Bitmap bitmap, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity);
        //public void TileImage(int xDst, int yDst, Bitmap bitmap, int width, int height, ushort opacity);
    }
}
