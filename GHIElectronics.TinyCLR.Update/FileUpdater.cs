using System.IO;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Update {
    public class FileUpdater {

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public static void FlashDeployment(FileStream stream, uint[] key, uint checksum);
    }
}
