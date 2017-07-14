namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ.</summary>
    public static class FEZ {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = FEZFirmware.GpioPin.Id;

            /// <summary>GPIO pin for LED1.</summary>
            public const int Led1 = FEZFirmware.GpioPin.PB9;
            /// <summary>GPIO pin for LED2.</summary>
            public const int Led2 = FEZFirmware.GpioPin.PC10;
            /// <summary>GPIO pin for BTN1.</summary>
            public const int Btn1 = FEZFirmware.GpioPin.PA15;
            /// <summary>GPIO pin for BTN2.</summary>
            public const int Btn2 = FEZFirmware.GpioPin.PC13;
            /// <summary>GPIO pin.</summary>
            public const int A0 = FEZFirmware.GpioPin.PA4;
            /// <summary>GPIO pin.</summary>
            public const int A1 = FEZFirmware.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int A2 = FEZFirmware.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int A3 = FEZFirmware.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int A4 = FEZFirmware.GpioPin.PB0;
            /// <summary>GPIO pin.</summary>
            public const int A5 = FEZFirmware.GpioPin.PB1;
            /// <summary>GPIO pin.</summary>
            public const int D0 = FEZFirmware.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int D1 = FEZFirmware.GpioPin.PA9;
            /// <summary>GPIO pin.</summary>
            public const int D2 = FEZFirmware.GpioPin.PC1;
            /// <summary>GPIO pin.</summary>
            public const int D3 = FEZFirmware.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int D4 = FEZFirmware.GpioPin.PC15;
            /// <summary>GPIO pin.</summary>
            public const int D5 = FEZFirmware.GpioPin.PB8;
            /// <summary>GPIO pin.</summary>
            public const int D6 = FEZFirmware.GpioPin.PC6;
            /// <summary>GPIO pin.</summary>
            public const int D7 = FEZFirmware.GpioPin.PC14;
            /// <summary>GPIO pin.</summary>
            public const int D8 = FEZFirmware.GpioPin.PC0;
            /// <summary>GPIO pin.</summary>
            public const int D9 = FEZFirmware.GpioPin.PC9;
            /// <summary>GPIO pin.</summary>
            public const int D10 = FEZFirmware.GpioPin.PC8;
            /// <summary>GPIO pin.</summary>
            public const int D11 = FEZFirmware.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int D12 = FEZFirmware.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int D13 = FEZFirmware.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int Sda = FEZFirmware.GpioPin.PB7;
            /// <summary>GPIO pin.</summary>
            public const int Scl = FEZFirmware.GpioPin.PB6;
            /// <summary>GPIO pin.</summary>
            public const int WiFiReset = FEZFirmware.GpioPin.PC2;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>API id.</summary>
            public const string Id = FEZFirmware.AdcChannel.Id;

            /// <summary>ADC channel.</summary>
            public const int A0 = FEZFirmware.AdcChannel.PA4;
            /// <summary>ADC channel.</summary>
            public const int A1 = FEZFirmware.AdcChannel.PA5;
            /// <summary>ADC channel.</summary>
            public const int A2 = FEZFirmware.AdcChannel.PA6;
            /// <summary>ADC channel.</summary>
            public const int A3 = FEZFirmware.AdcChannel.PA7;
            /// <summary>ADC channel.</summary>
            public const int A4 = FEZFirmware.AdcChannel.PB0;
            /// <summary>ADC channel.</summary>
            public const int A5 = FEZFirmware.AdcChannel.PB1;
            /// <summary>ADC channel.</summary>
            public const int D2 = FEZFirmware.AdcChannel.PC1;
            /// <summary>ADC channel.</summary>
            public const int D8 = FEZFirmware.AdcChannel.PC0;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = FEZFirmware.PwmPin.Controller1.Id;

                /// <summary>PWM pin.</summary>
                public const int D3 = 0;
                /// <summary>PWM pin.</summary>
                public const int D1 = 1;
                /// <summary>PWM pin.</summary>
                public const int D0 = 2;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller3 {
                /// <summary>API id.</summary>
                public const string Id = FEZFirmware.PwmPin.Controller3.Id;

                /// <summary>PWM pin.</summary>
                public const int D6 = 0;
                /// <summary>PWM pin.</summary>
                public const int D11 = 1;
                /// <summary>PWM pin.</summary>
                public const int D10 = 2;
                /// <summary>PWM pin.</summary>
                public const int D9 = 3;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller4 {
                /// <summary>API id.</summary>
                public const string Id = FEZFirmware.PwmPin.Controller4.Id;

                /// <summary>PWM pin.</summary>
                public const int Scl = 0;
                /// <summary>PWM pin.</summary>
                public const int Sda = 1;
                /// <summary>PWM pin.</summary>
                public const int D5 = 2;
                /// <summary>PWM pin.</summary>
                public const int Led1 = 3;
            }
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>Uart port on D1 (TX) and D0 (RX).</summary>
            public const string Uart1 = FEZFirmware.UartPort.Uart1;
            /// <summary>Uart port on WiFi TX (TX), WiFi RX (RX), WiFi CTS (CTS), and WiFi RTS (RTS).</summary>
            public const string WiFi = FEZFirmware.UartPort.Uart2;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on SDA (SDA) and SCL (SCL).</summary>
            public const string I2c1 = FEZFirmware.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on D11 (MOSI), D12 (MISO), and D13 (SCK).</summary>
            public const string Spi1 = FEZFirmware.SpiBus.Spi1;
        }
    }
}