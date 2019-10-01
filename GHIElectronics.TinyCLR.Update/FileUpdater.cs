using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Update {
    public static class FileUpdater {
        public static void LoadAndFlashDeployment(FileStream stream) => FileUpdater.LoadAndFlashDeployment(stream, 0);
        public static void LoadAndFlashDeployment(FileStream stream, uint checksum) => FileUpdater.LoadAndFlashDeployment(stream, checksum, null);
        public static void LoadAndFlashDeployment(FileStream stream, uint checksum, uint[] key) {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (key != null && key.Length == 0) throw new ArgumentException("A non-null key cannot be zero-length.", nameof(key));

            FileUpdater.NativeLoadAndFlashDeployment(stream, key, checksum);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeLoadAndFlashDeployment(FileStream stream, uint[] key, uint checksum);
    }
}
