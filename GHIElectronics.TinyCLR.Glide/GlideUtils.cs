////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using GHIElectronics.TinyCLR.Glide;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Media;
using System;
using System.Collections;
using System.Drawing;
using GHIElectronics.TinyCLR.Glide.Ext;

namespace GHIElectronics.TinyCLR.Glide
{
    /// <summary>
    /// The Utils class contains utility methods that help other classes.
    /// </summary>
    public static class GlideUtils
    {
        /// <summary>
        /// The Convert class contains methods to convert objects.
        /// </summary>
        public static class Convert
        {
            /// <summary>
            /// Converts a hex color code into a Color object.
            /// </summary>
            /// <param name="hexCode">Hex color code.</param>
            /// <returns>Color object.</returns>
            public static GHIElectronics.TinyCLR.UI.Media.Color ToColor(string hexCode)
            {
                int c = System.Convert.ToInt32(hexCode, 16);
                var col= System.Drawing.Color.FromArgb((c >> 16) | (c & 0x00FF00) | ((c & 0x0000FF) << 16));
                var col2 = GHIElectronics.TinyCLR.UI.Media.Color.FromArgb(byte.MaxValue,col.R,col.G,col.B);
                return col2;
            }
            public static System.Drawing.Color ToColorDrawing(string hexCode)
            {
                int c = System.Convert.ToInt32(hexCode, 16);
                var col = System.Drawing.Color.FromArgb((c >> 16) | (c & 0x00FF00) | ((c & 0x0000FF) << 16));
                var col2 = System.Drawing.Color.FromArgb(byte.MaxValue, col.R, col.G, col.B);
                return col2;
            }
            /// <summary>
            /// Converts a string into a Font object.
            /// </summary>
            /// <param name="font">A number that represents the font.</param>
            /// <returns>Font object.</returns>
            public static Font ToFont(string font)
            {
                int fontType = System.Convert.ToInt32(font);
                return FontManager.GetFont((FontManager.FontType)fontType);
            }

            /// <summary>
            /// Converts a string into an alignment flag.
            /// </summary>
            /// <param name="alignment"></param>
            /// <returns></returns>
            public static uint ToAlignment(string alignment)
            {
                switch (alignment.ToUpper())
                {
                    case "LEFT":
                        return Bitmaps.DT_AlignmentLeft;
                    case "CENTER":
                        return Bitmaps.DT_AlignmentCenter;
                    case "RIGHT":
                        return Bitmaps.DT_AlignmentRight;
                    default:
                        return Bitmaps.DT_AlignmentRight;
                }
            }

            /// <summary>
            /// Converts a string into a horizontal alignment constant.
            /// </summary>
            /// <param name="alignment"></param>
            /// <returns></returns>
            public static TextAlignment ToHorizontalAlign(string alignment)
            {
                switch (alignment.ToUpper())
                {
                    case "LEFT":
                        return TextAlignment.Left;
                    case "CENTER":
                        return TextAlignment.Center;
                    case "RIGHT":
                        return TextAlignment.Right;
                    default:
                        return TextAlignment.Right;
                }
            }

            /// <summary>
            /// Converts a string into a horizontal alignment constant.
            /// </summary>
            /// <param name="alignment"></param>
            /// <returns></returns>
            public static VerticalAlignment ToVerticalAlign(string alignment)
            {
                if (alignment == null)
                    return VerticalAlignment.Top;

                switch (alignment.ToUpper())
                {
                    case "BOTTOM":
                        return VerticalAlignment.Bottom;
                    case "MIDDLE":
                        return VerticalAlignment.Center;
                    case "TOP":
                        return VerticalAlignment.Top;
                    default:
                        return VerticalAlignment.Top;
                }
            }
        }

        /// <summary>
        /// The Math class contains methods to perform math functions.
        /// </summary>
        public static class Math
        {
            /// <summary>
            /// Checks a value against minimum and maximum limits.
            /// </summary>
            /// <param name="value">Value to be checked.</param>
            /// <param name="min">Minimum value.</param>
            /// <param name="max">Maximum value.</param>
            /// <returns>The value limited by min and max.</returns>
            public static int MinMax(int value, int min, int max)
            {
                return System.Math.Max(min, System.Math.Min(max, value));
            }
        }

        /// <summary>
        /// The Debug class contains methods to assist in debugging.
        /// </summary>
        public static class Debug
        {
            /// <summary>
            /// Prints a labeled string and array data.
            /// </summary>
            /// <param name="label">label</param>
            /// <param name="array">array</param>
            public static void Print(string label, object[] array)
            {
                string str = String.Empty;
                for (int i = 0; i < array.Length; i++)
                    str += ((i > 0) ? ", " : "") + array[i].ToString();
                
                System.Diagnostics.Debug.WriteLine(label + " [ " + str + " ]");
            }
        }

        /// <summary>
        /// Simple class to assist in timing code execution.
        /// </summary>
        public static class Timer
        {
            /// <summary>
            /// Ticks
            /// </summary>
            public static long Ticks = 0;

            /// <summary>
            /// Start timing.
            /// </summary>
            public static void Start()
            {
                Ticks = DateTime.Now.Ticks;
            }

            /// <summary>
            /// Stop timing.
            /// </summary>
            /// <param name="label">This text will appear before the time when printed.</param>
            public static void Stop(string label)
            {
                if (Ticks > 0)
                {
                    Ticks = DateTime.Now.Ticks - Ticks;
                    System.Diagnostics.Debug.WriteLine(label + " - " + (Ticks / TimeSpan.TicksPerMillisecond) + "ms");
                }
                else
                    System.Diagnostics.Debug.WriteLine(label + " - ERROR (Ticks equal zero)");
            }
        }
    }
}
