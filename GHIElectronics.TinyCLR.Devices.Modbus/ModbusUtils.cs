using System;

namespace GHIElectronics.TinyCLR.Devices.Modbus {
    /// <summary>
    /// Class with modbus utility functions
    /// </summary>
    public static class ModbusUtils {
        /// <summary>
        /// Inserts a unsigned short value into a byte array in big-endian format.
        /// </summary>
        /// <param name="buffer">Byte array to write to</param>
        /// <param name="pos">Index to write to.</param>
        /// <param name="value">Value to write.</param>
        public static void InsertUShort(byte[] buffer, int pos, ushort value) {
            buffer[pos] = (byte)((value & 0xff00) >> 8);
            buffer[pos + 1] = (byte)(value & 0x00ff);
        }

        /// <summary>
        /// Extracts a unsigned short value from an byte array in big-endian format.
        /// </summary>
        /// <param name="buffer">Byte array to read from</param>
        /// <param name="pos">Index to read from</param>
        /// <returns>Returns the unsigned short value.</returns>
        public static ushort ExtractUShort(byte[] buffer, int pos) => (ushort)((buffer[pos] << 8) + buffer[pos + 1]);

        /// <summary>
        /// Calculates the Modbus RTU CRC16 checksumm
        /// </summary>
        /// <param name="buffer">Buffer containing the telegram.</param>
        /// <param name="count">Count of bytes to use for CRC (not including the 2 bytes for CRC).</param>
        /// <returns>Returns the 16 bit CRC</returns>
        public static ushort CalcCrc(byte[] buffer, int count) {
            ushort crc = 0xffff;
            for (var i = 0; i < count; i++) {
                crc = (ushort)(crc ^ buffer[i]);
                for (var j = 0; j < 8; j++) {
                    var lsbHigh = (crc & 0x0001) != 0;
                    crc = (ushort)((crc >> 1) & 0x7FFF);

                    if (lsbHigh) {
                        crc = (ushort)(crc ^ 0xa001);
                    }
                }
            }
            return crc;
        }
    }
}
