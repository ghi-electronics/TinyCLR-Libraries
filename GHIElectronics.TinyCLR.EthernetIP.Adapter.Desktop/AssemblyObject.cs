// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public class AssemblyObject {

        private IntPtr impl = IntPtr.Zero;
        public IntPtr Impl {
            get => this.impl;
            internal set => this.impl = value;
        }

        public int InstanceId { get; }
        public byte[] Data { get; }
        public ushort Size { get; }

        public AssemblyObject(int instanceId, byte[] data, ushort size) {
            this.InstanceId = instanceId;
            this.Data = data;
            this.Size = size;

            //this.impl = this.CreateAssemblyObject(instanceId, data, size); ;
        }

        ////private IntPtr CreateAssemblyObject(int instanceId, byte[] data, ushort size) => throw new System.NotSupportedException("TODO - Not supported");
    }
}
