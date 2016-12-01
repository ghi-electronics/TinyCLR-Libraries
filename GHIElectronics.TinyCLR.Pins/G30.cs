namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the G30.</summary>
    /// <remarks>I2C can be found on PB7 (SDA) and PB6 (SCL).</remarks>
    public static class G30 {
        /// <summary>The analog input precision supported by the board.</summary>
        public const int SupportedAnalogInputPrecision = 12;

        /// <summary>GPIO definitions.</summary>
        public static class Gpio {
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA0 = (Cpu.Pin)(0);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA1 = (Cpu.Pin)(1);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA2 = (Cpu.Pin)(2);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA3 = (Cpu.Pin)(3);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA4 = (Cpu.Pin)(4);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA5 = (Cpu.Pin)(5);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA6 = (Cpu.Pin)(6);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA7 = (Cpu.Pin)(7);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA8 = (Cpu.Pin)(8);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA9 = (Cpu.Pin)(9);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA10 = (Cpu.Pin)(10);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA11 = (Cpu.Pin)(11);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA12 = (Cpu.Pin)(12);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA13 = (Cpu.Pin)(13);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA14 = (Cpu.Pin)(14);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PA15 = (Cpu.Pin)(15);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB0 = (Cpu.Pin)(0 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB1 = (Cpu.Pin)(1 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB2 = (Cpu.Pin)(2 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB3 = (Cpu.Pin)(3 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB4 = (Cpu.Pin)(4 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB5 = (Cpu.Pin)(5 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB6 = (Cpu.Pin)(6 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB7 = (Cpu.Pin)(7 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB8 = (Cpu.Pin)(8 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB9 = (Cpu.Pin)(9 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB10 = (Cpu.Pin)(10 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB12 = (Cpu.Pin)(12 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB13 = (Cpu.Pin)(13 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB14 = (Cpu.Pin)(14 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PB15 = (Cpu.Pin)(15 + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC0 = (Cpu.Pin)(0 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC1 = (Cpu.Pin)(1 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC2 = (Cpu.Pin)(2 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC3 = (Cpu.Pin)(3 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC4 = (Cpu.Pin)(4 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC5 = (Cpu.Pin)(5 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC6 = (Cpu.Pin)(6 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC7 = (Cpu.Pin)(7 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC8 = (Cpu.Pin)(8 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC9 = (Cpu.Pin)(9 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC10 = (Cpu.Pin)(10 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC11 = (Cpu.Pin)(11 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC12 = (Cpu.Pin)(12 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC13 = (Cpu.Pin)(13 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC14 = (Cpu.Pin)(14 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PC15 = (Cpu.Pin)(15 + 32);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD2 = (Cpu.Pin)(2 + 48);
        }

        /// <summary>Analog input definitions.</summary>
        public static class AnalogInput {
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PA0 = (Cpu.AnalogChannel)0;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PA1 = (Cpu.AnalogChannel)1;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PA2 = (Cpu.AnalogChannel)2;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PA3 = (Cpu.AnalogChannel)3;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PA4 = (Cpu.AnalogChannel)4;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PA5 = (Cpu.AnalogChannel)5;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PA6 = (Cpu.AnalogChannel)6;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PA7 = (Cpu.AnalogChannel)7;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PB0 = (Cpu.AnalogChannel)8;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PB1 = (Cpu.AnalogChannel)9;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PC0 = (Cpu.AnalogChannel)10;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PC1 = (Cpu.AnalogChannel)11;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PC2 = (Cpu.AnalogChannel)12;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PC3 = (Cpu.AnalogChannel)13;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PC4 = (Cpu.AnalogChannel)14;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel PC5 = (Cpu.AnalogChannel)15;
        }

        /// <summary>PWM output definitions.</summary>
        public static class PwmOutput {
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PA8 = (Cpu.PWMChannel)0;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PA9 = (Cpu.PWMChannel)1;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PA10 = (Cpu.PWMChannel)2;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PA0 = (Cpu.PWMChannel)3;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PA1 = (Cpu.PWMChannel)4;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PA2 = (Cpu.PWMChannel)5;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PA3 = (Cpu.PWMChannel)6;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PC6 = (Cpu.PWMChannel)7;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PC7 = (Cpu.PWMChannel)8;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PC8 = (Cpu.PWMChannel)9;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PC9 = (Cpu.PWMChannel)10;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PB6 = (Cpu.PWMChannel)11;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PB7 = (Cpu.PWMChannel)12;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PB8 = (Cpu.PWMChannel)13;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PB9 = (Cpu.PWMChannel)14;
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            /// <summary>Serial port on PA9 (TX) and PA10 (RX).</summary>
            public const string Com1 = "COM1";
            /// <summary>Serial port on PA2 (TX), PA3 (RX), PA0 (CTS), and PA1 (RTS).</summary>
            public const string Com2 = "COM2";
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on PB5 (MOSI), PB4 (MISO), and PB3 (SCK).</summary>
            public const SPI.SPI_module Spi1 = SPI.SPI_module.SPI1;
            /// <summary>SPI bus on PB15 (MOSI), PB14 (MISO), and PB13 (SCK).</summary>
            public const SPI.SPI_module Spi2 = SPI.SPI_module.SPI2;
        }
    }
}