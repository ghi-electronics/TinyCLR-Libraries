namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ.</summary>
    public static class FEZ {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            public const string Id = FEZChip.GpioPin.Id;

            /// <summary>GPIO pin for LED1.</summary>
            public const int Led1 = FEZChip.GpioPin.PB9;
            /// <summary>GPIO pin for LED2.</summary>
            public const int Led2 = FEZChip.GpioPin.PB8;
            /// <summary>GPIO pin for BTN1.</summary>
            public const int Btn1 = FEZChip.GpioPin.PA15;
            /// <summary>GPIO pin for BTN2.</summary>
            public const int Btn2 = FEZChip.GpioPin.PC13;
            /// <summary>GPIO pin.</summary>
            public const int A0 = FEZChip.GpioPin.PA4;
            /// <summary>GPIO pin.</summary>
            public const int A1 = FEZChip.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int A2 = FEZChip.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int A3 = FEZChip.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int A4 = FEZChip.GpioPin.PB0;
            /// <summary>GPIO pin.</summary>
            public const int A5 = FEZChip.GpioPin.PB1;
            /// <summary>GPIO pin.</summary>
            public const int D0 = FEZChip.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int D1 = FEZChip.GpioPin.PA9;
            /// <summary>GPIO pin.</summary>
            public const int D2 = FEZChip.GpioPin.PC1;
            /// <summary>GPIO pin.</summary>
            public const int D3 = FEZChip.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int D4 = FEZChip.GpioPin.PC15;
            /// <summary>GPIO pin.</summary>
            public const int D5 = FEZChip.GpioPin.PA1;
            /// <summary>GPIO pin.</summary>
            public const int D6 = FEZChip.GpioPin.PA0;
            /// <summary>GPIO pin.</summary>
            public const int D7 = FEZChip.GpioPin.PC14;
            /// <summary>GPIO pin.</summary>
            public const int D8 = FEZChip.GpioPin.PC0;
            /// <summary>GPIO pin.</summary>
            public const int D9 = FEZChip.GpioPin.PC9;
            /// <summary>GPIO pin.</summary>
            public const int D10 = FEZChip.GpioPin.PC8;
            /// <summary>GPIO pin.</summary>
            public const int D11 = FEZChip.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int D12 = FEZChip.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int D13 = FEZChip.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int Sda = FEZChip.GpioPin.PB7;
            /// <summary>GPIO pin.</summary>
            public const int Scl = FEZChip.GpioPin.PB6;
            /// <summary>GPIO pin.</summary>
            public const int WiFiReset = FEZChip.GpioPin.PC2;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            public const string Id = FEZChip.AdcChannel.Id;

            /// <summary>ADC channel.</summary>
            public const int A0 = FEZChip.AdcChannel.PA4;
            /// <summary>ADC channel.</summary>
            public const int A1 = FEZChip.AdcChannel.PA5;
            /// <summary>ADC channel.</summary>
            public const int A2 = FEZChip.AdcChannel.PA6;
            /// <summary>ADC channel.</summary>
            public const int A3 = FEZChip.AdcChannel.PA7;
            /// <summary>ADC channel.</summary>
            public const int A4 = FEZChip.AdcChannel.PB0;
            /// <summary>ADC channel.</summary>
            public const int A5 = FEZChip.AdcChannel.PB1;
            /// <summary>ADC channel.</summary>
            public const int D2 = FEZChip.AdcChannel.PC1;
            /// <summary>ADC channel.</summary>
            public const int D5 = FEZChip.AdcChannel.PA1;
            /// <summary>ADC channel.</summary>
            public const int D6 = FEZChip.AdcChannel.PA0;
            /// <summary>ADC channel.</summary>
            public const int D8 = FEZChip.AdcChannel.PC0;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                public const string Id = FEZChip.PwmPin.Controller1.Id;

                /// <summary>PWM pin.</summary>
                public const int D3 = 0;
                /// <summary>PWM pin.</summary>
                public const int D1 = 1;
                /// <summary>PWM pin.</summary>
                public const int D0 = 2;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller2 {
                public const string Id = FEZChip.PwmPin.Controller2.Id;

                /// <summary>PWM pin.</summary>
                public const int D6 = 0;
                /// <summary>PWM pin.</summary>
                public const int D5 = 1;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller3 {
                public const string Id = FEZChip.PwmPin.Controller3.Id;

                /// <summary>PWM pin.</summary>
                public const int D11 = 1;
                /// <summary>PWM pin.</summary>
                public const int D10 = 2;
                /// <summary>PWM pin.</summary>
                public const int D9 = 3;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller4 {
                public const string Id = FEZChip.PwmPin.Controller4.Id;

                /// <summary>PWM pin.</summary>
                public const int Scl = 0;
                /// <summary>PWM pin.</summary>
                public const int Sda = 1;
                /// <summary>PWM pin.</summary>
                public const int Led2 = 2;
                /// <summary>PWM pin.</summary>
                public const int Led1 = 3;
            }
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            /// <summary>Serial port on D1 (TX) and D0 (RX).</summary>
            public const string Com1 = FEZChip.SerialPort.Com1;
            /// <summary>Serial port on WiFi TX (TX) and WiFi RX (RX).</summary>
            public const string Com2 = FEZChip.SerialPort.Com2;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on SDA (SDA) and SCL (SCL).</summary>
            public const string I2c1 = FEZChip.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on D11 (MOSI), D12 (MISO), and D13 (SCK).</summary>
            public const string Spi1 = FEZChip.SpiBus.Spi1;
        }
    }
}