using System;

namespace GHIElectronics.TinyCLR.Native {
    // Public surface mirrors the impl. Allocate returns IntPtr.Zero;
    // ToBytes returns a real managed byte[length] (so callers like
    // UnmanagedBuffer can use it normally on Desktop). Free is a no-op
    // since we never actually allocated unmanaged memory.
    public class Memory {
        private static readonly Memory managed = new Memory();
        private static readonly Memory unmanaged = new Memory();

        private Memory() { }

        public static Memory ManagedMemory => Memory.managed;
        public static Memory UnmanagedMemory => Memory.unmanaged;

        public IntPtr Allocate(long length) => this.Allocate((IntPtr)length);
        public IntPtr Allocate(IntPtr length) => IntPtr.Zero;

        public void Free(IntPtr ptr) { }

        public byte[] ToBytes(IntPtr ptr, long length) => this.ToBytes(ptr, (IntPtr)length);
        public byte[] ToBytes(IntPtr ptr, IntPtr length) {
            var n = length.ToInt64();
            return n > 0 && n <= int.MaxValue ? new byte[(int)n] : new byte[0];
        }

        public void GetStats(out IntPtr used, out IntPtr free) {
            used = IntPtr.Zero;
            free = IntPtr.Zero;
        }

        public long UsedBytes { get { this.GetStats(out var used, out _); return used.ToInt64(); } }
        public long FreeBytes { get { this.GetStats(out _, out var free); return free.ToInt64(); } }

        public static void ExtendHeap() { }
        public static bool IsExtendedHeap() => false;
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

        public UnmanagedBuffer(int length) : this(length, UnmanagedBufferLocation.UnmanagedMemory) { }

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
