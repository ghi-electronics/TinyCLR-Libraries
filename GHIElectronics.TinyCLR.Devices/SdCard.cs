using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.SdCard.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.SdCard {
    public sealed class SdCardController : IDisposable {
        public ISdCardControllerProvider Provider { get; }

        private SdCardController(ISdCardControllerProvider provider) => this.Provider = provider;

        public static SdCardController GetDefault() => Api.GetDefaultFromCreator(ApiType.SdCardController) is SdCardController c ? c : SdCardController.FromName(Api.GetDefaultName(ApiType.SdCardController));
        public static SdCardController FromName(string name) => SdCardController.FromProvider(new SdCardControllerApiWrapper(Api.Find(name, ApiType.SdCardController)));
        public static SdCardController FromProvider(ISdCardControllerProvider provider) => new SdCardController(provider);

        public IntPtr Hdc => this.Provider is IApiImplementation a ? a.Implementation : throw new NotSupportedException();

        public void Dispose() => this.Provider.Dispose();
    }

    namespace Provider {
        public interface ISdCardControllerProvider : IDisposable {
            int ReadSectors(long sector, int count, byte[] buffer, int offset, int timeout);
            int WriteSectors(long sector, int count, byte[] buffer, int offset, int timeout);
            int EraseSectors(long sector, int count, int timeout);
            bool IsSectorErased(long sector);
            void GetSectorMap(out int[] sizes, out int count, out bool uniform);
        }

        public sealed class SdCardControllerApiWrapper : ISdCardControllerProvider, IApiImplementation {
            private readonly IntPtr impl;

            public Api Api { get; }

            IntPtr IApiImplementation.Implementation => this.impl;

            public SdCardControllerApiWrapper(Api api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int ReadSectors(long sector, int count, byte[] buffer, int offset, int timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int WriteSectors(long sector, int count, byte[] buffer, int offset, int timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int EraseSectors(long sector, int count, int timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool IsSectorErased(long sector);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetSectorMap(out int[] sizes, out int count, out bool uniform);
        }
    }
}
