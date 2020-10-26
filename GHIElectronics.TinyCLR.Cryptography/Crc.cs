using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Cryptography {
    public class Crc16 {
        private ushort crc16;

        public Crc16() => this.crc16 = 0;

        public ushort ComputeHash(byte[] data) => this.ComputeHash(data, 0, data.Length);

        public ushort ComputeHash(byte[] data, int offset, int count) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), "Must not be negative.");
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Must not be negative.");
            if (data.Length < offset + count) throw new ArgumentException("Invalid buffer size.", nameof(data));

            this.crc16 = this.Calculate(data, offset, count, this.crc16);

            return this.crc16;
        }

        public void Reset() => this.crc16 = 0;

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern ushort Calculate(byte[] data, int offset, int count, ushort crc16);
    }
}
