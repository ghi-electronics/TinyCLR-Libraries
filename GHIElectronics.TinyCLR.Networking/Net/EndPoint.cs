////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace System.Net
{

    // Generic abstraction to identify network addresses

    /// <devdoc>
    ///    <para>
    ///       Identifies a network address.
    ///    </para>
    /// </devdoc>
    [Serializable]
    public abstract class EndPoint
    {
        /// <summary>The address family to which the endpoint belongs.</summary>
        // Default returns Unspecified, matching full .NET. Concrete subclasses
        // (e.g. IPEndPoint) override to surface their actual family.
        public virtual AddressFamily AddressFamily => AddressFamily.Unspecified;

        /// <summary>Serializes endpoint information into a SocketAddress instance.</summary>
        public abstract SocketAddress Serialize();
        /// <summary>Creates an endpoint from a socket address.</summary>
        public abstract EndPoint Create(SocketAddress socketAddress);

    }; // abstract class EndPoint

} // namespace System.Net


