using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public class Memory {
        private static Memory managed = new Memory(NativeApi.Find("GHIElectronics.TinyCLR.NativeApis.TinyCLR.ManagedMemoryManager", NativeApiType.MemoryManager));
        private static Memory unmanaged = new Memory(NativeApi.Find("GHIElectronics.TinyCLR.NativeApis.TinyCLR.UnmanagedMemoryManager", NativeApiType.MemoryManager));

        private readonly IntPtr api;

        private Memory(NativeApi api) => this.api = api.Implementation;

        public static Memory ManagedMemory => Memory.managed;
        public static Memory UnmanagedMemory => Memory.unmanaged;

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

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void EnableExternalHeap();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsEnabledExternalHeap();
    }

    public enum UnmanagedBufferLocation {
        ManagedMemory,
        UnmanagedMemory
    }

    public class UnmanagedBuffer : IDisposable {
        private IntPtr ptr;
        private byte[] mem;
        private bool disposed;

        public byte[] Bytes => this.mem;

        public UnmanagedBuffer(int length) : this(length, UnmanagedBufferLocation.UnmanagedMemory) {

        }

        public UnmanagedBuffer(int length, UnmanagedBufferLocation location) {
            if (location != UnmanagedBufferLocation.UnmanagedMemory) throw new ArgumentOutOfRangeException(nameof(location));

            this.ptr = Memory.UnmanagedMemory.Allocate(length);
            this.mem = Memory.UnmanagedMemory.ToBytes(this.ptr, length);
        }

        ~UnmanagedBuffer() => this.Dispose(false);

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool fDisposing) {
            if (!this.disposed) {
                Memory.UnmanagedMemory.Free(this.ptr);

                this.ptr = IntPtr.Zero;
                this.mem = null;
                this.disposed = true;
            }
        }
    }
}
