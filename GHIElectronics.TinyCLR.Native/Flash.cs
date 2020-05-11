using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public static class Flash {
        public static void EnableExternalFlash() => NativeEnableExternalFlash();


        public static bool IsEnabledExternalFlash() => NativeIsEnabledExternalFlash();

        public static int OTP_Write(byte[] data, int sourceIndex, int destinationIndex, int length) {
            if (data == null)
                throw new ArgumentNullException();

            if (sourceIndex + length > data.Length)
                throw new ArgumentOutOfRangeException();

            return NativeOTP_Write(data, sourceIndex, destinationIndex, length);
        }

        public static int OTP_Read(byte[] data, int sourceIndex, int destinationIndex, int length) {
            if (data == null)
                throw new ArgumentNullException();

            if (sourceIndex + length > data.Length)
                throw new ArgumentOutOfRangeException();

            return NativeOTP_Read(data, sourceIndex, destinationIndex, length);
        }


        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern void NativeEnableExternalFlash();

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern bool NativeIsEnabledExternalFlash();

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern int NativeOTP_Write(byte[] data, int sourceIndex, int destinationIndex, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern int NativeOTP_Read(byte[] data, int sourceIndex, int destinationIndex, int length);


    }
}
