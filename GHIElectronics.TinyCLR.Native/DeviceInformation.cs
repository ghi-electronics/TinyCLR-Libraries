using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public enum DebugInterface {        
        Disable = 0,
        Usb = 1,
        Serial = 2,
    }

    public static class DeviceInformation {
        public static string DeviceName { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        public static string ManufacturerName { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        public static ulong Version { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        public static DebugInterface DebugInterface { [MethodImpl(MethodImplOptions.InternalCall)] get;  }        
        public static uint DebugPort { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        public static void SetDebugInterface(DebugInterface debugInterface) => SetDebugInterface(debugInterface, -1);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetDebugInterface(DebugInterface debugInterface, int debugPort);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsModePinDisabled();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void AppPinDisable();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsAppPinDisabled();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetDeviceName(string name);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern byte[] GetUniqueId();
    }
}
