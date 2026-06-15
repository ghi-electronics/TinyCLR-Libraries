using System;
using GHIElectronics.TinyCLR.Devices.Storage.Provider;
using GHIElectronics.TinyCLR.Native;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.Storage\Storage.cs.
// Bodies on Desktop are safe no-ops; Read/Erase/IsErased return harmless
// defaults. Descriptor exposes a small in-memory backing layout so user
// code that scans regions runs through.
namespace GHIElectronics.TinyCLR.Devices.Storage {
    public class StorageController : IDisposable {
        public IStorageControllerProvider Provider { get; }

        private StorageController(IStorageControllerProvider provider) => this.Provider = provider;

        public static StorageController GetDefault() => StorageController.FromName("Simulator");
        public static StorageController FromName(string name) => StorageController.FromProvider(new StorageControllerApiWrapper(NativeApi.Find(name, NativeApiType.StorageController)));
        public static StorageController FromProvider(IStorageControllerProvider provider) => new StorageController(provider);

        public IntPtr Hdc => this.Provider is IApiImplementation a ? a.Implementation : throw new NotSupportedException();

        public void Dispose() => this.Provider.Dispose();

        public void Open() => this.Provider.Open();
        public void Close() => this.Provider.Close();

        public StorageDescriptor Descriptor => this.Provider.Descriptor;
    }

    public class StorageDescriptor {
        public bool CanReadDirect { get; set; }
        public bool CanWriteDirect { get; set; }
        public bool CanExecuteDirect { get; set; }
        public bool EraseBeforeWrite { get; set; }
        public bool Removable { get; set; }
        public bool RegionsContiguous { get; set; }
        public bool RegionsEqualSized { get; set; }
        public int RegionCount { get; set; }
        public long[] RegionAddresses { get; set; }
        public int[] RegionSizes { get; set; }
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
            void EraseAll(TimeSpan timeout);
        }

        public sealed class StorageControllerApiWrapper : IStorageControllerProvider, IApiImplementation {
            public NativeApi Api { get; }

            IntPtr IApiImplementation.Implementation => IntPtr.Zero;

            public StorageControllerApiWrapper(NativeApi api) => this.Api = api;

            public void Dispose() { }

            public StorageDescriptor Descriptor => new StorageDescriptor {
                CanReadDirect = true,
                CanWriteDirect = true,
                CanExecuteDirect = false,
                EraseBeforeWrite = true,
                Removable = false,
                RegionsContiguous = true,
                RegionsEqualSized = true,
                RegionCount = 1,
                RegionAddresses = new long[] { 0 },
                RegionSizes = new[] { 4096 }
            };

            public void Open() { }
            public void Close() { }
            public int Read(long address, int count, byte[] buffer, int offset, TimeSpan timeout) => count;
            public int Write(long address, int count, byte[] buffer, int offset, TimeSpan timeout) => count;
            public int Erase(long sector, int count, TimeSpan timeout) => count;
            public bool IsErased(long address, int count) => true;
            public void EraseAll(TimeSpan timeout) { }
        }
    }
}
