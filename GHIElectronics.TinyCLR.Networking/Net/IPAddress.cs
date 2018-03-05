////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace System.Net {
    /// <devdoc>
    ///    <para>Provides an internet protocol (IP) address.</para>
    /// </devdoc>
    [Serializable]
    public class IPAddress {

        public static readonly IPAddress Any = new IPAddress(0x0000000000000000);
        public static readonly IPAddress Loopback = new IPAddress(0x000000000100007F);
        internal long m_Address;

        public IPAddress(long newAddress) {
            if (newAddress < 0 || newAddress > 0x00000000FFFFFFFF) {
                throw new ArgumentOutOfRangeException();
            }

            this.m_Address = newAddress;
        }

        public IPAddress(byte[] newAddressBytes)
            : this(((((newAddressBytes[3] << 0x18) | (newAddressBytes[2] << 0x10)) | (newAddressBytes[1] << 0x08)) | newAddressBytes[0]) & ((long)0xFFFFFFFF)) {
        }

        public override bool Equals(object obj) {
            var addr = obj as IPAddress;

            if (obj == null) return false;

            return this.m_Address == addr.m_Address;
        }

        public byte[] GetAddressBytes() => new byte[]
            {
                (byte)(this.m_Address),
                (byte)(this.m_Address >> 8),
                (byte)(this.m_Address >> 16),
                (byte)(this.m_Address >> 24)
            };

        public static IPAddress Parse(string ipString) {
            if (ipString == null)
                throw new ArgumentNullException();

            ulong ipAddress = 0L;
            var lastIndex = 0;
            var shiftIndex = 0;
            ulong mask = 0x00000000000000FF;
            ulong octet = 0L;
            var length = ipString.Length;

            for (var i = 0; i < length; ++i) {
                // Parse to '.' or end of IP address
                if (ipString[i] == '.' || i == length - 1)
                    // If the IP starts with a '.'
                    // or a segment is longer than 3 characters or shiftIndex > last bit position throw.
                    if (i == 0 || i - lastIndex > 3 || shiftIndex > 24) {
                        throw new ArgumentException();
                    }
                    else {
                        i = i == length - 1 ? ++i : i;
                        octet = (ulong)(ConvertStringToInt32(ipString.Substring(lastIndex, i - lastIndex)) & 0x00000000000000FF);
                        ipAddress = ipAddress + (ulong)((octet << shiftIndex) & mask);
                        lastIndex = i + 1;
                        shiftIndex = shiftIndex + 8;
                        mask = (mask << 8);
                    }
            }

            return new IPAddress((long)ipAddress);
        }

        public override int GetHashCode() => unchecked((int)this.m_Address);

        public override string ToString() => ((byte)(this.m_Address)).ToString() +
                    "." +
                    ((byte)(this.m_Address >> 8)).ToString() +
                    "." +
                    ((byte)(this.m_Address >> 16)).ToString() +
                    "." +
                    ((byte)(this.m_Address >> 24)).ToString();

        //--//
        ////////////////////////////////////////////////////////////////////////////////////////
        // this method ToInt32 is part of teh Convert class which we will bring over later
        // at that time we will get rid of this code
        //

        /// <summary>
        /// Converts the specified System.String representation of a number to an equivalent
        /// 32-bit signed integer.
        /// </summary>
        /// <param name="value">A System.String containing a number to convert.</param>
        /// <returns>
        /// A 32-bit signed integer equivalent to the value of value.-or- Zero if value
        /// is null.
        /// </returns>
        /// <exception cref="System.OverflowException">
        /// Value represents a number less than System.Int32.MinValue or greater than
        /// System.Int32.MaxValue.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// The value parameter is null.
        /// </exception>
        /// <exception cref="System.FormatException">
        /// Value does not consist of an optional sign followed by a sequence of digits
        /// (zero through nine).
        /// </exception>
        private static int ConvertStringToInt32(string value) {
            var num = value.ToCharArray();
            var result = 0;

            var isNegative = false;
            var signIndex = 0;

            if (num[0] == '-') {
                isNegative = true;
                signIndex = 1;
            }
            else if (num[0] == '+') {
                signIndex = 1;
            }

            var exp = 1;
            for (var i = num.Length - 1; i >= signIndex; i--) {
                if (num[i] < '0' || num[i] > '9') {
                    throw new ArgumentException();
                }

                result += ((num[i] - '0') * exp);
                exp *= 10;
            }

            return (isNegative) ? (-1 * result) : result;
        }
    } // class IPAddress
} // namespace System.Net


