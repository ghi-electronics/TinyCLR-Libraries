using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices {
    public static class DeviceInformation {
        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern DeviceInformation();

        public static string DeviceName { get; }
        public static string ManufacturerName { get; }
        [CLSCompliant(false)]
        public static ulong Version { get; }
    }
}
