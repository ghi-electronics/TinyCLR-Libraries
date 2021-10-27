using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Pins {
    public static class FEZBit {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>GPIO pin.</summary>
            public const int Led = SC20100.GpioPin.PE11;
            /// <summary>GPIO pin.</summary>
            public const int Buzzer = SC20100.GpioPin.PB1;
            /// <summary>GPIO pin.</summary>
            public const int WiFiInterrupt = SC20100.GpioPin.PB12;
            /// <summary>GPIO pin.</summary>
            public const int WiFiChipselect = SC20100.GpioPin.PD15;
            /// <summary>GPIO pin.</summary>
            public const int WiFiEnable = SC20100.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int WiFiReset = SC20100.GpioPin.PB13;
            /// <summary>GPIO pin.</summary>
            public const int Backlight = SC20100.GpioPin.PA15;
            /// <summary>GPIO pin.</summary>
            public const int DisplayChipselect = SC20100.GpioPin.PD10;
            /// <summary>GPIO pin.</summary>
            public const int DisplayRs = SC20100.GpioPin.PC4;
            /// <summary>GPIO pin.</summary>
            public const int DisplayReset = SC20100.GpioPin.PE15;
            /// <summary>GPIO pin.</summary>
            public const int ButtonLeft = SC20100.GpioPin.PE3;
            /// <summary>GPIO pin.</summary>
            public const int ButtonRight = SC20100.GpioPin.PB7;
            /// <summary>GPIO pin.</summary>
            public const int ButtonUp = SC20100.GpioPin.PE4;
            /// <summary>GPIO pin.</summary>
            public const int ButtonDown = SC20100.GpioPin.PA1;
            /// <summary>GPIO pin.</summary>
            public const int ButtonA = SC20100.GpioPin.PE5;
            /// <summary>GPIO pin.</summary>
            public const int ButtonB = SC20100.GpioPin.PE6;
            /// <summary>GPIO pin.</summary>
            public const int P0 = SC20100.GpioPin.PC6;
            /// <summary>GPIO pin.</summary>
            public const int P1 = SC20100.GpioPin.PC7;
            /// <summary>GPIO pin.</summary>
            public const int P2 = SC20100.GpioPin.PA0;
            /// <summary>GPIO pin.</summary>
            public const int P3 = SC20100.GpioPin.PB0;
            /// <summary>GPIO pin.</summary>
            public const int P4 = SC20100.GpioPin.PA4;
            /// <summary>GPIO pin.</summary>
            public const int P5 = SC20100.GpioPin.PD13;
            /// <summary>GPIO pin.</summary>
            public const int P6 = SC20100.GpioPin.PD12;
            /// <summary>GPIO pin.</summary>
            public const int P7 = SC20100.GpioPin.PD11;
            /// <summary>GPIO pin.</summary>
            public const int P8 = SC20100.GpioPin.PE8;
            /// <summary>GPIO pin.</summary>
            public const int P9 = SC20100.GpioPin.PC3;
            /// <summary>GPIO pin.</summary>
            public const int P10 = SC20100.GpioPin.PC0;
            /// <summary>GPIO pin.</summary>
            public const int P11 = SC20100.GpioPin.PD1;
            /// <summary>GPIO pin.</summary>
            public const int P12 = SC20100.GpioPin.PD0;
            /// <summary>GPIO pin.</summary>
            public const int P13 = SC20100.GpioPin.PA5;
            /// <summary>GPIO pin.</summary>
            public const int P14 = SC20100.GpioPin.PA6;
            /// <summary>GPIO pin.</summary>
            public const int P15 = SC20100.GpioPin.PA7;
            /// <summary>GPIO pin.</summary>
            public const int P16 = SC20100.GpioPin.PE7;
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
                }

                /// <summary>PWM controller.</summary>
                public static class Controller2 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim2;

                    /// <summary>PWM pin.</summary>
                    public const int Backlight = STM32H7.Timer.Channel0;
                    /// <summary>PWM pin.</summary>                    
                }

                /// <summary>PWM controller.</summary>
                public static class Controller3 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim3;

                    /// <summary>PWM pin.</summary>
                    public const int P0 = STM32H7.Timer.Channel0;
                    /// <summary>PWM pin.</summary>
                    public const int P1 = STM32H7.Timer.Channel1;
                    /// <summary>PWM pin.</summary>
                    public const int P3 = STM32H7.Timer.Channel2;
                    /// <summary>PWM pin.</summary>
                    public const int Buzzer = STM32H7.Timer.Channel3;

                }

                public static class Controller5 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim5;

                    /// <summary>PWM pin.</summary>
                    public const int P2 = STM32H7.Timer.Channel0;
                }
                public static class Controller13 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim13;

                    /// <summary>PWM pin.</summary>
                    public const int P14 = STM32H7.Timer.Channel0;
                }
                public static class Controller14 {
                    /// <summary>API id.</summary>
                    public const string Id = STM32H7.Timer.Tim14;

                    /// <summary>PWM pin.</summary>
                    public const int P15 = STM32H7.Timer.Channel0;
                }


                public static class Software {
                    public const string Id = STM32H7.Timer.SoftwarePwm;
                }
            }
            /// <summary>Capture pin definitions.</summary>
            public static class DigitalSignal {
                public static class Controller5 {

                    /// <summary>Capture pin.</summary>
                    public const int P2 = STM32H7.GpioPin.PA0;
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
                public const int P14 = STM32H7.Adc.Channel3;
                /// <summary>ADC pin.</summary>
                public const int P15 = STM32H7.Adc.Channel7;
                /// <summary>ADC pin.</summary>
                public const int P3 = STM32H7.Adc.Channel9;
                /// <summary>ADC pin.</summary>
                public const int P10 = STM32H7.Adc.Channel10;
                /// <summary>ADC pin.</summary>
                public const int P2 = STM32H7.Adc.Channel16;
                /// <summary>ADC pin.</summary>
                public const int P4 = STM32H7.Adc.Channel18;
                /// <summary>ADC pin.</summary>
                public const int P13 = STM32H7.Adc.Channel19;
            }
            /// <summary>ADC controller.</summary>
            public static class Controller3 {
                /// <summary>API id.</summary>
                public const string Id = STM32H7.Adc.Adc3;

                /// <summary>ADC pin.</summary>
                public const int P9 = STM32H7.Adc.Channel1;
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
            /// <summary>UART port on PD0 (TX) and PD1 (RX), PB0 (CTS) and PA15 (RTS).</summary>
            public const string Uart4 = STM32H7.UartPort.Uart4;          
            /// <summary>UART port on PC6 (TX) and PC7 (RX).</summary>
            public const string Uart6 = STM32H7.UartPort.Usart6;
            /// <summary>UART port on PE8 (TX) and PE7 (RX).</summary>
            public const string Uart7 = STM32H7.UartPort.Uart7;
        }

        /// <summary>SPI bus definitions.</summary>
        public static class SpiBus {
            /// <summary>SPI bus.</summary>
            public const string Display = STM32H7.SpiBus.Spi4;
            /// <summary>SPI bus.</summary>
            public const string WiFi = STM32H7.SpiBus.Spi3;
            /// <summary>SPI bus.</summary>
            public const string Edge = STM32H7.SpiBus.Spi6;

        }

        /// <summary>I2C bus definitions.</summary>
        public static class I2cBus {
            /// <summary>I2C bus on PB9 (SDA) and PB8 (SCL).</summary>
            public const string Edge = STM32H7.I2cBus.I2c1;
            /// <summary>I2C bus on PB9 (SDA) and PB8 (SCL).</summary>
            public const string Accelerometer = STM32H7.I2cBus.I2c1;
            /// <summary>I2C software.</summary>
            public const string Software = STM32H7.I2cBus.Software;
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
        }

        /// <summary>RTC controller definitions.</summary>
        public static class RtcController {
            /// <summary>API id.</summary>
            public const string Id = STM32H7.RtcController.Id;
        }
    }
}
