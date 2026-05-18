using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Network.Provider;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Devices.Uart;
using GHIElectronics.TinyCLR.Native;
using GHIElectronics.TinyCLR.Networking;

namespace GHIElectronics.TinyCLR.Devices.Network {
    /// <summary>Handler signature for <see cref="NetworkController.NetworkLinkConnectedChanged"/>.</summary>
    public delegate void NetworkLinkConnectedChangedEventHandler(NetworkController sender, NetworkLinkConnectedChangedEventArgs e);
    /// <summary>Handler signature for <see cref="NetworkController.NetworkAddressChanged"/>.</summary>
    public delegate void NetworkAddressChangedEventHandler(NetworkController sender, NetworkAddressChangedEventArgs e);

    /// <summary>Arguments for <see cref="NetworkController.NetworkLinkConnectedChanged"/>.</summary>
    public class NetworkLinkConnectedChangedEventArgs : EventArgs {
        /// <summary>True when the physical link is up.</summary>
        public bool Connected { get; }
        /// <summary>Driver-captured time of the transition.</summary>
        public DateTime Timestamp { get; }

        internal NetworkLinkConnectedChangedEventArgs(bool connected, DateTime timestamp) {
            this.Connected = connected;
            this.Timestamp = timestamp;
        }
    }

    /// <summary>Arguments for <see cref="NetworkController.NetworkAddressChanged"/>.</summary>
    public class NetworkAddressChangedEventArgs : EventArgs {
        /// <summary>Driver-captured time of the address change.</summary>
        public DateTime Timestamp { get; }

        internal NetworkAddressChangedEventArgs(DateTime timestamp) => this.Timestamp = timestamp;
    }

    /// <summary>
    /// Represents a network interface — Ethernet, WiFi (station or AP), or PPP.
    /// Configure the interface settings, optionally the underlying communication
    /// interface (built-in MAC, SPI, or UART), then <see cref="Enable"/> the
    /// controller. Subscribe to <see cref="NetworkLinkConnectedChanged"/> and
    /// <see cref="NetworkAddressChanged"/> for status. Use <see cref="SetAsDefaultController"/>
    /// to choose which interface handles outbound traffic when multiple are up.
    /// </summary>
    public class NetworkController : IDisposable {
        private NetworkLinkConnectedChangedEventHandler networkLinkConnectedChangedCallbacks;
        private NetworkAddressChangedEventHandler networkAddressChangedCallbacks;

        /// <summary>The controller most recently selected via <see cref="SetAsDefaultController"/>.</summary>
        public static NetworkController DefaultController { get; private set; }

        /// <summary>The low-level provider backing this controller.</summary>
        public INetworkControllerProvider Provider { get; }

        private NetworkController(INetworkControllerProvider provider) => this.Provider = provider;

        /// <summary>Returns the default network controller for this device.</summary>
        public static NetworkController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.NetworkController) is NetworkController c ? c : NetworkController.FromName(NativeApi.GetDefaultName(NativeApiType.NetworkController));
        /// <summary>Returns a network controller identified by its native API name.</summary>
        public static NetworkController FromName(string name) => NetworkController.FromProvider(new NetworkControllerApiWrapper(NativeApi.Find(name, NativeApiType.NetworkController)));
        /// <summary>Creates a controller from a custom <see cref="INetworkControllerProvider"/>.</summary>
        public static NetworkController FromProvider(INetworkControllerProvider provider) => new NetworkController(provider);

        /// <summary>The settings most recently applied via <see cref="SetInterfaceSettings"/>.</summary>
        public NetworkInterfaceSettings ActiveInterfaceSettings { get; private set; }
        /// <summary>The settings most recently applied via <see cref="SetCommunicationInterfaceSettings"/>.</summary>
        public NetworkCommunicationInterfaceSettings ActiveCommunicationInterfaceSettings { get; private set; }

        /// <summary>Interface type — Ethernet, WiFi, or PPP.</summary>
        public NetworkInterfaceType InterfaceType => this.Provider.InterfaceType;
        /// <summary>Physical bus carrying the interface — built-in MAC, SPI, or UART.</summary>
        public NetworkCommunicationInterface CommunicationInterface => this.Provider.CommunicationInterface;

        /// <summary>True once <see cref="Enable"/> has been called and the controller is still enabled.</summary>
        public bool IsEnable => this.enabled;

        internal bool enabled;

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() {
            this.Provider.Dispose();

            this.enabled = false;
        }

        /// <summary>
        /// Brings the interface up synchronously. Blocks until the PHY/WiFi firmware
        /// is ready; for non-blocking bring-up use <see cref="EnableAsync"/>.
        /// </summary>
        /// <remarks>
        /// Multi-interface coexistence: multiple controllers (e.g. Ethernet and WiFi)
        /// may be enabled at the same time. The native lwIP wrapper registers each as
        /// its own netif and routes outbound traffic by destination netmask
        /// (longest-prefix match). For destinations not covered by any interface's
        /// subnet (e.g. public internet), the controller most recently passed to
        /// <see cref="SetAsDefaultController"/> owns the route.
        /// </remarks>
        public void Enable() {

            this.Provider.Enable();

            this.enabled = true;

            if (this.InterfaceType == NetworkInterfaceType.WiFi) {
                var setting = (WiFiNetworkInterfaceSettings)this.ActiveInterfaceSettings;

                if (setting.Mode == WiFiMode.AccessPoint) {
                    setting.networkController = this;
                    setting.provider = this.Provider;

                    if (setting.DhcpEnable) {
                        setting.dhcpServer.Start(setting);
                    }

                }
            }
        }

        /// <summary>
        /// Non-blocking variant of <see cref="Enable"/>. Returns immediately while
        /// the slow PHY autonegotiation / WiFi firmware boot runs in a native RTOS
        /// task. The interface is NOT ready when this returns — subscribe to
        /// <see cref="NetworkLinkConnectedChanged"/> (link becomes physical-up) and
        /// <see cref="NetworkAddressChanged"/> (DHCP / static IP assigned) to learn
        /// when it is. Call at most once per controller per boot.
        /// </summary>
        public void EnableAsync() {

            if (this.Provider is NetworkControllerApiWrapper wrapper)
                wrapper.EnableAsync();
            else
                this.Provider.Enable();   // fallback for non-native providers

            this.enabled = true;

            if (this.InterfaceType == NetworkInterfaceType.WiFi) {
                var setting = (WiFiNetworkInterfaceSettings)this.ActiveInterfaceSettings;

                if (setting.Mode == WiFiMode.AccessPoint) {
                    setting.networkController = this;
                    setting.provider = this.Provider;

                    if (setting.DhcpEnable) {
                        setting.dhcpServer.Start(setting);
                    }
                }
            }
        }

        /// <summary>Brings the interface down.</summary>
        public void Disable() {
            if (this.InterfaceType == NetworkInterfaceType.WiFi) {
                var setting = (WiFiNetworkInterfaceSettings)this.ActiveInterfaceSettings;

                if (setting.Mode == WiFiMode.AccessPoint) {
                    if (setting.DhcpEnable)
                        setting.dhcpServer.Stop();
                }
            }

            this.Provider.Disable();

            this.enabled = false ;
        }

        /// <summary>Suspends the interface (low-power state with state preserved).</summary>
        public void Suspend() => this.Provider.Suspend();
        /// <summary>Resumes a previously <see cref="Suspend"/>ed interface.</summary>
        public void Resume() => this.Provider.Resume();

        /// <summary>True when the physical link is currently up.</summary>
        public bool GetLinkConnected() => this.Provider.GetLinkConnected();
        /// <summary>Returns the current IP address, subnet, gateway, and DNS servers.</summary>
        public NetworkIPProperties GetIPProperties() => this.Provider.GetIPProperties();
        /// <summary>Returns interface-specific properties (MAC address and friends).</summary>
        public NetworkInterfaceProperties GetInterfaceProperties() => this.Provider.GetInterfaceProperties();

        /// <summary>Applies <see cref="NetworkInterfaceSettings"/> (IP address, DHCP, DNS).</summary>
        public void SetInterfaceSettings(NetworkInterfaceSettings settings) {
            this.Provider.SetInterfaceSettings(settings);

            this.ActiveInterfaceSettings = settings;
        }

        /// <summary>Applies the underlying physical-bus settings (built-in MAC, SPI, or UART).</summary>
        public void SetCommunicationInterfaceSettings(NetworkCommunicationInterfaceSettings settings) {
            this.Provider.SetCommunicationInterfaceSettings(settings);

            this.ActiveCommunicationInterfaceSettings = settings;
        }

        /// <summary>
        /// Makes this controller the default across all layers — managed
        /// <see cref="DefaultController"/>, <see cref="Sockets.Socket.DefaultProvider"/>,
        /// and lwIP's netif_default. The lwIP-level update only fires when the
        /// controller is already enabled, so call after <see cref="Enable"/> to
        /// make this interface own the default route.
        /// </summary>
        public void SetAsDefaultController() {
            NetworkController.DefaultController = this;

            Socket.DefaultProvider = this.Provider;

            if (this.enabled && this.Provider is NetworkControllerApiWrapper wrapper)
                wrapper.SetAsDefault();
        }

        private void OnNetworkLinkConnectedChanged(NetworkController sender, NetworkLinkConnectedChangedEventArgs e) => this.networkLinkConnectedChangedCallbacks?.Invoke(this, e);
        private void OnNetworkAddressChanged(NetworkController sender, NetworkAddressChangedEventArgs e) => this.networkAddressChangedCallbacks?.Invoke(this, e);

        /// <summary>Raised when the physical link goes up or down.</summary>
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

        /// <summary>Raised when the IP address, gateway, or DNS servers change (e.g. on DHCP lease assignment).</summary>
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

    /// <summary>IP-level interface properties.</summary>
    public class NetworkIPProperties {
        /// <summary>Current IP address.</summary>
        public IPAddress Address { get; }
        /// <summary>Subnet mask.</summary>
        public IPAddress SubnetMask { get; }
        /// <summary>Default gateway.</summary>
        public IPAddress GatewayAddress { get; }
        /// <summary>DNS servers in order of preference.</summary>
        public IPAddress[] DnsAddresses { get; }
    }

    /// <summary>Common interface properties. Cast via <see cref="GetEthernetProperties"/> / <see cref="GetWiFiProperties"/> / <see cref="GetPppProperties"/> for transport-specific fields.</summary>
    public class NetworkInterfaceProperties {
        /// <summary>The interface's MAC address (6 bytes for Ethernet/WiFi).</summary>
        public byte[] MacAddress { get; }

        /// <summary>Returns Ethernet-specific properties, or null if this is not an Ethernet interface.</summary>
        public EthernetNetworkInterfaceProperties GetEthernetProperties() => this as EthernetNetworkInterfaceProperties;
        /// <summary>Returns WiFi-specific properties, or null if this is not a WiFi interface.</summary>
        public WiFiNetworkInterfaceProperties GetWiFiProperties() => this as WiFiNetworkInterfaceProperties;
        /// <summary>Returns PPP-specific properties, or null if this is not a PPP interface.</summary>
        public PppNetworkInterfaceProperties GetPppProperties() => this as PppNetworkInterfaceProperties;
    }

    /// <summary>Ethernet-specific interface properties.</summary>
    public class EthernetNetworkInterfaceProperties : NetworkInterfaceProperties {

    }

    /// <summary>WiFi-specific interface properties.</summary>
    public class WiFiNetworkInterfaceProperties : NetworkInterfaceProperties {

    }

    /// <summary>PPP-specific interface properties.</summary>
    public class PppNetworkInterfaceProperties : NetworkInterfaceProperties {

    }

    /// <summary>Transport type of a network interface.</summary>
    public enum NetworkInterfaceType {
        /// <summary>Wired Ethernet.</summary>
        Ethernet = 0,
        /// <summary>WiFi (Station or AP).</summary>
        WiFi = 1,
        /// <summary>Point-to-Point Protocol (e.g. cellular modem).</summary>
        Ppp = 2,
    }

    /// <summary>Common interface settings — IP/DHCP/DNS/MAC. Subclass per transport for additional fields.</summary>
    public class NetworkInterfaceSettings {
        /// <summary>Static IP address (used when <see cref="DhcpEnable"/> is false).</summary>
        public IPAddress Address { get; set; }
        /// <summary>Subnet mask for the static address.</summary>
        public IPAddress SubnetMask { get; set; }
        /// <summary>Default gateway for the static address.</summary>
        public IPAddress GatewayAddress { get; set; }
        /// <summary>DNS servers in order of preference.</summary>
        public IPAddress[] DnsAddresses { get; set; }
        /// <summary>MAC address for the interface (6 bytes).</summary>
        public byte[] MacAddress { get; set; }
        /// <summary>When true, IP/gateway/DNS are obtained via DHCP and the static fields above are ignored.</summary>
        public bool DhcpEnable { get; set; } = true;
        /// <summary>When true, the controller registers a Dynamic-DNS hostname.</summary>
        public bool DynamicDnsEnable { get; set; } = true;
        /// <summary>Seed entropy for the TLS RNG. Optional; if null, the stack supplies a default.</summary>
        public byte[] TlsEntropy { get; set; }
        /// <summary>When true, the controller responds to mDNS / Bonjour queries.</summary>
        public bool MulticastDnsEnable { get; set; } = false;
    }

    /// <summary>Ethernet interface settings.</summary>
    public class EthernetNetworkInterfaceSettings : NetworkInterfaceSettings {

    }

    /// <summary>WiFi role for a WiFi interface.</summary>
    public enum WiFiMode {
        /// <summary>Connect to an external access point.</summary>
        Station = 0,
        /// <summary>Act as an access point that other stations join.</summary>
        AccessPoint = 1
    }

    /// <summary>WiFi security protocol.</summary>
    public enum WiFiSecurityMode {
        /// <summary>No encryption.</summary>
        Open,
        /// <summary>WEP (legacy, insecure).</summary>
        WEP,
        /// <summary>WPA or WPA2.</summary>
        WPA_WPA2
    }

    /// <summary>WiFi-specific interface settings.</summary>
    public class WiFiNetworkInterfaceSettings : NetworkInterfaceSettings {
        /// <summary>Network SSID. In Station mode, the AP to join; in AccessPoint mode, the SSID to broadcast.</summary>
        public string Ssid { get; set; }
        /// <summary>Pre-shared key for WPA/WPA2 / WEP.</summary>
        public string Password { get; set; }
        /// <summary>AP channel (1..13). AccessPoint mode only.</summary>
        public uint Channel { get; set; } = 1;

        internal INetworkControllerProvider provider;
        internal NetworkController networkController;

        /// <summary>Handler signature for <see cref="AccessPointClientConnectionChanged"/>.</summary>
        public delegate void AccessPointClientConnectionChangedEventHandler(NetworkController sender, IPAddress clientAddress, string macAddress);
        /// <summary>Raised when a station connects to this access point.</summary>
        public event AccessPointClientConnectionChangedEventHandler AccessPointClientConnectionChanged;

        /// <summary>WiFi role — Station or AccessPoint.</summary>
        public WiFiMode Mode {
            get => this.mode;
            set {

                this.mode = value;

                if (this.mode == WiFiMode.AccessPoint && this.DhcpEnable && this.dhcpServer == null) {
                    this.dhcpServer = new DhcpServer();
                }
            }
        }
        
        private WiFiMode mode;        

        internal DhcpServer dhcpServer;

        /// <summary>Encryption protocol for the WiFi connection.</summary>
        public WiFiSecurityMode SecurityMode { get; set; }

        internal class DhcpServer {
            enum Port {
                Source = 67,
                Destination = 68,
            }

            enum MessageType {
                Discovery = 1,
                Offer = 2,
                Request = 3,
                Acknowledge = 5,
            }

            enum MessageOption {
                SubnetMask = 1,
                Router = 3,
                DomainNameServers = 6,
                DomainName = 15,
                IPAddressLeaseTime = 51,
                DHCPMessageType = 53,
                DHCPServerIdentifier = 54,
                ParameterRequestList = 55,
                RenewalTimeValue = 58,
                RebindingTimeValue = 59,
            }

            internal struct MessageOffer {
                public string ipAddress;
                public string subnetMask;

                public string domainName;
                public string serverIdentifiderAddress;
                public string rounterIpAddress;
                public string domainIpAddress;

                public uint ipAddressLeaseTime;
            }

            internal struct MessageFrame {
                public byte opcode;
                public byte addressType;
                public byte addressLength;
                public byte options;
                public byte[] transactionId;
                public byte[] elapsedTime;
                public byte[] flags;
                public byte[] clientIpAddress;
                public byte[] yourIpAddress;
                public byte[] serverIpAddress;
                public byte[] relayIpAddress;
                public byte[] clientHardwareAddress;
                public byte[] serverHostName;
                public byte[] bootFileName;
                public byte[] magicCode;
                public byte[] dhcpOptions;
            }

            internal class Message {
                internal MessageFrame messageFrame;
                internal MessageOffer messageOffer;

                internal Message(byte[] data) {
                    using (var stream = new System.IO.MemoryStream(data, 0, data.Length)) {
                        try {
                            var data32 = new byte[4];
                            var data16 = new byte[2];

                            this.messageFrame.opcode = (byte)stream.ReadByte();
                            this.messageFrame.addressType = (byte)stream.ReadByte();
                            this.messageFrame.addressLength = (byte)stream.ReadByte();
                            this.messageFrame.options = (byte)stream.ReadByte();

                            this.messageFrame.transactionId = new byte[4];
                            stream.Read(this.messageFrame.transactionId, 0, 4);

                            this.messageFrame.elapsedTime = new byte[2];
                            stream.Read(this.messageFrame.elapsedTime, 0, 2);

                            this.messageFrame.flags = new byte[2];
                            stream.Read(this.messageFrame.flags, 0, 2);

                            this.messageFrame.clientIpAddress = new byte[4];
                            stream.Read(this.messageFrame.clientIpAddress, 0, 4);

                            this.messageFrame.yourIpAddress = new byte[4];
                            stream.Read(this.messageFrame.yourIpAddress, 0, 4);

                            this.messageFrame.serverIpAddress = new byte[4];
                            stream.Read(this.messageFrame.serverIpAddress, 0, 4);

                            this.messageFrame.relayIpAddress = new byte[4];
                            stream.Read(this.messageFrame.relayIpAddress, 0, 4);

                            this.messageFrame.clientHardwareAddress = new byte[16];
                            stream.Read(this.messageFrame.clientHardwareAddress, 0, 16);

                            this.messageFrame.serverHostName = new byte[64];
                            stream.Read(this.messageFrame.serverHostName, 0, 64);

                            this.messageFrame.bootFileName = new byte[128];
                            stream.Read(this.messageFrame.bootFileName, 0, 128);

                            this.messageFrame.magicCode = new byte[4];
                            stream.Read(this.messageFrame.magicCode, 0, 4);

                            // DHCP option start from 240
                            this.messageFrame.dhcpOptions = new byte[data.Length - 240];
                            stream.Read(this.messageFrame.dhcpOptions, 0, data.Length - 240);
                        }
                        catch {

                        }
                    }
                }
            }

            private Socket udpSocket;
            private IPEndPoint localEndpoint;
            internal bool Started { get; private set; }
            internal bool ClientConnected { get; set; }
            internal WiFiNetworkInterfaceSettings WifiNetworkInterfaceSetting { get; set; }

            internal DhcpServer() {
                
            }


            internal string DomainName {
                get;
                set;
            } = "SITCore";

            internal uint LeaseTime {
                get;
                set;
            } = 5000;


            internal void Dispose() => this.Dispose(true);

            protected virtual void Dispose(bool disposing) {
                if (disposing) {
                    this.Stop();
                    GC.SuppressFinalize(this);
                }
            }

            internal void Start(WiFiNetworkInterfaceSettings setting) {
                if (this.Started) {
                    return;
                }

                this.WifiNetworkInterfaceSetting = setting;

                try {
                    var ipAddress = this.WifiNetworkInterfaceSetting.Address;

                    this.localEndpoint = new IPEndPoint(ipAddress, (int)Port.Source);

                    this.udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    this.udpSocket.Bind(this.localEndpoint);

                    this.Started = true;
                    this.ClientConnected = false;

                    new Thread(this.Run).Start();
                }
                catch {

                }
            }

            internal void Stop() {
                if (!this.Started) {
                    return;
                }

                try {
                    this.Started = false;
                    this.ClientConnected = false;

                    if (this.udpSocket != null)
                        this.udpSocket.Close();

                    this.udpSocket = null;
                    this.localEndpoint = null;

                }
                catch {

                }
            }

            private void Run() {

                while (this.Started) {
                    if (this.ClientConnected == true) {
                        if (this.WifiNetworkInterfaceSetting.networkController.enabled == false ||
                        this.WifiNetworkInterfaceSetting.provider.GetAccessPointClientLinkConnect(this.WifiNetworkInterfaceSetting) == false) {
                            this.ClientConnected = false;
                        }

                        Thread.Sleep(100);
                        continue;
                    }


                    if (this.udpSocket != null && this.udpSocket.Available > 0) {
                        var s = this.udpSocket;

                        EndPoint ep = new IPEndPoint(IPAddress.Any, 0);

                        var available = this.udpSocket.Available;

                        var read = new byte[available];

                        if (s.ReceiveFrom(read, available, SocketFlags.None, ref ep) > 0)
                            this.ProcessMessage(read);
                    }
                    else {

                        Thread.Sleep(1);
                    }
                }
            }

            private void ProcessMessage(byte[] data) {
                Message message;
                var macAddress = string.Empty;

                try {
                    message = new Message(data);

                    if (message == null)
                        return;

                    for (var i = 0; i < message.messageFrame.addressLength; i++) {
                        macAddress += message.messageFrame.clientHardwareAddress[i].ToString("x2");
                    }

                    var msgTypes = ParseOptionValue(MessageOption.DHCPMessageType, message);

                    var offerDestinationAddress = this.WifiNetworkInterfaceSetting.Address.GetAddressBytes();

                    offerDestinationAddress[3]++;

                    if (offerDestinationAddress[3] == 255)
                        offerDestinationAddress[3] = 1;

                    var ipOffer = new IPAddress(offerDestinationAddress);

                    if (msgTypes != null) {
                        switch ((MessageType)msgTypes[0]) {
                            case MessageType.Discovery:

                                message.messageOffer.ipAddress = ipOffer.ToString();
                                message.messageOffer.subnetMask = this.WifiNetworkInterfaceSetting.SubnetMask.ToString();
                                message.messageOffer.ipAddressLeaseTime = this.LeaseTime;
                                message.messageOffer.domainName = this.DomainName;
                                message.messageOffer.serverIdentifiderAddress = this.WifiNetworkInterfaceSetting.Address.ToString();
                                message.messageOffer.rounterIpAddress = this.WifiNetworkInterfaceSetting.Address.ToString();
                                message.messageOffer.domainIpAddress = this.WifiNetworkInterfaceSetting.DnsAddresses[0].ToString();

                                this.Send(message, MessageType.Offer);

                                break;
                            case MessageType.Request:

                                message.messageOffer.ipAddress = ipOffer.ToString();
                                message.messageOffer.subnetMask = this.WifiNetworkInterfaceSetting.SubnetMask.ToString();
                                message.messageOffer.ipAddressLeaseTime = this.LeaseTime;
                                message.messageOffer.domainName = this.DomainName;
                                message.messageOffer.serverIdentifiderAddress = this.WifiNetworkInterfaceSetting.Address.ToString();
                                message.messageOffer.rounterIpAddress = this.WifiNetworkInterfaceSetting.Address.ToString();
                                message.messageOffer.domainIpAddress = this.WifiNetworkInterfaceSetting.DnsAddresses[0].ToString();

                                this.Send(message, MessageType.Acknowledge);

                                this.WifiNetworkInterfaceSetting.AccessPointClientConnectionChanged?.Invoke(this.WifiNetworkInterfaceSetting.networkController, ipOffer, macAddress);

                                this.ClientConnected = true;

                                break;

                            default:

                                break;
                        }
                    }

                }
                catch {

                }
            }

            private void Send(byte[] data) {
                try {
                    var addresses = Dns.GetHostEntry(IPAddress.Broadcast.ToString()).AddressList;

                    if (addresses == null)
                        throw new ArgumentException("Invalid hostname");

                    var i = 0;
                    for (; i < addresses.Length && addresses[i].AddressFamily != AddressFamily.InterNetwork; i++) ;

                    if (addresses.Length == 0 || i == addresses.Length) {
                        throw new ArgumentException("Invalid hostname");
                    }

                    this.udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                    var ipEndPoint = new IPEndPoint(addresses[i], (int)Port.Destination);

                    this.udpSocket.SendTo(data, 0, data.Length, SocketFlags.None, ipEndPoint);
                }
                catch {

                }
            }

            private void Send(Message message, MessageType msgType) {
                byte[] subnetMask, domainName;
                try {
                    //reply
                    message.messageFrame.opcode = 0x02;

                    //subnet
                    subnetMask = IPAddress.Parse(message.messageOffer.subnetMask).GetAddressBytes();

                    //create your ip address
                    message.messageFrame.yourIpAddress = IPAddress.Parse(message.messageOffer.ipAddress).GetAddressBytes();

                    //domainName
                    domainName = System.Text.Encoding.UTF8.GetBytes(message.messageOffer.domainName);

                    message = CreateOptions(msgType, message);

                    //create option
                    try {

                        var options = AddOptionValue(new byte[] { message.messageFrame.opcode }, null);
                        options = AddOptionValue(new byte[] { message.messageFrame.addressType }, options);
                        options = AddOptionValue(new byte[] { message.messageFrame.addressLength }, options);
                        options = AddOptionValue(new byte[] { message.messageFrame.options }, options);
                        options = AddOptionValue(message.messageFrame.transactionId, options);
                        options = AddOptionValue(message.messageFrame.elapsedTime, options);
                        options = AddOptionValue(message.messageFrame.flags, options);
                        options = AddOptionValue(message.messageFrame.clientIpAddress, options);
                        options = AddOptionValue(message.messageFrame.yourIpAddress, options);
                        options = AddOptionValue(message.messageFrame.serverIpAddress, options);
                        options = AddOptionValue(message.messageFrame.relayIpAddress, options);
                        options = AddOptionValue(message.messageFrame.clientHardwareAddress, options);
                        options = AddOptionValue(message.messageFrame.serverHostName, options);
                        options = AddOptionValue(message.messageFrame.bootFileName, options);
                        options = AddOptionValue(message.messageFrame.magicCode, options);
                        options = AddOptionValue(message.messageFrame.dhcpOptions, options);

                        if (options != null)
                            this.Send(options);
                    }
                    catch {

                    }
                }
                catch {

                }

            }

            private static byte[] ParseOptionValue(MessageOption option, Message message) {
                byte messageId;
                byte[] data;

                try {
                    var optionId = (int)option;

                    for (var i = 0; i < message.messageFrame.dhcpOptions.Length; i++) {

                        messageId = message.messageFrame.dhcpOptions[i];
                        byte size;
                        if (messageId == optionId) {
                            size = message.messageFrame.dhcpOptions[i + 1];
                            data = new byte[size];
                            Array.Copy(message.messageFrame.dhcpOptions, i + 2, data, 0, size);
                            return data;
                        }
                        else {
                            size = message.messageFrame.dhcpOptions[i + 1];
                            i += 1 + size;
                        }
                    }
                }
                catch {

                }
                return null;
            }
            private static Message CreateOptions(MessageType messageType, Message message) {
                byte[] requests, parse, leaseTime, serverIdentifiderAddress;

                try {

                    requests = ParseOptionValue(MessageOption.ParameterRequestList, message);

                    message.messageFrame.dhcpOptions = CreateOptionValue(MessageOption.DHCPMessageType, new byte[] { (byte)messageType }, null);

                    serverIdentifiderAddress = IPAddress.Parse(message.messageOffer.serverIdentifiderAddress).GetAddressBytes();

                    message.messageFrame.dhcpOptions = CreateOptionValue(MessageOption.DHCPServerIdentifier, serverIdentifiderAddress, message.messageFrame.dhcpOptions);

                    foreach (var i in requests) {
                        parse = null;
                        switch ((MessageOption)i) {
                            case MessageOption.SubnetMask:
                                parse = IPAddress.Parse(message.messageOffer.subnetMask).GetAddressBytes();
                                break;
                            case MessageOption.Router:
                                parse = IPAddress.Parse(message.messageOffer.rounterIpAddress).GetAddressBytes();
                                break;
                            case MessageOption.DomainNameServers:
                                parse = IPAddress.Parse(message.messageOffer.domainIpAddress).GetAddressBytes();
                                break;
                            case MessageOption.DomainName:
                                parse = System.Text.Encoding.UTF8.GetBytes(message.messageOffer.domainName);
                                break;
                            case MessageOption.DHCPServerIdentifier:
                                parse = IPAddress.Parse(message.messageOffer.serverIdentifiderAddress).GetAddressBytes();
                                break;

                            default:
                                break;

                        }
                        if (parse != null)
                            message.messageFrame.dhcpOptions = CreateOptionValue((MessageOption)i, parse, message.messageFrame.dhcpOptions);
                    }

                    leaseTime = new byte[4];

                    leaseTime[0] = (byte)(message.messageOffer.ipAddressLeaseTime >> 24);
                    leaseTime[1] = (byte)(message.messageOffer.ipAddressLeaseTime >> 16);
                    leaseTime[2] = (byte)(message.messageOffer.ipAddressLeaseTime >> 8);
                    leaseTime[3] = (byte)(message.messageOffer.ipAddressLeaseTime);

                    message.messageFrame.dhcpOptions = CreateOptionValue(MessageOption.IPAddressLeaseTime, leaseTime, message.messageFrame.dhcpOptions);
                    message.messageFrame.dhcpOptions = CreateOptionValue(MessageOption.RenewalTimeValue, leaseTime, message.messageFrame.dhcpOptions);
                    message.messageFrame.dhcpOptions = CreateOptionValue(MessageOption.RebindingTimeValue, leaseTime, message.messageFrame.dhcpOptions);

                    var dataTmp = new byte[message.messageFrame.dhcpOptions.Length + 1];
                    Array.Copy(message.messageFrame.dhcpOptions, dataTmp, message.messageFrame.dhcpOptions.Length);

                    message.messageFrame.dhcpOptions = new byte[message.messageFrame.dhcpOptions.Length + 1];

                    message.messageFrame.dhcpOptions[message.messageFrame.dhcpOptions.Length - 1] = 255; // mark option end.

                    Array.Copy(dataTmp, message.messageFrame.dhcpOptions, dataTmp.Length);

                }
                catch {
                    return null;
                }

                return message;
            }

            private static byte[] AddOptionValue(byte[] value, byte[] options) {
                try {
                    if (options != null) {
                        var dataTmp = new byte[options.Length + value.Length];
                        Array.Copy(options, dataTmp, options.Length);

                        options = new byte[dataTmp.Length];
                        Array.Copy(dataTmp, options, dataTmp.Length);
                    }
                    else {
                        options = new byte[value.Length];
                    }

                    Array.Copy(value, 0, options, options.Length - value.Length, value.Length);
                }
                catch {
                    return null;
                }

                return options;
            }

            private static byte[] CreateOptionValue(MessageOption optionCode, byte[] value, byte[] options) {
                byte[] option;

                try {
                    option = new byte[value.Length + 2];

                    option[0] = (byte)optionCode;
                    option[1] = (byte)value.Length;

                    Array.Copy(value, 0, option, 2, value.Length);

                    if (options == null) {
                        options = new byte[option.Length];
                    }
                    else {
                        var dataTmp = new byte[options.Length + option.Length];

                        Array.Copy(options, dataTmp, options.Length);

                        options = new byte[dataTmp.Length];
                        Array.Copy(dataTmp, options, dataTmp.Length);
                    }
                    Array.Copy(option, 0, options, options.Length - option.Length, option.Length);
                }
                catch {
                    return null;
                }

                return options;
            }
        }
    }

    /// <summary>PPP authentication protocol.</summary>
    public enum PppAuthenticationType {
        /// <summary>No authentication.</summary>
        None = 0,
        /// <summary>Allow whichever the peer offers (PAP or CHAP).</summary>
        Any = 1,
        /// <summary>PAP — Password Authentication Protocol (cleartext).</summary>
        Pap = 2,
        /// <summary>CHAP — Challenge-Handshake Authentication Protocol.</summary>
        Chap = 3,
    }

    /// <summary>PPP-specific interface settings (cellular modems, dial-up).</summary>
    public class PppNetworkInterfaceSettings : NetworkInterfaceSettings {
        /// <summary>PPP username.</summary>
        public string Username { get; set; }
        /// <summary>PPP password.</summary>
        public string Password { get; set; }
        /// <summary>Authentication protocol to use.</summary>
        public PppAuthenticationType AuthenticationType { get; set; }
    }

    /// <summary>Underlying physical bus carrying the network interface.</summary>
    public enum NetworkCommunicationInterface {
        /// <summary>Built-in MAC peripheral.</summary>
        BuiltIn = 0,
        /// <summary>External controller over SPI (e.g. ENC28J60, WINC1500).</summary>
        Spi = 1,
        /// <summary>External controller over UART.</summary>
        Uart = 2,
    }

    /// <summary>Base class for transport-bus settings.</summary>
    public class NetworkCommunicationInterfaceSettings {

    }

    /// <summary>Built-in MAC — no extra wiring required.</summary>
    public class BuiltInNetworkCommunicationInterfaceSettings : NetworkCommunicationInterfaceSettings {

    }

    /// <summary>Settings for an external SPI-attached network controller (chip select, reset, interrupt pins, SPI mode).</summary>
    public class SpiNetworkCommunicationInterfaceSettings : NetworkCommunicationInterfaceSettings {
        /// <summary>Native API name of the SPI controller to use.</summary>
        public string SpiApiName { get; set; }
        /// <summary>SPI clock and mode settings.</summary>
        public SpiConnectionSettings SpiSettings { get; set; }

        /// <summary>Native API name of the GPIO controller owning the reset/interrupt pins.</summary>
        public string GpioApiName { get; set; }

        /// <summary>Pin used to reset the external controller.</summary>
        public GpioPin ResetPin { get; set; }
        /// <summary>Level that drives the chip into reset.</summary>
        public GpioPinValue ResetActiveState { get; set; }

        /// <summary>Pin the external controller uses to signal interrupts.</summary>
        public GpioPin InterruptPin { get; set; }
        /// <summary>Edge of the interrupt signal that fires an event.</summary>
        public GpioPinEdge InterruptEdge { get; set; }
        /// <summary>Drive mode applied to the interrupt pin.</summary>
        public GpioPinDriveMode InterruptDriveMode { get; set; }
    }

    /// <summary>Settings for an external UART-attached network controller (cellular modem, etc.).</summary>
    public class UartNetworkCommunicationInterfaceSettings : NetworkCommunicationInterfaceSettings {
        /// <summary>Native API name of the UART controller to use.</summary>
        public string ApiName { get; set; }

        /// <summary>UART baud rate.</summary>
        public int BaudRate { get; set; }
        /// <summary>UART data bits.</summary>
        public int DataBits { get; set; }
        /// <summary>UART parity.</summary>
        public UartParity Parity { get; set; }
        /// <summary>UART stop bits.</summary>
        public UartStopBitCount StopBits { get; set; }
        /// <summary>UART flow control.</summary>
        public UartHandshake Handshaking { get; set; }
    }

    namespace Provider {
        /// <summary>Provider contract for a network controller.</summary>
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

        /// <summary>Concrete <see cref="INetworkControllerProvider"/> backed by the native TinyCLR network HAL (lwIP + mbedTLS).</summary>
        public sealed class NetworkControllerApiWrapper : INetworkControllerProvider {
            private readonly IntPtr impl;
            private readonly NativeEventDispatcher networkLinkConnectedChangedDispatcher;
            private readonly NativeEventDispatcher networkAddressChangedDispatcher;
            private NetworkLinkConnectedChangedEventHandler networkLinkConnectedChangedCallbacks;
            private NetworkAddressChangedEventHandler networkAddressChangedCallbacks;

            public NativeApi Api { get; }

            public NetworkControllerApiWrapper(NativeApi api) {
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

            // Non-blocking variant of Enable. Spawns a native RTOS task that
            // runs the slow driver Enable in the background and returns
            // immediately, so the CLR scheduler stays free to run other
            // managed threads. The interface comes up later — subscribe to
            // NetworkLinkConnectedChanged / NetworkAddressChanged on the
            // owning NetworkController to learn when it is actually ready.
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void EnableAsync();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Suspend();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Resume();

            // Promotes this controller's netif to lwIP's default route at
            // the firmware level. Counterpart to SetAsDefaultController on
            // the public NetworkController class — without this, switching
            // the managed default would leave lwIP routing unmatched
            // destinations through whichever interface was Enable()d last.
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetAsDefault();

            public void SetInterfaceSettings(NetworkInterfaceSettings settings) {
                switch (this.InterfaceType) {
                    case NetworkInterfaceType.Ethernet when settings is EthernetNetworkInterfaceSettings enis:
                        this.SetInterfaceSettings(enis);
                        break;

                    case NetworkInterfaceType.WiFi when settings is WiFiNetworkInterfaceSettings wnis:
                        this.SetInterfaceSettings(wnis);
                        break;

                    case NetworkInterfaceType.Ppp when settings is PppNetworkInterfaceSettings pnis:
                        this.SetInterfaceSettings(pnis);
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

                    case NetworkCommunicationInterface.Uart when settings is UartNetworkCommunicationInterfaceSettings ucis:
                        this.SetCommunicationInterfaceSettings(ucis);
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
            private extern void SetInterfaceSettings(PppNetworkInterfaceSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetCommunicationInterfaceSettings(BuiltInNetworkCommunicationInterfaceSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetCommunicationInterfaceSettings(SpiNetworkCommunicationInterfaceSettings settings);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetCommunicationInterfaceSettings(UartNetworkCommunicationInterfaceSettings settings);

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
            public extern int AuthenticateAsClient(int socketHandle, string targetHost, X509Certificate caCertificate, X509Certificate clientCertificate, SslProtocols sslProtocols, SslVerification sslVerification);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int AuthenticateAsServer(int socketHandle, X509Certificate certificate, SslProtocols sslProtocols);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int SecureRead(int handle, byte[] buffer, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int SecureWrite(int handle, byte[] buffer, int offset, int count);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetHostByName(string name, out string canonicalName, out SocketAddress[] addresses);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool GetAccessPointClientLinkConnect(WiFiNetworkInterfaceSettings settings);

            // Half-close (lwIP shutdown). Appended at end so the metadata
            // token order — and therefore the firmware interop dispatch
            // table indices — for existing methods stay unchanged.
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Shutdown(int socket, SocketShutdown how);
        }
    }
}
