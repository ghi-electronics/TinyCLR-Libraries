// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public class CipAttribute {
        private IntPtr impl = IntPtr.Zero;
        public IntPtr Impl {
            get => this.impl;
            internal set => this.impl = value;
        }
    }
}
