using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.BrainPad {
    /// <summary>
    /// Provides names for the expansion the pins.
    /// </summary>
    public class Pins {
        public class GpioPin {
            public const int Mosi = G30.GpioPin.PB5;
            public const int Miso = G30.GpioPin.PB4;
            public const int Sck = G30.GpioPin.PB3;
            public const int Cs = G30.GpioPin.PC3;
            public const int Rst = G30.GpioPin.PA6;
            public const int An = G30.GpioPin.PA7;
            public const int Pwm = G30.GpioPin.PA8;
            public const int Int = G30.GpioPin.PA2;
            public const int Rx = G30.GpioPin.PA10;
            public const int Tx = G30.GpioPin.PA9;
        }
        public class AdcChannel {
            public const int An = G30.AdcChannel.PA7;
            public const int Rst = G30.AdcChannel.PA6;
            public const int Cs = G30.AdcChannel.PC3;
            public const int Int = G30.AdcChannel.PA2;
        }
        public class PwmPin {
            public const string Id = G30.PwmPin.Controller1.Id;
            public const int Pwm = G30.PwmPin.Controller1.PA8;
        }
    }
}
