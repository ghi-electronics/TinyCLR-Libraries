using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Pins {
    public static class FEZPico {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>GPIO pin.</summary>
            public const int PA4 = SC13048.GpioPin.PA4;           
            /// <summary>GPIO pin.</summary>
            public const int PA1 = SC13048.GpioPin.PA1;
            /// <summary>GPIO pin.</summary>
            public const int PA5 = SC13048.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int PA15 = SC13048.GpioPin.PA15;
            /// <summary>GPIO pin.</summary>
            public const int PB2 = SC13048.GpioPin.PB2;
            /// <summary>GPIO pin.</summary>
            public const int PB7 = SC13048.GpioPin.PB7;
            /// <summary>GPIO pin.</summary>
            public const int PB15 = SC13048.GpioPin.PB15;
            /// <summary>GPIO pin.</summary>
            public const int PB13 = SC13048.GpioPin.PB13;
            /// <summary>GPIO pin.</summary>
            public const int PH1 = SC13048.GpioPin.PH1;
            /// <summary>GPIO pin.</summary>
            public const int PB14 = SC13048.GpioPin.PB14;
            /// <summary>GPIO pin.</summary>
            public const int PA9 = SC13048.GpioPin.PA9;
            /// <summary>GPIO pin.</summary>
            public const int PA10 = SC13048.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int PA0 = SC13048.GpioPin.PA0;
            /// <summary>GPIO pin.</summary>
            public const int PA14 = SC13048.GpioPin.PA14;
            /// <summary>GPIO pin.</summary>
            public const int PB9 = SC13048.GpioPin.PB9;
            /// <summary>GPIO pin.</summary>
            public const int PB8 = SC13048.GpioPin.PB8;
            /// <summary>GPIO pin.</summary>
            public const int PB6 = SC13048.GpioPin.PB6;
            /// <summary>GPIO pin.</summary>
            public const int PB12 = SC13048.GpioPin.PB12;
            /// <summary>GPIO pin.</summary>
            public const int PA2 = SC13048.GpioPin.PA2;
            /// <summary>GPIO pin.</summary>
            public const int PA3 = SC13048.GpioPin.PA3;
            /// <summary>GPIO pin.</summary>
            public const int PB3 = SC13048.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int PB4 = SC13048.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int PB5 = SC13048.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int PA13 = SC13048.GpioPin.PA13;
            /// <summary>GPIO pin.</summary>
            public const int PH3 = SC13048.GpioPin.PH3;
            /// <summary>GPIO pin.</summary>
            public const int PH0 = SC13048.GpioPin.PH0;
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

        public static class CanBus {
            /// <summary>CAN bus on PB6 (TX) and PB12 (RX).</summary>
            public const string Can1 = STM32L4.CanBus.Can1;
        }

        /// <summary>DAC channel definitions.</summary>
        public static class Dac {
            /// <summary>API id.</summary>
            public const string Id = STM32L4.Dac.Id;
            /// <summary>DAC channel.</summary>
            public const int PA4 = STM32L4.Dac.Channel1;
        }

        public static class DigitalSignal {
            public static class Controller2 {

                /// <summary>Capture pin.</summary>
                public const int PA1 = STM32H7.GpioPin.PA1;
            }
        }

        public static class Timer {
            /// <summary>PWM pin definitions.</summary>
            public static class Pwm {
                /// <summary>PWM controller.</summary>
                public static class Controller1 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32L4.Timer.Tim1;

                    /// <summary>Chip index for System.Device.Pwm.</summary>
                    public const int Chip = 0;

                    /// <summary>PWM pin.</summary>
                    public const int Led = STM32L4.Timer.Channel1;

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

                    /// <summary>Chip index for System.Device.Pwm.</summary>
                    public const int Chip = 1;

                    /// <summary>PWM pin.</summary>
                    public const int PA5 = STM32L4.Timer.Channel1;
                    /// <summary>PWM pin.</summary>
                    public const int PA1 = STM32L4.Timer.Channel2;
                }
                public static class Controller15 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32L4.Timer.Tim15;

                    /// <summary>Chip index for System.Device.Pwm.</summary>
                    public const int Chip = 14;

                    /// <summary>PWM pin.</summary>
                    public const int PA2 = STM32L4.Timer.Channel1;
                    /// <summary>PWM pin.</summary>
                    public const int PA3 = STM32L4.Timer.Channel2;
                }

                public static class Controller16 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32L4.Timer.Tim16;

                    /// <summary>Chip index for System.Device.Pwm.</summary>
                    public const int Chip = 15;

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
            /// <summary>UART port on PA9 (TX) and PA10 (RX)</summary>
            public const string Uart1 = STM32L4.UartPort.Usart1;
            /// <summary>UART port on PA2 (TX) and PA3 (RX), PA1 (CTS) and PA0 (RTS).</summary>
            public const string Uart2 = STM32L4.UartPort.Usart2;
            /// <summary>UART port on PA0 (TX) and PA1 (RX).</summary>
            public const string Uart4 = STM32L4.UartPort.Uart4;
        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on PB9 (SDA) and PB8 (SCL).</summary>
            public const string I2c1 = STM32L4.I2cBus.I2c1;
            /// <summary>Bus id for System.Device.I2c.</summary>
            public const int Chip0 = 0;
            /// <summary>I2C bus on PB14 (SDA) and PB13 (SCL).</summary>
            public const string I2c2 = STM32L4.I2cBus.I2c2;   
            /// <summary>Bus id for System.Device.I2c.</summary>
            public const int Chip1 = 1;
            /// <summary>I2C software.</summary>
            public const string Software = STM32L4.I2cBus.Software;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus on PB3 (MOSI), PB4 (MISO), and PB5 (SCK).</summary>
            public const string Spi1 = STM32L4.SpiBus.Spi2;
            /// <summary>Bus id for System.Device.Spi.</summary>
            public const int Chip1 = 1;
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
