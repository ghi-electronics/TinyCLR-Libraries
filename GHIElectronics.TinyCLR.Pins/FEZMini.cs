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

            /// <summary>ADC channel.</summary>
            public const int An0 = USBizi100.AdcChannel.P0_23;
            /// <summary>ADC channel.</summary>
            public const int An1 = USBizi100.AdcChannel.P0_24;
            /// <summary>ADC channel.</summary>
            public const int An2 = USBizi100.AdcChannel.P0_25;
            /// <summary>ADC channel.</summary>
            public const int An3 = USBizi100.AdcChannel.P0_26;
            /// <summary>ADC channel.</summary>
            public const int An6 = USBizi100.AdcChannel.P1_30;
            /// <summary>ADC channel.</summary>
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
        public static class PwmChannel {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = USBizi100.PwmChannel.Controller1.Id;

                /// <summary>PWM pin.</summary>
                public const int Di3 = USBizi100.PwmChannel.Controller1.P1_18;
                /// <summary>PWM pin.</summary>
                public const int Di5 = USBizi100.PwmChannel.Controller1.P1_20;
                /// <summary>PWM pin.</summary>
                public const int Di6 = USBizi100.PwmChannel.Controller1.P1_21;
                /// <summary>PWM pin.</summary>
                public const int Di9 = USBizi100.PwmChannel.Controller1.P2_4;
                /// <summary>PWM pin.</summary>
                public const int Di10 = USBizi100.PwmChannel.Controller1.P2_5;
            }
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>UART port on OUT (TX) and IN (RX).</summary>
            public const string Uart0 = USBizi100.UartPort.Uart0;
            /// <summary>UART port on UEXT TXD (TX), UEXT RXD (RX), UEXT CTS (CTS), and UEXT RTS (RTS).</summary>
            public const string Uart1 = USBizi100.UartPort.Uart1;
            /// <summary>UART port on Di8 (TX) and Di7 (RX).</summary>
            public const string Uart2 = USBizi100.UartPort.Uart2;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on SDA (SDA) and SCL (SCL).</summary>
            public const string I2c1 = USBizi100.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on UEXT MOSI (MOSI), UEXT MISO (MISO), and UEXT SCK (SCK).</summary>
            public const string Spi1 = USBizi100.SpiBus.Spi1;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN bus on Di4 (TX) and Di2 (RX).</summary>
            public const string Can1 = USBizi100.CanBus.Can1;
        }

        /// <summry>USB client port definitions.</summry>
        public static class UsbClientPort {
            /// <summary>USB client port on D- (D-), D+ (D+), and VBUS (VBUS).</summary>
            public const string UsbDevice = USBizi100.UsbClientPort.UsbDevice;
        }
    }
}
