using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Drawing {
    [Serializable(), DebuggerDisplay("{NameAndARGBValue}")]
    public struct Color {
        public static readonly Color Empty = new Color();

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

        private const int ARGBAlphaShift = 24;
        private const int ARGBRedShift = 16;
        private const int ARGBGreenShift = 8;
        private const int ARGBBlueShift = 0;

        internal readonly long value;

        internal Color(long value) => this.value = value;

        public byte R => (byte)((this.value >> ARGBRedShift) & 0xFF);
        public byte G => (byte)((this.value >> ARGBGreenShift) & 0xFF);
        public byte B => (byte)((this.value >> ARGBBlueShift) & 0xFF);
        public byte A => (byte)((this.value >> ARGBAlphaShift) & 0xFF);

        public bool IsEmpty => false;

        private string NameAndARGBValue => $"ARGB=({this.A}, {this.R}, {this.G}, {this.B})";

        public string Name => this.value.ToString("x");

        private static long MakeArgb(byte alpha, byte red, byte green, byte blue) => (long)(unchecked((uint)(red << ARGBRedShift | green << ARGBGreenShift | blue << ARGBBlueShift | alpha << ARGBAlphaShift))) & 0xffffffff;

        public static Color FromArgb(int argb) => new Color(argb & 0xffffffff);
        public static Color FromArgb(int red, int green, int blue) => Color.FromArgb(255, red, green, blue);

        public static Color FromArgb(int alpha, int red, int green, int blue) {
            if (alpha < 0 || alpha > 255) throw new ArgumentOutOfRangeException(nameof(alpha));
            if (red < 0 || red > 255) throw new ArgumentOutOfRangeException(nameof(red));
            if (green < 0 || green > 255) throw new ArgumentOutOfRangeException(nameof(green));
            if (blue < 0 || blue > 255) throw new ArgumentOutOfRangeException(nameof(blue));

            return new Color(Color.MakeArgb((byte)alpha, (byte)red, (byte)green, (byte)blue));
        }

        public static Color FromArgb(int alpha, Color baseColor) {
            if (alpha < 0 || alpha > 255) throw new ArgumentOutOfRangeException(nameof(alpha));

            return new Color(Color.MakeArgb(unchecked((byte)alpha), baseColor.R, baseColor.G, baseColor.B));
        }

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

        public int ToArgb() => unchecked((int)this.value);
        internal int ToRgb() => unchecked((int)this.value) & 0x00FFFFFF;

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

        public static bool operator ==(Color left, Color right) => left.value == right.value;
        public static bool operator !=(Color left, Color right) => !(left == right);

        public override int GetHashCode() => this.value.GetHashCode();

        //C# compiler crashes when using pattern matching
        public override bool Equals(object obj) {
            if (obj is Color)
                return this.value == ((Color)obj).value;

            return false;
        }

        public enum ColorFormat {
            Rgb8888 = 0,
            Rgb888 = 1,
            Rgb565 = 2,
            Rgb444 = 3,
            Rgb332 = 4,
        }

        public enum RgbFormat {
            Rgb = 0,
            Bgr = 1,
            Grg = 2,
            Rbg = 3
        }

        public enum BitFormat {
            Vertical = 0,
            Horizontal = 1
        }

        public static void Convert(byte[] inArray, byte[] outArray, ColorFormat colorFormat) => Convert(inArray, outArray, colorFormat, RgbFormat.Rgb, 0, null);
        public static void Convert(byte[] inArray, byte[] outArray, ColorFormat colorFormat, RgbFormat rgbFormat) => Convert(inArray, outArray, colorFormat, rgbFormat, 0, null);
        public static void Convert(byte[] inArray, byte[] outArray, ColorFormat colorFormat, RgbFormat rgbFormat, byte alpha) => Convert(inArray, outArray, colorFormat, rgbFormat, alpha, null);
        public static void Convert(byte[] inArray, byte[] outArray, ColorFormat colorFormat, RgbFormat rgbFormat, byte alpha, byte[] colorTable) {
            if (inArray == null || outArray == null)
                throw new ArgumentNullException();

            NativeConvert(inArray, outArray, colorFormat, rgbFormat, alpha, colorTable);
        }

        public static void ConvertTo1Bpp(byte[] inArray, byte[] outArray, uint width) => ConvertTo1Bpp(inArray, outArray, width, BitFormat.Vertical);
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
