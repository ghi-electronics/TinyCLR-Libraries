using System;
using BclHMACSHA256 = System.Security.Cryptography.HMACSHA256;

namespace GHIElectronics.TinyCLR.Cryptography {
    public class HMACSHA256 {

        public byte[] Key { get; internal set; }
        public byte[] Hash { get; internal set; }
        public string HashName { get; internal set; }

        public HMACSHA256() : this(GenerateRandomKey(32)) { }

        public HMACSHA256(byte[] key) {
            this.Key = key;
            this.Hash = new byte[32];
            this.HashName = "SHA256";
        }

        public byte[] ComputeHash(byte[] buffer) {
            if (buffer == null) throw new ArgumentNullException();
            return this.ComputeHash(buffer, 0, buffer.Length);
        }

        public byte[] ComputeHash(byte[] buffer, int offset, int count) {
            if (buffer == null) throw new ArgumentNullException();
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            using (var hmac = new BclHMACSHA256(this.Key)) {
                this.Hash = hmac.ComputeHash(buffer, offset, count);
            }

            return this.Hash;
        }

        private static byte[] GenerateRandomKey(int size) {
            var key = new byte[size];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create()) {
                rng.GetBytes(key);
            }
            return key;
        }
    }
}
