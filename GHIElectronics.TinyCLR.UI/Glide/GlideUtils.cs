////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Drawing;
using GHIElectronics.TinyCLR.UI.Glide.Ext;

namespace GHIElectronics.TinyCLR.UI.Glide
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
            public static System.Drawing.Color ToColor(string hexCode)
            {
                int c = System.Convert.ToInt32(hexCode, 16);
                var col= System.Drawing.Color.FromArgb((c >> 16) | (c & 0x00FF00) | ((c & 0x0000FF) << 16));
                col = System.Drawing.Color.FromArgb(byte.MaxValue,col.R,col.G,col.B);
                return col;
            }
            /*
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
            */
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
                        return Bitmap.DT_AlignmentLeft;
                    case "CENTER":
                        return Bitmap.DT_AlignmentCenter;
                    case "RIGHT":
                        return Bitmap.DT_AlignmentRight;
                    default:
                        return Bitmap.DT_AlignmentRight;
                }
            }

            /// <summary>
            /// Converts a string into a horizontal alignment constant.
            /// </summary>
            /// <param name="alignment"></param>
            /// <returns></returns>
            public static HorizontalAlignment ToHorizontalAlign(string alignment)
            {
                switch (alignment.ToUpper())
                {
                    case "LEFT":
                        return HorizontalAlignment.Left;
                    case "CENTER":
                        return HorizontalAlignment.Center;
                    case "RIGHT":
                        return HorizontalAlignment.Right;
                    default:
                        return HorizontalAlignment.Right;
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
