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

// Namespace was previously GHIElectronics.TinyCLR.Networking.Net which broke
// dual-mode TypeForwardedTo (the shim forwards System.Net.WebUtility). Aligned
// to System.Net to match full .NET. Old-namespace user code needs to update
// `using GHIElectronics.TinyCLR.Networking.Net;` -> `using System.Net;`.
namespace System.Net {
    /// <summary>Provides methods for encoding and decoding URLs and HTML text.</summary>
    public static class WebUtility {
        /// <summary>URL-encodes the specified string.</summary>
        public static string UrlEncode(string value) {
            if (value == null)
                return null;

            var bytes = Encoding.UTF8.GetBytes(value);
            return Encoding.UTF8.GetString(UrlEncode(bytes, 0, bytes.Length, false /* alwaysCreateNewReturnValue */));
        }

        /// <summary>Decodes a URL-encoded string.</summary>
        public static string UrlDecode(string encodedValue) {
            if (encodedValue == null)
                return null;

            var len = encodedValue.Length;
            var bytes = new byte[len];
            var pos = 0;

            for (var i = 0; i < len; i++) {
                var ch = encodedValue[i];

                if (ch == '+') {
                    bytes[pos++] = (byte)' ';
                }
                else if (ch == '%' && i + 2 < len) {
                    var h1 = HexToInt(encodedValue[i + 1]);
                    var h2 = HexToInt(encodedValue[i + 2]);
                    if (h1 >= 0 && h2 >= 0) {
                        bytes[pos++] = (byte)((h1 << 4) | h2);
                        i += 2;
                    }
                    else {
                        // Malformed escape — preserve verbatim.
                        bytes[pos++] = (byte)ch;
                    }
                }
                else if (ch < 0x80) {
                    bytes[pos++] = (byte)ch;
                }
                else {
                    // High char — round-trip through UTF-8 to preserve.
                    var enc = Encoding.UTF8.GetBytes(ch.ToString());
                    if (pos + enc.Length > bytes.Length) {
                        var grown = new byte[bytes.Length + enc.Length];
                        Array.Copy(bytes, grown, pos);
                        bytes = grown;
                    }
                    Array.Copy(enc, 0, bytes, pos, enc.Length);
                    pos += enc.Length;
                }
            }

            return Encoding.UTF8.GetString(bytes, 0, pos);
        }

        private static int HexToInt(char h) {
            if (h >= '0' && h <= '9') return h - '0';
            if (h >= 'a' && h <= 'f') return h - 'a' + 10;
            if (h >= 'A' && h <= 'F') return h - 'A' + 10;
            return -1;
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

        /// <summary>HTML-encodes the specified string.</summary>
        // ----------------------------------------------------------------
        // HTML encode / decode — supports the five mandatory entities plus
        // numeric character references (&#NNN; and &#xHH;). Common need on
        // the device for serving HTML status pages from an embedded HTTP
        // server. ~80 lines for a serviceable subset.
        // ----------------------------------------------------------------

        public static string HtmlEncode(string value) {
            if (value == null) return null;
            if (value.Length == 0) return value;

            // First scan: skip work if nothing needs encoding.
            var needs = false;
            for (var i = 0; i < value.Length; i++) {
                var c = value[i];
                if (c == '<' || c == '>' || c == '&' || c == '"' || c == '\'' || c > 0x7E) {
                    needs = true;
                    break;
                }
            }
            if (!needs) return value;

            var sb = new StringBuilder(value.Length + 16);
            for (var i = 0; i < value.Length; i++) {
                var c = value[i];
                switch (c) {
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '&': sb.Append("&amp;"); break;
                    case '"': sb.Append("&quot;"); break;
                    case '\'': sb.Append("&#39;"); break;
                    default:
                        if (c > 0x7E) {
                            // Non-ASCII — emit as &#NNN; (decimal NCR).
                            sb.Append("&#");
                            sb.Append(((int)c).ToString());
                            sb.Append(";");
                        }
                        else {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>Decodes an HTML-encoded string.</summary>
        public static string HtmlDecode(string value) {
            if (value == null) return null;
            if (value.Length == 0) return value;
            // Skip work if no entities at all.
            if (value.IndexOf('&') < 0) return value;

            var sb = new StringBuilder(value.Length);
            var i = 0;
            while (i < value.Length) {
                var c = value[i];
                if (c != '&') { sb.Append(c); i++; continue; }

                // Find the terminating semicolon within a reasonable window.
                var semi = value.IndexOf(';', i + 1);
                if (semi < 0 || semi - i > 10) { sb.Append(c); i++; continue; }

                var entity = value.Substring(i + 1, semi - i - 1);
                string replacement = null;
                if (entity == "lt") replacement = "<";
                else if (entity == "gt") replacement = ">";
                else if (entity == "amp") replacement = "&";
                else if (entity == "quot") replacement = "\"";
                else if (entity == "apos") replacement = "\'";
                else if (entity.Length > 1 && entity[0] == '#') {
                    // Numeric character reference: &#N; or &#xH;
                    var num = 0;
                    var ok = true;
                    if (entity[1] == 'x' || entity[1] == 'X') {
                        for (var k = 2; k < entity.Length; k++) {
                            var d = HexToInt(entity[k]);
                            if (d < 0) { ok = false; break; }
                            num = (num << 4) | d;
                        }
                    }
                    else {
                        for (var k = 1; k < entity.Length; k++) {
                            var d = entity[k];
                            if (d < '0' || d > '9') { ok = false; break; }
                            num = num * 10 + (d - '0');
                        }
                    }
                    if (ok && num >= 0 && num <= 0xFFFF) replacement = ((char)num).ToString();
                }

                if (replacement != null) {
                    sb.Append(replacement);
                    i = semi + 1;
                }
                else {
                    sb.Append(c);
                    i++;
                }
            }
            return sb.ToString();
        }

    }
}
