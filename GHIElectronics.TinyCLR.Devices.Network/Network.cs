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

        public static NetworkController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.NetworkController) is NetworkController c ? c : NetworkController.FromName(NativeApi.GetDefaultName(NativeApiType.NetworkController));
        public static NetworkController FromName(string name) => NetworkController.FromProvider(new NetworkControllerApiWrapper(NativeApi.Find(name, NativeApiType.NetworkController)));
        public static NetworkController FromProvider(INetworkControllerProvider provider) => new NetworkController(provider);

        public NetworkInterfaceSettings ActiveInterfaceSettings { get; private set; }
        public NetworkCommunicationInterfaceSettings ActiveCommunicationInterfaceSettings { get; private set; }

        public NetworkInterfaceType InterfaceType => this.Provider.InterfaceType;
        public NetworkCommunicationInterface CommunicationInterface => this.Provider.CommunicationInterface;

        internal bool enabled;

        public void Dispose() {
            this.Provider.Dispose();

            this.enabled = false;
        }

        public void Enable() {

            this.Provider.Enable();

            this.enabled = true;

            if (this.InterfaceType == NetworkInterfaceType.WiFi) {
                var setting = (WiFiNetworkInterfaceSettings)this.ActiveInterfaceSettings;

                if (setting.Mode == WiFiMode.AccessPoint) {
                    setting.networkController = this;
                    setting.provider = this.Provider;

                    if (setting.DhcpEnable)
                        setting.dhcpServer.Start();

                }
            }
        }

        public void Disable() {
            if (this.InterfaceType == NetworkInterfaceType.WiFi) {
                var setting = (WiFiNetworkInterfaceSettings)this.ActiveInterfaceSettings;

                if (setting.Mode == WiFiMode.AccessPoint) {
                    if (setting.DhcpEnable)
                        setting.dhcpServer.Stop();
                }
            }

            this.Provider.Disable();
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
        public PppNetworkInterfaceProperties GetPppProperties() => this as PppNetworkInterfaceProperties;
    }

    public class EthernetNetworkInterfaceProperties : NetworkInterfaceProperties {

    }

    public class WiFiNetworkInterfaceProperties : NetworkInterfaceProperties {

    }

    public class PppNetworkInterfaceProperties : NetworkInterfaceProperties {

    }

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

    public class EthernetNetworkInterfaceSettings : NetworkInterfaceSettings {

    }

    public enum WiFiMode {
        Station = 0,
        AccessPoint = 1
    }

    public class WiFiNetworkInterfaceSettings : NetworkInterfaceSettings {
        public string Ssid { get; set; }
        public string Password { get; set; }
        public uint Channel { get; set; } = 1;

        internal INetworkControllerProvider provider;
        internal NetworkController networkController;

        public delegate void AccessPointClientConnectionChangedEventHandler(NetworkController sender, IPAddress clientAddress, string macAddress);
        public event AccessPointClientConnectionChangedEventHandler AccessPointClientConnectionChanged;

        public WiFiMode Mode {
            get => this.mode;
            set {

                this.mode = value;

                if (this.mode == WiFiMode.AccessPoint && this.DhcpEnable && this.dhcpServer == null) {
                    this.dhcpServer = new DhcpServer(this);
                }
            }
        }

        private WiFiMode mode;


        internal DhcpServer dhcpServer;

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

            internal DhcpServer(WiFiNetworkInterfaceSettings setting) {
                this.WifiNetworkInterfaceSetting = setting;

                if (this.WifiNetworkInterfaceSetting.Address == null)
                    this.WifiNetworkInterfaceSetting.Address = new IPAddress(new byte[] { 192, 168, 1, 1 });

                if (this.WifiNetworkInterfaceSetting.GatewayAddress == null)
                    this.WifiNetworkInterfaceSetting.GatewayAddress = new IPAddress(new byte[] { 192, 168, 1, 1 });

                if (this.WifiNetworkInterfaceSetting.SubnetMask == null)
                    this.WifiNetworkInterfaceSetting.SubnetMask = IPAddress.Any;

                if (this.WifiNetworkInterfaceSetting.DnsAddresses == null)
                    this.WifiNetworkInterfaceSetting.DnsAddresses = new IPAddress[] { IPAddress.Any };
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

            internal void Start() {
                if (this.Started) {
                    return;
                }

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

    public class NetworkCommunicationInterfaceSettings {

    }

    public class BuiltInNetworkCommunicationInterfaceSettings : NetworkCommunicationInterfaceSettings {

    }

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

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Suspend();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Resume();

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
        }
    }
}
