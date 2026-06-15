using System;
using System.IO;

namespace GHIElectronics.TinyCLR.Cryptography {
    internal class SHA256 : IDisposable {
        private const int DigestLength = 32;
        public int HashSize => 256;
        private readonly byte[] hash;
        private readonly uint[] x = new uint[64];
        private readonly byte[] xBuf;

        private int xBufOff;
        private int xOff;
        private long byteCount;

        private uint h1, h2, h3, h4, h5, h6, h7, h8;

        public byte[] Hash => this.hash;

        private SHA256() {
            this.xBuf = new byte[4];
            this.hash = new byte[DigestLength];
            this.Clear();
        }

        public static SHA256 Create() => new SHA256();

        public byte[] ComputeHash(byte[] buffer) => this.ComputeHash(buffer, 0, buffer.Length);

        public byte[] ComputeHash(byte[] buffer, int offset, int count) {
            if (buffer == null)
                throw new ArgumentNullException();

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            this.BlockUpdate(buffer, offset, count);
            this.DoFinal(this.hash, 0);
            return (byte[])this.hash.Clone();
        }

        public byte[] ComputeHash(Stream inputStream) {
            if (inputStream == null) throw new ArgumentNullException();

            var buffer = new byte[64];
            int read;
            while ((read = inputStream.Read(buffer, 0, buffer.Length)) > 0) {
                this.BlockUpdate(buffer, 0, read);
            }

            this.DoFinal(this.hash, 0);
            return (byte[])this.hash.Clone();
        }

        public void Initialize() => this.Clear();

        public void Dispose() { }

        public void Clear() {
            this.byteCount = 0;
            this.xBufOff = 0;
            Array.Clear(this.xBuf, 0, this.xBuf.Length);
            this.xOff = 0;
            Array.Clear(this.x, 0, this.x.Length);

            this.h1 = 0x6a09e667;
            this.h2 = 0xbb67ae85;
            this.h3 = 0x3c6ef372;
            this.h4 = 0xa54ff53a;
            this.h5 = 0x510e527f;
            this.h6 = 0x9b05688c;
            this.h7 = 0x1f83d9ab;
            this.h8 = 0x5be0cd19;
        }

        private void Update(byte input) {
            this.xBuf[this.xBufOff++] = input;

            if (this.xBufOff == this.xBuf.Length) {
                this.ProcessWord(this.xBuf, 0);
                this.xBufOff = 0;
            }

            this.byteCount++;
        }

        private void BlockUpdate(byte[] input, int inOff, int length) {
            length = Math.Max(0, length);

            var i = 0;
            if (this.xBufOff != 0) {
                while (i < length) {
                    this.xBuf[this.xBufOff++] = input[inOff + i++];
                    if (this.xBufOff == 4) {
                        this.ProcessWord(this.xBuf, 0);
                        this.xBufOff = 0;
                        break;
                    }
                }
            }

            var limit = ((length - i) & ~3) + i;
            for (; i < limit; i += 4)
                this.ProcessWord(input, inOff + i);

            while (i < length)
                this.xBuf[this.xBufOff++] = input[inOff + i++];

            this.byteCount += length;
        }

        private int DoFinal(byte[] output, int outOff) {
            this.Finish();

            UInt32ToBE(this.h1, output, outOff + 0);
            UInt32ToBE(this.h2, output, outOff + 4);
            UInt32ToBE(this.h3, output, outOff + 8);
            UInt32ToBE(this.h4, output, outOff + 12);
            UInt32ToBE(this.h5, output, outOff + 16);
            UInt32ToBE(this.h6, output, outOff + 20);
            UInt32ToBE(this.h7, output, outOff + 24);
            UInt32ToBE(this.h8, output, outOff + 28);

            this.Clear();

            return DigestLength;
        }

        private void Finish() {
            var bitLength = this.byteCount << 3;

            this.Update(0x80);

            while (this.xBufOff != 0)
                this.Update(0);

            this.ProcessLength(bitLength);
            this.ProcessBlock();
        }

        private void ProcessWord(byte[] input, int inOff) {
            this.x[this.xOff] = BEToUInt32(input, inOff);

            if (++this.xOff == 16)
                this.ProcessBlock();
        }

        private void ProcessLength(long bitLength) {
            if (this.xOff > 14)
                this.ProcessBlock();

            this.x[14] = (uint)((ulong)bitLength >> 32);
            this.x[15] = (uint)((ulong)bitLength);
        }

        private void ProcessBlock() {
            for (var t = 16; t <= 63; t++)
                this.x[t] = Theta1(this.x[t - 2]) + this.x[t - 7] + Theta0(this.x[t - 15]) + this.x[t - 16];

            var a = this.h1;
            var b = this.h2;
            var c = this.h3;
            var d = this.h4;
            var e = this.h5;
            var f = this.h6;
            var g = this.h7;
            var h = this.h8;

            for (var t = 0; t <= 63; t++) {
                var t1 = h + Sum1(e) + Ch(e, f, g) + K[t] + this.x[t];
                var t2 = Sum0(a) + Maj(a, b, c);
                h = g;
                g = f;
                f = e;
                e = d + t1;
                d = c;
                c = b;
                b = a;
                a = t1 + t2;
            }

            this.h1 += a;
            this.h2 += b;
            this.h3 += c;
            this.h4 += d;
            this.h5 += e;
            this.h6 += f;
            this.h7 += g;
            this.h8 += h;

            this.xOff = 0;
            Array.Clear(this.x, 0, 16);
        }

        private static uint Ch(uint x, uint y, uint z) => (x & y) ^ (~x & z);
        private static uint Maj(uint x, uint y, uint z) => (x & y) ^ (x & z) ^ (y & z);
        private static uint Sum0(uint x) => RotateRight(x, 2) ^ RotateRight(x, 13) ^ RotateRight(x, 22);
        private static uint Sum1(uint x) => RotateRight(x, 6) ^ RotateRight(x, 11) ^ RotateRight(x, 25);
        private static uint Theta0(uint x) => RotateRight(x, 7) ^ RotateRight(x, 18) ^ (x >> 3);
        private static uint Theta1(uint x) => RotateRight(x, 17) ^ RotateRight(x, 19) ^ (x >> 10);
        private static uint RotateRight(uint x, int n) => (x >> n) | (x << (32 - n));

        private static void UInt32ToBE(uint n, byte[] bs, int off) {
            bs[off] = (byte)(n >> 24);
            bs[off + 1] = (byte)(n >> 16);
            bs[off + 2] = (byte)(n >> 8);
            bs[off + 3] = (byte)n;
        }

        private static uint BEToUInt32(byte[] bs, int off) =>
            (uint)bs[off] << 24 |
            (uint)bs[off + 1] << 16 |
            (uint)bs[off + 2] << 8 |
            (uint)bs[off + 3];

        private static readonly uint[] K = {
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
            0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
            0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
            0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
            0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
            0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
            0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
            0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
            0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
            0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
            0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
            0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
            0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
            0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
            0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
            0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
        };
    }
}
