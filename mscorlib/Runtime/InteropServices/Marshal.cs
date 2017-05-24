using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices {
    public delegate void FunctionPointerDelegate(ref IntPtr ptr);

    public static class Marshal {
        private class InvokeHelper {
            public IntPtr Target;

            public void Invoke(ref IntPtr para) => InvokeHelper.InvokeIntPtr(this.Target, ref para);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private static extern void InvokeIntPtr(IntPtr target, ref IntPtr para);
        }

        public static byte ReadByte(IntPtr ptr, int ofs) => Marshal.ReadByte(ptr + ofs);
        public static short ReadInt16(IntPtr ptr, int ofs) => Marshal.ReadInt16(ptr + ofs);
        public static int ReadInt32(IntPtr ptr, int ofs) => Marshal.ReadInt32(ptr + ofs);
        public static long ReadInt64(IntPtr ptr, int ofs) => Marshal.ReadInt64(ptr + ofs);
        public static IntPtr ReadIntPtr(IntPtr ptr) => (IntPtr)Marshal.ReadInt32(ptr, 0);
        public static IntPtr ReadIntPtr(IntPtr ptr, int ofs) => (IntPtr)Marshal.ReadInt32(ptr, ofs);

        public static void WriteByte(IntPtr ptr, int ofs, byte val) => Marshal.WriteByte(ptr + ofs, val);
        public static void WriteInt16(IntPtr ptr, int ofs, short val) => Marshal.WriteInt16(ptr + ofs, val);
        public static void WriteInt16(IntPtr ptr, char val) => Marshal.WriteInt16(ptr, 0, (short)val);
        public static void WriteInt16(IntPtr ptr, int ofs, char val) => Marshal.WriteInt16(ptr, ofs, (short)val);
        public static void WriteInt32(IntPtr ptr, int ofs, int val) => Marshal.WriteInt32(ptr + ofs, val);
        public static void WriteInt64(IntPtr ptr, int ofs, long val) => Marshal.WriteInt64(ptr + ofs, val);
        public static void WriteIntPtr(IntPtr ptr, IntPtr val) => Marshal.WriteInt32(ptr, 0, (int)val);
        public static void WriteIntPtr(IntPtr ptr, int ofs, IntPtr val) => Marshal.WriteInt32(ptr, ofs, (int)val);

        public static IntPtr AllocHGlobal(int cb) => AllocHGlobal((IntPtr)cb);

        public static Delegate GetDelegateForFunctionPointer(IntPtr ptr, Type t) {
            if (t != typeof(FunctionPointerDelegate)) throw new NotSupportedException($"Only {nameof(FunctionPointerDelegate)} is supported as the delegate type.");

            return new FunctionPointerDelegate(new InvokeHelper { Target = ptr }.Invoke);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern byte ReadByte(IntPtr ptr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern short ReadInt16(IntPtr ptr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int ReadInt32(IntPtr ptr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern long ReadInt64(IntPtr ptr);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteByte(IntPtr ptr, byte val);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteInt16(IntPtr ptr, short val);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteInt32(IntPtr ptr, int val);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteInt64(IntPtr ptr, long val);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern IntPtr AllocHGlobal(IntPtr cb);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void FreeHGlobal(IntPtr hglobal);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(int[] source, int startIndex, IntPtr destination, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(char[] source, int startIndex, IntPtr destination, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(short[] source, int startIndex, IntPtr destination, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(long[] source, int startIndex, IntPtr destination, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(float[] source, int startIndex, IntPtr destination, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(double[] source, int startIndex, IntPtr destination, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(byte[] source, int startIndex, IntPtr destination, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(IntPtr[] source, int startIndex, IntPtr destination, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(IntPtr source, int[] destination, int startIndex, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(IntPtr source, char[] destination, int startIndex, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(IntPtr source, short[] destination, int startIndex, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(IntPtr source, long[] destination, int startIndex, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(IntPtr source, float[] destination, int startIndex, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(IntPtr source, double[] destination, int startIndex, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(IntPtr source, byte[] destination, int startIndex, int length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Copy(IntPtr source, IntPtr[] destination, int startIndex, int length);
    }
}
