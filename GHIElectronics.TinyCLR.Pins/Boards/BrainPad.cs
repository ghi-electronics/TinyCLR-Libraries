namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the BrainPad.</summary>
    public static class BrainPad {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            public const string Id = FEZFirmware.GpioPin.Id;

            /// <summary>GPIO pin.</summary>
            public const int Mosi = FEZFirmware.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int Miso = FEZFirmware.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int Sck = FEZFirmware.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int Cs = FEZFirmware.GpioPin.PC3;
            /// <summary>GPIO pin.</summary>
            public const int Rst = FEZFirmware.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int An = FEZFirmware.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int Pwm = FEZFirmware.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int Int = FEZFirmware.GpioPin.PA2;
            /// <summary>GPIO pin.</summary>
            public const int Rx = FEZFirmware.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int Tx = FEZFirmware.GpioPin.PA9;
        }

        /// <summary>ADc channel definitions.</summary>
        public static class AdcChannel {
            public const string Id = FEZFirmware.AdcChannel.Id;

            /// <summary>ADC channel.</summary>
            public const int An = FEZFirmware.AdcChannel.PA7;
            /// <summary>ADC channel.</summary>
            public const int Rst = FEZFirmware.AdcChannel.PA6;
            /// <summary>ADC channel.</summary>
            public const int Cs = FEZFirmware.AdcChannel.PC3;
            /// <summary>ADC channel.</summary>
            public const int Int = FEZFirmware.AdcChannel.PA2;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                public const string Id = FEZFirmware.PwmPin.Controller1.Id;

                /// <summary>PWM pin.</summary>
                public const int Pwm = FEZFirmware.PwmPin.Controller1.PA8;
                /// <summary>PWM pin.</summary>
                public const int Rx = FEZFirmware.PwmPin.Controller1.PA10;
                /// <summary>PWM pin.</summary>
                public const int Tx = FEZFirmware.PwmPin.Controller1.PA9;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller2 {
                public const string Id = FEZFirmware.PwmPin.Controller2.Id;

                /// <summary>PWM pin.</summary>
                public const int Int = FEZFirmware.PwmPin.Controller2.PA2;
            }
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>Uart port on TX (TX) and RX (RX).</summary>
            public const string Uart1 = FEZFirmware.UartPort.Uart1;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on SDA (SDA) and SCL (SCL).</summary>
            public const string I2c1 = FEZFirmware.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on MOSI (MOSI), MISO (MISO), and SCK (SCK).</summary>
            public const string Spi1 = FEZFirmware.SpiBus.Spi1;
        }
    }
}
