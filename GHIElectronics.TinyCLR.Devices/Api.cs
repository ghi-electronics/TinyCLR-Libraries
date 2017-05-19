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

        public static bool ParseIdAndIndex(string id, out string providerId, out uint controllerIndex) {
            providerId = null;
            controllerIndex = uint.MaxValue;

            if (id == null) return false;

            var parts = id.Split('\\');

            if (parts.Length != 2) return false;

            var res = uint.TryParse(parts[1], out controllerIndex);

            if (res)
                providerId = parts[0];

            return res;
        }
    }
}
