////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net {
    using System.Diagnostics;
    using System.Net.Sockets;

    /// <summary>Stores serialized information from an endpoint.</summary>
    public class SocketAddress
    {
        internal const int IPv4AddressSize = 16;

        internal byte[] m_Buffer;

        /// <summary>The address family of the socket address.</summary>
        public AddressFamily Family => (AddressFamily)(this.m_Buffer[0] | (this.m_Buffer[1] << 8));

        internal SocketAddress(byte[] address) => this.m_Buffer = address;

        /// <summary>Initializes a new instance for the given address family and buffer size.</summary>
        public SocketAddress(AddressFamily family, int size)
        {
            Debug.Assert(size > 2);

            this.m_Buffer = new byte[size]; //(size / IntPtr.Size + 2) * IntPtr.Size];//sizeof DWORD

            this.m_Buffer[0] = unchecked((byte)((int)family     ));
            this.m_Buffer[1] = unchecked((byte)((int)family >> 8));
        }

        /// <summary>The size, in bytes, of the underlying buffer.</summary>
        public int Size => this.m_Buffer.Length;

        /// <summary>Gets or sets the byte at the specified offset in the buffer.</summary>
        public byte this[int offset] {
            get => this.m_Buffer[offset];
            set => this.m_Buffer[offset] = value;
        }

        /// <summary>Determines whether the specified object is equal to this socket address.</summary>
        public override bool Equals(object comparand) {
            var other = comparand as SocketAddress;
            if (other == null || this.Size != other.Size) return false;
            for (var i = 0; i < this.Size; i++) {
                if (this[i] != other[i]) return false;
            }
            return true;
        }

        /// <summary>Returns a hash code for this socket address.</summary>
        public override int GetHashCode() {
            // FNV-1a over the buffer. Stable across runs; cheap on embedded.
            var hash = unchecked((int)2166136261);
            for (var i = 0; i < this.m_Buffer.Length; i++) {
                hash = unchecked((hash ^ this.m_Buffer[i]) * 16777619);
            }
            return hash;
        }

        /// <summary>Returns a string representation of the socket address.</summary>
        public override string ToString() {
            // Matches full .NET shape: "Family:Size:{b2,b3,...}" — skips the
            // 2 leading family bytes since those are already shown.
            var sb = new System.Text.StringBuilder();
            sb.Append(this.Family.ToString());
            sb.Append(":");
            sb.Append(this.Size.ToString());
            sb.Append(":{");
            for (var i = 2; i < this.Size; i++) {
                if (i > 2) sb.Append(",");
                sb.Append(this.m_Buffer[i].ToString());
            }
            sb.Append("}");
            return sb.ToString();
        }

    } // class SocketAddress
} // namespace System.Net


