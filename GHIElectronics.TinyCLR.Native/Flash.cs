using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public static class Flash {
        public static void EnableExternalFlash() => NativeEnableExternalFlash();


        public static bool IsEnabledExternalFlash() => NativeIsEnabledExternalFlash();

        public static int OtpWrite(byte[] data, uint sourceIndex, uint destinationIndex, uint length) {
            if (data == null)
                throw new ArgumentNullException();

            if (sourceIndex + length > data.Length)
                throw new ArgumentOutOfRangeException();

            return NativeOtpWrite(data, sourceIndex, destinationIndex, length);
        }

        public static int OtpRead(byte[] data, uint sourceIndex, uint destinationIndex, uint length) {
            if (data == null)
                throw new ArgumentNullException();

            if (sourceIndex + length > data.Length)
                throw new ArgumentOutOfRangeException();

            return NativeOtpRead(data, sourceIndex, destinationIndex, length);
        }


        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern void NativeEnableExternalFlash();

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern bool NativeIsEnabledExternalFlash();

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern int NativeOtpWrite(byte[] data, uint sourceIndex, uint destinationIndex, uint length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern int NativeOtpRead(byte[] data, uint sourceIndex, uint destinationIndex, uint length);


    }
}
