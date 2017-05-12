namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ.</summary>
    public static class FEZ {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>GPIO pin.</summary>
            public const int PA0 = 0;
            /// <summary>GPIO pin.</summary>
            public const int PA1 = 1;
            /// <summary>GPIO pin.</summary>
            public const int PA2 = 2;
            /// <summary>GPIO pin.</summary>
            public const int PA3 = 3;
            /// <summary>GPIO pin.</summary>
            public const int PA4 = 4;
            /// <summary>GPIO pin.</summary>
            public const int PA5 = 5;
            /// <summary>GPIO pin.</summary>
            public const int PA6 = 6;
            /// <summary>GPIO pin.</summary>
            public const int PA7 = 7;
            /// <summary>GPIO pin.</summary>
            public const int PA8 = 8;
            /// <summary>GPIO pin.</summary>
            public const int PA9 = 9;
            /// <summary>GPIO pin.</summary>
            public const int PA10 = 10;
            /// <summary>GPIO pin.</summary>
            public const int PA11 = 11;
            /// <summary>GPIO pin.</summary>
            public const int PA12 = 12;
            /// <summary>GPIO pin.</summary>
            public const int PA13 = 13;
            /// <summary>GPIO pin.</summary>
            public const int PA14 = 14;
            /// <summary>GPIO pin.</summary>
            public const int PA15 = 15;
            /// <summary>GPIO pin.</summary>
            public const int PB0 = 0 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB1 = 1 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB2 = 2 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB3 = 3 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB4 = 4 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB5 = 5 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB6 = 6 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB7 = 7 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB8 = 8 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB9 = 9 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB10 = 10 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB12 = 12 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB13 = 13 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB14 = 14 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB15 = 15 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PC0 = 0 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC1 = 1 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC2 = 2 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC3 = 3 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC4 = 4 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC5 = 5 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC6 = 6 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC7 = 7 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC8 = 8 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC9 = 9 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC10 = 10 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC11 = 11 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC12 = 12 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC13 = 13 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC14 = 14 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC15 = 15 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PD2 = 2 + 48;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>ADC channel.</summary>
            public const int PA0 = 0;
            /// <summary>ADC channel.</summary>
            public const int PA1 = 1;
            /// <summary>ADC channel.</summary>
            public const int PA2 = 2;
            /// <summary>ADC channel.</summary>
            public const int PA3 = 3;
            /// <summary>ADC channel.</summary>
            public const int PA4 = 4;
            /// <summary>ADC channel.</summary>
            public const int PA5 = 5;
            /// <summary>ADC channel.</summary>
            public const int PA6 = 6;
            /// <summary>ADC channel.</summary>
            public const int PA7 = 7;
            /// <summary>ADC channel.</summary>
            public const int PB0 = 8;
            /// <summary>ADC channel.</summary>
            public const int PB1 = 9;
            /// <summary>ADC channel.</summary>
            public const int PC0 = 10;
            /// <summary>ADC channel.</summary>
            public const int PC1 = 11;
            /// <summary>ADC channel.</summary>
            public const int PC2 = 12;
            /// <summary>ADC channel.</summary>
            public const int PC3 = 13;
            /// <summary>ADC channel.</summary>
            public const int PC4 = 14;
            /// <summary>ADC channel.</summary>
            public const int PC5 = 15;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                public const string Id = "CON1";

                /// <summary>PWM pin.</summary>
                public const int PA8 = 0;
                /// <summary>PWM pin.</summary>
                public const int PA9 = 1;
                /// <summary>PWM pin.</summary>
                public const int PA10 = 2;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller2 {
                public const string Id = "CON2";

                /// <summary>PWM pin.</summary>
                public const int PA0 = 0;
                /// <summary>PWM pin.</summary>
                public const int PA1 = 1;
                /// <summary>PWM pin.</summary>
                public const int PA2 = 2;
                /// <summary>PWM pin.</summary>
                public const int PA3 = 3;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller3 {
                public const string Id = "CON3";

                /// <summary>PWM pin.</summary>
                public const int PC6 = 0;
                /// <summary>PWM pin.</summary>
                public const int PB5 = 1;
                /// <summary>PWM pin.</summary>
                public const int PC8 = 2;
                /// <summary>PWM pin.</summary>
                public const int PC9 = 3;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller4 {
                public const string Id = "CON4";

                /// <summary>PWM pin.</summary>
                public const int PB6 = 0;
                /// <summary>PWM pin.</summary>
                public const int PB7 = 1;
                /// <summary>PWM pin.</summary>
                public const int PB8 = 2;
                /// <summary>PWM pin.</summary>
                public const int PB9 = 3;
            }
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            /// <summary>Serial port on PA9 (TX) and PA10 (RX).</summary>
            public const string Com1 = "COM1";
            /// <summary>Serial port on PA2 (TX), PA3 (RX), PA0 (CTS), and PA1 (RTS).</summary>
            public const string Com2 = "COM2";
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on PB7 (SDA) and PB6 (SCL).</summary>
            public const string I2c1 = "I2C1";
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on PB5 (MOSI), PB4 (MISO), and PB3 (SCK).</summary>
            public const string Spi1 = "SPI1";
            /// <summary>SPI bus on PB15 (MOSI), PB14 (MISO), and PB13 (SCK).</summary>
            public const string Spi2 = "SPI2";
        }
    }
}