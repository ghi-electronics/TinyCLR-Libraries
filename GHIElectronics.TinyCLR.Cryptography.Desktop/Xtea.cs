using System;

namespace GHIElectronics.TinyCLR.Cryptography {
    public class Xtea {
        private readonly uint[] key;

        public Xtea(uint[] key) {
            this.key = key ?? throw new ArgumentNullException(nameof(key));
            if (this.key.Length != 4) throw new ArgumentOutOfRangeException(nameof(key));
        }

        public byte[] Encrypt(byte[] buffer, uint offset, uint count) =>
            throw new NotSupportedException("TODO - Not supported on Desktop: device XTEA mode/rounds unspecified.");

        public byte[] Decrypt(byte[] buffer, uint offset, uint count) =>
            throw new NotSupportedException("TODO - Not supported on Desktop: device XTEA mode/rounds unspecified.");
    }
}
