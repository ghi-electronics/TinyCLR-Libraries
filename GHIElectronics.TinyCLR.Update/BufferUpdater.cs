using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Update {
    public static class BufferUpdater {
        public static void PrepareFirmware(uint[] key, uint checksum) {
            if (key == null) throw new ArgumentException("key can not be null", nameof(key));
            NativePrepareFirmware(key, checksum);
        }

        public static void PrepareDeployment(uint[] key, uint checksum) {
            if (key == null) throw new ArgumentException("key can not be null", nameof(key));
            NativePrepareDeployment(key, checksum);
        }

        public static void LoadFirmware(byte[] data, long offset, long count) {
            if (data == null)
                throw new ArgumentException("data can not be null", nameof(data));

            if (offset > count || offset + count > data.Length)
                throw new ArgumentOutOfRangeException("out of range");

            NativeLoadFirmware(data, offset, count);
        }

        public static void LoadDeployment(byte[] data, long offset, long count) {
            if (data == null)
                throw new ArgumentException("data can not be null", nameof(data));

            if (offset > count || offset + count > data.Length)
                throw new ArgumentOutOfRangeException("out of range");

            NativeLoadDeployment(data, offset, count);
        }

        public static void Flash() => NativeFlash();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativePrepareFirmware(uint[] key, uint checksum);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativePrepareDeployment(uint[] key, uint checksum);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativeLoadFirmware(byte[] data, long offset, long count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativeLoadDeployment(byte[] data, long offset, long count);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static void NativeFlash();
    }
}
