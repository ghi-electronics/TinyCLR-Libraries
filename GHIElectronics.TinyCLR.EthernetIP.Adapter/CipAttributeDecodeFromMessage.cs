// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public partial class AdapterController {

        /// <summary>Selects the native function used to decode an attribute value from a received message.</summary>
        public enum CipAttributeDecodeFromMessage : uint {
            /// <summary>No decode function.</summary>
            None = 0,
            /// <summary>Decode a CIP BOOL value.</summary>
            DecodeCipBool = 0x01,
            /// <summary>Decode a CIP BYTE value.</summary>
            DecodeCipByte = 0x02,
            /// <summary>Decode a CIP byte array value.</summary>
            DecodeCipByteArray = 0x4,
            /// <summary>Decode a CIP WORD value.</summary>
            DecodeCipWord = 0x8,
            /// <summary>Decode a CIP DWORD value.</summary>
            DecodeCipDword = 0x10,
            /// <summary>Decode a CIP LWORD value.</summary>
            DecodeCipLword = 0x20,
            /// <summary>Decode a CIP USINT value.</summary>
            DecodeCipUsint = 0x40,
            /// <summary>Decode a CIP UINT value.</summary>
            DecodeCipUint = 0x80,
            /// <summary>Decode a CIP UDINT value.</summary>
            DecodeCipUdint = 0x100,
            /// <summary>Decode a CIP ULINT value.</summary>
            DecodeCipUlint = 0x200,
            /// <summary>Decode a CIP SINT value.</summary>
            DecodeCipSint = 0x400,
            /// <summary>Decode a CIP INT value.</summary>
            DecodeCipInt = 0x800,
            /// <summary>Decode a CIP DINT value.</summary>
            DecodeCipDint = 0x1000,
            /// <summary>Decode a CIP LINT value.</summary>
            DecodeCipLint = 0x2000,
            /// <summary>Decode a CIP REAL value.</summary>
            DecodeCipReal = 0x4000,
            /// <summary>Decode a CIP LREAL value.</summary>
            DecodeCipLreal = 0x8000,
            /// <summary>Decode a CIP STRING value.</summary>
            DecodeCipString = 0x10000,
            /// <summary>Decode a CIP SHORT_STRING value.</summary>
            DecodeCipShortString = 0x20000,

        }


    }
}
