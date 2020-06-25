////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace GHIElectronics.TinyCLR.UI.Glide.Geom
{
    /// <summary>
    ///  The Size struct indicates the width and height of an object.
    /// </summary>
    public struct Size
    {
        /// <summary>
        /// The width of an object, in pixels.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of an object, in pixels.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Formats the size as a string for debugging.
        /// </summary>
        /// <returns>The size as a string. E.g. [100 x 100]</returns>
        public override string ToString()
        {
            return "[" + Width + " x " + Height + "]";
        }
    }
}
