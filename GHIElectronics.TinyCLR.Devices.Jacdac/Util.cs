using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.Jacdac {
    public static class Util {
        public static byte[] Slice(byte[] source, int start, int end = 0) {
            if (start < 0) {
                start = source.Length + start;
            }

            if (end == 0) {

                var dest = new byte[source.Length - start];

                Array.Copy(source, start, dest, 0, dest.Length);

                return dest;

            }
            else {
                if (end > 0) {
                    var length = end - start;

                    var dest = new byte[length];

                    Array.Copy(source, start, dest, 0, dest.Length);

                    return dest;
                }
                else {
                    end = source.Length + end;

                    var length = end - start;

                    var dest = new byte[length];

                    Array.Copy(source, start, dest, 0, dest.Length);

                    return dest;
                }
            }
        }

        public static void Set(byte[] dest, byte[] source, int start) {
            var i1 = 0;

            for (var i2 = start; i1 < source.Length;) {

                dest[i2] = source[i1];
                i1++;
            }
        }

        public static byte[] BufferConcat(byte[] a, byte[] b) {
            var r = new byte[a.Length + b.Length];

            Array.Copy(a, 0, r, 0, a.Length);
            Array.Copy(b, 0, r, a.Length, b.Length);

            return r;
        }

        public static string ToHex(byte[] data) {
            var hex = "";
            for (var i = 0; i < data.Length; i++) {
                hex += data[i].ToString("x2");
            }

            return hex;

        }

        public static byte[] FromHex(string hex) {
            var data = new byte[hex.Length / 2];

            for (var i = 0; i < hex.Length; i += 2) {
                var sub = hex.Substring(i, 2);
                data[i >> 1] = (byte)Convert.ToInt32(sub, 16);
            }

            return data;

        }

        public static ushort Read16(byte[] data, int pos) => BitConverter.ToUInt16(data, pos);

        public static uint Read32(byte[] data, int pos) => BitConverter.ToUInt32(data, pos);

        public static void Write16(byte[] data, int pos, ushort v) {
            data[pos + 0] = (byte)((v >> 0) & 0xff);
            data[pos + 1] = (byte)((v >> 8) & 0xff);
        }

        public enum NumberFormat {
            Int8LE = 1,
            UInt8LE = 2,
            Int16LE = 3,
            UInt16LE = 4,
            Int32LE = 5,
            Int8BE = 6,
            UInt8BE = 7,
            Int16BE = 8,
            UInt16BE = 9,
            Int32BE = 10,
            UInt32LE = 11,
            UInt32BE = 12,
            Float32LE = 13,
            Float64LE = 14,
            Float32BE = 15,
            Float64BE = 16,
        }

        public static uint GetNumber(byte[] buf, NumberFormat fmt, int offset) {
            switch (fmt) {
                case NumberFormat.UInt8BE:
                case NumberFormat.UInt8LE:
                    return buf[offset];

                case NumberFormat.Int8BE:
                case NumberFormat.Int8LE:
                    return (uint)(buf[offset] << 24) >> 24;

                case NumberFormat.UInt16LE:
                    return Read16(buf, offset);

                case NumberFormat.Int16LE:
                    return (uint)(Read16(buf, offset) << 16) >> 16;

                case NumberFormat.UInt32LE:
                    return Read32(buf, offset);

                case NumberFormat.Int32LE:
                    return Read32(buf, offset) >> 0;

                default:
                    throw new Exception("unsupported fmt:" + fmt);
            }
        }

        public static ushort CRC(byte[] p) =>
            //ushort crc = 0xffff;
            //for (var i = 0; i < p.Length; ++i) {
            //    var data = p[i];
            //    var x = (crc >> 8) ^ data;
            //    x ^= x >> 4;
            //    crc = (ushort)((crc << 8) ^ (x << 12) ^ (x << 5) ^ x);
            //    crc &= 0xffff;
            //}
            //return crc;

            NativeCrc(p, 0, p.Length);
        public static ushort CRC(byte[] p, int start, int size) =>
            //ushort crc = 0xffff;
            //for (var i = start; i < start + size; ++i) {
            //    var data = p[i];
            //    var x = (crc >> 8) ^ data;
            //    x ^= x >> 4;
            //    crc = (ushort)((crc << 8) ^ (x << 12) ^ (x << 5) ^ x);
            //    crc &= 0xffff;
            //}
            //return crc;
            NativeCrc(p, start, size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern ushort NativeCrc(byte[] data, int offset, int cout);

    }
}
