namespace GHIElectronics.TinyCLR.UI.Media {
    public struct Color {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        private Color(byte a, byte r, byte g, byte b) {
            this.A = a;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public static Color FromArgb(byte a, byte r, byte g, byte b) => new Color(a, r, g, b);
        public static Color FromRgb(byte r, byte g, byte b) => new Color(255, r, g, b);

        internal uint ToNativeColor() => (uint)(this.R << 16 | this.G << 8 | this.B << 0);
        internal ushort ToNativeAlpha() => this.A;
    }

    public sealed class Colors {
        public static Color Transparent { get; } = Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF);
        public static Color Black { get; } = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
        public static Color White { get; } = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
        public static Color Gray { get; } = Color.FromArgb(0xFF, 0x80, 0x80, 0x80);
        public static Color Red { get; } = Color.FromArgb(0xFF, 0xFF, 0x00, 0x00);
        public static Color Green { get; } = Color.FromArgb(0xFF, 0x00, 0x80, 0x00);
        public static Color Blue { get; } = Color.FromArgb(0xFF, 0x00, 0x00, 0xFF);
        public static Color Yellow { get; } = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00);
        public static Color Purple { get; } = Color.FromArgb(0xFF, 0x80, 0x00, 0x80);
        public static Color Teal { get; } = Color.FromArgb(0xFF, 0x00, 0x80, 0x80);
    }
}
