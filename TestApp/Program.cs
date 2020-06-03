
using GHI.GlideX;
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Pins;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using TestApp.Properties;

namespace TestApp
{
    class Program : Application
    {
        public Program(DisplayController d) : base(d)
        {
        }

        private static void Main()
        {
            try
            {

                //TestScreen();
                TestApp();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }

       

        static void TestScreen()
        {

            var lcd = new DisplayDriver43(SC20260.GpioPin.PA15);
            var app = new Program(lcd.display);
            app.Run(Program.CreateWindow(lcd.display));
        }

        private static Window CreateWindow(DisplayController display)
        {
            var window = new Window
            {
                Height = (int)display.ActiveConfiguration.Height,
                Width = (int)display.ActiveConfiguration.Width
            };

            window.Background = new LinearGradientBrush
                (Colors.Blue, Colors.Teal, 0, 0, window.Width, window.Height);

            window.Visibility = Visibility.Visible;

            return window;
        }
        static Program app;
        private const int SCREEN_WIDTH = 480;
        private const int SCREEN_HEIGHT = 272;
        private static void TestApp()
        {
            var lcd = new DisplayDriver43(SC20260.GpioPin.PA15);
            //must be declared before setup glide..
            app = new Program(lcd.display);
            OnScreenKeyboard.Font = Resources.GetFont(Resources.FontResources.NinaB);
            GlideX.SetupGlide(SCREEN_WIDTH, SCREEN_HEIGHT, 96, 0, lcd.display);
            string GlideXML = Resources.GetString(Resources.StringResources.SampleForm);  
            
            //Resources.GetString(Resources.StringResources.Window)
            Window window = GlideLoader.LoadWindow(GlideXML);
            GlideX.MainWindow = window;
            /*
            var GvData = (DataGrid)GlideX.GetChildByName("GvData");
            var TxtSlider = (Text)GlideX.GetChildByName("txt1");
            var Slider1 = (Slider)GlideX.GetChildByName("slider1");
            GvData.AddColumn(new DataGridColumn("Time", 100));
            GvData.AddColumn(new DataGridColumn("Sensor A", 100));
            GvData.AddColumn(new DataGridColumn("Sensor B", 100));
            Random rnd = new Random();
            int counter = 0;
            Slider1.RaiseTouchDownEvent += (object Sender, GHIElectronics.TinyCLR.UI.Glide.Geom.Point e)=>
            {
                app.InputProvider.RaiseTouch(e.X, e.Y, TouchMessages.Down, DateTime.UtcNow);
            };
            Slider1.RaiseTouchUpEvent += (object Sender, GHIElectronics.TinyCLR.UI.Glide.Geom.Point e) =>
            {
                app.InputProvider.RaiseTouch(e.X, e.Y, TouchMessages.Up, DateTime.UtcNow);
            };
            Slider1.RaiseTouchMoveEvent += (object Sender, GHIElectronics.TinyCLR.UI.Glide.Geom.Point e) =>
            {
                app.InputProvider.RaiseTouch(e.X, e.Y, TouchMessages.Move, DateTime.UtcNow);
            };

            Timer timer = new Timer((object o) => {
                Application.Current.Dispatcher.Invoke(TimeSpan.FromMilliseconds(1), _ =>
                {
                    //insert to db
                    var item = new DataGridItem(new object[] { DateTime.Now.ToString("HH:mm:ss"), $"{(20 + rnd.Next() * 50).ToString("n2")}C", $"{(rnd.Next() * 1000).ToString("n2")}L" });
                    //add data to grid
                    GvData.AddItem(item);
                    GvData.Invalidate();
                    if (counter++ > 4)
                    {
                        counter = 0;
                        GvData.Clear();
                    }
                    TxtSlider.TextContent = Slider1.Value.ToString("n");
                    TxtSlider.Invalidate();
                    return null;
                }, null);
                
            }, null, 1000, 1000);
            
          */
            
            ArrayList options = new ArrayList();
            for (int i = 0; i < 15; i++)
            {
                //options.Add("Item " + i);
                options.Add( new object[] { "Item " + i, "Item " + i });

            }

            var listMessage = new List(options, 300, GlideX.LCD.Width, GlideX.LCD.Height, GlideX.MainWindow);
            listMessage.CloseEvent += (object sender) =>
            {
                GlideX.CloseList();
            };
            
            //Font font = Resources.GetFont(Resources.FontResources.NinaB);
            //var listBox = new ListBox();
            //for (int i = 0; i < 15; i++)
            //{
            //    listBox.Items.Add(new Text(font, "Item "+i));

            //}

            var txt = (Text)GlideX.GetChildByName("TxtTest");
            var btn = (Button)GlideX.GetChildByName("btn");
            var cmb = (ComboBox)GlideX.GetChildByName("cmb1");
            //cmb.Options = options;
            
            cmb.TapEvent += (object sender) =>
            {
                GlideX.OpenList(sender, listMessage);
            };
            cmb.ValueChangedEvent += (object sender) =>
            {
                var dropdown = (ComboBox)sender;
                if (dropdown.Value == null) return;
                Debug.WriteLine("selected:"+dropdown.Value.ToString());
                //Debug.Print("Dropdown value: " + dropdown.Text + " : " + dropdown.Value.ToString());
            };

            Font _font = Resources.GetFont(Resources.FontResources.NinaB);
            Dropdown dropdown1 = new Dropdown();
            dropdown1.ID = "dropdown1";
            dropdown1.Width = 200;
            dropdown1.Height = 32;
            dropdown1.Alpha = 255;
            dropdown1.Font = _font;
            dropdown1.Options = new ArrayList();
            for (int i = 0; i < 15; i++)
            {
                dropdown1.Options.Add("Item " + i);
            }

            GlideX.AddChildToMainWindow(dropdown1);
            Canvas.SetLeft(dropdown1, 200);
            Canvas.SetTop(dropdown1, 100);

            //dropdown.Child = txt;
            //dropdown.Text = text;

            //cmb.Options = new ArrayList();
            //cmb.Options.Add(new object[2] { "Item 1", "Item 1" });
            //cmb.Options.Add(new object[2] { "Item 2", "Item 2" });
            //cmb.Options.Add(new object[2] { "Item 3", "Item 3" });
            //cmb.Options.Add(new object[2] { "Item 4", "Item 4" });
            cmb.Invalidate();
            if (btn != null)
            {
                btn.Click += (a,b) =>
                {
                    txt.TextContent = "Welcome to Glide for TinyCLR 2 - Cheers from Mif ;)";
                    Debug.WriteLine("Button tapped.");

                    window.Invalidate();
                    txt.Invalidate();
                };
            }

            //GlideTouch.Initialize();
            /*
            GHI.Glide.UI.Button btn = (GHI.Glide.UI.Button)window.GetChildByName("btn");
            GHI.Glide.UI.TextBlock txt = (GHI.Glide.UI.TextBlock)window.GetChildByName("TxtTest");
            btn.TapEvent += (object sender) =>
            {
                txt.Text = "Welcome to Glide for TinyCLR 2 - Cheers from Mif ;)";
                Debug.WriteLine("Button tapped.");

                window.Invalidate();
                txt.Invalidate();
            };*/



            lcd.CapacitiveScreenReleased += Lcd_CapacitiveScreenReleased;
            lcd.CapacitiveScreenPressed += Lcd_CapacitiveScreenPressed;
            lcd.CapacitiveScreenMove += Lcd_CapacitiveScreenMove;

            Graphics.OnFlushEvent += (IntPtr hdc, byte[] data) =>
            {
                lcd.display.DrawBuffer(0, 0, 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT, SCREEN_WIDTH, data, 0);
            };

           

            app.Run(GlideX.MainWindow);
            //Thread.Sleep(Timeout.Infinite);
        }

      




        #region Lcd Capacitive Touch Events
        /// <summary>
        /// Function called when released event raises
        /// </summary>
        /// <param name="sender">sender of event</param>
        /// <param name="e">EventArgs of event</param>
        private static void Lcd_CapacitiveScreenReleased(object sender, DisplayDriver43.TouchEventArgs e)
        {
            Debug.WriteLine("you release the lcd at X:" + e.X + " ,Y:" + e.Y);
            app.InputProvider.RaiseTouch(e.X, e.Y, TouchMessages.Up, DateTime.UtcNow);
           
            //GlideTouch.RaiseTouchUpEvent(e.X, e.Y);
        }

        /// <summary>
        /// Function called when pressed event raises
        /// </summary>
        /// <param name="sender">sender of event</param>
        /// <param name="e">EventArgs of event</param>
        private static void Lcd_CapacitiveScreenPressed(object sender, DisplayDriver43.TouchEventArgs e)
        {
            Debug.WriteLine("you press the lcd at X:" + e.X + " ,Y:" + e.Y);
            //GlideTouch.RaiseTouchDownEvent(e.X, e.Y);
            app.InputProvider.RaiseTouch(e.X, e.Y, TouchMessages.Down, DateTime.UtcNow);
        }

        private static void Lcd_CapacitiveScreenMove(object sender, DisplayDriver43.TouchEventArgs e)
        {
            Debug.WriteLine("you move finger on the lcd at X:" + e.X + " ,Y:" + e.Y);
            //GlideTouch.RaiseTouchMoveEvent(sender, new TouchEventArgs(new  GHI.Glide.Geom.Point(e.X,e.Y)));
            app.InputProvider.RaiseTouch(e.X, e.Y, TouchMessages.Move, DateTime.UtcNow);
        }
        #endregion
    }
   
}
