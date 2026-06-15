namespace System.Drawing {
    /// <summary>An integer rectangle defined by upper-left corner (X,Y) and size (Width,Height).</summary>
    public struct Rectangle {
        /// <summary>Initializes a new rectangle with the given location and size.</summary>
        public Rectangle(int x, int y, int width, int height) {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        /// <summary>Gets or sets the x-coordinate of the upper-left corner.</summary>
        public int X { get; set; }
        /// <summary>Gets or sets the y-coordinate of the upper-left corner.</summary>
        public int Y { get; set; }
        /// <summary>Gets or sets the width of the rectangle.</summary>
        public int Width { get; set; }
        /// <summary>Gets or sets the height of the rectangle.</summary>
        public int Height { get; set; }
    }

    /// <summary>A floating-point rectangle defined by upper-left corner (X,Y) and size (Width,Height).</summary>
    public struct RectangleF {
        /// <summary>Initializes a new rectangle with the given location and size.</summary>
        public RectangleF(float x, float y, float width, float height) {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        /// <summary>Gets or sets the x-coordinate of the upper-left corner.</summary>
        public float X { get; set; }
        /// <summary>Gets or sets the y-coordinate of the upper-left corner.</summary>
        public float Y { get; set; }
        /// <summary>Gets or sets the width of the rectangle.</summary>
        public float Width { get; set; }
        /// <summary>Gets or sets the height of the rectangle.</summary>
        public float Height { get; set; }
    }
}
