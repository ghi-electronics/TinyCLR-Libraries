namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the BrainPad.</summary>
    public static class BrainPad {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            public const string Id = FEZ.GpioPin.Id;

            /// <summary>GPIO pin.</summary>
            public const int Mosi = G30.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int Miso = G30.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int Sck = G30.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int Cs = G30.GpioPin.PC3;
            /// <summary>GPIO pin.</summary>
            public const int Rst = G30.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int An = G30.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int Pwm = G30.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int Int = G30.GpioPin.PA2;
            /// <summary>GPIO pin.</summary>
            public const int Rx = G30.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int Tx = G30.GpioPin.PA9;
            /// <summary>GPIO pin.</summary>
            public const int Scl = G30.GpioPin.PB6;
            /// <summary>GPIO pin.</summary>
            public const int Sda = G30.GpioPin.PB7;
        }

        /// <summary>ADc channel definitions.</summary>
        public static class AdcChannel {
            public const string Id = FEZ.AdcChannel.Id;

            /// <summary>ADC channel.</summary>
            public const int An = G30.AdcChannel.PA7;
            /// <summary>ADC channel.</summary>
            public const int Rst = G30.AdcChannel.PA6;
            /// <summary>ADC channel.</summary>
            public const int Cs = G30.AdcChannel.PC3;
            /// <summary>ADC channel.</summary>
            public const int Int = G30.AdcChannel.PA2;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                public const string Id = FEZ.PwmPin.Controller1.Id;

                /// <summary>PWM pin.</summary>
                public const int Pwm = G30.PwmPin.Controller1.PA8;
                /// <summary>PWM pin.</summary>
                public const int Rx = G30.PwmPin.Controller1.PA10;
                /// <summary>PWM pin.</summary>
                public const int Tx = G30.PwmPin.Controller1.PA9;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller2 {
                public const string Id = FEZ.PwmPin.Controller2.Id;

                /// <summary>PWM pin.</summary>
                public const int Int = G30.PwmPin.Controller2.PA2;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller4 {
                public const string Id = FEZ.PwmPin.Controller4.Id;

                /// <summary>PWM pin.</summary>
                public const int Scl = G30.PwmPin.Controller4.PB6;
                /// <summary>PWM pin.</summary>
                public const int Sda = G30.PwmPin.Controller4.PB7;
            }
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            /// <summary>Serial port on TX (TX) and RX (RX).</summary>
            public const string Com1 = FEZ.SerialPort.Com1;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on SDA (SDA) and SCL (SCL).</summary>
            public const string I2c1 = FEZ.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on MOSI (MOSI), MISO (MISO), and SCK (SCK).</summary>
            public const string Spi1 = FEZ.SpiBus.Spi1;
        }
    }
}
