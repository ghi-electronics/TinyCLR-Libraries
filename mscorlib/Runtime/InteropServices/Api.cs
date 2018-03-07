using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices {
    //Keep in sync with native
    [CLSCompliant(false)]
    public enum ApiType : uint {
        ApiProvider = 0,
        DebuggerProvider = 1,
        InteropProvider = 2,
        MemoryProvider = 3,
        TaskProvider = 4,
        DeploymentProvider = 0 | 0x20000000,
        InterruptProvider = 1 | 0x20000000,
        PowerProvider = 2 | 0x20000000,
        TimeProvider = 3 | 0x20000000,
        AdcProvider = 0 | 0x40000000,
        CanProvider = 1 | 0x40000000,
        DacProvider = 2 | 0x40000000,
        DisplayProvider = 3 | 0x40000000,
        GpioProvider = 4 | 0x40000000,
        I2cProvider = 5 | 0x40000000,
        PwmProvider = 6 | 0x40000000,
        RtcProvider = 7 | 0x40000000,
        SpiProvider = 8 | 0x40000000,
        UartProvider = 9 | 0x40000000,
        UsbClientProvider = 10 | 0x40000000,
        UsbHostProvider = 11 | 0x40000000,
        Custom = 0 | 0x80000000,
    }

    public sealed class Api {
        private Api() { }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Add(IntPtr address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Remove(IntPtr address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [CLSCompliant(false)]
        public static extern Api Find(string name, ApiType type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [CLSCompliant(false)]
        public static extern string GetDefaultSelector(ApiType type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [CLSCompliant(false)]
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
        public IntPtr State { get; }
    }
}
