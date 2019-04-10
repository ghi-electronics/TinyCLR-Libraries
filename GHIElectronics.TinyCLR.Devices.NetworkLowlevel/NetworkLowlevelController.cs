using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.NetworkLowlevel.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.NetworkLowlevel {
    public sealed class NetworkLowlevelController {
        public INetworkLowlevelControllerProvider Provider { get; }
        public IntPtr Hdc => this.Provider is IApiImplementation a ? a.Implementation : throw new NotSupportedException();
        private NetworkLowlevelController(INetworkLowlevelControllerProvider provider) => this.Provider = provider;

        public static NetworkLowlevelController GetDefault() => Api.GetDefaultFromCreator(ApiType.NetworkLowlevelController) is NetworkLowlevelController c ? c : NetworkLowlevelController.FromName(Api.GetDefaultName(ApiType.NetworkLowlevelController));
        public static NetworkLowlevelController FromName(string name) => NetworkLowlevelController.FromProvider(new NetworkLowlevelControllerApiWrapper(Api.Find(name, ApiType.NetworkLowlevelController)));
        public static NetworkLowlevelController FromProvider(INetworkLowlevelControllerProvider provider) => new NetworkLowlevelController(provider);

        ~NetworkLowlevelController() => this.Dispose();

        public void Dispose() => GC.SuppressFinalize(this);

    }

    namespace Provider {
        public interface INetworkLowlevelControllerProvider : IDisposable {
        }
    }

    public sealed class NetworkLowlevelControllerApiWrapper : INetworkLowlevelControllerProvider, IApiImplementation {
        private readonly IntPtr impl;

        public Api Api { get; }

        IntPtr IApiImplementation.Implementation => this.impl;

        public NetworkLowlevelControllerApiWrapper(Api api) {
            this.Api = api;

            this.impl = api.Implementation;

            this.Acquire();
        }

        public void Dispose() => this.Release();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Acquire();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Release();
    }
}
