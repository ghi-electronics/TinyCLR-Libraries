namespace GHIElectronics.TinyCLR.Pins {
    /// <summary>Board definition for the SC20100.</summary>
    public static class SC13048 {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>API id.</summary>
            public const string Id = STM32L4.GpioPin.Id;

            /// <summary>GPIO pin.</summary>
            public const int PA0 = STM32L4.GpioPin.PA0;
            /// <summary>GPIO pin.</summary>
            public const int PA1 = STM32L4.GpioPin.PA1;
            /// <summary>GPIO pin.</summary>
            public const int PA2 = STM32L4.GpioPin.PA2;
            /// <summary>GPIO pin.</summary>
            public const int PA3 = STM32L4.GpioPin.PA3;
            /// <summary>GPIO pin.</summary>
            public const int PA4 = STM32L4.GpioPin.PA4;
            /// <summary>GPIO pin.</summary>
            public const int PA5 = STM32L4.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int PA6 = STM32L4.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int PA7 = STM32L4.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int PA8 = STM32L4.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int PA9 = STM32L4.GpioPin.PA9;
            /// <summary>GPIO pin.</summary>
            public const int PA10 = STM32L4.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int PA13 = STM32L4.GpioPin.PA13;
            /// <summary>GPIO pin.</summary>
            public const int PA14 = STM32L4.GpioPin.PA14;
            /// <summary>GPIO pin.</summary>            
            public const int PA15 = STM32L4.GpioPin.PA15;
            /// <summary>GPIO pin.</summary>
            public const int PB0 = STM32L4.GpioPin.PB0;
            /// <summary>GPIO pin.</summary>
            public const int PB1 = STM32L4.GpioPin.PB1;
            /// <summary>GPIO pin.</summary>
            public const int PB2 = STM32L4.GpioPin.PB2;
            /// <summary>GPIO pin.</summary>
            public const int PB3 = STM32L4.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int PB4 = STM32L4.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int PB5 = STM32L4.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int PB6 = STM32L4.GpioPin.PB6;
            /// <summary>GPIO pin.</summary>
            public const int PB7 = STM32L4.GpioPin.PB7;
            /// <summary>GPIO pin.</summary>
            public const int PB8 = STM32L4.GpioPin.PB8;
            /// <summary>GPIO pin.</summary>
            public const int PB9 = STM32L4.GpioPin.PB9;
            /// <summary>GPIO pin.</summary>
            public const int PB10 = STM32L4.GpioPin.PB10;
            /// <summary>GPIO pin.</summary>
            public const int PB11 = STM32L4.GpioPin.PB11;
            /// <summary>GPIO pin.</summary>
            public const int PB12 = STM32L4.GpioPin.PB12;
            /// <summary>GPIO pin.</summary>
            public const int PB13 = STM32L4.GpioPin.PB13;
            /// <summary>GPIO pin.</summary>
            public const int PB14 = STM32L4.GpioPin.PB14;
            /// <summary>GPIO pin.</summary>
            public const int PB15 = STM32L4.GpioPin.PB15;
            /// <summary>GPIO pin.</summary>
            public const int PC13 = STM32L4.GpioPin.PC13;
            /// <summary>GPIO pin.</summary>
            public const int PC14 = STM32L4.GpioPin.PC14;
            /// <summary>GPIO pin.</summary>
            public const int PC15 = STM32L4.GpioPin.PC15;
            /// <summary>GPIO pin.</summary>
            public const int PH0 = STM32L4.GpioPin.PH0;
            /// <summary>GPIO pin.</summary>
            public const int PH1 = STM32L4.GpioPin.PH1;
            /// <summary>GPIO pin.</summary>
            public const int PH3 = STM32L4.GpioPin.PH3;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class Adc {
            /// <summary>ADC controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = STM32L4.Adc.Adc1;
                /// <summary>ADC pin.</summary>
                public const int PA3 = STM32L4.Adc.Channel8;
                /// <summary>ADC pin.</summary>
                public const int PA0 = STM32L4.Adc.Channel5;
                /// <summary>ADC pin.</summary>
                public const int PA1 = STM32L4.Adc.Channel6;
                /// <summary>ADC pin.</summary>
                public const int PA2 = STM32L4.Adc.Channel7;
                /// <summary>ADC pin.</summary>
                public const int PA4 = STM32L4.Adc.Channel9;
                /// <summary>ADC pin.</summary>
                public const int PA5 = STM32L4.Adc.Channel10;
                /// <summary>ADC pin.</summary>
                public const int PA6 = STM32L4.Adc.Channel11;
                /// <summary>ADC pin.</summary>
                public const int PA7 = STM32L4.Adc.Channel12;
                /// <summary>ADC pin.</summary>
                public const int PB0 = STM32L4.Adc.Channel15;
                /// <summary>ADC pin.</summary>
                public const int PB1 = STM32L4.Adc.Channel16;                
                /// <summary>ADC pin.</summary>                
                public const int InternalTemperatureSensor = STM32L4.Adc.Channel17;                
            }          
        }

        /// <summary>DAC channel definitions.</summary>
        public static class Dac {
            /// <summary>API id.</summary>
            public const string Id = STM32L4.Dac.Id;
            /// <summary>DAC channel.</summary>
            public const int PA4 = STM32L4.Dac.Channel1;
        }

        /// <summary>Network controller definitions.</summary>
        public static class NetworkController {
            /// <summary>Enc28j60 id.</summary>
            public const string Enc28j60 = "GHIElectronics.TinyCLR.NativeApis.ENC28J60.NetworkController";

            /// <summary>AT Winc15x0 id.</summary>
            public const string ATWinc15x0 = "GHIElectronics.TinyCLR.NativeApis.ATWINC15xx.NetworkController";

            /// <summary>PPP id.</summary>
            public const string Ppp = "GHIElectronics.TinyCLR.NativeApis.Ppp.NetworkController";
        }

        public static class Timer {
            /// <summary>PWM pin definitions.</summary>
            public static class Pwm {
                /// <summary>PWM controller.</summary>
                public static class Controller1 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32L4.Timer.Tim1;

                    /// <summary>PWM pin.</summary>
                    public const int PA8 = STM32L4.Timer.Channel1;
                    /// <summary>PWM pin.</summary>
                    public const int PA9 = STM32L4.Timer.Channel2;
                    /// <summary>PWM pin.</summary>
                    public const int PA10 = STM32L4.Timer.Channel3;
                    /// <summary>PWM pin.</summary>
                    public const int PA11 = STM32L4.Timer.Channel4;
                }

                /// <summary>PWM controller.</summary>
                public static class Controller2 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32L4.Timer.Tim2;

                    /// <summary>PWM pin.</summary>
                    public const int PA5 = STM32L4.Timer.Channel1;
                    /// <summary>PWM pin.</summary>
                    public const int PA1 = STM32L4.Timer.Channel2;
                    /// <summary>PWM pin.</summary>
                    public const int PB10 = STM32L4.Timer.Channel3;
                    /// <summary>PWM pin.</summary>
                    public const int PB11 = STM32L4.Timer.Channel4;
                }
                public static class Controller15 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32L4.Timer.Tim15;

                    /// <summary>PWM pin.</summary>
                    public const int PA2 = STM32L4.Timer.Channel1;
                    /// <summary>PWM pin.</summary>
                    public const int PA3 = STM32L4.Timer.Channel2;
                }

                public static class Controller16 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32L4.Timer.Tim16;

                    /// <summary>PWM pin.</summary>
                    public const int PB8 = STM32L4.Timer.Channel1;
                }
                
                public static class Software {
                    public const string Id = STM32L4.Timer.SoftwarePwm;
                }
            }
            /// <summary>Capture pin definitions.</summary>
            public static class DigitalSignal {
                public static class Controller1 {

                    /// <summary>Capture pin.</summary>
                    public const int PA1 = GpioPin.PA1;
                }
            }
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>UART port on PA9 (TX) and PA10 (RX).</summary>
            public const string Uart1 = STM32L4.UartPort.Usart1;

            /// <summary>UART port on PD5 (TX) and PD6 (RX), PD3 (CTS) and PD4 (RTS).</summary>
            public const string Uart2 = STM32L4.UartPort.Usart2;

            /// <summary>UART port on PD8 (TX) and PD9 (RX).</summary>
            public const string Uart3 = STM32L4.UartPort.Usart3;

            /// <summary>UART port on PD0 (TX) and PD1 (RX), PB0 (CTS) and PA15 (RTS).</summary>
            public const string Uart4 = STM32L4.UartPort.Uart4;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on PB9 (SDA) and PB8 (SCL).</summary>
            public const string I2c1 = STM32L4.I2cBus.I2c1;
            /// <summary>I2C bus on PB14 (SDA) and PB13 (SCL).</summary>
            public const string I2c2 = STM32L4.I2cBus.I2c2;
            /// <summary>I2C bus on PB11 (SDA) and PB10 (SCL).</summary>
            public const string I2c4 = STM32L4.I2cBus.I2c4;
            /// <summary>I2C software.</summary>
            public const string Software = STM32L4.I2cBus.Software;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on PB5 (MOSI), PB4 (MISO), and PB3 (SCK).</summary>
            public const string Spi1 = STM32L4.SpiBus.Spi1;
            /// <summary>SPI bus on PB15 (MOSI), PB14 (MISO), and PB13 (SCK).</summary>
            public const string Spi2 = STM32L4.SpiBus.Spi2;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN bus on PD1 (TX) and PD0 (RX).</summary>
            public const string Can1 = STM32L4.CanBus.Can1;
        }

        ///// <summary>USB client port definitions.</summary>
        //public static class UsbClientPort {
        //    /// <summary>USB client port on USBC D- (DM) and USBC D+ (DP).</summary>
        //    public const string Udphs = STM32L4.UsbClientPort.Udphs;
        //}

        ///// <summary>USB host port definitions.</summary>
        //public static class UsbHostPort {
        //    /// <summary>USB host port on USBC D- (DM) and USBC D+ (DP).</summary>
        //    public const string UhphsA = STM32L4.UsbHostPort.UhphsA;
        //    /// <summary>USB host port on USBH0 D- (DM) and USBH0 D+ (DP).</summary>
        //    public const string UhphsB = STM32L4.UsbHostPort.UhphsB;
        //    /// <summary>USB host port on USBH1 D- (DM) and USBH1 D+ (DP).</summary>
        //    public const string UhphsC = STM32L4.UsbHostPort.UhphsC;
        //}        

        /// <summary>Storage controller definitions.</summary>
        public static class StorageController {
            /// <summary>API id.</summary>            
            public const string QuadSpi = STM32L4.StorageController.QuadSpi;
        }

        /// <summary>RTC controller definitions.</summary>
        public static class RtcController {
            /// <summary>API id.</summary>
            public const string Id = STM32L4.RtcController.Id;
        }
    }
}
