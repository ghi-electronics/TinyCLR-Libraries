// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public partial class AdapterController {
        /// <summary>Standard CIP object class codes.</summary>
        public enum ClassId : int {
            // Standard CIP class codes per ODVA Vol 1 + Vol 2.
            // Required for an EtherNet/IP adapter: Identity (1), MessageRouter (2),
            // Assembly (4), ConnectionManager (6), TcpIpInterface (0xF5), EthernetLink (0xF6).
            // Required by ODVA Pub 70 v10: Port (0xF4). QoS (0x48) required if device
            // supports DSCP attributes. DLR (0x47) for Device Level Ring support.
            /// <summary>Identity object.</summary>
            Identity = 0x01,
            /// <summary>Message Router object.</summary>
            MessageRouter = 0x02,
            /// <summary>DeviceNet object.</summary>
            DeviceNet = 0x03,
            /// <summary>Assembly object.</summary>
            Assembly = 0x04,
            /// <summary>Connection object.</summary>
            Connection = 0x05,
            /// <summary>Connection Manager object.</summary>
            ConnectionManager = 0x06,
            /// <summary>Device Level Ring (DLR) object.</summary>
            Dlr = 0x47,
            /// <summary>Quality of Service (QoS) object.</summary>
            QoS = 0x48,
            /// <summary>Port object.</summary>
            Port = 0xF4,
            /// <summary>TCP/IP Interface object.</summary>
            TcpIpInterface = 0xF5,
            /// <summary>Ethernet Link object.</summary>
            EthernetLink = 0xF6,
        }

        /// <summary>CIP service codes for the common and object-specific services.</summary>
        public enum CIPServiceCode : uint {
            /// <summary>Get Attribute All service.</summary>
            kGetAttributeAll = 0x01,
            /// <summary>Set Attribute All service.</summary>
            kSetAttributeAll = 0x02,
            /// <summary>Get Attribute List service.</summary>
            kGetAttributeList = 0x03,
            /// <summary>Set Attribute List service.</summary>
            kSetAttributeList = 0x04,
            /// <summary>Reset service.</summary>
            kReset = 0x05,
            /// <summary>Start service.</summary>
            kStart = 0x06,
            /// <summary>Stop service.</summary>
            kStop = 0x07,
            /// <summary>Create service.</summary>
            kCreate = 0x08,
            /// <summary>Delete service.</summary>
            kDelete = 0x09,
            /// <summary>Multiple Service Packet service.</summary>
            kMultipleServicePacket = 0x0A,
            /// <summary>Apply Attributes service.</summary>
            kApplyAttributes = 0x0D,
            /// <summary>Get Attribute Single service.</summary>
            kGetAttributeSingle = 0x0E,
            /// <summary>Set Attribute Single service.</summary>
            kSetAttributeSingle = 0x10,
            /// <summary>Find Next Object Instance service.</summary>
            kFindNextObjectInstance = 0x11,
            /// <summary>Restore service.</summary>
            kRestore = 0x15,
            /// <summary>Save service.</summary>
            kSave = 0x16,
            /// <summary>No Operation service.</summary>
            kNoOperation = 0x17,
            /// <summary>Get Member service.</summary>
            kGetMember = 0x18,
            /// <summary>Set Member service.</summary>
            kSetMember = 0x19,
            /// <summary>Insert Member service.</summary>
            kInsertMember = 0x1A,
            /// <summary>Remove Member service.</summary>
            kRemoveMember = 0x1B,
            /// <summary>Group Sync service.</summary>
            kGroupSync = 0x1C,
            /// <summary>Get Connection Point Member List service.</summary>
            kGetConnectionPointMemberList = 0x1D,
            /* End CIP common services */

            /* Start CIP object-specific services */
            /// <summary>Ethernet Link object's Get_And_Clear service.</summary>
            kEthLinkGetAndClear = 0x4C, /**< Ethernet Link object's Get_And_Clear service */
            /// <summary>Forward Open service.</summary>
            kForwardOpen = 0x54,
            /// <summary>Large Forward Open service.</summary>
            kLargeForwardOpen = 0x5B,
            /// <summary>Forward Close service.</summary>
            kForwardClose = 0x4E,
            /// <summary>Unconnected Send service.</summary>
            kUnconnectedSend = 0x52,
            /// <summary>Get Connection Owner service.</summary>
            kGetConnectionOwner = 0x5A,
            /// <summary>Get Connection Data service.</summary>
            kGetConnectionData = 0x56,
            /// <summary>Search Connection Data service.</summary>
            kSearchConnectionData = 0x57
        }
    }
}
