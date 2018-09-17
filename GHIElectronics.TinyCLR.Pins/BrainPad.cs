namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the BrainPad.</summary>
    public static class BrainPad {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = BrainPadBP2.GpioPin.Id;

            /// <summary>GPIO pin for Button Left.</summary>
            public const int ButtonLeft = BrainPadBP2.GpioPin.PA15;
            /// <summary>GPIO pin for Button Right.</summary>
            public const int ButtonRight = BrainPadBP2.GpioPin.PC13;
            /// <summary>GPIO pin for Button Up.</summary>
            public const int ButtonUp = BrainPadBP2.GpioPin.PA5;
            /// <summary>GPIO pin for Button Down.</summary>
            public const int ButtonDown = BrainPadBP2.GpioPin.PB10;
            /// <summary>GPIO pin for LightBulb Red.</summary>
            public const int LightBulbRed = BrainPadBP2.GpioPin.PC9;
            /// <summary>GPIO pin for LightBulb Green.</summary>
            public const int LightBulbGreen = BrainPadBP2.GpioPin.PC8;
            /// <summary>GPIO pin for LightBulb Blue.</summary>
            public const int LightBulbBlue = BrainPadBP2.GpioPin.PC6;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>API id.</summary>\
            public const string Id = BrainPadBP2.AdcChannel.Id;

            /// <summary>ADC channel for Temperature Sensor.</summary>
            public const int TemperatureSensor = BrainPadBP2.AdcChannel.PB0;
            /// <summary>ADC channel for Light Sensor.</summary>
            public const int LightSensor = BrainPadBP2.AdcChannel.PB1;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmChannel {
            /// <summary>PWM controller definitions.</summary>
            public static class Controller2 {
                /// <summary>API id.</summary>
                public const string Id = BrainPadBP2.PwmChannel.Controller2.Id;

                /// <summary>PWM pin for Servo one.</summary>
                public const int ServoOne = BrainPadBP2.PwmChannel.Controller2.PA3;
                /// <summary>PWM pin for Servo two.</summary>
                public const int ServoTwo = BrainPadBP2.PwmChannel.Controller2.PA0;
            }

            /// <summary>PWM controller definitions.</summary>
            public static class Controller3 {
                /// <summary>API id.</summary>
                public const string Id = BrainPadBP2.PwmChannel.Controller3.Id;

                /// <summary>PWM pin for LightBulb Red.</summary>
                public const int LightBulbRed = BrainPadBP2.PwmChannel.Controller3.PC9;
                /// <summary>PWM pin for LightBulb Green.</summary>
                public const int LightBulbGreen = BrainPadBP2.PwmChannel.Controller3.PC8;
                /// <summary>PWM pin for LightBulb Blue.</summary>
                public const int LightBulbBlue = BrainPadBP2.PwmChannel.Controller3.PC6;
            }

            /// <summary>PWM controller definitions.</summary>
            public static class Controller4 {
                /// <summary>API id.</summary>
                public const string Id = BrainPadBP2.PwmChannel.Controller4.Id;

                /// <summary>PWM pin for Buzzer.</summary>
                public const int Buzzer = BrainPadBP2.PwmChannel.Controller4.PB8;
            }
        }

        /// <summary>I2C bus pin definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus for Accelerometer.</summary>
            public const string Accelerometer = BrainPadBP2.I2cBus.I2c1;
            /// <summary>I2C bus for Display.</summary>
            public const string Display = BrainPadBP2.I2cBus.I2c1;
        }

        /// <summary>Expansion definitions.</summary>
        public static class Expansion {
            /// <summary>GPIO pin definitions.</summary>
            public static class GpioPin {
                /// <summary>API id.</summary>
                public const string Id = BrainPadBP2.GpioPin.Id;

                /// <summary>GPIO pin.</summary>
                public const int Mosi = BrainPadBP2.GpioPin.PB5;
                /// <summary>GPIO pin.</summary>
                public const int Miso = BrainPadBP2.GpioPin.PB4;
                /// <summary>GPIO pin.</summary>
                public const int Sck = BrainPadBP2.GpioPin.PB3;
                /// <summary>GPIO pin.</summary>
                public const int Cs = BrainPadBP2.GpioPin.PC3;
                /// <summary>GPIO pin.</summary>
                public const int Rst = BrainPadBP2.GpioPin.PA6;
                /// <summary>GPIO pin.</summary>
                public const int An = BrainPadBP2.GpioPin.PA7;
                /// <summary>GPIO pin.</summary>
                public const int Pwm = BrainPadBP2.GpioPin.PA8;
                /// <summary>GPIO pin.</summary>
                public const int Int = BrainPadBP2.GpioPin.PA2;
                /// <summary>GPIO pin.</summary>
                public const int Rx = BrainPadBP2.GpioPin.PA10;
                /// <summary>GPIO pin.</summary>
                public const int Tx = BrainPadBP2.GpioPin.PA9;
            }

            /// <summary>ADC channel definitions.</summary>
            public static class AdcChannel {
                /// <summary>API id.</summary>
                public const string Id = BrainPadBP2.AdcChannel.Id;

                /// <summary>ADC channel.</summary>
                public const int An = BrainPadBP2.AdcChannel.PA7;
                /// <summary>ADC channel.</summary>
                public const int Rst = BrainPadBP2.AdcChannel.PA6;
                /// <summary>ADC channel.</summary>
                public const int Cs = BrainPadBP2.AdcChannel.PC3;
                /// <summary>ADC channel.</summary>
                public const int Int = BrainPadBP2.AdcChannel.PA2;
            }

            /// <summary>PWM pin definitions.</summary>
            public static class PwmChannel {
                /// <summary>PWM controller.</summary>
                public static class Controller1 {
                    /// <summary>API id.</summary>
                    public const string Id = BrainPadBP2.PwmChannel.Controller1.Id;

                    /// <summary>PWM pin.</summary>
                    public const int Pwm = BrainPadBP2.PwmChannel.Controller1.PA8;
                    /// <summary>PWM pin.</summary>
                    public const int Rx = BrainPadBP2.PwmChannel.Controller1.PA10;
                    /// <summary>PWM pin.</summary>
                    public const int Tx = BrainPadBP2.PwmChannel.Controller1.PA9;
                }

                /// <summary>PWM controller.</summary>
                public static class Controller2 {
                    /// <summary>API id.</summary>
                    public const string Id = BrainPadBP2.PwmChannel.Controller2.Id;

                    /// <summary>PWM pin.</summary>
                    public const int Int = BrainPadBP2.PwmChannel.Controller2.PA2;
                }
            }

            /// <summary>UART port definitions.</summary>
            public static class UartPort {
                /// <summary>UART port on TX (TX) and RX (RX).</summary>
                public const string Usart1 = BrainPadBP2.UartPort.Usart1;
            }

            /// <summary>I2C bus definitions.</summary>
            public static class I2cBus {
                /// <summary>I2C bus on SDA (SDA) and SCL (SCL).</summary>
                public const string I2c1 = BrainPadBP2.I2cBus.I2c1;
            }

            /// <summary>SPI bus definitions.</summary>
            public static class SpiBus {
                /// <summary>SPI bus on MOSI (MOSI), MISO (MISO), and SCK (SCK).</summary>
                public const string Spi1 = BrainPadBP2.SpiBus.Spi1;
            }

            /// <summary>USB client port definitions.</summary>
            public static class UsbClientPort {
                /// <summary>USB client port on D- (DM), D+ (DP), and VBUS (VBUS).</summary>
                public const string UsbOtg = BrainPadBP2.UsbClientPort.UsbOtg;
            }
        }
    }
}
