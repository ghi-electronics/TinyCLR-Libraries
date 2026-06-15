using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace GHIElectronics.TinyCLR.Networking {
    /// <summary>Defines the low-level operations a network stack must implement to back sockets.</summary>
    public interface INetworkProvider {
        /// <summary>Creates a new socket and returns its handle.</summary>
        int Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
        /// <summary>Closes the specified socket.</summary>
        void Close(int socket);
        /// <summary>Disables send and/or receive operations on the socket.</summary>
        void Shutdown(int socket, SocketShutdown how);
        /// <summary>Binds the socket to the specified local address.</summary>
        void Bind(int socket, SocketAddress address);
        /// <summary>Places the socket in a listening state with the specified backlog.</summary>
        void Listen(int socket, int backlog);
        /// <summary>Accepts a pending connection and returns the new socket handle.</summary>
        int Accept(int socket);
        /// <summary>Connects the socket to the specified remote address.</summary>
        void Connect(int socket, SocketAddress address);
        /// <summary>Returns the number of bytes available to read from the socket.</summary>
        int Available(int socket);
        /// <summary>Polls the socket for the specified status within the given timeout.</summary>
        bool Poll(int socket, int microSeconds, SelectMode mode);
        /// <summary>Sends data on a connected socket and returns the number of bytes sent.</summary>
        int Send(int socket, byte[] buffer, int offset, int count, SocketFlags flags);
        /// <summary>Receives data from a connected socket and returns the number of bytes read.</summary>
        int Receive(int socket, byte[] buffer, int offset, int count, SocketFlags flags);
        /// <summary>Sends data to the specified address and returns the number of bytes sent.</summary>
        int SendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, SocketAddress address);
        /// <summary>Receives data and reports the sender's address, returning the number of bytes read.</summary>
        int ReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, ref SocketAddress address);
        /// <summary>Gets the remote address the socket is connected to.</summary>
        void GetRemoteAddress(int socket, out SocketAddress address);
        /// <summary>Gets the local address the socket is bound to.</summary>
        void GetLocalAddress(int socket, out SocketAddress address);
        /// <summary>Reads the value of the specified socket option.</summary>
        void GetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);
        /// <summary>Sets the value of the specified socket option.</summary>
        void SetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

        /// <summary>Performs the client side of an SSL/TLS handshake on the socket.</summary>
        int AuthenticateAsClient(int socketHandle, string targetHost, X509Certificate caCertificate, X509Certificate clientCertificate, SslProtocols sslProtocols, SslVerification sslVerification);
        /// <summary>Performs the server side of an SSL/TLS handshake on the socket.</summary>
        int AuthenticateAsServer(int socketHandle, X509Certificate certificate, SslProtocols sslProtocols);
        /// <summary>Reads decrypted data from a secured socket.</summary>
        int SecureRead(int handle, byte[] buffer, int offset, int count);
        /// <summary>Writes data to be encrypted on a secured socket.</summary>
        int SecureWrite(int handle, byte[] buffer, int offset, int count);

        /// <summary>Resolves a host name to its canonical name and addresses.</summary>
        void GetHostByName(string name, out string canonicalName, out SocketAddress[] addresses);
    }
}
