using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Pins {
    public static class FEZPortal {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>GPIO pin.</summary>
            public const int PH9 = SC20260.GpioPin.PH9;
            /// <summary>GPIO pin.</summary>
            public const int PG10 = SC20260.GpioPin.PG10;
            /// <summary>GPIO pin.</summary>
            public const int PI1 = SC20260.GpioPin.PI1;
            /// <summary>GPIO pin.</summary>
            public const int PI2 = SC20260.GpioPin.PI2;
            /// <summary>GPIO pin.</summary>
            public const int PI3 = SC20260.GpioPin.PI3;
            /// <summary>GPIO pin.</summary>
            public const int PA3 = SC20260.GpioPin.PA3;
            /// <summary>GPIO pin.</summary>
            public const int PH10 = SC20260.GpioPin.PH10;
            /// <summary>GPIO pin.</summary>
            public const int PF9 = SC20260.GpioPin.PF9;
            /// <summary>GPIO pin.</summary>
            public const int PF8 = SC20260.GpioPin.PF8;
            /// <summary>GPIO pin.</summary>
            public const int PB5 = SC20260.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int PB4 = SC20260.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int PB3 = SC20260.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int PJ10 = SC20260.GpioPin.PJ10;
            /// <summary>GPIO pin.</summary>
            public const int PJ11 = SC20260.GpioPin.PJ11;
            /// <summary>GPIO pin.</summary>
            public const int PK0 = SC20260.GpioPin.PK0;
            /// <summary>GPIO pin.</summary>
            public const int PB8 = SC20260.GpioPin.PB8;
            /// <summary>GPIO pin.</summary>
            public const int PB9 = SC20260.GpioPin.PB9;
            /// <summary>GPIO pin.</summary>
            public const int PD7 = SC20260.GpioPin.PD7;
            /// <summary>GPIO pin.</summary>
            public const int PB13 = SC20260.GpioPin.PB13;
            /// <summary>GPIO pin.</summary>
            public const int PB12 = SC20260.GpioPin.PB12;
            /// <summary>GPIO pin.</summary>
            public const int PF7 = SC20260.GpioPin.PF7;
            /// <summary>GPIO pin.</summary>
            public const int PF6 = SC20260.GpioPin.PF6;
            /// <summary>GPIO pin.</summary>
            public const int PH12 = SC20260.GpioPin.PH12;
            /// <summary>GPIO pin.</summary>
            public const int PE4 = SC20260.GpioPin.PE4;
            /// <summary>GPIO pin.</summary>
            public const int PI4 = SC20260.GpioPin.PI4;
            /// <summary>GPIO pin.</summary>
            public const int PA0 = SC20260.GpioPin.PA0;
            /// <summary>GPIO pin.</summary>
            public const int PA4 = SC20260.GpioPin.PA4;
            /// <summary>GPIO pin.</summary>
            public const int PJ8 = SC20260.GpioPin.PJ8;
            /// <summary>GPIO pin.</summary>
            public const int PJ9 = SC20260.GpioPin.PJ9;
            /// <summary>GPIO pin.</summary>
            public const int PA5 = SC20260.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int PE6 = SC20260.GpioPin.PE6;
            /// <summary>GPIO pin.</summary>
            public const int PH13 = SC20260.GpioPin.PH13;
            /// <summary>GPIO pin.</summary>
            public const int PH14 = SC20260.GpioPin.PH14;
            /// <summary>GPIO pin.</summary>
            public const int PC6 = SC20260.GpioPin.PC6;
            /// <summary>GPIO pin.</summary>
            public const int PC7 = SC20260.GpioPin.PC7;
            /// <summary>GPIO pin.</summary>
            public const int PC0 = SC20260.GpioPin.PC0;
            /// <summary>GPIO pin.</summary>
            public const int PD4 = SC20260.GpioPin.PD4;
            /// <summary>GPIO pin.</summary>
            public const int PC13 = SC20260.GpioPin.PC13;
            /// <summary>GPIO pin.</summary>
            public const int PE5 = SC20260.GpioPin.PE5;
            /// <summary>GPIO pin.</summary>
            public const int PC2 = SC20260.GpioPin.PC2;
            /// <summary>GPIO pin.</summary>
            public const int PD6 = SC20260.GpioPin.PD6;
            /// <summary>GPIO pin.</summary>
            public const int PD5 = SC20260.GpioPin.PD5;

            /// <summary>Led pin.</summary>
            public const int Led = SC20260.GpioPin.PB0;
            /// <summary>Buzzer pin.</summary>
            public const int Buzzer = SC20260.GpioPin.PB1;
            /// <summary>Backlight pin.</summary>
            public const int Backlight = SC20260.GpioPin.PA15;
            /// <summary>TouchInterrupt pin.</summary>
            public const int TouchInterrupt = SC20260.GpioPin.PG9;
            /// <summary>ButtonLdr pin.</summary>
            public const int ButtonLdr = SC20260.GpioPin.PE3;
            /// <summary>ButtonApp pin.</summary>
            public const int ButtonApp = SC20260.GpioPin.PB7;
            /// <summary>WiFiChipSelect pin.</summary>
            public const int WiFiChipSelect = SC20260.GpioPin.PA6;
            /// <summary>WiFiChipEnable pin.</summary>
            public const int WiFiEnable = SC20260.GpioPin.PA8;
            /// <summary>WiFiChipReset pin.</summary>
            public const int WiFiReset = SC20260.GpioPin.PC3;
            /// <summary>WiFiChipInterrupt pin.</summary>
            public const int WiFiInterrupt = SC20260.GpioPin.PF10;
        }

        /// <summary>ADC channel definitions.</summary>
        public static class Adc {
            /// <summary>ADC controller.</summary>
            public static class Controller1 {
                /// <summary>API id.</summary>
                public const string Id = STM32H7.Adc.Adc1;
                
                /// <summary>ADC pin.</summary>
                public const int PC0 = STM32H7.Adc.Channel10;
                /// <summary>ADC pin.</summary>
                public const int PC2 = STM32H7.Adc.Channel12;                
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
                public const int PF9 = STM32H7.Adc.Channel2;
                /// <summary>ADC pin.</summary>
                public const int PF7 = STM32H7.Adc.Channel3;                
                /// <summary>ADC pin.</summary>
                public const int PF8 = STM32H7.Adc.Channel7;
                /// <summary>ADC pin.</summary>
                public const int PF6 = STM32H7.Adc.Channel8;                
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

        public static class Timer {
            /// <summary>PWM pin definitions.</summary>        
            public static class Pwm {

                /// <summary>PWM controller.</summary>
                public static class Controller1 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim1;
                    
                    /// <summary>PWM pin.</summary>
                    public const int PJ11 = STM32H7.Timer.Channel1;
                    /// <summary>PWM pin.</summary>
                    public const int PJ9 = STM32H7.Timer.Channel2;

                }

                /// <summary>PWM controller.</summary>
                public static class Controller2 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim2;

                    /// <summary>PWM pin.</summary>
                    public const int Backlight = STM32H7.Timer.Channel0;
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
                    public const int Led = STM32H7.Timer.Channel2;
                    /// <summary>PWM pin.</summary>
                    public const int Buzzer = STM32H7.Timer.Channel3;
                }

               
                public static class Controller5 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim5;

                    /// <summary>PWM pin.</summary>
                    public const int PA0 = STM32H7.Timer.Channel0;
                    /// <summary>PWM pin.</summary>
                    public const int PH12 = STM32H7.Timer.Channel2;
                    
                }

                public static class Controller8 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim8;

                    /// <summary>PWM pin.</summary>
                    public const int PI2 = STM32H7.Timer.Channel3;
                }
                public static class Controller12 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim12;
             
                    /// <summary>PWM pin.</summary>
                    public const int PH9 = STM32H7.Timer.Channel1;
                }
                public static class Controller13 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim13;

                    /// <summary>PWM pin.</summary>
                    public const int PF8 = STM32H7.Timer.Channel0;
                }
                public static class Controller14 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim14;

                    /// <summary>PWM pin.</summary>
                    public const int PF9 = STM32H7.Timer.Channel0;
                }

                public static class Controller15 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim15;

                    /// <summary>PWM pin.</summary>
                    public const int PE5 = STM32H7.Timer.Channel0;

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
                    /// <summary>API id.</summary>
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

        /// <summary>UART port definitions.</summary>
        public static class UartPort {            
            /// <summary>UART port on PD5 (TX) and PD6 (RX) </summary>
            public const string Uart2 = STM32H7.UartPort.Usart2;
            /// <summary>UART port on PD5 (TX) and PD6 (RX) </summary>
            public const string MikroBus = STM32H7.UartPort.Usart2;
            /// <summary>UART port on PH13 (TX) and PH14 (RX).</summary>
            public const string Uart4 = STM32H7.UartPort.Uart4;
            /// <summary>UART port on PB13 (TX) and PB12 (RX).</summary>
            public const string Uart5 = STM32H7.UartPort.Uart5;
            /// <summary>UART port on PC6 (TX) and PC7 (RX).</summary>
            public const string Uart6 = STM32H7.UartPort.Usart6;
            /// <summary>UART port on PF7 (TX) and PF6 (RX), PF9 (CTS) and PF8 (RTS).</summary>
            public const string Uart7 = STM32H7.UartPort.Uart7;
            /// <summary>UART port on PJ8 (TX) and PJ9 (RX).</summary>
            public const string Uart8 = STM32H7.UartPort.Uart8;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on PB9 (SDA) and PB8 (SCL).</summary>
            public const string I2c1 = STM32H7.I2cBus.I2c1;
            /// <summary>I2C bus on PB9 (SDA) and PB8 (SCL).</summary>
            public const string Touch = STM32H7.I2cBus.I2c1;
            /// <summary>I2C bus on PB9 (SDA) and PB8 (SCL).</summary>
            public const string MikroBus = STM32H7.I2cBus.I2c1;
            /// <summary>I2C software.</summary>
            public const string Software = STM32H7.I2cBus.Software;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on PI3 (MOSI), PI2 (MISO), and PI1 (SCK).</summary>
            public const string Spi2 = STM32H7.SpiBus.Spi2;
            /// <summary>SPI bus on PI3 (MOSI), PI2 (MISO), and PI1 (SCK).</summary>
            public const string MikroBus = STM32H7.SpiBus.Spi2;
            /// <summary>SPI bus on PB5 (MOSI), PB4 (MISO), and PB3 (SCK).</summary>
            public const string Spi3 = STM32H7.SpiBus.Spi3;
            /// <summary>SPI bus on PJ10 (MOSI), PJ11 (MISO), and PK0 (SCK).</summary>
            public const string Spi5 = STM32H7.SpiBus.Spi5;            
        }

        /// <summary>CAN bus definitions.</summary>
        public static class CanBus {
            /// <summary>CAN bus on PH13 (TX) and PH14 (RX).</summary>
            public const string Can1 = STM32H7.CanBus.Can1;
            /// <summary>CAN bus on PB13 (TX) and PB12 (RX).</summary>
            public const string Can2 = STM32H7.CanBus.Can2;
        }

        /// <summary>Display definitions.</summary>
        public static class Display {
            /// <summary>API id.</summary>
            public const string Lcd = STM32H7.Display.Lcd;
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
