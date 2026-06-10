using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Drawing {
    /// <summary>32-bit ARGB color value. Construct via <see cref="FromArgb(int, int, int)"/> / <see cref="FromArgb(int, int, int, int)"/>, or use one of the named static constants.</summary>
    [Serializable(), DebuggerDisplay("{NameAndARGBValue}")]
    public struct Color {
        /// <summary>Represents a color that is null or uninitialized.</summary>
        public static readonly Color Empty = new Color();

        /// <summary>Gets a fully transparent color.</summary>
        public static Color Transparent { get; } = Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF);
        /// <summary>Gets the color black.</summary>
        public static Color Black { get; } = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
        /// <summary>Gets the color white.</summary>
        public static Color White { get; } = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
        /// <summary>Gets the color gray.</summary>
        public static Color Gray { get; } = Color.FromArgb(0xFF, 0x80, 0x80, 0x80);
        /// <summary>Gets the color red.</summary>
        public static Color Red { get; } = Color.FromArgb(0xFF, 0xFF, 0x00, 0x00);
        /// <summary>Gets the color green.</summary>
        public static Color Green { get; } = Color.FromArgb(0xFF, 0x00, 0x80, 0x00);
        /// <summary>Gets the color blue.</summary>
        public static Color Blue { get; } = Color.FromArgb(0xFF, 0x00, 0x00, 0xFF);
        /// <summary>Gets the color yellow.</summary>
        public static Color Yellow { get; } = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00);
        /// <summary>Gets the color purple.</summary>
        public static Color Purple { get; } = Color.FromArgb(0xFF, 0x80, 0x00, 0x80);
        /// <summary>Gets the color teal.</summary>
        public static Color Teal { get; } = Color.FromArgb(0xFF, 0x00, 0x80, 0x80);

        private const int ARGBAlphaShift = 24;
        private const int ARGBRedShift = 16;
        private const int ARGBGreenShift = 8;
        private const int ARGBBlueShift = 0;

        internal readonly long value;

        internal Color(long value) => this.value = value;

        /// <summary>Gets the red component of this color.</summary>
        public byte R => (byte)((this.value >> ARGBRedShift) & 0xFF);
        /// <summary>Gets the green component of this color.</summary>
        public byte G => (byte)((this.value >> ARGBGreenShift) & 0xFF);
        /// <summary>Gets the blue component of this color.</summary>
        public byte B => (byte)((this.value >> ARGBBlueShift) & 0xFF);
        /// <summary>Gets the alpha component of this color.</summary>
        public byte A => (byte)((this.value >> ARGBAlphaShift) & 0xFF);

        /// <summary>Gets a value indicating whether this color is empty.</summary>
        public bool IsEmpty => false;

        private string NameAndARGBValue => $"ARGB=({this.A}, {this.R}, {this.G}, {this.B})";

        /// <summary>Gets the hexadecimal string representation of this color's value.</summary>
        public string Name => this.value.ToString("x");

        private static long MakeArgb(byte alpha, byte red, byte green, byte blue) => (long)(unchecked((uint)(red << ARGBRedShift | green << ARGBGreenShift | blue << ARGBBlueShift | alpha << ARGBAlphaShift))) & 0xffffffff;

        /// <summary>Creates a color from a 32-bit ARGB value.</summary>
        public static Color FromArgb(int argb) => new Color(argb & 0xffffffff);
        /// <summary>Creates an opaque color from the given red, green, and blue components.</summary>
        public static Color FromArgb(int red, int green, int blue) => Color.FromArgb(255, red, green, blue);

        /// <summary>Creates a color from the given alpha, red, green, and blue components.</summary>
        public static Color FromArgb(int alpha, int red, int green, int blue) {
            if (alpha < 0 || alpha > 255) throw new ArgumentOutOfRangeException(nameof(alpha));
            if (red < 0 || red > 255) throw new ArgumentOutOfRangeException(nameof(red));
            if (green < 0 || green > 255) throw new ArgumentOutOfRangeException(nameof(green));
            if (blue < 0 || blue > 255) throw new ArgumentOutOfRangeException(nameof(blue));

            return new Color(Color.MakeArgb((byte)alpha, (byte)red, (byte)green, (byte)blue));
        }

        /// <summary>Creates a color from an existing color with the given alpha value.</summary>
        public static Color FromArgb(int alpha, Color baseColor) {
            if (alpha < 0 || alpha > 255) throw new ArgumentOutOfRangeException(nameof(alpha));

            return new Color(Color.MakeArgb(unchecked((byte)alpha), baseColor.R, baseColor.G, baseColor.B));
        }

        /// <summary>Gets the brightness (lightness) of this color as a value from 0 to 1.</summary>
        public float GetBrightness() {
            var r = (float)this.R / 255.0f;
            var g = (float)this.G / 255.0f;
            var b = (float)this.B / 255.0f;

            float max, min;

            max = r; min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            return (max + min) / 2;
        }

        /// <summary>Gets the hue of this color in degrees (0 to 360).</summary>
        public float GetHue() {
            if (this.R == this.G && this.G == this.B)
                return 0; // 0 makes as good an UNDEFINED value as any

            var r = this.R / 255.0f;
            var g = this.G / 255.0f;
            var b = this.B / 255.0f;

            float max, min;
            float delta;
            var hue = 0.0f;

            max = r; min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            delta = max - min;

            if (r == max) {
                hue = (g - b) / delta;
            }
            else if (g == max) {
                hue = 2 + (b - r) / delta;
            }
            else if (b == max) {
                hue = 4 + (r - g) / delta;
            }
            hue *= 60;

            if (hue < 0.0f) {
                hue += 360.0f;
            }
            return hue;
        }

        /// <summary>Gets the saturation of this color as a value from 0 to 1.</summary>
        public float GetSaturation() {
            var r = this.R / 255.0f;
            var g = this.G / 255.0f;
            var b = this.B / 255.0f;

            float max, min;
            float l, s = 0;

            max = r; min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            // if max == min, then there is no color and
            // the saturation is zero.
            //
            if (max != min) {
                l = (max + min) / 2;

                if (l <= .5) {
                    s = (max - min) / (max + min);
                }
                else {
                    s = (max - min) / (2 - max - min);
                }
            }
            return s;
        }

        /// <summary>Gets the 32-bit ARGB value of this color.</summary>
        public int ToArgb() => unchecked((int)this.value);
        internal int ToRgb() => unchecked((int)this.value) & 0x00FFFFFF;

        /// <summary>Returns a string describing this color's ARGB components.</summary>
        public override string ToString() {
            var sb = new StringBuilder(32);
            sb.Append(GetType().Name);
            sb.Append(" [");

            sb.Append("A=");
            sb.Append(this.A);
            sb.Append(", R=");
            sb.Append(this.R);
            sb.Append(", G=");
            sb.Append(this.G);
            sb.Append(", B=");
            sb.Append(this.B);

            sb.Append("]");

            return sb.ToString();
        }

        /// <summary>Determines whether two colors have the same ARGB value.</summary>
        public static bool operator ==(Color left, Color right) => left.value == right.value;
        /// <summary>Determines whether two colors have different ARGB values.</summary>
        public static bool operator !=(Color left, Color right) => !(left == right);

        /// <summary>Returns a hash code for this color.</summary>
        public override int GetHashCode() => this.value.GetHashCode();

        /// <summary>Determines whether the specified object is a color with the same ARGB value.</summary>
        //C# compiler crashes when using pattern matching
        public override bool Equals(object obj) {
            if (obj is Color)
                return this.value == ((Color)obj).value;

            return false;
        }

        /// <summary>Specifies the bits-per-pixel layout of color data.</summary>
        public enum ColorFormat {
            /// <summary>32 bits per pixel (8 bits each for alpha, red, green, blue).</summary>
            Rgb8888 = 0,
            /// <summary>24 bits per pixel (8 bits each for red, green, blue).</summary>
            Rgb888 = 1,
            /// <summary>16 bits per pixel (5 red, 6 green, 5 blue).</summary>
            Rgb565 = 2,
            /// <summary>12 bits per pixel (4 bits each for red, green, blue).</summary>
            Rgb444 = 3,
            /// <summary>8 bits per pixel (3 red, 3 green, 2 blue).</summary>
            Rgb332 = 4,
        }

        /// <summary>Specifies the channel ordering of color data.</summary>
        public enum RgbFormat {
            /// <summary>Red, green, blue order.</summary>
            Rgb = 0,
            /// <summary>Blue, green, red order.</summary>
            Bgr = 1,
            /// <summary>Green, red, green order.</summary>
            Grg = 2,
            /// <summary>Red, blue, green order.</summary>
            Rbg = 3
        }

        /// <summary>Specifies the bit ordering used when converting to 1 bit per pixel.</summary>
        public enum BitFormat {
            /// <summary>Bits are packed vertically.</summary>
            Vertical = 0,
            /// <summary>Bits are packed horizontally.</summary>
            Horizontal = 1
        }

        /// <summary>Converts color data from one format to another.</summary>
        public static void Convert(byte[] inArray, byte[] outArray, ColorFormat colorFormat) => Convert(inArray, outArray, colorFormat, RgbFormat.Rgb, 0, null);
        /// <summary>Converts color data using the specified color and channel formats.</summary>
        public static void Convert(byte[] inArray, byte[] outArray, ColorFormat colorFormat, RgbFormat rgbFormat) => Convert(inArray, outArray, colorFormat, rgbFormat, 0, null);
        /// <summary>Converts color data using the specified formats and alpha value.</summary>
        public static void Convert(byte[] inArray, byte[] outArray, ColorFormat colorFormat, RgbFormat rgbFormat, byte alpha) => Convert(inArray, outArray, colorFormat, rgbFormat, alpha, null);
        /// <summary>Converts color data using the specified formats, alpha value, and color table.</summary>
        public static void Convert(byte[] inArray, byte[] outArray, ColorFormat colorFormat, RgbFormat rgbFormat, byte alpha, byte[] colorTable) {
            if (inArray == null || outArray == null)
                throw new ArgumentNullException();

            NativeConvert(inArray, outArray, colorFormat, rgbFormat, alpha, colorTable);
        }

        /// <summary>Converts color data to a 1-bit-per-pixel monochrome representation.</summary>
        public static void ConvertTo1Bpp(byte[] inArray, byte[] outArray, uint width) => ConvertTo1Bpp(inArray, outArray, width, BitFormat.Vertical);
        /// <summary>Converts color data to a 1-bit-per-pixel monochrome representation using the given bit format.</summary>
        public static void ConvertTo1Bpp(byte[] inArray, byte[] outArray, uint width, BitFormat bitFormat) {
            if (inArray == null || outArray == null)
                throw new ArgumentNullException();

            NativeConvertTo1Bpp(inArray, outArray, bitFormat, width);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern static void NativeConvert(byte[] inArray, byte[] outArray, ColorFormat colorFormat, RgbFormat rgbFormat, byte alpha, byte[] colorTable);

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern static void NativeConvertTo1Bpp(byte[] inArray, byte[] outArray, BitFormat bitFormat, uint width);        
    }
}
