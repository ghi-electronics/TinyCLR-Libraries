namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Cobra II Extender.</summary>
    public static class FEZCobraIIExtender {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = G120.GpioPin.Id;

            /// <summary>Socket definition.</summary>
            public static class Socket7 {
                /// <summary>GPIO pin.</summary>
                public const int Pin3 = G120.GpioPin.P0_4;
                /// <summary>GPIO pin.</summary>
                public const int Pin4 = G120.GpioPin.P4_28;
                /// <summary>GPIO pin.</summary>
                public const int Pin5 = G120.GpioPin.P4_29;
                /// <summary>GPIO pin.</summary>
                public const int Pin6 = G120.GpioPin.P1_30;
                /// <summary>GPIO pin.</summary>
                public const int Pin7 = G120.GpioPin.P3_26;
                /// <summary>GPIO pin.</summary>
                public const int Pin8 = G120.GpioPin.P3_25;
                /// <summary>GPIO pin.</summary>
                public const int Pin9 = G120.GpioPin.P3_24;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket8 {
                /// <summary>GPIO pin.</summary>
                public const int Pin3 = G120.GpioPin.P0_10;
                /// <summary>GPIO pin.</summary>
                public const int Pin4 = G120.GpioPin.P2_0;
                /// <summary>GPIO pin.</summary>
                public const int Pin5 = G120.GpioPin.P0_16;
                /// <summary>GPIO pin.</summary>
                public const int Pin6 = G120.GpioPin.P0_6;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket9 {
                /// <summary>GPIO pin.</summary>
                public const int Pin3 = G120.GpioPin.P0_12;
                /// <summary>GPIO pin.</summary>
                public const int Pin4 = G120.GpioPin.P1_31;
                /// <summary>GPIO pin.</summary>
                public const int Pin5 = G120.GpioPin.P0_26;
                /// <summary>GPIO pin.</summary>
                public const int Pin6 = G120.GpioPin.P1_5;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket10 {
                /// <summary>GPIO pin.</summary>
                public const int Pin3 = G120.GpioPin.P0_11;
                /// <summary>GPIO pin.</summary>
                public const int Pin4 = G120.GpioPin.P0_1;
                /// <summary>GPIO pin.</summary>
                public const int Pin5 = G120.GpioPin.P0_0;
                /// <summary>GPIO pin.</summary>
                public const int Pin6 = G120.GpioPin.P0_5;
            }
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>API id.</summary>
            public const string Id = G120.AdcChannel.Id;

            /// <summary>Socket definition.</summary>
            public static class Socket9 {
                /// <summary>ADC channel.</summary>
                public const int Pin3 = G120.AdcChannel.P0_12;
                /// <summary>ADC channel.</summary>
                public const int Pin4 = G120.AdcChannel.P1_31;
                /// <summary>ADC channel.</summary>
                public const int Pin5 = G120.AdcChannel.P0_26;
            }
        }

        /// <summary>DAC channel definitions.</summary>
        public static class DacChannel {
            /// <summary>API id.</summary>
            public const string Id = G120.DacChannel.Id;

            /// <summary>Socket definition.</summary>
            public static class Socket9 {
                /// <summary>DAC channel.</summary>
                public const int Pin5 = G120.DacChannel.P0_26;
            }
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>UART port.</summary>
            public const string Socket7 = G120.UartPort.Usart4;
            /// <summary>UART port.</summary>
            public const string Socket8 = G120.UartPort.Uart2;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus.</summary>
            public const string Socket8 = G120.I2cBus.I2c0;
            /// <summary>I2C bus.</summary>
            public const string Socket10 = G120.I2cBus.I2c0;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus.</summary>
            public const string Socket9 = G120.SpiBus.Spi0;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN bus.</summary>
            public const string Socket10 = G120.CanBus.Can1;
        }
    }
}
