using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public static class DeviceInformation {
        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern DeviceInformation();

        public static string DeviceName { get; }
        public static string ManufacturerName { get; }
        public static ulong Version { get; }
    }
}
