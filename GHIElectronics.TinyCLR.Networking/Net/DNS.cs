
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


using System.Collections;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using GHIElectronics.TinyCLR.Native;

namespace System.Net {
    /// <summary>Provides simple host name resolution functionality.</summary>
    public static class Dns {
        /// <summary>Returns the host name of the local device.</summary>
        // Returns the device's host name. On full .NET this is the OS host
        // name; on TinyCLR we surface the device name from the Native lib —
        // it's the meaningful "what is my host" identifier on a microcontroller
        // and is configurable via DeviceInformation.SetDeviceName.
        public static string GetHostName() => DeviceInformation.DeviceName ?? string.Empty;

        // Serialize callers so the non-blocking native state machine only ever
        // services one query at a time (otherwise a late-waking thread could
        // pick up another thread's resolved IP).
        private static readonly object dnsLock = new object();

        // Overall managed-side timeout for a single DNS resolution.
        private const int DnsTimeoutMs = 15000;
        private const int DnsPollMs = 50;

        /// <summary>Resolves a host name or IP address to an array of IP addresses.</summary>
        // Convenience wrapper around GetHostEntry that returns just the IP
        // addresses. Matches full .NET signature.
        public static IPAddress[] GetHostAddresses(string hostNameOrAddress) =>
            GetHostEntry(hostNameOrAddress).AddressList;

        /// <summary>Resolves a host name or IP address to an IPHostEntry containing its addresses.</summary>
        public static IPHostEntry GetHostEntry(string hostNameOrAddress) {
            if (hostNameOrAddress == null) throw new ArgumentNullException(nameof(hostNameOrAddress));

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
                        throw new SocketException(SocketError.TryAgain);
                    Thread.Sleep(DnsPollMs);
                    elapsed += DnsPollMs;
                }
            }

            // Filter to IPv4 only and PACK without null slots. Previously the
            // result array kept a slot per native entry and left nulls where
            // the family was non-InterNetwork (e.g. IPv6) — callers doing
            // entry.AddressList[0].ToString() could NRE.
            var ipv4 = new ArrayList();
            for (var i = 0; i < addresses.Length; i++) {
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
                    var ipAddr = (uint)((address[7] << 24) | (address[6] << 16) | (address[5] << 8) | (address[4]));
                    ipv4.Add(new IPAddress((long)ipAddr));
                }
            }

            var ipAddresses = new IPAddress[ipv4.Count];
            for (var i = 0; i < ipv4.Count; i++)
                ipAddresses[i] = (IPAddress)ipv4[i];

            var ipHostEntry = new IPHostEntry();
            ipHostEntry.hostName = canonicalName;
            ipHostEntry.addressList = ipAddresses;

            return ipHostEntry;
        }
    }

    /// <summary>Provides multicast DNS (mDNS) host name advertisement.</summary>
    public static class MulticastDns {
        /// <summary>Starts advertising the specified host name over multicast DNS with the given time-to-live.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static void Start(string hostname, TimeSpan dnsTTL);

        /// <summary>Stops multicast DNS host name advertisement.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern static void Stop();

    }
}


