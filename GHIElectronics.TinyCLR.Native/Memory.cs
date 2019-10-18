using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public class Memory {
        private static Memory secure = new Memory(NativeApi.Find("GHIElectronics.TinyCLR.NativeApis.TinyCLR.SecureMemoryManager", NativeApiType.MemoryManager));
        private static Memory unsecure = new Memory(NativeApi.Find("GHIElectronics.TinyCLR.NativeApis.TinyCLR.UnsecureMemoryManager", NativeApiType.MemoryManager));

        private readonly IntPtr api;

        private Memory(NativeApi api) => this.api = api.Implementation;

        public static Memory SecureMemory => Memory.secure;
        public static Memory UnsecureMemory => Memory.unsecure;

        public IntPtr Allocate(long length) => this.Allocate((IntPtr)length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern IntPtr Allocate(IntPtr length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Free(IntPtr ptr);

        public byte[] ToBytes(IntPtr ptr, long length) => this.ToBytes(ptr, (IntPtr)length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern byte[] ToBytes(IntPtr ptr, IntPtr length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void GetStats(out IntPtr used, out IntPtr free);

        public long UsedBytes { get { this.GetStats(out var used, out _); return used.ToInt64(); } }
        public long FreeBytes { get { this.GetStats(out _, out var free); return free.ToInt64(); } }
    }

    public class ExternalBuffer : IDisposable {
        private IntPtr ptr;
        private byte[] mem;
        private bool disposed;

        public byte[] Bytes => this.mem;

        public ExternalBuffer(int length) {
            this.ptr = Memory.UnsecureMemory.Allocate(length);
            this.mem = Memory.UnsecureMemory.ToBytes(this.ptr, length);
        }

        ~ExternalBuffer() => this.Dispose(false);

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool fDisposing) {
            if (!this.disposed) {
                Memory.UnsecureMemory.Free(this.ptr);

                this.ptr = IntPtr.Zero;
                this.mem = null;
                this.disposed = true;
            }
        }
    }
}
