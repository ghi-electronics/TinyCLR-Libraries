namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the BrainPad.</summary>
    public static class BrainPad {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            public const string Id = FEZChip.GpioPin.Id;

            /// <summary>GPIO pin.</summary>
            public const int Mosi = FEZChip.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int Miso = FEZChip.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int Sck = FEZChip.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int Cs = FEZChip.GpioPin.PC3;
            /// <summary>GPIO pin.</summary>
            public const int Rst = FEZChip.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int An = FEZChip.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int Pwm = FEZChip.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int Int = FEZChip.GpioPin.PA2;
            /// <summary>GPIO pin.</summary>
            public const int Rx = FEZChip.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int Tx = FEZChip.GpioPin.PA9;
        }

        /// <summary>ADc channel definitions.</summary>
        public static class AdcChannel {
            public const string Id = FEZChip.AdcChannel.Id;

            /// <summary>ADC channel.</summary>
            public const int An = FEZChip.AdcChannel.PA7;
            /// <summary>ADC channel.</summary>
            public const int Rst = FEZChip.AdcChannel.PA6;
            /// <summary>ADC channel.</summary>
            public const int Cs = FEZChip.AdcChannel.PC3;
            /// <summary>ADC channel.</summary>
            public const int Int = FEZChip.AdcChannel.PA2;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                public const string Id = FEZChip.PwmPin.Controller1.Id;

                /// <summary>PWM pin.</summary>
                public const int Pwm = FEZChip.PwmPin.Controller1.PA8;
                /// <summary>PWM pin.</summary>
                public const int Rx = FEZChip.PwmPin.Controller1.PA10;
                /// <summary>PWM pin.</summary>
                public const int Tx = FEZChip.PwmPin.Controller1.PA9;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller2 {
                public const string Id = FEZChip.PwmPin.Controller2.Id;

                /// <summary>PWM pin.</summary>
                public const int Int = FEZChip.PwmPin.Controller2.PA2;
            }
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            /// <summary>Serial port on TX (TX) and RX (RX).</summary>
            public const string Com1 = FEZChip.SerialPort.Com1;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on SDA (SDA) and SCL (SCL).</summary>
            public const string I2c1 = FEZChip.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on MOSI (MOSI), MISO (MISO), and SCK (SCK).</summary>
            public const string Spi1 = FEZChip.SpiBus.Spi1;
        }
    }
}
