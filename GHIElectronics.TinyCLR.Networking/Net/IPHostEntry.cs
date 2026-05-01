////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net
{

    public class IPHostEntry
    {
        // Backing fields for binary-compat with previous releases. Internal so
        // sibling code (e.g. Dns) can populate them; public properties below
        // mirror full .NET shape (settable).
        internal string hostName;
        internal IPAddress[] addressList;
        internal string[] aliases;

        public string HostName {
            get => this.hostName;
            set => this.hostName = value;
        }

        public IPAddress[] AddressList {
            get => this.addressList;
            set => this.addressList = value;
        }

        public string[] Aliases {
            get => this.aliases;
            set => this.aliases = value;
        }
    } // class IPHostEntry
} // namespace System.Net


