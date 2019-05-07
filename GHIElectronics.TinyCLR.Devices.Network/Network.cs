using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Network.Provider;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Networking;

namespace GHIElectronics.TinyCLR.Devices.Network {
    public delegate void NetworkLinkConnectedChangedEventHandler(NetworkController sender, NetworkLinkConnectedChangedEventArgs e);
    public delegate void NetworkAddressChangedEventHandler(NetworkController sender, NetworkAddressChangedEventArgs e);

    public sealed class NetworkLinkConnectedChangedEventArgs : EventArgs {
        public bool Connected { get; }
        public DateTime Timestamp { get; }

        internal NetworkLinkConnectedChangedEventArgs(bool connected, DateTime timestamp) {
            this.Connected = connected;
            this.Timestamp = timestamp;
        }
    }

    public sealed class NetworkAddressChangedEventArgs : EventArgs {
        public DateTime Timestamp { get; }

        internal NetworkAddressChangedEventArgs(DateTime timestamp) => this.Timestamp = timestamp;
    }

    public sealed class NetworkController : IDisposable {
        private NetworkLinkConnectedChangedEventHandler networkLinkConnectedChangedCallbacks;
        private NetworkAddressChangedEventHandler networkAddressChangedCallbacks;

        public static NetworkController DefaultController { get; private set; }

        public INetworkControllerProvider Provider { get; }

        private NetworkController(INetworkControllerProvider provider) => this.Provider = provider;

        public static NetworkController GetDefault() => Api.GetDefaultFromCreator(ApiType.NetworkController) is NetworkController c ? c : NetworkController.FromName(Api.GetDefaultName(ApiType.NetworkController));
        public static NetworkController FromName(string name) => NetworkController.FromProvider(new NetworkControllerApiWrapper(Api.Find(name, ApiType.NetworkController)));
        public static NetworkController FromProvider(INetworkControllerProvider provider) => new NetworkController(provider);

        public NetworkInterfaceSettings ActiveInterfaceSettings { get; private set; }
        public NetworkCommunicationInterfaceSettings ActiveCommunicationInterfaceSettings { get; private set; }

        public NetworkInterfaceType InterfaceType => this.Provider.InterfaceType;
        public NetworkCommunicationInterface CommunicationInterface => this.Provider.CommunicationInterface;

        public void Dispose() => this.Provider.Dispose();

        public void Enable() => this.Provider.Enable();
        public void Disable() => this.Provider.Disable();

        public bool GetLinkConnected() => this.Provider.GetLinkConnected();
        public NetworkIPProperties GetIPProperties() => this.Provider.GetIPProperties();
        public NetworkInterfaceProperties GetInterfaceProperties() => this.Provider.GetInterfaceProperties();

        public void SetInterfaceSettings(NetworkInterfaceSettings settings) {
            this.Provider.SetInterfaceSettings(settings);

            this.ActiveInterfaceSettings = settings;
        }

        public void SetCommunicationInterfaceSettings(NetworkCommunicationInterfaceSettings settings) {
            this.Provider.SetCommunicationInterfaceSettings(settings);

            this.ActiveCommunicationInterfaceSettings = settings;
        }

        public void SetAsDefaultController() {
            NetworkController.DefaultController = this;

            Socket.DefaultProvider = this.Provider;
        }

        private void OnNetworkLinkConnectedChanged(NetworkController sender, NetworkLinkConnectedChangedEventArgs e) => this.networkLinkConnectedChangedCallbacks?.Invoke(this, e);
        private void OnNetworkAddressChanged(NetworkController sender, NetworkAddressChangedEventArgs e) => this.networkAddressChangedCallbacks?.Invoke(this, e);

        public event NetworkLinkConnectedChangedEventHandler NetworkLinkConnectedChanged {
            add {
                if (this.networkLinkConnectedChangedCallbacks == null)
                    this.Provider.NetworkLinkConnectedChanged += this.OnNetworkLinkConnectedChanged;

                this.networkLinkConnectedChangedCallbacks += value;
            }
            remove {
                this.networkLinkConnectedChangedCallbacks -= value;

                if (this.networkLinkConnectedChangedCallbacks == null)
                    this.Provider.NetworkLinkConnectedChanged -= this.OnNetworkLinkConnectedChanged;
            }
        }

        public event NetworkAddressChangedEventHandler NetworkAddressChanged {
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

    public class NetworkIPProperties {
        public IPAddress Address { get; }
        public IPAddress SubnetMask { get; }
        public IPAddress GatewayAddress { get; }
        public IPAddress[] DnsAddresses { get; }
    }

    public class NetworkInterfaceProperties {
        public byte[] MacAddress { get; }

        public EthernetNetworkInterfaceProperties GetEthernetProperties() => this as EthernetNetworkInterfaceProperties;
        public WiFiNetworkInterfaceProperties GetWiFiProperties() => this as WiFiNetworkInterfaceProperties;
    }

    public class EthernetNetworkInterfaceProperties : NetworkInterfaceProperties {

    }

    public class WiFiNetworkInterfaceProperties : NetworkInterfaceProperties {

    }

    public enum NetworkInterfaceType {
        Ethernet = 0,
        WiFi = 1,
    }

    public class NetworkInterfaceSettings {
        public IPAddress Address { get; set; }
        public IPAddress SubnetMask { get; set; }
        public IPAddress GatewayAddress { get; set; }
        public IPAddress[] DnsAddresses { get; set; }
        public byte[] MacAddress { get; set; }
        public bool IsDhcpEnabled { get; set; } = true;
        public bool IsDynamicDnsEnabled { get; set; } = true;
    }

    public class EthernetNetworkInterfaceSettings : NetworkInterfaceSettings {

    }

    public class WiFiNetworkInterfaceSettings : NetworkInterfaceSettings {
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
        public int ResetPin { get; set; }
        public int InterruptPin { get; set; }
        public bool InterruptActiveState { get; set; }
        public GpioPinEdge InterruptEdge { get; set; }
        public GpioPinDriveMode InterruptDriveMode { get; set; }
    }

    public class I2cNetworkCommunicationInterfaceSettings : NetworkCommunicationInterfaceSettings {
        public string ApiName { get; set; }
        public I2cConnectionSettings Settings { get; set; }
    }

    namespace Provider {
        public interface INetworkControllerProvider : IDisposable, INetworkProvider {
            NetworkInterfaceType InterfaceType { get; }
            NetworkCommunicationInterface CommunicationInterface { get; }

            void Enable();
            void Disable();

            bool GetLinkConnected();
            NetworkIPProperties GetIPProperties();
            NetworkInterfaceProperties GetInterfaceProperties();

            void SetInterfaceSettings(NetworkInterfaceSettings settings);
            void SetCommunicationInterfaceSettings(NetworkCommunicationInterfaceSettings settings);

            event NetworkLinkConnectedChangedEventHandler NetworkLinkConnectedChanged;
            event NetworkAddressChangedEventHandler NetworkAddressChanged;
        }

        public sealed class NetworkControllerApiWrapper : INetworkControllerProvider {
            private readonly IntPtr impl;
            private readonly NativeEventDispatcher networkLinkConnectedChangedDispatcher;
            private readonly NativeEventDispatcher networkAddressChangedDispatcher;
            private NetworkLinkConnectedChangedEventHandler networkLinkConnectedChangedCallbacks;
            private NetworkAddressChangedEventHandler networkAddressChangedCallbacks;

            public Api Api { get; }

            public NetworkControllerApiWrapper(Api api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();

                this.networkLinkConnectedChangedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Network.NetworkLinkConnectedChanged");
                this.networkAddressChangedDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Network.NetworkAddressChanged");

                this.networkLinkConnectedChangedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.networkLinkConnectedChangedCallbacks?.Invoke(null, new NetworkLinkConnectedChangedEventArgs(d0 != 0, ts)); };
                this.networkAddressChangedDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => { if (this.Api.Name == apiName) this.networkAddressChangedCallbacks?.Invoke(null, new NetworkAddressChangedEventArgs(ts)); };
            }

            public event NetworkLinkConnectedChangedEventHandler NetworkLinkConnectedChanged {
                add {
                    if (this.networkLinkConnectedChangedCallbacks == null)
                        this.SetNetworkLinkConnectedChangedEventEnabled(true);

                    this.networkLinkConnectedChangedCallbacks += value;
                }
                remove {
                    this.networkLinkConnectedChangedCallbacks -= value;

                    if (this.networkLinkConnectedChangedCallbacks == null)
                        this.SetNetworkLinkConnectedChangedEventEnabled(false);
                }
            }

            public event NetworkAddressChangedEventHandler NetworkAddressChanged {
                add {
                    if (this.networkAddressChangedCallbacks == null)
                        this.SetNetworkAddressChangedEventEnabled(true);

                    this.networkAddressChangedCallbacks += value;
                }
                remove {
                    this.networkAddressChangedCallbacks -= value;

                    if (this.networkAddressChangedCallbacks == null)
                        this.SetNetworkAddressChangedEventEnabled(false);
                }
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetNetworkLinkConnectedChangedEventEnabled(bool enabled);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetNetworkAddressChangedEventEnabled(bool enabled);

            public extern NetworkInterfaceType InterfaceType { [MethodImpl(MethodImplOptions.InternalCall)] get; }
            public extern NetworkCommunicationInterface CommunicationInterface { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            public void SetInterfaceSettings(NetworkInterfaceSettings settings) {
                switch (this.InterfaceType) {
                    case NetworkInterfaceType.Ethernet when settings is EthernetNetworkInterfaceSettings enis:
                        this.SetInterfaceSettings(enis);
                        break;

                    case NetworkInterfaceType.WiFi when settings is WiFiNetworkInterfaceSettings wnis:
                        this.SetInterfaceSettings(wnis);
                        break;

                    default:
                        throw new ArgumentException("Must pass an instance whose type matches the interface type.");
                }
            }

            public void SetCommunicationInterfaceSettings(NetworkCommunicationInterfaceSettings settings) {
                switch (this.CommunicationInterface) {
                    case NetworkCommunicationInterface.BuiltIn when settings is BuiltInNetworkCommunicationInterfaceSettings bcis:
                        this.SetCommunicationInterfaceSettings(bcis);
                        break;

                    case NetworkCommunicationInterface.Spi when settings is SpiNetworkCommunicationInterfaceSettings scis:
                        this.SetCommunicationInterfaceSettings(scis);
                        break;

                    case NetworkCommunicationInterface.I2c when settings is I2cNetworkCommunicationInterfaceSettings icis:
                        this.SetCommunicationInterfaceSettings(icis);
                        break;

                    default:
                        throw new ArgumentException("Must pass an instance whose type matches the communication interface type.");
                }
            }

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetInterfaceSettings(EthernetNetworkInterfaceSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetInterfaceSettings(WiFiNetworkInterfaceSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetCommunicationInterfaceSettings(BuiltInNetworkCommunicationInterfaceSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetCommunicationInterfaceSettings(SpiNetworkCommunicationInterfaceSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetCommunicationInterfaceSettings(I2cNetworkCommunicationInterfaceSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool GetLinkConnected();
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern NetworkIPProperties GetIPProperties();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern NetworkInterfaceProperties GetInterfaceProperties();


            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Close(int socket);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Bind(int socket, SocketAddress address);

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
            public extern int Send(int socket, byte[] buffer, int offset, int count, SocketFlags flags);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Receive(int socket, byte[] buffer, int offset, int count, SocketFlags flags);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int SendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, SocketAddress address);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int ReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, ref SocketAddress address);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetRemoteAddress(int socket, out SocketAddress address);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetLocalAddress(int socket, out SocketAddress address);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);


            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int AuthenticateAsClient(int socketHandle, string targetHost, X509Certificate certificate, SslProtocols[] sslProtocols);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int AuthenticateAsServer(int socketHandle, X509Certificate certificate, SslProtocols[] sslProtocols);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int SecureRead(int handle, byte[] buffer, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int SecureWrite(int handle, byte[] buffer, int offset, int count);


            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetHostByName(string name, out string canonicalName, out SocketAddress[] addresses);
        }
    }
}
