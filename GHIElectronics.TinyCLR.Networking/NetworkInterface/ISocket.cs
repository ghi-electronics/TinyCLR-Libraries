using System.Net.Sockets;

namespace System.Net.NetworkInterface {
    public interface ISocket {
        int Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
        void Close(int socket);

        void Bind(int socket, SocketAddress address);
        void Listen(int socket, int backlog);
        int Accept(int socket);

        void Connect(int socket, SocketAddress address);

        int Available(int socket);
        bool Poll(int socket, int microSeconds, SelectMode mode);

        int Send(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout);
        int Receive(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout);
        int SendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, SocketAddress address);
        int ReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, ref SocketAddress address);

        void GetRemoteAddress(int socket, out SocketAddress address);
        void GetLocalAddress(int socket, out SocketAddress address);

        void GetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);
        void SetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);
    }

    public interface IDns {
        void GetHostByName(string name, out string canonicalName, out byte[][] addresses);
    }
}
