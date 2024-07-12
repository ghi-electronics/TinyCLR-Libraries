using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public partial class AdapterController {
        public enum ClassId : int {
            Identity = 0x01,
            MessageRouter = 0x02,
            DeviceNet = 0x03,
            Assembly = 0x04,
            Connection = 0x05,
            ConnectionManager = 0x06,
            //TcpIpInterface = 0xF5,
        }

        public enum CIPServiceCode : uint {
            kGetAttributeAll = 0x01,
            kSetAttributeAll = 0x02,
            kGetAttributeList = 0x03,
            kSetAttributeList = 0x04,
            kReset = 0x05,
            kStart = 0x06,
            kStop = 0x07,
            kCreate = 0x08,
            kDelete = 0x09,
            kMultipleServicePacket = 0x0A,
            kApplyAttributes = 0x0D,
            kGetAttributeSingle = 0x0E,
            kSetAttributeSingle = 0x10,
            kFindNextObjectInstance = 0x11,
            kRestore = 0x15,
            kSave = 0x16,
            kNoOperation = 0x17,
            kGetMember = 0x18,
            kSetMember = 0x19,
            kInsertMember = 0x1A,
            kRemoveMember = 0x1B,
            kGroupSync = 0x1C,
            kGetConnectionPointMemberList = 0x1D,
            /* End CIP common services */

            /* Start CIP object-specific services */
            kEthLinkGetAndClear = 0x4C, /**< Ethernet Link object's Get_And_Clear service */
            kForwardOpen = 0x54,
            kLargeForwardOpen = 0x5B,
            kForwardClose = 0x4E,
            kUnconnectedSend = 0x52,
            kGetConnectionOwner = 0x5A,
            kGetConnectionData = 0x56,
            kSearchConnectionData = 0x57
        }
    }
}
