////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GHIElectronics.TinyCLR.Glide.Geom
{
    /// <summary>
    /// A Rectangle object is defined by its position, width and height.
    /// </summary>
    public struct Rectangle
    {
        /// <summary>
        /// The x coordinate of the top-left corner of the rectangle.
        /// </summary>
        public int X;

        /// <summary>
        /// The y coordinate of the top-left corner of the rectangle.
        /// </summary>
        public int Y;

        /// <summary>
        /// The width of the rectangle, in pixels.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the rectangle, in pixels.
        /// </summary>
        public int Height;

        /// <summary>
        /// Creates a new Rectangle.
        /// </summary>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Clones this rectangle.
        /// </summary>
        /// <returns>Rectangle object.</returns>
        public Rectangle Clone()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        /// <summary>
        /// Determines whether the specified point is contained within the rectangular region.
        /// </summary>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <returns>True if the X-axis and Y-axis are within this rectangle, otherwise false.</returns>
        public bool Contains(int x, int y)
        {
            return (x >= X && x <= X + Width && y >= Y && y <= Y + Height);
        }

        /// <summary>
        /// Determines whether the specified point is contained within the rectangular region
        /// </summary>
        /// <param name="point">Position of the touch.</param>
        /// <returns>True if the point is within this rectangle, otherwise false.</returns>
        public bool Contains(Point point)
        {
            return (point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height);
        }

        /// <summary>
        /// Determines whether the object speficied by the parameters is contained within this Rectangle object.
        /// </summary>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>True if the area within this rectangle, otherwise false.</returns>
        public bool Contains(int x, int y, int width, int height)
        {
            return (x >= X && x + width <= X + Width && y >= Y && y + height <= Y + Height);
        }

        /// <summary>
        /// Determines whether the Rectangle object parameter is contained within this Rectangle object.
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <returns>True if the rectangle is within this rectangle, otherwise false.</returns>
        public bool Contains(Rectangle rect)
        {
            return (rect.X >= X && rect.X + rect.Width <= X + Width && rect.Y >= Y && rect.Y + rect.Height <= Y + Height);
        }

        /// <summary>
        /// Determines whether the object speficied by the parameters intersects with this Rectangle object.
        /// </summary>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">X-axis position.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns></returns>
        public bool Intersects(int x, int y, int width, int height)
        {
            return !(x >= (X + Width) || (x + width) <= X || y >= (Y + Height) || (y + height) <= Y);
        }

        /// <summary>
        /// Determines whether the Rectangle object parameter intersects with this Rectangle object.
        /// </summary>
        /// <param name="rect">Rectangle object.</param>
        /// <returns>True if the rectangle intersects with this rectangle, otherwise false.</returns>
        public bool Intersects(Rectangle rect)
        {
            return !(rect.X >= (X + Width) || (rect.X + rect.Width) <= X || rect.Y >= (Y + Height) || (rect.Y + rect.Height) <= Y);
        }

        /// <summary>
        /// Formats the rectangle as a string for debugging.
        /// </summary>
        /// <returns>The Rectangle as a string. E.g. [0, 0, 100, 100]</returns>
        public override string ToString()
        {
            return "[" + X + ", " + Y + ", " + Width + "x" + Height + "]";
        }

    }
}
