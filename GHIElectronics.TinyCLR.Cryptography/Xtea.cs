using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Cryptography {
    /// <summary>XTEA (eXtended Tiny Encryption Algorithm) block cipher. 128-bit key, 64-bit blocks.</summary>
    public class Xtea {
        private readonly uint[] key;

        /// <summary>Creates an XTEA cipher with the given 128-bit key (four 32-bit words).</summary>
        public Xtea(uint[] key) {
            this.key = key ?? throw new ArgumentNullException(nameof(key));

            if (this.key.Length != 4) throw new ArgumentOutOfRangeException(nameof(key));
        }

        /// <summary>Encrypts a range of the buffer and returns the ciphertext.</summary>
        public byte[] Encrypt(byte[] buffer, uint offset, uint count) {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset + count > buffer.Length || count == 0)
                throw new ArgumentException();

            return this.NativeEncrypt(buffer, offset, count, this.key);
        }

        /// <summary>Decrypts a range of the buffer and returns the plaintext.</summary>
        public byte[] Decrypt(byte[] buffer, uint offset, uint count) {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset + count > buffer.Length || count == 0)
                throw new ArgumentException();

            return this.NativeDecrypt(buffer, offset, count, this.key);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern byte[] NativeEncrypt(byte[] buffer, uint offset, uint count, uint[] key);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern byte[] NativeDecrypt(byte[] buffer, uint offset, uint count, uint[] key);
    }
}
