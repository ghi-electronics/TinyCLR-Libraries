using System;
using System.IO;
using BclHMACSHA256 = System.Security.Cryptography.HMACSHA256;

namespace GHIElectronics.TinyCLR.Cryptography {
    internal class HMACSHA256 : IDisposable {

        public byte[] Key { get; set; }
        public byte[] Hash { get; internal set; }
        public string HashName { get; set; }
        public int HashSize => 256;

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

            return (byte[])this.Hash.Clone();
        }

        public byte[] ComputeHash(Stream inputStream) {
            if (inputStream == null) throw new ArgumentNullException();
            using (var hmac = new BclHMACSHA256(this.Key)) {
                this.Hash = hmac.ComputeHash(inputStream);
            }
            return (byte[])this.Hash.Clone();
        }

        public void Initialize() {
            this.Hash = new byte[32];
        }

        public void Dispose() { }

        private static byte[] GenerateRandomKey(int size) {
            var key = new byte[size];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create()) {
                rng.GetBytes(key);
            }
            return key;
        }
    }
}
