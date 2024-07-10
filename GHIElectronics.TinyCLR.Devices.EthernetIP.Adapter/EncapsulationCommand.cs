using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.EthernetIP.Adapter {
    public partial class AdapterController {
        public enum EncapsulationCommand : ushort{
            //NoOperation = 0x0000, /**< only allowed for TCP */
            //ListServices = 0x0004, /**< allowed for both UDP and TCP */
            //ListIdentity = 0x0063, /**< allowed for both UDP and TCP */
            //ListInterfaces = 0x0064, /**< optional, allowed for both UDP and TCP */
            RegisterSession = 0x0065, /**< only allowed for TCP */
            UnregisterSession = 0x0066, /**< only allowed for TCP */
            //SendRequestReplyData = 0x006F, /**< only allowed for TCP */
            //SendUnitData = 0x0070 /**< only allowed for TCP */
        };

    }
}
