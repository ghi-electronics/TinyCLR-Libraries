namespace System.Drawing {
    /// <summary>An integer rectangle defined by upper-left corner (X,Y) and size (Width,Height).</summary>
    public struct Rectangle {
        public Rectangle(int x, int y, int width, int height) {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    /// <summary>A floating-point rectangle defined by upper-left corner (X,Y) and size (Width,Height).</summary>
    public struct RectangleF {
        public RectangleF(float x, float y, float width, float height) {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }
}
