using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Cryptography {
    public class HMACSHA256 {

        public byte[] Key { get; internal set; }
        public byte[] Hash { get; internal set; }
        public string HashName { get; internal set; }

        public HMACSHA256() {
            this.Key = new byte[32];

            var random = new Random();

            random.NextBytes(this.Key);
        }

        public HMACSHA256(byte[] key) {
            this.Key = key;
            this.Hash = new byte[32];
        }

        public byte[] ComputeHash(byte[] buffer) {
            if (buffer == null) {
                throw new ArgumentNullException();
            }

            return this.ComputeHash(buffer, 0, buffer.Length);
        }

        public byte[] ComputeHash(byte[] buffer, int offset, int count) {
            if (buffer == null) {
                throw new ArgumentNullException();
            }

            if (offset + count > buffer.Length) {
                throw new ArgumentOutOfRangeException();
            }

            this.NativeComputeHash(buffer, offset, count, this.Key, this.Hash);

            return this.Hash;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeComputeHash(byte[] buffer, int offset, int count, byte[] key, byte[] hash);
    }
}
