using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Network.Provider;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Networking;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.Network\Network.cs.
// Bodies on Desktop are safe no-ops:
//   * Enable/Disable/SetSettings/etc. — empty.
//   * GetIPProperties / GetInterfaceProperties — return non-null instances
//     populated with safe defaults (IPAddress.Any, empty MAC).
//   * Events stored but never raised (no native interrupt source).
//   * GetDefault() routes through FromName("Simulator").
//   * DhcpServer (WiFi AP path) is a no-op stub — Start/Stop do nothing.
namespace GHIElectronics.TinyCLR.Devices.Network {
    public delegate void NetworkLinkConnectedChangedEventHandler(NetworkController sender, NetworkLinkConnectedChangedEventArgs e);
    public delegate void NetworkAddressChangedEventHandler(NetworkController sender, NetworkAddressChangedEventArgs e);

    public class NetworkLinkConnectedChangedEventArgs : EventArgs {
        public bool Connected { get; }
        public DateTime Timestamp { get; }

        internal NetworkLinkConnectedChangedEventArgs(bool connected, DateTime timestamp) {
            this.Connected = connected;
            this.Timestamp = timestamp;
        }
    }

    public class NetworkAddressChangedEventArgs : EventArgs {
        public DateTime Timestamp { get; }

        internal NetworkAddressChangedEventArgs(DateTime timestamp) => this.Timestamp = timestamp;
    }

    public class NetworkController : IDisposable {
        private NetworkLinkConnectedChangedEventHandler networkLinkConnectedChangedCallbacks;
        private NetworkAddressChangedEventHandler networkAddressChangedCallbacks;

        public static NetworkController DefaultController { get; private set; }

        public INetworkControllerProvider Provider { get; }

        private NetworkController(INetworkControllerProvider provider) => this.Provider = provider;

        public static NetworkController GetDefault() => NetworkController.FromName("Simulator");
        public static NetworkController FromName(string name) => NetworkController.FromProvider(new NetworkControllerApiWrapper(NativeApi.Find(name, NativeApiType.NetworkController)));
        public static NetworkController FromProvider(INetworkControllerProvider provider) => new NetworkController(provider);

        public NetworkInterfaceSettings ActiveInterfaceSettings { get; private set; }
        public NetworkCommunicationInterfaceSettings ActiveCommunicationInterfaceSettings { get; private set; }

        public NetworkInterfaceType InterfaceType => this.Provider.InterfaceType;
        public NetworkCommunicationInterface CommunicationInterface => this.Provider.CommunicationInterface;

        public bool IsEnable => this.enabled;

        internal bool enabled;

        public void Dispose() {
            this.Provider.Dispose();
            this.enabled = false;
        }

        public void Enable() {
            this.Provider.Enable();
            this.enabled = true;
        }

        public void EnableAsync() {
            this.Provider.Enable();
            this.enabled = true;
        }

        public void Disable() {
            this.Provider.Disable();
            this.enabled = false;
        }

        public void Suspend() => this.Provider.Suspend();
        public void Resume() => this.Provider.Resume();

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
            // Socket.DefaultProvider would be set on TinyCLR; on Desktop System.Net.Sockets.Socket
            // is the framework type (forwarded). No equivalent setter — skip.
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
        public IPAddress Address { get; internal set; } = IPAddress.Any;
        public IPAddress SubnetMask { get; internal set; } = IPAddress.Any;
        public IPAddress GatewayAddress { get; internal set; } = IPAddress.Any;
        public IPAddress[] DnsAddresses { get; internal set; } = new IPAddress[0];
    }

    public class NetworkInterfaceProperties {
        public byte[] MacAddress { get; internal set; } = new byte[6];

        public EthernetNetworkInterfaceProperties GetEthernetProperties() => this as EthernetNetworkInterfaceProperties;
        public WiFiNetworkInterfaceProperties GetWiFiProperties() => this as WiFiNetworkInterfaceProperties;
        public PppNetworkInterfaceProperties GetPppProperties() => this as PppNetworkInterfaceProperties;
    }

    public class EthernetNetworkInterfaceProperties : NetworkInterfaceProperties { }
    public class WiFiNetworkInterfaceProperties : NetworkInterfaceProperties { }
    public class PppNetworkInterfaceProperties : NetworkInterfaceProperties { }

    public enum NetworkInterfaceType {
        Ethernet = 0,
        WiFi = 1,
        Ppp = 2,
    }

    public class NetworkInterfaceSettings {
        public IPAddress Address { get; set; }
        public IPAddress SubnetMask { get; set; }
        public IPAddress GatewayAddress { get; set; }
        public IPAddress[] DnsAddresses { get; set; }
        public byte[] MacAddress { get; set; }
        public bool DhcpEnable { get; set; } = true;
        public bool DynamicDnsEnable { get; set; } = true;
        public byte[] TlsEntropy { get; set; }
        public bool MulticastDnsEnable { get; set; } = false;
    }

    public class EthernetNetworkInterfaceSettings : NetworkInterfaceSettings { }

    public enum WiFiMode {
        Station = 0,
        AccessPoint = 1
    }

    public enum WiFiSecurityMode {
        Open,
        WEP,
        WPA_WPA2
    }

    public class WiFiNetworkInterfaceSettings : NetworkInterfaceSettings {
        public string Ssid { get; set; }
        public string Password { get; set; }
        public uint Channel { get; set; } = 1;

        // Internal fields kept for binary compatibility with impl's internals,
        // even though the shim never reads them.
        internal INetworkControllerProvider provider;
        internal NetworkController networkController;

        public delegate void AccessPointClientConnectionChangedEventHandler(NetworkController sender, IPAddress clientAddress, string macAddress);
        public event AccessPointClientConnectionChangedEventHandler AccessPointClientConnectionChanged;

        public WiFiMode Mode { get; set; }

        public WiFiSecurityMode SecurityMode { get; set; }
    }

    public enum PppAuthenticationType {
        None = 0,
        Any = 1,
        Pap = 2,
        Chap = 3,
    }

    public class PppNetworkInterfaceSettings : NetworkInterfaceSettings {
        public string Username { get; set; }
        public string Password { get; set; }
        public PppAuthenticationType AuthenticationType { get; set; }
    }

    public enum NetworkCommunicationInterface {
        BuiltIn = 0,
        Spi = 1,
        Uart = 2,
    }

    public class NetworkCommunicationInterfaceSettings { }
    public class BuiltInNetworkCommunicationInterfaceSettings : NetworkCommunicationInterfaceSettings { }

    public class SpiNetworkCommunicationInterfaceSettings : NetworkCommunicationInterfaceSettings {
        public string SpiApiName { get; set; }
        public SpiConnectionSettings SpiSettings { get; set; }

        public string GpioApiName { get; set; }

        public GpioPin ResetPin { get; set; }
        public GpioPinValue ResetActiveState { get; set; }

        public GpioPin InterruptPin { get; set; }
        public GpioPinEdge InterruptEdge { get; set; }
        public GpioPinDriveMode InterruptDriveMode { get; set; }
    }

    public class UartNetworkCommunicationInterfaceSettings : NetworkCommunicationInterfaceSettings {
        public string ApiName { get; set; }

        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public UartParity Parity { get; set; }
        public UartStopBitCount StopBits { get; set; }
        public UartHandshake Handshaking { get; set; }
    }

    namespace Provider {
        public interface INetworkControllerProvider : IDisposable, INetworkProvider {
            NetworkInterfaceType InterfaceType { get; }
            NetworkCommunicationInterface CommunicationInterface { get; }

            void Enable();
            void Disable();

            void Suspend();
            void Resume();

            bool GetLinkConnected();
            bool GetAccessPointClientLinkConnect(WiFiNetworkInterfaceSettings settings);

            NetworkIPProperties GetIPProperties();
            NetworkInterfaceProperties GetInterfaceProperties();

            void SetInterfaceSettings(NetworkInterfaceSettings settings);
            void SetCommunicationInterfaceSettings(NetworkCommunicationInterfaceSettings settings);

            event NetworkLinkConnectedChangedEventHandler NetworkLinkConnectedChanged;
            event NetworkAddressChangedEventHandler NetworkAddressChanged;
        }

        // No-op provider. All methods do nothing or return safe defaults.
        // Properties round-trip via stored fields where impl had a setter;
        // factory methods return non-null instances.
        public sealed class NetworkControllerApiWrapper : INetworkControllerProvider {
            private NetworkInterfaceSettings activeInterfaceSettings;
            private NetworkCommunicationInterfaceSettings activeCommunicationInterfaceSettings;

            public NativeApi Api { get; }

            public NetworkControllerApiWrapper(NativeApi api) => this.Api = api;

            public event NetworkLinkConnectedChangedEventHandler NetworkLinkConnectedChanged;
            public event NetworkAddressChangedEventHandler NetworkAddressChanged;

            public void Dispose() { }

            public void SetNetworkLinkConnectedChangedEventEnabled(bool enabled) { }
            public void SetNetworkAddressChangedEventEnabled(bool enabled) { }

            public NetworkInterfaceType InterfaceType => NetworkInterfaceType.Ethernet;
            public NetworkCommunicationInterface CommunicationInterface => NetworkCommunicationInterface.BuiltIn;

            // Synthesize link-up + address-assigned events so Desktop user
            // code that waits on phyReady / linkReady completes naturally
            // (without timing out or hanging). Subscribers attached BEFORE
            // Enable() — the typical pattern — see both events fire here.
            public void Enable() {
                this.NetworkLinkConnectedChanged?.Invoke(null, new NetworkLinkConnectedChangedEventArgs(true, DateTime.UtcNow));
                this.NetworkAddressChanged?.Invoke(null, new NetworkAddressChangedEventArgs(DateTime.UtcNow));
            }
            public void EnableAsync() => this.Enable();
            public void Disable() {
                this.NetworkLinkConnectedChanged?.Invoke(null, new NetworkLinkConnectedChangedEventArgs(false, DateTime.UtcNow));
            }
            public void Suspend() { }
            public void Resume() { }
            public void SetAsDefault() { }

            public void SetInterfaceSettings(NetworkInterfaceSettings settings) => this.activeInterfaceSettings = settings;
            public void SetCommunicationInterfaceSettings(NetworkCommunicationInterfaceSettings settings) => this.activeCommunicationInterfaceSettings = settings;

            public bool GetLinkConnected() => false;

            // Use the user's static settings if they were applied; otherwise
            // reasonable defaults. This keeps user code that does
            // GetIPProperties().Address.ToString() working without NRE.
            public NetworkIPProperties GetIPProperties() {
                var settings = this.activeInterfaceSettings;
                return new NetworkIPProperties {
                    Address = settings?.Address ?? IPAddress.Any,
                    SubnetMask = settings?.SubnetMask ?? IPAddress.Any,
                    GatewayAddress = settings?.GatewayAddress ?? IPAddress.Any,
                    DnsAddresses = settings?.DnsAddresses ?? new IPAddress[0],
                };
            }

            public NetworkInterfaceProperties GetInterfaceProperties() {
                var settings = this.activeInterfaceSettings;
                return new EthernetNetworkInterfaceProperties {
                    MacAddress = settings?.MacAddress ?? new byte[6],
                };
            }

            // INetworkProvider members (socket layer): all no-op. User code
            // that goes through System.Net.Sockets.Socket on Desktop uses the
            // framework's signed System.dll (via TypeForwardedTo from the
            // Networking shim), NOT through this provider, so these stubs
            // only matter if user code instantiates a TinyCLR socket directly.
            public int Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) => 0;
            public void Close(int socket) { }
            public void Shutdown(int socket, SocketShutdown how) { }
            public void Bind(int socket, SocketAddress address) { }
            public void Listen(int socket, int backlog) { }
            public int Accept(int socket) => 0;
            public void Connect(int socket, SocketAddress address) { }
            public int Available(int socket) => 0;
            public bool Poll(int socket, int microSeconds, SelectMode mode) => false;
            public int Send(int socket, byte[] buffer, int offset, int count, SocketFlags flags) => count;
            public int Receive(int socket, byte[] buffer, int offset, int count, SocketFlags flags) => 0;
            public int SendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, SocketAddress address) => count;
            public int ReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, ref SocketAddress address) => 0;
            public void GetRemoteAddress(int socket, out SocketAddress address) => address = null;
            public void GetLocalAddress(int socket, out SocketAddress address) => address = null;
            public void GetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) { }
            public void SetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) { }
            public int AuthenticateAsClient(int socketHandle, string targetHost, X509Certificate caCertificate, X509Certificate clientCertificate, SslProtocols sslProtocols, SslVerification sslVerification) => 0;
            public int AuthenticateAsServer(int socketHandle, X509Certificate certificate, SslProtocols sslProtocols) => 0;
            public int SecureRead(int handle, byte[] buffer, int offset, int count) => 0;
            public int SecureWrite(int handle, byte[] buffer, int offset, int count) => count;
            public void GetHostByName(string name, out string canonicalName, out SocketAddress[] addresses) {
                canonicalName = name;
                addresses = new SocketAddress[0];
            }
            public bool GetAccessPointClientLinkConnect(WiFiNetworkInterfaceSettings settings) => false;
        }
    }
}
