namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Cerbot.</summary>
    public static class FEZCerbot {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = Cerb.GpioPin.Id;

            /// <summary>Debug LED definition</summary>
            public const int DebugLed = Cerb.GpioPin.PA14;
            /// <summary>ReflectiveSensor1  definition</summary>
            public const int ReflectiveSensor1 = Cerb.GpioPin.PB13;
            /// <summary>ReflectiveSensor2 definition</summary>
            public const int ReflectiveSensor2 = Cerb.GpioPin.PB14;
            /// <summary>LedArrayLatch definition</summary>
            public const int LedArrayLatch = Cerb.GpioPin.PB2;
            /// <summary>LedArrayEnable definition</summary>
            public const int LedArrayEnable = Cerb.GpioPin.PA15;

            /// <summary>Socket definition.</summary>
            public static class Socket1 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PB15;
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
            public static class Socket2 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PC5;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = Cerb.GpioPin.PA10;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.GpioPin.PB12;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = Cerb.GpioPin.PC7;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket3 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PB8;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = Cerb.GpioPin.PB10;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.GpioPin.PB11;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = Cerb.GpioPin.PA0;
                /// <summary>Pin definition.</summary>
                public const int Pin7 = Cerb.GpioPin.PB5;
                /// <summary>Pin definition.</summary>
                public const int Pin8 = Cerb.GpioPin.PB4;
                /// <summary>Pin definition.</summary>
                public const int Pin9 = Cerb.GpioPin.PB3;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket4 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PC0;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = Cerb.GpioPin.PC1;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.GpioPin.PA4;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = Cerb.GpioPin.PA1;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket5 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PC14;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = Cerb.GpioPin.PC15;
            }

            /// <summary>Socket definition.</summary>
            public static class Socket6 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.GpioPin.PC13;
                /// <summary>Pin definition.</summary>
                public const int Pin6 = Cerb.GpioPin.PC3;
            }
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>ReflectiveSensor1  definition</summary>
            public const int ReflectiveSensor1 = Cerb.GpioPin.PA5;
            /// <summary>ReflectiveSensor2 definition</summary>
            public const int ReflectiveSensor2 = Cerb.GpioPin.PC2;

            /// <summary>Socket definition.</summary>
            public static class Socket4 {
                /// <summary>Pin definition.</summary>
                public const int Pin3 = Cerb.AdcChannel.PC0;
                /// <summary>Pin definition.</summary>
                public const int Pin4 = Cerb.AdcChannel.PC1;
                /// <summary>Pin definition.</summary>
                public const int Pin5 = Cerb.AdcChannel.PA4;
            }
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmPin.Controller1.Id;

                /// <summary>Socket definition.</summary>
                public static class Socket6 {
                    /// <summary>PWM pin.</summary>
                    public const int Pin7 = Cerb.PwmPin.Controller1.PA9;
                    /// <summary>PWM pin.</summary>
                    public const int Pin9 = Cerb.PwmPin.Controller1.PA8;
                }
            }

            /// <summary>PWM controller.</summary>
            public static class Controller2 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmPin.Controller2.Id;

                /// <summary>PWM pin.</summary>
                public const int LedArrayEnable = Cerb.PwmPin.Controller2.PA15;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller3 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmPin.Controller3.Id;

                /// <summary>Debug LightBulb Red definition</summary>
                public const int MotorPwmA = Cerb.PwmPin.Controller3.PB0;
                /// <summary>Debug LightBulb Green definition</summary>
                public const int MotorPwmB = Cerb.PwmPin.Controller3.PB1;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller4 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmPin.Controller1.Id;

                /// <summary>Socket definition.</summary>
                public static class Socket6 {
                    /// <summary>PWM pin.</summary>
                    public const int Pin8 = Cerb.PwmPin.Controller4.PB9;
                }
            }
        }
    }
}