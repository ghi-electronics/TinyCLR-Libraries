using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.EthernetIP.Adapter {
    public class AssemblyObject {

        private IntPtr impl;
        public IntPtr Impl {
            get => this.impl;
            set => this.impl = value;
        }

        public AssemblyObject(int instanceId, byte[] data, ushort size) {

            this.impl = this.CreateAssemblyObject(instanceId, data, size); ;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr CreateAssemblyObject(int instanceId, byte[] data, ushort size);
    }
}
