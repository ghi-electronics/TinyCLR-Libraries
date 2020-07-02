////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace GHIElectronics.TinyCLR.Glide.Geom
{
    /// <summary>
    /// The Point object represents a location in a two-dimensional coordinate system.
    /// </summary>
    [Serializable]
    public struct Point
    {
        /// <summary>
        /// The horizontal coordinate of the point.
        /// </summary>
        public int X;

        /// <summary>
        /// The vertical coordinate of the point.
        /// </summary>
        public int Y;

        /// <summary>
        /// Creates a new Point.
        /// </summary>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Formats the point as a string for debugging.
        /// </summary>
        /// <returns>The point as a string. E.g. [100, 100]</returns>
        public override string ToString()
        {
            return "[" + X + ", " + Y + "]";
        }
    }
}