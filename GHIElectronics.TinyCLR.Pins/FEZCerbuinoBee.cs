namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Cerbuino Bee.</summary>
    public static class FEZCerbuinoBee {
        /// <summary>GPIO Pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API Id.</summary>
            public const string Id = Cerb.GpioPin.Id;

            /// <summary>Debug LED definition</summary>
            public const int DebugLed = Cerb.GpioPin.PB2;
            /// <summary>SD Card Dectect definition</summary>
            public const int SdCardDetect = Cerb.GpioPin.PC2;
            /// <summary>Xbee Reset definition</summary>
            public const int XbeeReset = Cerb.GpioPin.PB0;
            /// <summary>Xbee Sleep definition</summary>
            public const int XbeeSleep = Cerb.GpioPin.PC13;
            /// <summary>GPIO Pin.</summary>
            public const int D0 = Cerb.GpioPin.PB11;
            /// <summary>GPIO Pin.</summary>
            public const int D1 = Cerb.GpioPin.PB10;
            /// <summary>GPIO Pin.</summary>
            public const int D2 = Cerb.GpioPin.PB12;
            /// <summary>GPIO Pin.</summary>
            public const int D3 = Cerb.GpioPin.PC14;
            /// <summary>GPIO Pin.</summary>
            public const int D4 = Cerb.GpioPin.PC15;
            /// <summary>GPIO Pin.</summary>
            public const int D5 = Cerb.GpioPin.PA8;
            /// <summary>GPIO Pin.</summary>
            public const int D6 = Cerb.GpioPin.PA10;
            /// <summary>GPIO Pin.</summary>
            public const int D7 = Cerb.GpioPin.PC4;
            /// <summary>GPIO Pin.</summary>
            public const int D8 = Cerb.GpioPin.PB13;
            /// <summary>GPIO Pin.</summary>
            public const int D9 = Cerb.GpioPin.PA9;
            /// <summary>GPIO Pin.</summary>
            public const int D10 = Cerb.GpioPin.PA15;
            /// <summary>GPIO Pin.</summary>
            public const int D11 = Cerb.GpioPin.PB5;
            /// <summary>GPIO Pin.</summary>
            public const int D12 = Cerb.GpioPin.PB4;
            /// <summary>GPIO Pin.</summary>
            public const int D13 = Cerb.GpioPin.PB3;
            /// <summary>GPIO Pin.</summary>
            public const int A0 = Cerb.GpioPin.PB1;
            /// <summary>GPIO Pin.</summary>
            public const int A1 = Cerb.GpioPin.PA5;
            /// <summary>GPIO Pin.</summary>
            public const int A2 = Cerb.GpioPin.PB0;
            /// <summary>GPIO Pin.</summary>
            public const int A3 = Cerb.GpioPin.PC3;
            /// <summary>GPIO Pin.</summary>
            public const int A4 = Cerb.GpioPin.PC1;
            /// <summary>GPIO Pin.</summary>
            public const int A5 = Cerb.GpioPin.PA4;

            /// <summary>Socket definition.</summary>
            public static class Socket1 {
                /// <summary>GPIO Pin.</summary>
                public const int Pin3 = Cerb.GpioPin.PA14;
                /// <summary>GPIO Pin.</summary>
                public const int Pin4 = Cerb.GpioPin.PA10;
                /// <summary>GPIO Pin.</summary>
                public const int Pin5 = Cerb.GpioPin.PB11;
                /// <summary>GPIO Pin.</summary>
                public const int Pin6 = Cerb.GpioPin.PA13;
                /// <summary>GPIO Pin.</summary>
                public const int Pin7 = Cerb.GpioPin.PB5;
                /// <summary>GPIO Pin.</summary>
                public const int Pin8 = Cerb.GpioPin.PB4;
                /// <summary>GPIO Pin.</summary>
                public const int Pin9 = Cerb.GpioPin.PB3;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket2 {
                /// <summary>GPIO Pin.</summary>
                public const int Pin3 = Cerb.GpioPin.PA6;
                /// <summary>GPIO Pin.</summary>
                public const int Pin4 = Cerb.GpioPin.PA2;
                /// <summary>GPIO Pin.</summary>
                public const int Pin5 = Cerb.GpioPin.PA3;
                /// <summary>GPIO Pin.</summary>
                public const int Pin6 = Cerb.GpioPin.PA1;
                /// <summary>GPIO Pin.</summary>
                public const int Pin7 = Cerb.GpioPin.PA0;
                /// <summary>GPIO Pin.</summary>
                public const int Pin8 = Cerb.GpioPin.PB7;
                /// <summary>GPIO Pin.</summary>
                public const int Pin9 = Cerb.GpioPin.PB6;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket3 {
                /// <summary>GPIO Pin.</summary>
                public const int Pin3 = Cerb.GpioPin.PC0;
                /// <summary>GPIO Pin.</summary>
                public const int Pin4 = Cerb.GpioPin.PC1;
                /// <summary>GPIO Pin.</summary>
                public const int Pin5 = Cerb.GpioPin.PA4;
                /// <summary>GPIO Pin.</summary>
                public const int Pin6 = Cerb.GpioPin.PC5;
                /// <summary>GPIO Pin.</summary>
                public const int Pin7 = Cerb.GpioPin.PB8;
                /// <summary>GPIO Pin.</summary>
                public const int Pin8 = Cerb.GpioPin.PA7;
                /// <summary>GPIO Pin.</summary>
                public const int Pin9 = Cerb.GpioPin.PB9;
            }
        }

        /// <summary>Analog channel definition.</summary>
        public static class AdcChannel {
            /// <summary>Pin definition.</summary>
            public const int D7 = Cerb.AdcChannel.PC4;
            /// <summary>Pin definition.</summary>
            public const int A0 = Cerb.AdcChannel.PB1;
            /// <summary>Pin definition.</summary>
            public const int A1 = Cerb.AdcChannel.PA5;
            /// <summary>Pin definition.</summary>
            public const int A2 = Cerb.AdcChannel.PB0;
            /// <summary>Pin definition.</summary>
            public const int A3 = Cerb.AdcChannel.PC3;
            /// <summary>Pin definition.</summary>
            public const int A4 = Cerb.AdcChannel.PC1;
            /// <summary>Pin definition.</summary>
            public const int A5 = Cerb.AdcChannel.PA4;

            /// <summary>Socket definition.</summary>
            public static class Socket2 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.AdcChannel.PA6;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = Cerb.AdcChannel.PA2;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.AdcChannel.PA3;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket3 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.AdcChannel.PC0;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = Cerb.AdcChannel.PA4;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.AdcChannel.PC5;
            }
        }

        /// <summary>Analog output channel definition.</summary>
        public static class DacChannel {
            /// <summary>Pin definition.</summary>
            public const int A1 = Cerb.DacChannel.PA5;

            /// <summary>Socket definition.</summary>
            public static class Socket3 {
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.DacChannel.PA4;
            }
        }

        /// <summary>PWM channel definition.</summary>
        public static class PwmPin {
            /// <summary>Controller 1 definition.</summary>
            public static class Controller1 {
                /// <summary>API Id.</summary>
                public const string Id = Cerb.PwmPin.Controller1.Id;

                /// <summary>Pin definition.</summary>
                public const int D5 = Cerb.PwmPin.Controller1.PA8;
                /// <summary>Pin definition.</summary>
                public const int D6 = Cerb.PwmPin.Controller1.PA10;
            }

            /// <summary>Controller 2 definition.</summary>
            public static class Controller2 {
                /// <summary>API Id.</summary>
                public const string Id = Cerb.PwmPin.Controller2.Id;

                /// <summary>Pin definition.</summary>
                public const int D10 = Cerb.PwmPin.Controller2.PA15;
                /// <summary>Pin definition.</summary>
                public const int D13 = Cerb.PwmPin.Controller2.PB3;

                /// <summary>Socket definition.</summary>
                public static class Socket1 {
                    /// <summary>Pin definition.</summary>
                    public const int Pin9 = Cerb.PwmPin.Controller2.PB3;
                }
            }

            /// <summary>Controller 3 definition.</summary>
            public static class Controller3 {
                /// <summary>API Id.</summary>
                public const string Id = Cerb.PwmPin.Controller3.Id;

                /// <summary>Pin definition.</summary>
                public const int D12 = Cerb.PwmPin.Controller3.PB4;

                /// <summary>Socket definition.</summary>
                public static class Socket1 {
                    /// <summary>Pin definition.</summary>
                    public const int Pin7 = Cerb.PwmPin.Controller3.PB5;
                    /// <summary>Pin definition.</summary>
                    public const int Pin8 = Cerb.PwmPin.Controller3.PB4;
                }
            }
            /// <summary>Uart port definition.</summary>
            public static class UartPort {
                /// <summary>Socket definition.</summary>
                public const string Socket1 = Cerb.UartPort.Usart6;
                /// <summary>Socket definition.</summary>
                public const string Socket2 = Cerb.UartPort.Usart2;
                /// <summary>Xbee definition.</summary>
                public const string Xbee = Cerb.UartPort.Usart6;
                /// <summary>UART D0 (RX) and D1 (TX).</summary>
                public const string Usart3 = Cerb.UartPort.Usart3;
            }
            /// <summary>SPI Bus definition.</summary>
            public static class SpiBus {
                /// <summary>Socket definition.</summary>
                public const string Socket1 = Cerb.SpiBus.Spi1;
                /// <summary>SPI bus on D11 (MOSI), D12 (MISO), and D13 (SCK).</summary>
                public const string Spi1 = Cerb.SpiBus.Spi1;
            }

            /// <summary>CAN bus definitions.</summary>
            public static class CanBus {
                /// <summary>CAN channel on PB13 (TX) and PB12 (RX).</summary>
                public const string Can2 = Cerb.CanBus.Can2;
                /// <summary>Socket definition.</summary>
                public const string Socket3 = Cerb.CanBus.Can1;
            }

            /// <summary>I2c Bus definition.</summary>
            public static class I2cBus {
                /// <summary>Socket definition.</summary>
                public const string Socket2 = Cerb.I2cBus.I2c1;
            }
        }
    }
}
