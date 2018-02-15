using System.Collections;
using GHIElectronics.TinyCLR.Net.NetworkInterface;

namespace System.Net.NetworkInterface {
    public enum NetworkInterfaceType {
        Unknown = 1,
        Ethernet = 6,
        TokenRing = 9,
        Fddi = 15,
        BasicIsdn = 20,
        PrimaryIsdn = 21,
        Ppp = 23,
        Loopback = 24,
        Ethernet3Megabit = 26,
        Slip = 28, // GenericSlip
        Atm = 37,
        GenericModem = 48, // GenericModem
        FastEthernetT = 62, // FastEthernet(100BaseT)
        Isdn = 63, // ISDNandX.25
        FastEthernetFx = 69, // FastEthernet(100BaseFX)
        Wireless80211 = 71, // IEEE80211
        AsymmetricDsl = 94, // AsymmetricDigitalSubscrbrLoop
        RateAdaptDsl = 95, // Rate-AdaptDigitalSubscrbrLoop
        SymmetricDsl = 96, // SymmetricDigitalSubscriberLoop
        VeryHighSpeedDsl = 97, // VeryH-SpeedDigitalSubscrbLoop
        IPOverAtm = 114,
        GigabitEthernet = 117,
        Tunnel = 131,
        MultiRateSymmetricDsl = 143, // Multi-rate Symmetric DSL
        HighPerformanceSerialBus = 144, // ieee1394
        Wman = 237, // IF_TYPE_IEEE80216_WMAN WIMAX
        Wwanpp = 243, // IF_TYPE_WWANPP Mobile Broadband devices based on GSM technology
        Wwanpp2 = 244, // IF_TYPE_WWANPP2 Mobile Broadband devices based on CDMA technology
    }

    public enum OperationalStatus {
        Up = 1,
        Down,
        Testing,
        Unknown,
        Dormant,
        NotPresent,
        LowerLayerDown
    }

    public enum NetworkInterfaceComponent {
        IPv4,
        IPv6
    }

    public abstract class NetworkInterface {
        private static ArrayList interfaces = new ArrayList();
        private static NetworkInterface active;

        public static void RegisterNetworkInterface(NetworkInterface ni) => NetworkInterface.interfaces.Add(ni ?? throw new ArgumentNullException());

        public static void DeregisterNetworkInterface(NetworkInterface ni) {
            if (ni == null) throw new ArgumentNullException();

            if (NetworkInterface.ActiveNetworkInterface == ni)
                NetworkInterface.ActiveNetworkInterface = null;

            NetworkInterface.interfaces.Remove(ni);
        }

        internal static ISocketProvider GetActiveForSocket() => (NetworkInterface.ActiveNetworkInterface ?? throw new InvalidOperationException("No active interface")) is ISocketProvider t ? t : throw new InvalidOperationException("Active interface does not support sockets.");
        internal static IDnsProvider GetActiveForDns() => (NetworkInterface.ActiveNetworkInterface ?? throw new InvalidOperationException("No active interface")) is IDnsProvider t ? t : throw new InvalidOperationException("Active interface does not support DNS.");

        public static NetworkInterface ActiveNetworkInterface {
            get => NetworkInterface.active;
            set => NetworkInterface.active = value == null || Array.IndexOf(NetworkInterface.GetAllNetworkInterfaces(), value) != -1 ? value : throw new InvalidOperationException();
        }

        public static NetworkInterface[] GetAllNetworkInterfaces() => (NetworkInterface[])NetworkInterface.interfaces.ToArray(typeof(NetworkInterface));

        public static bool GetIsNetworkAvailable() {
            try {
                var networkInterfaces = GetAllNetworkInterfaces();
                foreach (var netInterface in networkInterfaces) {
                    if (netInterface.OperationalStatus == OperationalStatus.Up && netInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                        && netInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback) {
                        return true;
                    }
                }
            }
            catch (Exception) {
            }

            return false;
        }

        public static int LoopbackInterfaceIndex => throw new NotSupportedException();

        public static int IPv6LoopbackInterfaceIndex => throw new NotSupportedException();

        public virtual string Id => throw new NotImplementedException();

        public virtual string Name => throw new NotImplementedException();

        public virtual string Description => throw new NotImplementedException();

        public virtual OperationalStatus OperationalStatus => throw new NotImplementedException();

        public virtual long Speed => throw new NotImplementedException();

        public virtual bool IsReceiveOnly => throw new NotImplementedException();

        public virtual bool SupportsMulticast => throw new NotImplementedException();

        public virtual PhysicalAddress GetPhysicalAddress() => throw new NotImplementedException();

        public virtual NetworkInterfaceType NetworkInterfaceType => throw new NotImplementedException();

        public virtual bool Supports(NetworkInterfaceComponent networkInterfaceComponent) => throw new NotImplementedException();

    }
}
