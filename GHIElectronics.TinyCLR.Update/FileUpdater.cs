using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Update {
    public static class FileUpdater {
        public static void PrepareDeployment(uint[] key, uint checksum) {
            if (key == null) throw new ArgumentException("key can not be null", nameof(key));

            NativePrepareDeployment(key, checksum);
        }

        public static void Flash(FileStream stream, long offset, long count) {
            if (stream == null) throw new ArgumentException("stream can not be null", nameof(stream));

            if (offset + count > stream.Length)
                throw new ArgumentOutOfRangeException("out of range");

            NativeFlash(stream, offset, count);
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativePrepareDeployment(uint[] key, uint checksum);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativeFlash(FileStream stream, long offset, long count);
    }
}
