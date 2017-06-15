namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ.</summary>
    public static class FEZ {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            public const string Id = "GHIElectronics.TinyCLR.NativeApis.FEZ.GpioProvider\\0";

            /// <summary>GPIO pin for LED1.</summary>
            public const int Led1 = G30.GpioPin.PB9;
            /// <summary>GPIO pin for LED2.</summary>
            public const int Led2 = G30.GpioPin.PB8;
            /// <summary>GPIO pin for BTN1.</summary>
            public const int Btn1 = G30.GpioPin.PC13;
            /// <summary>GPIO pin for BTN2.</summary>
            public const int Btn2 = G30.GpioPin.PA15;
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
            public const int D2 = G30.GpioPin.PC1;
            /// <summary>GPIO pin.</summary>
            public const int D3 = G30.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int D4 = G30.GpioPin.PC15;
            /// <summary>GPIO pin.</summary>
            public const int D5 = G30.GpioPin.PA1;
            /// <summary>GPIO pin.</summary>
            public const int D6 = G30.GpioPin.PA0;
            /// <summary>GPIO pin.</summary>
            public const int D7 = G30.GpioPin.PC14;
            /// <summary>GPIO pin.</summary>
            public const int D8 = G30.GpioPin.PC0;
            /// <summary>GPIO pin.</summary>
            public const int D9 = G30.GpioPin.PC9;
            /// <summary>GPIO pin.</summary>
            public const int D10 = G30.GpioPin.PC8;
            /// <summary>GPIO pin.</summary>
            public const int D11 = G30.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int D12 = G30.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int D13 = G30.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int Sda = G30.GpioPin.PB7;
            /// <summary>GPIO pin.</summary>
            public const int Scl = G30.GpioPin.PB6;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            public const string Id = "GHIElectronics.TinyCLR.NativeApis.FEZ.AdcProvider\\0";

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
            public const int D2 = G30.AdcChannel.PC1;
            /// <summary>ADC channel.</summary>
            public const int D5 = G30.AdcChannel.PA1;
            /// <summary>ADC channel.</summary>
            public const int D6 = G30.AdcChannel.PA0;
            /// <summary>ADC channel.</summary>
            public const int D8 = G30.AdcChannel.PC0;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                public const string Id = "GHIElectronics.TinyCLR.NativeApis.FEZ.PwmProvider\\0";

                /// <summary>PWM pin.</summary>
                public const int D3 = 0;
                /// <summary>PWM pin.</summary>
                public const int D1 = 1;
                /// <summary>PWM pin.</summary>
                public const int D0 = 2;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller2 {
                public const string Id = "GHIElectronics.TinyCLR.NativeApis.FEZ.PwmProvider\\1";

                /// <summary>PWM pin.</summary>
                public const int D6 = 0;
                /// <summary>PWM pin.</summary>
                public const int D5 = 1;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller3 {
                public const string Id = "GHIElectronics.TinyCLR.NativeApis.FEZ.PwmProvider\\2";

                /// <summary>PWM pin.</summary>
                public const int D11 = 1;
                /// <summary>PWM pin.</summary>
                public const int D10 = 2;
                /// <summary>PWM pin.</summary>
                public const int D9 = 3;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller4 {
                public const string Id = "GHIElectronics.TinyCLR.NativeApis.FEZ.PwmProvider\\3";

                /// <summary>PWM pin.</summary>
                public const int Scl = 0;
                /// <summary>PWM pin.</summary>
                public const int Sda = 1;
            }
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            /// <summary>Serial port on D1 (TX) and D0 (RX).</summary>
            public const string Com1 = "GHIElectronics.TinyCLR.NativeApis.FEZ.UartProvider\\0";
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on SDA (SDA) and SCL (SCL).</summary>
            public const string I2c1 = "GHIElectronics.TinyCLR.NativeApis.FEZ.I2cProvider\\0";
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on D11 (MOSI), D12 (MISO), and D13 (SCK).</summary>
            public const string Spi1 = "GHIElectronics.TinyCLR.NativeApis.FEZ.SpiProvider\\0";
        }
    }
}