namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Lemur.</summary>
    /// <remarks>I2C can be found on D2 (SDA) and D3 (SCL).</remarks>
    public static class FEZLemur {
        /// <summary>The analog input precision supported by the board.</summary>
        public const int SupportedAnalogInputPrecision = G30.SupportedAnalogInputPrecision;

        /// <summary>GPIO definitions.</summary>
        public static class Gpio {
            /// <summary>The Cpu.Pin for LED 1.</summary>
            public const Cpu.Pin Led1 = G30.Gpio.PB9;
            /// <summary>The Cpu.Pin for LED 2.</summary>
            public const Cpu.Pin Led2 = G30.Gpio.PB8;
            /// <summary>The Cpu.Pin for the LDR0 button.</summary>
            public const Cpu.Pin Ldr0 = G30.Gpio.PA15;
            /// <summary>The Cpu.Pin for the LDR1 button.</summary>
            public const Cpu.Pin Ldr1 = G30.Gpio.PC13;
            /// <summary>The SD card detect pin.</summary>
            public const Cpu.Pin SdCardDetect = G30.Gpio.PB12;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin A0 = G30.Gpio.PA4;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin A1 = G30.Gpio.PA5;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin A2 = G30.Gpio.PA6;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin A3 = G30.Gpio.PA7;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin A4 = G30.Gpio.PB0;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin A5 = G30.Gpio.PB1;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D0 = G30.Gpio.PA10;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D1 = G30.Gpio.PA9;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D2 = G30.Gpio.PB7;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D3 = G30.Gpio.PB6;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D4 = G30.Gpio.PC15;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D5 = G30.Gpio.PA1;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D6 = G30.Gpio.PA0;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D7 = G30.Gpio.PC14;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D8 = G30.Gpio.PA8;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D9 = G30.Gpio.PA2;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D10 = G30.Gpio.PA3;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D11 = G30.Gpio.PB5;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D12 = G30.Gpio.PB4;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D13 = G30.Gpio.PB3;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D20 = G30.Gpio.PC0;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D21 = G30.Gpio.PC1;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D22 = G30.Gpio.PA13;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D23 = G30.Gpio.PA14;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D24 = G30.Gpio.PC2;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D25 = G30.Gpio.PC3;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin Mod = G30.Gpio.PB10;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D26 = G30.Gpio.PB13;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D27 = G30.Gpio.PB14;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D28 = G30.Gpio.PB15;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D29 = G30.Gpio.PC6;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D30 = G30.Gpio.PC7;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D31 = G30.Gpio.PB2;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D32 = G30.Gpio.PC4;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D33 = G30.Gpio.PC5;
        }

        /// <summary>Analog input definitions.</summary>
        public static class AnalogInput {
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D6 = G30.AnalogInput.PA0;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D5 = G30.AnalogInput.PA1;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D9 = G30.AnalogInput.PA2;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D10 = G30.AnalogInput.PA3;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel A0 = G30.AnalogInput.PA4;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel A1 = G30.AnalogInput.PA5;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel A2 = G30.AnalogInput.PA6;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel A3 = G30.AnalogInput.PA7;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel A4 = G30.AnalogInput.PB0;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel A5 = G30.AnalogInput.PB1;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D20 = G30.AnalogInput.PC0;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D21 = G30.AnalogInput.PC1;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D24 = G30.AnalogInput.PC2;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D25 = G30.AnalogInput.PC3;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D32 = G30.AnalogInput.PC4;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D33 = G30.AnalogInput.PC5;
        }

        /// <summary>PWM output definitions.</summary>
        public static class PwmOutput {
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D8 = G30.PwmOutput.PA8;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D1 = G30.PwmOutput.PA9;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D0 = G30.PwmOutput.PA10;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel Led2 = G30.PwmOutput.PB8;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel Led1 = G30.PwmOutput.PB9;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D6 = G30.PwmOutput.PA0;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D5 = G30.PwmOutput.PA1;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D9 = G30.PwmOutput.PA2;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D10 = G30.PwmOutput.PA3;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D29 = G30.PwmOutput.PC6;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D30 = G30.PwmOutput.PC7;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D3 = G30.PwmOutput.PB6;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D2 = G30.PwmOutput.PB7;
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            /// <summary>Serial port on D1 (TX) and D0 (RX).</summary>
            public const string Com1 = G30.SerialPort.Com1;
            /// <summary>Serial port on D9 (TX), D10 (RX), D6 (CTS), and D5 (RTS).</summary>
            public const string Com2 = G30.SerialPort.Com2;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on D11 (MOSI), D12 (MISO), and D13 (SCK).</summary>
            public const SPI.SPI_module Spi1 = G30.SpiBus.Spi1;
            /// <summary>SPI bus on D28 (MOSI), D27 (MISO), and D26 (SCK).</summary>
            public const SPI.SPI_module Spi2 = G30.SpiBus.Spi2;
        }
    }
}