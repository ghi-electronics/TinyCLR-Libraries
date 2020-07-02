////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;
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
    /// <summary>
    /// The list component provides a list of options.
    /// </summary>
    public class List : ContentControl,IDisposable
    {
        /// <summary>
     /// Indicates the x coordinate of the DisplayObject instance relative to the local coordinates of the parent DisplayObjectContainer.
     /// </summary>
        public int X;

        /// <summary>
        /// Indicates the y coordinate of the DisplayObject instance relative to the local coordinates of the parent DisplayObjectContainer.
        /// </summary>
        public int Y;
        private Glide.Geom.Rectangle _rect = new Glide.Geom.Rectangle();
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

        public int LCDWidth;
        public int LCDHeight;
        public int AvailableHeight;
        private Glide.Geom.Rectangle ButtonUp;
        private Glide.Geom.Rectangle ButtonDown;
        private int PageIndex;
        private int PageSize;
        private double MaxPage;
        /// <summary>
        /// Indicates the alpha transparency value of the object specified.
        /// </summary>
        /// <remarks>Valid values are 0 (fully transparent) and 255 (fully opaque). Default value is 255.</remarks>
        const int ButtonWidth = 50;
        public ushort Alpha = 255;
        private GlideGraphics _bitmap;
        private int _renderedWithNumChildren = 0;
        private int _lastPressY;
        private int _lastListY;
        private int _listY = 0;
        private int _listMaxY;
        private bool _pressed = false;
        private bool _moving = false;
        private int _ignoredTouchMoves = 0;
        private int _maxIgnoredTouchMoves = 1;
        int firstHeight;
        ArrayList Items;
        Canvas parent;
        Hashtable Children = new Hashtable();
        /// <summary>
        /// Creates a new List component.
        /// </summary>
        /// <param name="options">Array of options.</param>
        /// <param name="width">Width</param>
        /// <remarks>The list cannot be smaller than 100 or greater than the LCD size. We recommend keeping the size to a minimum; only use what you need.</remarks>
        public List(ArrayList options, int width, int LcdWidth, int LcdHeight, Window mainWindow)
        {
            Items = options;
            this.LCDHeight = LcdHeight;
            this.LCDWidth = LcdWidth;
            object[] option;
            ListItem item;
            firstHeight = 0;
            for (int i = 0; i < options.Count; i++)
            {
                option = (object[])options[i];
                item = new ListItem(this, option[0].ToString(), option[1]);
               
                item.Y = i * item.Height;
                item.ID = "listItem_"+i;
                if(mainWindow!=null && mainWindow.Child != null)
                {
                    parent = mainWindow.Child as Canvas;
                    item.Width = LcdWidth/2;
                    Children.Add(item.ID,item);
                }
                if (i == 0) firstHeight = item.Height;
               
            }

            int itemHeight = firstHeight;
            int numItems = (int)(Math.Floor((double)LCDHeight / itemHeight)) - 1;
            PageSize = numItems;
            PageIndex = 0;
            MaxPage = Math.Ceiling( (double)NumChildren / PageSize);
            //ID = "list";
            if (width < 100)
                width = 100;
            else if (width > LcdWidth)
                width = LcdWidth;
            Width = width;
            Height = numItems * itemHeight;
            //Height = NumChildren * itemHeight;
          
            _rect.X = (LcdWidth - Width) / 2;
            _rect.Y = (LcdHeight - Height) / 2;
            _rect.Width = Width;
            _rect.Height = Height;

            ButtonUp = new Glide.Geom.Rectangle(_rect.X+_rect.Width,_rect.Y,ButtonWidth,_rect.Height/2);
            ButtonDown = new Glide.Geom.Rectangle(_rect.X + _rect.Width, _rect.Y + (_rect.Height / 2), ButtonWidth, _rect.Height / 2);
            Width += ButtonWidth;
            _font = Resources.GetFont(Resources.FontResources.droid_reg12);

            X = 0;
            Y = 0;

            Canvas.SetLeft(this, _rect.X);
            Canvas.SetTop(this, _rect.Y);
        }
       
        public int NumChildren
        {
            get
            {
                return Items.Count;
            }
        }
        /// <summary>
        /// Tap option event.
        /// </summary>
        public event OnTapOption TapOptionEvent;

        /// <summary>
        /// Triggers a tap option event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        /// <param name="args">Tap option event arguments.</param>
        internal virtual void TriggerTapOptionEvent(object sender, TapOptionEventArgs args)
        {
            if (TapOptionEvent != null)
                TapOptionEvent(sender, args);
        }
        ListItem GetItem(int index)
        {
            var ctl = "listItem_" + index;
            if(Children.Contains(ctl))
                return Children[ctl] as ListItem;
            return null;
            
        }
        /// <summary>
        /// Close event.
        /// </summary>
        public event OnClose CloseEvent;

        /// <summary>
        /// Triggers a close event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        public void TriggerCloseEvent(object sender)
        {
            if (CloseEvent != null)
                CloseEvent(sender);
        }

        /// <summary>
        /// Renders the List onto it's parent container's graphics.
        /// </summary>
        public override void OnRender(DrawingContext dc)
        {
           
            // Only render the child bitmap if children change
            if (NumChildren > 0) // && _renderedWithNumChildren < NumChildren
            {
                //_renderedWithNumChildren = NumChildren;
                var start = PageIndex * PageSize;
                var end = (PageIndex + 1) * PageSize;
                if (end > NumChildren) end = NumChildren;
                _bitmap = new GlideGraphics(_rect.Width, (end - start) * firstHeight);
                _bitmap.DrawRectangle(new Glide.Geom.Rectangle() {X = 0, Y = 0,  Width= _bitmap.GetBitmap().Width,Height= _bitmap.GetBitmap().Height }, System.Drawing.Color.White,100); //Glide.Ext.Colors.White,1, 0, 0, _bitmap.GetBitmap().Width, _bitmap.GetBitmap().Height, 0, 0, Glide.Ext.Colors.White, 0, 0, Glide.Ext.Colors.White, 0, 0, Alpha);

                int counter = 0;
                for (int i = start; i < end; i++)
                {
                    var item = GetItem(i);
                    item.Render(_bitmap, counter * item.Height);
                    counter++;
                }

                _listMaxY = _bitmap.GetBitmap().Height - Height;
            }
            Media.Brush brush = new SolidColorBrush(Media.Colors.Black);
            var pen = new Media.Pen(Media.Colors.Black, 1);
          
            //dc.DrawRectangle(brush,pen, 0, 0, LCDWidth, LCDHeight);
            dc.DrawImage(BitmapImage.FromGraphics(System.Drawing.Graphics.FromImage(_bitmap.GetBitmap())),X, Y, 0, _listY, _rect.Width, Height);
            Media.Brush buttonBrush = new SolidColorBrush(Media.Colors.Gray);
            //draw button up and down
            dc.DrawRectangle(buttonBrush, pen, _rect.Width, 0, ButtonUp.Width, ButtonUp.Height);
            dc.DrawRectangle(buttonBrush, pen, _rect.Width, ButtonUp.Height, ButtonDown.Width, ButtonDown.Height);
            dc.DrawText("Up", _font, Media.Colors.White, _rect.Width+5, ButtonUp.Height / 2);
            dc.DrawText("Down", _font, Media.Colors.White, _rect.Width+5, (ButtonUp.Height) + (ButtonDown.Height / 2));


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
            if (Rect.Contains(x,y) || ButtonUp.Contains(x, y) || ButtonDown.Contains(x, y))
            {
                //e.StopPropagation();
                _lastPressY = y;
                _lastListY = _listY;
                _pressed = true;
            }

            //return e;
            var evt = new RoutedEvent("TouchDownEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
            var args = new RoutedEventArgs(evt, this);

            //this.Click?.Invoke(this, args);

            e.Handled = args.Handled;

            if (this.Parent != null)
                this.Invalidate();
        }

        /// <summary>
        /// Handles the touch up event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        protected override void OnTouchUp(TouchEventArgs e)
        {
            var AY = Rect.Y;
            var x = e.Touches[0].X;
            var y = e.Touches[0].Y;
            if (!_pressed)
            { 
                TriggerCloseEvent(this);
                _ignoredTouchMoves = 0;
                return;
            }
            if (ButtonUp.Contains(x, y))
            {
                //up clicked
                if (PageIndex > 0)
                {
                    PageIndex--;
                }

            }else if (ButtonDown.Contains(x, y))
            {
                //down clicked
                if (PageIndex < MaxPage)
                {
                    PageIndex++;
                }
            }
            else if (Rect.Contains(x, y))
            {


                //if (!_moving)
                {
                    int index = (PageSize * PageIndex) + (int)System.Math.Floor((double)((_listY + y) - AY) / firstHeight);
                    if (index >= 0 && index < this.NumChildren)
                    {
                        ListItem option = GetItem(index);
                        TriggerTapOptionEvent(this, new TapOptionEventArgs(index, option.Label, option.Value));
                    }
                }
            }
            //else
            //    _moving = false;

            _pressed = false;
            _ignoredTouchMoves = 0;
            //e.StopPropagation();
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
        /// Handles the touch move event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        //protected override void OnTouchMove(TouchEventArgs e)
        //{
        //    /*
        //    var x = e.Touches[0].X;
        //    var y = e.Touches[0].Y;
        //    // The pressed state only triggers when touches occur within this UI element's boundaries.
        //    // This check guarantees whether we need to process move events or not.
        //    if (!_pressed)
        //        return;

        //    if (!_moving)
        //    {
        //        if (_ignoredTouchMoves < _maxIgnoredTouchMoves)
        //            _ignoredTouchMoves++;
        //        else
        //            _moving = true;
        //    }
        //    else
        //    {
        //        int dragDistance = y - _lastPressY;
        //        _listY = _lastListY - dragDistance;
        //        _listY = GlideUtils.Math.MinMax(_listY, 0, _listMaxY);

        //        //Graphics.DrawImage(X, Y, _bitmap.GetBitmap(), 0, _listY, Width, Height);
        //        //Glide.Flush(this);
        //    }
        //    */

        //    //e.StopPropagation();
        //    //return e;
        //    var evt = new RoutedEvent("TouchMoveEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
        //    var args = new RoutedEventArgs(evt, this);

        //    //this.Click?.Invoke(this, args);

        //    e.Handled = args.Handled;

        //    //this.isPressed = false;

        //    if (this.Parent != null)
        //        this.Invalidate();
        //}

        /// <summary>
        /// Disposes all disposable objects in this object.
        /// </summary>
        public void  Dispose()
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }
    }
}
