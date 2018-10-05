namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ GameO.</summary>
    public static class FEZGameO {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = Cerb.GpioPin.Id;

            /// <summary>GPIO pin for wake up button.</summary>
            public const int Wakeup = Cerb.GpioPin.PA0;
            /// <summary>GPIO pin for button 2.</summary>
            public const int Btn2 = Cerb.GpioPin.PC1;
            /// <summary>GPIO pin for button 3.</summary>
            public const int Btn3 = Cerb.GpioPin.PB12;
            /// <summary>GPIO pin for button 4.</summary>
            public const int Btn4 = Cerb.GpioPin.PC3;
            /// <summary>GPIO pin for button 5.</summary>
            public const int Btn5 = Cerb.GpioPin.PC4;
            /// <summary>GPIO pin for button 6.</summary>
            public const int Btn6 = Cerb.GpioPin.PC5;
            /// <summary>GPIO pin for button 7.</summary>
            public const int Btn7 = Cerb.GpioPin.PB13;
            /// <summary>GPIO pin for button 8.</summary>
            public const int Btn8 = Cerb.GpioPin.PB14;
            /// <summary>GPIO pin for button 9.</summary>
            public const int Btn9 = Cerb.GpioPin.PB15;
            /// <summary>GPIO pin for accelerometer interrupt 1.</summary>
            public const int Int1 = Cerb.GpioPin.PC6;
            /// <summary>GPIO pin for accelerometer interrupt 2.</summary>
            public const int Int2 = Cerb.GpioPin.PC7;
            /// <summary>GPIO pin for LCD reset.</summary>
            public const int LcdReset = Cerb.GpioPin.PC13;
        }

        /// <summary>DAC channel definition.</summary>
        public static class DacChannel {
            /// <summary>API id.</summary>
            public const string Id = Cerb.DacChannel.Id;

            /// <summary>DAC channel for speaker.</summary>
            public const int Speaker = Cerb.DacChannel.PA4;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmChannel {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmChannel.Controller1.Id;

                /// <summary>PWM pin for speaker digital volume control.</summary>
                public const int Volume = Cerb.PwmChannel.Controller1.PA8;
            }

            public static class Controller14 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmChannel.Controller14.Id;

                /// <summary>PWM pin for LCD backlight.</summary>
                public const int LcdBacklight = Cerb.PwmChannel.Controller14.PA7;
            }
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus for accelerometer.</summary>
            public const string Accelerometer = Cerb.I2cBus.I2c1;
        }

        /// <summary>USB client port definitions.</summary>
        public static class UsbClientPort {
            /// <summary>USB client port on J1 pin 2 (DM), J1 pin 3 (DP), and J1 pin 1 (VBUS).</summary>
            public const string UsbOtg = Cerb.UsbClientPort.UsbOtg;
        }

        /// <summary>StorageController definitions.</summary>
        public static class StorageController {
            /// <summary>API id.</summary>
            public const string SdCard = Cerb.StorageController.SdCard;
        }

        /// <summary>RTC controller definitions.</summary>
        public static class RtcController {
            /// <summary>API id.</summary>
            public const string Id = Cerb.RtcController.Id;
        }
    }
}
