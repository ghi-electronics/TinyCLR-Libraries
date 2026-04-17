
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Net {
    public static class Dns {
        // Serialize callers so the non-blocking native state machine only ever
        // services one query at a time (otherwise a late-waking thread could
        // pick up another thread's resolved IP).
        private static readonly object dnsLock = new object();

        // Overall managed-side timeout for a single DNS resolution.
        private const int DnsTimeoutMs = 15000;
        private const int DnsPollMs = 50;

        public static IPHostEntry GetHostEntry(string hostNameOrAddress) {
            var dns = Sockets.Socket.DefaultProvider;

            string canonicalName;
            SocketAddress[] addresses;

            lock (dnsLock) {
                // Poll the native non-blocking resolver. While we sleep between
                // polls, the interpreter runs other managed threads (UI etc.).
                var elapsed = 0;
                while (true) {
                    dns.GetHostByName(hostNameOrAddress, out canonicalName, out addresses);
                    if (addresses != null && addresses.Length > 0) break;
                    if (elapsed >= DnsTimeoutMs)
                        throw new Exception("DNS resolution timed out for \"" + hostNameOrAddress + "\"");
                    Thread.Sleep(DnsPollMs);
                    elapsed += DnsPollMs;
                }
            }

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

    public static class MulticastDns {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static void Start(string hostname, TimeSpan dnsTTL);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static void Stop();

    }
}


