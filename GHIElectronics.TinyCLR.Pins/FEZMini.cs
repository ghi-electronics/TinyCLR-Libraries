namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Mini.</summary>
    public static class FEZMini {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = USBizi100.GpioPin.Id;

            /// <summary>GPIO pin.</summary>
            public const int Di2 = USBizi100.GpioPin.P0_0;
            /// <summary>GPIO pin.</summary>
            public const int Di3 = USBizi100.GpioPin.P1_18;
            /// <summary>GPIO pin.</summary>
            public const int Di4 = USBizi100.GpioPin.P0_1;
            /// <summary>GPIO pin.</summary>
            public const int Di5 = USBizi100.GpioPin.P1_20;
            /// <summary>GPIO pin.</summary>
            public const int Di6 = USBizi100.GpioPin.P1_21;
            /// <summary>GPIO pin.</summary>
            public const int Di7 = USBizi100.GpioPin.P0_11;
            /// <summary>GPIO pin.</summary>
            public const int Di8 = USBizi100.GpioPin.P0_10;
            /// <summary>GPIO pin.</summary>
            public const int Di9 = USBizi100.GpioPin.P2_4;
            /// <summary>GPIO pin.</summary>
            public const int Di10 = USBizi100.GpioPin.P2_5;
            /// <summary>GPIO pin.</summary>
            public const int An0 = USBizi100.GpioPin.P0_23;
            /// <summary>GPIO pin.</summary>
            public const int An1 = USBizi100.GpioPin.P0_24;
            /// <summary>GPIO pin.</summary>
            public const int An2 = USBizi100.GpioPin.P0_25;
            /// <summary>GPIO pin.</summary>
            public const int An3 = USBizi100.GpioPin.P0_26;
            /// <summary>GPIO pin.</summary>
            public const int An6 = USBizi100.GpioPin.P1_30;
            /// <summary>GPIO pin.</summary>
            public const int An7 = USBizi100.GpioPin.P1_31;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>API id.</summary>
            public const string Id = USBizi100.AdcChannel.Id;

            /// <summary>ADC pin.</summary>
            public const int An0 = USBizi100.AdcChannel.P0_23;
            /// <summary>GPIO pin.</summary>
            public const int An1 = USBizi100.AdcChannel.P0_24;
            /// <summary>GPIO pin.</summary>
            public const int An2 = USBizi100.AdcChannel.P0_25;
            /// <summary>GPIO pin.</summary>
            public const int An3 = USBizi100.AdcChannel.P0_26;
            /// <summary>GPIO pin.</summary>
            public const int An6 = USBizi100.AdcChannel.P1_30;
            /// <summary>GPIO pin.</summary>
            public const int An7 = USBizi100.AdcChannel.P1_31;
        }

        /// <summary>DAC channel definitions.</summary>
        public static class DacChannel {
            /// <summary>API id.</summary>
            public const string Id = USBizi100.DacChannel.Id;

            /// <summary>DAC channel.</summary>
            public const int An3 = USBizi100.DacChannel.P0_26;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = USBizi100.PwmPin.Controller1.Id;

                /// <summary>PWM pin.</summary>
                public const int Di3 = USBizi100.PwmPin.Controller1.P1_18;
                /// <summary>PWM pin.</summary>
                public const int Di5 = USBizi100.PwmPin.Controller1.P1_20;
                /// <summary>PWM pin.</summary>
                public const int Di6 = USBizi100.PwmPin.Controller1.P1_21;
                /// <summary>PWM pin.</summary>
                public const int Di9 = USBizi100.PwmPin.Controller1.P2_4;
                /// <summary>PWM pin.</summary>
                public const int Di10 = USBizi100.PwmPin.Controller1.P2_5;
            }
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>UART port on Di7 (TX) and Di8 (RX).</summary>
            public const string Usart1 = USBizi100.UartPort.Uart0;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on An4 (SDA) and An5 (SCL).</summary>
            public const string I2c1 = USBizi100.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on D11 (MOSI), D12 (MISO), and D13 (SCK).</summary>
            public const string Spi1 = USBizi100.SpiBus.Spi1;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN bus on D7 (TX) and D4 (RX).</summary>
            public const string Can1 = USBizi100.CanBus.Can1;
        }
    }
}