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

        public void DrawTextInRect(string text, int x, int y, int width, int height, uint dtFlags, Color color, Font font) => throw new NotSupportedException();

        public void DrawLine(uint color, int thickness, int x0, int y0, int x1, int y1) => throw new NotImplementedException();
        public void DrawRectangle(uint colorOutline, int thicknessOutline, int x, int y, int width, int height, int xCornerRadius, int yCornerRadius, uint colorGradientStart, int xGradientStart, int yGradientStart, uint colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity) => throw new NotImplementedException();
        public void DrawEllipse(uint colorOutline, int thicknessOutline, int x, int y, int xRadius, int yRadius, uint colorGradientStart, int xGradientStart, int yGradientStart, uint colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity) => throw new NotImplementedException();

        public void DrawText(string text, Font font, uint color, int x, int y) {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (font == null) throw new ArgumentNullException(nameof(font));
            if (!font.IsGHIMono8x5) throw new NotSupportedException();

            var hScale = font.Size / 8;
            var vScale = hScale;

            for (var i = 0; i < text.Length; i++) {

            }
        }

        readonly byte[] GHIMono8x5 = new byte[95 * 5] {
            0x00, 0x00, 0x00, 0x00, 0x00, /* Space	0x20 */
            0x00, 0x00, 0x4f, 0x00, 0x00, /* ! */
            0x00, 0x07, 0x00, 0x07, 0x00, /* " */
            0x14, 0x7f, 0x14, 0x7f, 0x14, /* # */
            0x24, 0x2a, 0x7f, 0x2a, 0x12, /* $ */
            0x23, 0x13, 0x08, 0x64, 0x62, /* % */
            0x36, 0x49, 0x55, 0x22, 0x20, /* & */
            0x00, 0x05, 0x03, 0x00, 0x00, /* ' */
            0x00, 0x1c, 0x22, 0x41, 0x00, /* ( */
            0x00, 0x41, 0x22, 0x1c, 0x00, /* ) */
            0x14, 0x08, 0x3e, 0x08, 0x14, /* // */
            0x08, 0x08, 0x3e, 0x08, 0x08, /* + */
            0x50, 0x30, 0x00, 0x00, 0x00, /* , */
            0x08, 0x08, 0x08, 0x08, 0x08, /* - */
            0x00, 0x60, 0x60, 0x00, 0x00, /* . */
            0x20, 0x10, 0x08, 0x04, 0x02, /* / */
            0x3e, 0x51, 0x49, 0x45, 0x3e, /* 0		0x30 */
            0x00, 0x42, 0x7f, 0x40, 0x00, /* 1 */
            0x42, 0x61, 0x51, 0x49, 0x46, /* 2 */
            0x21, 0x41, 0x45, 0x4b, 0x31, /* 3 */
            0x18, 0x14, 0x12, 0x7f, 0x10, /* 4 */
            0x27, 0x45, 0x45, 0x45, 0x39, /* 5 */
            0x3c, 0x4a, 0x49, 0x49, 0x30, /* 6 */
            0x01, 0x71, 0x09, 0x05, 0x03, /* 7 */
            0x36, 0x49, 0x49, 0x49, 0x36, /* 8 */
            0x06, 0x49, 0x49, 0x29, 0x1e, /* 9 */
            0x00, 0x36, 0x36, 0x00, 0x00, /* : */
            0x00, 0x56, 0x36, 0x00, 0x00, /* ; */
            0x08, 0x14, 0x22, 0x41, 0x00, /* < */
            0x14, 0x14, 0x14, 0x14, 0x14, /* = */
            0x00, 0x41, 0x22, 0x14, 0x08, /* > */
            0x02, 0x01, 0x51, 0x09, 0x06, /* ? */
            0x3e, 0x41, 0x5d, 0x55, 0x1e, /* @		0x40 */
            0x7e, 0x11, 0x11, 0x11, 0x7e, /* A */
            0x7f, 0x49, 0x49, 0x49, 0x36, /* B */
            0x3e, 0x41, 0x41, 0x41, 0x22, /* C */
            0x7f, 0x41, 0x41, 0x22, 0x1c, /* D */
            0x7f, 0x49, 0x49, 0x49, 0x41, /* E */
            0x7f, 0x09, 0x09, 0x09, 0x01, /* F */
            0x3e, 0x41, 0x49, 0x49, 0x7a, /* G */
            0x7f, 0x08, 0x08, 0x08, 0x7f, /* H */
            0x00, 0x41, 0x7f, 0x41, 0x00, /* I */
            0x20, 0x40, 0x41, 0x3f, 0x01, /* J */
            0x7f, 0x08, 0x14, 0x22, 0x41, /* K */
            0x7f, 0x40, 0x40, 0x40, 0x40, /* L */
            0x7f, 0x02, 0x0c, 0x02, 0x7f, /* M */
            0x7f, 0x04, 0x08, 0x10, 0x7f, /* N */
            0x3e, 0x41, 0x41, 0x41, 0x3e, /* O */
            0x7f, 0x09, 0x09, 0x09, 0x06, /* P		0x50 */
            0x3e, 0x41, 0x51, 0x21, 0x5e, /* Q */
            0x7f, 0x09, 0x19, 0x29, 0x46, /* R */
            0x26, 0x49, 0x49, 0x49, 0x32, /* S */
            0x01, 0x01, 0x7f, 0x01, 0x01, /* T */
            0x3f, 0x40, 0x40, 0x40, 0x3f, /* U */
            0x1f, 0x20, 0x40, 0x20, 0x1f, /* V */
            0x3f, 0x40, 0x38, 0x40, 0x3f, /* W */
            0x63, 0x14, 0x08, 0x14, 0x63, /* X */
            0x07, 0x08, 0x70, 0x08, 0x07, /* Y */
            0x61, 0x51, 0x49, 0x45, 0x43, /* Z */
            0x00, 0x7f, 0x41, 0x41, 0x00, /* [ */
            0x02, 0x04, 0x08, 0x10, 0x20, /* \ */
            0x00, 0x41, 0x41, 0x7f, 0x00, /* ] */
            0x04, 0x02, 0x01, 0x02, 0x04, /* ^ */
            0x40, 0x40, 0x40, 0x40, 0x40, /* _ */
            0x00, 0x00, 0x03, 0x05, 0x00, /* `		0x60 */
            0x20, 0x54, 0x54, 0x54, 0x78, /* a */
            0x7F, 0x44, 0x44, 0x44, 0x38, /* b */
            0x38, 0x44, 0x44, 0x44, 0x44, /* c */
            0x38, 0x44, 0x44, 0x44, 0x7f, /* d */
            0x38, 0x54, 0x54, 0x54, 0x18, /* e */
            0x04, 0x04, 0x7e, 0x05, 0x05, /* f */
            0x08, 0x54, 0x54, 0x54, 0x3c, /* g */
            0x7f, 0x08, 0x04, 0x04, 0x78, /* h */
            0x00, 0x44, 0x7d, 0x40, 0x00, /* i */
            0x20, 0x40, 0x44, 0x3d, 0x00, /* j */
            0x7f, 0x10, 0x28, 0x44, 0x00, /* k */
            0x00, 0x41, 0x7f, 0x40, 0x00, /* l */
            0x7c, 0x04, 0x7c, 0x04, 0x78, /* m */
            0x7c, 0x08, 0x04, 0x04, 0x78, /* n */
            0x38, 0x44, 0x44, 0x44, 0x38, /* o */
            0x7c, 0x14, 0x14, 0x14, 0x08, /* p		0x70 */
            0x08, 0x14, 0x14, 0x14, 0x7c, /* q */
            0x7c, 0x08, 0x04, 0x04, 0x08, /* r */
            0x48, 0x54, 0x54, 0x54, 0x24, /* s */
            0x04, 0x04, 0x3f, 0x44, 0x44, /* t */
            0x3c, 0x40, 0x40, 0x20, 0x7c, /* u */
            0x1c, 0x20, 0x40, 0x20, 0x1c, /* v */
            0x3c, 0x40, 0x30, 0x40, 0x3c, /* w */
            0x44, 0x28, 0x10, 0x28, 0x44, /* x */
            0x0c, 0x50, 0x50, 0x50, 0x3c, /* y */
            0x44, 0x64, 0x54, 0x4c, 0x44, /* z */
            0x08, 0x36, 0x41, 0x41, 0x00, /* { */
            0x00, 0x00, 0x77, 0x00, 0x00, /* | */
            0x00, 0x41, 0x41, 0x36, 0x08, /* } */
            0x08, 0x08, 0x2a, 0x1c, 0x08  /* ~ */
        };

        public void StretchImage(int xDst, int yDst, int widthDst, int heightDst, IGraphics image, int xSrc, int ySrc, int widthSrc, int heightSrc, ushort opacity) {
            if (!(image is ManagedGraphics mg)) throw new NotSupportedException();

            var dt = mg.drawTarget;

            if (xSrc != 0 || ySrc != 0 || widthSrc != dt.Width || heightSrc != dt.Height || widthSrc != widthDst || heightSrc != heightDst || opacity != 0xFF) throw new NotSupportedException();

            for (var y = 0; y < widthDst; y++)
                for (var x = 0; x < widthDst; x++)
                    this.SetPixel(x, y, mg.GetPixel(x, y));
        }

        private void DrawLetter(int x, int y, char letter, uint color, int HScale, int VScale) {
            var index = 5 * (letter - 32);

            for (var horizontalFontSize = 0; horizontalFontSize < 5; horizontalFontSize++) {
                for (var hs = 0; hs < HScale; hs++) {
                    for (var verticleFoneSize = 0; verticleFoneSize < 8; verticleFoneSize++) {
                        for (var vs = 0; vs < VScale; vs++) {
                            if ((this.GHIMono8x5[index + horizontalFontSize] & (1 << verticleFoneSize)) != 0)
                                this.SetPixel(x + (horizontalFontSize * HScale) + hs, y + (verticleFoneSize * VScale) + vs, color);
                        }
                    }
                }
            }
        }
    }
}
