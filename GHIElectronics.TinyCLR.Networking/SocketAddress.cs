////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net {
    using System.Diagnostics;
    using System.Net.Sockets;

    public class SocketAddress
    {
        internal const int IPv4AddressSize = 16;

        internal byte[] m_Buffer;

        public AddressFamily Family => (AddressFamily)(this.m_Buffer[0] | (this.m_Buffer[1] << 8));

        internal SocketAddress(byte[] address) => this.m_Buffer = address;

        public SocketAddress(AddressFamily family, int size)
        {
            Debug.Assert(size > 2);

            this.m_Buffer = new byte[size]; //(size / IntPtr.Size + 2) * IntPtr.Size];//sizeof DWORD

            this.m_Buffer[0] = unchecked((byte)((int)family     ));
            this.m_Buffer[1] = unchecked((byte)((int)family >> 8));
        }

        public int Size => this.m_Buffer.Length;

        public byte this[int offset] {
            get => this.m_Buffer[offset];
            set => this.m_Buffer[offset] = value;
        }

    } // class SocketAddress
} // namespace System.Net


