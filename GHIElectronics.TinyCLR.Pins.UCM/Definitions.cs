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

        }

        private static void SetUC2550() {
            UCMStandard.GpioPin.A = UC2550.GpioPin.PC4;
            UCMStandard.GpioPin.B = UC2550.GpioPin.PC5;
            UCMStandard.GpioPin.IrqA = UC2550.GpioPin.PC0;

            UCMStandard.PwmPin.A = new UCMStandard.IdPinPair(UC2550.PwmPin.Controller1.Id, UC2550.PwmPin.Controller1.PE9);

            UCMStandard.UartPort.A = UC2550.UartPort.Usart1;
            UCMStandard.UartPort.HandshakingA = UC2550.UartPort.Usart2;
        }

        private static void SetUC5550() {

        }
    }
}
