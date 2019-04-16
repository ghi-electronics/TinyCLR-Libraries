using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Network.Provider;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Network {
    public sealed class NetworkController : IDisposable {
        private readonly Hashtable netifSockets;

        public static NetworkController DefaultController { get; private set; }
        public INetworkControllerProvider Provider { get; }

        public NetworkController(INetworkControllerProvider provider) {
            this.Provider = provider;

            this.netifSockets = new Hashtable();
        }

        ~NetworkController() => this.Dispose();

        public void Dispose() => GC.SuppressFinalize(this);


        public static NetworkController GetDefault() => Api.GetDefaultFromCreator(ApiType.NetworkController) is NetworkController c ? c : NetworkController.FromName(Api.GetDefaultName(ApiType.NetworkController));
        public static NetworkController FromName(string name) => NetworkController.FromProvider(new NetworkControllerApiWrapper(Api.Find(name, ApiType.NetworkController)));
        public static NetworkController FromProvider(INetworkControllerProvider provider) => new NetworkController(provider);

        public void SetAsDefaultController() {
            //Socket, DNS, SSLStream will use this member instead of looking into NetworkInterface
            //We'll need to add an InternalsVisibleTo since we don't want it public
            NetworkController.DefaultController = this;

            Socket.DefaultProvider = this.Provider;
        }

        public void Enable() => this.Provider.Enable();
        public void Disable() => this.Provider.Disable();
        public void SetConfiguration(NetworkControllerSettings controllerSettings, NetworkCommunicationInterfaceSettings communicationInterfaceSettings) => this.Provider.SetConfiguration(controllerSettings, communicationInterfaceSettings);

        private NetworkAvailabilityEventHandler networkAvailabilityChangedCallbacks;
        private NetworkAddressEventHandler networkAddressChangedCallbacks;

        private void OnNetworkAvailabilityChanged(NetworkController sender, NetworkAvailabilityChangedEventArgs e) => this.networkAvailabilityChangedCallbacks?.Invoke(this, e);
        private void OnNetworkAddressChanged(NetworkController sender, NetworAddressChangedEventArgs e) => this.networkAddressChangedCallbacks?.Invoke(this, e);

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

    public enum NetworkControllerType {
        Ethernet = 0,
        WiFi = 1,
    }

    public class NetworkControllerSettings {
        public IPAddress ipaddr;
        public IPAddress subnetmask;
        public IPAddress gateway;
        public IPAddress[] dnsServer;

        public byte[] macAddressBuffer;

        public bool useDhcp;
        public bool useDynamicDns;
    }

    public class EthernetNetworkControllerSettings : NetworkControllerSettings {

    }

    public class WiFiNetworkControllerSettings : NetworkControllerSettings {
        public string Ssid { get; set; }
        public string Password { get; set; }
    }

    public enum NetworkCommunicationInterface {
        BuiltIn = 0,
        Spi = 1,
        I2c = 2,
    }

    public class NetworkCommunicationInterfaceSettings {

    }

    public class BuiltInNetworkCommunicationInterfaceSettings : NetworkCommunicationInterfaceSettings {

    }

    public class SpiNetworkCommunicationInterfaceSettings : NetworkCommunicationInterfaceSettings {
        public string ApiName { get; set; }
        public SpiConnectionSettings Settings { get; set; }
    }



    public class I2cNetworkCommunicationInterfaceSettings : NetworkCommunicationInterfaceSettings {
        public string ApiName { get; set; }
        public I2cConnectionSettings Settings { get; set; }

    }

    namespace Provider {
        public interface INetworkControllerProvider : IDisposable, GHIElectronics.TinyCLR.Networking.INetworkProvider {
            NetworkControllerSettings ControllerSetting { get; }
            NetworkControllerType ControllerType { get; }
            NetworkCommunicationInterface CommunicationInterface { get; }

            void Enable();
            void Disable();
            void SetConfiguration(NetworkControllerSettings controllerSettings, NetworkCommunicationInterfaceSettings communicationInterfaceSettings);
            event NetworkAvailabilityEventHandler NetworkAvailabilityChanged;
            event NetworkAddressEventHandler NetworkAddressChanged;
        }

        public sealed class NetworkControllerApiWrapper : INetworkControllerProvider, IApiImplementation {
            private readonly IntPtr impl;

            private readonly NativeEventDispatcher networkAvailabilityChangedDispatcher;
            private readonly NativeEventDispatcher networkAddressChangedDispatcher;

            private NetworkAvailabilityEventHandler networkAvailabilityChangedCallbacks;
            private NetworkAddressEventHandler networkAddressChangedCallbacks;

            public Api Api { get; }

            IntPtr IApiImplementation.Implementation => this.impl;

            public NetworkControllerApiWrapper(Api api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.networkAvailabilityChangedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.Devices.Network.NetworkAvailabilityChanged");
                this.networkAddressChangedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.Devices.Network.NetworkAddressChanged");

                this.networkAvailabilityChangedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.networkAvailabilityChangedCallbacks?.Invoke(null, new NetworkAvailabilityChangedEventArgs(d0 != 0, ts)); };
                this.networkAddressChangedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.networkAddressChangedCallbacks?.Invoke(null, new NetworAddressChangedEventArgs((uint)d0, ts)); };
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            public void SetConfiguration(NetworkControllerSettings controllerSettings, NetworkCommunicationInterfaceSettings communicationInterfaceSettings) {
                switch (this.CommunicationInterface) {
                    case NetworkCommunicationInterface.BuiltIn when communicationInterfaceSettings is BuiltInNetworkCommunicationInterfaceSettings bcfg:
                        this.SetInterfaceSettings(bcfg);
                        break;

                    case NetworkCommunicationInterface.Spi when communicationInterfaceSettings is SpiNetworkCommunicationInterfaceSettings scfg:
                        this.SetInterfaceSettings(scfg);
                        break;

                    case NetworkCommunicationInterface.I2c when communicationInterfaceSettings is I2cNetworkCommunicationInterfaceSettings icfg:
                        this.SetInterfaceSettings(icfg);
                        break;

                    default:
                        throw new ArgumentException("Must pass an instance whose type matches the communication interface type.");
                }

                switch (this.ControllerType) {
                    case NetworkControllerType.Ethernet when controllerSettings is EthernetNetworkControllerSettings ecfg:
                        this.SetControllerSettings(ecfg);
                        break;

                    case NetworkControllerType.WiFi when controllerSettings is WiFiNetworkControllerSettings wcfg:
                        this.SetControllerSettings(wcfg);
                        break;

                    default:
                        throw new ArgumentException("Must pass an instance whose type matches the controller type.");
                }
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetControllerSettings(EthernetNetworkControllerSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetControllerSettings(WiFiNetworkControllerSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetInterfaceSettings(BuiltInNetworkCommunicationInterfaceSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetInterfaceSettings(SpiNetworkCommunicationInterfaceSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetInterfaceSettings(I2cNetworkCommunicationInterfaceSettings settings);

            public extern NetworkControllerSettings ControllerSetting { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern NetworkControllerType ControllerType { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern NetworkCommunicationInterface CommunicationInterface { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            //DNS
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetHostByName(string name, out string canonicalName, out SocketAddress[] addresses);

            //Socket
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Bind(int socket, SocketAddress address);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Send(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout);

            //SSL
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int AuthenticateAsClient(int socketHandle, string targetHost, X509Certificate certificate, SslProtocols[] sslProtocols);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Close(int socket);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Listen(int socket, int backlog);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Accept(int socket);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Connect(int socket, SocketAddress address);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Available(int socket);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool Poll(int socket, int microSeconds, SelectMode mode);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Receive(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int SendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, SocketAddress address);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int ReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, ref SocketAddress address);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetRemoteAddress(int socket, out SocketAddress address);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetLocalAddress(int socket, out SocketAddress address);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int AuthenticateAsServer(int socketHandle, X509Certificate certificate, SslProtocols[] sslProtocols);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Read(int handle, byte[] buffer, int offset, int count, int timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Write(int handle, byte[] buffer, int offset, int count, int timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetNetworkAvailabilityChangedHandler(bool enable);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetNetworkAddressChangedHandler(bool enable);

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
        public IPAddress IpAddress { get; }
        public DateTime Timestamp { get; }

        internal NetworAddressChangedEventArgs(uint address, DateTime timestamp) {
            var ip = new byte[4];
            ip[0] = (byte)(address >> 0);
            ip[1] = (byte)(address >> 8);
            ip[2] = (byte)(address >> 16);
            ip[3] = (byte)(address >> 24);

            this.IpAddress = new IPAddress(ip);
            this.Timestamp = timestamp;
        }
    }

    public delegate void NetworkAvailabilityEventHandler(NetworkController sender, NetworkAvailabilityChangedEventArgs e);
    public delegate void NetworkAddressEventHandler(NetworkController sender, NetworAddressChangedEventArgs e);


}
