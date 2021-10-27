using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Pins {
    public static class FEZDuino {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>GPIO pin.</summary>
            public const int PA10 = SC20100.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int PA9 = SC20100.GpioPin.PA9;
            /// <summary>GPIO pin.</summary>
            public const int PA1 = SC20100.GpioPin.PA1;
            /// <summary>GPIO pin.</summary>
            public const int PB0 = SC20100.GpioPin.PB0;
            /// <summary>GPIO pin.</summary>
            public const int PA2 = SC20100.GpioPin.PA2;
            /// <summary>GPIO pin.</summary>
            public const int PC7 = SC20100.GpioPin.PC7;
            /// <summary>GPIO pin.</summary>
            public const int PC6 = SC20100.GpioPin.PC6;
            /// <summary>GPIO pin.</summary>
            public const int PC4 = SC20100.GpioPin.PC4;
            /// <summary>GPIO pin.</summary>
            public const int PC5 = SC20100.GpioPin.PC5;
            /// <summary>GPIO pin.</summary>
            public const int PA15 = SC20100.GpioPin.PA15;
            /// <summary>GPIO pin.</summary>
            public const int PB1 = SC20100.GpioPin.PB1;
            /// <summary>GPIO pin.</summary>
            public const int PA7 = SC20100.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int PA6 = SC20100.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int PA5 = SC20100.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int PB11 = SC20100.GpioPin.PB11;
            /// <summary>GPIO pin.</summary>
            public const int PB10 = SC20100.GpioPin.PB10;
            /// <summary>GPIO pin.</summary>
            public const int PA3 = SC20100.GpioPin.PA3;
            /// <summary>GPIO pin.</summary>
            public const int PA4 = SC20100.GpioPin.PA4;
            /// <summary>GPIO pin.</summary>
            public const int PC3 = SC20100.GpioPin.PC3;
            /// <summary>GPIO pin.</summary>
            public const int PA0 = SC20100.GpioPin.PA0;
            /// <summary>GPIO pin.</summary>
            public const int PC0 = SC20100.GpioPin.PC0;
            /// <summary>GPIO pin.</summary>
            public const int PC2 = SC20100.GpioPin.PC2;
            /// <summary>GPIO pin.</summary>
            public const int PD7 = SC20100.GpioPin.PD7;
            /// <summary>GPIO pin.</summary>
            public const int PD8 = SC20100.GpioPin.PD8;
            /// <summary>GPIO pin.</summary>
            public const int PD9 = SC20100.GpioPin.PD9;
            /// <summary>GPIO pin.</summary>
            public const int PE9 = SC20100.GpioPin.PE9;
            /// <summary>GPIO pin.</summary>
            public const int PE10 = SC20100.GpioPin.PE10;
            /// <summary>GPIO pin.</summary>
            public const int PE8 = SC20100.GpioPin.PE8;
            /// <summary>GPIO pin.</summary>
            public const int PE7 = SC20100.GpioPin.PE7;
            /// <summary>GPIO pin.</summary>
            public const int PD1 = SC20100.GpioPin.PD1;
            /// <summary>GPIO pin.</summary>
            public const int PD0 = SC20100.GpioPin.PD0;
            /// <summary>GPIO pin.</summary>
            public const int PB5 = SC20100.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int PB4 = SC20100.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int PB3 = SC20100.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int PB8 = SC20100.GpioPin.PB8;
            /// <summary>GPIO pin.</summary>
            public const int PB9 = SC20100.GpioPin.PB9;
            /// <summary>GPIO pin.</summary>
            public const int PE1 = SC20100.GpioPin.PE1;
            /// <summary>GPIO pin.</summary>
            public const int PE0= SC20100.GpioPin.PE0;
            /// <summary>GPIO pin.</summary>
            public const int Led = SC20100.GpioPin.PE11;
            /// <summary>GPIO pin.</summary>
            public const int WiFiInterrupt = SC20100.GpioPin.PB12;
            /// <summary>GPIO pin.</summary>
            public const int WiFiChipselect = SC20100.GpioPin.PD15;
            /// <summary>GPIO pin.</summary>
            public const int WiFiReset = SC20100.GpioPin.PB13;
            /// <summary>GPIO pin.</summary>
            public const int WiFiEnable = SC20100.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int ButtonLdr = SC20100.GpioPin.PE3;
            /// <summary>GPIO pin.</summary>
            public const int ButtonApp = SC20100.GpioPin.PB7;
        }

        public static class Timer {
            /// <summary>PWM pin definitions.</summary>
            public static class Pwm {
                /// <summary>PWM controller.</summary>
                public static class Controller1 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim1;

                    /// <summary>PWM pin.</summary>
                    public const int PE9 = STM32H7.Timer.Channel0;
                    /// <summary>PWM pin.</summary>
                    public const int Led = STM32H7.Timer.Channel1;                                        
                }

                /// <summary>PWM controller.</summary>
                public static class Controller2 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim2;

                    /// <summary>PWM pin.</summary>
                    public const int PA15 = STM32H7.Timer.Channel0;
                    /// <summary>PWM pin.</summary>
                    public const int PB3 = STM32H7.Timer.Channel1;
                    /// <summary>PWM pin.</summary>
                    public const int PA3 = STM32H7.Timer.Channel3;
                }

                /// <summary>PWM controller.</summary>
                public static class Controller3 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim3;

                    /// <summary>PWM pin.</summary>
                    public const int PC6 = STM32H7.Timer.Channel0;
                    /// <summary>PWM pin.</summary>
                    public const int PC7 = STM32H7.Timer.Channel1;
                    /// <summary>PWM pin.</summary>
                    public const int PB0 = STM32H7.Timer.Channel2;
                    /// <summary>PWM pin.</summary>
                    public const int PB1 = STM32H7.Timer.Channel3;
                }
                

                public static class Controller5 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim5;

                    /// <summary>PWM pin.</summary>
                    public const int PA0 = STM32H7.Timer.Channel0;
                }
                public static class Controller13 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim13;

                    /// <summary>PWM pin.</summary>
                    public const int PA6 = STM32H7.Timer.Channel0;
                }
                public static class Controller14 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim14;

                    /// <summary>PWM pin.</summary>
                    public const int PA7 = STM32H7.Timer.Channel0;
                }                

                public static class Controller16 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim16;

                    /// <summary>PWM pin.</summary>
                    public const int PB8 = STM32H7.Timer.Channel0;
                }

                public static class Controller17 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim17;

                    /// <summary>PWM pin.</summary>
                    public const int PB9 = STM32H7.Timer.Channel0;
                }

                public static class Software {
                    public const string Id = STM32H7.Timer.SoftwarePwm;
                }

                
            }
            /// <summary>Capture pin definitions.</summary>
            public static class DigitalSignal {
                public static class Controller5 {

                    /// <summary>Capture pin.</summary>
                    public const int PA0 = GpioPin.PA0;
                }

                public static class Controller2 {

                    /// <summary>Capture pin.</summary>
                    public const int PB3 = GpioPin.PB3;
                }
            }
        }

        /// <summary>ADC channel definitions.</summary>
        public static class Adc {
            /// <summary>ADC controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = STM32H7.Adc.Adc1;

                /// <summary>ADC pin.</summary>
                public const int PA6 = STM32H7.Adc.Channel3;
                /// <summary>ADC pin.</summary>
                public const int PB1 = STM32H7.Adc.Channel5;
                /// <summary>ADC pin.</summary>
                public const int PA7 = STM32H7.Adc.Channel7;
                /// <summary>ADC pin.</summary>
                public const int PB0 = STM32H7.Adc.Channel9;
                /// <summary>ADC pin.</summary>
                public const int PC0 = STM32H7.Adc.Channel10;              
                /// <summary>ADC pin.</summary>
                public const int PA3 = STM32H7.Adc.Channel15;
                /// <summary>ADC pin.</summary>
                public const int PA0 = STM32H7.Adc.Channel16;
                /// <summary>ADC pin.</summary>
                public const int PA4 = STM32H7.Adc.Channel18;
                /// <summary>ADC pin.</summary>
                public const int PA5 = STM32H7.Adc.Channel19;
            }
            /// <summary>ADC controller.</summary>
            public static class Controller3 {
                /// <summary>API id.</summary>
                public const string Id = STM32H7.Adc.Adc3;

                /// <summary>ADC pin.</summary>
                public const int PC2 = STM32H7.Adc.Channel0;
                /// <summary>ADC pin.</summary>
                public const int PC3 = STM32H7.Adc.Channel1;
                /// <summary>ADC pin.</summary>
                public const int InternalReferenceVoltage = STM32H7.Adc.Channel17;
                /// <summary>ADC pin.</summary>
                public const int InternalTemperatureSensor = STM32H7.Adc.Channel18;
                /// <summary>ADC pin.</summary>
                public const int VBAT = STM32H7.Adc.Channel19;
            }
        }

        /// <summary>DAC channel definitions.</summary>
        public static class Dac {
            /// <summary>API id.</summary>
            public const string Id = STM32H7.Dac.Id;

            /// <summary>DAC channel.</summary>
            public const int PA4 = STM32H7.Dac.Channel1;
            /// <summary>DAC channel.</summary>
            public const int PA5 = STM32H7.Dac.Channel2;
        }

        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>UART port on PA9 (TX) and PA10 (RX).</summary>
            public const string Uart1 = STM32H7.UartPort.Usart1;
            /// <summary>UART port on PD8 (TX) and PD9 (RX).</summary>
            public const string Uart3 = STM32H7.UartPort.Usart3;
            /// <summary>UART port on PD0 (TX) and PD1 (RX), PB0 (CTS) and PA15 (RTS).</summary>
            public const string Uart4 = STM32H7.UartPort.Uart4;            
            /// <summary>UART port on PC6 (TX) and PC7 (RX).</summary>
            public const string Uart6 = STM32H7.UartPort.Usart6;
            /// <summary>UART port on PE8 (TX) and PE7 (RX), PE10 (CTS) and PE9 (RTS).</summary>
            public const string Uart7 = STM32H7.UartPort.Uart7;
            /// <summary>UART port on PE0 (TX) and PE1 (RX).</summary>
            public const string Uart8 = STM32H7.UartPort.Uart8;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on PB5 (MOSI), PB4 (MISO), and PB3 (SCK).</summary>
            public const string Spi3 = STM32H7.SpiBus.Spi3;           
            /// <summary>SPI bus on PA7 (MOSI), PA6 (MISO), and PA5 (SCK).</summary>
            public const string Spi6 = STM32H7.SpiBus.Spi6;
            /// <summary>SPI bus on PB5 (MOSI), PB4 (MISO), and PB3 (SCK).</summary>
            public const string WiFi = STM32H7.SpiBus.Spi3;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on PB9 (SDA) and PB8 (SCL).</summary>
            public const string I2c1 = STM32H7.I2cBus.I2c1;
            /// <summary>I2C bus on PB11 (SDA) and PB10 (SCL).</summary>
            public const string I2c2 = STM32H7.I2cBus.I2c2;
            /// <summary>I2C software.</summary>
            public const string Software = STM32H7.I2cBus.Software;
        }

        /// <summary>Storage controller definitions.</summary>
        public static class StorageController {
            /// <summary>API id.</summary>
            public const string SdCard = STM32H7.StorageController.SdCard;
            public const string UsbHostMassStorage = STM32H7.StorageController.UsbHostMassStorage;
            public const string QuadSpi = STM32H7.StorageController.QuadSpi;
        }

        /// <summary>RTC controller definitions.</summary>
        public static class RtcController {
            /// <summary>API id.</summary>
            public const string Id = STM32H7.RtcController.Id;
        }
    }
}
