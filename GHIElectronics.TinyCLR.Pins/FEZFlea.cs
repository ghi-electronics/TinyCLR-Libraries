using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Pins {
    public static class FEZFlea {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>GPIO pin.</summary>
            public const int PA4 = SC13048.GpioPin.PA4;
            /// <summary>GPIO pin.</summary>
            public const int PA0 = SC13048.GpioPin.PA0;
            /// <summary>GPIO pin.</summary>
            public const int PA1 = SC13048.GpioPin.PA1;
            /// <summary>GPIO pin.</summary>
            public const int PA5 = SC13048.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int PB9 = SC13048.GpioPin.PB9;
            /// <summary>GPIO pin.</summary>
            public const int PB8 = SC13048.GpioPin.PB8;
            /// <summary>GPIO pin.</summary>
            public const int PA2 = SC13048.GpioPin.PA2;
            /// <summary>GPIO pin.</summary>
            public const int PB15 = SC13048.GpioPin.PB15;
            /// <summary>GPIO pin.</summary>
            public const int PB14 = SC13048.GpioPin.PB14;
            /// <summary>GPIO pin.</summary>
            public const int PB13 = SC13048.GpioPin.PB13;
            /// <summary>GPIO pin.</summary>
            public const int PA3 = SC13048.GpioPin.PA3;
            /// <summary>Led pin.</summary>
            public const int Led = SC13048.GpioPin.PA8;
            /// <summary>ButtonLdr pin.</summary>
            public const int ButtonLdr = SC13048.GpioPin.PC13;            

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

        public static class Timer {
            /// <summary>PWM pin definitions.</summary>
            public static class Pwm {
                /// <summary>PWM controller.</summary>
                public static class Controller1 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32L4.Timer.Tim1;

                    /// <summary>PWM pin.</summary>
                    public const int Led = STM32L4.Timer.Channel1;                    
                }

                /// <summary>PWM controller.</summary>
                public static class Controller2 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32L4.Timer.Tim2;

                    /// <summary>PWM pin.</summary>
                    public const int PA5 = STM32L4.Timer.Channel1;
                    /// <summary>PWM pin.</summary>
                    public const int PA1 = STM32L4.Timer.Channel2;                   
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
            /// <summary>UART port on PA2 (TX) and PA3 (RX), PA1 (CTS) and PA0 (RTS).</summary>
            public const string Uart2 = STM32L4.UartPort.Usart2;            
            /// <summary>UART port on PA0 (TX) and PA1 (RX).</summary>
            public const string Uart4 = STM32L4.UartPort.Uart4;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on PB9 (SDA) and PB8 (SCL).</summary>
            public const string I2c1 = STM32L4.I2cBus.I2c1;
            /// <summary>I2C bus on PB14 (SDA) and PB13 (SCL).</summary>
            public const string I2c2 = STM32L4.I2cBus.I2c2;            
            /// <summary>I2C software.</summary>
            public const string Software = STM32L4.I2cBus.Software;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {           
            /// <summary>SPI bus on PB15 (MOSI), PB14 (MISO), and PB13 (SCK).</summary>
            public const string Spi2 = STM32L4.SpiBus.Spi2;
        }

        /// <summary>RTC controller definitions.</summary>
        public static class RtcController {
            /// <summary>API id.</summary>
            public const string Id = STM32L4.RtcController.Id;
        }
    }
}
