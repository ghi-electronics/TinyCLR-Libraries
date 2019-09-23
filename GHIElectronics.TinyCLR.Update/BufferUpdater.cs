using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Update {
    public class BufferUpdater {

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static void LoadFirmware(byte[] data, uint[] key, uint checksum);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static void LoadDeployment(byte[] data, uint[] key, uint checksum);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static void Flash();
    }
}
