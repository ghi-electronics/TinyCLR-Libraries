using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Storage.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Storage {
    public sealed class StorageController : IDisposable {
        public IStorageControllerProvider Provider { get; }

        private StorageController(IStorageControllerProvider provider) {
            this.Provider = provider;

            this.Provider.PresenceChanged += (_, e) => this.PresenceChanged?.Invoke(this, e);
        }

        public static StorageController GetDefault() => Api.GetDefaultFromCreator(ApiType.StorageController) is StorageController c ? c : StorageController.FromName(Api.GetDefaultName(ApiType.StorageController));
        public static StorageController FromName(string name) => StorageController.FromProvider(new StorageControllerApiWrapper(Api.Find(name, ApiType.StorageController)));
        public static StorageController FromProvider(IStorageControllerProvider provider) => new StorageController(provider);

        public IntPtr Hdc => this.Provider is IApiImplementation a ? a.Implementation : throw new NotSupportedException();

        public void Dispose() => this.Provider.Dispose();

        public void Open() => this.Provider.Open();
        public void Close() => this.Provider.Close();

        public bool IsPresent => this.Provider.IsPresent;
        public StorageDescriptor Descriptor => this.Provider.Descriptor;

        public event PresenceChangedEventHandler PresenceChanged;
    }

    public sealed class StorageDescriptor {
        public bool CanReadDirect { get; }
        public bool CanWriteDirect { get; }
        public bool CanExecuteDirect { get; }
        public bool EraseBeforeWrite { get; }
        public bool Removable { get; }
        public bool RegionsRepeat { get; }
        public int RegionCount { get; }
        public long[] RegionAddresses { get; }
        public int[] RegionSizes { get; }
    }

    public delegate void PresenceChangedEventHandler(StorageController sender, PresenceChangedEventArgs e);

    public sealed class PresenceChangedEventArgs {
        public bool Present { get; }

        internal PresenceChangedEventArgs(bool present) => this.Present = present;
    }

    namespace Provider {
        public interface IStorageControllerProvider : IDisposable {
            bool IsPresent { get; }
            StorageDescriptor Descriptor { get; }

            void Open();
            void Close();
            int Read(long address, int count, byte[] buffer, int offset, long timeout);
            int Write(long address, int count, byte[] buffer, int offset, long timeout);
            int Erase(long address, int count, long timeout);
            bool IsErased(long address, int count);

            event PresenceChangedEventHandler PresenceChanged;
        }

        public sealed class StorageControllerApiWrapper : IStorageControllerProvider, IApiImplementation {
            private readonly IntPtr impl;
            private readonly NativeEventDispatcher presenceChangedDispatcher;

            public Api Api { get; }

            IntPtr IApiImplementation.Implementation => this.impl;

            public StorageControllerApiWrapper(Api api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.presenceChangedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Storage.PresenceChanged");

                this.presenceChangedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.PresenceChanged?.Invoke(null, new PresenceChangedEventArgs(d0 != 0)); };
            }

            public event PresenceChangedEventHandler PresenceChanged;

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            public extern bool IsPresent { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern StorageDescriptor Descriptor { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Open();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Close();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(long address, int count, byte[] buffer, int offset, long timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Write(long address, int count, byte[] buffer, int offset, long timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Erase(long sector, int count, long timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool IsErased(long address, int count);
        }
    }
}
