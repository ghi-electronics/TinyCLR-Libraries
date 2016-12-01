namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the G120E.</summary>
    /// <remarks>I2C can be found on P0_27 (SDA) and P0_28 (SCL).</remarks>
    public static class G120E {
        /// <summary>The analog input precision supported by the board.</summary>
        public const int SupportedAnalogInputPrecision = 12;

        /// <summary>The analog output precision supported by the board.</summary>
        public const int SupportedAnalogOutputPrecision = 10;

        /// <summary>GPIO definitions.</summary>
        public static class Gpio {
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_0 = (Cpu.Pin)((0 * 32) + 0);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_1 = (Cpu.Pin)((0 * 32) + 1);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_2 = (Cpu.Pin)((0 * 32) + 2);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_3 = (Cpu.Pin)((0 * 32) + 3);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_4 = (Cpu.Pin)((0 * 32) + 4);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_5 = (Cpu.Pin)((0 * 32) + 5);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_6 = (Cpu.Pin)((0 * 32) + 6);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_10 = (Cpu.Pin)((0 * 32) + 10);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_11 = (Cpu.Pin)((0 * 32) + 11);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_12 = (Cpu.Pin)((0 * 32) + 12);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_13 = (Cpu.Pin)((0 * 32) + 13);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_15 = (Cpu.Pin)((0 * 32) + 15);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_16 = (Cpu.Pin)((0 * 32) + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_17 = (Cpu.Pin)((0 * 32) + 17);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_18 = (Cpu.Pin)((0 * 32) + 18);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_22 = (Cpu.Pin)((0 * 32) + 22);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_23 = (Cpu.Pin)((0 * 32) + 23);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_24 = (Cpu.Pin)((0 * 32) + 24);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_25 = (Cpu.Pin)((0 * 32) + 25);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_26 = (Cpu.Pin)((0 * 32) + 26);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_27 = (Cpu.Pin)((0 * 32) + 27);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P0_28 = (Cpu.Pin)((0 * 32) + 28);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_2 = (Cpu.Pin)((1 * 32) + 2);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_3 = (Cpu.Pin)((1 * 32) + 3);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_6 = (Cpu.Pin)((1 * 32) + 6);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_7 = (Cpu.Pin)((1 * 32) + 7);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_11 = (Cpu.Pin)((1 * 32) + 11);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_12 = (Cpu.Pin)((1 * 32) + 12);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_19 = (Cpu.Pin)((1 * 32) + 19);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_20 = (Cpu.Pin)((1 * 32) + 20);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_21 = (Cpu.Pin)((1 * 32) + 21);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_22 = (Cpu.Pin)((1 * 32) + 22);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_23 = (Cpu.Pin)((1 * 32) + 23);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_24 = (Cpu.Pin)((1 * 32) + 24);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_25 = (Cpu.Pin)((1 * 32) + 25);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_26 = (Cpu.Pin)((1 * 32) + 26);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_27 = (Cpu.Pin)((1 * 32) + 27);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_28 = (Cpu.Pin)((1 * 32) + 28);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_29 = (Cpu.Pin)((1 * 32) + 29);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_30 = (Cpu.Pin)((1 * 32) + 30);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P1_31 = (Cpu.Pin)((1 * 32) + 31);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_0 = (Cpu.Pin)((2 * 32) + 0);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_1 = (Cpu.Pin)((2 * 32) + 1);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_2 = (Cpu.Pin)((2 * 32) + 2);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_3 = (Cpu.Pin)((2 * 32) + 3);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_4 = (Cpu.Pin)((2 * 32) + 4);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_5 = (Cpu.Pin)((2 * 32) + 5);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_6 = (Cpu.Pin)((2 * 32) + 6);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_7 = (Cpu.Pin)((2 * 32) + 7);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_8 = (Cpu.Pin)((2 * 32) + 8);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_9 = (Cpu.Pin)((2 * 32) + 9);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_10 = (Cpu.Pin)((2 * 32) + 10);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_12 = (Cpu.Pin)((2 * 32) + 12);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_13 = (Cpu.Pin)((2 * 32) + 13);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_21 = (Cpu.Pin)((2 * 32) + 21);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_22 = (Cpu.Pin)((2 * 32) + 22);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_23 = (Cpu.Pin)((2 * 32) + 23);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_25 = (Cpu.Pin)((2 * 32) + 25);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_26 = (Cpu.Pin)((2 * 32) + 26);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_27 = (Cpu.Pin)((2 * 32) + 27);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_30 = (Cpu.Pin)((2 * 32) + 30);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P2_31 = (Cpu.Pin)((2 * 32) + 31);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_16 = (Cpu.Pin)((3 * 32) + 16);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_17 = (Cpu.Pin)((3 * 32) + 17);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_18 = (Cpu.Pin)((3 * 32) + 18);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_19 = (Cpu.Pin)((3 * 32) + 19);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_20 = (Cpu.Pin)((3 * 32) + 20);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_21 = (Cpu.Pin)((3 * 32) + 21);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_22 = (Cpu.Pin)((3 * 32) + 22);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_24 = (Cpu.Pin)((3 * 32) + 24);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_25 = (Cpu.Pin)((3 * 32) + 25);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_26 = (Cpu.Pin)((3 * 32) + 26);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_27 = (Cpu.Pin)((3 * 32) + 27);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_28 = (Cpu.Pin)((3 * 32) + 28);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_29 = (Cpu.Pin)((3 * 32) + 29);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_30 = (Cpu.Pin)((3 * 32) + 30);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P3_31 = (Cpu.Pin)((3 * 32) + 31);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P4_28 = (Cpu.Pin)((4 * 32) + 28);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P4_29 = (Cpu.Pin)((4 * 32) + 29);
            /// <summary>GPIO pin.</summary>
            public const Cpu.Pin P4_31 = (Cpu.Pin)((4 * 32) + 31);
        }

        /// <summary>Analog input definitions.</summary>
        public static class AnalogInput {
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel P0_23 = (Cpu.AnalogChannel)0;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel P0_24 = (Cpu.AnalogChannel)1;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel P0_25 = (Cpu.AnalogChannel)2;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel P0_26 = (Cpu.AnalogChannel)3;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel P1_30 = (Cpu.AnalogChannel)4;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel P1_31 = (Cpu.AnalogChannel)5;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel P0_12 = (Cpu.AnalogChannel)6;
            /// <summary>Analog channel.</summary>
            public const Cpu.AnalogChannel P0_13 = (Cpu.AnalogChannel)7;
        }

        /// <summary>Analog output definitions.</summary>
        public static class AnalogOutput {
            /// <summary>Analog output channel.</summary>
            public const Cpu.AnalogOutputChannel P0_26 = (Cpu.AnalogOutputChannel)0;
        }

        /// <summary>PWM output definitions.</summary>
        public static class PwmOutput {
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel P3_16 = (Cpu.PWMChannel)0;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel P3_17 = (Cpu.PWMChannel)1;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel P3_18 = (Cpu.PWMChannel)2;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel P3_19 = (Cpu.PWMChannel)3;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel P3_20 = (Cpu.PWMChannel)4;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel P3_21 = (Cpu.PWMChannel)5;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel P3_24 = (Cpu.PWMChannel)6;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel P3_25 = (Cpu.PWMChannel)7;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel P3_26 = (Cpu.PWMChannel)8;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel P3_27 = (Cpu.PWMChannel)9;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel P3_28 = (Cpu.PWMChannel)10;
            /// <summary>PWM channel.</summary>
            public const Cpu.PWMChannel P3_29 = (Cpu.PWMChannel)11;
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            /// <summary>Serial port on P0.2 (TX) and P0.3 (RX).</summary>
            public const string Com1 = "COM1";
            /// <summary>Serial port on P2.0 (TX), P0.16 (RX), P3.18 (CTS), and P0.6 (RTS).</summary>
            public const string Com2 = "COM2";
            /// <summary>Serial port on P0.10 (TX) and P0.11 (RX).</summary>
            public const string Com3 = "COM3";
            /// <summary>Serial port on P0.25 (TX) and P0.26 (RX).</summary>
            public const string Com4 = "COM4";
            /// <summary>Serial port on P1.29 (TX) and P2.9 (RX).</summary>
            public const string Com5 = "COM5";
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on P0.18 (MOSI), P0.17 (MISO), and P0.15 (SCK).</summary>
            public const SPI.SPI_module Spi1 = SPI.SPI_module.SPI1;
            /// <summary>SPI bus.</summary>
            public const SPI.SPI_module Spi2 = SPI.SPI_module.SPI2;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN channel on P0.1 (TX) and P0.0 (RX).</summary>
            public const int Can1 = 1;
            /// <summary>CAN channel on P0.5 (TX) and P0.4 (RX).</summary>
            public const int Can2 = 2;
        }
    }
}