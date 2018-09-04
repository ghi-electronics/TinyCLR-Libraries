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

        public Color GetPixel(int x, int y) {
            var idx = (y * this.Width + x) * 2;
            var b1 = this.data[idx];
            var b2 = this.data[idx + 1];

            var r = (b1 & 0b1111_1000) << 0;
            var g = ((b1 & 0b0000_0111) << 5) | ((b2 & 0b1110_0000) >> 3);
            var b = (b2 & 0b0001_1111) << 3;

            return Color.FromArgb(r, g, b);
        }

        public void SetPixel(int x, int y, Color color) {
            var r = (color.R & 0b1111_1000) << 8;
            var g = (color.G & 0b1111_1100) << 3;
            var b = (color.B & 0b1111_1000) >> 3;
            var c = r | g | b;

            var idx = (y * this.Width + x) * 2;
            var b1 = (byte)((c & 0xFF00) >> 8);
            var b2 = (byte)((c & 0x00FF) >> 0);

            this.data[idx] = b1;
            this.data[idx + 1] = b2;
        }

        public byte[] GetData() => this.data;

        public void Flush() {

        }

        public void Clear(Color color) {
            if (color != Color.Black) throw new NotSupportedException();

            Array.Clear(this.data, 0, this.data.Length);
        }

        public void Dispose() {

        }
    }

    internal sealed class ManagedGraphics : IGraphics {
        private readonly IDrawTarget drawTarget;

        public ManagedGraphics(IDrawTarget drawTarget) => this.drawTarget = drawTarget;

        public int Width => this.drawTarget.Width;
        public int Height => this.drawTarget.Height;

        public void Clear() => this.drawTarget.Clear(Color.Black);
        public void Flush(IntPtr hdc) => this.drawTarget.Flush();
        public void Dispose() => this.drawTarget.Dispose();

        public uint GetPixel(int x, int y) => (uint)this.drawTarget.GetPixel(x, y).ToArgb();
        public void SetPixel(int x, int y, uint color) => this.drawTarget.SetPixel(x, y, new Color(color));
        public byte[] GetBitmap() => this.drawTarget.GetData();

        public void DrawLine(uint color, int thickness, int x0, int y0, int x1, int y1) => throw new NotImplementedException();
        public void DrawRectangle(uint colorOutline, int thicknessOutline, int x, int y, int width, int height, int xCornerRadius, int yCornerRadius, uint colorGradientStart, int xGradientStart, int yGradientStart, uint colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity) => throw new NotImplementedException();
        public void DrawEllipse(uint colorOutline, int thicknessOutline, int x, int y, int xRadius, int yRadius, uint colorGradientStart, int xGradientStart, int yGradientStart, uint colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity) => throw new NotImplementedException();
        public void DrawText(string text, Font font, uint color, int x, int y) => throw new NotImplementedException();
        public void DrawTextInRect(string text, int x, int y, int width, int height, uint dtFlags, Color color, Font font) => throw new NotImplementedException();
        public void StretchImage(int xDst, int yDst, int widthDst, int heightDst, IGraphics image, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity) => throw new NotImplementedException();
    }
}
