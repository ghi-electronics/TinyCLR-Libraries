using System.Text;

namespace System.Net.NetworkInterface {
    public class PhysicalAddress {
        byte[] address = null;
        bool changed = true;
        int hash = 0;

        // FxCop: if this class is ever made mutable (like, given any non-readonly fields),
        // the readonly should be removed from the None decoration.
        public static readonly PhysicalAddress None = new PhysicalAddress(new byte[0]);

        // constructors
        public PhysicalAddress(byte[] address) => this.address = address;

        public override int GetHashCode() {
            if (this.changed) {
                this.changed = false;
                this.hash = 0;

                int i;
                var size = this.address.Length & ~3;

                for (i = 0; i < size; i += 4) {
                    this.hash ^= (int)this.address[i]
                            | ((int)this.address[i + 1] << 8)
                            | ((int)this.address[i + 2] << 16)
                            | ((int)this.address[i + 3] << 24);
                }
                if ((this.address.Length & 3) != 0) {

                    var remnant = 0;
                    var shift = 0;

                    for (; i < this.address.Length; ++i) {
                        remnant |= ((int)this.address[i]) << shift;
                        shift += 8;
                    }
                    this.hash ^= remnant;
                }
            }
            return this.hash;
        }

        public override bool Equals(object comparand) {
            var address = comparand as PhysicalAddress;
            if (address == null)
                return false;

            if (this.address.Length != address.address.Length) {
                return false;
            }
            for (var i = 0; i < address.address.Length; i++) {
                if (this.address[i] != address.address[i])
                    return false;
            }
            return true;
        }


        public override string ToString() {
            var addressString = new StringBuilder();

            foreach (var value in this.address) {

                var tmp = (value >> 4) & 0x0F;

                for (var i = 0; i < 2; i++) {
                    if (tmp < 0x0A) {
                        addressString.Append((char)(tmp + 0x30));
                    }
                    else {
                        addressString.Append((char)(tmp + 0x37));
                    }
                    tmp = ((int)value & 0x0F);
                }
            }
            return addressString.ToString();
        }

        public byte[] GetAddressBytes() {
            var tmp = new byte[this.address.Length];
            Array.Copy(this.address, 0, tmp, 0, this.address.Length);
            return tmp;
        }

        public static PhysicalAddress Parse(string address) {
            var validCount = 0;
            var hasDashes = false;
            byte[] buffer = null;

            if (address == null) {
                return PhysicalAddress.None;
            }

            //has dashes?
            if (address.IndexOf('-') >= 0) {
                hasDashes = true;
                buffer = new byte[(address.Length + 1) / 3];
            }
            else {

                if (address.Length % 2 > 0) {  //should be even
                    throw new FormatException();
                }

                buffer = new byte[address.Length / 2];
            }

            var j = 0;
            for (var i = 0; i < address.Length; i++) {

                var value = (int)address[i];

                if (value >= 0x30 && value <= 0x39) {
                    value -= 0x30;
                }
                else if (value >= 0x41 && value <= 0x46) {
                    value -= 0x37;
                }
                else if (value == (int)'-') {
                    if (validCount == 2) {
                        validCount = 0;
                        continue;
                    }
                    else {
                        throw new FormatException();
                    }
                }
                else {
                    throw new FormatException();
                }

                //we had too many characters after the last dash
                if (hasDashes && validCount >= 2) {
                    throw new FormatException();
                }

                if (validCount % 2 == 0) {
                    buffer[j] = (byte)(value << 4);
                }
                else {
                    buffer[j++] |= (byte)value;
                }

                validCount++;
            }

            //we too few characters after the last dash
            if (validCount < 2) {
                throw new FormatException();
            }

            return new PhysicalAddress(buffer);
        }
    }
}
