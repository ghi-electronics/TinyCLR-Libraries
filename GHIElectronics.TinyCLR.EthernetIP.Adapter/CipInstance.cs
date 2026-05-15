// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public class CipInstance {

        private IntPtr impl;
        // Phase 3.5: setter narrowed to internal. The setter was public for historical
        // reasons (parallel-with-AddCipInstance returning instances) but exposing a
        // raw mutable pointer to user code is a footgun — they could overwrite it
        // with anything and the native side would dereference garbage. Other wrappers
        // (CIPClass, AssemblyObject, CipAttribute) were already internal-set.
        public IntPtr Impl {
            get => this.impl;
            internal set => this.impl = value;
        }
        //public static CipInstance GetCipInstance(CIPClass cipClass, uint instanceNumber) {
        //    var instance = new CipInstance {
        //        impl = NativeGetCipInstance(cipClass.Impl, instanceNumber)
        //    };

        //    return instance;
        //}

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //extern static IntPtr NativeGetCipInstance(IntPtr cipClassPtr, uint instanceNumber);
    }
}
