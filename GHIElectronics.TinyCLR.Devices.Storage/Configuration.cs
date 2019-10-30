using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.Storage {
    public static class Configuration {
        public static uint Size => NativeGetSize();

        public static int Read(uint sourceOffset, byte[] destinationData, uint destinationOffset, uint count) {
            if (destinationData == null) throw new ArgumentNullException(nameof(destinationData));
            if (destinationOffset < 0) throw new ArgumentOutOfRangeException(nameof(destinationOffset));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (destinationOffset + count > Size) throw new ArgumentOutOfRangeException(nameof(count));

            return NativeRead(destinationData, destinationOffset, count);
        }
        
        public static int Write(byte[] sourceData, uint sourceOffset, uint destinationOffset, uint count) {
            if (sourceData == null) throw new ArgumentNullException(nameof(sourceData));
            if (sourceOffset < 0) throw new ArgumentOutOfRangeException(nameof(sourceOffset));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (sourceOffset + count > sourceData.Length) throw new ArgumentOutOfRangeException(nameof(sourceData));

            return NativeWrite(sourceData, sourceOffset, destinationOffset, count);
        }

        public static bool Erase(uint destinationOffset, uint count) {
            if (destinationOffset + count > Size) throw new ArgumentOutOfRangeException(nameof(count));

            return NativeErase(destinationOffset, count);
        }

        public static bool IsErased(uint destinationOffset, uint count) {
            if (destinationOffset + count > Size) throw new ArgumentOutOfRangeException(nameof(count));

            return NativeIsErased(destinationOffset, count);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int NativeRead(byte[] data, uint destinationOffset, uint count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int NativeWrite(byte[] sourceData, uint sourceOffset, uint destinationOffset, uint count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool NativeErase(uint destinationOffset, uint count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool NativeIsErased(uint destinationOffset, uint count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern uint NativeGetSize();
    }
}
