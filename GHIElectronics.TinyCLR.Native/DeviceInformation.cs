using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public enum DebugInterface {
        Usb = 0,
        Serial = 1,
        Disable = 255,
    }

    public static class DeviceInformation {
        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern DeviceInformation();

        public static string DeviceName { get; }
        public static string ManufacturerName { get; }
        public static ulong Version { get; }
        public static DebugInterface DebugInterface { get; }
    }
}
