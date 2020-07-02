////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using GHIElectronics.TinyCLR.Glide.Geom;
using System.Drawing;
using GHIElectronics.TinyCLR.Glide.Properties;

namespace GHIElectronics.TinyCLR.Glide
{
    /// <summary>
    /// Manages font-related functionality.
    /// </summary>
    public static class FontManager
    {
        /// <summary>
        /// Different types of fonts/sizes.
        /// </summary>
        public enum FontType
        {
            /// <summary>
            /// Droid Sans Regular 8
            /// </summary>
            droid_reg08,

            /// <summary>
            /// Droid Sans Regular 9
            /// </summary>
            droid_reg09,

            /// <summary>
            /// Droid Sans Regular 10
            /// </summary>
            droid_reg10,

            /// <summary>
            /// Droid Sans Regular 11
            /// </summary>
            droid_reg11,

            /// <summary>
            /// Droid Sans Regular 12
            /// </summary>
            droid_reg12,

            /// <summary>
            /// Droid Sans Regular 14
            /// </summary>
            droid_reg14,

            /// <summary>
            /// Droid Sans Regular 18
            /// </summary>
            droid_reg18,

            /// <summary>
            /// Droid Sans Regular 24
            /// </summary>
            droid_reg24,

            /// <summary>
            /// Droid Sans Regular 32
            /// </summary>
            droid_reg32,

            /// <summary>
            /// Droid Sans Regular 48
            /// </summary>
            droid_reg48,
        }

        /// <summary>
        /// Returns a font resource specified by a font type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Font GetFont(FontType type)
        {
            switch (type)
            {
                case FontType.droid_reg08:
                    return Resources.GetFont(Resources.FontResources.droid_reg08);

                case FontType.droid_reg09:
                    return Resources.GetFont(Resources.FontResources.droid_reg09);

                case FontType.droid_reg10:
                    return Resources.GetFont(Resources.FontResources.droid_reg10);

                case FontType.droid_reg11:
                    return Resources.GetFont(Resources.FontResources.droid_reg11);

                case FontType.droid_reg12:
                    return Resources.GetFont(Resources.FontResources.droid_reg12);

                case FontType.droid_reg14:
                    return Resources.GetFont(Resources.FontResources.droid_reg14);

                case FontType.droid_reg18:
                    return Resources.GetFont(Resources.FontResources.droid_reg18);

                case FontType.droid_reg24:
                    return Resources.GetFont(Resources.FontResources.droid_reg24);

                case FontType.droid_reg32:
                    return Resources.GetFont(Resources.FontResources.droid_reg32);

                case FontType.droid_reg48:
                    return Resources.GetFont(Resources.FontResources.droid_reg48);

                default:
                    throw new ArgumentException("No such font exists.");
            }
        }

        /// <summary>
        /// Returns a Rectangle object the same size of a string.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Geom.Rectangle GetRect(Font font, string str)
        {
            //int width = 0, height = 0;
            var calc = Glide.screen.MeasureString(str, font);
            //font.ComputeExtent(str, out width, out height);
            return new Geom.Rectangle(0, 0,(int) calc.Width, (int)calc.Height);
        }

        /// <summary>
        /// Returns the size of a string.
        /// </summary>
        /// <param name="font"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Size GetSize(Font font, string str)
        {
            //int width = 0, height = 0;
            //font.ComputeExtent(str, out width, out height);
            var calc = Glide.screen.MeasureString(str, font);
            return new Size { Width = (int)calc.Width, Height=(int)calc.Height} ;
        }

    }
}

