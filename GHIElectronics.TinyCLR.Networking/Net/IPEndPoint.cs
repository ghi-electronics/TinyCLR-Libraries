////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net {
    using System.Diagnostics;
    using System.Net.Sockets;

    [Serializable]
    public class IPEndPoint : EndPoint {
        public const int MinPort = 0x00000000;
        public const int MaxPort = 0x0000FFFF;

        private IPAddress m_Address;
        private int m_Port;

        public IPEndPoint(long address, int port) {
            this.m_Port = port;
            this.m_Address = new IPAddress(address);
        }

        public IPEndPoint(IPAddress address, int port) {
            this.m_Port = port;
            this.m_Address = address;
        }

        public IPAddress Address => this.m_Address;

        public int Port => this.m_Port;

        public override SocketAddress Serialize() {
            // create a new SocketAddress
            //
            var socketAddress = new SocketAddress(AddressFamily.InterNetwork, SocketAddress.IPv4AddressSize);
            var buffer = socketAddress.m_Buffer;
            //
            // populate it
            //
            buffer[2] = unchecked((byte)(this.m_Port >> 8));
            buffer[3] = unchecked((byte)(this.m_Port));

            buffer[4] = unchecked((byte)(this.m_Address.m_Address));
            buffer[5] = unchecked((byte)(this.m_Address.m_Address >> 8));
            buffer[6] = unchecked((byte)(this.m_Address.m_Address >> 16));
            buffer[7] = unchecked((byte)(this.m_Address.m_Address >> 24));

            return socketAddress;
        }

        public override EndPoint Create(SocketAddress socketAddress) {
            // strip out of SocketAddress information on the EndPoint
            //

            var buf = socketAddress.m_Buffer;

            Debug.Assert(socketAddress.Family == AddressFamily.InterNetwork);

            var port = (int)(
                    (buf[2] << 8 & 0xFF00) |
                    (buf[3])
                    );

            var address = (long)(
                    (buf[4] & 0x000000FF) |
                    (buf[5] << 8 & 0x0000FF00) |
                    (buf[6] << 16 & 0x00FF0000) |
                    (buf[7] << 24)
                    ) & 0x00000000FFFFFFFF;

            var created = new IPEndPoint(address, port);

            return created;
        }

        public override string ToString() => this.m_Address.ToString() + ":" + this.m_Port.ToString();

        public override bool Equals(object obj) {
            var ep = obj as IPEndPoint;
            if (ep == null) {
                return false;
            }

            return ep.m_Address.Equals(this.m_Address) && ep.m_Port == this.m_Port;
        }

        public override int GetHashCode() => this.m_Address.GetHashCode() ^ this.m_Port;
    } // class IPEndPoint
} // namespace System.Net


