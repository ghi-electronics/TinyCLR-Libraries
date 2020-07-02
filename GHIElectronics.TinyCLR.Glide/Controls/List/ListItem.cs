////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using GHIElectronics.TinyCLR.Glide.Properties;
using GHIElectronics.TinyCLR.UI.Glide;
using GHIElectronics.TinyCLR.UI.Glide.Ext;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls
{
    /// <summary>
    /// The ListItem class holds information relevant to a specific option in a list-based component.
    /// </summary>
    public class ListItem : IListItem
    {
        public string ID;
        List Parent;
        public int Width;
        public int Height;
        /// <summary>
        /// Indicates the x coordinate of the DisplayObject instance relative to the local coordinates of the parent DisplayObjectContainer.
        /// </summary>
        public int X;

        /// <summary>
        /// Indicates the y coordinate of the DisplayObject instance relative to the local coordinates of the parent DisplayObjectContainer.
        /// </summary>
        public int Y;

        /// <summary>
        /// Indicates the alpha transparency value of the object specified.
        /// </summary>
        /// <remarks>Valid values are 0 (fully transparent) and 255 (fully opaque). Default value is 255.</remarks>
        public ushort Alpha = 255;
        private Font _font = Resources.GetFont( Resources.FontResources.droid_reg12);

        /// <summary>
        /// Creates a new ListItem.
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="value">Value</param>
        public ListItem(List Parent, string label, object value)
        {
            this.Parent = Parent;
            Label = label;
            Value = value;
            Height = 32;
            Width = 100;
           
        }

        /// <summary>
        /// Renders the ListItem onto the provided bitmap.
        /// </summary>
        /// <param name="bitmap">Bitmap this item will be drawn on.</param>
        public void Render(GlideGraphics bitmap)
        {
           
            Width = Parent.Rect.Width;
            //Bitmaps.DT_AlignmentCenter
            bitmap.DrawTextInRect(Label, X, Y + (Height - _font.Height) / 2, Width, _font.Height, new StringFormat() { Alignment = StringAlignment.Center }, Glide.Ext.Colors.Black, _font);
            bitmap.DrawLine(System.Drawing.Color.Gray, 1, 0, Y + Height, Width, Y + Height);

        }
        public void Render(GlideGraphics bitmap, int Ay)
        {

            Width = Parent.Rect.Width;
            //Bitmaps.DT_AlignmentCenter
            bitmap.DrawTextInRect(Label, X, Ay + (Height - _font.Height) / 2, Width, _font.Height, new StringFormat() { Alignment = StringAlignment.Center }, Glide.Ext.Colors.Black, _font);
            bitmap.DrawLine(System.Drawing.Color.Gray, 1, 0, Ay + Height, Width, Ay + Height);

        }
        /// <summary>
        /// A string of text that describes this item.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public object Value { get; set; }
    }
}
