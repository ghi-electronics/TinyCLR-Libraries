namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the G80.</summary>
    /// <remarks>I2C can be found on PB7 (SDA) and PB6 (SCL).</remarks>
    public static class G80 {
        /// <summary>The analog input precision supported by the board.</summary>
        public const int SupportedAnalogInputPrecision = 12;

        /// <summary>The analog output precision supported by the board.</summary>
        public const int SupportedAnalogOutputPrecision = 12;

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
            public const Cpu.Pin PB11 = (Cpu.Pin)(11 + 16);
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
            public const Cpu.Pin PD0 = (Cpu.Pin)(0 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD1 = (Cpu.Pin)(1 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD2 = (Cpu.Pin)(2 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD3 = (Cpu.Pin)(3 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD4 = (Cpu.Pin)(4 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD5 = (Cpu.Pin)(5 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD6 = (Cpu.Pin)(6 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD7 = (Cpu.Pin)(7 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD8 = (Cpu.Pin)(8 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD9 = (Cpu.Pin)(9 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD10 = (Cpu.Pin)(10 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD11 = (Cpu.Pin)(11 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD12 = (Cpu.Pin)(12 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD13 = (Cpu.Pin)(13 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD14 = (Cpu.Pin)(14 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PD15 = (Cpu.Pin)(15 + 48);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE0 = (Cpu.Pin)(0 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE1 = (Cpu.Pin)(1 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE2 = (Cpu.Pin)(2 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE3 = (Cpu.Pin)(3 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE4 = (Cpu.Pin)(4 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE5 = (Cpu.Pin)(5 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE6 = (Cpu.Pin)(6 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE7 = (Cpu.Pin)(7 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE8 = (Cpu.Pin)(8 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE9 = (Cpu.Pin)(9 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE10 = (Cpu.Pin)(10 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE11 = (Cpu.Pin)(11 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE12 = (Cpu.Pin)(12 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE13 = (Cpu.Pin)(13 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE14 = (Cpu.Pin)(14 + 64);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin PE15 = (Cpu.Pin)(15 + 64);
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

        /// <summary>Analog output definitions.</summary>
        public static class AnalogOutput {
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogOutputChannel PA4 = (Cpu.AnalogOutputChannel)0;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogOutputChannel PA5 = (Cpu.AnalogOutputChannel)1;
        }

        /// <summary>PWM output definitions.</summary>
        public static class PwmOutput {
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PE9 = (Cpu.PWMChannel)0;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PE11 = (Cpu.PWMChannel)1;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PE13 = (Cpu.PWMChannel)2;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PE14 = (Cpu.PWMChannel)3;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PA15 = (Cpu.PWMChannel)4;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PB3 = (Cpu.PWMChannel)5;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PB10 = (Cpu.PWMChannel)6;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PB11 = (Cpu.PWMChannel)7;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PB4 = (Cpu.PWMChannel)8;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PB5 = (Cpu.PWMChannel)9;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PB0 = (Cpu.PWMChannel)10;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PB1 = (Cpu.PWMChannel)11;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PD12 = (Cpu.PWMChannel)12;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PD13 = (Cpu.PWMChannel)13;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PD14 = (Cpu.PWMChannel)14;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PD15 = (Cpu.PWMChannel)15;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PC6 = (Cpu.PWMChannel)16;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PC7 = (Cpu.PWMChannel)17;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PC8 = (Cpu.PWMChannel)18;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PC9 = (Cpu.PWMChannel)19;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PA2 = (Cpu.PWMChannel)20;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PA3 = (Cpu.PWMChannel)21;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PB8 = (Cpu.PWMChannel)22;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PB9 = (Cpu.PWMChannel)23;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PA6 = (Cpu.PWMChannel)24;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel PA7 = (Cpu.PWMChannel)25;
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            /// <summary>Serial port on PA9 (TX) and PA10 (RX).</summary>
            public const string Com1 = "COM1";
            /// <summary>Serial port on PD5 (TX), PD6 (RX), PD3 (CTS), and PD4 (RTS).</summary>
            public const string Com2 = "COM2";
            /// <summary>Serial port on PD8 (TX), PD9 (RX), PD11 (CTS), and PD12 (RTS).</summary>
            public const string Com3 = "COM3";
            /// <summary>Serial port on PA0 (TX) and PA1 (RX).</summary>
            public const string Com4 = "COM4";
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on PB5 (MOSI), PB4 (MISO), and PB3 (SCK).</summary>
            public const SPI.SPI_module Spi1 = SPI.SPI_module.SPI1;
            /// <summary>SPI bus on PC3 (MOSI), PC2 (MISO), and PB10 (SCK).</summary>
            public const SPI.SPI_module Spi2 = SPI.SPI_module.SPI2;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN channel on PD1 (TX) and PD0 (RX).</summary>
            public const int Can1 = 1;
            /// <summary>CAN channel on PB13 (TX) and PB12 (RX).</summary>
            public const int Can2 = 2;
        }
    }
}