// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public partial class AdapterController {
        /// <summary>Access and callback flags applied to a CIP attribute.</summary>
        public enum CIPAttributeFlag: uint {
            /// <summary>Neither settable nor gettable.</summary>
            kNotSetOrGetable = 0x00, // Neither set-able nor get-able
            /// <summary>Gettable, also part of the Get Attribute All service.</summary>
            kGetableAll = 0x01, // Get-able, also part of Get Attribute All service
            /// <summary>Gettable via Get Attribute Single.</summary>
            kGetableSingle = 0x02, // Get-able via Get Attribute
            /// <summary>Settable via Set Attribute.</summary>
            kSetable = 0x04, // Set-able via Set Attribute
            /* combined for convenience */
            /// <summary>Both settable and gettable.</summary>
            kSetAndGetAble = 0x07, // both set and get-able
            /// <summary>Gettable via both single and all.</summary>
            kGetableSingleAndAll = 0x03, // both single and all
            /* Flags to control the usage of callbacks per attribute from the Get* and Set* services */
            /// <summary>Gettable but a dummy attribute.</summary>
            kGetableAllDummy = 0x08, // Get-able but a dummy Attribute
            /// <summary>Enable the pre-get callback.</summary>
            kPreGetFunc = 0x10, // enable pre get callback
            /// <summary>Enable the post-get callback.</summary>
            kPostGetFunc = 0x20, // enable post get callback
            /// <summary>Enable the pre-set callback.</summary>
            kPreSetFunc = 0x40, // enable pre set callback
            /// <summary>Enable the post-set callback.</summary>
            kPostSetFunc = 0x80, // enable post set callback
            /// <summary>Enable the non-volatile data callback (same value as the post-set callback).</summary>
            kNvDataFunc = 0x80, // enable Non Volatile data callback, is the same as @ref kPostSetFunc
        }
    }
}
