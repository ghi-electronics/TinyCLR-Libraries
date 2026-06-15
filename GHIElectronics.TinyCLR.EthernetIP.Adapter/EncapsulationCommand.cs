// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public partial class AdapterController {
        /// <summary>EtherNet/IP encapsulation command codes reported by the explicit-data events.</summary>
        // Full set of EtherNet/IP encapsulation commands per ENIP Vol 2 §2-3.2.
        // Exposed so user event handlers can switch on the received command code from
        // ReceivedExplict{Tcp,Udp}Data events.
        public enum EncapsulationCommand : ushort {
            /// <summary>No operation (TCP only).</summary>
            NoOperation = 0x0000,           // TCP only
            /// <summary>List Services command (TCP and UDP).</summary>
            ListServices = 0x0004,          // TCP and UDP
            /// <summary>List Identity command (TCP and UDP).</summary>
            ListIdentity = 0x0063,          // TCP and UDP
            /// <summary>List Interfaces command (optional, TCP and UDP).</summary>
            ListInterfaces = 0x0064,        // optional, TCP and UDP
            /// <summary>Register Session command (TCP only).</summary>
            RegisterSession = 0x0065,       // TCP only
            /// <summary>Unregister Session command (TCP only).</summary>
            UnregisterSession = 0x0066,     // TCP only
            /// <summary>Send RR (request/reply) Data command (TCP only).</summary>
            SendRequestReplyData = 0x006F,  // TCP only
            /// <summary>Send Unit Data command (TCP only).</summary>
            SendUnitData = 0x0070,          // TCP only
        };

    }
}
