using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.BrainPad {
    internal class Board {
        private static bool typeSet;
        private static BoardType boardType;

        public static BoardType BoardType {
            get {
                if (!Board.typeSet) {
                    using (var detectPin = GpioController.GetDefault().OpenPin(G30.GpioPin.PC1)) {
                        detectPin.SetDriveMode(GpioPinDriveMode.InputPullDown);

                        Board.boardType = detectPin.Read() == GpioPinValue.High ? BoardType.BP1 : BoardType.Original;
                    }

                    Board.typeSet = true;
                }

                return Board.boardType;
            }
        }
    }

    internal enum BoardType {
        Original,
        BP1
    }
}
