namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the FEZ Lemur.</summary>
    public static class FEZLemur {
        /// <summary>GPIO definitions.</summary>
        public static class GpioPin {
            /// <summary>The pin for LED 1.</summary>
            public const int Led1 = G30.GpioPin.PB9;
            /// <summary>The pin for LED 2.</summary>
            public const int Led2 = G30.GpioPin.PB8;
            /// <summary>The pin for the LDR0 button.</summary>
            public const int Ldr0 = G30.GpioPin.PA15;
            /// <summary>The pin for the LDR1 button.</summary>
            public const int Ldr1 = G30.GpioPin.PC13;
            /// <summary>The SD card detect pin.</summary>
            public const int SdCardDetect = G30.GpioPin.PB12;
            /// <summary>GPIO pin.</summary>
            public const int A0 = G30.GpioPin.PA4;
            /// <summary>GPIO pin.</summary>
            public const int A1 = G30.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int A2 = G30.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int A3 = G30.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int A4 = G30.GpioPin.PB0;
            /// <summary>GPIO pin.</summary>
            public const int A5 = G30.GpioPin.PB1;
            /// <summary>GPIO pin.</summary>
            public const int D0 = G30.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int D1 = G30.GpioPin.PA9;
            /// <summary>GPIO pin.</summary>
            public const int D2 = G30.GpioPin.PB7;
            /// <summary>GPIO pin.</summary>
            public const int D3 = G30.GpioPin.PB6;
            /// <summary>GPIO pin.</summary>
            public const int D4 = G30.GpioPin.PC15;
            /// <summary>GPIO pin.</summary>
            public const int D5 = G30.GpioPin.PA1;
            /// <summary>GPIO pin.</summary>
            public const int D6 = G30.GpioPin.PA0;
            /// <summary>GPIO pin.</summary>
            public const int D7 = G30.GpioPin.PC14;
            /// <summary>GPIO pin.</summary>
            public const int D8 = G30.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int D9 = G30.GpioPin.PA2;
            /// <summary>GPIO pin.</summary>
            public const int D10 = G30.GpioPin.PA3;
            /// <summary>GPIO pin.</summary>
            public const int D11 = G30.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int D12 = G30.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int D13 = G30.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int D20 = G30.GpioPin.PC0;
            /// <summary>GPIO pin.</summary>
            public const int D21 = G30.GpioPin.PC1;
            /// <summary>GPIO pin.</summary>
            public const int D22 = G30.GpioPin.PA13;
            /// <summary>GPIO pin.</summary>
            public const int D23 = G30.GpioPin.PA14;
            /// <summary>GPIO pin.</summary>
            public const int D24 = G30.GpioPin.PC2;
            /// <summary>GPIO pin.</summary>
            public const int D25 = G30.GpioPin.PC3;
            /// <summary>GPIO pin.</summary>
            public const int Mod = G30.GpioPin.PB10;
            /// <summary>GPIO pin.</summary>
            public const int D26 = G30.GpioPin.PB13;
            /// <summary>GPIO pin.</summary>
            public const int D27 = G30.GpioPin.PB14;
            /// <summary>GPIO pin.</summary>
            public const int D28 = G30.GpioPin.PB15;
            /// <summary>GPIO pin.</summary>
            public const int D29 = G30.GpioPin.PC6;
            /// <summary>GPIO pin.</summary>
            public const int D30 = G30.GpioPin.PC7;
            /// <summary>GPIO pin.</summary>
            public const int D31 = G30.GpioPin.PB2;
            /// <summary>GPIO pin.</summary>
            public const int D32 = G30.GpioPin.PC4;
            /// <summary>GPIO pin.</summary>
            public const int D33 = G30.GpioPin.PC5;
        }

        /// <summary>Analog input definitions.</summary>
        public static class AdcChannel {
            /// <summary>Analog channel.</summary>
            public const int D6 = G30.AdcChannel.PA0;
            /// <summary>Analog channel.</summary>
            public const int D5 = G30.AdcChannel.PA1;
            /// <summary>Analog channel.</summary>
            public const int D9 = G30.AdcChannel.PA2;
            /// <summary>Analog channel.</summary>
            public const int D10 = G30.AdcChannel.PA3;
            /// <summary>Analog channel.</summary>
            public const int A0 = G30.AdcChannel.PA4;
            /// <summary>Analog channel.</summary>
            public const int A1 = G30.AdcChannel.PA5;
            /// <summary>Analog channel.</summary>
            public const int A2 = G30.AdcChannel.PA6;
            /// <summary>Analog channel.</summary>
            public const int A3 = G30.AdcChannel.PA7;
            /// <summary>Analog channel.</summary>
            public const int A4 = G30.AdcChannel.PB0;
            /// <summary>Analog channel.</summary>
            public const int A5 = G30.AdcChannel.PB1;
            /// <summary>Analog channel.</summary>
            public const int D20 = G30.AdcChannel.PC0;
            /// <summary>Analog channel.</summary>
            public const int D21 = G30.AdcChannel.PC1;
            /// <summary>Analog channel.</summary>
            public const int D24 = G30.AdcChannel.PC2;
            /// <summary>Analog channel.</summary>
            public const int D25 = G30.AdcChannel.PC3;
            /// <summary>Analog channel.</summary>
            public const int D32 = G30.AdcChannel.PC4;
            /// <summary>Analog channel.</summary>
            public const int D33 = G30.AdcChannel.PC5;
        }

        /// <summary>PWM output definitions.</summary>
        public static class PwmPin {
            /// <summary>PWM channel.</summary>
            public const int D8 = G30.PwmPin.PA8;
            /// <summary>PWM channel.</summary>
            public const int D1 = G30.PwmPin.PA9;
            /// <summary>PWM channel.</summary>
            public const int D0 = G30.PwmPin.PA10;
            /// <summary>PWM channel.</summary>
            public const int Led2 = G30.PwmPin.PB8;
            /// <summary>PWM channel.</summary>
            public const int Led1 = G30.PwmPin.PB9;
            /// <summary>PWM channel.</summary>
            public const int D6 = G30.PwmPin.PA0;
            /// <summary>PWM channel.</summary>
            public const int D5 = G30.PwmPin.PA1;
            /// <summary>PWM channel.</summary>
            public const int D9 = G30.PwmPin.PA2;
            /// <summary>PWM channel.</summary>
            public const int D10 = G30.PwmPin.PA3;
            /// <summary>PWM channel.</summary>
            public const int D29 = G30.PwmPin.PC6;
            /// <summary>PWM channel.</summary>
            public const int D30 = G30.PwmPin.PC7;
            /// <summary>PWM channel.</summary>
            public const int D3 = G30.PwmPin.PB6;
            /// <summary>PWM channel.</summary>
            public const int D2 = G30.PwmPin.PB7;
        }

        /// <summary>Serial port definitions.</summary>
        public static class SerialPort {
            /// <summary>Serial port on D1 (TX) and D0 (RX).</summary>
            public const string Com1 = G30.SerialPort.Com1;
            /// <summary>Serial port on D9 (TX), D10 (RX), D6 (CTS), and D5 (RTS).</summary>
            public const string Com2 = G30.SerialPort.Com2;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>SPI bus on D2 (SDA) and D3 (SCL).</summary>
            public const string I2c1 = G30.I2cBus.I2c1;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on D11 (MOSI), D12 (MISO), and D13 (SCK).</summary>
            public const string Spi1 = G30.SpiBus.Spi1;
            /// <summary>SPI bus on D28 (MOSI), D27 (MISO), and D26 (SCK).</summary>
            public const string Spi2 = G30.SpiBus.Spi2;
        }
    }
}