////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{

    /// <summary>Holds the host name, aliases, and IP addresses associated with a host.</summary>
    public class IPHostEntry
    {
        // Backing fields for binary-compat with previous releases. Internal so
        // sibling code (e.g. Dns) can populate them; public properties below
        // mirror full .NET shape (settable).
        internal string hostName;
        internal IPAddress[] addressList;
        internal string[] aliases;

        /// <summary>The DNS name of the host.</summary>
        public string HostName {
            get => this.hostName;
            set => this.hostName = value;
        }

        /// <summary>The list of IP addresses associated with the host.</summary>
        public IPAddress[] AddressList {
            get => this.addressList;
            set => this.addressList = value;
        }

        /// <summary>The list of aliases associated with the host.</summary>
        public string[] Aliases {
            get => this.aliases;
            set => this.aliases = value;
        }
    } // class IPHostEntry
} // namespace System.Net


