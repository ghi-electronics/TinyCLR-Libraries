using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.EthernetIP.Adapter {
    public class CipAttribute {
        private IntPtr impl = IntPtr.Zero;
        public IntPtr Impl {
            get => this.impl;
            internal set => this.impl = value;
        }
    }
}
