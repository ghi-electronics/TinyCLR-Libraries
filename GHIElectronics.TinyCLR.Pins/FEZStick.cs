using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Pins {
    public static class FEZStick {
        /// <summary>GPIO pin definitions.</summary>
        public static class GpioPin {
            /// <summary>GPIO pin.</summary>
            public const int PE5 = SC20260.GpioPin.PE5;
            /// <summary>GPIO pin.</summary>
            public const int PC5 = SC20260.GpioPin.PC5;
            /// <summary>GPIO pin.</summary>
            public const int PA10 = SC20260.GpioPin.PA10;
            /// <summary>GPIO pin.</summary>
            public const int PA9 = SC20260.GpioPin.PA9;
            /// <summary>GPIO pin.</summary>
            public const int PB8 = SC20260.GpioPin.PB8;
            /// <summary>GPIO pin.</summary>
            public const int PB9 = SC20260.GpioPin.PB9;
            /// <summary>GPIO pin.</summary>
            public const int PD2 = SC20260.GpioPin.PD2;
            /// <summary>GPIO pin.</summary>
            public const int PC12 = SC20260.GpioPin.PC12;
            /// <summary>GPIO pin.</summary>
            public const int PC11 = SC20260.GpioPin.PC11;
            /// <summary>GPIO pin.</summary>
            public const int PC10 = SC20260.GpioPin.PC10;
            /// <summary>GPIO pin.</summary>
            public const int PB3 = SC20260.GpioPin.PB3;
            /// <summary>GPIO pin.</summary>
            public const int PB4 = SC20260.GpioPin.PB4;
            /// <summary>GPIO pin.</summary>
            public const int PB5 = SC20260.GpioPin.PB5;
            /// <summary>GPIO pin.</summary>
            public const int PC1 = SC20260.GpioPin.PC1;
            /// <summary>GPIO pin.</summary>
            public const int PD15 = SC20260.GpioPin.PD15;
            /// <summary>GPIO pin.</summary>
            public const int PD14 = SC20260.GpioPin.PD14;
            /// <summary>GPIO pin.</summary>
            public const int PA3 = SC20260.GpioPin.PA3;
            /// <summary>GPIO pin.</summary>
            public const int PA8 = SC20260.GpioPin.PA8;
            /// <summary>GPIO pin.</summary>
            public const int PB12 = SC20260.GpioPin.PB12;
            /// <summary>GPIO pin.</summary>
            public const int PB13 = SC20260.GpioPin.PB13;
            /// <summary>GPIO pin.</summary>
            public const int PB10 = SC20260.GpioPin.PB10;
            /// <summary>GPIO pin.</summary>
            public const int PB11 = SC20260.GpioPin.PB11;
            /// <summary>GPIO pin.</summary>
            public const int PC8 = SC20260.GpioPin.PC8;
            /// <summary>GPIO pin.</summary>
            public const int PC9 = SC20260.GpioPin.PC9;
            /// <summary>GPIO pin.</summary>
            public const int PE14 = SC20260.GpioPin.PE14;
            /// <summary>GPIO pin.</summary>
            public const int PE13 = SC20260.GpioPin.PE13;
            /// <summary>GPIO pin.</summary>
            public const int PE12 = SC20260.GpioPin.PE12;
            /// <summary>GPIO pin.</summary>
            public const int PD3 = SC20260.GpioPin.PD3;
            /// <summary>GPIO pin.</summary>
            public const int PD4 = SC20260.GpioPin.PD4;
            /// <summary>GPIO pin.</summary>
            public const int PC0 = SC20260.GpioPin.PC0;
            /// <summary>GPIO pin.</summary>
            public const int PB7 = SC20260.GpioPin.PB7;
            /// <summary>GPIO pin.</summary>
            public const int PD7 = SC20260.GpioPin.PD7;
            /// <summary>GPIO pin.</summary>        
            public const int PE3 = SC20260.GpioPin.PE3;
            /// <summary>Led pin.</summary>
            public const int Led = SC20260.GpioPin.PE11;
            /// <summary>ButtonLdr pin.</summary>
            public const int ButtonLdr = SC20100.GpioPin.PE3;
            /// <summary>ButtonApp pin.</summary>
            public const int ButtonApp = SC20100.GpioPin.PB7;
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
                public const int PC1 = STM32H7.Adc.Channel11;
                /// <summary>ADC pin.</summary>
                public const int PA3 = STM32H7.Adc.Channel15;
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

            public static class Timer {
                /// <summary>PWM pin definitions.</summary>
                public static class Pwm {
                    /// <summary>PWM controller.</summary>
                    public static class Controller1 {
                        /// <summary>API id.</summary>
                        public const string Id = STM32H7.Timer.Tim1;

                        /// <summary>Led pin.</summary>
                        public const int Led = STM32H7.Timer.Channel1;
                        /// <summary>PWM pin.</summary>
                        public const int PE13 = STM32H7.Timer.Channel2;
                        /// <summary>PWM pin.</summary>
                        public const int PE14 = STM32H7.Timer.Channel3;
                    }

                    /// <summary>PWM controller.</summary>
                    public static class Controller2 {
                        /// <summary>API id.</summary>
                        public const string Id = STM32H7.Timer.Tim2;


                        /// <summary>PWM pin.</summary>
                        public const int PB3 = STM32H7.Timer.Channel1;
                        /// <summary>PWM pin.</summary>
                        public const int PA3 = STM32H7.Timer.Channel3;
                    }


                    /// <summary>PWM controller.</summary>
                    public static class Controller4 {
                        /// <summary>API id.</summary>
                        public const string Id = STM32H7.Timer.Tim4;

                        /// <summary>PWM pin.</summary>
                        public const int PB7 = STM32H7.Timer.Channel1;

                    }



                    public static class Controller15 {
                        /// <summary>API id.</summary>
                        public const string Id = STM32H7.Timer.Tim15;

                        /// <summary>PWM pin.</summary>
                        public const int PE5 = STM32H7.Timer.Channel0;

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
                    public static class Controller2 {

                        /// <summary>Capture pin.</summary>
                        public const int PB3 = STM32H7.GpioPin.PB3;
                    }
                }
            }

            /// <summary>UART port definitions.</summary>
            public static class UartPort {
                /// <summary>UART port on PA9 (TX) and PA10 (RX).</summary>
                public const string Uart1 = STM32H7.UartPort.Usart1;

                /// <summary>UART port on PB13 (TX) and PB12 (RX), PC9 (CTS) and PC8 (RTS).</summary>
                public const string Uart5 = STM32H7.UartPort.Uart5;
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

            /// <summary>SPI bus definitions.</summary>
            public static class SpiBus {
                /// <summary>SPI bus on PB5 (MOSI), PB4 (MISO), and PB3 (SCK).</summary>
                public const string Spi3 = STM32H7.SpiBus.Spi3;
                /// <summary>SPI bus on PE14 (MOSI), PE13 (MISO), and PE12 (SCK).</summary>
                public const string Spi4 = STM32H7.SpiBus.Spi4;
            }

            /// <summary>CAN bus definitions.</summary>
            public static class CanBus {
                /// <summary>CAN bus on PB13 (TX) and PB12 (RX).</summary>
                public const string Can2 = STM32H7.CanBus.Can2;
            }

            /// <summary>Storage controller definitions.</summary>
            public static class StorageController {
                public const string UsbHostMassStorage = STM32H7.StorageController.UsbHostMassStorage;
            }

            /// <summary>RTC controller definitions.</summary>
            public static class RtcController {
                /// <summary>API id.</summary>
                public const string Id = STM32H7.RtcController.Id;
            }
        }
    }
}
