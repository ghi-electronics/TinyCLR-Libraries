
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System.Net.Sockets;

namespace System.Net {
    public static class Dns {
        public static IPHostEntry GetHostEntry(string hostNameOrAddress) {
            var dns = Sockets.Socket.DefaultProvider;

            dns.GetHostByName(hostNameOrAddress, out var canonicalName, out var addresses);

            var cAddresses = addresses.Length;
            var ipAddresses = new IPAddress[cAddresses];
            var ipHostEntry = new IPHostEntry();

            for (var i = 0; i < cAddresses; i++) {
                var address = addresses[i];

                AddressFamily family;

                if (SystemInfo.IsBigEndian) {
                    family = (AddressFamily)((address[0] << 8) | address[1]);
                }
                else {
                    family = (AddressFamily)((address[1] << 8) | address[0]);
                }
                //port address[2-3]

                if (family == AddressFamily.InterNetwork) {
                    //This only works with IPv4 addresses

                    var ipAddr = (uint)((address[7] << 24) | (address[6] << 16) | (address[5] << 8) | (address[4]));

                    ipAddresses[i] = new IPAddress((long)ipAddr);
                }
            }

            ipHostEntry.hostName = canonicalName;
            ipHostEntry.addressList = ipAddresses;

            return ipHostEntry;
        }
    }
}


