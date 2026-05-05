using System;
using System.Collections;

namespace GHIElectronics.TinyCLR.Native {
    // Public surface mirrors GHIElectronics.TinyCLR.Native\NativeApi.cs.
    // Bodies on Desktop are safe no-ops: factories return non-null instances
    // with default property values so callers' static initializers (e.g.
    // Memory.managed) don't NPE. No native InternalCall on Desktop.

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

        // Singleton non-null shim instance. Callers that store the result of
        // Find() and access Implementation get IntPtr.Zero — workable for
        // no-op consumers like Memory's static initializer.
        private static readonly NativeApi shimInstance = new NativeApi();

        private NativeApi() { }

        public static object GetDefaultFromCreator(NativeApiType apiType) =>
            NativeApi.defaultCreators.Contains(apiType)
                ? ((DefaultCreator)NativeApi.defaultCreators[apiType])?.Invoke()
                : null;

        public static NativeApi Find(string name, NativeApiType type) => NativeApi.shimInstance;

        public static string GetDefaultName(NativeApiType type) => string.Empty;

        public static NativeApi[] FindAll() => new NativeApi[0];

        public string Author => string.Empty;
        public string Name => string.Empty;
        public ulong Version => 0;
        public NativeApiType Type => 0;
        public IntPtr Implementation => IntPtr.Zero;
        public IntPtr State => IntPtr.Zero;
    }
}
