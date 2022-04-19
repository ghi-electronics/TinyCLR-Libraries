using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Pins {
    public static class FEZFeather {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>GPIO pin.</summary>
            public const int PD2 = SC20260.GpioPin.PD2;
            /// <summary>GPIO pin.</summary>
            public const int PC12 = SC20260.GpioPin.PC12;
            /// <summary>GPIO pin.</summary>
            public const int PC11 = SC20260.GpioPin.PC11;
            /// <summary>GPIO pin.</summary>
            public const int PC10 = SC20260.GpioPin.PC10;
            /// <summary>GPIO pin.</summary>
            public const int PC9 = SC20260.GpioPin.PC9;
            /// <summary>GPIO pin.</summary>
            public const int PC8 = SC20260.GpioPin.PC8;
            /// <summary>GPIO pin.</summary>
            public const int PD7 = SC20260.GpioPin.PD7;
            /// <summary>GPIO pin.</summary>
            public const int PA9 = SC20260.GpioPin.PA9;
            /// <summary>GPIO pin.</summary>
            public const int PA10 = SC20260.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int PE13 = SC20260.GpioPin.PE13;
            /// <summary>GPIO pin.</summary>
            public const int PE14 = SC20260.GpioPin.PE14;
            /// <summary>GPIO pin.</summary>
            public const int PE12 = SC20260.GpioPin.PE12;
            /// <summary>GPIO pin.</summary>
            public const int PB1 = SC20260.GpioPin.PB1;
            /// <summary>GPIO pin.</summary>
            public const int PA5 = SC20260.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int PA7 = SC20260.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int PA6 = SC20260.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int PA3 = SC20260.GpioPin.PA3;
            /// <summary>GPIO pin.</summary>
            public const int PA0 = SC20260.GpioPin.PA0;
            /// <summary>GPIO pin.</summary>
            public const int PE8 = SC20260.GpioPin.PE8;
            /// <summary>GPIO pin.</summary>
            public const int PE7 = SC20260.GpioPin.PE7;
            /// <summary>GPIO pin.</summary>
            public const int PD1 = SC20260.GpioPin.PD1;
            /// <summary>GPIO pin.</summary>
            public const int PD0 = SC20260.GpioPin.PD0;
            /// <summary>GPIO pin.</summary>
            public const int PB9 = SC20260.GpioPin.PB9;
            /// <summary>GPIO pin.</summary>
            public const int PB8 = SC20260.GpioPin.PB8;
            /// <summary>GPIO pin.</summary>
            public const int PD3 = SC20260.GpioPin.PD3;
            /// <summary>GPIO pin.</summary>
            public const int PD4 = SC20260.GpioPin.PD4;
            /// <summary>GPIO pin.</summary>
            public const int PD5 = SC20260.GpioPin.PD5;
            /// <summary>GPIO pin.</summary>
            public const int PD6 = SC20260.GpioPin.PD6;
            /// <summary>GPIO pin.</summary>
            public const int PE1 = SC20260.GpioPin.PE1;
            /// <summary>GPIO pin.</summary>
            public const int PE0 = SC20260.GpioPin.PE0;
            /// <summary>GPIO pin.</summary>
            public const int PE6 = SC20260.GpioPin.PE6;
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
                public const int PA0 = STM32H7.Adc.Channel16;                
                /// <summary>ADC pin.</summary>
                public const int PA5 = STM32H7.Adc.Channel19;
            }
            /// <summary>ADC controller.</summary>
            public static class Controller3 {
                /// <summary>API id.</summary>
                public const string Id = STM32H7.Adc.Adc3;
                
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
            public const int PA5 = STM32H7.Dac.Channel2;
        }

        public static class Timer {
            /// <summary>PWM pin definitions.</summary>
            public static class Pwm {
                /// <summary>PWM controller.</summary>
                public static class Controller1 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim1;
                    
                    /// <summary>PWM pin.</summary>
                    public const int Led = STM32H7.Timer.Channel1;                    
                    /// <summary>PWM pin.</summary>
                    public const int PE14 = STM32H7.Timer.Channel3;
                }

                /// <summary>PWM controller.</summary>
                public static class Controller2 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim2;

                  
                    /// <summary>PWM pin.</summary>
                    public const int PA3 = STM32H7.Timer.Channel3;
                }

                /// <summary>PWM controller.</summary>
                public static class Controller3 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim3;
                                     
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
                public static class Controller15 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim15;
                
                    /// <summary>PWM pin.</summary>
                    public const int PE6 = STM32H7.Timer.Channel1;
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
            }
        }
        /// <summary>UART port definitions.</summary>
        public static class UartPort {
            /// <summary>UART port on PA9 (TX) and PA10 (RX).</summary>
            public const string Uart1 = STM32H7.UartPort.Usart1;

            /// <summary>UART port on PD5 (TX) and PD6 (RX), PD3 (CTS) and PD4 (RTS).</summary>
            public const string Uart2 = STM32H7.UartPort.Usart2;            
            /// <summary>UART port on PD0 (TX) and PD1 (RX).</summary>
            public const string Uart4 = STM32H7.UartPort.Uart4;                        
            /// <summary>UART port on PE8 (TX) and PE7 (RX).</summary>
            public const string Uart7 = STM32H7.UartPort.Uart7;
            /// <summary>UART port on PE0 (TX) and PE1 (RX).</summary>
            public const string Uart8 = STM32H7.UartPort.Uart8;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on PB9 (SDA) and PB8 (SCL).</summary>
            public const string I2c1 = STM32H7.I2cBus.I2c1;
            /// <summary>I2C software.</summary>
            public const string Software = STM32H7.I2cBus.Software;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on PE14 (MOSI), PE13 (MISO), and PE12 (SCK).</summary>
            public const string Spi4 = STM32H7.SpiBus.Spi4;
            /// <summary>SPI bus on PA7 (MOSI), PA6 (MISO), and PA5 (SCK).</summary>
            public const string Spi6 = STM32H7.SpiBus.Spi6;
            /// <summary>SPI bus.</summary>
            public const string WiFi = STM32H7.SpiBus.Spi3;
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN bus on PD1 (TX) and PD0 (RX).</summary>
            public const string Can1 = STM32H7.CanBus.Can1;           
        }


        /// <summary>Storage controller definitions.</summary>
        public static class StorageController {
            /// <summary>API id.</summary>
            public const string SdCard = STM32H7.StorageController.SdCard;
            public const string UsbHostMassStorage = STM32H7.StorageController.UsbHostMassStorage;            
        }

        /// <summary>RTC controller definitions.</summary>
        public static class RtcController {
            /// <summary>API id.</summary>
            public const string Id = STM32H7.RtcController.Id;
        }


    }
}
