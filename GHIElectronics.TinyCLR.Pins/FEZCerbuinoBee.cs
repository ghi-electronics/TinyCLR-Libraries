namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Cerbuino Bee.</summary>
    public static class FEZCerbuinoBee {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = Cerb.GpioPin.Id;

            /// <summary>GPIO pin for debug LED.</summary>
            public const int DebugLed = Cerb.GpioPin.PB2;
            /// <summary>GPIO pin for SD card detect.</summary>
            public const int SdCardDetect = Cerb.GpioPin.PC2;
            /// <summary>GPIO pin for Xbee Reset.</summary>
            public const int XbeeReset = Cerb.GpioPin.PB0;
            /// <summary>GPIO pin for Xbee Sleep.</summary>
            public const int XbeeSleep = Cerb.GpioPin.PC13;
            /// <summary>GPIO pin.</summary>
            public const int D0 = Cerb.GpioPin.PB11;
            /// <summary>GPIO pin.</summary>
            public const int D1 = Cerb.GpioPin.PB10;
            /// <summary>GPIO pin.</summary>
            public const int D2 = Cerb.GpioPin.PB12;
            /// <summary>GPIO pin.</summary>
            public const int D3 = Cerb.GpioPin.PC14;
            /// <summary>GPIO pin.</summary>
            public const int D4 = Cerb.GpioPin.PC15;
            /// <summary>GPIO pin.</summary>
            public const int D5 = Cerb.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int D6 = Cerb.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int D7 = Cerb.GpioPin.PC4;
            /// <summary>GPIO pin.</summary>
            public const int D8 = Cerb.GpioPin.PB13;
            /// <summary>GPIO pin.</summary>
            public const int D9 = Cerb.GpioPin.PA9;
            /// <summary>GPIO pin.</summary>
            public const int D10 = Cerb.GpioPin.PA15;
            /// <summary>GPIO pin.</summary>
            public const int D11 = Cerb.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int D12 = Cerb.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int D13 = Cerb.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int A0 = Cerb.GpioPin.PB1;
            /// <summary>GPIO pin.</summary>
            public const int A1 = Cerb.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int A2 = Cerb.GpioPin.PB0;
            /// <summary>GPIO pin.</summary>
            public const int A3 = Cerb.GpioPin.PC3;
            /// <summary>GPIO pin.</summary>
            public const int A4 = Cerb.GpioPin.PC1;
            /// <summary>GPIO pin.</summary>
            public const int A5 = Cerb.GpioPin.PA4;

            /// <summary>Socket definition.</summary>
            public static class Socket1 {
                /// <summary>GPIO pin.</summary>
                public const int Pin3 = Cerb.GpioPin.PA14;
                /// <summary>GPIO pin.</summary>
                public const int Pin4 = Cerb.GpioPin.PA10;
                /// <summary>GPIO pin.</summary>
                public const int Pin5 = Cerb.GpioPin.PB11;
                /// <summary>GPIO pin.</summary>
                public const int Pin6 = Cerb.GpioPin.PA13;
                /// <summary>GPIO pin.</summary>
                public const int Pin7 = Cerb.GpioPin.PB5;
                /// <summary>GPIO pin.</summary>
                public const int Pin8 = Cerb.GpioPin.PB4;
                /// <summary>GPIO pin.</summary>
                public const int Pin9 = Cerb.GpioPin.PB3;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket2 {
                /// <summary>GPIO pin.</summary>
                public const int Pin3 = Cerb.GpioPin.PA6;
                /// <summary>GPIO pin.</summary>
                public const int Pin4 = Cerb.GpioPin.PA2;
                /// <summary>GPIO pin.</summary>
                public const int Pin5 = Cerb.GpioPin.PA3;
                /// <summary>GPIO pin.</summary>
                public const int Pin6 = Cerb.GpioPin.PA1;
                /// <summary>GPIO pin.</summary>
                public const int Pin7 = Cerb.GpioPin.PA0;
                /// <summary>GPIO pin.</summary>
                public const int Pin8 = Cerb.GpioPin.PB7;
                /// <summary>GPIO pin.</summary>
                public const int Pin9 = Cerb.GpioPin.PB6;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket3 {
                /// <summary>GPIO pin.</summary>
                public const int Pin3 = Cerb.GpioPin.PC0;
                /// <summary>GPIO pin.</summary>
                public const int Pin4 = Cerb.GpioPin.PC1;
                /// <summary>GPIO pin.</summary>
                public const int Pin5 = Cerb.GpioPin.PA4;
                /// <summary>GPIO pin.</summary>
                public const int Pin6 = Cerb.GpioPin.PC5;
                /// <summary>GPIO pin.</summary>
                public const int Pin7 = Cerb.GpioPin.PB8;
                /// <summary>GPIO pin.</summary>
                public const int Pin8 = Cerb.GpioPin.PA7;
                /// <summary>GPIO pin.</summary>
                public const int Pin9 = Cerb.GpioPin.PB9;
            }
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>API id.</summary>
            public const string Id = Cerb.AdcChannel.Id;

            /// <summary>ADC channel.</summary>
            public const int D7 = Cerb.AdcChannel.PC4;
            /// <summary>ADC channel.</summary>
            public const int A0 = Cerb.AdcChannel.PB1;
            /// <summary>ADC channel.</summary>
            public const int A1 = Cerb.AdcChannel.PA5;
            /// <summary>ADC channel.</summary>
            public const int A2 = Cerb.AdcChannel.PB0;
            /// <summary>ADC channel.</summary>
            public const int A3 = Cerb.AdcChannel.PC3;
            /// <summary>ADC channel.</summary>
            public const int A4 = Cerb.AdcChannel.PC1;
            /// <summary>ADC channel.</summary>
            public const int A5 = Cerb.AdcChannel.PA4;

            /// <summary>Socket definition.</summary>
            public static class Socket2 {
                /// <summary>ADC channel.</summary>
                public const int Pin3 = Cerb.AdcChannel.PA6;
                /// <summary>ADC channel.</summary>
                public const int Pin4 = Cerb.AdcChannel.PA2;
                /// <summary>ADC channel.</summary>
                public const int Pin5 = Cerb.AdcChannel.PA3;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket3 {
                /// <summary>ADC channel.</summary>
                public const int Pin3 = Cerb.AdcChannel.PC0;
                /// <summary>ADC channel.</summary>
                public const int Pin4 = Cerb.AdcChannel.PA4;
                /// <summary>ADC channel.</summary>
                public const int Pin5 = Cerb.AdcChannel.PC5;
            }
        }

        /// <summary>DAC channel definitions.</summary>
        public static class DacChannel {
            /// <summary>API id.</summary>
            public const string Id = Cerb.DacChannel.Id;

            /// <summary>DAC channel.</summary>
            public const int A1 = Cerb.DacChannel.PA5;

            /// <summary>Socket definition.</summary>
            public static class Socket3 {
                /// <summary>DAC channel.</summary>
                public const int Pin5 = Cerb.DacChannel.PA4;
            }
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmChannel {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmChannel.Controller1.Id;

                /// <summary>PWM pin.</summary>
                public const int D5 = Cerb.PwmChannel.Controller1.PA8;
                /// <summary>PWM pin.</summary>
                public const int D6 = Cerb.PwmChannel.Controller1.PA10;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller2 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmChannel.Controller2.Id;

                /// <summary>PWM pin.</summary>
                public const int D10 = Cerb.PwmChannel.Controller2.PA15;
                /// <summary>PWM pin.</summary>
                public const int D13 = Cerb.PwmChannel.Controller2.PB3;

                /// <summary>Socket definition.</summary>
                public static class Socket1 {
                    /// <summary>PWM pin.</summary>
                    public const int Pin9 = Cerb.PwmChannel.Controller2.PB3;
                }
            }

            /// <summary>PWM controller.</summary>
            public static class Controller3 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmChannel.Controller3.Id;

                /// <summary>PWM pin.</summary>
                public const int D12 = Cerb.PwmChannel.Controller3.PB4;

                /// <summary>Socket definition.</summary>
                public static class Socket1 {
                    /// <summary>PWM pin.</summary>
                    public const int Pin7 = Cerb.PwmChannel.Controller3.PB5;
                    /// <summary>PWM pin.</summary>
                    public const int Pin8 = Cerb.PwmChannel.Controller3.PB4;
                }
            }
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>UART port.</summary>
            public const string Socket1 = Cerb.UartPort.Usart6;
            /// <summary>UART port.</summary>
            public const string Socket2 = Cerb.UartPort.Usart2;
            /// <summary>UART port for Xbee.</summary>
            public const string Xbee = Cerb.UartPort.Usart6;
            /// <summary>UART port on D0 (RX) and D1 (TX).</summary>
            public const string Usart3 = Cerb.UartPort.Usart3;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus.</summary>
            public const string Socket2 = Cerb.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus.</summary>
            public const string Socket1 = Cerb.SpiBus.Spi1;
            /// <summary>SPI bus on D11 (MOSI), D12 (MISO), and D13 (SCK).</summary>
            public const string Spi1 = Cerb.SpiBus.Spi1;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN bus on PB13 (TX) and PB12 (RX).</summary>
            public const string Can2 = Cerb.CanBus.Can2;
            /// <summary>CAN bus.</summary>
            public const string Socket3 = Cerb.CanBus.Can1;
        }

        /// <summary>USB client port definitions.</summary>
        public static class UsbClientPort {
            /// <summary>USB client port on D- (DM), D+ (DP), and VBUS (VBUS).</summary>
            public const string UsbOtg = Cerb.UsbClientPort.UsbOtg;
        }

        /// <summary>USB host port definitions.</summary>
        public static class UsbHostPort {
            /// <summary>USB host port on connector 5 pin 2 (DM) and connector 5 pin 3 (DP).</summary>
            public const string UsbOtg = Cerb.UsbHostPort.UsbOtg;
        }

        /// <summary>Storage controller definitions.</summary>
        public static class StorageController {
            /// <summary>API id.</summary>
            public const string SdCard = Cerb.StorageController.SdCard;
        }

        /// <summary>RTC controller definitions.</summary>
        public static class RtcController {
            /// <summary>API id.</summary>
            public const string Id = Cerb.RtcController.Id;
        }
    }
}
