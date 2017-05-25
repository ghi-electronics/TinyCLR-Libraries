using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices {
    //Keep in sync with native
    public enum ApiType {
        Custom = 0,
        DeploymentProvider = 1,
        InterruptProvider = 2,
        PowerProvider = 3,
        TimeProvider = 4,
        AdcProvider = 5,
        CanProvider = 6,
        DacProvider = 7,
        DisplayProvider = 8,
        GpioProvider = 9,
        I2cProvider = 10,
        PwmProvider = 11,
        SpiProvider = 12,
        UartProvider = 13,
        UsbClientProvider = 14,
        UsbHostProvider = 15,
    }

    public sealed class Api {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Add(IntPtr address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Remove(IntPtr address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern IntPtr Find(string name, ApiType type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string GetDefaultSelector(ApiType type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetDefaultSelector(ApiType type, string selector);

        [CLSCompliant(false)]
        public static bool ParseSelector(string selector, out string providerId, out uint controllerIndex) {
            providerId = null;
            controllerIndex = uint.MaxValue;

            if (selector == null) return false;

            var parts = selector.Split('\\');

            if (parts.Length < 1 || parts.Length > 2) return false;

            var res = true;

            if (parts.Length == 2)
                res = uint.TryParse(parts[1], out controllerIndex);

            if (res)
                providerId = parts[0];

            return res;
        }
    }
}
