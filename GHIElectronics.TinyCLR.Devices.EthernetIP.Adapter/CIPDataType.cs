using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public partial class AdapterController {
        public enum CIPDataType : byte {
            kCipAny = 0x00, /**< data type that can not be directly encoded */
            kCipBool = 0xC1, /**< boolean data type */
            kCipSint = 0xC2, /**< 8-bit signed integer */
            kCipInt = 0xC3, /**< 16-bit signed integer */
            kCipDint = 0xC4, /**< 32-bit signed integer */
            kCipLint = 0xC5, /**< 64-bit signed integer */
            kCipUsint = 0xC6, /**< 8-bit unsigned integer */
            kCipUint = 0xC7, /**< 16-bit unsigned integer */
            kCipUdint = 0xC8, /**< 32-bit unsigned integer */
            kCipUlint = 0xC9, /**< 64-bit unsigned integer */
            kCipReal = 0xCA, /**< Single precision floating point */
            kCipLreal = 0xCB, /**< Double precision floating point*/
            kCipStime = 0xCC, /**< Synchronous time information*, type of DINT */
            kCipDate = 0xCD, /**< Date only*/
            kCipTimeOfDay = 0xCE, /**< Time of day */
            kCipDateAndTime = 0xCF, /**< Date and time of day */
            kCipString = 0xD0, /**< Character string, 1 byte per character */
            kCipByte = 0xD1, /**< 8-bit bit string */
            kCipWord = 0xD2, /**< 16-bit bit string */
            kCipDword = 0xD3, /**< 32-bit bit string */
            kCipLword = 0xD4, /**< 64-bit bit string */
            kCipString2 = 0xD5, /**< Character string, 2 byte per character */
            kCipFtime = 0xD6, /**< Duration in micro-seconds, high resolution; range of DINT */
            kCipLtime = 0xD7, /**< Duration in micro-seconds, high resolution, range of LINT */
            kCipItime = 0xD8, /**< Duration in milli-seconds, short; range of INT*/
            kCipStringN = 0xD9, /**< Character string, N byte per character */
            kCipShortString = 0xDA, /**< Character string, 1 byte per character, 1 byte
                             length indicator */
            kCipTime = 0xDB, /**< Duration in milli-seconds; range of DINT */
            kCipEpath = 0xDC, /**< CIP path segments*/
            kCipEngUnit = 0xDD, /**< Engineering Units, range of UINT*/
            /* definition of some CIP structs */
            /* need to be validated in IEC 61131-3 subclause 2.3.3 */
            /* TODO: Check these codes */
            kCipUsintUsint = 0xA0, /**< Used for CIP Identity attribute 4 Revision*/
            kCipUdintUdintUdintUdintUdintString = 0xA1, /**< TCP/IP attribute 5 - IP address, subnet mask, gateway, IP name
                                                 server 1, IP name server 2, domain name*/
            kCip6Usint = 0xA2, /**< Struct for MAC Address (six USINTs)*/
            kCipMemberList = 0xA3, /**< */
            kCipByteArray = 0xA4, /**< */
            kInternalUint6 = 0xF0, /**< bogus hack, for port class attribute 9, TODO
                            figure out the right way to handle it */
            kCipStringI
        }
    }
}
