namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Cobra III.</summary>
    /// <remarks>I2C can be found on D2 (SDA) and D3 (SCL).</remarks>
    public static class FEZCobraIII {
        /// <summary>The analog input precision supported by the board.</summary>
        public const int SupportedAnalogInputPrecision = G120.SupportedAnalogInputPrecision;

        /// <summary>The analog output precision supported by the board.</summary>
        public const int SupportedAnalogOutputPrecision = G120.SupportedAnalogOutputPrecision;

        /// <summary>GPIO definitions.</summary>
        public static class Gpio {
            /// <summary>The Cpu.Pin for LED 1.</summary>
            public const Cpu.Pin Led1 = G120.P1_14;
            /// <summary>The Cpu.Pin for LED 2.</summary>
            public const Cpu.Pin Led2 = G120.P1_19;
            /// <summary>The Cpu.Pin for the LDR0 button.</summary>
            public const Cpu.Pin Ldr0 = G120.P2_10;
            /// <summary>The Cpu.Pin for the LDR1 button.</summary>
            public const Cpu.Pin Ldr1 = G120.P0_22;
            /// <summary>The SD card detect pin.</summary>
            public const Cpu.Pin SdCardDetect = G120.P2_11;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D0 = G120.P0_3;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D1 = G120.P0_2;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D2 = G120.P0_27;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D3 = G120.P0_28;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D4 = G120.P0_0;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D5 = G120.P3_26;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D6 = G120.P3_25;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D7 = G120.P0_1;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D8 = G120.P1_10;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D9 = G120.P1_9;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D10 = G120.P1_8;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D11 = G120.P0_18;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D12 = G120.P0_17;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D13 = G120.P0_15;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D14 = G120.P1_30;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D15 = G120.P1_31;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D16 = G120.P0_25;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D17 = G120.P0_26;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D18 = G120.P0_12;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D19 = G120.P0_13;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D20 = G120.P0_11;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D21 = G120.P0_10;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D22 = G120.P1_17;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D23 = G120.P1_16;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D24 = G120.P1_15;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D25 = G120.P1_0;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D26 = G120.P1_4;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D27 = G120.P1_1;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D28 = G120.P0_16;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D29 = G120.P2_0;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin Mod = G120.P2_1;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D30 = G120.P2_3;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D31 = G120.P2_5;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D32 = G120.P2_2;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D33 = G120.P2_12;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D34 = G120.P2_6;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D35 = G120.P2_7;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D36 = G120.P2_8;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D37 = G120.P2_9;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D38 = G120.P2_4;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D39 = G120.P1_20;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D40 = G120.P1_21;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D41 = G120.P1_22;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D42 = G120.P1_23;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D43 = G120.P1_24;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D44 = G120.P1_25;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D45 = G120.P2_13;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D46 = G120.P1_26;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D47 = G120.P1_27;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D48 = G120.P1_28;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D49 = G120.P1_29;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D50 = G120.P0_4;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D51 = G120.P0_5;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D52 = G120.P0_24;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D54 = G120.P0_23;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D56 = G120.P1_5;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D58 = G120.P3_24;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D59 = G120.P4_28;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D60 = G120.P0_6;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D61 = G120.P4_29;
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin D62 = G120.P2_21;
        }

        /// <summary>Analog input definitions.</summary>
        public static class AnalogInput {
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D18 = G120.AnalogInput.P0_12;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D19 = G120.AnalogInput.P0_13;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D54 = G120.AnalogInput.P0_23;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D52 = G120.AnalogInput.P0_24;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D16 = G120.AnalogInput.P0_25;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D17 = G120.AnalogInput.P0_26;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D14 = G120.AnalogInput.P1_30;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel D15 = G120.AnalogInput.P1_31;
        }

        /// <summary>Analog output definitions.</summary>
        public static class AnalogOutput {
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogOutputChannel D17 = G120.AnalogOutput.P0_26;
        }

        /// <summary>PWM output definitions.</summary>
        public static class PwmOutput {
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D56 = G120.PwmOutput.P1_5;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D30 = G120.PwmOutput.P2_3;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D38 = G120.PwmOutput.P2_4;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D31 = G120.PwmOutput.P2_5;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D58 = G120.PwmOutput.P3_24;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D6 = G120.PwmOutput.P3_25;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel D5 = G120.PwmOutput.P3_26;
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            /// <summary>Serial port on D1 (TX) and D0 (RX).</summary>
            public const string Com1 = G120.SerialPort.Com1;
            /// <summary>Serial port on D29 (TX), D28 (RX), D12 (CTS), and D60 (RTS).</summary>
            public const string Com2 = G120.SerialPort.Com2;
            /// <summary>Serial port on D21 (TX) and D20 (RX).</summary>
            public const string Com3 = G120.SerialPort.Com3;
            /// <summary>Serial port on D59 (TX) and D61 (RX).</summary>
            public const string Com4 = G120.SerialPort.Com4;
            /// <summary>Serial port on D49 (TX) and D37 (RX).</summary>
            public const string Com5 = G120.SerialPort.Com5;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on D11 (MOSI), D12 (MISO), and D13 (SCK).</summary>
            public const SPI.SPI_module Spi1 = G120.SpiBus.Spi1;
            /// <summary>SPI bus on D57 (MOSI), D55 (MISO), and D53 (SCK).</summary>
            public const SPI.SPI_module Spi2 = G120.SpiBus.Spi2;
            /// <summary>SPI bus on D27 (MOSI), D26 (MISO), and D25 (SCK).</summary>
            public const SPI.SPI_module Spi3 = G120.SpiBus.Spi3;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN channel on D7 (TX) and D4 (RX).</summary>
            public const int Can1 = G120.CanBus.Can1;
            /// <summary>CAN channel on D51 (TX) and D50 (RX).</summary>
            public const int Can2 = G120.CanBus.Can2;
        }
    }
}
