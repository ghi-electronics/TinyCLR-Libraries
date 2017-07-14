namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Lemur.</summary>
    public static class FEZLemur {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = G30.GpioPin.Id;

            /// <summary>GPIO pin for LED1.</summary>
            public const int Led1 = G30.GpioPin.PB9;
            /// <summary>GPIO pin for LED2.</summary>
            public const int Led2 = G30.GpioPin.PB8;
            /// <summary>GPIO pin for LDR0.</summary>
            public const int Ldr0 = G30.GpioPin.PA15;
            /// <summary>GPIO pin for LDR1.</summary>
            public const int Ldr1 = G30.GpioPin.PC13;
            /// <summary>GPIO pin for SD card detect.</summary>
            public const int SdCardDetect = G30.GpioPin.PB12;
            /// <summary>GPIO pin.</summary>
            public const int A0 = G30.GpioPin.PA4;
            /// <summary>GPIO pin.</summary>
            public const int A1 = G30.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int A2 = G30.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int A3 = G30.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int A4 = G30.GpioPin.PB0;
            /// <summary>GPIO pin.</summary>
            public const int A5 = G30.GpioPin.PB1;
            /// <summary>GPIO pin.</summary>
            public const int D0 = G30.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int D1 = G30.GpioPin.PA9;
            /// <summary>GPIO pin.</summary>
            public const int D2 = G30.GpioPin.PB7;
            /// <summary>GPIO pin.</summary>
            public const int D3 = G30.GpioPin.PB6;
            /// <summary>GPIO pin.</summary>
            public const int D4 = G30.GpioPin.PC15;
            /// <summary>GPIO pin.</summary>
            public const int D5 = G30.GpioPin.PA1;
            /// <summary>GPIO pin.</summary>
            public const int D6 = G30.GpioPin.PA0;
            /// <summary>GPIO pin.</summary>
            public const int D7 = G30.GpioPin.PC14;
            /// <summary>GPIO pin.</summary>
            public const int D8 = G30.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int D9 = G30.GpioPin.PA2;
            /// <summary>GPIO pin.</summary>
            public const int D10 = G30.GpioPin.PA3;
            /// <summary>GPIO pin.</summary>
            public const int D11 = G30.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int D12 = G30.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int D13 = G30.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int D20 = G30.GpioPin.PC0;
            /// <summary>GPIO pin.</summary>
            public const int D21 = G30.GpioPin.PC1;
            /// <summary>GPIO pin.</summary>
            public const int D22 = G30.GpioPin.PA13;
            /// <summary>GPIO pin.</summary>
            public const int D23 = G30.GpioPin.PA14;
            /// <summary>GPIO pin.</summary>
            public const int D24 = G30.GpioPin.PC2;
            /// <summary>GPIO pin.</summary>
            public const int D25 = G30.GpioPin.PC3;
            /// <summary>GPIO pin.</summary>
            public const int Mod = G30.GpioPin.PB10;
            /// <summary>GPIO pin.</summary>
            public const int D26 = G30.GpioPin.PB13;
            /// <summary>GPIO pin.</summary>
            public const int D27 = G30.GpioPin.PB14;
            /// <summary>GPIO pin.</summary>
            public const int D28 = G30.GpioPin.PB15;
            /// <summary>GPIO pin.</summary>
            public const int D29 = G30.GpioPin.PC6;
            /// <summary>GPIO pin.</summary>
            public const int D30 = G30.GpioPin.PC7;
            /// <summary>GPIO pin.</summary>
            public const int D31 = G30.GpioPin.PB2;
            /// <summary>GPIO pin.</summary>
            public const int D32 = G30.GpioPin.PC4;
            /// <summary>GPIO pin.</summary>
            public const int D33 = G30.GpioPin.PC5;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>API id.</summary>
            public const string Id = G30.AdcChannel.Id;

            /// <summary>ADC channel.</summary>
            public const int D6 = G30.AdcChannel.PA0;
            /// <summary>ADC channel.</summary>
            public const int D5 = G30.AdcChannel.PA1;
            /// <summary>ADC channel.</summary>
            public const int D9 = G30.AdcChannel.PA2;
            /// <summary>ADC channel.</summary>
            public const int D10 = G30.AdcChannel.PA3;
            /// <summary>ADC channel.</summary>
            public const int A0 = G30.AdcChannel.PA4;
            /// <summary>ADC channel.</summary>
            public const int A1 = G30.AdcChannel.PA5;
            /// <summary>ADC channel.</summary>
            public const int A2 = G30.AdcChannel.PA6;
            /// <summary>ADC channel.</summary>
            public const int A3 = G30.AdcChannel.PA7;
            /// <summary>ADC channel.</summary>
            public const int A4 = G30.AdcChannel.PB0;
            /// <summary>ADC channel.</summary>
            public const int A5 = G30.AdcChannel.PB1;
            /// <summary>ADC channel.</summary>
            public const int D20 = G30.AdcChannel.PC0;
            /// <summary>ADC channel.</summary>
            public const int D21 = G30.AdcChannel.PC1;
            /// <summary>ADC channel.</summary>
            public const int D24 = G30.AdcChannel.PC2;
            /// <summary>ADC channel.</summary>
            public const int D25 = G30.AdcChannel.PC3;
            /// <summary>ADC channel.</summary>
            public const int D32 = G30.AdcChannel.PC4;
            /// <summary>ADC channel.</summary>
            public const int D33 = G30.AdcChannel.PC5;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = G30.PwmPin.Controller1.Id;

                /// <summary>PWM pin.</summary>
                public const int D8 = G30.PwmPin.Controller1.PA8;
                /// <summary>PWM pin.</summary>
                public const int D1 = G30.PwmPin.Controller1.PA9;
                /// <summary>PWM pin.</summary>
                public const int D0 = G30.PwmPin.Controller1.PA10;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller2 {
                /// <summary>API id.</summary>
                public const string Id = G30.PwmPin.Controller2.Id;

                /// <summary>PWM pin.</summary>
                public const int D6 = G30.PwmPin.Controller2.PA0;
                /// <summary>PWM pin.</summary>
                public const int D5 = G30.PwmPin.Controller2.PA1;
                /// <summary>PWM pin.</summary>
                public const int D9 = G30.PwmPin.Controller2.PA2;
                /// <summary>PWM pin.</summary>
                public const int D10 = G30.PwmPin.Controller2.PA3;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller3 {
                /// <summary>API id.</summary>
                public const string Id = G30.PwmPin.Controller3.Id;

                /// <summary>PWM pin.</summary>
                public const int D29 = G30.PwmPin.Controller3.PC6;
                /// <summary>PWM pin.</summary>
                public const int D30 = G30.PwmPin.Controller3.PC7;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller4 {
                /// <summary>API id.</summary>
                public const string Id = G30.PwmPin.Controller4.Id;

                /// <summary>PWM pin.</summary>
                public const int D3 = G30.PwmPin.Controller4.PB6;
                /// <summary>PWM pin.</summary>
                public const int D2 = G30.PwmPin.Controller4.PB7;
                /// <summary>PWM pin.</summary>
                public const int Led2 = G30.PwmPin.Controller4.PB8;
                /// <summary>PWM pin.</summary>
                public const int Led1 = G30.PwmPin.Controller4.PB9;
            }
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>UART port on D1 (TX) and D0 (RX).</summary>
            public const string Usart1 = G30.UartPort.Usart1;
            /// <summary>UART port on D9 (TX), D10 (RX), D6 (CTS), and D5 (RTS).</summary>
            public const string Usart2 = G30.UartPort.Usart2;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on D2 (SDA) and D3 (SCL).</summary>
            public const string I2c1 = G30.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on D11 (MOSI), D12 (MISO), and D13 (SCK).</summary>
            public const string Spi1 = G30.SpiBus.Spi1;
            /// <summary>SPI bus on D28 (MOSI), D27 (MISO), and D26 (SCK).</summary>
            public const string Spi2 = G30.SpiBus.Spi2;
        }
    }
}