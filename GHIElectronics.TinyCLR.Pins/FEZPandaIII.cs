namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Panda III.</summary>
    /// <remarks>I2C can be found on D2 (SDA) and D3 (SCL).</remarks>
    public static class FEZPandaIII {
        /// <summary>The analog input precision supported by the board.</summary>
        public const int SupportedAnalogInputPrecision = G80.SupportedAnalogInputPrecision;

        /// <summary>The analog output precision supported by the board.</summary>
        public const int SupportedAnalogOutputPrecision = G80.SupportedAnalogOutputPrecision;

        /// <summary>GPIO definitions.</summary>
        public static class Gpio {
            /// <summary>The Cpu.Pin for LED 1.</summary>
            public const Cpu.Pin Led1 = G80.Gpio.PE14;
            /// <summary>The Cpu.Pin for LED 2.</summary>
            public const Cpu.Pin Led2 = G80.Gpio.PE13;
            /// <summary>The Cpu.Pin for LED 3.</summary>
            public const Cpu.Pin Led3 = G80.Gpio.PE11;
            /// <summary>The Cpu.Pin for LED 4.</summary>
            public const Cpu.Pin Led4 = G80.Gpio.PE9;
            /// <summary>The Cpu.Pin for the LDR0 button.</summary>
            public const Cpu.Pin Ldr0 = G80.Gpio.PE3;
            /// <summary>The Cpu.Pin for the LDR1 button.</summary>
            public const Cpu.Pin Ldr1 = G80.Gpio.PE4;
            /// <summary>The SD card detect pin.</summary>
            public const Cpu.Pin SdCardDetect = G80.Gpio.PD10;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin A0 = G80.Gpio.PA2;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin A1 = G80.Gpio.PA3;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin A2 = G80.Gpio.PA4;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin A3 = G80.Gpio.PA5;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin A4 = G80.Gpio.PA6;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin A5 = G80.Gpio.PA7;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D0 = G80.Gpio.PA10;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D1 = G80.Gpio.PA9;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D2 = G80.Gpio.PB7;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D3 = G80.Gpio.PB6;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D4 = G80.Gpio.PD0;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D5 = G80.Gpio.PB9;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D6 = G80.Gpio.PB8;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D7 = G80.Gpio.PD1;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D8 = G80.Gpio.PB1;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D9 = G80.Gpio.PB0;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D10 = G80.Gpio.PA15;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D11 = G80.Gpio.PB5;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D12 = G80.Gpio.PB4;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D13 = G80.Gpio.PB3;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D20 = G80.Gpio.PE2;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D21 = G80.Gpio.PB11;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D22 = G80.Gpio.PE5;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D23 = G80.Gpio.PC0;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D24 = G80.Gpio.PE6;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D25 = G80.Gpio.PC1;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D26 = G80.Gpio.PA13;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D27 = G80.Gpio.PC4;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D28 = G80.Gpio.PA14;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D29 = G80.Gpio.PC5;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D30 = G80.Gpio.PB12;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D31 = G80.Gpio.PA1;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D32 = G80.Gpio.PB13;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D33 = G80.Gpio.PA0;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D34 = G80.Gpio.PB2;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D35 = G80.Gpio.PB10;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin Mod = G80.Gpio.PE15;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D36 = G80.Gpio.PC2;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D37 = G80.Gpio.PD6;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D38 = G80.Gpio.PC3;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D39 = G80.Gpio.PD5;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D40 = G80.Gpio.PD9;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D41 = G80.Gpio.PD3;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D42 = G80.Gpio.PD8;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D43 = G80.Gpio.PD4;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D44 = G80.Gpio.PD11;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D45 = G80.Gpio.PE7;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D46 = G80.Gpio.PD12;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D47 = G80.Gpio.PE8;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D48 = G80.Gpio.PC6;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D49 = G80.Gpio.PE0;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D50 = G80.Gpio.PC7;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D51 = G80.Gpio.PE1;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D52 = G80.Gpio.PA8;
        }

        /// <summary>Analog input definitions.</summary>
        public static class AnalogInput {
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D33 = G80.AnalogInput.PA0;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D31 = G80.AnalogInput.PA1;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel A0 = G80.AnalogInput.PA2;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel A1 = G80.AnalogInput.PA3;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel A2 = G80.AnalogInput.PA4;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel A3 = G80.AnalogInput.PA5;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel A4 = G80.AnalogInput.PA6;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel A5 = G80.AnalogInput.PA7;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D9 = G80.AnalogInput.PB0;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D8 = G80.AnalogInput.PB1;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D23 = G80.AnalogInput.PC0;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D25 = G80.AnalogInput.PC1;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D36 = G80.AnalogInput.PC2;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D38 = G80.AnalogInput.PC3;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D27 = G80.AnalogInput.PC4;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D29 = G80.AnalogInput.PC5;
        }

        /// <summary>Analog output definitions.</summary>
        public static class AnalogOutput {
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogOutputChannel A2 = G80.AnalogOutput.PA4;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogOutputChannel A3 = G80.AnalogOutput.PA5;
        }

        /// <summary>PWM output definitions.</summary>
        public static class PwmOutput {
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel Led4 = G80.PwmOutput.PE9;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel Led3 = G80.PwmOutput.PE11;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel Led2 = G80.PwmOutput.PE13;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel Led1 = G80.PwmOutput.PE14;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D10 = G80.PwmOutput.PA15;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D13 = G80.PwmOutput.PB3;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D35 = G80.PwmOutput.PB10;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D21 = G80.PwmOutput.PB11;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D12 = G80.PwmOutput.PB4;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D11 = G80.PwmOutput.PB5;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D9 = G80.PwmOutput.PB0;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D8 = G80.PwmOutput.PB1;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D46 = G80.PwmOutput.PD12;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D48 = G80.PwmOutput.PC6;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D50 = G80.PwmOutput.PC7;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel A0 = G80.PwmOutput.PA2;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel A1 = G80.PwmOutput.PA3;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D6 = G80.PwmOutput.PB8;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D5 = G80.PwmOutput.PB9;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel A4 = G80.PwmOutput.PA6;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel A5 = G80.PwmOutput.PA7;
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

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on D11 (MOSI), D12 (MISO), and D13 (SCK).</summary>
            public const SPI.SPI_module Spi1 = G80.SpiBus.Spi1;
            /// <summary>SPI bus on D38 (MOSI), D36 (MISO), and D35 (SCK).</summary>
            public const SPI.SPI_module Spi2 = G80.SpiBus.Spi2;
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