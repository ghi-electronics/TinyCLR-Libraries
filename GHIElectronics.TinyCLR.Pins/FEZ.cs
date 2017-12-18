namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ.</summary>
    public static class FEZ {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = FEZCLR.GpioPin.Id;

            /// <summary>GPIO pin for LED1.</summary>
            public const int Led1 = FEZCLR.GpioPin.PB9;
            /// <summary>GPIO pin for LED2.</summary>
            public const int Led2 = FEZCLR.GpioPin.PC10;
            /// <summary>GPIO pin for BTN1.</summary>
            public const int Btn1 = FEZCLR.GpioPin.PA15;
            /// <summary>GPIO pin for BTN2.</summary>
            public const int Btn2 = FEZCLR.GpioPin.PC13;
            /// <summary>GPIO pin.</summary>
            public const int A0 = FEZCLR.GpioPin.PA4;
            /// <summary>GPIO pin.</summary>
            public const int A1 = FEZCLR.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int A2 = FEZCLR.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int A3 = FEZCLR.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int A4 = FEZCLR.GpioPin.PB0;
            /// <summary>GPIO pin.</summary>
            public const int A5 = FEZCLR.GpioPin.PB1;
            /// <summary>GPIO pin.</summary>
            public const int D0 = FEZCLR.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int D1 = FEZCLR.GpioPin.PA9;
            /// <summary>GPIO pin.</summary>
            public const int D2 = FEZCLR.GpioPin.PC1;
            /// <summary>GPIO pin.</summary>
            public const int D3 = FEZCLR.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int D4 = FEZCLR.GpioPin.PC15;
            /// <summary>GPIO pin.</summary>
            public const int D5 = FEZCLR.GpioPin.PB8;
            /// <summary>GPIO pin.</summary>
            public const int D6 = FEZCLR.GpioPin.PC6;
            /// <summary>GPIO pin.</summary>
            public const int D7 = FEZCLR.GpioPin.PC14;
            /// <summary>GPIO pin.</summary>
            public const int D8 = FEZCLR.GpioPin.PC0;
            /// <summary>GPIO pin.</summary>
            public const int D9 = FEZCLR.GpioPin.PC9;
            /// <summary>GPIO pin.</summary>
            public const int D10 = FEZCLR.GpioPin.PC8;
            /// <summary>GPIO pin.</summary>
            public const int D11 = FEZCLR.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int D12 = FEZCLR.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int D13 = FEZCLR.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int Sda = FEZCLR.GpioPin.PB7;
            /// <summary>GPIO pin.</summary>
            public const int Scl = FEZCLR.GpioPin.PB6;
            /// <summary>GPIO pin.</summary>
            public const int WiFiReset = FEZCLR.GpioPin.PC2;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>API id.</summary>
            public const string Id = FEZCLR.AdcChannel.Id;

            /// <summary>ADC channel.</summary>
            public const int A0 = FEZCLR.AdcChannel.PA4;
            /// <summary>ADC channel.</summary>
            public const int A1 = FEZCLR.AdcChannel.PA5;
            /// <summary>ADC channel.</summary>
            public const int A2 = FEZCLR.AdcChannel.PA6;
            /// <summary>ADC channel.</summary>
            public const int A3 = FEZCLR.AdcChannel.PA7;
            /// <summary>ADC channel.</summary>
            public const int A4 = FEZCLR.AdcChannel.PB0;
            /// <summary>ADC channel.</summary>
            public const int A5 = FEZCLR.AdcChannel.PB1;
            /// <summary>ADC channel.</summary>
            public const int D2 = FEZCLR.AdcChannel.PC1;
            /// <summary>ADC channel.</summary>
            public const int D8 = FEZCLR.AdcChannel.PC0;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = FEZCLR.PwmPin.Controller1.Id;

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
                public const string Id = FEZCLR.PwmPin.Controller3.Id;

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
                public const string Id = FEZCLR.PwmPin.Controller4.Id;

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
            /// <summary>UART port on D1 (TX) and D0 (RX).</summary>
            public const string Usart1 = FEZCLR.UartPort.Usart1;
            /// <summary>UART port on WiFi TX (TX), WiFi RX (RX), WiFi CTS (CTS), and WiFi RTS (RTS).</summary>
            public const string WiFi = FEZCLR.UartPort.Usart2;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on SDA (SDA) and SCL (SCL).</summary>
            public const string I2c1 = FEZCLR.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on D11 (MOSI), D12 (MISO), and D13 (SCK).</summary>
            public const string Spi1 = FEZCLR.SpiBus.Spi1;
        }

        /// <summary>USB client port definitions.</summary>
        public static class UsbClientPort {
            /// <summary>USB client port on PA11 (DM), PA12 (DP), and VBUS (VBUS).</summary>
            public const string UsbOtg = FEZCLR.UsbClientPort.UsbOtg;
        }
    }
}
