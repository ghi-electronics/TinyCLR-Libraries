using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Cryptography {
    public class Xtea {
        private readonly uint[] key;

        public Xtea(uint[] key) {
            this.key = key ?? throw new ArgumentNullException(nameof(key));

            if (this.key.Length != 4) throw new ArgumentOutOfRangeException(nameof(key));
        }

        public byte[] Encrypt(byte[] buffer, uint offset, uint count) {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset + count > buffer.Length || count == 0)
                throw new ArgumentException();

            return this.NativeEncrypt(buffer, offset, count, this.key);
        }

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
