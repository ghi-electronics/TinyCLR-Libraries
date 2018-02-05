using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GHIElectronics.TinyCLR.Networking {
    public enum NetworkInterfaceType {
        Unknown = 1,
        Ethernet = 6,
        Wireless80211 = 71,
    }

    public class NetworkInterface {
        //set update flags...
        private const int UPDATE_FLAGS_DNS = 0x1;
        private const int UPDATE_FLAGS_DHCP = 0x2;
        private const int UPDATE_FLAGS_DHCP_RENEW = 0x4;
        private const int UPDATE_FLAGS_DHCP_RELEASE = 0x8;
        private const int UPDATE_FLAGS_MAC = 0x10;

        private const uint FLAGS_DHCP = 0x1;
        private const uint FLAGS_DYNAMIC_DNS = 0x2;

        private readonly int _interfaceIndex;

        private uint _flags;
        private uint _ipAddress;
        private uint _gatewayAddress;
        private uint _subnetMask;
        private uint _dnsAddress1;
        private uint _dnsAddress2;
        private NetworkInterfaceType _networkInterfaceType;
        private byte[] _macAddress;

        protected NetworkInterface(int interfaceIndex) {
            this._interfaceIndex = interfaceIndex;
            this._networkInterfaceType = NetworkInterfaceType.Unknown;
        }

        public static NetworkInterface[] GetAllNetworkInterfaces() {
            var count = GetNetworkInterfaceCount();
            var ifaces = new NetworkInterface[count];

            for (uint i = 0; i < count; i++) {
                ifaces[i] = GetNetworkInterface(i);
            }

            return ifaces;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static int GetNetworkInterfaceCount();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static NetworkInterface GetNetworkInterface(uint interfaceIndex);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void InitializeNetworkInterfaceSettings();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void UpdateConfiguration(int updateType);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern uint IPAddressFromString(string ipAddress);

        private string IPAddressToString(uint ipAddress) {
            if (SystemInfo.IsBigEndian) {
                return string.Concat(
                                ((ipAddress >> 24) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 16) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 8) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 0) & 0xFF).ToString()
                                );
            }
            else {
                return string.Concat(
                                ((ipAddress >> 0) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 8) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 16) & 0xFF).ToString(),
                                 ".",
                                ((ipAddress >> 24) & 0xFF).ToString()
                                );
            }
        }

        public void EnableStaticIP(string ipAddress, string subnetMask, string gatewayAddress) {
            try {
                this._ipAddress = IPAddressFromString(ipAddress);
                this._subnetMask = IPAddressFromString(subnetMask);
                this._gatewayAddress = IPAddressFromString(gatewayAddress);
                this._flags &= ~FLAGS_DHCP;

                UpdateConfiguration(UPDATE_FLAGS_DHCP);
            }
            finally {
                ReloadSettings();
            }
        }

        public void EnableDhcp() {
            try {
                this._flags |= FLAGS_DHCP;
                UpdateConfiguration(UPDATE_FLAGS_DHCP);
            }
            finally {
                ReloadSettings();
            }
        }

        public void EnableStaticDns(string[] dnsAddresses) {
            if (dnsAddresses == null || dnsAddresses.Length == 0 || dnsAddresses.Length > 2) {
                throw new ArgumentException();
            }

            var addresses = new uint[2];

            var iAddress = 0;
            for (var i = 0; i < dnsAddresses.Length; i++) {
                var address = IPAddressFromString(dnsAddresses[i]);

                addresses[iAddress] = address;

                if (address != 0) {
                    iAddress++;
                }
            }

            try {
                this._dnsAddress1 = addresses[0];
                this._dnsAddress2 = addresses[1];

                this._flags &= ~FLAGS_DYNAMIC_DNS;

                UpdateConfiguration(UPDATE_FLAGS_DNS);
            }
            finally {
                ReloadSettings();
            }
        }

        public void EnableDynamicDns() {
            try {
                this._flags |= FLAGS_DYNAMIC_DNS;

                UpdateConfiguration(UPDATE_FLAGS_DNS);
            }
            finally {
                ReloadSettings();
            }
        }

        public string IPAddress => IPAddressToString(this._ipAddress);

        public string GatewayAddress => IPAddressToString(this._gatewayAddress);

        public string SubnetMask => IPAddressToString(this._subnetMask);

        public bool IsDhcpEnabled => (this._flags & FLAGS_DHCP) != 0;

        public bool IsDynamicDnsEnabled => (this._flags & FLAGS_DYNAMIC_DNS) != 0;

        public string[] DnsAddresses {
            get {
                var list = new ArrayList();

                if (this._dnsAddress1 != 0) {
                    list.Add(IPAddressToString(this._dnsAddress1));
                }

                if (this._dnsAddress2 != 0) {
                    list.Add(IPAddressToString(this._dnsAddress2));
                }

                return (string[])list.ToArray(typeof(string));
            }
        }

        private void ReloadSettings() {
            Thread.Sleep(100);
            InitializeNetworkInterfaceSettings();
        }

        public void ReleaseDhcpLease() {
            try {
                UpdateConfiguration(UPDATE_FLAGS_DHCP_RELEASE);
            }
            finally {
                ReloadSettings();
            }
        }

        public void RenewDhcpLease() {
            try {
                UpdateConfiguration(UPDATE_FLAGS_DHCP_RELEASE | UPDATE_FLAGS_DHCP_RENEW);
            }
            finally {
                ReloadSettings();
            }
        }

        public byte[] PhysicalAddress {
            get => this._macAddress;
            set {
                try {
                    this._macAddress = value;
                    UpdateConfiguration(UPDATE_FLAGS_MAC);
                }
                finally {
                    ReloadSettings();
                }
            }
        }

        public NetworkInterfaceType NetworkInterfaceType => this._networkInterfaceType;
    }
}
