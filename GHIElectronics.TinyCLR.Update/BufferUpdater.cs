using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Update {
    public static class BufferUpdater {
        public static void InitializeFirmwareUpdate(uint checksum) => BufferUpdater.InitializeFirmwareUpdate(checksum, null);
        public static void InitializeFirmwareUpdate(uint[] key) => BufferUpdater.InitializeFirmwareUpdate(0, key);
        public static void InitializeFirmwareUpdate(uint checksum, uint[] key) {
            if (key != null && key.Length == 0) throw new ArgumentException("A non-null key cannot be zero-length.", nameof(key));

            BufferUpdater.NativeInitializeFirmwareUpdate(checksum, key);
        }

        public static void InitializeDeploymentUpdate(uint checksum) => BufferUpdater.InitializeDeploymentUpdate(checksum, null);
        public static void InitializeDeploymentUpdate(uint[] key) => BufferUpdater.InitializeDeploymentUpdate(0, key);
        public static void InitializeDeploymentUpdate(uint checksum, uint[] key) {
            if (key != null && key.Length == 0) throw new ArgumentException("A non-null key cannot be zero-length.", nameof(key));

            BufferUpdater.NativeInitializeDeploymentUpdate(checksum, key);
        }

        public static void LoadFirmware(byte[] data) => BufferUpdater.LoadFirmware(data, 0, data != null ? data.Length : 0);
        public static void LoadFirmware(byte[] data, int offset, int count) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(data));

            BufferUpdater.NativeLoadFirmware(data, offset, count);
        }

        public static void LoadDeployment(byte[] data) => BufferUpdater.LoadDeployment(data, 0, data != null ? data.Length : 0);
        public static void LoadDeployment(byte[] data, int offset, int count) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (offset + count > data.Length) throw new ArgumentOutOfRangeException(nameof(data));

            BufferUpdater.NativeLoadDeployment(data, offset, count);
        }

        public static void UpdateAndReset() => BufferUpdater.NativeUpdateAndReset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeInitializeFirmwareUpdate(uint checksum, uint[] key);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeInitializeDeploymentUpdate(uint checksum, uint[] key);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeLoadFirmware(byte[] data, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeLoadDeployment(byte[] data, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void NativeUpdateAndReset();
    }
}
