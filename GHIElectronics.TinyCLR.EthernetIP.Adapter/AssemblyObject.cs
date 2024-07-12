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

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //private extern IntPtr CreateAssemblyObject(int instanceId, byte[] data, ushort size);
    }
}
