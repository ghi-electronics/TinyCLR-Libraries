using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using static GHIElectronics.TinyCLR.EthernetIP.Adapter.AdapterController;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    public class CIPClass {

        private IntPtr impl = IntPtr.Zero;
        public IntPtr Impl {
            get => this.impl;
            internal set => this.impl = value;
        }

        public ClassId ClassCode { get; }
        public int NumberClassAttributes { get; }
        public uint HighestClassAttributeNumber { get; }
        public int NumberClassServices { get; }
        public int NumberInstanceAttributes { get; }
        public uint HighestInstanceAttributeNumber { get; }
        public int NumberInstanceServices { get; }
        public uint NumberInstances { get; }
        public string Name { get; }
        public ushort Revision { get; }
        public bool DefaultInitialize { get; } = true;

        public CIPClass() {
        }
        public CIPClass(ClassId classCode, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize = true) {
            this.ClassCode = classCode;
            this.NumberClassAttributes = numberClassAttributes;
            this.HighestClassAttributeNumber = highestClassAttributeNumber;
            this.NumberClassServices = numberClassServices;
            this.NumberInstanceAttributes = numberInstanceAttributes;
            this.HighestInstanceAttributeNumber = highestInstanceAttributeNumber;
            this.NumberInstanceServices = numberInstanceServices;
            this.NumberInstances = numberInstances;
            this.Name = name;
            this.Revision = revision;
            this.DefaultInitialize = defaultInitialize;

            //this.impl = this.CreateCipClass((uint) classCode, numberClassAttributes, highestClassAttributeNumber, numberClassServices, numberInstanceAttributes,  highestInstanceAttributeNumber,  numberInstanceServices,  numberInstances,  name,  revision, defaultInitialize); ;
        }

        //[MethodImpl(MethodImplOptions.InternalCall)]
        //private extern IntPtr CreateCipClass(uint classCode, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize);
    }
}
