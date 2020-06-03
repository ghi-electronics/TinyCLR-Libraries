
using GHIElectronics.TinyCLR.Devices.Display;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using System.Drawing;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Drivers.FocalTech.FT5xx6;
using System.Diagnostics;

namespace TestApp
{
    public class DisplayDriver43
    {



        private delegate void NullParamsDelegate();

        /// <summary>The delegate that is used to handle the capacitive touch events.</summary>
        /// <param name="sender">The DisplayNHVN object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        public delegate void CapacitiveTouchEventHandler(object sender, TouchEventArgs e);

        /// <summary>Raised when the module detects a capacitive press.</summary>
        public event CapacitiveTouchEventHandler CapacitiveScreenPressed;

        /// <summary>Raised when the module detects a capacitive release.</summary>
        public event CapacitiveTouchEventHandler CapacitiveScreenReleased;

        /// <summary>Raised when the module detects a capacitive release.</summary>
        public event CapacitiveTouchEventHandler CapacitiveScreenMove;

        /// <summary>Constructs a new instance.</summary>
        /// <param name="Pin9OnGSocket">Backlight pin.</param>
        /// <param name="Pin3OnISocket">Interrupt pin for capacitive touch panel (i2c).</param>
        public DisplayDriver43(int BacklightPin)
        {


            //var settings = new I2cConnectionSettings(0x1C, 100_000); //The slave's address and the bus speed.
           
            //var device = controller.GetDevice(settings);

            i2CController = I2cController.FromName(SC20260.I2cBus.I2c1);//I2cController.GetDefault();
            gpioController = GpioController.GetDefault();

            this.backlightPin = gpioController.OpenPin(BacklightPin);
            this.backlightPin.SetDriveMode(GpioPinDriveMode.Output);
            BacklightEnabled = true;

            SetupCapacitiveTouchController();
            
            ConfigureDisplay();
            //this.backlightPin = GpioPinFactory.Create(gSocket, Socket.Pin.Nine, true, this);


        }

        /// <summary>Whether or not the backlight is enabled.</summary>
        public bool BacklightEnabled
        {
            get
            {
                return this.backlightPin.Read() == GpioPinValue.High;
            }

            set
            {
                this.backlightPin.Write(value ? GpioPinValue.High : GpioPinValue.Low);
            }
        }
        public I2cController i2CController { get; set; }
        public Graphics Screen { set; get; }
        public DisplayController display { set; get; }
        /// <summary>Constructs a new instance.</summary>
        /// <param name="DigitalPin9onGSocket">Pin 9 on Socket G.</param>
        public void ConfigureDisplay()
        {

            display = DisplayController.GetDefault();

            var controllerSetting = new
                GHIElectronics.TinyCLR.Devices.Display.ParallelDisplayControllerSettings
            {
                Width = 480,
                Height = 272,
                DataFormat = GHIElectronics.TinyCLR.Devices.Display.DisplayDataFormat.Rgb565,
                PixelClockRate = 10000000,
                PixelPolarity = false,
                DataEnablePolarity = false,
                DataEnableIsFixed = false,
                HorizontalFrontPorch = 2,
                HorizontalBackPorch = 2,
                HorizontalSyncPulseWidth = 41,
                HorizontalSyncPolarity = false,
                VerticalFrontPorch = 2,
                VerticalBackPorch = 2,
                VerticalSyncPulseWidth = 10,
                VerticalSyncPolarity = false,
            };

            display.SetConfiguration(controllerSetting);
            display.Enable();

         
            Screen = Graphics.FromHdc(display.Hdc); //Calling flush on the object returned will flush to the display represented by Hdc. Only one active display is supported at this time.
            
            //var ptr = Memory.UnmanagedMemory.Allocate(640 * 480 * 2);
            //var data = Memory.UnmanagedMemory.ToBytes(ptr, 640 * 480 * 2);
        }
        protected void ErrorPrint(string message)
        {
            Debug.WriteLine(this.ToString() + " ERROR : " + message);
        }


        /// <summary>Renders display data on the display device.</summary>
        /// <param name="bitmap">The bitmap object to render on the display.</param>
        /// <param name="x">The start x coordinate of the dirty area.</param>
        /// <param name="y">The start y coordinate of the dirty area.</param>
        /// <param name="width">The width of the dirty area.</param>
        /// <param name="height">The height of the dirty area.</param>
        public void Paint(Bitmap bitmap, int x, int y, int width, int height)
        {
            try
            {
               
                Screen.DrawImage(bitmap, x, y);
                Screen.Flush();
            }
            catch
            {
                this.ErrorPrint("Painting error");
            }
        }



        private GpioPin backlightPin;


        /// <summary>
        /// Event arguments for the capacitive touch events.
        /// </summary>
        public class TouchEventArgs
        {
            /// <summary>
            /// The X coordinate of the touch event.
            /// </summary>
            public int X { get; set; }

            /// <summary>
            /// The Y coordinate of the touch event.
            /// </summary>
            public int Y { get; set; }

            internal TouchEventArgs(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        #region TouchController
        private GpioController gpioController {set;get;}
        private FT5xx6Controller touch { set; get; }

        private void SetupCapacitiveTouchController()
        {
            var conf = FT5xx6Controller.GetConnectionSettings();
            var dev = i2CController.GetDevice(conf);
            var gpio = gpioController.OpenPin(SC20260.GpioPin.PJ14);//I don't know which pin to use for interrupt
            touch = new FT5xx6Controller(dev,gpio );//UCMStandard.GpioPin.B - ref:https://docs.ghielectronics.com/hardware/ucm/standard.html#pin-assignments

            touch.TouchDown += (_, e) => {
                if (this.CapacitiveScreenPressed != null)
                    this.CapacitiveScreenPressed(this, new TouchEventArgs(e.X , e.Y));
            };
            touch.TouchUp += (_, e) =>
            {
                if (this.CapacitiveScreenReleased != null)
                    this.CapacitiveScreenReleased(this, new TouchEventArgs(e.X, e.Y));
            };
            touch.TouchMove += (_, e) =>
            {
                if (this.CapacitiveScreenMove != null)
                    this.CapacitiveScreenMove(this, new TouchEventArgs(e.X, e.Y));
            };



        }

      
        #endregion
    }

}
