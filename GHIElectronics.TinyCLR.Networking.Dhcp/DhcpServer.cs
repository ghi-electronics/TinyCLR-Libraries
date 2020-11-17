
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace GHIElectronics.TinyCLR.Networking.Dhcp {
    public enum DhcpPort {
        Source = 67,
        Destination = 68,
    }

    public enum MessageType {
        Discovery = 1,
        Offer = 2,
        Request = 3,
        Acknowledge = 5,
    }

    public enum MessageOption {
        SubnetMask = 1,
        TimeOffset,
        Router,
        TimeServers,
        NameServers,
        DomainNameServers,
        LogServers,
        CookieServers,
        LPRServers,
        ImpressServers,
        ResourceLocationServers,
        HostName,
        BootFileSize,
        MeritDumpFile,
        DomainName,
        SwapServer,
        RootPath,
        ExtensionsPath,
        IPForwarding,
        NonLocalSourceRouting,
        PolicyFilter,
        MaximumDatagramReassemblySize,
        DefaultIPTimeToLive,
        PathMTUAgingTimeout,
        PathMTUPlateauTable,
        InterfaceMTU,
        AllSubnetsareLocal,
        BroadcastAddress,
        PerformMaskDiscovery,
        MaskSupplier,
        PerformRouterDiscovery,
        RouterSolicitationAddressOption,
        StaticRoute,
        TrailerEncapsulation,
        ARPCacheTimeout,
        EthernetEncapsulation,
        TCPDefaultTTL,
        TCPKeepaliveInterval,
        TCPKeepaliveGarbage,
        NetworkInformationServiceDomain,
        NetworkInformationServers,
        NetworkTimeProtocolServersOption,
        VendorSpecificInformation,
        NetBIOSoverTCPIPNameServer,
        NetBIOSoverTCPIPDatagramDistributionServer,
        NetBIOSoverTCPIPNodeType,
        NetBIOSoverTCPIPScope,
        XWindowSystemFontServer,
        XWindowSystemDisplayManager,
        RequestedIPAddress,
        IPAddressLeaseTime,
        OptionOverload,
        DHCPMessageType,
        DHCPServerIdentifier,
        ParameterRequestList,
        Message = 56,
        DHCPMaximumDHCPMessageSize,
        RenewalTimeValue,
        RebindingTimeValue,
        ClientIdentifier1,
        ClientIdentifier2,
        NetwareIPDomainName,
        NetwareIPSubOptions,
        NISV3ClientDomainName,
        NISV3ServerAddress,
        TFTPServerName,
        BootFileName,
        HomeAgentAddresses,
        SimpleMailServerAddresses,
        PostOfficeServerAddresses,
        NetworkNewsServerAddresses,
        WWWServerAddresses,
        FingerServerAddresses,
        ChatServerAddresses,
        StreetTalkServerAddresses,
        StreetTalkDirectoryAssistanceAddresses,
        UserClassInformation,
        SLPDirectoryAgent,
        SLPServiceScope,
        RapidCommit,
        FQDNFullyQualifiedDomainName,
        RelayAgentInformation,
        InternetStorageNameService
    }

    public struct MessageOffer {
        public string ipAddress;
        public string subnetMask;

        public string domainName;
        public string serverIdentifiderAddress;
        public string rounterIpAddress;
        public string domainIpAddress;

        public uint ipAddressLeaseTime;
    }

    public struct MessageFrame {
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

    public class Message {
        public MessageFrame messageFrame;
        public MessageOffer messageOffer;

        public Message(byte[] data) {
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

    public class DhcpServer {
        public delegate void OnClientConnectedEventHandler(string ip, string macAddress);
        public event OnClientConnectedEventHandler OnClientConnected;

        private Socket udpSocket;
        private IPEndPoint localEndpoint;

        public bool Started { get; private set; }

        public string SourceIpAddress {
            get;
            set;
        } = "192.168.1.1";

        public string DestinationIpAddress {
            get;
            set;
        } = "192.168.1.2";

        public string SubnetMaskIpAddress {
            get;
            set;
        } = "255.255.255.0";

        public string DomainIpAddress {
            get;
            set;
        } = "0.0.0.0";

        public string DomainName {
            get;
            set;
        } = "SITCore";

        public string RouterIpAddress {
            get;
            set;
        } = "0.0.0.0";

        public uint LeaseTime {
            get;
            set;
        } = 5000;

        public DhcpServer() {

        }

        public void Dispose() => this.Dispose(true);

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.Stop();
                GC.SuppressFinalize(this);
            }
        }

        public void Start() {
            if (this.Started) {
                throw new Exception("Already started.");
            }

            try {
                var ipAddress = IPAddress.Parse(this.SourceIpAddress);

                this.localEndpoint = new IPEndPoint(ipAddress, (int)DhcpPort.Source);

                this.udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                this.udpSocket.Bind(this.localEndpoint);

                this.Started = true;

                new Thread(this.Run).Start(); ;
            }
            catch {

            }
        }

        public void Stop() {
            if (!this.Started) {
                throw new Exception("Already stopped.");
            }

            try {
                this.Started = false;

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

                if (msgTypes != null) {
                    switch ((MessageType)msgTypes[0]) {
                        case MessageType.Discovery:

                            message.messageOffer.ipAddress = this.DestinationIpAddress;
                            message.messageOffer.subnetMask = this.SubnetMaskIpAddress;
                            message.messageOffer.ipAddressLeaseTime = this.LeaseTime;
                            message.messageOffer.domainName = this.DomainName;
                            message.messageOffer.serverIdentifiderAddress = this.SourceIpAddress;
                            message.messageOffer.rounterIpAddress = this.RouterIpAddress;
                            message.messageOffer.domainIpAddress = this.DomainIpAddress;

                            this.Send(message, MessageType.Offer);

                            break;
                        case MessageType.Request:

                            message.messageOffer.ipAddress = this.DestinationIpAddress;
                            message.messageOffer.subnetMask = this.SubnetMaskIpAddress;
                            message.messageOffer.ipAddressLeaseTime = this.LeaseTime;
                            message.messageOffer.domainName = this.DomainName;
                            message.messageOffer.serverIdentifiderAddress = this.SourceIpAddress;
                            message.messageOffer.rounterIpAddress = this.RouterIpAddress;
                            message.messageOffer.domainIpAddress = this.DomainIpAddress;
                            this.Send(message, MessageType.Acknowledge);

                            OnClientConnected?.Invoke(this.DestinationIpAddress, macAddress);
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

                var ipEndPoint = new IPEndPoint(addresses[i], (int)DhcpPort.Destination);

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

                CreateOptions(msgType, ref message);

                //create option
                try {
                    var options = new byte[0];

                    AddOptionValue(new byte[] { message.messageFrame.opcode }, ref options);
                    AddOptionValue(new byte[] { message.messageFrame.addressType }, ref options);
                    AddOptionValue(new byte[] { message.messageFrame.addressLength }, ref options);
                    AddOptionValue(new byte[] { message.messageFrame.options }, ref options);
                    AddOptionValue(message.messageFrame.transactionId, ref options);
                    AddOptionValue(message.messageFrame.elapsedTime, ref options);
                    AddOptionValue(message.messageFrame.flags, ref options);
                    AddOptionValue(message.messageFrame.clientIpAddress, ref options);
                    AddOptionValue(message.messageFrame.yourIpAddress, ref options);
                    AddOptionValue(message.messageFrame.serverIpAddress, ref options);
                    AddOptionValue(message.messageFrame.relayIpAddress, ref options);
                    AddOptionValue(message.messageFrame.clientHardwareAddress, ref options);
                    AddOptionValue(message.messageFrame.serverHostName, ref options);
                    AddOptionValue(message.messageFrame.bootFileName, ref options);
                    AddOptionValue(message.messageFrame.magicCode, ref options);
                    AddOptionValue(message.messageFrame.dhcpOptions, ref options);

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
        private static void CreateOptions(MessageType messageType, ref Message message) {
            byte[] requests, parse, leaseTime, serverIdentifiderAddress;

            try {

                requests = ParseOptionValue(MessageOption.ParameterRequestList, message);

                message.messageFrame.dhcpOptions = null;

                CreateOptionValue(MessageOption.DHCPMessageType, new byte[] { (byte)messageType }, ref message.messageFrame.dhcpOptions);

                serverIdentifiderAddress = IPAddress.Parse(message.messageOffer.serverIdentifiderAddress).GetAddressBytes();
                CreateOptionValue(MessageOption.DHCPServerIdentifier, serverIdentifiderAddress, ref message.messageFrame.dhcpOptions);

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
                        CreateOptionValue((MessageOption)i, parse, ref message.messageFrame.dhcpOptions);
                }

                leaseTime = new byte[4];

                leaseTime[0] = (byte)(message.messageOffer.ipAddressLeaseTime >> 24);
                leaseTime[1] = (byte)(message.messageOffer.ipAddressLeaseTime >> 16);
                leaseTime[2] = (byte)(message.messageOffer.ipAddressLeaseTime >> 8);
                leaseTime[3] = (byte)(message.messageOffer.ipAddressLeaseTime);

                CreateOptionValue(MessageOption.IPAddressLeaseTime, leaseTime, ref message.messageFrame.dhcpOptions);
                CreateOptionValue(MessageOption.RenewalTimeValue, leaseTime, ref message.messageFrame.dhcpOptions);
                CreateOptionValue(MessageOption.RebindingTimeValue, leaseTime, ref message.messageFrame.dhcpOptions);
                
                var dataTmp = new byte[message.messageFrame.dhcpOptions.Length + 1];
                Array.Copy(message.messageFrame.dhcpOptions, dataTmp, message.messageFrame.dhcpOptions.Length);

                message.messageFrame.dhcpOptions = new byte[message.messageFrame.dhcpOptions.Length + 1];

                message.messageFrame.dhcpOptions[message.messageFrame.dhcpOptions.Length - 1] = 255; // mark option end.

                Array.Copy(dataTmp, message.messageFrame.dhcpOptions, dataTmp.Length);
                
            }
            catch {

            }
        }

        private static void AddOptionValue(byte[] value, ref byte[] options) {
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

            }
        }

        private static void CreateOptionValue(MessageOption optionCode, byte[] value, ref byte[] options) {
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

            }
        }
    }
}
