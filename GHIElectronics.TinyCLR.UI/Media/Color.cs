namespace GHIElectronics.TinyCLR.UI.Media {
    /// <summary>Represents an ARGB color.</summary>
    public struct Color {
        /// <summary>The alpha (opacity) component of the color.</summary>
        public byte A { get; set; }
        /// <summary>The red component of the color.</summary>
        public byte R { get; set; }
        /// <summary>The green component of the color.</summary>
        public byte G { get; set; }
        /// <summary>The blue component of the color.</summary>
        public byte B { get; set; }

        private Color(byte a, byte r, byte g, byte b) {
            this.A = a;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        /// <summary>Creates a color from alpha, red, green and blue components.</summary>
        public static Color FromArgb(byte a, byte r, byte g, byte b) => new Color(a, r, g, b);
        /// <summary>Creates an opaque color from red, green and blue components.</summary>
        public static Color FromRgb(byte r, byte g, byte b) => new Color(255, r, g, b);

        internal uint ToNativeColor() => (uint)(this.R << 16 | this.G << 8 | this.B << 0);
        internal ushort ToNativeAlpha() => this.A;
    }

    /// <summary>Provides a set of predefined colors.</summary>
    public sealed class Colors {
        /// <summary>A fully transparent color.</summary>
        public static Color Transparent { get; } = Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF);
        /// <summary>The color black.</summary>
        public static Color Black { get; } = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
        /// <summary>The color white.</summary>
        public static Color White { get; } = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
        /// <summary>The color gray.</summary>
        public static Color Gray { get; } = Color.FromArgb(0xFF, 0x80, 0x80, 0x80);
        /// <summary>The color red.</summary>
        public static Color Red { get; } = Color.FromArgb(0xFF, 0xFF, 0x00, 0x00);
        /// <summary>The color green.</summary>
        public static Color Green { get; } = Color.FromArgb(0xFF, 0x00, 0x80, 0x00);
        /// <summary>The color blue.</summary>
        public static Color Blue { get; } = Color.FromArgb(0xFF, 0x00, 0x00, 0xFF);
        /// <summary>The color yellow.</summary>
        public static Color Yellow { get; } = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00);
        /// <summary>The color purple.</summary>
        public static Color Purple { get; } = Color.FromArgb(0xFF, 0x80, 0x00, 0x80);
        /// <summary>The color teal.</summary>
        public static Color Teal { get; } = Color.FromArgb(0xFF, 0x00, 0x80, 0x80);
        /// <summary>Default focus ring / accent (PC-style highlight).</summary>
        public static Color CornflowerBlue { get; } = Color.FromArgb(0xFF, 0x64, 0x95, 0xED);
        /// <summary>The color light gray.</summary>
        public static Color LightGray { get; } = Color.FromArgb(0xFF, 0xD3, 0xD3, 0xD3);
    }
}
