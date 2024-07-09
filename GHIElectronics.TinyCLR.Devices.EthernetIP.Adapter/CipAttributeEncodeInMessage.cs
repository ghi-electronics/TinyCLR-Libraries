using System;
using System.Collections;
using System.Text;
using System.Threading;
using static GHIElectronics.TinyCLR.Devices.EthernetIP.Adapter.AdapterController;

namespace GHIElectronics.TinyCLR.Devices.EthernetIP.Adapter {
    public partial class AdapterController {

        public enum CipAttributeEncodeInMessage : uint {
            None = 0,
            EncodeCipBool = 0x01,
            EncodeCipByte = 0x02,
            EncodeCipWord = 0x4,
            EncodeCipDword = 0x8,
            EncodeCipLword = 0x10,
            EncodeCipUsint = 0x20,
            EncodeCipUint = 0x40,
            EncodeCipUdint = 0x80,
            EncodeCipUlint = 0x100,
            EncodeCipSint = 0x200,
            EncodeCipInt = 0x400,
            EncodeCipDint = 0x800,
            EncodeCipLint = 0x1000,
            EncodeCipReal = 0x2000,
            EncodeCipLreal = 0x4000,
            EncodeCipShortString = 0x8000,
            EncodeCipString = 0x10000,
            EncodeCipString2 = 0x20000,
            EncodeCipStringN = 0x40000,
            EncodeCipStringI = 0x80000,
            EncodeCipByteArray = 0x100000,
            EncodeCipEPath = 0x2000000,
            EncodeEPath = 0x4000000,
            EncodeCipEthernetLinkPhyisicalAddress = 0x8000000,            
        }

       
    }
}
