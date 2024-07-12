using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace System.Net.Sockets {
    public class MulticastOption {
        private IPAddress _group;
        private IPAddress _localAddress;
        private int _ifIndex;

        // Creates a new instance of the MulticastOption class with the specified IP address
        // group and local address.
        public MulticastOption(IPAddress group, IPAddress mcint) {
            this._group = group ?? throw new ArgumentNullException("group");
            this.LocalAddress = mcint ?? throw new ArgumentNullException("mcint");
        }

        // Creates a new version of the MulticastOption class for the specified group.
        public MulticastOption(IPAddress group) {
            this._group = group ?? throw new ArgumentNullException("group");

            this.LocalAddress = IPAddress.Any;
        }

        // Sets the IP address of a multicast group.
        public IPAddress Group {
            get => this._group;
            set => this._group = value ?? throw new ArgumentNullException("value");
        }

        // Sets the local address of a multicast group.
        public IPAddress LocalAddress {
            get => this._localAddress;
            set {
                this._ifIndex = 0;
                this._localAddress = value;
            }
        }

        public int InterfaceIndex {
            get => this._ifIndex;
            set {
                if (value < 0 || value > 0x00FFFFFF) throw new ArgumentOutOfRangeException("interfaceIndex");

                this._localAddress = null;
                this._ifIndex = value;
            }
        }

        public byte[] ToBytes() {
            var tobytes = new byte[8];

            Array.Copy(this._group.GetAddressBytes(), 0, tobytes, 0, 4);
            Array.Copy(this._localAddress.GetAddressBytes(), 0, tobytes, 4, 4);

            return tobytes;

        }
    }
}
