using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    /// <summary>
    /// Identifies a category of native API. Each managed library (Gpio, Spi, …)
    /// uses its own value to look up a matching native implementation via
    /// <see cref="NativeApi.Find(string, NativeApiType)"/>.
    /// </summary>
    //Keep in sync with native
    public enum NativeApiType : uint {
        /// <summary>API manager itself.</summary>
        ApiManager = 0,
        /// <summary>Debugger transport manager.</summary>
        DebuggerManager = 1,
        /// <summary>Interop / extern-call manager.</summary>
        InteropManager = 2,
        /// <summary>Managed heap / allocator manager.</summary>
        MemoryManager = 3,
        /// <summary>System-time manager (DateTime, ticks).</summary>
        TimeManager = 4,
        /// <summary>ADC controller.</summary>
        AdcController = 0 | 0x40000000,
        /// <summary>CAN controller.</summary>
        CanController = 1 | 0x40000000,
        /// <summary>DAC controller.</summary>
        DacController = 2 | 0x40000000,
        /// <summary>DCMI / parallel camera controller.</summary>
        DcmiController = 3 | 0x40000000,
        /// <summary>Display controller.</summary>
        DisplayController = 4 | 0x40000000,
        /// <summary>GPIO controller.</summary>
        GpioController = 5 | 0x40000000,
        /// <summary>I²C controller.</summary>
        I2cController = 6 | 0x40000000,
        /// <summary>I²S audio controller.</summary>
        I2sController = 7 | 0x40000000,
        /// <summary>Network controller.</summary>
        NetworkController = 8 | 0x40000000,
        /// <summary>1-Wire controller.</summary>
        OneWireController = 9 | 0x40000000,
        /// <summary>Power-management controller.</summary>
        PowerController = 10 | 0x40000000,
        /// <summary>PWM controller.</summary>
        PwmController = 11 | 0x40000000,
        /// <summary>RTC controller.</summary>
        RtcController = 12 | 0x40000000,
        /// <summary>SAI audio controller.</summary>
        SaiController = 13 | 0x40000000,
        /// <summary>SPI controller.</summary>
        SpiController = 14 | 0x40000000,
        /// <summary>Block-storage controller (internal flash, SD, etc.).</summary>
        StorageController = 15 | 0x40000000,
        /// <summary>Task / RTOS controller.</summary>
        TaskController = 16 | 0x40000000,
        /// <summary>Touch controller.</summary>
        TouchController = 17 | 0x40000000,
        /// <summary>UART controller.</summary>
        UartController = 18 | 0x40000000,
        /// <summary>USB device controller.</summary>
        UsbClientController = 19 | 0x40000000,
        /// <summary>USB host controller.</summary>
        UsbHostController = 20 | 0x40000000,
        /// <summary>Watchdog timer.</summary>
        WatchdogController = 21 | 0x40000000,
        /// <summary>Custom / out-of-tree API.</summary>
        Custom = 0 | 0x80000000,
    }

    /// <summary>Implemented by managed wrappers that expose their underlying native handle.</summary>
    public interface IApiImplementation {
        /// <summary>Pointer to the native implementation struct.</summary>
        IntPtr Implementation { get; }
    }

    /// <summary>
    /// Handle to a native API surfaced by the firmware. Use <see cref="Find"/>
    /// to obtain a specific API by name, or <see cref="FindAll"/> to enumerate
    /// everything the firmware exposes.
    /// </summary>
    public sealed class NativeApi {
        /// <summary>Factory used when a managed library asks for the "default" API of a given type.</summary>
        public delegate object DefaultCreator();

        private static readonly Hashtable defaultCreators = new Hashtable();

        private NativeApi() { }

        /// <summary>Invokes a registered default-creator for the given API type, if any.</summary>
        public static object GetDefaultFromCreator(NativeApiType apiType) => NativeApi.defaultCreators.Contains(apiType) ? ((DefaultCreator)NativeApi.defaultCreators[apiType])?.Invoke() : null;
        //public static void SetDefaultCreator(NativeApiType apiType, DefaultCreator creator) => NativeApi.defaultCreators[apiType] = creator;

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static extern void Add(IntPtr address);

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static extern void Remove(IntPtr address);

        /// <summary>Locates a native API by name and type.</summary>
        /// <param name="name">Native API name (e.g. one of the platform-specific constants).</param>
        /// <param name="type">Category of API to find.</param>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern NativeApi Find(string name, NativeApiType type);

        /// <summary>Returns the name of the default API for a given type.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string GetDefaultName(NativeApiType type);

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //public static extern void SetDefaultName(NativeApiType type, string selector);

        /// <summary>Returns every native API the firmware exposes.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern NativeApi[] FindAll();

        /// <summary>The author / vendor string the firmware attached to this API.</summary>
        public string Author { get; }
        /// <summary>The native API name.</summary>
        public string Name { get; }
        /// <summary>Version number of the native API (packed major/minor/build).</summary>
        public ulong Version { get; }
        /// <summary>Category of native API.</summary>
        public NativeApiType Type { get; }
        /// <summary>Pointer to the native implementation struct.</summary>
        public IntPtr Implementation { get; }
        /// <summary>Pointer to per-API state.</summary>
        public IntPtr State { get; }
    }
}
