using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Cryptography {
    public class Crc16 {       
        public Crc16() {

        }

        public ushort ComputeHash(byte[] data) => this.ComputeHash(data, 0, data.Length, 0);

        public ushort ComputeHash(byte[] data, int offset, int count) => this.Calculate(data, offset, count, 0);

        public ushort ComputeHash(byte[] data, int offset, int count, ushort seed) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), "Must not be negative.");
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Must not be negative.");
            if (data.Length < offset + count) throw new ArgumentException("Invalid buffer size.", nameof(data));

            return this.Calculate(data, offset, count, seed);
            
        } 

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern ushort Calculate(byte[] data, int offset, int count, ushort seed);
    }
}
