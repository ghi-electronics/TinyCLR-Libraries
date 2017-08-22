namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Cerberus.</summary>
    public static class FEZCerberus {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>Debug LED definition</summary>
            public const int DebugLed = Cerb.GpioPin.PC4;

            /// <summary>Socket definition.</summary>
            public static class Socket1 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PC2;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = Cerb.GpioPin.PC13;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket2 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PA6;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = Cerb.GpioPin.PA2;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.GpioPin.PA3;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = Cerb.GpioPin.PA1;
                /// <summary>Pin definition.</summary>
                public const int Pin7 = Cerb.GpioPin.PA0;
                /// <summary>Pin definition.</summary>
                public const int Pin8 = Cerb.GpioPin.PB7;
                /// <summary>Pin definition.</summary>
                public const int Pin9 = Cerb.GpioPin.PB6;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket3 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PC0;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = Cerb.GpioPin.PC1;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.GpioPin.PA4;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = Cerb.GpioPin.PC5;
                /// <summary>Pin definition.</summary>
                public const int Pin7 = Cerb.GpioPin.PC6;
                /// <summary>Pin definition.</summary>
                public const int Pin8 = Cerb.GpioPin.PA7;
                /// <summary>Pin definition.</summary>
                public const int Pin9 = Cerb.GpioPin.PC7;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket4 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PC2;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = Cerb.GpioPin.PC3;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.GpioPin.PA5;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = Cerb.GpioPin.PC13;
                /// <summary>Pin definition.</summary>
                public const int Pin7 = Cerb.GpioPin.PA8;
                /// <summary>Pin definition.</summary>
                public const int Pin8 = Cerb.GpioPin.PB0;
                /// <summary>Pin definition.</summary>
                public const int Pin9 = Cerb.GpioPin.PB1;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket5 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PC14;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = Cerb.GpioPin.PB9;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.GpioPin.PB8;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = Cerb.GpioPin.PC15;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket6 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PA14;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = Cerb.GpioPin.PB10;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.GpioPin.PB11;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = Cerb.GpioPin.PA13;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket7 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PA15;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = Cerb.GpioPin.PC8;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.GpioPin.PC9;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = Cerb.GpioPin.PD2;
                /// <summary>Pin definition.</summary>
                public const int Pin7 = Cerb.GpioPin.PC10;
                /// <summary>Pin definition.</summary>
                public const int Pin8 = Cerb.GpioPin.PC11;
                /// <summary>Pin definition.</summary>
                public const int Pin9 = Cerb.GpioPin.PC12;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket8 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PA9;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = Cerb.GpioPin.PB12;
                /// <summary>Pin definition.</summary>
                public const int Pin7 = Cerb.GpioPin.PA10;
            }
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
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
                public const int Pin4 = Cerb.AdcChannel.PC1;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.AdcChannel.PA4;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket4 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.AdcChannel.PC2;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = Cerb.AdcChannel.PC3;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.AdcChannel.PA5;
            }
        }

        /// <summary>DAC channel definitions.</summary>
        public static class DacChannel {
            /// <summary>Socket definition.</summary>
            public static class Socket3 {
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.DacChannel.PA4;
            }
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmPin.Controller1.Id;

                /// <summary>Socket definition.</summary>
                public static class Socket4 {
                    /// <summary>PWM pin.</summary>
                    public const int Pin7 = Cerb.PwmPin.Controller1.PA8;
                }
            }

            /// <summary>PWM controller.</summary>
            public static class Controller2 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmPin.Controller2.Id;

                /// <summary>Socket definition.</summary>
                public static class Socket5 {
                    /// <summary>PWM pin.</summary>
                    public const int Pin9 = Cerb.PwmPin.Controller2.PB3;
                }
            }

            /// <summary>PWM controller.</summary>
            public static class Controller3 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmPin.Controller3.Id;

                /// <summary>Socket definition.</summary>
                public static class Socket4 {
                    /// <summary>PWM pin.</summary>
                    public const int Pin8 = Cerb.PwmPin.Controller3.PB0;
                    /// <summary>PWM pin.</summary>
                    public const int Pin9 = Cerb.PwmPin.Controller3.PB1;
                }

                /// <summary>Socket definition.</summary>
                public static class Socket5 {
                    /// <summary>PWM pin.</summary>
                    public const int Pin7 = Cerb.PwmPin.Controller3.PB5;
                    /// <summary>PWM pin.</summary>
                    public const int Pin8 = Cerb.PwmPin.Controller3.PB4;
                }
            }

            /// <summary>PWM controller.</summary>
            public static class Controller8 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmPin.Controller8.Id;

                /// <summary>Socket definition.</summary>
                public static class Socket3 {
                    /// <summary>PWM pin.</summary>
                    public const int Pin7 = Cerb.PwmPin.Controller8.PC6;
                    /// <summary>PWM pin.</summary>
                    public const int Pin9 = Cerb.PwmPin.Controller8.PC7;
                }
            }

            /// <summary>PWM controller.</summary>
            public static class Controller14 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmPin.Controller14.Id;

                /// <summary>Socket definition.</summary>
                public static class Socket3 {
                    /// <summary>PWM pin.</summary>
                    public const int Pin8 = Cerb.PwmPin.Controller14.PA7;
                }
            }
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>Socket definition.</summary>
            public const string Socket2 = Cerb.UartPort.Usart2;
            /// <summary>Socket definition.</summary>
            public const string Socket6 = Cerb.UartPort.Usart3;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>Socket definition.</summary>
            public const string Socket2 = Cerb.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>Socket definition.</summary>
            public const string Socket5 = Cerb.SpiBus.Spi1;
            /// <summary>Socket definition.</summary>
            public const string Socket6 = Cerb.SpiBus.Spi1;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>Socket definition.</summary>
            public const string Socket5 = Cerb.CanBus.Can1;
        }
    }
}