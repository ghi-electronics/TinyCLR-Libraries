////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net {
    using System.Diagnostics;
    using System.Net.Sockets;

    /// <summary>Represents a network endpoint as an IP address and a port number.</summary>
    [Serializable]
    public class IPEndPoint : EndPoint {
        /// <summary>The minimum value that can be assigned to the Port property.</summary>
        public const int MinPort = 0x00000000;
        /// <summary>The maximum value that can be assigned to the Port property.</summary>
        public const int MaxPort = 0x0000FFFF;

        private IPAddress m_Address;
        private int m_Port;

        /// <summary>Initializes a new instance with the specified address and port.</summary>
        public IPEndPoint(long address, int port) {
            if (port < MinPort || port > MaxPort) throw new ArgumentOutOfRangeException(nameof(port));

            this.m_Port = port;
            this.m_Address = new IPAddress(address);
        }

        /// <summary>Initializes a new instance with the specified address and port.</summary>
        public IPEndPoint(IPAddress address, int port) {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (port < MinPort || port > MaxPort) throw new ArgumentOutOfRangeException(nameof(port));

            this.m_Port = port;
            this.m_Address = address;
        }

        /// <inheritdoc/>
        public override AddressFamily AddressFamily => this.m_Address != null ? this.m_Address.AddressFamily : AddressFamily.InterNetwork;

        /// <summary>The IP address of the endpoint.</summary>
        public IPAddress Address {
            get => this.m_Address;
            set => this.m_Address = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>The port number of the endpoint.</summary>
        public int Port {
            get => this.m_Port;
            set {
                if (value < MinPort || value > MaxPort) throw new ArgumentOutOfRangeException(nameof(value));
                this.m_Port = value;
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <summary>Returns the endpoint as an "address:port" string.</summary>
        public override string ToString() => this.m_Address.ToString() + ":" + this.m_Port.ToString();

        /// <summary>Determines whether the specified object is equal to this endpoint.</summary>
        public override bool Equals(object obj) {
            var ep = obj as IPEndPoint;
            if (ep == null) {
                return false;
            }

            return ep.m_Address.Equals(this.m_Address) && ep.m_Port == this.m_Port;
        }

        /// <summary>Returns a hash code for this endpoint.</summary>
        public override int GetHashCode() => this.m_Address.GetHashCode() ^ this.m_Port;

        /// <summary>Parses an "address:port" string into an IP endpoint.</summary>
        // ----------------------------------------------------------------
        // Parse / TryParse — added in .NET Core 3.0+ and very useful for
        // config strings ("192.168.1.10:8080"). IPv4-only on TinyCLR.
        // ----------------------------------------------------------------

        public static IPEndPoint Parse(string s) {
            if (s == null) throw new ArgumentNullException(nameof(s));

            var colonIdx = s.LastIndexOf(':');
            if (colonIdx < 0) throw new FormatException("Endpoint string must contain ':' separating address from port.");

            var addrPart = s.Substring(0, colonIdx);
            var portPart = s.Substring(colonIdx + 1);

            var address = IPAddress.Parse(addrPart);

            // Parse port manually — TinyCLR's int.Parse is available but we
            // already do hand-rolled parsing in this lib, stay consistent.
            if (portPart.Length == 0) throw new FormatException("Missing port.");
            var port = 0;
            for (var i = 0; i < portPart.Length; i++) {
                var c = portPart[i];
                if (c < '0' || c > '9') throw new FormatException("Port must be numeric.");
                port = port * 10 + (c - '0');
                if (port > MaxPort) throw new FormatException("Port out of range.");
            }

            return new IPEndPoint(address, port);
        }

        /// <summary>Attempts to parse an "address:port" string into an IP endpoint, returning whether it succeeded.</summary>
        public static bool TryParse(string s, out IPEndPoint result) {
            try {
                result = Parse(s);
                return true;
            }
            catch {
                result = null;
                return false;
            }
        }
    } // class IPEndPoint
} // namespace System.Net


