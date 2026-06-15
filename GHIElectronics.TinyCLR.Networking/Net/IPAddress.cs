////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Net.Sockets;

namespace System.Net {
    /// <devdoc>
    ///    <para>Provides an internet protocol (IP) address.</para>
    /// </devdoc>
    [Serializable]
    public class IPAddress {

        /// <summary>An IP address that indicates any available address (0.0.0.0).</summary>
        public static readonly IPAddress Any = new IPAddress(0x0000000000000000);
        /// <summary>The loopback IP address (127.0.0.1).</summary>
        public static readonly IPAddress Loopback = new IPAddress(0x000000000100007F);
        /// <summary>The limited broadcast IP address (255.255.255.255).</summary>
        public static readonly IPAddress Broadcast = new IPAddress(0x00000000FFFFFFFF);
        /// <summary>An IP address that indicates no address.</summary>
        public static readonly IPAddress None = Broadcast;
        internal long m_Address;

        private AddressFamily m_Family = AddressFamily.InterNetwork;

        /// <summary>Initializes a new instance from an address specified as a 32-bit value.</summary>
        public IPAddress(long newAddress) {
            if (newAddress < 0 || newAddress > 0x00000000FFFFFFFF) {
                throw new ArgumentOutOfRangeException();
            }

            this.m_Address = newAddress;
        }

        /// <summary>Initializes a new instance from an address specified as a byte array.</summary>
        public IPAddress(byte[] newAddressBytes)
            : this(((((newAddressBytes[3] << 0x18) | (newAddressBytes[2] << 0x10)) | (newAddressBytes[1] << 0x08)) | newAddressBytes[0]) & ((long)0xFFFFFFFF)) {
        }

        /// <summary>Determines whether the specified object is equal to this IP address.</summary>
        public override bool Equals(object obj) {
            var addr = obj as IPAddress;

            if (addr == null) return false;

            return this.m_Address == addr.m_Address;
        }

        /// <summary>Returns the IP address as a byte array.</summary>
        public byte[] GetAddressBytes() => new byte[]
            {
                (byte)(this.m_Address),
                (byte)(this.m_Address >> 8),
                (byte)(this.m_Address >> 16),
                (byte)(this.m_Address >> 24)
            };

        /// <summary>Attempts to parse a string into an IP address, returning whether it succeeded.</summary>
        public static bool TryParse(string ipString, out IPAddress address) {
            try {
                address = Parse(ipString);
                return true;
            }
            catch {
                address = null;
                return false;
            }
        }

        /// <summary>Parses a dotted-quad string into an IP address.</summary>
        public static IPAddress Parse(string ipString) {
            if (ipString == null)
                throw new ArgumentNullException();

            ulong ipAddress = 0L;
            var lastIndex = 0;
            var shiftIndex = 0;
            ulong mask = 0x00000000000000FF;
            ulong octet = 0L;
            var length = ipString.Length;

            for (var i = 0; i < length; ++i) {
                // Parse to '.' or end of IP address
                if (ipString[i] == '.' || i == length - 1)
                    // If the IP starts with a '.'
                    // or a segment is longer than 3 characters or shiftIndex > last bit position throw.
                    if (i == 0 || i - lastIndex > 3 || shiftIndex > 24) {
                        throw new ArgumentException();
                    }
                    else {
                        i = i == length - 1 ? ++i : i;
                        octet = (ulong)(ConvertStringToInt32(ipString.Substring(lastIndex, i - lastIndex)) & 0x00000000000000FF);
                        ipAddress = ipAddress + (ulong)((octet << shiftIndex) & mask);
                        lastIndex = i + 1;
                        shiftIndex = shiftIndex + 8;
                        mask = (mask << 8);
                    }
            }

            return new IPAddress((long)ipAddress);
        }

        /// <summary>Returns a hash code for this IP address.</summary>
        public override int GetHashCode() => unchecked((int)this.m_Address);

        // ----------------------------------------------------------------
        // Static helpers — match full .NET surface so binary-protocol code
        // (port packing, raw socket payloads) ports without conditionals.
        // ----------------------------------------------------------------

        /// <summary>Determines whether the specified IP address is a loopback address.</summary>
        public static bool IsLoopback(IPAddress address) {
            if (address == null) throw new ArgumentNullException(nameof(address));
            // 127.0.0.0/8. Match any address whose first octet is 127.
            return ((byte)address.m_Address) == 127;
        }

        /// <summary>Converts a 16-bit value from host byte order to network byte order.</summary>
        public static short HostToNetworkOrder(short host) =>
            unchecked((short)((host & 0xFF) << 8 | (host >> 8) & 0xFF));

        /// <summary>Converts a 32-bit value from host byte order to network byte order.</summary>
        public static int HostToNetworkOrder(int host) =>
            unchecked((int)(((uint)HostToNetworkOrder((short)host) & 0xFFFF) << 16
                         | ((uint)HostToNetworkOrder((short)(host >> 16)) & 0xFFFF)));

        /// <summary>Converts a 64-bit value from host byte order to network byte order.</summary>
        public static long HostToNetworkOrder(long host) =>
            unchecked(((long)HostToNetworkOrder((int)host) & 0xFFFFFFFFL) << 32
                   | ((long)HostToNetworkOrder((int)(host >> 32)) & 0xFFFFFFFFL));

        /// <summary>Converts a 16-bit value from network byte order to host byte order.</summary>
        public static short NetworkToHostOrder(short network) => HostToNetworkOrder(network);
        /// <summary>Converts a 32-bit value from network byte order to host byte order.</summary>
        public static int NetworkToHostOrder(int network) => HostToNetworkOrder(network);
        /// <summary>Converts a 64-bit value from network byte order to host byte order.</summary>
        public static long NetworkToHostOrder(long network) => HostToNetworkOrder(network);

        /// <summary>Returns the IP address as a dotted-quad string.</summary>
        public override string ToString() => ((byte)(this.m_Address)).ToString() +
                    "." +
                    ((byte)(this.m_Address >> 8)).ToString() +
                    "." +
                    ((byte)(this.m_Address >> 16)).ToString() +
                    "." +
                    ((byte)(this.m_Address >> 24)).ToString();

        //--//
        ////////////////////////////////////////////////////////////////////////////////////////
        // this method ToInt32 is part of teh Convert class which we will bring over later
        // at that time we will get rid of this code
        //

        /// <summary>
        /// Converts the specified System.String representation of a number to an equivalent
        /// 32-bit signed integer.
        /// </summary>
        /// <param name="value">A System.String containing a number to convert.</param>
        /// <returns>
        /// A 32-bit signed integer equivalent to the value of value.-or- Zero if value
        /// is null.
        /// </returns>
        /// <exception cref="System.OverflowException">
        /// Value represents a number less than System.Int32.MinValue or greater than
        /// System.Int32.MaxValue.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// The value parameter is null.
        /// </exception>
        /// <exception cref="System.FormatException">
        /// Value does not consist of an optional sign followed by a sequence of digits
        /// (zero through nine).
        /// </exception>
        private static int ConvertStringToInt32(string value) {
            var num = value.ToCharArray();
            var result = 0;

            var isNegative = false;
            var signIndex = 0;

            if (num[0] == '-') {
                isNegative = true;
                signIndex = 1;
            }
            else if (num[0] == '+') {
                signIndex = 1;
            }

            var exp = 1;
            for (var i = num.Length - 1; i >= signIndex; i--) {
                if (num[i] < '0' || num[i] > '9') {
                    throw new ArgumentException();
                }

                result += ((num[i] - '0') * exp);
                exp *= 10;
            }

            return (isNegative) ? (-1 * result) : result;
        }

        internal bool IsBroadcast
        {
            get
            {
                return m_Address == Broadcast.m_Address;
            }
        }

        /// <summary>The address family of the IP address.</summary>
        public AddressFamily AddressFamily
        {
            get
            {
                return m_Family;
            }
        }

    } // class IPAddress
} // namespace System.Net


