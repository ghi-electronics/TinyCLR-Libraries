namespace System.Drawing {
    internal sealed class Rgb565 : IDrawTarget {
        private readonly byte[] data;

        public Rgb565(int width, int height) {
            this.Width = width;
            this.Height = height;

            this.data = new byte[width * height * 2];
        }

        public int Width { get; }
        public int Height { get; }

        public void SetPixel(int x, int y, Color color) => throw new NotImplementedException();

        public void Flush() => throw new NotSupportedException();
        public void Clear(Color color) => throw new NotSupportedException();

        public void Dispose() => throw new NotSupportedException();
    }

    internal sealed class ManagedGraphics : IGraphics {
        private readonly IDrawTarget drawTarget;

        public ManagedGraphics(IDrawTarget drawTarget) => this.drawTarget = drawTarget;

        public int Width => this.drawTarget.Width;
        public int Height => this.drawTarget.Height;

        public void Clear() => this.drawTarget.Clear(Color.Black);
        public void Flush(IntPtr hdc) => this.drawTarget.Flush();
        public void Dispose() => this.drawTarget.Dispose();

        public uint GetPixel(int x, int y) => throw new NotImplementedException();
        public void SetPixel(int x, int y, uint color) => throw new NotImplementedException();
        public byte[] GetBitmap() => throw new NotImplementedException();

        public void DrawLine(uint color, int thickness, int x0, int y0, int x1, int y1) => throw new NotImplementedException();
        public void DrawRectangle(uint colorOutline, int thicknessOutline, int x, int y, int width, int height, int xCornerRadius, int yCornerRadius, uint colorGradientStart, int xGradientStart, int yGradientStart, uint colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity) => throw new NotImplementedException();
        public void DrawEllipse(uint colorOutline, int thicknessOutline, int x, int y, int xRadius, int yRadius, uint colorGradientStart, int xGradientStart, int yGradientStart, uint colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity) => throw new NotImplementedException();
        public void DrawText(string text, Font font, uint color, int x, int y) => throw new NotImplementedException();
        public void DrawTextInRect(string text, int x, int y, int width, int height, uint dtFlags, Color color, Font font) => throw new NotImplementedException();
        public void StretchImage(int xDst, int yDst, int widthDst, int heightDst, IGraphics image, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity) => throw new NotImplementedException();
    }
}
