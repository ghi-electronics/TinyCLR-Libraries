// TinyCLR-specific types in the Networking namespace that have NO framework
// counterpart, so TypeForwardedTo doesn't apply. We mirror them here as
// stubs so dependent shims (notably Devices.Network) can reference them at
// runtime on Desktop without TypeLoadException.

using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace GHIElectronics.TinyCLR.Networking {
    // INetworkProvider is the abstraction that NetworkController providers
    // implement. INetworkControllerProvider (in Devices.Network) extends
    // this interface, so it MUST be present in the Networking shim.
    public interface INetworkProvider {
        int Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
        void Close(int socket);
        void Shutdown(int socket, SocketShutdown how);
        void Bind(int socket, SocketAddress address);
        void Listen(int socket, int backlog);
        int Accept(int socket);
        void Connect(int socket, SocketAddress address);
        int Available(int socket);
        bool Poll(int socket, int microSeconds, SelectMode mode);
        int Send(int socket, byte[] buffer, int offset, int count, SocketFlags flags);
        int Receive(int socket, byte[] buffer, int offset, int count, SocketFlags flags);
        int SendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, SocketAddress address);
        int ReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, ref SocketAddress address);
        void GetRemoteAddress(int socket, out SocketAddress address);
        void GetLocalAddress(int socket, out SocketAddress address);
        void GetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);
        void SetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

        int AuthenticateAsClient(int socketHandle, string targetHost, X509Certificate caCertificate, X509Certificate clientCertificate, SslProtocols sslProtocols, SslVerification sslVerification);
        int AuthenticateAsServer(int socketHandle, X509Certificate certificate, SslProtocols sslProtocols);
        int SecureRead(int handle, byte[] buffer, int offset, int count);
        int SecureWrite(int handle, byte[] buffer, int offset, int count);

        void GetHostByName(string name, out string canonicalName, out SocketAddress[] addresses);
    }
}

namespace System.Security.Authentication {
    // TinyCLR-specific SSL verification mode. Framework's System.Security.Authentication
    // has no equivalent, so we mirror it as a plain enum here.
    public enum SslVerification {
        None = 0,
        Optional = 1,
        Required = 2,
        VerifyOnce = 3
    }
}
