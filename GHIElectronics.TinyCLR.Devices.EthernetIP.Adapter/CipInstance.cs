using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.EthernetIP.Adapter {
    public class CipInstance {

        private IntPtr impl;
        public IntPtr Impl {
            get => this.impl;
            set => this.impl = value;
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
