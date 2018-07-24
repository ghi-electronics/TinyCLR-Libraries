using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    //Keep in sync with native
    public enum ApiType : uint {
        ApiManager = 0,
        DebuggerManager = 1,
        InteropManager = 2,
        MemoryManager = 3,
        TaskManager = 4,
        SystemTimeManager = 5,
        DeploymentController = 0 | 0x20000000,
        InterruptController = 1 | 0x20000000,
        PowerController = 2 | 0x20000000,
        NativeTimeController = 3 | 0x20000000,
        AdcController = 0 | 0x40000000,
        CanController = 1 | 0x40000000,
        DacController = 2 | 0x40000000,
        DcmiController = 3 | 0x40000000,
        DisplayController = 4 | 0x40000000,
        EthernetMacController = 5 | 0x40000000,
        GpioController = 6 | 0x40000000,
        I2cController = 7 | 0x40000000,
        I2sController = 8 | 0x40000000,
        PwmController = 9 | 0x40000000,
        RtcController = 10 | 0x40000000,
        SaiController = 11 | 0x40000000,
        SdCardController = 12 | 0x40000000,
        SpiController = 13 | 0x40000000,
        UartController = 14 | 0x40000000,
        UsbClientController = 15 | 0x40000000,
        UsbHostController = 16 | 0x40000000,
        WatchdogController = 17 | 0x40000000,
        Custom = 0 | 0x80000000,
    }

    public interface IApiImplementation {
        IntPtr Implementation { get; }
    }

    public sealed class Api {
        public delegate object DefaultCreator();

        private static readonly Hashtable defaultCreators = new Hashtable();

        private Api() { }

        public static object GetDefaultFromCreator(ApiType apiType) => Api.defaultCreators.Contains(apiType) ? ((DefaultCreator)Api.defaultCreators[apiType])?.Invoke() : null;
        public static void SetDefaultCreator(ApiType apiType, DefaultCreator creator) => Api.defaultCreators[apiType] = creator;

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Add(IntPtr address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Remove(IntPtr address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern Api Find(string name, ApiType type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string GetDefaultName(ApiType type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetDefaultName(ApiType type, string selector);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern Api[] FindAll();

        public string Author { get; }
        public string Name { get; }
        public ulong Version { get; }
        public ApiType Type { get; }
        public IntPtr Implementation { get; }
        public IntPtr State { get; }
    }
}
