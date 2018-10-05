namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZCerb40.</summary>
    public static class FEZCerb40 {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = Cerb.GpioPin.Id;

            /// <summary>GPIO pin.</summary>
            public const int PA0 = Cerb.GpioPin.PA0;
            /// <summary>GPIO pin.</summary>
            public const int PA1 = Cerb.GpioPin.PA1;
            /// <summary>GPIO pin.</summary>
            public const int PA2 = Cerb.GpioPin.PA2;
            /// <summary>GPIO pin.</summary>
            public const int PA3 = Cerb.GpioPin.PA3;
            /// <summary>GPIO pin.</summary>
            public const int PA4 = Cerb.GpioPin.PA4;
            /// <summary>GPIO pin.</summary>
            public const int PA5 = Cerb.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int PA6 = Cerb.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int PA7 = Cerb.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int PA8 = Cerb.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int PA13 = Cerb.GpioPin.PA13;
            /// <summary>GPIO pin.</summary>
            public const int PA14 = Cerb.GpioPin.PA14;
            /// <summary>GPIO pin.</summary>
            public const int PB3 = Cerb.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int PB4 = Cerb.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int PB5 = Cerb.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int PB6 = Cerb.GpioPin.PB6;
            /// <summary>GPIO pin.</summary>
            public const int PB7 = Cerb.GpioPin.PB7;
            /// <summary>GPIO pin.</summary>
            public const int PB8 = Cerb.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int PB9 = Cerb.GpioPin.PB9;
            /// <summary>GPIO pin.</summary>
            public const int PB10 = Cerb.GpioPin.PB10;
            /// <summary>GPIO pin.</summary>
            public const int PB11 = Cerb.GpioPin.PB11;
            /// <summary>GPIO pin.</summary>
            public const int PB14 = Cerb.GpioPin.PB10;
            /// <summary>GPIO pin.</summary>
            public const int PB15 = Cerb.GpioPin.PB15;
            /// <summary>GPIO pin.</summary>
            public const int PC0 = Cerb.GpioPin.PC0;
            /// <summary>GPIO pin.</summary>
            public const int PC1 = Cerb.GpioPin.PC1;
            /// <summary>GPIO pin.</summary>
            public const int PC2 = Cerb.GpioPin.PC2;
            /// <summary>GPIO pin.</summary>
            public const int PC3 = Cerb.GpioPin.PC3;
            /// <summary>GPIO pin.</summary>
            public const int PC6 = Cerb.GpioPin.PC6;
            /// <summary>GPIO pin.</summary>
            public const int PC7 = Cerb.GpioPin.PC7;
            /// <summary>GPIO pin.</summary>
            public const int PC8 = Cerb.GpioPin.PC8;
            /// <summary>GPIO pin.</summary>
            public const int PC9 = Cerb.GpioPin.PC9;
            /// <summary>GPIO pin.</summary>
            public const int PC10 = Cerb.GpioPin.PC10;
            /// <summary>GPIO pin.</summary>
            public const int PC11 = Cerb.GpioPin.PC11;
            /// <summary>GPIO pin.</summary>
            public const int PC12 = Cerb.GpioPin.PC12;
            /// <summary>GPIO pin.</summary>
            public const int PD2 = Cerb.GpioPin.PD2;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>API id.</summary>
            public const string Id = Cerb.AdcChannel.Id;

            /// <summary>ADC channel.</summary>
            public const int PA6 = Cerb.AdcChannel.PA6;
            /// <summary>ADC channel.</summary>
            public const int PA2 = Cerb.AdcChannel.PA2;
            /// <summary>ADC channel.</summary>
            public const int PA3 = Cerb.AdcChannel.PA3;
            /// <summary>ADC channel.</summary>
            public const int PC0 = Cerb.AdcChannel.PC0;
            /// <summary>ADC channel.</summary>
            public const int PC1 = Cerb.AdcChannel.PC1;
            /// <summary>ADC channel.</summary>
            public const int PC2 = Cerb.AdcChannel.PC2;
            /// <summary>ADC channel.</summary>
            public const int PC3 = Cerb.AdcChannel.PC3;
            /// <summary>ADC channel.</summary>
            public const int PA4 = Cerb.AdcChannel.PA4;
            /// <summary>ADC channel.</summary>
            public const int PA5 = Cerb.AdcChannel.PA4;
        }

        /// <summary>DAC channel definitions.</summary>
        public static class DacChannel {
            /// <summary>API id.</summary>
            public const string Id = Cerb.DacChannel.Id;

            /// <summary>DAC channel.</summary>
            public const int PA4 = Cerb.DacChannel.PA4;
            /// <summary>DAC channel.</summary>
            public const int PA5 = Cerb.DacChannel.PA4;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmChannel {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmChannel.Controller1.Id;

                /// <summary>PWM pin.</summary>
                public const int PA8 = Cerb.PwmChannel.Controller1.PA8;
            }

            public static class Controller2 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmChannel.Controller2.Id;

                /// <summary>PWM pin.</summary>
                public const int PB3 = Cerb.PwmChannel.Controller2.PB3;
            }

            public static class Controller3 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmChannel.Controller3.Id;

                /// <summary>PWM pin.</summary>
                public const int PB5 = Cerb.PwmChannel.Controller3.PB5;
                /// <summary>PWM pin.</summary>
                public const int PB4 = Cerb.PwmChannel.Controller3.PB4;
            }

            public static class Controller8 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmChannel.Controller8.Id;

                /// <summary>PWM pin.</summary>
                public const int PC6 = Cerb.PwmChannel.Controller8.PC6;
                /// <summary>PWM pin.</summary>
                public const int PC7 = Cerb.PwmChannel.Controller8.PC7;
            }

            public static class Controller14 {
                /// <summary>API id.</summary>
                public const string Id = Cerb.PwmChannel.Controller14.Id;

                /// <summary>PWM pin.</summary>
                public const int PA7 = Cerb.PwmChannel.Controller14.PA7;
            }
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>UART port on PC6 (TX) and PC7 (RX).</summary>
            public const string Usart1 = Cerb.UartPort.Usart1;
            /// <summary>UART port on PA2 (TX), PA3 (RX), PA0 (CTS), and PA1 (RTS).</summary>
            public const string Usart2 = Cerb.UartPort.Usart2;
            /// <summary>UART port on PB10 (TX) and PB11 (RX).</summary>
            public const string Usart3 = Cerb.UartPort.Usart3;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on PB7 (SDA) and PB6 (SCL).</summary>
            public const string I2c1 = Cerb.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            public const string Socket1 = Cerb.SpiBus.Spi1;
            /// <summary>SPI bus on PB5 (MOSI), PB4 (MISO), and PB3 (SCK).</summary>
            public const string Spi1 = Cerb.SpiBus.Spi1;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN bus on PB9 (TX) and PB8 (RX).</summary>
            public const string Can1 = Cerb.CanBus.Can1;
        }

        /// <summary>USB client port definitions.</summary>
        public static class UsbClientPort {
            /// <summary>USB client port on D- (DM), D+ (DP), and VBUS (VBUS).</summary>
            public const string UsbOtg = Cerb.UsbClientPort.UsbOtg;
        }

        /// <summary>USB host port definitions.</summary>
        public static class UsbHostPort {
            /// <summary>USB host port on PB14 (DM) and PB15 (DP).</summary>
            public const string UsbOtg = Cerb.UsbHostPort.UsbOtg;
        }

        /// <summary>StorageController definitions.</summary>
        public static class StorageController {
            /// <summary>API id.</summary>
            public const string SdCard = Cerb.StorageController.SdCard;
        }

        /// <summary>RtcController definitions.</summary>
        public static class RtcController {
            /// <summary>API id.</summary>
            public const string Id = Cerb.RtcController.Id;
        }
    }
}
