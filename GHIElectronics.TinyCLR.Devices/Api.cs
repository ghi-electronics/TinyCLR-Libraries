using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Runtime {
    //Keep in sync with native
    public enum ApiType {
        Custom = 0,
        AdcProvider = 1,
        CanProvider = 2,
        DacProvider = 3,
        DisplayProvider = 4,
        GpioProvider = 5,
        I2cProvider = 6,
        PwmProvider = 7,
        SpiProvider = 8,
        UartProvider = 9,
        UsbClientProvider = 10,
        UsbHostProvider = 11,
    }

    public class Api {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string GetDefaultName(ApiType type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetDefaultName(ApiType type, string name);
    }
}
