// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public partial class AdapterController {
        /// <summary>The CIP data type codes used to encode attribute values on the wire.</summary>
        public enum CIPDataType : byte {
            /// <summary>Data type that cannot be directly encoded.</summary>
            kCipAny = 0x00, /**< data type that can not be directly encoded */
            /// <summary>Boolean data type.</summary>
            kCipBool = 0xC1, /**< boolean data type */
            /// <summary>8-bit signed integer.</summary>
            kCipSint = 0xC2, /**< 8-bit signed integer */
            /// <summary>16-bit signed integer.</summary>
            kCipInt = 0xC3, /**< 16-bit signed integer */
            /// <summary>32-bit signed integer.</summary>
            kCipDint = 0xC4, /**< 32-bit signed integer */
            /// <summary>64-bit signed integer.</summary>
            kCipLint = 0xC5, /**< 64-bit signed integer */
            /// <summary>8-bit unsigned integer.</summary>
            kCipUsint = 0xC6, /**< 8-bit unsigned integer */
            /// <summary>16-bit unsigned integer.</summary>
            kCipUint = 0xC7, /**< 16-bit unsigned integer */
            /// <summary>32-bit unsigned integer.</summary>
            kCipUdint = 0xC8, /**< 32-bit unsigned integer */
            /// <summary>64-bit unsigned integer.</summary>
            kCipUlint = 0xC9, /**< 64-bit unsigned integer */
            /// <summary>Single-precision floating point.</summary>
            kCipReal = 0xCA, /**< Single precision floating point */
            /// <summary>Double-precision floating point.</summary>
            kCipLreal = 0xCB, /**< Double precision floating point*/
            /// <summary>Synchronous time information; type of DINT.</summary>
            kCipStime = 0xCC, /**< Synchronous time information*, type of DINT */
            /// <summary>Date only.</summary>
            kCipDate = 0xCD, /**< Date only*/
            /// <summary>Time of day.</summary>
            kCipTimeOfDay = 0xCE, /**< Time of day */
            /// <summary>Date and time of day.</summary>
            kCipDateAndTime = 0xCF, /**< Date and time of day */
            /// <summary>Character string, 1 byte per character.</summary>
            kCipString = 0xD0, /**< Character string, 1 byte per character */
            /// <summary>8-bit bit string.</summary>
            kCipByte = 0xD1, /**< 8-bit bit string */
            /// <summary>16-bit bit string.</summary>
            kCipWord = 0xD2, /**< 16-bit bit string */
            /// <summary>32-bit bit string.</summary>
            kCipDword = 0xD3, /**< 32-bit bit string */
            /// <summary>64-bit bit string.</summary>
            kCipLword = 0xD4, /**< 64-bit bit string */
            /// <summary>Character string, 2 bytes per character.</summary>
            kCipString2 = 0xD5, /**< Character string, 2 byte per character */
            /// <summary>Duration in microseconds, high resolution; range of DINT.</summary>
            kCipFtime = 0xD6, /**< Duration in micro-seconds, high resolution; range of DINT */
            /// <summary>Duration in microseconds, high resolution; range of LINT.</summary>
            kCipLtime = 0xD7, /**< Duration in micro-seconds, high resolution, range of LINT */
            /// <summary>Duration in milliseconds, short; range of INT.</summary>
            kCipItime = 0xD8, /**< Duration in milli-seconds, short; range of INT*/
            /// <summary>Character string, N bytes per character.</summary>
            kCipStringN = 0xD9, /**< Character string, N byte per character */
            /// <summary>Character string, 1 byte per character with a 1-byte length indicator.</summary>
            kCipShortString = 0xDA, /**< Character string, 1 byte per character, 1 byte
                             length indicator */
            /// <summary>Duration in milliseconds; range of DINT.</summary>
            kCipTime = 0xDB, /**< Duration in milli-seconds; range of DINT */
            /// <summary>CIP path segments (EPATH).</summary>
            kCipEpath = 0xDC, /**< CIP path segments*/
            /// <summary>Engineering units; range of UINT.</summary>
            kCipEngUnit = 0xDD, /**< Engineering Units, range of UINT*/
            /* definition of some CIP structs */
            /* need to be validated in IEC 61131-3 subclause 2.3.3 */
            /* TODO: Check these codes */
            /// <summary>Struct of two USINTs, used for CIP Identity attribute 4 (revision).</summary>
            kCipUsintUsint = 0xA0, /**< Used for CIP Identity attribute 4 Revision*/
            /// <summary>Struct for TCP/IP interface attribute 5 (IP address, mask, gateway, name servers, domain name).</summary>
            kCipUdintUdintUdintUdintUdintString = 0xA1, /**< TCP/IP attribute 5 - IP address, subnet mask, gateway, IP name
                                                 server 1, IP name server 2, domain name*/
            /// <summary>Struct for a MAC address (six USINTs).</summary>
            kCip6Usint = 0xA2, /**< Struct for MAC Address (six USINTs)*/
            /// <summary>A member list struct.</summary>
            kCipMemberList = 0xA3, /**< */
            /// <summary>A byte array struct.</summary>
            kCipByteArray = 0xA4, /**< */
            /// <summary>Internal struct of six UINTs used for the Port class attribute 9.</summary>
            kInternalUint6 = 0xF0, /**< bogus hack, for port class attribute 9, TODO
                            figure out the right way to handle it */
            /// <summary>International (multi-language) character string.</summary>
            kCipStringI
        }
    }
}
