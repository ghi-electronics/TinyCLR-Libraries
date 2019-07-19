using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Storage.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Storage {
    public sealed class StorageController : IDisposable {
        public IStorageControllerProvider Provider { get; }

        private StorageController(IStorageControllerProvider provider) => this.Provider = provider;

        public static StorageController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.StorageController) is StorageController c ? c : StorageController.FromName(NativeApi.GetDefaultName(NativeApiType.StorageController));
        public static StorageController FromName(string name) => StorageController.FromProvider(new StorageControllerApiWrapper(NativeApi.Find(name, NativeApiType.StorageController)));
        public static StorageController FromProvider(IStorageControllerProvider provider) => new StorageController(provider);

        public IntPtr Hdc => this.Provider is IApiImplementation a ? a.Implementation : throw new NotSupportedException();

        public void Dispose() => this.Provider.Dispose();

        public void Open() => this.Provider.Open();
        public void Close() => this.Provider.Close();

        public StorageDescriptor Descriptor => this.Provider.Descriptor;
    }

    public sealed class StorageDescriptor {
        public bool CanReadDirect { get; }
        public bool CanWriteDirect { get; }
        public bool CanExecuteDirect { get; }
        public bool EraseBeforeWrite { get; }
        public bool Removable { get; }
        public bool RegionsContiguous { get; }
        public bool RegionsEqualSized { get; }
        public int RegionCount { get; }
        public long[] RegionAddresses { get; }
        public int[] RegionSizes { get; }
    }

    namespace Provider {
        public interface IStorageControllerProvider : IDisposable {
            StorageDescriptor Descriptor { get; }

            void Open();
            void Close();
            int Read(long address, int count, byte[] buffer, int offset, TimeSpan timeout);
            int Write(long address, int count, byte[] buffer, int offset, TimeSpan timeout);
            int Erase(long address, int count, TimeSpan timeout);
            bool IsErased(long address, int count);
        }

        public sealed class StorageControllerApiWrapper : IStorageControllerProvider, IApiImplementation {
            private readonly IntPtr impl;

            public NativeApi Api { get; }

            IntPtr IApiImplementation.Implementation => this.impl;

            public StorageControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            public extern StorageDescriptor Descriptor { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Open();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Close();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(long address, int count, byte[] buffer, int offset, TimeSpan timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Write(long address, int count, byte[] buffer, int offset, TimeSpan timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Erase(long sector, int count, TimeSpan timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool IsErased(long address, int count);
        }
    }
}
