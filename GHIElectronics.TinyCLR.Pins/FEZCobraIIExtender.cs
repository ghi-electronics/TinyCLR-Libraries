namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ CobraII Extender.</summary>
    public static class FEZCobraIIExtender {
        /// <summary>Gpio Pin definition.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = G120.GpioPin.Id;

            /// <summary>Socket definition.</summary>
            public static class Socket7 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = G120.GpioPin.P0_4;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = G120.GpioPin.P4_28;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = G120.GpioPin.P4_29;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = G120.GpioPin.P1_30;
                /// <summary>Pin definition.</summary>
                public const int Pin7 = G120.GpioPin.P3_26;
                /// <summary>Pin definition.</summary>
                public const int Pin8 = G120.GpioPin.P3_25;
                /// <summary>Pin definition.</summary>
                public const int Pin9 = G120.GpioPin.P3_24;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket8 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = G120.GpioPin.P0_10;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = G120.GpioPin.P2_0;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = G120.GpioPin.P0_16;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = G120.GpioPin.P0_6;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket9 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = G120.GpioPin.P0_12;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = G120.GpioPin.P1_31;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = G120.GpioPin.P0_26;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = G120.GpioPin.P1_5;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket10 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = G120.GpioPin.P0_11;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = G120.GpioPin.P0_1;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = G120.GpioPin.P0_0;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = G120.GpioPin.P0_5;
            }
        }

        /// <summary>Analog channel definition.</summary>
        public static class AdcChannel {
            /// <summary>API Id.</summary>
            public const string Id = G120.AdcChannel.Id;

            /// <summary>Socket definition.</summary>
            public static class Socket9 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = G120.AdcChannel.P0_12;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = G120.AdcChannel.P1_31;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = G120.AdcChannel.P0_26;
            }
        }

        /// <summary>Analog output definition.</summary>
        public static class DacChannel {
            /// <summary>API Id.</summary>
            public const string Id = G120.DacChannel.Id;

            /// <summary>Socket definition.</summary>
            public static class Socket9 {
                /// <summary>Pin definition.</summary>
                public const int Pin5 = G120.DacChannel.P0_26;
            }
        }

        /// <summary>Uart port definition.</summary>
        public static class UartPort {
            /// <summary>Socket definition.</summary>
            public const string Socket7 = G120.UartPort.Usart4;
            /// <summary>Socket definition.</summary>
            public const string Socket8 = G120.UartPort.Uart2;
        }

        /// <summary>I2c Bus definition.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on Pin18 (SDA) and Pin20 (SCL).</summary>
            public const string I2c0 = G120.I2cBus.I2c0;
            /// <summary>Socket definition.</summary>
            public const string Socket8 = G120.I2cBus.I2c0;
            /// <summary>Socket definition.</summary>
            public const string Socket10 = G120.I2cBus.I2c0;
        }

        /// <summary>SPI Bus definition.</summary>
        public static class SpiBus {
            /// <summary>Socket definition.</summary>
            public const string Socket9 = G120.SpiBus.Spi0;
        }

        /// <summary>CAN Bus definition.</summary>
        public static class CanBus {
            /// <summary>Socket definition.</summary>
            public const string Socket10 = G120.CanBus.Can1;
        }
    }
}

