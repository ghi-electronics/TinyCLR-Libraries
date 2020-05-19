using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public static class Flash {
        public static void EnableExternalFlash() => NativeEnableExternalFlash();

        public static bool IsEnabledExternalFlash() => NativeIsEnabledExternalFlash();

        public const byte BLOCK_SIZE = 0x20;

        public static int OtpWrite(uint blockIndex, byte[] data) {
            if (data == null)
                throw new ArgumentNullException();

            if (data.Length != BLOCK_SIZE)
                throw new ArgumentOutOfRangeException();

            return NativeOtpWrite(blockIndex, data);
        }

        public static int OtpRead(uint blockIndex, byte[] data) {
            if (data == null)
                throw new ArgumentNullException();

            if (data.Length != BLOCK_SIZE)
                throw new ArgumentOutOfRangeException();

            return NativeOtpRead(blockIndex, data);
        }

        public static bool OtpIsBlank(uint blockIndex) {
            var data = new byte[BLOCK_SIZE];

            var read = NativeOtpRead(blockIndex, data);

            for (var i = 0; i < data.Length; i++) {
                if (data[i] != 0xFF)
                    return false;
            }

            return read == BLOCK_SIZE;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern void NativeEnableExternalFlash();

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern bool NativeIsEnabledExternalFlash();

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern int NativeOtpWrite(uint blockIndex, byte[] data);

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern int NativeOtpRead(uint blockIndex, byte[] data);


    }
}
