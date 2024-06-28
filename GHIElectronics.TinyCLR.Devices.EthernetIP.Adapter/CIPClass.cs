using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using static GHIElectronics.TinyCLR.Devices.EthernetIP.Adapter.AdapterController;

namespace GHIElectronics.TinyCLR.Devices.EthernetIP.Adapter {
    public class CIPClass {

        private IntPtr impl;
        public IntPtr Impl {
            get => this.impl;
            set => this.impl = value;
        }

        public CIPClass(ClassId classCode, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize = true) {
            this.impl = this.CreateCipClass((uint) classCode, numberClassAttributes, highestClassAttributeNumber, numberClassServices, numberInstanceAttributes,  highestInstanceAttributeNumber,  numberInstanceServices,  numberInstances,  name,  revision, defaultInitialize); ;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr CreateCipClass(uint classCode, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize);
    }
}
