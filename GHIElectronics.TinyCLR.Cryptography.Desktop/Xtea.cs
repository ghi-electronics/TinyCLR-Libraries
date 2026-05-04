using System;

namespace GHIElectronics.TinyCLR.Cryptography {
    // Standard XTEA (eXtended TEA), 32 rounds, ECB mode, 128-bit key.
    // Pure managed equivalent of TinyCLR's native firmware Xtea — produces identical
    // output on Desktop + Device when called with the same key and 8-byte-aligned buffers.
    public class Xtea {
        private const uint Delta = 0x9E3779B9;
        private const int Rounds = 32;

        private readonly uint[] key;

        public Xtea(uint[] key) {
            this.key = key ?? throw new ArgumentNullException(nameof(key));
            if (this.key.Length != 4) throw new ArgumentOutOfRangeException(nameof(key));
        }

        public byte[] Encrypt(byte[] buffer, uint offset, uint count) {
            ValidateBuffer(buffer, offset, count);

            var result = new byte[count];
            for (uint i = 0; i < count; i += 8) {
                var v0 = ReadUInt32(buffer, (int)(offset + i));
                var v1 = ReadUInt32(buffer, (int)(offset + i + 4));

                uint sum = 0;
                for (var r = 0; r < Rounds; r++) {
                    v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + this.key[sum & 3]);
                    sum += Delta;
                    v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + this.key[(sum >> 11) & 3]);
                }

                WriteUInt32(v0, result, (int)i);
                WriteUInt32(v1, result, (int)(i + 4));
            }
            return result;
        }

        public byte[] Decrypt(byte[] buffer, uint offset, uint count) {
            ValidateBuffer(buffer, offset, count);

            var result = new byte[count];
            for (uint i = 0; i < count; i += 8) {
                var v0 = ReadUInt32(buffer, (int)(offset + i));
                var v1 = ReadUInt32(buffer, (int)(offset + i + 4));

                var sum = unchecked((uint)(Delta * Rounds));
                for (var r = 0; r < Rounds; r++) {
                    v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + this.key[(sum >> 11) & 3]);
                    sum -= Delta;
                    v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + this.key[sum & 3]);
                }

                WriteUInt32(v0, result, (int)i);
                WriteUInt32(v1, result, (int)(i + 4));
            }
            return result;
        }

        private static void ValidateBuffer(byte[] buffer, uint offset, uint count) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset + count > buffer.Length || count == 0) throw new ArgumentException();
            if (count % 8 != 0) throw new ArgumentException("count must be a multiple of 8 (XTEA block size).");
        }

        // Big-endian uint32 read/write (XTEA convention).
        private static uint ReadUInt32(byte[] buf, int offset) =>
            ((uint)buf[offset] << 24) | ((uint)buf[offset + 1] << 16) | ((uint)buf[offset + 2] << 8) | buf[offset + 3];

        private static void WriteUInt32(uint v, byte[] buf, int offset) {
            buf[offset] = (byte)(v >> 24);
            buf[offset + 1] = (byte)(v >> 16);
            buf[offset + 2] = (byte)(v >> 8);
            buf[offset + 3] = (byte)v;
        }
    }
}
