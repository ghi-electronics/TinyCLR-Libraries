using System;

namespace GHIElectronics.TinyCLR.Native {
    public enum DebugInterface {
        Disable = 0,
        Usb = 1,
        Serial = 2,
    }

    public static class DeviceInformation {
        public static string DeviceName => "Desktop";
        public static string DeviceFamily => "STM32H7";
        public static string ManufacturerName => "GHI Electronics";
        public static ulong Version => 0;
        public static DebugInterface DebugInterface => DebugInterface.Disable;
        public static uint DebugPort => 0;

        public static void SetDebugInterface(DebugInterface debugInterface) { }
        public static void SetDebugInterface(DebugInterface debugInterface, int debugPort) { }
        public static bool IsModePinDisabled() => false;
        public static void AppPinDisable() { }
        public static bool IsAppPinDisabled() => false;
        public static void SetDeviceName(string name) { }
        public static byte[] GetUniqueId() => new byte[16];

        public static double GetCpuUsageStatistic() => 0.0;
    }
}
