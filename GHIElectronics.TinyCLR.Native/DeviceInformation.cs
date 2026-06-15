using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    /// <summary>Transport used by the debugger to attach to the device.</summary>
    public enum DebugInterface {
        /// <summary>Debugging disabled.</summary>
        Disable = 0,
        /// <summary>USB CDC debug port.</summary>
        Usb = 1,
        /// <summary>UART debug port.</summary>
        Serial = 2,
    }

    /// <summary>
    /// Read-only metadata about the running device (name, manufacturer, firmware
    /// version) plus debug-interface and unique-ID controls.
    /// </summary>
    public static class DeviceInformation {
        /// <summary>The device's friendly name (settable via <see cref="SetDeviceName"/>).</summary>
        public static string DeviceName { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        /// <summary>The MCU family of the running device (e.g. "STM32H7", "STM32L4"). Supplied by the board/firmware layer; not user-settable.</summary>
        public static string DeviceFamily { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        /// <summary>The manufacturer reported by the firmware.</summary>
        public static string ManufacturerName { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        /// <summary>Firmware version (packed major/minor/build).</summary>
        public static ulong Version { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        /// <summary>Active debugger transport.</summary>
        public static DebugInterface DebugInterface { [MethodImpl(MethodImplOptions.InternalCall)] get;  }
        /// <summary>Hardware port the debugger is bound to (controller index).</summary>
        public static uint DebugPort { [MethodImpl(MethodImplOptions.InternalCall)] get; }

        /// <summary>Routes the debugger to a different transport without specifying a port.</summary>
        public static void SetDebugInterface(DebugInterface debugInterface) => SetDebugInterface(debugInterface, -1);
        /// <summary>Routes the debugger to a specific transport and port (e.g. UART2).</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetDebugInterface(DebugInterface debugInterface, int debugPort);

        /// <summary>True when the bootloader-mode pin has been disabled by firmware policy.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsModePinDisabled();

        /// <summary>Permanently disables the app-mode pin. Cannot be undone.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void AppPinDisable();

        /// <summary>True when the app-mode pin has been disabled by <see cref="AppPinDisable"/>.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsAppPinDisabled();

        /// <summary>Writes a new value for <see cref="DeviceName"/>. Persisted in secure storage.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetDeviceName(string name);

        /// <summary>Returns the chip's unique 96-bit identifier (12 bytes).</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern byte[] GetUniqueId();


        /// <summary>Returns a 0.0..1.0 estimate of recent CPU load.</summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public double GetCpuUsageStatistic();
    }
}
