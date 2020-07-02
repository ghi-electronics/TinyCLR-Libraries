using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Glide.Ext
{
    public class Colors
    {
        public static Color Transparent { get; } = Color.FromArgb(0, (int)byte.MaxValue, (int)byte.MaxValue, (int)byte.MaxValue);

        public static Color Black { get; } = Color.FromArgb((int)byte.MaxValue, 0, 0, 0);

        public static Color White { get; } = Color.FromArgb((int)byte.MaxValue, (int)byte.MaxValue, (int)byte.MaxValue, (int)byte.MaxValue);

        public static Color Gray { get; } = Color.FromArgb((int)byte.MaxValue, 128, 128, 128);

        public static Color Red { get; } = Color.FromArgb((int)byte.MaxValue, (int)byte.MaxValue, 0, 0);

        public static Color Green { get; } = Color.FromArgb((int)byte.MaxValue, 0, 128, 0);

        public static Color Blue { get; } = Color.FromArgb((int)byte.MaxValue, 0, 0, (int)byte.MaxValue);

        public static Color Yellow { get; } = Color.FromArgb((int)byte.MaxValue, (int)byte.MaxValue, (int)byte.MaxValue, 0);

        public static Color Purple { get; } = Color.FromArgb((int)byte.MaxValue, 128, 0, 128);

        public static Color Teal { get; } = Color.FromArgb((int)byte.MaxValue, 0, 128, 128);

        public static readonly Color Brown = System.Drawing.Color.FromArgb(2763429);
        public static readonly Color Cyan = System.Drawing.Color.FromArgb(16776960);
        public static readonly Color DarkGray = System.Drawing.Color.FromArgb(11119017);

        public static readonly Color LightGray = System.Drawing.Color.FromArgb(13882323);
        public static readonly Color Magenta = System.Drawing.Color.FromArgb(16711935);
        public static readonly Color Orange = System.Drawing.Color.FromArgb(42495);
        

        public static readonly Color Fuchsia = System.Drawing.Color.FromArgb(255, 0, 255);
        public static uint ToNativeColor(Color x)
        {
            return (uint)((x.R | (x.G << 8)) | (x.B << 0x10));
        }

        public static ushort ToNativeAlpha(Color x)
        {
            return x.A;
        }
    }
}
