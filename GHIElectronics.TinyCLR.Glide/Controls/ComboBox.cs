
using System;
using System.Collections;
using GHIElectronics.TinyCLR.UI.Glide.Geom;
using System.Drawing;
using GHIElectronics.TinyCLR.UI.Glide.Ext;
using GHIElectronics.TinyCLR.UI.Glide;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.Glide.Properties;

namespace GHIElectronics.TinyCLR.UI.Controls
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    //  Copyright (c) GHI Electronics, LLC.
    //
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    /// <summary>
    /// THe Dropdown component contains a list of options in which a user can select one.
    /// </summary>
    public class ComboBox : ContentControl, IDisposable
    {
        private System.Drawing.Bitmap _DropdownText_Up = Resources.GetBitmap(Resources.BitmapResources.DropdownText_Up);
        private System.Drawing.Bitmap _DropdownText_Down = Resources.GetBitmap(Resources.BitmapResources.DropdownText_Down);
        private System.Drawing.Bitmap _DropdownButton_Up = Resources.GetBitmap(Resources.BitmapResources.DropdownButton_Up);
        private System.Drawing.Bitmap _DropdownButton_Down = Resources.GetBitmap(Resources.BitmapResources.DropdownButton_Down);
        private bool _pressed = false;
        private int _leftMargin = 10;
        private Glide.Geom.Rectangle _rect = new Glide.Geom.Rectangle();
        //private bool isListOpen = false;
        public Glide.Geom.Rectangle Rect
        {
            get
            {
                //_rect.X = X;
                //if (Parent != null)
                //    _rect.X += Parent.X;

                //_rect.Y = Y;
                //if (Parent != null)
                //    _rect.Y += Parent.Y;

                return _rect;
            }
        }
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
        /// <summary>
        /// Creates a new Dropdown.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="alpha">Alpha</param>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public ComboBox(string name, ushort alpha, int x, int y, int width, int height)
        {
            //ID = name;
            Alpha = alpha;
            X = 0;
            Y = 0;
            Width = width;
            Height = height;

            // Default
            Options = new ArrayList();
            Value = null;
            Text = String.Empty;
            Font = Resources.GetFont(Resources.FontResources.droid_reg12);
            FontColor = Media.Colors.Black;

            _rect.X = x;
            _rect.Y = y;
            _rect.Width = Width;
            _rect.Height = Height;
        }

        /// <summary>
        /// Value changed event.
        /// </summary>
        public event OnValueChanged ValueChangedEvent;

        /// <summary>
        /// Triggers a value changed event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        public void TriggerValueChangedEvent(object sender)
        {
            if (ValueChangedEvent != null)
                ValueChangedEvent(sender);
        }

        /// <summary>
        /// Renders the Dropdown onto it's parent container's graphics.
        /// </summary>
        public override void OnRender(DrawingContext dc)
        {
            int x = X;
            int y = Y;
            int textY = y + ((Height - Font.Height) / 2);
            ushort alpha = (IsEnabled) ? Alpha : (ushort)(Alpha / 3);

            if (_pressed)
            {
                textY++;
                dc.Scale9Image(x, y, Width, Height, BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(_DropdownText_Down )), 5, 5, 5, 5, alpha);
                dc.DrawImage(BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(_DropdownButton_Down)), (x + Width) - _DropdownButton_Down.Width, y, 0, 0, _DropdownButton_Down.Width, _DropdownButton_Down.Height);
            }
            else
            {
                dc.Scale9Image(x, y, Width, Height, BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(_DropdownText_Up)), 5, 5, 5, 5, alpha);
                dc.DrawImage(BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(_DropdownButton_Up)), (x + Width) - _DropdownButton_Up.Width, y, 0, 0, _DropdownButton_Up.Width, _DropdownButton_Up.Height);
            }
            var txt = Text;
            //Bitmaps.DT_AlignmentLeft
            dc.DrawText(ref txt, Font, FontColor, x + _leftMargin, textY, Width, Height, TextAlignment.Left, TextTrimming.WordEllipsis);
        }

        /// <summary>
        /// Handles the touch down event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        protected override void OnTouchDown(TouchEventArgs e)
        {
            var x = e.Touches[0].X;
            var y = e.Touches[0].Y;
            if (Rect.Contains(x, y))
            {
                _pressed = true;
                Invalidate();
                //e.StopPropagation();
            }
            var evt = new RoutedEvent("TouchDownEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
            var args = new RoutedEventArgs(evt, this);

            //this.Click?.Invoke(this, args);

            e.Handled = args.Handled;

            if (this.Parent != null)
                this.Invalidate();
            //return e;
        }

        /// <summary>
        /// Handles the touch up event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        protected override void OnTouchUp(TouchEventArgs e)
        {
            var x = e.Touches[0].X;
            var y = e.Touches[0].Y;
            if (Rect.Contains(x, y))
            {
                //if (!isListOpen)
                {
                    if (_pressed)
                    {

                        _pressed = false;
                        Invalidate();
                        TriggerTapEvent(this);
                        //isListOpen = true;


                        //e.StopPropagation();
                    }
                }
                //else
                {

                }
            }
            else
            {
                if (_pressed)
                {
                    _pressed = false;
                    Invalidate();
                }
            }
            //return e;
            var evt = new RoutedEvent("TouchUpEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
            var args = new RoutedEventArgs(evt, this);

            //this.Click?.Invoke(this, args);

            e.Handled = args.Handled;

            //this.isPressed = false;

            if (this.Parent != null)
                this.Invalidate();
        }

        /// <summary>
        /// Disposes all disposable objects in this object.
        /// </summary>
        public void Dispose()
        {
            _DropdownText_Up.Dispose();
            _DropdownText_Down.Dispose();
            _DropdownButton_Up.Dispose();
            _DropdownButton_Down.Dispose();
        }

        /// <summary>
        /// An array of objects that represent the options.
        /// </summary>
        public ArrayList Options { get; set; }

        /// <summary>
        /// Value of the selected option.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Font used by the text.
        /// </summary>
        public Font Font { get; set; }

        /// <summary>
        /// Font color.
        /// </summary>
        public Media.Color FontColor { get; set; }

        public void TriggerTapEvent(object sender)
        {
            if (TapEvent != null)
                TapEvent(sender);
        }

        public event OnTap TapEvent;
    }
}

