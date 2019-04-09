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
    /*
    namespace GHIElectronics.TinyCLR.Devices.EthernetEmac {
        public class EthernetEmacController : NetworkInterface, ISocketProvider, ISslStreamProvider, IDnsProvider, IDisposable {
            private readonly Hashtable netifSockets;

            public IEthernetEmacControllerProvider Provider { get; }

            private EthernetEmacController(IEthernetEmacControllerProvider provider) {
                this.Provider = provider;

                this.netifSockets = new Hashtable();

                NetworkInterface.RegisterNetworkInterface(this);
            }

            public static EthernetEmacController GetDefault() => Api.GetDefaultFromCreator(ApiType.EthernetMacController) is EthernetEmacController c ? c : EthernetEmacController.FromName(Api.GetDefaultName(ApiType.EthernetMacController));
            public static EthernetEmacController FromName(string name) => EthernetEmacController.FromProvider(new EthernetEmacControllerApiWrapper(Api.Find(name, ApiType.EthernetMacController)));
            public static EthernetEmacController FromProvider(IEthernetEmacControllerProvider provider) => new EthernetEmacController(provider);

            ~EthernetEmacController() => this.Dispose(false);

            public void Dispose() {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing) {
                if (disposing) {
                    NetworkInterface.DeregisterNetworkInterface(this);
                }
            }

            public void Open() => this.Provider.Open();
            void Close() => this.Provider.Close();
            void EnableStaticIP(string ipAddress, string subnetMask, string gatewayAddress) => this.Provider.EnableStaticIP(ipAddress, subnetMask, gatewayAddress);
            void EnableStaticDns(string[] dnsAddresses) => this.Provider.EnableStaticDns(dnsAddresses);

            int ISocketProvider.Accept(int socket) => this.Provider.ISocketProviderNativeAccept(socket);

            int ISslStreamProvider.AuthenticateAsClient(int socketHandle, string targetHost, X509Certificate certificate, SslProtocols[] sslProtocols) => throw new NotImplementedException();

            int ISslStreamProvider.AuthenticateAsServer(int socketHandle, X509Certificate certificate, SslProtocols[] sslProtocols) => throw new NotImplementedException();

            int ISslStreamProvider.Available(int handle) => throw new NotImplementedException();

            void ISslStreamProvider.Close(int handle) => throw new NotImplementedException();

            int ISocketProvider.Available(int socket) => this.Provider.ISocketProviderNativeAvailable(socket);

            void ISocketProvider.Bind(int socket, SocketAddress address) => this.Provider.ISocketProviderNativeBind(socket, address);

            void ISocketProvider.Close(int socket) {
                this.Provider.ISocketProviderNativeClose(socket);

                this.netifSockets.Remove(socket);
            }

            void ISocketProvider.Connect(int socket, SocketAddress address) {
                if (!this.netifSockets.Contains(socket)) throw new ArgumentException();
                if (address.Family != AddressFamily.InterNetwork) throw new ArgumentException();

                if (this.Provider.ISocketProviderNativeConnect(socket, address) == true) {
                    this.netifSockets[socket] = socket;
                }
            }

            int ISocketProvider.Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
                var id = this.Provider.ISocketProviderNativeCreate(addressFamily, socketType, protocolType);

                if (id >= 0) {
                    this.netifSockets.Add(id, 0);

                    return id;
                }

                throw new SocketException(SocketError.TooManyOpenSockets);
            }

            void ISocketProvider.GetLocalAddress(int socket, out SocketAddress address) => address = new SocketAddress(AddressFamily.InterNetwork, 16);

            void ISocketProvider.GetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) {
                if (optionLevel == SocketOptionLevel.Socket && optionName == SocketOptionName.Type)
                    Array.Copy(BitConverter.GetBytes((int)SocketType.Stream), optionValue, 4);
            }

            void ISocketProvider.GetRemoteAddress(int socket, out SocketAddress address) => address = new SocketAddress(AddressFamily.InterNetwork, 16);

            void ISocketProvider.Listen(int socket, int backlog) => this.Provider.ISocketProviderNativeListen(socket, backlog);

            bool ISocketProvider.Poll(int socket, int microSeconds, SelectMode mode) => this.Provider.ISocketProviderNativePoll(socket, microSeconds, mode);

            int ISslStreamProvider.Read(int handle, byte[] buffer, int offset, int count, int timeout) => throw new NotImplementedException();

            int ISocketProvider.Receive(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout) => this.Provider.ISocketProviderNativeReceive(socket, buffer, offset, count, flags, timeout);

            int ISocketProvider.ReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, ref SocketAddress address) => this.Provider.ISocketProviderNativeReceiveFrom(socket, buffer, offset, count, flags, timeout, ref address);

            int ISocketProvider.Send(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout) => this.Provider.ISocketProviderNativeSend(socket, buffer, offset, count, flags, timeout);

            int ISocketProvider.SendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, SocketAddress address) => this.Provider.ISocketProviderNativeSendTo(socket, buffer, offset, count, flags, timeout, address);

            void ISocketProvider.SetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) => this.Provider.ISocketProviderNativeSetOption(socket, optionLevel, optionName, optionValue);

            int ISslStreamProvider.Write(int handle, byte[] buffer, int offset, int count, int timeout) => throw new NotImplementedException();

            void IDnsProvider.GetHostByName(string name, out string canonicalName, out SocketAddress[] addresses) {

                this.Provider.IDnsProviderNativeGetHostByName(name, out var ipAddress);

                canonicalName = "";

                addresses = new[] { new IPEndPoint(ipAddress, 443).Serialize() };
            }

            // override
            public override PhysicalAddress GetPhysicalAddress() {
                this.Provider.NativeGetPhysicalAddress(out var ip);

                if (ip == null) ip = new byte[] { 0, 0, 0, 0 };

                return new PhysicalAddress(ip);
            }

            public override string Id => nameof(EthernetEmac);
            public override string Name => this.Id;
            public override string Description => string.Empty;
            public override OperationalStatus OperationalStatus => throw new NotImplementedException();
            public override bool IsReceiveOnly => false;
            public override bool SupportsMulticast => false;
            public override NetworkInterfaceType NetworkInterfaceType => NetworkInterfaceType.Ethernet;
            public override bool Supports(NetworkInterfaceComponent networkInterfaceComponent) => networkInterfaceComponent == NetworkInterfaceComponent.IPv4;
            public bool DhcpEnable {
                get => this.Provider.IsDhcpEnabled();
                set => this.Provider.SetDhcp(value);
            }

            private NetworkAvailabilityEventHandler networkAvailabilityChangedCallbacks;
            private NetworkAddressEventHandler networkAddressChangedCallbacks;

            private void OnNetworkAvailabilityChanged(EthernetEmacController sender, NetworkAvailabilityChangedEventArgs e) => this.networkAvailabilityChangedCallbacks?.Invoke(this, e);
            private void OnNetworkAddressChanged(EthernetEmacController sender, NetworAddressChangedEventArgs e) => this.networkAddressChangedCallbacks?.Invoke(this, e);

            public event NetworkAvailabilityEventHandler NetworkAvailabilityChanged {
                add {
                    if (this.networkAvailabilityChangedCallbacks == null)
                        this.Provider.NetworkAvailabilityChanged += this.OnNetworkAvailabilityChanged;

                    this.networkAvailabilityChangedCallbacks += value;
                }
                remove {
                    this.networkAvailabilityChangedCallbacks -= value;

                    if (this.networkAvailabilityChangedCallbacks == null)
                        this.Provider.NetworkAvailabilityChanged -= this.OnNetworkAvailabilityChanged;
                }
            }

            public event NetworkAddressEventHandler NetworkAddressChanged {
                add {
                    if (this.networkAddressChangedCallbacks == null)
                        this.Provider.NetworkAddressChanged += this.OnNetworkAddressChanged;

                    this.networkAddressChangedCallbacks += value;
                }
                remove {
                    this.networkAddressChangedCallbacks -= value;

                    if (this.networkAddressChangedCallbacks == null)
                        this.Provider.NetworkAddressChanged -= this.OnNetworkAddressChanged;
                }
            }
        }

        namespace Provider {
            public interface IEthernetEmacControllerProvider : IDisposable {
                void Open();
                void Close();
                int ISocketProviderNativeAccept(int socket);
                int ISslStreamProviderNativeAuthenticateAsClient(int socketHandle, string targetHost, X509Certificate certificate, SslProtocols[] sslProtocols);
                int ISslStreamProviderNativeAuthenticateAsServer(int socketHandle, X509Certificate certificate, SslProtocols[] sslProtocols);
                int ISslStreamProviderNativeAvailable(int handle);
                void ISslStreamProviderNativeClose(int handle);
                int ISocketProviderNativeAvailable(int socket);
                void ISocketProviderNativeBind(int socket, SocketAddress address);
                void ISocketProviderNativeClose(int socket);
                bool ISocketProviderNativeConnect(int socket, SocketAddress address);
                int ISocketProviderNativeCreate(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
                void ISocketProviderNativeGetLocalAddress(int socket, out SocketAddress address);
                void ISocketProviderNativeGetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);
                void ISocketProviderNativeGetRemoteAddress(int socket, out SocketAddress address);
                void ISocketProviderNativeListen(int socket, int backlog);
                bool ISocketProviderNativePoll(int socket, int microSeconds, SelectMode mode);
                int ISslStreamProviderNativeRead(int handle, byte[] buffer, int offset, int count, int timeout);
                int ISocketProviderNativeReceive(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout);
                int ISocketProviderNativeReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, ref SocketAddress address);
                int ISocketProviderNativeSend(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout);
                int ISocketProviderNativeSendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, SocketAddress address);
                void ISocketProviderNativeSetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);
                int ISslStreamProviderNativeWrite(int handle, byte[] buffer, int offset, int count, int timeout);
                void IDnsProviderNativeGetHostByName(string name, out long address);
                void NativeGetPhysicalAddress(out byte[] ip);
                void EnableStaticIP(string ipAddress, string subnetMask, string gatewayAddress);
                void EnableStaticDns(string[] dnsAddresses);
                bool IsDhcpEnabled();
                void SetDhcp(bool enable);

                event NetworkAvailabilityEventHandler NetworkAvailabilityChanged;
                event NetworkAddressEventHandler NetworkAddressChanged;

            }

            public sealed class EthernetEmacControllerApiWrapper : IEthernetEmacControllerProvider {
                private readonly IntPtr impl;
                private readonly NativeEventDispatcher networkAvailabilityChangedDispatcher;
                private readonly NativeEventDispatcher networkAddressChangedDispatcher;

                private NetworkAvailabilityEventHandler networkAvailabilityChangedCallbacks;
                private NetworkAddressEventHandler networkAddressChangedCallbacks;
                public Api Api { get; }

                public EthernetEmacControllerApiWrapper(Api api) {
                    this.Api = api;

                    this.impl = api.Implementation;

                    this.Acquire();

                    this.networkAvailabilityChangedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.EthernetEmac.NetworkAvailabilityChanged");
                    this.networkAddressChangedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.EthernetEmac.NetworkAddressChanged");

                    this.networkAvailabilityChangedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.networkAvailabilityChangedCallbacks?.Invoke(null, new NetworkAvailabilityChangedEventArgs(d0 != 0, ts)); };
                    this.networkAddressChangedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.networkAddressChangedCallbacks?.Invoke(null, new NetworAddressChangedEventArgs((uint)d0, ts)); };
                }

                public void Dispose() => this.Release();

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void Acquire();

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void Release();

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void Open();

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void Close();

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern int ISocketProviderNativeAccept(int socket);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern int ISslStreamProviderNativeAuthenticateAsClient(int socketHandle, string targetHost, X509Certificate certificate, SslProtocols[] sslProtocols);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern int ISslStreamProviderNativeAuthenticateAsServer(int socketHandle, X509Certificate certificate, SslProtocols[] sslProtocols);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern int ISslStreamProviderNativeAvailable(int handle);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void ISslStreamProviderNativeClose(int handle);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern int ISocketProviderNativeAvailable(int socket);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void ISocketProviderNativeBind(int socket, SocketAddress address);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void ISocketProviderNativeClose(int socket);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern bool ISocketProviderNativeConnect(int socket, SocketAddress address);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern int ISocketProviderNativeCreate(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void ISocketProviderNativeGetLocalAddress(int socket, out SocketAddress address);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void ISocketProviderNativeGetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void ISocketProviderNativeGetRemoteAddress(int socket, out SocketAddress address);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void ISocketProviderNativeListen(int socket, int backlog);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern bool ISocketProviderNativePoll(int socket, int microSeconds, SelectMode mode);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern int ISslStreamProviderNativeRead(int handle, byte[] buffer, int offset, int count, int timeout);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern int ISocketProviderNativeReceive(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern int ISocketProviderNativeReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, ref SocketAddress address);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern int ISocketProviderNativeSend(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern int ISocketProviderNativeSendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, SocketAddress address);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void ISocketProviderNativeSetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern int ISslStreamProviderNativeWrite(int handle, byte[] buffer, int offset, int count, int timeout);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void IDnsProviderNativeGetHostByName(string name, out long address);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void NativeGetPhysicalAddress(out byte[] ip);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void EnableStaticIP(string ipAddress, string subnetMask, string gatewayAddress);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void EnableStaticDns(string[] dnsAddresses);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern bool IsDhcpEnabled();

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern void SetDhcp(bool enable);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern bool SetNetworkAvailabilityChangedHandler(bool enable);

                [MethodImpl(MethodImplOptions.InternalCall)]
                public extern bool SetNetworkAddressChangedHandler(bool enable);

                public event NetworkAvailabilityEventHandler NetworkAvailabilityChanged {
                    add {
                        if (this.networkAvailabilityChangedCallbacks == null) {
                            this.SetNetworkAvailabilityChangedHandler(true);
                        }

                        this.networkAvailabilityChangedCallbacks += value;
                    }
                    remove {
                        this.networkAvailabilityChangedCallbacks -= value;

                        if (this.networkAvailabilityChangedCallbacks == null) {
                            this.SetNetworkAvailabilityChangedHandler(false);
                        }
                    }
                }

                public event NetworkAddressEventHandler NetworkAddressChanged {
                    add {
                        if (this.networkAddressChangedCallbacks == null) {
                            this.SetNetworkAddressChangedHandler(true);
                        }

                        this.networkAddressChangedCallbacks += value;
                    }
                    remove {
                        this.networkAddressChangedCallbacks -= value;

                        if (this.networkAddressChangedCallbacks == null) {
                            this.SetNetworkAddressChangedHandler(false);
                        }
                    }
                }
            }
        }

        // Event
        public sealed class NetworkAvailabilityChangedEventArgs {
            public bool IsAvailable { get; }
            public DateTime Timestamp { get; }

            internal NetworkAvailabilityChangedEventArgs(bool isAvailable, DateTime timestamp) {
                this.IsAvailable = isAvailable;
                this.Timestamp = timestamp;
            }
        }

        public sealed class NetworAddressChangedEventArgs {
            public PhysicalAddress IpAddress { get; }
            public DateTime Timestamp { get; }

            internal NetworAddressChangedEventArgs(uint address, DateTime timestamp) {
                var ip = new byte[4];
                ip[0] = (byte)(address >> 0);
                ip[1] = (byte)(address >> 8);
                ip[2] = (byte)(address >> 16);
                ip[3] = (byte)(address >> 24);

                this.IpAddress = new PhysicalAddress(ip);
                this.Timestamp = timestamp;
            }
        }

        public delegate void NetworkAvailabilityEventHandler(EthernetEmacController sender, NetworkAvailabilityChangedEventArgs e);
        public delegate void NetworkAddressEventHandler(EthernetEmacController sender, NetworAddressChangedEventArgs e);
    */
}
