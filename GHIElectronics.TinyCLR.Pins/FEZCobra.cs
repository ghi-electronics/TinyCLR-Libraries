namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Cobra.</summary>
    public static class FEZCobra {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = EMX.GpioPin.Id;

            /// <summary>GPIO pin for Button Left.</summary>
            public const int ButtonUp = EMX.GpioPin.P2_5;
            /// <summary>GPIO pin for Button Right.</summary>
            public const int ButtonSelect = EMX.GpioPin.P2_11;
            /// <summary>GPIO pin for Button Down.</summary>
            public const int ButtonDown = EMX.GpioPin.P0_4;
            /// <summary>GPIO pin for SD card detect.</summary>
            public const int SdCardDetect = EMX.GpioPin.P0_6;
            /// <summary>GPIO pin.</summary>
            public const int IO71 = EMX.GpioPin.P3_23;
            /// <summary>GPIO pin.</summary>
            public const int IO37 = EMX.GpioPin.P2_0;
            /// <summary>GPIO pin.</summary>
            public const int IO34 = EMX.GpioPin.P2_2;
            /// <summary>GPIO pin.</summary>
            public const int IO32 = EMX.GpioPin.P2_1;
            /// <summary>GPIO pin.</summary>
            public const int IO31 = EMX.GpioPin.P2_7;
            /// <summary>GPIO pin.</summary>
            public const int IO30 = EMX.GpioPin.P2_11;
            /// <summary>GPIO pin.</summary>
            public const int IO29 = EMX.GpioPin.P4_22;
            /// <summary>GPIO pin.</summary>
            public const int IO28 = EMX.GpioPin.P4_23;
            /// <summary>GPIO pin.</summary>
            public const int IO27 = EMX.GpioPin.P0_15;
            /// <summary>GPIO pin.</summary>
            public const int IO25 = EMX.GpioPin.P0_17;
            /// <summary>GPIO pin.</summary>
            public const int IO24 = EMX.GpioPin.P0_18;
            /// <summary>GPIO pin.</summary>
            public const int IO50 = EMX.GpioPin.P3_17;
            /// <summary>GPIO pin.</summary>
            public const int IO49 = EMX.GpioPin.P3_26;
            /// <summary>GPIO pin.</summary>
            public const int Piezo = EMX.GpioPin.P4_28;
            /// <summary>GPIO pin.</summary>
            public const int IO75 = EMX.GpioPin.P4_31;
            /// <summary>GPIO pin.</summary>
            public const int IO26 = EMX.GpioPin.P4_31;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>API id.</summary>
            public const string Id = EMX.AdcChannel.Id;

            /// <summary>ADC channel.</summary>
            public const int IO8 = EMX.AdcChannel.P0_23;
            /// <summary>ADC channel.</summary>
            public const int IO5 = EMX.AdcChannel.P0_24;
            /// <summary>ADC channel.</summary>
            public const int IO6 = EMX.AdcChannel.P0_25;
            /// <summary>ADC channel.</summary>
            public const int IO7 = EMX.AdcChannel.P0_26;
            /// <summary>ADC channel.</summary>
            public const int IO46 = EMX.AdcChannel.P0_13;

        }

        /// <summary>DAC channel definitions.</summary>
        public static class DacChannel {
            /// <summary>API id.</summary>
            public const string Id = EMX.DacChannel.Id;

            /// <summary>DAC channel.</summary>
            public const int IO7 = EMX.DacChannel.P0_26;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller0 {
                /// <summary>API id.</summary>
                public const string Id = EMX.PwmPin.Controller0.Id;

                /// <summary>PWM pin.</summary>
                public const int IO50 = EMX.PwmPin.Controller0.P3_17;
            }

            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = EMX.PwmPin.Controller1.Id;

                /// <summary>PWM pin.</summary>
                public const int IO49 = EMX.PwmPin.Controller1.P3_26;
                /// <summary>PWM pin.</summary>
                public const int IO74 = EMX.PwmPin.Controller1.P3_29;
                /// <summary>PWM pin.</summary>
                public const int IO48 = EMX.PwmPin.Controller1.P3_27;
            }
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>UART port on IO3 (TX) and IO2 (RX).</summary>
            public const string Uart0 = EMX.UartPort.Uart0;
            /// <summary>UART port on IO37 (TX), IO32 (RX), IO34 (CTS) and IO31 (RTS).</summary>
            public const string Uart1 = EMX.UartPort.Uart1;
            /// <summary>UART port on IO29 (TX) and IO28 (RX)
            public const string Uart2 = EMX.UartPort.Uart2;
            /// <summary>UART port on IO6 (TX) and IO7 (RX)
            public const string Uart3 = EMX.UartPort.Uart3;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on IO12 (SDA) and IO11 (SCL).</summary>
            public const string I2c1 = EMX.I2cBus.I2c0;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on IO24 (MOSI), IO25 (MISO), and IO27 (SCK).</summary>
            public const string Spi0 = EMX.SpiBus.Spi0;
            /// <summary>SPI bus on IO38 (MOSI), IO36 (MISO), and IO35 (SCK).</summary>
            public const string Spi1 = EMX.SpiBus.Spi1;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN bus on IO20 (TX) and IO22 (RX).</summary>
            public const string Can1 = EMX.CanBus.Can1;
            /// <summary>CAN bus on IO0 (TX) and IO1 (RX).</summary>
            public const string Can2 = EMX.CanBus.Can2;
        }

        /// <summary>USB client port definitions.</summary>
        public static class UsbClientPort {
            /// <summary>USB client port on UD_D- (D-), UD_D+ (D+), and UD_VBUS (VBUS).</summary>
            public const string UsbDevice = EMX.UsbClientPort.UsbDevice;
        }

        /// <summmary>USB host port definitions.</summmary>
        public static class UsbHostPort {
            /// <summary>USB host port on UH_D- (D-) and UH_D+ (D+).</summary>
            public const string UsbHost1 = EMX.UsbHostPort.UsbHost1;
        }
    }
}
