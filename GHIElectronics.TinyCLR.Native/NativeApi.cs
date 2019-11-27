using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    //Keep in sync with native
    public enum NativeApiType : uint {
        ApiManager = 0,
        DebuggerManager = 1,
        InteropManager = 2,
        MemoryManager = 3,
        TimeManager = 4,
        AdcController = 0 | 0x40000000,
        CanController = 1 | 0x40000000,
        DacController = 2 | 0x40000000,
        DcmiController = 3 | 0x40000000,
        DisplayController = 4 | 0x40000000,
        GpioController = 5 | 0x40000000,
        I2cController = 6 | 0x40000000,
        I2sController = 7 | 0x40000000,
        NetworkController = 8 | 0x40000000,
        OneWireController = 9 | 0x40000000,
        PowerController = 10 | 0x40000000,
        PwmController = 11 | 0x40000000,
        RtcController = 12 | 0x40000000,
        SaiController = 13 | 0x40000000,
        SpiController = 14 | 0x40000000,
        StorageController = 15 | 0x40000000,
        TaskController = 16 | 0x40000000,
        TouchController = 17 | 0x40000000,
        UartController = 18 | 0x40000000,
        UsbClientController = 19 | 0x40000000,
        UsbHostController = 20 | 0x40000000,
        WatchdogController = 21 | 0x40000000,
        Custom = 0 | 0x80000000,
    }

    public interface IApiImplementation {
        IntPtr Implementation { get; }
    }

    public sealed class NativeApi {
        public delegate object DefaultCreator();

        private static readonly Hashtable defaultCreators = new Hashtable();

        private NativeApi() { }

        public static object GetDefaultFromCreator(NativeApiType apiType) => NativeApi.defaultCreators.Contains(apiType) ? ((DefaultCreator)NativeApi.defaultCreators[apiType])?.Invoke() : null;
        //public static void SetDefaultCreator(NativeApiType apiType, DefaultCreator creator) => NativeApi.defaultCreators[apiType] = creator;

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static extern void Add(IntPtr address);

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static extern void Remove(IntPtr address);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern NativeApi Find(string name, NativeApiType type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string GetDefaultName(NativeApiType type);

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static extern void SetDefaultName(NativeApiType type, string selector);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern NativeApi[] FindAll();

        public string Author { get; }
        public string Name { get; }
        public ulong Version { get; }
        public NativeApiType Type { get; }
        public IntPtr Implementation { get; }
        public IntPtr State { get; }
    }
}
