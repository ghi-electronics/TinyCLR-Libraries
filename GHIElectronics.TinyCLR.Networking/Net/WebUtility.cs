//------------------------------------------------------------------------------ 
// Copyright (C) 2021 GHI Electronics
//
// This file is a modified version from Microsoft.
//
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Networking.Net {
    public static class WebUtility {
        public static string UrlEncode(string value) {
            if (value == null)
                return null;

            var bytes = Encoding.UTF8.GetBytes(value);
            return Encoding.UTF8.GetString(UrlEncode(bytes, 0, bytes.Length, false /* alwaysCreateNewReturnValue */));
        }

        private static byte[] UrlEncode(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue) {
            var encoded = UrlEncode(bytes, offset, count);

            return (alwaysCreateNewReturnValue && (encoded != null) && (encoded == bytes))
                ? (byte[])encoded.Clone()
                : encoded;
        }

        private static byte[] UrlEncode(byte[] bytes, int offset, int count) {
            if (!ValidateUrlEncodingParameters(bytes, offset, count)) {
                return null;
            }

            var cSpaces = 0;
            var cUnsafe = 0;

            // count them first
            for (var i = 0; i < count; i++) {
                var ch = (char)bytes[offset + i];

                if (ch == ' ')
                    cSpaces++;
                else if (!IsUrlSafeChar(ch))
                    cUnsafe++;
            }

            // nothing to expand?
            if (cSpaces == 0 && cUnsafe == 0) {
                // DevDiv 912606: respect "offset" and "count"
                if (0 == offset && bytes.Length == count) {
                    return bytes;
                }
                else {
                    var subarray = new byte[count];
                    Array.Copy(bytes, offset, subarray, 0, count);
                    return subarray;
                }
            }

            // expand not 'safe' characters into %XX, spaces to +s
            var expandedBytes = new byte[count + cUnsafe * 2];
            var pos = 0;

            for (var i = 0; i < count; i++) {
                var b = bytes[offset + i];
                var ch = (char)b;

                if (IsUrlSafeChar(ch)) {
                    expandedBytes[pos++] = b;
                }
                else if (ch == ' ') {
                    expandedBytes[pos++] = (byte)'+';
                }
                else {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)IntToHex(b & 0x0f);
                }
            }

            return expandedBytes;
        }

        private static char IntToHex(int n) {
            if (n <= 9)
                return (char)(n + (int)'0');
            else
                return (char)(n - 10 + (int)'A');
        }

        // Set of safe chars, from RFC 1738.4 minus '+'
        private static bool IsUrlSafeChar(char ch) {
            if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9')
                return true;

            switch (ch) {
                case '-':
                case '_':
                case '.':
                case '!':
                case '*':
                case '(':
                case ')':
                    return true;
            }

            return false;
        }

        private static bool ValidateUrlEncodingParameters(byte[] bytes, int offset, int count) {
            if (bytes == null && count == 0)
                return false;
            if (bytes == null) {
                throw new ArgumentNullException("bytes");
            }
            if (offset < 0 || offset > bytes.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > bytes.Length) {
                throw new ArgumentOutOfRangeException("count");
            }

            return true;
        }

    }
}
