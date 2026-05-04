using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Cryptography {
    public class HMACSHA1 : IDisposable {
        private const byte IPAD = 0x36;
        private const byte OPAD = 0x5C;

        private readonly SHA1 digest;
        private readonly int digestSize = 20;
        private readonly int blockLength = BYTE_LENGTH;
        private readonly byte[] inputPad;
        private readonly byte[] outputBuf;

        private const int BYTE_LENGTH = 64;

        private byte[] hash;
        private byte[] _key;

        public byte[] Hash => this.hash;
        public int HashSize => 160;
        public byte[] Key {
            get => this._key;
            set {
                this._key = value ?? throw new ArgumentNullException();
                this.Initialize(this._key);
            }
        }
        public string HashName => "SHA1";

        public HMACSHA1() : this(null) {

        }
        public HMACSHA1(byte[] key) {

            this.digest = SHA1.Create();
            this.hash = new byte[this.digestSize];
            this.inputPad = new byte[this.blockLength];
            this.outputBuf = new byte[this.blockLength + this.digestSize];

            // Setting Key triggers Initialize internally; null key generates a random one.
            this.Key = key ?? GenerateRandomKey(64);
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

            if (offset < 0 || count < 0 || offset + count > buffer.Length) {
                throw new ArgumentOutOfRangeException();
            }

            this.BlockUpdate(buffer, offset, count);

            this.DoFinal(this.hash, 0);

            return (byte[])this.hash.Clone();
        }

        public byte[] ComputeHash(Stream inputStream) {
            if (inputStream == null) throw new ArgumentNullException();

            var buf = new byte[64];
            int read;
            while ((read = inputStream.Read(buf, 0, buf.Length)) > 0) {
                this.BlockUpdate(buf, 0, read);
            }

            this.DoFinal(this.hash, 0);
            return (byte[])this.hash.Clone();
        }

        // .NET HMAC.Initialize() resets internal state without re-deriving from key.
        public void Initialize() => this.Reset();

        public void Dispose() { }

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

        private static byte[] GenerateRandomKey(int size) {
            var key = new byte[size];

            try {
                using (var rsa = new RSACryptoServiceProvider(1024)) {
                    var seed = rsa.ExportParameters(false).Modulus;

                    if (seed == null || seed.Length == 0)
                        throw new InvalidOperationException();

                    FillWithSha1Expansion(key, seed);
                    return key;
                }
            }
            catch {
                var random = new Random();
                random.NextBytes(key);
                return key;
            }
        }

        private static void FillWithSha1Expansion(byte[] destination, byte[] seed) {
            var sha1 = SHA1.Create();
            var counter = 0;
            var destOffset = 0;

            while (destOffset < destination.Length) {
                var input = new byte[seed.Length + 4];
                Array.Copy(seed, 0, input, 0, seed.Length);
                input[seed.Length] = (byte)(counter >> 24);
                input[seed.Length + 1] = (byte)(counter >> 16);
                input[seed.Length + 2] = (byte)(counter >> 8);
                input[seed.Length + 3] = (byte)counter;
                counter++;

                var block = sha1.ComputeHash(input);
                var copyLen = Math.Min(block.Length, destination.Length - destOffset);
                Array.Copy(block, 0, destination, destOffset, copyLen);
                destOffset += copyLen;
            }
        }
    }
}
