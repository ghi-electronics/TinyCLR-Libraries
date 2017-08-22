namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Panda.</summary>
    public static class FEZPanda {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = USBizi100.GpioPin.Id;

            /// <summary>Debug LED definition</summary>
            public const int DebugLed = USBizi100.GpioPin.P0_16;
            /// <summary>GPIO pin.</summary>
            public const int A0 = USBizi100.GpioPin.P0_23;
            /// <summary>GPIO pin.</summary>
            public const int A1 = USBizi100.GpioPin.P0_24;
            /// <summary>GPIO pin.</summary>
            public const int A2 = USBizi100.GpioPin.P0_25;
            /// <summary>GPIO pin.</summary>
            public const int A3 = USBizi100.GpioPin.P0_26;
            /// <summary>GPIO pin.</summary>
            public const int A4 = USBizi100.GpioPin.P1_30;
            /// <summary>GPIO pin.</summary>
            public const int A5 = USBizi100.GpioPin.P1_31;
            /// <summary>GPIO pin.</summary>
            public const int D0 = USBizi100.GpioPin.P0_3;
            /// <summary>GPIO pin.</summary>
            public const int D1 = USBizi100.GpioPin.P0_2;
            /// <summary>GPIO pin.</summary>
            public const int D2 = USBizi100.GpioPin.P0_27;
            /// <summary>GPIO pin.</summary>
            public const int D3 = USBizi100.GpioPin.P0_28;
            /// <summary>GPIO pin.</summary>
            public const int D4 = USBizi100.GpioPin.P0_0;
            /// <summary>GPIO pin.</summary>
            public const int D5 = USBizi100.GpioPin.P2_4;
            /// <summary>GPIO pin.</summary>
            public const int D6 = USBizi100.GpioPin.P2_5;
            /// <summary>GPIO pin.</summary>
            public const int D7 = USBizi100.GpioPin.P0_1;
            /// <summary>GPIO pin.</summary>
            public const int D8 = USBizi100.GpioPin.P1_21;
            /// <summary>GPIO pin.</summary>
            public const int D9 = USBizi100.GpioPin.P1_20;
            /// <summary>GPIO pin.</summary>
            public const int D10 = USBizi100.GpioPin.P1_18;
            /// <summary>GPIO pin.</summary>
            public const int D11 = USBizi100.GpioPin.P0_18;
            /// <summary>GPIO pin.</summary>
            public const int D12 = USBizi100.GpioPin.P0_17;
            /// <summary>GPIO pin.</summary>
            public const int D13 = USBizi100.GpioPin.P0_15;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class AdcChannel {
            /// <summary>API id.</summary>
            public const string Id = USBizi100.AdcChannel.Id;

            /// <summary>ADC channel.</summary>
            public const int A0 = USBizi100.GpioPin.P0_23;
            /// <summary>ADC channel.</summary>
            public const int A1 = USBizi100.GpioPin.P0_24;
            /// <summary>ADC channel.</summary>
            public const int A2 = USBizi100.GpioPin.P0_25;
            /// <summary>ADC channel.</summary>
            public const int A3 = USBizi100.GpioPin.P0_26;
            /// <summary>ADC channel.</summary>
            public const int A4 = USBizi100.GpioPin.P1_30;
            /// <summary>ADC channel.</summary>
            public const int A5 = USBizi100.GpioPin.P1_31;
        }

        /// <summary>DAC channel definitions.</summary>
        public static class DacChannel {
            /// <summary>API id.</summary>
            public const string Id = USBizi100.DacChannel.Id;

            /// <summary>DAC channel.</summary>
            public const int A3 = USBizi100.DacChannel.P0_26;
        }

        /// <summary>PWM pin definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = USBizi100.PwmPin.Controller1.Id;

                /// <summary>PWM channel.</summary>
                public const int D10 = USBizi100.PwmPin.Controller1.P1_18;
                /// <summary>PWM channel.</summary>
                public const int D9 = USBizi100.PwmPin.Controller1.P1_20;
                /// <summary>PWM channel.</summary>
                public const int D8 = USBizi100.PwmPin.Controller1.P1_21;
                /// <summary>PWM channel.</summary>
                public const int D6 = USBizi100.PwmPin.Controller1.P2_4;
                /// <summary>PWM channel.</summary>
                public const int D5 = USBizi100.PwmPin.Controller1.P2_5;
            }
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>UART port on D1 (TX) and D0 (RX).</summary>
            public const string Usart1 = USBizi100.UartPort.Uart0;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on D2 (SDA) and D3 (SCL).</summary>
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