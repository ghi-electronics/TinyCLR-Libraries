using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Cryptography {
    public class HMACSHA1 {
        private const byte IPAD = 0x36;
        private const byte OPAD = 0x5C;

        private readonly SHA1 digest;
        private readonly int digestSize = 20;
        private readonly int blockLength = BYTE_LENGTH;
        private readonly byte[] inputPad;
        private readonly byte[] outputBuf;

        private const int BYTE_LENGTH = 64;

        private byte[] hash;
        public byte[] Hash => this.hash;
        public byte[] Key { get; internal set; }
        public string HashName => "SHA1";

        public HMACSHA1() : this(null) {

        }
        public HMACSHA1(byte[] key) {

            this.digest = SHA1.Create();
            this.hash = new byte[this.digestSize];
            this.inputPad = new byte[this.blockLength];
            this.outputBuf = new byte[this.blockLength + this.digestSize];

            if (key != null) {
                this.Key = key;
            }
            else {
                this.Key = new byte[64];

                var random = new Random();

                random.NextBytes(this.Key);
            }

            this.Initialize(this.Key);
        }

        private void Initialize(byte[] key) {
            this.digest.Clear();

            var keyLength = key.Length;

            if (keyLength > this.blockLength) {
                this.digest.BlockUpdate(key, 0, keyLength);
                this.digest.DoFinal(this.inputPad, 0);

                keyLength = this.digestSize;
            }
            else {
                Array.Copy(key, 0, this.inputPad, 0, keyLength);
            }

            Array.Clear(this.inputPad, keyLength, this.blockLength - keyLength);
            Array.Copy(this.inputPad, 0, this.outputBuf, 0, this.blockLength);

            XorPad(this.inputPad, this.blockLength, IPAD);
            XorPad(this.outputBuf, this.blockLength, OPAD);

            this.digest.BlockUpdate(this.inputPad, 0, this.inputPad.Length);
        }

        public byte[] ComputeHash(byte[] buffer) => this.ComputeHash(buffer, 0, buffer.Length);
        public byte[] ComputeHash(byte[] buffer, int offset, int count) {
            if (buffer == null) {
                throw new ArgumentNullException();
            }

            if (offset + count > buffer.Length) {
                throw new ArgumentOutOfRangeException();
            }

            this.BlockUpdate(buffer, offset, count);

            this.DoFinal(this.hash, 0);

            return this.hash;
        }

        private void BlockUpdate(byte[] input, int inOff, int len) => this.digest.BlockUpdate(input, inOff, len);

        private int DoFinal(byte[] output, int outOff) {
            this.digest.DoFinal(this.outputBuf, this.blockLength);


            this.digest.BlockUpdate(this.outputBuf, 0, this.outputBuf.Length);


            var len = this.digest.DoFinal(output, outOff);

            Array.Clear(this.outputBuf, this.blockLength, this.digestSize);

            this.digest.BlockUpdate(this.inputPad, 0, this.inputPad.Length);


            return len;
        }

        /**
        * Reset the mac generator.
        */
        public void Reset() {
            // Reset underlying digest
            this.digest.Clear();

            // Initialise the digest
            this.digest.BlockUpdate(this.inputPad, 0, this.inputPad.Length);
        }

        private static void XorPad(byte[] pad, int len, byte n) {
            for (var i = 0; i < len; ++i) {
                pad[i] ^= n;
            }
        }
    }
}
