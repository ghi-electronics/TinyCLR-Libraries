////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using GHIElectronics.TinyCLR.Glide.Geom;

namespace GHIElectronics.TinyCLR.Glide.Display
{
    /// <summary>
    /// The DisplayObject is the base class for all objects.
    /// </summary>
    public class DisplayObject
    {
        private int _width = 0;
        private int _height = 0;
        private Rectangle _rect = new Rectangle();

        /// <summary>
        /// Indicates the instance name of the DisplayObject.
        /// </summary>
        public string Name;

        /// <summary>
        /// Indicates the alpha transparency value of the object specified.
        /// </summary>
        /// <remarks>Valid values are 0 (fully transparent) and 255 (fully opaque). Default value is 255.</remarks>
        public ushort Alpha = 255;

        /// <summary>
        /// Indicates whether or not the DisplayObject is visible.
        /// </summary>
        /// <remarks>Invisible objects are not drawn nor do they receive touch events.</remarks>
        public bool Visible = true;

        /// <summary>
        /// Indicates whether or not the DisplayObject is enabled.
        /// </summary>
        /// <remarks>Disabled objects do not receive touch events.</remarks>
        public bool Enabled = true;

        /// <summary>
        /// Indicates whether or not the DisplayObject is interactive.
        /// </summary>
        /// <remarks>Non-interactive objects do not receive touch events. This allows disabled objects to keep their state.</remarks>
        public bool Interactive = true;

        /// <summary>
        /// Object that contains data about the display object.
        /// </summary>
        public object Tag = null;

        

        

    }
}
