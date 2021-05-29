namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the STM32L4.</summary>
    public static class STM32L4 {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = "GHIElectronics.TinyCLR.NativeApis.STM32L4.GpioController\\0";

            /// <summary>GPIO pin.</summary>
            public const int PA0 = 0;
            /// <summary>GPIO pin.</summary>
            public const int PA1 = 1;
            /// <summary>GPIO pin.</summary>
            public const int PA2 = 2;
            /// <summary>GPIO pin.</summary>
            public const int PA3 = 3;
            /// <summary>GPIO pin.</summary>
            public const int PA4 = 4;
            /// <summary>GPIO pin.</summary>
            public const int PA5 = 5;
            /// <summary>GPIO pin.</summary>
            public const int PA6 = 6;
            /// <summary>GPIO pin.</summary>
            public const int PA7 = 7;
            /// <summary>GPIO pin.</summary>
            public const int PA8 = 8;
            /// <summary>GPIO pin.</summary>
            public const int PA9 = 9;
            /// <summary>GPIO pin.</summary>
            public const int PA10 = 10;
            /// <summary>GPIO pin.</summary>
            public const int PA11 = 11;
            /// <summary>GPIO pin.</summary>
            public const int PA12 = 12;
            /// <summary>GPIO pin.</summary>
            public const int PA13 = 13;
            /// <summary>GPIO pin.</summary>
            public const int PA14 = 14;
            /// <summary>GPIO pin.</summary>
            public const int PA15 = 15;
            /// <summary>GPIO pin.</summary>
            public const int PB0 = 0 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB1 = 1 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB2 = 2 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB3 = 3 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB4 = 4 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB5 = 5 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB6 = 6 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB7 = 7 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB8 = 8 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB9 = 9 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB10 = 10 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB11 = 11 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB12 = 12 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB13 = 13 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB14 = 14 + 16;
            /// <summary>GPIO pin.</summary>
            public const int PB15 = 15 + 16;      
            /// <summary>GPIO pin.</summary>
            public const int PC13 = 13 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC14 = 14 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PC15 = 15 + 32;
            /// <summary>GPIO pin.</summary>
            public const int PH0 = 0 + 112;
            /// <summary>GPIO pin.</summary>
            public const int PH1 = 1 + 112;
            /// <summary>GPIO pin.</summary>
            public const int PH3 = 3 + 112;

        }

        /// <summary>ADC channel definitions.</summary>
        public static class Adc {
            /// <summary>API id.</summary>
            public const string Adc1 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.AdcController\\0";          
            /// <summary>ADC channel.</summary>
            public const int Channel5 = 5;
            /// <summary>ADC channel.</summary>
            public const int Channel6 = 6;
            /// <summary>ADC channel.</summary>
            public const int Channel7 = 7;
            /// <summary>ADC channel.</summary>
            public const int Channel8 = 8;
            /// <summary>ADC channel.</summary>
            public const int Channel9 = 9;
            /// <summary>ADC channel.</summary>
            public const int Channel10 = 10;
            /// <summary>ADC channel.</summary>
            public const int Channel11 = 11;
            /// <summary>ADC channel.</summary>
            public const int Channel12 = 12;           
            /// <summary>ADC channel.</summary>
            public const int Channel15 = 15;
            /// <summary>ADC channel.</summary>
            public const int Channel16 = 16;
            /// <summary>ADC channel.</summary>
            public const int Channel17 = 17;
            /// <summary>ADC channel.</summary>
            public const int Channel18 = 18;            
        }

        /// <summary>DAC channel definitions.</summary>
        public static class Dac {
            /// <summary>API id.</summary>
            public const string Id = "GHIElectronics.TinyCLR.NativeApis.STM32L4.DacController\\0";
            /// <summary>DAC channel.</summary>
            public const int Channel1 = 0;

        }

        /// <summary>PWM pin definitions.</summary>
        public static class Timer {
            /// <summary>API id.</summary>
            public const string Tim1 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.PwmController\\0";
            /// <summary>API id.</summary>
            public const string Tim2 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.PwmController\\1";           
            /// <summary>API id.</summary>
            public const string Tim15 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.PwmController\\14";
            /// <summary>API id.</summary>
            public const string Tim16 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.PwmController\\15";
            /// <summary>API id.</summary>
            public const string SoftwarePwm = "GHIElectronics.TinyCLR.NativeApis.STM32L4.SoftwarePwmController";
            /// <summary>PWM pin.</summary>
            public const int Channel1 = 0;
            /// <summary>PWM pin.</summary>
            public const int Channel2 = 1;
            /// <summary>PWM pin.</summary>
            public const int Channel3 = 2;
            /// <summary>PWM pin.</summary>
            public const int Channel4 = 3;

        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>UART port.</summary>
            public const string Usart1 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.UartController\\0";
            /// <summary>UART port.</summary>
            public const string Usart2 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.UartController\\1";
            /// <summary>UART port.</summary>
            public const string Usart3 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.UartController\\2";
            /// <summary>UART port.</summary>
            public const string Uart4 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.UartController\\3";
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus.</summary>
            public const string I2c1 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.I2cController\\0";
            /// <summary>I2C bus.</summary>
            public const string I2c2 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.I2cController\\1";
            /// <summary>I2C bus.</summary>
            public const string I2c4 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.I2cController\\3";
            /// <summary>I2C software.</summary>
            public const string Software = "GHIElectronics.TinyCLR.NativeApis.SoftwareI2cController";
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus.</summary>
            public const string Spi1 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.SpiController\\0";
            /// <summary>SPI bus.</summary>
            public const string Spi2 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.SpiController\\1";
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN bus.</summary>
            public const string Can1 = "GHIElectronics.TinyCLR.NativeApis.STM32L4.CanController\\0";
        }

        /// <summary>USB client port definitions.</summary>
        public static class UsbClientPort {
            /// <summary>USB client port.</summary>
            public const string UsbOtg = "GHIElectronics.TinyCLR.NativeApis.STM32L4.UsbClientController\\0";
        }

        /// <summary>USB host port definitions.</summary>
        public static class UsbHostPort {
            /// <summary>USB host port.</summary>
            public const string UsbOtg = "GHIElectronics.TinyCLR.NativeApis.STM32L4.UsbHostController\\0";
        }

        /// <summary>Display definitions.</summary>
        public static class Display {
            /// <summary>API id.</summary>
            public const string Lcd = "GHIElectronics.TinyCLR.NativeApis.STM32L4.DisplayController\\0";
        }
        
        /// <summary>Storage controller definitions.</summary>
        public static class StorageController {
            /// <summary>API id.</summary>
            public const string UsbHostMassStorage = "GHIElectronics.TinyCLR.NativeApis.STM32L4.UsbHostMassStorageStorageController\\0";
            public const string QuadSpi = "GHIElectronics.TinyCLR.NativeApis.STM32L4.QspiStorageController\\0";
        }

        /// <summary>Network controller definitions.</summary>
        public static class EthernetController {          
            /// <summary>Enc28j60 id.</summary>
            public const string EthernetEmac = "GHIElectronics.TinyCLR.NativeApis.STM32L4.EthernetEmacController\\0";
        }

        /// <summary>RTC controller definitions.</summary>
        public static class RtcController {
            /// <summary>API id.</summary>
            public const string Id = "GHIElectronics.TinyCLR.NativeApis.STM32L4.RtcController\\0";
        }
    }
}
