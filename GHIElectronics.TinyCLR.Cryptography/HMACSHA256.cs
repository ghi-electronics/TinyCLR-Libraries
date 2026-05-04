using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Cryptography {
    public class HMACSHA256 : IDisposable {

        public byte[] Key { get; set; }
        public byte[] Hash { get; internal set; }
        public string HashName { get; set; }
        public int HashSize => 256;

        public HMACSHA256() {
            this.Key = GenerateRandomKey(32);
            this.Hash = new byte[32];
            this.HashName = "SHA256";
        }

        public HMACSHA256(byte[] key) {
            this.Key = key;
            this.Hash = new byte[32];
            this.HashName = "SHA256";
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

            if (offset < 0 || count < 0 || offset + count > buffer.Length) {
                throw new ArgumentOutOfRangeException();
            }

            this.NativeComputeHash(buffer, offset, count, this.Key, this.Hash);

            return (byte[])this.Hash.Clone();
        }

        // The native HMACSHA256 path is single-shot (key + buffer in, hash out), so
        // streaming requires reading the whole stream first. Acceptable for typical
        // small payloads; large streams should be chunked by the caller.
        public byte[] ComputeHash(Stream inputStream) {
            if (inputStream == null) throw new ArgumentNullException();

            using (var ms = new MemoryStream()) {
                var buf = new byte[4096];
                int read;
                while ((read = inputStream.Read(buf, 0, buf.Length)) > 0) {
                    ms.Write(buf, 0, read);
                }
                return this.ComputeHash(ms.ToArray());
            }
        }

        public void Initialize() {
            // Single-shot HMAC has no incremental state; reset Hash buffer for parity.
            this.Hash = new byte[32];
        }

        public void Dispose() { }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeComputeHash(byte[] buffer, int offset, int count, byte[] key, byte[] hash);

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
