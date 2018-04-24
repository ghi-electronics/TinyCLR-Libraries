namespace GHIElectronics.TinyCLR.Pins {
    public static class UCMStandard {
        public static void SetModel(UCMModel model) => Definitions.SetModel(model);

        public class IdPinPair {
            public string Id { get; }
            public int Pin { get; }

            public IdPinPair(string id, int pin) {
                this.Id = id;
                this.Pin = pin;
            }
        }

        public static class GpioPin {
            public static string Id { get; internal set; }

            public static int A { get; internal set; }
            public static int B { get; internal set; }
            public static int C { get; internal set; }
            public static int D { get; internal set; }
            public static int E { get; internal set; }
            public static int F { get; internal set; }
            public static int G { get; internal set; }
            public static int H { get; internal set; }
            public static int I { get; internal set; }
            public static int J { get; internal set; }
            public static int K { get; internal set; }
            public static int L { get; internal set; }

            public static int IrqA { get; internal set; }
            public static int IrqB { get; internal set; }
            public static int IrqC { get; internal set; }
            public static int IrqD { get; internal set; }
        }

        public static class AdcChannel {
            public static string Id { get; internal set; }

            public static int A { get; internal set; }
            public static int B { get; internal set; }
            public static int C { get; internal set; }
            public static int D { get; internal set; }
            public static int E { get; internal set; }
            public static int F { get; internal set; }
            public static int G { get; internal set; }
            public static int H { get; internal set; }
        }

        public static class PwmPin {
            public static IdPinPair A { get; internal set; }
            public static IdPinPair B { get; internal set; }
            public static IdPinPair C { get; internal set; }
            public static IdPinPair D { get; internal set; }
            public static IdPinPair E { get; internal set; }
            public static IdPinPair F { get; internal set; }
            public static IdPinPair G { get; internal set; }
            public static IdPinPair H { get; internal set; }
        }

        public static class UartPort {
            public static string A { get; internal set; }
            public static string B { get; internal set; }

            public static string HandshakingA { get; internal set; }
            public static string HandshakingB { get; internal set; }
        }

        public static class I2cBus {
            public static string A { get; internal set; }
            public static string B { get; internal set; }
        }

        public static class SpiBus {
            public static string A { get; internal set; }
            public static string B { get; internal set; }
        }

        public static class CanBus {
            public static string A { get; internal set; }
            public static string B { get; internal set; }
        }

        public static class UsbClientPort {
            public static string A { get; internal set; }
        }

        public static class UsbHostPort {
            public static string A { get; internal set; }
        }

        public static class Display {
            public static string A { get; internal set; }
        }
    }
}
