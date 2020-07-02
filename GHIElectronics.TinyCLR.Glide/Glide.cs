////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Drawing;
using GHIElectronics.TinyCLR.Glide.Geom;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.Glide.Properties;
using GHIElectronics.TinyCLR.UI.Glide;

namespace GHIElectronics.TinyCLR.Glide
{
    /// <summary>
    /// The Glide class provides core functionality.
    /// </summary>
    public static class Glide
    {
        private static Size screenSize;
        internal static System.Drawing.Graphics screen;
        private static GlideWindow _mainWindow;
        public static IntPtr Hdc;
        private static ComboBox _dropdown;
        private static List _list;
        const string listName = "list";
        
        //private static ListBox _list;
        //private static ScrollViewer _viewer;
        //private static StackPanel _panel;
        static Hashtable listOfDisableControl;
        static Glide()
        {
            
        }
        public static UIElement GetChildByName(string ComponentID)
        {
            if (_mainWindow != null)
            {
                var mainCanvas = _mainWindow.Child as Canvas;
                if (mainCanvas != null)
                {
                    /*
                    foreach (UIElement component in mainCanvas.Children)
                    {
                        if (component.ID == ComponentID)
                        {
                            return component;
                        }
                    }*/
                    return MainWindow.GetControl(ComponentID);
                }
            }
            return null;
        }

        public static bool AddChildToMainWindow(UIElement element)
        {
            if (_mainWindow != null)
            {
                var mainCanvas = _mainWindow.Child as Canvas;
                if (mainCanvas != null)
                {
                    mainCanvas.Children.Add(element);
                    return true;
                }
            }
            return false;
        }

        public static bool RemoveChildByName(string ComponentID)
        {
            if (_mainWindow != null)
            {
                var mainCanvas = _mainWindow.Child as Canvas;
                if (mainCanvas != null)
                {
                    /*
                    foreach (UIElement component in mainCanvas.Children)
                    {
                        if (component.ID == ComponentID)
                        {
                            mainCanvas.Children.Remove(component);
                            return true;
                        }
                    }*/
                    var comp = MainWindow.GetControl(ComponentID);
                    if (comp != null) {
                        mainCanvas.Children.Remove(comp);
                        return true;
                    }
                }
            }
            return false;
        }
        public static void SetupGlide(int width,int height, int bitsPerPixel, int orientationDeg, DisplayController displayController)
        {
            Hdc = displayController.Hdc;
            LCD = new Size() { Width = width, Height = height };
            screen = System.Drawing.Graphics.FromHdc(Hdc);// new (width, height);
            IsEmulator = false;
            FitToScreen = false;

            // Show loading
            System.Drawing.Bitmap loading = Resources.GetBitmap(Resources.BitmapResources.loading);

            screen.DrawImage(loading, (LCD.Width - loading.Width) / 2, (LCD.Height - loading.Height) / 2, loading.Width, loading.Height);
            screen.Flush();
        }
        /// <summary>
        /// Returns the screen resolution.
        /// </summary>
        public static Size LCD
        {
            get
            {
                return Glide.screenSize;
            }
            private set
            {
                Glide.screenSize = value;
            }
        }

        /// <summary>
        /// Returns a reference to the bitmap that represents the current screen.
        /// This is only useful for drawing the bitmap to a display that does not
        /// support bitmap.flush().
        /// </summary>
        public static System.Drawing.Graphics Screen
        {
            get
            {
                return Glide.screen;
            }
        }

        /// <summary>
        /// This method changes the underlying size of the bitmap that is drawn to
        /// the screen. Do not call this method if you are using a regular display.
        /// It is only useful when you are using a non-native display such as a
        /// SPI display like our DisplayN18.
        /// </summary>
        /// <param name="width">The width of the display.</param>
        /// <param name="height">The height of the display.</param>
        public static void SetScreenSize(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Width and height must be positive.");

            Glide.screenSize.Width = width;
            Glide.screenSize.Height = height;
                 
            Glide.screen.Dispose();
            Glide.screen = System.Drawing.Graphics.FromHdc(Hdc); 
                 
          
        }


        public static void Flush(int x, int y, int width, int height)
        {
            // Ignore flushes if no MainWindow is set.
            if (_mainWindow == null)
                return;
            _mainWindow.Invalidate();
            //GlideX.screen.Flush();
        }

       

        /// <summary>
        /// Flushes the rectangular area to the screen.
        /// </summary>
        /// <param name="rect">Rectangle</param>
        public static void Flush(Geom.Rectangle rect)
        {
            Flush(rect.X, rect.Y, rect.Width, rect.Height);
        }

     

        /// <summary>
        /// Indicates whether or not we're using the emulator.
        /// </summary>
        public static bool IsEmulator;

        /// <summary>
        /// Indicates whether or not to resize windows to the LCD's resolution.
        /// </summary>
        /// <remarks>This does not affect component placement. They will remain in their assigned position.</remarks>
        public static bool FitToScreen;

       
        public static GlideWindow MainWindow
        {
            get { return _mainWindow; }
            set
            {
               

                // Change to the new window
                _mainWindow = value;

                // Begin handling events
                if (_mainWindow != null)
                { 
                    // Call render after because windows only flush if they're handling events
                    _mainWindow.Invalidate();
                }
               
            }
        }
        /// <summary>
        /// Opens a List component.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        /// <param name="list">List component that needs to be opened.</param>
        public static void OpenList(object sender, List list)
        {
            if (_list == null)
            {
                _dropdown = (ComboBox)sender;
                _list = list;
                //_list.SelectionChanged += _list_SelectionChanged;
                _list.TapOptionEvent += new OnTapOption(list_TapOptionEvent);
                if (listOfDisableControl == null) listOfDisableControl = new Hashtable();
                listOfDisableControl.Clear();
                //for (int i = 0; i < MainWindow.NumChildren; i++)
                //    MainWindow[i].Interactive = false;
                if (_mainWindow != null)
                {
                    var mainCanvas = _mainWindow.Child as Canvas;
                    if (mainCanvas != null)
                    {
                        /*
                        foreach (UIElement component in mainCanvas.Children)
                        {
                            if (component.Visibility == Visibility.Visible)
                            {
                                listOfDisableControl.Add(component.ID, component);
                                component.Visibility = Visibility.Hidden;
                            }

                        }*/
                        foreach (var comp in MainWindow.GetAllControls()) {
                            var component = comp.Element;
                            if (component.Visibility == Visibility.Visible) {
                                listOfDisableControl.Add(comp.ElementName, component);
                                component.Visibility = Visibility.Hidden;
                            }

                        }
                    }
                }
                /*
                if (_viewer == null)
                {
                    //_panel = new StackPanel() { Orientation= Orientation.Vertical };
                    _viewer = new ScrollViewer() { ID = "_mainListViewer", Child = _list };
                }
                else
                {
                    _viewer.Child = _list;
                }*/
                //_panel.Children.Clear();
                //_panel.Children.Add(_list);
                //set scroll position
                //if (Width < 100)
                //    Width = 100;
                //else if (Width > LCDWidth)
                //    Width = LCDWidth;
                //var itemHeight = 32;
                //int numItems = (int)(System.Math.Floor(LCDHeight / itemHeight)) - 1;
                //var AvailableHeight = numItems * itemHeight;
                //var Height = list.Items.Count * itemHeight;
                //_list.Width = Width; //(LcdWidth - Width) / 2;
                //_list.Height = AvailableHeight;//.Y = (LcdHeight - Height) / 2;
                //var ax = (LCDWidth - Width) / 2;
                //var ay = (LCDHeight - AvailableHeight) / 2;
                //Canvas.SetLeft(_list, ax);
                //Canvas.SetTop(_list, ay);
                AddChild(listName, _list);

                MainWindow.Invalidate();
            }
            else
                throw new SystemException("You already have a List open.");
        }
        /*
        private static void _list_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            _dropdown.Text =_list.Items[args.SelectedIndex].ToString();
            _dropdown.Value = _list.Items[args.SelectedIndex].ToString();
            _dropdown.TriggerValueChangedEvent(_dropdown);

            CloseList();
        }
        */
        static void AddChild(string name, UIElement child)
        {
            if (MainWindow != null && MainWindow.Child != null)
            {
                Canvas parent = MainWindow.Child as Canvas;
                parent.Children.Add(child);
                MainWindow.AddControls(name, child);
            }
        }
        private static void list_TapOptionEvent(object sender, TapOptionEventArgs args)
        {
            _dropdown.Text = args.Label;
            _dropdown.Value = args.Value;
            _dropdown.TriggerValueChangedEvent(_dropdown);

            CloseList();
        }
        /// <summary>
        /// Closes a List component.
        /// </summary>
        public static void CloseList()
        {
            if (_list != null)
            {
                //_list.SelectionChanged -= _list_SelectionChanged;
                _list.TapOptionEvent -= new OnTapOption(list_TapOptionEvent);

                RemoveChildByName(listName);
                
                _list = null;
                //_viewer = null;
                //for (int i = 0; i < MainWindow.NumChildren; i++)
                //    MainWindow[i].Interactive = true; 
                if (_mainWindow != null)
                {
                    var mainCanvas = _mainWindow.Child as Canvas;
                    if (mainCanvas != null)
                    {
                        /*
                        foreach (UIElement component in mainCanvas.Children)
                        {
                            if (listOfDisableControl.Contains(component.ID))
                            {  
                                component.Visibility = Visibility.Visible;
                            }

                        }*/
                        foreach (var comp in MainWindow.GetAllControls()) {
                            var component = comp.Element;
                            if (listOfDisableControl.Contains(comp.ElementName)) {
                                component.Visibility = Visibility.Visible;
                            }

                        }
                        listOfDisableControl.Clear();
                    }
               
                }

                MainWindow.Invalidate();
            }
        }
    }
}
