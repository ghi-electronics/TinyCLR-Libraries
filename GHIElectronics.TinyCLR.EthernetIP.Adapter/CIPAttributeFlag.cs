// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public partial class AdapterController {
        public enum CIPAttributeFlag: uint {
            kNotSetOrGetable = 0x00, /**< Neither set-able nor get-able */
            kGetableAll = 0x01, /**< Get-able, also part of Get Attribute All service */
            kGetableSingle = 0x02, /**< Get-able via Get Attribute */
            kSetable = 0x04, /**< Set-able via Set Attribute */
            /* combined for convenience */
            kSetAndGetAble = 0x07, /**< both set and get-able */
            kGetableSingleAndAll = 0x03, /**< both single and all */
            /* Flags to control the usage of callbacks per attribute from the Get* and Set* services */
            kGetableAllDummy = 0x08, /**< Get-able but a dummy Attribute */
            kPreGetFunc = 0x10, /**< enable pre get callback */
            kPostGetFunc = 0x20, /**< enable post get callback */
            kPreSetFunc = 0x40, /**< enable pre set callback */
            kPostSetFunc = 0x80, /**< enable post set callback */
            kNvDataFunc = 0x80, /**< enable Non Volatile data callback, is the same as @ref kPostSetFunc */
        }
    }
}
