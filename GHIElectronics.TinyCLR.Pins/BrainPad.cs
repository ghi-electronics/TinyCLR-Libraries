namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the BrainPad.</summary>
    public static class BrainPad {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = FEZCLR.GpioPin.Id;

            /// <summary>GPIO pin for Button Left.</summary>
            public const int ButtonLeft = FEZCLR.GpioPin.PA15;
            /// <summary>GPIO pin for Button Right.</summary>
            public const int ButtonRight = FEZCLR.GpioPin.PC13;
            /// <summary>GPIO pin for Button Up.</summary>
            public const int ButtonUp = FEZCLR.GpioPin.PA5;
            /// <summary>GPIO pin for Button Down.</summary>
            public const int ButtonDown = FEZCLR.GpioPin.PB10;
            /// <summary>GPIO pin for LightBulb Red.</summary>
            public const int LightBulbRed = FEZCLR.GpioPin.PC9;
            /// <summary>GPIO pin for LightBulb Green.</summary>
            public const int LightBulbGreen = FEZCLR.GpioPin.PC8;
            /// <summary>GPIO pin for LightBulb Blue.</summary>
            public const int LightBulbBlue = FEZCLR.GpioPin.PC6;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>ADC channel for the Temperature Sensor.</summary>
            public const int TemperatureSensor = FEZCLR.AdcChannel.PB0;
            /// <summary>ADC channel for the Light Sensor.</summary>
            public const int LightSensor = FEZCLR.AdcChannel.PB1;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>Controller 2 definitions.</summary>
            public static class Controller2 {
                /// <summary>API id.</summary>
                public const string Id = FEZCLR.PwmPin.Controller2.Id;

                /// <summary>PWM pin for Servo one.</summary>
                public const int ServoOne = FEZCLR.PwmPin.Controller2.PA3;
                /// <summary>PWM pin for Servo two.</summary>
                public const int ServoTwo = FEZCLR.PwmPin.Controller2.PA0;
            }

            /// <summary>Controller 3 definitions.</summary>
            public static class Controller3 {
                /// <summary>API id.</summary>
                public const string Id = FEZCLR.PwmPin.Controller3.Id;

                /// <summary>PWM pin for LightBulb Red.</summary>
                public const int LightBulbRed = FEZCLR.PwmPin.Controller3.PC9;
                /// <summary>PWM pin for LightBulb Green.</summary>
                public const int LightBulbGreen = FEZCLR.PwmPin.Controller3.PC8;
                /// <summary>PWM pin for LightBulb Blue.</summary>
                public const int LightBulbBlue = FEZCLR.PwmPin.Controller3.PC6;
            }

            /// <summary>Controller 4 definitions.</summary>
            public static class Controller4 {
                /// <summary>API id.</summary>
                public const string Id = FEZCLR.PwmPin.Controller4.Id;

                /// <summary>PWM pin for Buzzer.</summary>
                public const int Buzzer = FEZCLR.PwmPin.Controller4.PB8;
            }
        }

        /// <summary>I2C bus pin definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus for Accelerometer.</summary>
            public const string Accelerometer = FEZCLR.I2cBus.I2c1;
            /// <summary>I2C bus for Display.</summary>
            public const string Display = FEZCLR.I2cBus.I2c1;
        }

        /// <summary>Expansion definitions.</summary>
        public static class Expansion {
            /// <summary>GPIO pin definitions.</summary>
            public static class GpioPin {
                /// <summary>API id.</summary>
                public const string Id = FEZCLR.GpioPin.Id;

                /// <summary>GPIO pin.</summary>
                public const int Mosi = FEZCLR.GpioPin.PB5;
                /// <summary>GPIO pin.</summary>
                public const int Miso = FEZCLR.GpioPin.PB4;
                /// <summary>GPIO pin.</summary>
                public const int Sck = FEZCLR.GpioPin.PB3;
                /// <summary>GPIO pin.</summary>
                public const int Cs = FEZCLR.GpioPin.PC3;
                /// <summary>GPIO pin.</summary>
                public const int Rst = FEZCLR.GpioPin.PA6;
                /// <summary>GPIO pin.</summary>
                public const int An = FEZCLR.GpioPin.PA7;
                /// <summary>GPIO pin.</summary>
                public const int Pwm = FEZCLR.GpioPin.PA8;
                /// <summary>GPIO pin.</summary>
                public const int Int = FEZCLR.GpioPin.PA2;
                /// <summary>GPIO pin.</summary>
                public const int Rx = FEZCLR.GpioPin.PA10;
                /// <summary>GPIO pin.</summary>
                public const int Tx = FEZCLR.GpioPin.PA9;
            }

            /// <summary>ADC channel definitions.</summary>
            public static class AdcChannel {
                /// <summary>API id.</summary>
                public const string Id = FEZCLR.AdcChannel.Id;

                /// <summary>ADC channel.</summary>
                public const int An = FEZCLR.AdcChannel.PA7;
                /// <summary>ADC channel.</summary>
                public const int Rst = FEZCLR.AdcChannel.PA6;
                /// <summary>ADC channel.</summary>
                public const int Cs = FEZCLR.AdcChannel.PC3;
                /// <summary>ADC channel.</summary>
                public const int Int = FEZCLR.AdcChannel.PA2;
            }

            /// <summary>PWM pin definitions.</summary>
            public static class PwmPin {
                /// <summary>PWM controller.</summary>
                public static class Controller1 {
                    /// <summary>API id.</summary>
                    public const string Id = FEZCLR.PwmPin.Controller1.Id;

                    /// <summary>PWM pin.</summary>
                    public const int Pwm = FEZCLR.PwmPin.Controller1.PA8;
                    /// <summary>PWM pin.</summary>
                    public const int Rx = FEZCLR.PwmPin.Controller1.PA10;
                    /// <summary>PWM pin.</summary>
                    public const int Tx = FEZCLR.PwmPin.Controller1.PA9;
                }

                /// <summary>PWM controller.</summary>
                public static class Controller2 {
                    /// <summary>API id.</summary>
                    public const string Id = FEZCLR.PwmPin.Controller2.Id;

                    /// <summary>PWM pin.</summary>
                    public const int Int = FEZCLR.PwmPin.Controller2.PA2;
                }
            }

            /// <summary>UART port definitions.</summary>
            public static class UartPort {
                /// <summary>UART port on TX (TX) and RX (RX).</summary>
                public const string Usart1 = FEZCLR.UartPort.Usart1;
            }

            /// <summary>I2C bus definitions.</summary>
            public static class I2cBus {
                /// <summary>I2C bus on SDA (SDA) and SCL (SCL).</summary>
                public const string I2c1 = FEZCLR.I2cBus.I2c1;
            }

            /// <summary>SPI bus definitions.</summary>
            public static class SpiBus {
                /// <summary>SPI bus on MOSI (MOSI), MISO (MISO), and SCK (SCK).</summary>
                public const string Spi1 = FEZCLR.SpiBus.Spi1;
            }

            /// <summary>USB client port definitions.</summary>
            public static class UsbClientPort {
                /// <summary>USB client port on D- (DM), D+ (DP), and VBUS (VBUS).</summary>
                public const string UsbOtg = FEZCLR.UsbClientPort.UsbOtg;
            }
        }
    }
}
