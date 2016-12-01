namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Panda III.</summary>
    public static class FEZPandaIII {
        /// <summary>The analog input precision supported by the board.</summary>
        public const int SupportedAnalogInputPrecision = G80.SupportedAnalogInputPrecision;

        /// <summary>The analog output precision supported by the board.</summary>
        public const int SupportedAnalogOutputPrecision = G80.SupportedAnalogOutputPrecision;

        /// <summary>GPIO definitions.</summary>
        public static class Gpio {
            /// <summary>The pin for LED 1.</summary>
            public const int Led1 = G80.Gpio.PE14;
            /// <summary>The pin for LED 2.</summary>
            public const int Led2 = G80.Gpio.PE13;
            /// <summary>The pin for LED 3.</summary>
            public const int Led3 = G80.Gpio.PE11;
            /// <summary>The pin for LED 4.</summary>
            public const int Led4 = G80.Gpio.PE9;
            /// <summary>The pin for the LDR0 button.</summary>
            public const int Ldr0 = G80.Gpio.PE3;
            /// <summary>The pin for the LDR1 button.</summary>
            public const int Ldr1 = G80.Gpio.PE4;
            /// <summary>The SD card detect pin.</summary>
            public const int SdCardDetect = G80.Gpio.PD10;
            /// <summary>GPIO pin.</summary>
            public const int A0 = G80.Gpio.PA2;
            /// <summary>GPIO pin.</summary>
            public const int A1 = G80.Gpio.PA3;
            /// <summary>GPIO pin.</summary>
            public const int A2 = G80.Gpio.PA4;
            /// <summary>GPIO pin.</summary>
            public const int A3 = G80.Gpio.PA5;
            /// <summary>GPIO pin.</summary>
            public const int A4 = G80.Gpio.PA6;
            /// <summary>GPIO pin.</summary>
            public const int A5 = G80.Gpio.PA7;
            /// <summary>GPIO pin.</summary>
            public const int D0 = G80.Gpio.PA10;
            /// <summary>GPIO pin.</summary>
            public const int D1 = G80.Gpio.PA9;
            /// <summary>GPIO pin.</summary>
            public const int D2 = G80.Gpio.PB7;
            /// <summary>GPIO pin.</summary>
            public const int D3 = G80.Gpio.PB6;
            /// <summary>GPIO pin.</summary>
            public const int D4 = G80.Gpio.PD0;
            /// <summary>GPIO pin.</summary>
            public const int D5 = G80.Gpio.PB9;
            /// <summary>GPIO pin.</summary>
            public const int D6 = G80.Gpio.PB8;
            /// <summary>GPIO pin.</summary>
            public const int D7 = G80.Gpio.PD1;
            /// <summary>GPIO pin.</summary>
            public const int D8 = G80.Gpio.PB1;
            /// <summary>GPIO pin.</summary>
            public const int D9 = G80.Gpio.PB0;
            /// <summary>GPIO pin.</summary>
            public const int D10 = G80.Gpio.PA15;
            /// <summary>GPIO pin.</summary>
            public const int D11 = G80.Gpio.PB5;
            /// <summary>GPIO pin.</summary>
            public const int D12 = G80.Gpio.PB4;
            /// <summary>GPIO pin.</summary>
            public const int D13 = G80.Gpio.PB3;
            /// <summary>GPIO pin.</summary>
            public const int D20 = G80.Gpio.PE2;
            /// <summary>GPIO pin.</summary>
            public const int D21 = G80.Gpio.PB11;
            /// <summary>GPIO pin.</summary>
            public const int D22 = G80.Gpio.PE5;
            /// <summary>GPIO pin.</summary>
            public const int D23 = G80.Gpio.PC0;
            /// <summary>GPIO pin.</summary>
            public const int D24 = G80.Gpio.PE6;
            /// <summary>GPIO pin.</summary>
            public const int D25 = G80.Gpio.PC1;
            /// <summary>GPIO pin.</summary>
            public const int D26 = G80.Gpio.PA13;
            /// <summary>GPIO pin.</summary>
            public const int D27 = G80.Gpio.PC4;
            /// <summary>GPIO pin.</summary>
            public const int D28 = G80.Gpio.PA14;
            /// <summary>GPIO pin.</summary>
            public const int D29 = G80.Gpio.PC5;
            /// <summary>GPIO pin.</summary>
            public const int D30 = G80.Gpio.PB12;
            /// <summary>GPIO pin.</summary>
            public const int D31 = G80.Gpio.PA1;
            /// <summary>GPIO pin.</summary>
            public const int D32 = G80.Gpio.PB13;
            /// <summary>GPIO pin.</summary>
            public const int D33 = G80.Gpio.PA0;
            /// <summary>GPIO pin.</summary>
            public const int D34 = G80.Gpio.PB2;
            /// <summary>GPIO pin.</summary>
            public const int D35 = G80.Gpio.PB10;
            /// <summary>GPIO pin.</summary>
            public const int Mod = G80.Gpio.PE15;
            /// <summary>GPIO pin.</summary>
            public const int D36 = G80.Gpio.PC2;
            /// <summary>GPIO pin.</summary>
            public const int D37 = G80.Gpio.PD6;
            /// <summary>GPIO pin.</summary>
            public const int D38 = G80.Gpio.PC3;
            /// <summary>GPIO pin.</summary>
            public const int D39 = G80.Gpio.PD5;
            /// <summary>GPIO pin.</summary>
            public const int D40 = G80.Gpio.PD9;
            /// <summary>GPIO pin.</summary>
            public const int D41 = G80.Gpio.PD3;
            /// <summary>GPIO pin.</summary>
            public const int D42 = G80.Gpio.PD8;
            /// <summary>GPIO pin.</summary>
            public const int D43 = G80.Gpio.PD4;
            /// <summary>GPIO pin.</summary>
            public const int D44 = G80.Gpio.PD11;
            /// <summary>GPIO pin.</summary>
            public const int D45 = G80.Gpio.PE7;
            /// <summary>GPIO pin.</summary>
            public const int D46 = G80.Gpio.PD12;
            /// <summary>GPIO pin.</summary>
            public const int D47 = G80.Gpio.PE8;
            /// <summary>GPIO pin.</summary>
            public const int D48 = G80.Gpio.PC6;
            /// <summary>GPIO pin.</summary>
            public const int D49 = G80.Gpio.PE0;
            /// <summary>GPIO pin.</summary>
            public const int D50 = G80.Gpio.PC7;
            /// <summary>GPIO pin.</summary>
            public const int D51 = G80.Gpio.PE1;
            /// <summary>GPIO pin.</summary>
            public const int D52 = G80.Gpio.PA8;
        }

        /// <summary>Analog input definitions.</summary>
        public static class AnalogInput {
            /// <summary>Analog channel.</summary>
            public const int D33 = G80.AnalogInput.PA0;
            /// <summary>Analog channel.</summary>
            public const int D31 = G80.AnalogInput.PA1;
            /// <summary>Analog channel.</summary>
            public const int A0 = G80.AnalogInput.PA2;
            /// <summary>Analog channel.</summary>
            public const int A1 = G80.AnalogInput.PA3;
            /// <summary>Analog channel.</summary>
            public const int A2 = G80.AnalogInput.PA4;
            /// <summary>Analog channel.</summary>
            public const int A3 = G80.AnalogInput.PA5;
            /// <summary>Analog channel.</summary>
            public const int A4 = G80.AnalogInput.PA6;
            /// <summary>Analog channel.</summary>
            public const int A5 = G80.AnalogInput.PA7;
            /// <summary>Analog channel.</summary>
            public const int D9 = G80.AnalogInput.PB0;
            /// <summary>Analog channel.</summary>
            public const int D8 = G80.AnalogInput.PB1;
            /// <summary>Analog channel.</summary>
            public const int D23 = G80.AnalogInput.PC0;
            /// <summary>Analog channel.</summary>
            public const int D25 = G80.AnalogInput.PC1;
            /// <summary>Analog channel.</summary>
            public const int D36 = G80.AnalogInput.PC2;
            /// <summary>Analog channel.</summary>
            public const int D38 = G80.AnalogInput.PC3;
            /// <summary>Analog channel.</summary>
            public const int D27 = G80.AnalogInput.PC4;
            /// <summary>Analog channel.</summary>
            public const int D29 = G80.AnalogInput.PC5;
        }

        /// <summary>Analog output definitions.</summary>
        public static class AnalogOutput {
            /// <summary>Analog channel.</summary>
            public const int A2 = G80.AnalogOutput.PA4;
            /// <summary>Analog channel.</summary>
            public const int A3 = G80.AnalogOutput.PA5;
        }

        /// <summary>PWM output definitions.</summary>
        public static class PwmOutput {
            /// <summary>PWM channel.</summary>
            public const int Led4 = G80.PwmOutput.PE9;
            /// <summary>PWM channel.</summary>
            public const int Led3 = G80.PwmOutput.PE11;
            /// <summary>PWM channel.</summary>
            public const int Led2 = G80.PwmOutput.PE13;
            /// <summary>PWM channel.</summary>
            public const int Led1 = G80.PwmOutput.PE14;
            /// <summary>PWM channel.</summary>
            public const int D10 = G80.PwmOutput.PA15;
            /// <summary>PWM channel.</summary>
            public const int D13 = G80.PwmOutput.PB3;
            /// <summary>PWM channel.</summary>
            public const int D35 = G80.PwmOutput.PB10;
            /// <summary>PWM channel.</summary>
            public const int D21 = G80.PwmOutput.PB11;
            /// <summary>PWM channel.</summary>
            public const int D12 = G80.PwmOutput.PB4;
            /// <summary>PWM channel.</summary>
            public const int D11 = G80.PwmOutput.PB5;
            /// <summary>PWM channel.</summary>
            public const int D9 = G80.PwmOutput.PB0;
            /// <summary>PWM channel.</summary>
            public const int D8 = G80.PwmOutput.PB1;
            /// <summary>PWM channel.</summary>
            public const int D46 = G80.PwmOutput.PD12;
            /// <summary>PWM channel.</summary>
            public const int D48 = G80.PwmOutput.PC6;
            /// <summary>PWM channel.</summary>
            public const int D50 = G80.PwmOutput.PC7;
            /// <summary>PWM channel.</summary>
            public const int A0 = G80.PwmOutput.PA2;
            /// <summary>PWM channel.</summary>
            public const int A1 = G80.PwmOutput.PA3;
            /// <summary>PWM channel.</summary>
            public const int D6 = G80.PwmOutput.PB8;
            /// <summary>PWM channel.</summary>
            public const int D5 = G80.PwmOutput.PB9;
            /// <summary>PWM channel.</summary>
            public const int A4 = G80.PwmOutput.PA6;
            /// <summary>PWM channel.</summary>
            public const int A5 = G80.PwmOutput.PA7;
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            /// <summary>Serial port on D1 (TX) and D0 (RX).</summary>
            public const string Com1 = G80.SerialPort.Com1;
            /// <summary>Serial port on D39 (TX), D37 (RX), D41 (CTS), and D43 (RTS).</summary>
            public const string Com2 = G80.SerialPort.Com2;
            /// <summary>Serial port on D42 (TX), D40 (RX), D44 (CTS), and D46 (RTS).</summary>
            public const string Com3 = G80.SerialPort.Com3;
            /// <summary>Serial port on D33 (TX) and D31 (RX).</summary>
            public const string Com4 = G80.SerialPort.Com4;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>SPI bus on D2 (SDA) and D3 (SCL).</summary>
            public const string I2c1 = G80.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on D11 (MOSI), D12 (MISO), and D13 (SCK).</summary>
            public const string Spi1 = G80.SpiBus.Spi1;
            /// <summary>SPI bus on D38 (MOSI), D36 (MISO), and D35 (SCK).</summary>
            public const string Spi2 = G80.SpiBus.Spi2;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN channel on D7 (TX) and D4 (RX).</summary>
            public const int Can1 = G80.CanBus.Can1;
            /// <summary>CAN channel on D32 (TX) and D30 (RX).</summary>
            public const int Can2 = G80.CanBus.Can2;
        }
    }
}