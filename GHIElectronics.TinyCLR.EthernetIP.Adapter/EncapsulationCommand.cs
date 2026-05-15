// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public partial class AdapterController {
        // Full set of EtherNet/IP encapsulation commands per ENIP Vol 2 §2-3.2.
        // Exposed so user event handlers can switch on the received command code from
        // ReceivedExplict{Tcp,Udp}Data events.
        public enum EncapsulationCommand : ushort {
            NoOperation = 0x0000,           // TCP only
            ListServices = 0x0004,          // TCP and UDP
            ListIdentity = 0x0063,          // TCP and UDP
            ListInterfaces = 0x0064,        // optional, TCP and UDP
            RegisterSession = 0x0065,       // TCP only
            UnregisterSession = 0x0066,     // TCP only
            SendRequestReplyData = 0x006F,  // TCP only
            SendUnitData = 0x0070,          // TCP only
        };

    }
}
