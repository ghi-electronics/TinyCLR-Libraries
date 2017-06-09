using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices {
    //Keep in sync with native
    [CLSCompliant(false)]
    public enum ApiType : uint {
        ApiProvider = 0,
        InteropProvider = 1,
        MemoryProvider = 2,
        TaskProvider = 3,
        DeploymentProvider = 4,
        ErrorHandlerProvider = 5,
        InterruptProvider = 6,
        PowerProvider = 7,
        TimeProvider = 8,
        AdcProvider = 9,
        CanProvider = 10,
        DacProvider = 11,
        DisplayProvider = 12,
        GpioProvider = 13,
        I2cProvider = 14,
        PwmProvider = 15,
        SpiProvider = 16,
        UartProvider = 17,
        UsbClientProvider = 18,
        UsbHostProvider = 19,
        Custom = 0x80000000,
    }

    public sealed class Api {
        private Api() { }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Add(IntPtr address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Remove(IntPtr address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern Api Find(string name, ApiType type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string GetDefaultSelector(ApiType type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetDefaultSelector(ApiType type, string selector);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern Api[] FindAll();

        public static bool ParseSelector(string selector, out string providerId) => Api.ParseSelector(selector, out providerId, out _);

        [CLSCompliant(false)]
        public static bool ParseSelector(string selector, out string providerId, out uint controllerIndex) {
            providerId = null;
            controllerIndex = 0;

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

        public static string CreateSelector(string providerId) => providerId ?? throw new ArgumentNullException(nameof(providerId));

        [CLSCompliant(false)]
        public static string CreateSelector(string providerId, uint controllerIndex) => $"{Api.CreateSelector(providerId)}\\{controllerIndex}";

        public string Author { get; }
        public string Name { get; }
        [CLSCompliant(false)]
        public ulong Version { get; }
        [CLSCompliant(false)]
        public ApiType Type { get; }
        [CLSCompliant(false)]
        public uint Count { get; }
        public IntPtr[] Implementation { get; }
    }
}
