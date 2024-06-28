using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.EthernetIP.Adapter {
    public partial class AdapterController {

        public enum CipAttributeDecodeFromMessage : uint {
            DecodeCipBool = 0x01,
            DecodeCipByte = 0x02,
            DecodeCipByteArray = 0x4,
            DecodeCipWord = 0x8,
            DecodeCipDword = 0x10,
            DecodeCipLword = 0x20,
            DecodeCipUsint = 0x40,
            DecodeCipUint = 0x80,
            DecodeCipUdint = 0x100,
            DecodeCipUlint = 0x200,
            DecodeCipSint = 0x400,
            DecodeCipInt = 0x800,
            DecodeCipDint = 0x1000,
            DecodeCipLint = 0x2000,
            DecodeCipReal = 0x4000,
            DecodeCipLreal = 0x8000,
            DecodeCipString = 0x10000,
            DecodeCipShortString = 0x20000,
            
        }


    }
}
