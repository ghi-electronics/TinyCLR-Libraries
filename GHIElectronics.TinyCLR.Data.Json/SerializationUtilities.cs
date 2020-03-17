using System;
using System.Text;

namespace GHIElectronics.TinyCLR.Data.Json
{
    public static class SerializationUtilities
    {
        public static void Marshall(byte[] buffer, ref int offset, object arg)
        {
            var type = arg.GetType();
            if (type == typeof(byte))
            {
                buffer[offset++] = (byte)arg;
            }
            else if (type == typeof(Int16))
            {
                buffer[offset++] = (byte)(((Int16)arg) & 0xff);
                buffer[offset++] = (byte)(((Int16)arg >> 8) & 0xff);
            }
            else if (type == typeof(UInt16))
            {
                buffer[offset++] = (byte)(((UInt16)arg) & 0xff);
                buffer[offset++] = (byte)(((UInt16)arg >> 8) & 0xff);
            }
            else if (type == typeof(Int32))
            {
                buffer[offset++] = (byte)(((Int32)arg) & 0xff);
                buffer[offset++] = (byte)(((Int32)arg >> 8) & 0xff);
                buffer[offset++] = (byte)(((Int32)arg >> 16) & 0xff);
                buffer[offset++] = (byte)(((Int32)arg >> 24) & 0xff);
            }
            else if (type == typeof(UInt32))
            {
                buffer[offset++] = (byte)(((UInt32)arg) & 0xff);
                buffer[offset++] = (byte)(((UInt32)arg >> 8) & 0xff);
                buffer[offset++] = (byte)(((UInt32)arg >> 16) & 0xff);
                buffer[offset++] = (byte)(((UInt32)arg >> 24) & 0xff);
            }
            else if (type == typeof(Int64))
            {
                buffer[offset++] = (byte)(((Int64)arg) & 0xff);
                buffer[offset++] = (byte)(((Int64)arg >> 8) & 0xff);
                buffer[offset++] = (byte)(((Int64)arg >> 16) & 0xff);
                buffer[offset++] = (byte)(((Int64)arg >> 24) & 0xff);

                buffer[offset++] = (byte)(((Int64)arg >> 32) & 0xff);
                buffer[offset++] = (byte)(((Int64)arg >> 40) & 0xff);
                buffer[offset++] = (byte)(((Int64)arg >> 48) & 0xff);
                buffer[offset++] = (byte)(((Int64)arg >> 56) & 0xff);
            }
            else if (type == typeof(UInt64))
            {
                buffer[offset++] = (byte)(((UInt64)arg) & 0xff);
                buffer[offset++] = (byte)(((UInt64)arg >> 8) & 0xff);
                buffer[offset++] = (byte)(((UInt64)arg >> 16) & 0xff);
                buffer[offset++] = (byte)(((UInt64)arg >> 24) & 0xff);

                buffer[offset++] = (byte)(((UInt64)arg >> 32) & 0xff);
                buffer[offset++] = (byte)(((UInt64)arg >> 40) & 0xff);
                buffer[offset++] = (byte)(((UInt64)arg >> 48) & 0xff);
                buffer[offset++] = (byte)(((UInt64)arg >> 56) & 0xff);
            }
            else if (type == typeof(DateTime))
            {
                Marshall(buffer, ref offset, ((DateTime)arg).Ticks);
            }
            else if (type == typeof(byte[])) // special case for the typecode array
            {
                var value = (byte[])arg;
                var length = value.Length;
                Array.Copy(value, 0, buffer, offset, length);
                offset += length;
            }
            else if (type == typeof(string)) // string and unknown types
            {
                var value = Encoding.UTF8.GetBytes(arg.ToString());
                int length = value.Length;
                Array.Copy(value, 0, buffer, offset, length);
                offset += length;
                buffer[offset++] = 0;
            }
            else if (type == typeof(float))
            {
                var bytes = BitConverter.GetBytes((double)((float)arg));
                foreach (var b in bytes)
                {
                    buffer[offset++] = b;
                }
            }
            else if (type == typeof(double))
            {
                var bytes = BitConverter.GetBytes((double)arg);
                foreach (var b in bytes)
                {
                    buffer[offset++] = b;
                }
            }
            else
                throw new Exception("unsupported type for Marshall");
        }

        public static object Unmarshall(byte[] buffer, ref int offset, TypeCode tc)
        {
            object result = null;
            switch (tc)
            {
                case TypeCode.Empty: // secret code for the argument typecode array
                    var argCount = (int)buffer[offset++];
                    var tcArray = new TypeCode[argCount];
                    for (var i = 0; i < argCount; ++i)
                    {
                        tcArray[i] = (TypeCode)buffer[offset++];
                    }
                    result = tcArray;
                    break;
                case TypeCode.Byte:
                    result = buffer[offset++];
                    break;
                case TypeCode.Int16:
                    result = (Int16)(buffer[offset] | buffer[offset + 1] << 8);
                    offset += 2;
                    break;
                case TypeCode.UInt16:
                    result = (UInt16)(buffer[offset] | buffer[offset + 1] << 8);
                    offset += 2;
                    break;
                case TypeCode.Int32:
                    result = (Int32)(buffer[offset] | buffer[offset + 1] << 8 | buffer[offset + 2] << 16 | buffer[offset + 3] << 24);
                    offset += 4;
                    break;
                case TypeCode.UInt32:
                    result = (UInt32)(buffer[offset] | buffer[offset + 1] << 8 | buffer[offset + 2] << 16 | buffer[offset + 3] << 24);
                    offset += 4;
                    break;
                case TypeCode.Int64:
                    result = (Int64)(((UInt64)buffer[offset]) | ((UInt64)buffer[offset + 1]) << 8 | ((UInt64)buffer[offset + 2]) << 16 | ((UInt64)buffer[offset + 3]) << 24 |
                                     ((UInt64)buffer[offset + 4]) << 32 | ((UInt64)buffer[offset + 5]) << 40 | ((UInt64)buffer[offset + 6]) << 48 | ((UInt64)buffer[offset + 7]) << 56);
                    offset += 8;
                    break;
                case TypeCode.UInt64:
                    result = (UInt64)(((UInt64)buffer[offset]) | ((UInt64)buffer[offset + 1]) << 8 | ((UInt64)buffer[offset + 2]) << 16 | ((UInt64)buffer[offset + 3]) << 24 |
                                     ((UInt64)buffer[offset + 4]) << 32 | ((UInt64)buffer[offset + 5]) << 40 | ((UInt64)buffer[offset + 6]) << 48 | ((UInt64)buffer[offset + 7]) << 56);
                    offset += 8;
                    break;
                case TypeCode.DateTime:
                    result = new DateTime((long)Unmarshall(buffer, ref offset, TypeCode.Int64));
                    break;
                case TypeCode.String:
                    var idxNul = JToken.FindNul(buffer, offset);
                    if (idxNul == -1)
                        throw new Exception("Missing ename terminator");
                    result = JToken.ConvertToString(buffer, offset, idxNul - offset);
                    offset = idxNul + 1;
                    break;
                case TypeCode.Double:
                    var i64 = (Int64)(((UInt64)buffer[offset]) | ((UInt64)buffer[offset + 1]) << 8 | ((UInt64)buffer[offset + 2]) << 16 | ((UInt64)buffer[offset + 3]) << 24 |
                                     ((UInt64)buffer[offset + 4]) << 32 | ((UInt64)buffer[offset + 5]) << 40 | ((UInt64)buffer[offset + 6]) << 48 | ((UInt64)buffer[offset + 7]) << 56);
                    result = BitConverter.Int64BitsToDouble(i64);
                    offset += 8;
                    break;
                default:
                    throw new Exception("Unsupported type");
            }
            return result;
        }
    }
}
