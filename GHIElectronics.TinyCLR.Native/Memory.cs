using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    /// <summary>
    /// Allocator façade exposing both the managed (GC) heap and the unmanaged
    /// heap. Most apps don't touch this directly — use <see cref="UnmanagedBuffer"/>
    /// when you need a fixed-address byte buffer (for DMA, native interop, etc.).
    /// </summary>
    public class Memory {
        private static Memory managed = new Memory(NativeApi.Find("GHIElectronics.TinyCLR.NativeApis.TinyCLR.ManagedMemoryManager", NativeApiType.MemoryManager));
        private static Memory unmanaged = new Memory(NativeApi.Find("GHIElectronics.TinyCLR.NativeApis.TinyCLR.UnmanagedMemoryManager", NativeApiType.MemoryManager));

        private readonly IntPtr api;

        private Memory(NativeApi api) => this.api = api.Implementation;

        /// <summary>The managed (GC) heap allocator.</summary>
        public static Memory ManagedMemory => Memory.managed;
        /// <summary>The unmanaged heap allocator (fixed addresses, not GC-tracked).</summary>
        public static Memory UnmanagedMemory => Memory.unmanaged;

        /// <summary>Allocates <paramref name="length"/> bytes; returns the pointer.</summary>
        public IntPtr Allocate(long length) => this.Allocate((IntPtr)length);

        /// <summary>Allocates the given number of bytes.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern IntPtr Allocate(IntPtr length);

        /// <summary>Frees a pointer returned by <see cref="Allocate(long)"/>.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Free(IntPtr ptr);

        /// <summary>Returns a byte[] aliased to a fixed-address region (no copy).</summary>
        public byte[] ToBytes(IntPtr ptr, long length) => this.ToBytes(ptr, (IntPtr)length);

        /// <summary>Returns a byte[] aliased to a fixed-address region (no copy).</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern byte[] ToBytes(IntPtr ptr, IntPtr length);

        /// <summary>Reads current heap usage and free space.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void GetStats(out IntPtr used, out IntPtr free);

        /// <summary>Bytes currently allocated from this heap.</summary>
        public long UsedBytes { get { this.GetStats(out var used, out _); return used.ToInt64(); } }
        /// <summary>Bytes currently free in this heap.</summary>
        public long FreeBytes { get { this.GetStats(out _, out var free); return free.ToInt64(); } }

        /// <summary>Permanently extends the managed heap into external SDRAM (where supported).</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void ExtendHeap();

        /// <summary>True when the managed heap has been extended via <see cref="ExtendHeap"/>.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool IsExtendedHeap();
    }

    /// <summary>Which heap an <see cref="UnmanagedBuffer"/> draws from.</summary>
    public enum UnmanagedBufferLocation {
        /// <summary>Allocated from the managed (GC) heap.</summary>
        ManagedMemory,
        /// <summary>Allocated from the unmanaged heap (fixed address).</summary>
        UnmanagedMemory
    }

    /// <summary>
    /// A fixed-address byte buffer suitable for DMA targets and native interop.
    /// The <see cref="Bytes"/> property exposes it as a regular byte[]. Dispose to
    /// release the underlying memory.
    /// </summary>
    public class UnmanagedBuffer : IDisposable {
        private IntPtr ptr;
        private byte[] mem;
        private bool disposed;

        /// <summary>Byte[] view of the unmanaged region. Same address every read.</summary>
        public byte[] Bytes => this.mem;

        /// <summary>Allocates a buffer of the given length in the unmanaged heap.</summary>
        public UnmanagedBuffer(int length) : this(length, UnmanagedBufferLocation.UnmanagedMemory) {

        }

        /// <summary>Allocates a buffer of the given length from the specified heap.</summary>
        /// <param name="length">Buffer size in bytes.</param>
        /// <param name="location">Heap to allocate from. Only <see cref="UnmanagedBufferLocation.UnmanagedMemory"/> is supported.</param>
        public UnmanagedBuffer(int length, UnmanagedBufferLocation location) {
            if (location != UnmanagedBufferLocation.UnmanagedMemory) throw new ArgumentOutOfRangeException(nameof(location));

            this.ptr = Memory.UnmanagedMemory.Allocate(length);
            this.mem = Memory.UnmanagedMemory.ToBytes(this.ptr, length);
        }

        /// <summary>Finalizer; releases the unmanaged region if not already disposed.</summary>
        ~UnmanagedBuffer() => this.Dispose(false);

        /// <summary>Releases the unmanaged region.</summary>
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
