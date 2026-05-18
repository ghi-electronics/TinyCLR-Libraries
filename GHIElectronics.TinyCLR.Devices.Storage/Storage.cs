using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Storage.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Storage {
    /// <summary>
    /// Represents a block-storage device — internal flash, external SPI/SD flash,
    /// SD/MMC, or USB mass-storage. Use <see cref="Hdc"/> to mount the controller
    /// with the file-system stack, or call the provider's Read/Write/Erase methods
    /// directly for raw block access.
    /// </summary>
    public class StorageController : IDisposable {
        /// <summary>The low-level provider backing this controller.</summary>
        public IStorageControllerProvider Provider { get; }

        private StorageController(IStorageControllerProvider provider) => this.Provider = provider;

        /// <summary>Returns the default storage controller for this device.</summary>
        public static StorageController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.StorageController) is StorageController c ? c : StorageController.FromName(NativeApi.GetDefaultName(NativeApiType.StorageController));
        /// <summary>Returns a storage controller identified by its native API name.</summary>
        public static StorageController FromName(string name) => StorageController.FromProvider(new StorageControllerApiWrapper(NativeApi.Find(name, NativeApiType.StorageController)));
        /// <summary>Creates a controller from a custom <see cref="IStorageControllerProvider"/>.</summary>
        public static StorageController FromProvider(IStorageControllerProvider provider) => new StorageController(provider);

        /// <summary>Native handle (HDC) of the underlying provider, for use with the file-system mount API.</summary>
        public IntPtr Hdc => this.Provider is IApiImplementation a ? a.Implementation : throw new NotSupportedException();

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>Powers on and initializes the underlying media.</summary>
        public void Open() => this.Provider.Open();
        /// <summary>Powers off the underlying media.</summary>
        public void Close() => this.Provider.Close();

        /// <summary>Reports the media's block layout and capabilities.</summary>
        public StorageDescriptor Descriptor => this.Provider.Descriptor;
    }

    /// <summary>Describes a storage medium's geometry and capabilities.</summary>
    public class StorageDescriptor {
        /// <summary>True when single-byte reads are supported (NOR-style flash, RAM).</summary>
        public bool CanReadDirect { get; set; }
        /// <summary>True when single-byte writes are supported.</summary>
        public bool CanWriteDirect { get; set; }
        /// <summary>True when code can be executed in-place from this media (XIP).</summary>
        public bool CanExecuteDirect { get; set; }
        /// <summary>True when a sector must be erased before it can be overwritten (most flash).</summary>
        public bool EraseBeforeWrite { get; set; }
        /// <summary>True when the media can be physically removed (SD card, USB stick).</summary>
        public bool Removable { get; set; }
        /// <summary>True when <see cref="RegionAddresses"/> form a contiguous span.</summary>
        public bool RegionsContiguous { get; set; }
        /// <summary>True when every entry in <see cref="RegionSizes"/> has the same value.</summary>
        public bool RegionsEqualSized { get; set; }
        /// <summary>Number of distinct erase regions reported.</summary>
        public int RegionCount { get; set; }
        /// <summary>Starting address of each erase region.</summary>
        public long[] RegionAddresses { get; set; }
        /// <summary>Size in bytes of each erase region (parallel to <see cref="RegionAddresses"/>).</summary>
        public int[] RegionSizes { get; set; }
    }

    namespace Provider {
        /// <summary>Provider contract for a block-storage controller.</summary>
        public interface IStorageControllerProvider : IDisposable {
            /// <summary>Media geometry and capabilities.</summary>
            StorageDescriptor Descriptor { get; }

            /// <summary>Powers on and initializes the media.</summary>
            void Open();
            /// <summary>Powers off the media.</summary>
            void Close();
            /// <summary>Reads <paramref name="count"/> bytes from <paramref name="address"/> into <paramref name="buffer"/>+<paramref name="offset"/>.</summary>
            int Read(long address, int count, byte[] buffer, int offset, TimeSpan timeout);
            /// <summary>Writes <paramref name="count"/> bytes from <paramref name="buffer"/>+<paramref name="offset"/> to <paramref name="address"/>.</summary>
            int Write(long address, int count, byte[] buffer, int offset, TimeSpan timeout);
            /// <summary>Erases <paramref name="count"/> sectors starting at <paramref name="address"/>.</summary>
            int Erase(long address, int count, TimeSpan timeout);
            /// <summary>True when the addressed span is in its erased (all-0xFF) state.</summary>
            bool IsErased(long address, int count);
            /// <summary>Erases every sector on the media.</summary>
            void EraseAll(TimeSpan timeout);
        }

        /// <summary>Concrete <see cref="IStorageControllerProvider"/> backed by the native TinyCLR storage HAL.</summary>
        public sealed class StorageControllerApiWrapper : IStorageControllerProvider, IApiImplementation {
            private readonly IntPtr impl;

            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            IntPtr IApiImplementation.Implementation => this.impl;

            /// <summary>Wraps the given native API as a provider.</summary>
            public StorageControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            /// <summary>Releases the native controller.</summary>
            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            /// <inheritdoc/>
            public extern StorageDescriptor Descriptor { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Open();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Close();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(long address, int count, byte[] buffer, int offset, TimeSpan timeout);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Write(long address, int count, byte[] buffer, int offset, TimeSpan timeout);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Erase(long sector, int count, TimeSpan timeout);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool IsErased(long address, int count);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void EraseAll(TimeSpan timeout);
        }
    }
}
