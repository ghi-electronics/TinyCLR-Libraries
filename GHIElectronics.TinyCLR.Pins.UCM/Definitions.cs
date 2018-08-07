using System;

namespace GHIElectronics.TinyCLR.Pins {
    internal static class Definitions {
        public static void SetModel(UCMModel model) {
            switch (model) {
                case UCMModel.G400D: Definitions.SetG400D(); break;
                case UCMModel.UC2550: Definitions.SetUC2550(); break;
                case UCMModel.UC5550: Definitions.SetUC5550(); break;
                default: throw new ArgumentException("Invalid model type.", nameof(model));
            }
        }

        private static void SetG400D() {
            UCMStandard.GpioPin.Id = G400D.GpioPin.Id;

            UCMStandard.GpioPin.A = G400D.GpioPin.PD14;
            UCMStandard.GpioPin.B = G400D.GpioPin.PD13;
            UCMStandard.GpioPin.C = G400D.GpioPin.PD12;
            UCMStandard.GpioPin.D = G400D.GpioPin.PD11;
            UCMStandard.GpioPin.E = G400D.GpioPin.PD10;
            UCMStandard.GpioPin.F = G400D.GpioPin.PD9;
            UCMStandard.GpioPin.G = G400D.GpioPin.PD8;
            UCMStandard.GpioPin.H = G400D.GpioPin.PD2;
            UCMStandard.GpioPin.I = G400D.GpioPin.PC24;
            UCMStandard.GpioPin.J = G400D.GpioPin.PD7;
            UCMStandard.GpioPin.K = G400D.GpioPin.PC26;
            UCMStandard.GpioPin.L = G400D.GpioPin.PA26;

            UCMStandard.GpioPin.IrqA = G400D.GpioPin.PD18;
            UCMStandard.GpioPin.IrqB = G400D.GpioPin.PD17;
            UCMStandard.GpioPin.IrqC = G400D.GpioPin.PD16;
            UCMStandard.GpioPin.IrqD = G400D.GpioPin.PD15;

            UCMStandard.AdcChannel.Id = G400D.AdcChannel.Id;

            UCMStandard.AdcChannel.A = G400D.AdcChannel.PB8;
            UCMStandard.AdcChannel.B = G400D.AdcChannel.PB11;
            UCMStandard.AdcChannel.C = G400D.AdcChannel.PB12;
            UCMStandard.AdcChannel.D = G400D.AdcChannel.PB17;
            UCMStandard.AdcChannel.E = G400D.AdcChannel.PB16;
            UCMStandard.AdcChannel.F = G400D.AdcChannel.PB13;
            UCMStandard.AdcChannel.G = G400D.AdcChannel.PB14;
            UCMStandard.AdcChannel.H = G400D.AdcChannel.PB15;

            UCMStandard.PwmChannel.A = new UCMStandard.IdPinPair(G400D.PwmChannel.Controller0.Id, G400D.PwmChannel.Controller0.PC18);
            UCMStandard.PwmChannel.B = new UCMStandard.IdPinPair(G400D.PwmChannel.Controller1.Id, G400D.PwmChannel.Controller1.PC19);
            UCMStandard.PwmChannel.C = new UCMStandard.IdPinPair(G400D.PwmChannel.Controller3.Id, G400D.PwmChannel.Controller3.PC21);
            UCMStandard.PwmChannel.D = new UCMStandard.IdPinPair(G400D.PwmChannel.Controller2.Id, G400D.PwmChannel.Controller2.PC20);

            UCMStandard.UartPort.A = G400D.UartPort.Debug;

            UCMStandard.UartPort.HandshakingA = G400D.UartPort.Usart0;

            UCMStandard.I2cBus.A = G400D.I2cBus.Twi0;

            UCMStandard.SpiBus.A = G400D.SpiBus.Spi1;

            UCMStandard.CanBus.A = G400D.CanBus.Can1;

            UCMStandard.UsbClientPort.A = G400D.UsbClientPort.Udphs;

            UCMStandard.UsbHostPort.A = G400D.UsbHostPort.UhphsB;

            UCMStandard.Display.A = G400D.Display.Lcd;
        }

        private static void SetUC2550() {
            UCMStandard.GpioPin.Id = UC2550.GpioPin.Id;

            UCMStandard.GpioPin.A = UC2550.GpioPin.PC4;
            UCMStandard.GpioPin.B = UC2550.GpioPin.PC5;
            UCMStandard.GpioPin.C = UC2550.GpioPin.PA15;
            UCMStandard.GpioPin.D = UC2550.GpioPin.PB0;
            UCMStandard.GpioPin.E = UC2550.GpioPin.PB7;
            UCMStandard.GpioPin.F = UC2550.GpioPin.PD7;
            UCMStandard.GpioPin.G = UC2550.GpioPin.PD10;
            UCMStandard.GpioPin.H = UC2550.GpioPin.PE10;
            UCMStandard.GpioPin.I = UC2550.GpioPin.PD14;
            UCMStandard.GpioPin.J = UC2550.GpioPin.PD15;

            UCMStandard.GpioPin.IrqA = UC2550.GpioPin.PC0;
            UCMStandard.GpioPin.IrqB = UC2550.GpioPin.PC1;
            UCMStandard.GpioPin.IrqC = UC2550.GpioPin.PC2;
            UCMStandard.GpioPin.IrqD = UC2550.GpioPin.PC3;

            UCMStandard.AdcChannel.Id = UC2550.AdcChannel.Id;

            UCMStandard.AdcChannel.A = UC2550.AdcChannel.PA0;
            UCMStandard.AdcChannel.B = UC2550.AdcChannel.PA1;
            UCMStandard.AdcChannel.C = UC2550.AdcChannel.PA2;
            UCMStandard.AdcChannel.D = UC2550.AdcChannel.PA3;
            UCMStandard.AdcChannel.E = UC2550.AdcChannel.PA4;
            UCMStandard.AdcChannel.F = UC2550.AdcChannel.PA5;
            UCMStandard.AdcChannel.G = UC2550.AdcChannel.PA6;
            UCMStandard.AdcChannel.H = UC2550.AdcChannel.PA7;

            UCMStandard.PwmChannel.A = new UCMStandard.IdPinPair(UC2550.PwmChannel.Controller1.Id, UC2550.PwmChannel.Controller1.PE9);
            UCMStandard.PwmChannel.B = new UCMStandard.IdPinPair(UC2550.PwmChannel.Controller1.Id, UC2550.PwmChannel.Controller1.PE11);
            UCMStandard.PwmChannel.C = new UCMStandard.IdPinPair(UC2550.PwmChannel.Controller3.Id, UC2550.PwmChannel.Controller3.PC6);
            UCMStandard.PwmChannel.D = new UCMStandard.IdPinPair(UC2550.PwmChannel.Controller3.Id, UC2550.PwmChannel.Controller3.PC7);
            UCMStandard.PwmChannel.E = new UCMStandard.IdPinPair(UC2550.PwmChannel.Controller9.Id, UC2550.PwmChannel.Controller9.PE5);
            UCMStandard.PwmChannel.F = new UCMStandard.IdPinPair(UC2550.PwmChannel.Controller9.Id, UC2550.PwmChannel.Controller9.PE6);
            UCMStandard.PwmChannel.G = new UCMStandard.IdPinPair(UC2550.PwmChannel.Controller12.Id, UC2550.PwmChannel.Controller12.PB14);
            UCMStandard.PwmChannel.H = new UCMStandard.IdPinPair(UC2550.PwmChannel.Controller12.Id, UC2550.PwmChannel.Controller12.PB15);

            UCMStandard.UartPort.A = UC2550.UartPort.Usart1;
            UCMStandard.UartPort.B = UC2550.UartPort.Uart7;
            UCMStandard.UartPort.C = UC2550.UartPort.Uart8;
            UCMStandard.UartPort.D = UC2550.UartPort.Uart9;

            UCMStandard.UartPort.HandshakingA = UC2550.UartPort.Usart2;
            UCMStandard.UartPort.HandshakingB = UC2550.UartPort.Usart3;

            UCMStandard.I2cBus.A = UC2550.I2cBus.I2c2;
            UCMStandard.I2cBus.B = UC2550.I2cBus.I2c3;

            UCMStandard.SpiBus.A = UC2550.SpiBus.Spi1;
            UCMStandard.SpiBus.B = UC2550.SpiBus.Spi5;

            UCMStandard.CanBus.A = UC2550.CanBus.Can1;
            UCMStandard.CanBus.B = UC2550.CanBus.Can2;

            UCMStandard.UsbClientPort.A = UC2550.UsbClientPort.UsbOtg;
        }

        private static void SetUC5550() {
            UCMStandard.GpioPin.Id = UC5550.GpioPin.Id;

            UCMStandard.GpioPin.A = UC5550.GpioPin.PD7;
            UCMStandard.GpioPin.B = UC5550.GpioPin.PE3;
            UCMStandard.GpioPin.C = UC5550.GpioPin.PG3;
            UCMStandard.GpioPin.D = UC5550.GpioPin.PG6;
            UCMStandard.GpioPin.E = UC5550.GpioPin.PG7;
            UCMStandard.GpioPin.F = UC5550.GpioPin.PH4;
            UCMStandard.GpioPin.G = UC5550.GpioPin.PI0;
            UCMStandard.GpioPin.H = UC5550.GpioPin.PA1;
            UCMStandard.GpioPin.I = UC5550.GpioPin.PA2;
            UCMStandard.GpioPin.J = UC5550.GpioPin.PA7;
            UCMStandard.GpioPin.K = UC5550.GpioPin.PC4;
            UCMStandard.GpioPin.L = UC5550.GpioPin.PC5;

            UCMStandard.GpioPin.IrqA = UC5550.GpioPin.PI8;
            UCMStandard.GpioPin.IrqB = UC5550.GpioPin.PI11;
            UCMStandard.GpioPin.IrqC = UC5550.GpioPin.PH14;
            UCMStandard.GpioPin.IrqD = UC5550.GpioPin.PH15;

            UCMStandard.AdcChannel.Id = UC5550.AdcChannel.Id;

            UCMStandard.AdcChannel.A = UC5550.AdcChannel.PA0;
            UCMStandard.AdcChannel.B = UC5550.AdcChannel.PA4;
            UCMStandard.AdcChannel.C = UC5550.AdcChannel.PA5;
            UCMStandard.AdcChannel.D = UC5550.AdcChannel.PB0;
            UCMStandard.AdcChannel.E = UC5550.AdcChannel.PB1;
            UCMStandard.AdcChannel.F = UC5550.AdcChannel.PC0;
            UCMStandard.AdcChannel.G = UC5550.AdcChannel.PC2;
            UCMStandard.AdcChannel.H = UC5550.AdcChannel.PC3;

            UCMStandard.PwmChannel.A = new UCMStandard.IdPinPair(UC5550.PwmChannel.Controller2.Id, UC5550.PwmChannel.Controller2.PA15);
            UCMStandard.PwmChannel.B = new UCMStandard.IdPinPair(UC5550.PwmChannel.Controller4.Id, UC5550.PwmChannel.Controller4.PB7);
            UCMStandard.PwmChannel.C = new UCMStandard.IdPinPair(UC5550.PwmChannel.Controller5.Id, UC5550.PwmChannel.Controller5.PH11);
            UCMStandard.PwmChannel.D = new UCMStandard.IdPinPair(UC5550.PwmChannel.Controller8.Id, UC5550.PwmChannel.Controller8.PI5);
            UCMStandard.PwmChannel.E = new UCMStandard.IdPinPair(UC5550.PwmChannel.Controller8.Id, UC5550.PwmChannel.Controller8.PI6);
            UCMStandard.PwmChannel.F = new UCMStandard.IdPinPair(UC5550.PwmChannel.Controller8.Id, UC5550.PwmChannel.Controller8.PI7);
            UCMStandard.PwmChannel.G = new UCMStandard.IdPinPair(UC5550.PwmChannel.Controller8.Id, UC5550.PwmChannel.Controller8.PI2);
            UCMStandard.PwmChannel.H = new UCMStandard.IdPinPair(UC5550.PwmChannel.Controller2.Id, UC5550.PwmChannel.Controller9.PA3);

            UCMStandard.UartPort.A = UC5550.UartPort.Usart1;
            UCMStandard.UartPort.B = UC5550.UartPort.Usart6;
            UCMStandard.UartPort.C = UC5550.UartPort.Uart7;
            UCMStandard.UartPort.D = UC5550.UartPort.Usart3;

            UCMStandard.UartPort.HandshakingA = UC5550.UartPort.Usart2;

            UCMStandard.I2cBus.A = UC5550.I2cBus.I2c1;

            UCMStandard.SpiBus.A = UC5550.SpiBus.Spi1;
            UCMStandard.SpiBus.B = UC5550.SpiBus.Spi5;

            UCMStandard.CanBus.A = UC5550.CanBus.Can1;
            UCMStandard.CanBus.B = UC5550.CanBus.Can2;

            UCMStandard.UsbClientPort.A = UC5550.UsbClientPort.UsbOtg;

            UCMStandard.UsbHostPort.A = UC5550.UsbHostPort.UsbOtg;

            UCMStandard.Display.A = UC5550.Display.Lcd;
        }
    }
}
