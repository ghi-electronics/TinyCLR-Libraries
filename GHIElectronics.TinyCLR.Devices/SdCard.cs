using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.SdCard.Provider;

namespace GHIElectronics.TinyCLR.Devices.SdCard {
    public sealed class SdCardController : IDisposable {
        public ISdCardControllerProvider Provider { get; }

        private SdCardController(ISdCardControllerProvider provider) => this.Provider = provider;

        public static SdCardController GetDefault() => Api.GetDefaultFromCreator(ApiType.SdCardController) is SdCardController c ? c : SdCardController.FromName(Api.GetDefaultName(ApiType.SdCardController));
        public static SdCardController FromName(string name) => SdCardController.FromProvider(new SdCardControllerApiWrapper(Api.Find(name, ApiType.SdCardController)));
        public static SdCardController FromProvider(ISdCardControllerProvider provider) => new SdCardController(provider);

        public IntPtr Hdc => this.Provider is ISdCardControllerHdc h ? h.Hdc : throw new NotSupportedException();

        public void Dispose() => this.Provider.Dispose();
    }

    namespace Provider {
        public interface ISdCardControllerProvider : IDisposable {
            uint ReadSectors(ulong sector, uint count, byte[] buffer, int offset, uint timeout);
            uint WriteSectors(ulong sector, uint count, byte[] buffer, int offset, uint timeout);
            uint EraseSectors(ulong sector, uint count, uint timeout);
            bool IsSectorErased(ulong sector);
            void GetSectorMap(out uint[] sizes, out uint count, out bool uniform);
        }

        public interface ISdCardControllerHdc {
            IntPtr Hdc { get; }
        }

        public sealed class SdCardControllerApiWrapper : ISdCardControllerProvider, ISdCardControllerHdc {
            private readonly IntPtr impl;

            public Api Api { get; }

            public IntPtr Hdc => this.impl;

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
            public extern uint ReadSectors(ulong sector, uint count, byte[] buffer, int offset, uint timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern uint WriteSectors(ulong sector, uint count, byte[] buffer, int offset, uint timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern uint EraseSectors(ulong sector, uint count, uint timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool IsSectorErased(ulong sector);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetSectorMap(out uint[] sizes, out uint count, out bool uniform);
        }
    }
}
