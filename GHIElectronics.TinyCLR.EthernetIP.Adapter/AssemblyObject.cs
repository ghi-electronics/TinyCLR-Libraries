// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    /// <summary>Describes a CIP Assembly (Class 4) instance that a scanner can read or write.</summary>
    public class AssemblyObject {

        private IntPtr impl = IntPtr.Zero;
        /// <summary>Native handle to the underlying assembly object.</summary>
        public IntPtr Impl {
            get => this.impl;
            internal set => this.impl = value;
        }

        /// <summary>The CIP instance ID of this assembly.</summary>
        public int InstanceId { get; }
        /// <summary>The data buffer backing this assembly's contents.</summary>
        public byte[] Data { get; }
        /// <summary>The size of the assembly data in bytes.</summary>
        public ushort Size { get; }

        /// <summary>Creates an assembly object with the given instance ID, data buffer, and size.</summary>
        public AssemblyObject(int instanceId, byte[] data, ushort size) {
            this.InstanceId = instanceId;
            this.Data = data;
            this.Size = size;

            //this.impl = this.CreateAssemblyObject(instanceId, data, size); ;
        }

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //private extern IntPtr CreateAssemblyObject(int instanceId, byte[] data, ushort size);
    }
}
