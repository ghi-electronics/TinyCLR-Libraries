// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Collections;
using System.Text;
using System.Threading;
using static GHIElectronics.TinyCLR.EthernetIP.Adapter.AdapterController;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public partial class AdapterController {

        /// <summary>Selects the native function used to encode an attribute value into an outgoing message.</summary>
        public enum CipAttributeEncodeInMessage : uint {
            /// <summary>No encode function.</summary>
            None = 0,
            /// <summary>Encode a CIP BOOL value.</summary>
            EncodeCipBool = 0x01,
            /// <summary>Encode a CIP BYTE value.</summary>
            EncodeCipByte = 0x02,
            /// <summary>Encode a CIP WORD value.</summary>
            EncodeCipWord = 0x4,
            /// <summary>Encode a CIP DWORD value.</summary>
            EncodeCipDword = 0x8,
            /// <summary>Encode a CIP LWORD value.</summary>
            EncodeCipLword = 0x10,
            /// <summary>Encode a CIP USINT value.</summary>
            EncodeCipUsint = 0x20,
            /// <summary>Encode a CIP UINT value.</summary>
            EncodeCipUint = 0x40,
            /// <summary>Encode a CIP UDINT value.</summary>
            EncodeCipUdint = 0x80,
            /// <summary>Encode a CIP ULINT value.</summary>
            EncodeCipUlint = 0x100,
            /// <summary>Encode a CIP SINT value.</summary>
            EncodeCipSint = 0x200,
            /// <summary>Encode a CIP INT value.</summary>
            EncodeCipInt = 0x400,
            /// <summary>Encode a CIP DINT value.</summary>
            EncodeCipDint = 0x800,
            /// <summary>Encode a CIP LINT value.</summary>
            EncodeCipLint = 0x1000,
            /// <summary>Encode a CIP REAL value.</summary>
            EncodeCipReal = 0x2000,
            /// <summary>Encode a CIP LREAL value.</summary>
            EncodeCipLreal = 0x4000,
            /// <summary>Encode a CIP SHORT_STRING value.</summary>
            EncodeCipShortString = 0x8000,
            /// <summary>Encode a CIP STRING value.</summary>
            EncodeCipString = 0x10000,
            /// <summary>Encode a CIP STRING2 value.</summary>
            EncodeCipString2 = 0x20000,
            /// <summary>Encode a CIP STRINGN value.</summary>
            EncodeCipStringN = 0x40000,
            /// <summary>Encode a CIP STRINGI value.</summary>
            EncodeCipStringI = 0x80000,
            /// <summary>Encode a CIP byte array value.</summary>
            EncodeCipByteArray = 0x100000,
            /// <summary>Encode a padded CIP EPATH value.</summary>
            EncodeCipEPath = 0x2000000,
            /// <summary>Encode a packed EPATH value.</summary>
            EncodeEPath = 0x4000000,
            /// <summary>Encode an Ethernet Link object physical (MAC) address.</summary>
            EncodeCipEthernetLinkPhyisicalAddress = 0x8000000,
        }

       
    }
}
