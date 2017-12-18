namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the CANxtra.</summary>
    public static class CANxtra {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = EmbeddedMasterNonTFT.GpioPin.Id;

            /// <summary>GPIO pin for green LED.</summary>
            public const int GreenLed = EmbeddedMasterNonTFT.GpioPin.P3_16;
            /// <summary>GPIO pin for buzzer.</summary>
            public const int Buzzer = EmbeddedMasterNonTFT.GpioPin.P4_28;
            /// <summary>GPIO pin for LCD reset.</summary>
            public const int LcdReset = EmbeddedMasterNonTFT.GpioPin.P4_29;
            /// <summary>GPIO pin for button row 0 ('1', '2', '3').</summary>
            public const int ButtonRow0 = EmbeddedMasterNonTFT.GpioPin.P2_11;
            /// <summary>GPIO pin for button row 1 ('4', '5', '6').</summary>
            public const int ButtonRow1 = EmbeddedMasterNonTFT.GpioPin.P0_8;
            /// <summary>GPIO pin for button row 2 ('7', '8', '9').</summary>
            public const int ButtonRow2 = EmbeddedMasterNonTFT.GpioPin.P2_10;
            /// <summary>GPIO pin for button row 3 ('Enter', '0', 'Cancel').</summary>
            public const int ButtonRow3 = EmbeddedMasterNonTFT.GpioPin.P0_10;
            /// <summary>GPIO pin for button column 0 ('1', '4', '7', 'Enter').</summary>
            public const int ButtonColumn0 = EmbeddedMasterNonTFT.GpioPin.P0_11;
            /// <summary>GPIO pin for button column 1 ('2', '5', '8', '0').</summary>
            public const int ButtonColumn1 = EmbeddedMasterNonTFT.GpioPin.P0_23;
            /// <summary>GPIO pin for button column 2 ('3', '6', '9', 'Cancel').</summary>
            public const int ButtonColumn2 = EmbeddedMasterNonTFT.GpioPin.P2_5;

            /// <summary>DB25 connector definition.</summary>
            public static class Db25 {
                /// <summary>Pin definition.</summary>
                public const int Pin8 = EmbeddedMasterNonTFT.GpioPin.P0_27;
                /// <summary>Pin definition.</summary>
                public const int Pin9 = EmbeddedMasterNonTFT.GpioPin.P0_28;
                /// <summary>Pin definition.</summary>
                public const int Pin10 = EmbeddedMasterNonTFT.GpioPin.P0_15;
                /// <summary>Pin definition.</summary>
                public const int Pin11 = EmbeddedMasterNonTFT.GpioPin.P0_16;
                /// <summary>Pin definition.</summary>
                public const int Pin12 = EmbeddedMasterNonTFT.GpioPin.P0_17;
                /// <summary>Pin definition.</summary>
                public const int Pin13 = EmbeddedMasterNonTFT.GpioPin.P0_18;
                /// <summary>Pin definition.</summary>
                public const int Pin14 = EmbeddedMasterNonTFT.GpioPin.P0_4;
                /// <summary>Pin definition.</summary>
                public const int Pin15 = EmbeddedMasterNonTFT.GpioPin.P0_5;
                /// <summary>Pin definition.</summary>
                public const int Pin16 = EmbeddedMasterNonTFT.GpioPin.P0_3;
                /// <summary>Pin definition.</summary>
                public const int Pin17 = EmbeddedMasterNonTFT.GpioPin.P0_2;
                /// <summary>Pin definition.</summary>
                public const int Pin18 = EmbeddedMasterNonTFT.GpioPin.P0_24;
                /// <summary>Pin definition.</summary>
                public const int Pin19 = EmbeddedMasterNonTFT.GpioPin.P0_25;
                /// <summary>Pin definition.</summary>
                public const int Pin21 = EmbeddedMasterNonTFT.GpioPin.P0_26;
                /// <summary>Pin definition.</summary>
                public const int Pin22 = EmbeddedMasterNonTFT.GpioPin.P3_24;
            }
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>API id.</summary>
            public const string Id = EmbeddedMasterNonTFT.AdcChannel.Id;

            /// <summary>DB25 connector definition.</summary>
            public static class Db25 {
                /// <summary>Pin definition.</summary>
                public const int Pin18 = EmbeddedMasterNonTFT.AdcChannel.P0_24;
                /// <summary>Pin definition.</summary>
                public const int Pin19 = EmbeddedMasterNonTFT.AdcChannel.P0_25;
                /// <summary>Pin definition.</summary>
                public const int Pin21 = EmbeddedMasterNonTFT.AdcChannel.P0_26;
            }
        }

        /// <summary>DAC channel definitions.</summary>
        public static class DacChannel {
            /// <summary>API id.</summary>
            public const string Id = EmbeddedMasterNonTFT.DacChannel.Id;

            /// <summary>DB25 connector definition.</summary>
            public static class Db25 {
                /// <summary>Pin definition.</summary>
                public const int Pin21 = EmbeddedMasterNonTFT.DacChannel.P0_26;
            }
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller0 {
                /// <summary>API id.</summary>
                public const string Id = EmbeddedMasterNonTFT.PwmPin.Controller0.Id;

                /// <summary>Pin definition.</summary>
                public const int GreenLed = EmbeddedMasterNonTFT.PwmPin.Controller0.P3_16;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = EmbeddedMasterNonTFT.PwmPin.Controller1.Id;

                /// <summary>DB25 connector definition.</summary>
                public static class Db25 {
                    /// <summary>Pin definition.</summary>
                    public const int Pin22 = EmbeddedMasterNonTFT.PwmPin.Controller1.P3_24;
                }
            }
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>UART port on DB25 pin 17 / P0.2 (TX) and DB25 pin 16 / P0.3 (RX).</summary>
            public const string Uart0 = EmbeddedMasterNonTFT.UartPort.Uart0;
            /// <summary>UART port on DB25 pin 2 / P2.0 (TX), DB25 pin 3 / P2.1 (RX), DB25 pin 5 / P2.2 (CTS), and DB25 pin 4 / P2.7 (RTS).</summary>
            public const string Uart1 = EmbeddedMasterNonTFT.UartPort.Uart1;
            /// <summary>UART port for LIN on P2.8 (TX) and P2.9 (RX).</summary>
            public const string Uart2 = EmbeddedMasterNonTFT.UartPort.Uart2;
            /// <summary>UART port on DB25 pin 19 / P0.25 (TX) and DB25 pin 21 / P0.26 (RX).</summary>
            public const string Uart3 = EmbeddedMasterNonTFT.UartPort.Uart3;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on DB25 pin 8 / P0.27 (SDA) and DB25 pin 9 / P0.28 (SCL).</summary>
            public const string I2c0 = EmbeddedMasterNonTFT.I2cBus.I2c0;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on DB25 pin 13 / P0.18 (MOSI), DB25 pin12 / P0.17 (MISO), and DB25 pin 10 / P0.15 (SCK).</summary>
            public const string Spi1 = EmbeddedMasterNonTFT.SpiBus.Spi1;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN bus on P0.1 (TX) and P0.0 (RX).</summary>
            public const string Can1 = EmbeddedMasterNonTFT.CanBus.Can1;
        }

        /// <summary>USB client port definitions.</summary>
        public static class UsbClientPort {
            /// <summary>USB client port on USB Device D- / UD_DM (D-), USB Device D+ / UD_DP (D+), and USB Device VBUS / UD_VBUS (VBUS).</summary>
            public const string UsbDevice = EmbeddedMasterNonTFT.UsbClientPort.UsbDevice;
        }

        /// <summary>USB host port definitions.</summary>
        public static class UsbHostPort {
            /// <summary>USB host port on USB Host D- / UH_DM (D-) and USB Host D + / UH_DP (D+).</summary>
            public const string UsbHost1 = EmbeddedMasterNonTFT.UsbHostPort.UsbHost1;
        }

        /// <summary>Display definitions.</summary>
        public static class Display {
            /// <summary>LCD definition.</summary>
            public const string Lcd = EmbeddedMasterNonTFT.Display.Lcd;
        }
    }
}
