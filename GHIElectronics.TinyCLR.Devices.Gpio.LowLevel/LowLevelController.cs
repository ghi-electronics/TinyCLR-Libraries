using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Gpio.LowLevel {
    public enum OutputType {
        PushPull = 0,
        OpenDrain = 1,
    }
    public enum PullDirection {
        None = 0,
        PullUp = 1,
        PullDown = 2,
    }

    public enum PortMode {
        GpioInput = 0,
        GpioOutput = 1,
        AlternateFunction = 2,
        Analog = 3
    }
    public enum OutputSpeed {
        Low = 0,
        Medium = 1,
        High = 2,
        VeryHigh = 3
    }

    public enum AlternateFunction {
        AF0 = 0,
        AF1 = 1,
        AF2 = 2,
        AF3 = 3,
        AF4 = 4,
        AF5 = 5,
        AF6 = 6,
        AF7 = 7,
        AF8 = 8,
        AF9 = 9,
        AF10 = 10,
        AF11 = 11,
        AF12 = 12,
        AF13 = 13,
        AF14 = 14,
        AF15 = 15
    }

    public class Settings {
        public PortMode mode;
        public OutputType type;
        public PullDirection driveDirection;
        public OutputSpeed speed;
        public AlternateFunction alternate;
    }
    static public class LowLevelController {
        static IGpioControllerProvider provider = new GpioControllerApiWrapper(NativeApi.Find(NativeApi.GetDefaultName(NativeApiType.GpioController), NativeApiType.GpioController));
        public static void TransferFeature(int pinSource, int pinDestination, Settings settings) => TransferFeature(pinSource, pinDestination, (uint)settings.mode, (uint)settings.type, (uint)settings.driveDirection, (uint)settings.speed, (uint)settings.alternate);
        private static void TransferFeature(int pinSource, int pinDestination, uint mode, uint type, uint direction, uint speed, uint alternate) => provider.TransferFeature(pinSource, pinDestination, mode, type, direction, speed, alternate);
    }
}
