using System;

namespace GHIElectronics.TinyCLR.Cryptography {
    public class Crc16 {
        public Crc16() { }

        public ushort ComputeHash(byte[] data) =>
            throw new NotSupportedException("TODO - Not supported on Desktop: device CRC16 polynomial unspecified.");

        public ushort ComputeHash(byte[] data, int offset, int count) =>
            throw new NotSupportedException("TODO - Not supported on Desktop: device CRC16 polynomial unspecified.");

        public ushort ComputeHash(byte[] data, int offset, int count, ushort seed) =>
            throw new NotSupportedException("TODO - Not supported on Desktop: device CRC16 polynomial unspecified.");
    }
}
