using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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

        public void Dispose() => this.Provider.Dispose();

        public void Enable() => this.Provider.Enable();
        public void Disable() => this.Provider.Disable();

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
        public bool IsDhcpEnabled { get; set; } = true;
        public bool IsDynamicDnsEnabled { get; set; } = true;
        public byte[] TlsEntropy { get; set; }
    }

    public class EthernetNetworkInterfaceSettings : NetworkInterfaceSettings {

    }

    public class WiFiNetworkInterfaceSettings : NetworkInterfaceSettings {
        public string Ssid { get; set; }
        public string Password { get; set; }
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
            public extern void NativeAccept(int socket, long evenId);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void NativeConnect(int socket, SocketAddress address, long evenId);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern int Available(int socket);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool Poll(int socket, int microSeconds, SelectMode mode);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void NativeSend(int socket, byte[] buffer, int offset, int count, SocketFlags flags, long eventId);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void NativeReceive(int socket, byte[] buffer, int offset, int count, SocketFlags flags, long eventId);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void NativeSendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, SocketAddress address, long eventId);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void NativeReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, long eventId);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetRemoteAddress(int socket, out SocketAddress address);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetLocalAddress(int socket, out SocketAddress address);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void NativeAuthenticateAsClient(int socketHandle, string targetHost, X509Certificate caCertificate, X509Certificate clientCertificate, SslProtocols sslProtocols, SslVerification sslVerification, long eventId);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void NativeAuthenticateAsServer(int socketHandle, X509Certificate certificate, SslProtocols sslProtocols, long eventId);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void NativeSecureRead(int handle, byte[] buffer, int offset, int count, long eventId);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void NativeSecureWrite(int handle, byte[] buffer, int offset, int count, long eventId);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void GetHostByName(string name, out string canonicalName, out SocketAddress[] addresses);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern bool WaitForEvent(int socket, EventType type, long evenId, out int result, ref SocketAddress address);

            private bool WaitForEvent(int socket, EventType type, long evenId, out int result) {
                SocketAddress address = null;

                return this.WaitForEvent(socket, type, evenId, out result, ref address);
            }

            private enum EventType {
                Connect = 1,
                Accept = 2,
                Send = 3,
                SendTo = 4,
                Receive = 5,
                ReceiveFrom = 6,
                NativeAuthenticateAsClient = 7,
                NativeAuthenticateAsServer = 8,
                NativeSecureRead = 9,
                NativeSecureWrite = 10,

            }

            public void Connect(int socket, SocketAddress address) {
                var now = DateTime.Now.Ticks;

                this.NativeConnect(socket, address, now);

                while (!this.WaitForEvent(socket, EventType.Connect, now, out var obj)) ;
            }

            public int Accept(int socket) {
                var now = DateTime.Now.Ticks;

                this.NativeAccept(socket, now);

                int sock;

                while (!this.WaitForEvent(socket, EventType.Accept, now, out sock)) ;

                return sock;
            }

            public int Send(int socket, byte[] buffer, int offset, int count, SocketFlags flags) {
                var now = DateTime.Now.Ticks;

                this.NativeSend(socket, buffer, offset, count, flags, now);

                int sent;

                while (!this.WaitForEvent(socket, EventType.Send, now, out sent)) ;

                return sent;
            }

            public int Receive(int socket, byte[] buffer, int offset, int count, SocketFlags flags) {
                var now = DateTime.Now.Ticks;

                this.NativeReceive(socket, buffer, offset, count, flags, now);

                int rec;

                while (!this.WaitForEvent(socket, EventType.Receive, now, out rec)) ;

                return rec;
            }

            public int SendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, SocketAddress address) {
                var now = DateTime.Now.Ticks;

                this.NativeSendTo(socket, buffer, offset, count, flags, address, now);

                int sent;

                while (!this.WaitForEvent(socket, EventType.SendTo, now, out sent)) ;

                return sent;
            }

            public int ReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, ref SocketAddress address) {
                var now = DateTime.Now.Ticks;

                this.NativeReceiveFrom(socket, buffer, offset, count, flags, now);

                int rec;

                while (!this.WaitForEvent(socket, EventType.ReceiveFrom, now, out rec, ref address)) ;

                return rec;
            }

            public int AuthenticateAsClient(int socketHandle, string targetHost, X509Certificate caCertificate, X509Certificate clientCertificate, SslProtocols sslProtocols, SslVerification sslVerification) {
                var now = DateTime.Now.Ticks;

                this.NativeAuthenticateAsClient(socketHandle, targetHost, caCertificate, clientCertificate, sslProtocols, sslVerification, now);

                int aut;

                while (!this.WaitForEvent(socketHandle, EventType.NativeAuthenticateAsClient, now, out aut)) ;

                return aut;
            }

            public int AuthenticateAsServer(int socketHandle, X509Certificate certificate, SslProtocols sslProtocols) {
                var now = DateTime.Now.Ticks;

                this.NativeAuthenticateAsServer(socketHandle, certificate, sslProtocols, now);

                int aut;

                while (!this.WaitForEvent(socketHandle, EventType.NativeAuthenticateAsServer, now, out aut)) ;

                return aut;
            }

            public int SecureRead(int handle, byte[] buffer, int offset, int count) {
                var now = DateTime.Now.Ticks;

                this.NativeSecureRead(handle, buffer, offset, count, now);

                int read;

                while (!this.WaitForEvent(handle, EventType.NativeSecureRead, now, out read)) ;

                return read;
            }

            public int SecureWrite(int handle, byte[] buffer, int offset, int count) {
                var now = DateTime.Now.Ticks;

                this.NativeSecureWrite(handle, buffer, offset, count, now);

                int write;

                while (!this.WaitForEvent(handle, EventType.NativeSecureWrite, now, out write)) ;

                return write;
            }

        }
    }
}
