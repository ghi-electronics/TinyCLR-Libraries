// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using static GHIElectronics.TinyCLR.EthernetIP.Adapter.AdapterController;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter {
    /// <summary>Describes a CIP object class to be registered with the adapter.</summary>
    public class CIPClass {

        private IntPtr impl = IntPtr.Zero;
        /// <summary>Native handle to the underlying CIP class.</summary>
        public IntPtr Impl {
            get => this.impl;
            internal set => this.impl = value;
        }

        /// <summary>The CIP class code identifying this object class.</summary>
        public ClassId ClassCode { get; }
        /// <summary>The number of class-level attributes.</summary>
        public int NumberClassAttributes { get; }
        /// <summary>The highest class-attribute number used.</summary>
        public uint HighestClassAttributeNumber { get; }
        /// <summary>The number of class-level services.</summary>
        public int NumberClassServices { get; }
        /// <summary>The number of instance-level attributes.</summary>
        public int NumberInstanceAttributes { get; }
        /// <summary>The highest instance-attribute number used.</summary>
        public uint HighestInstanceAttributeNumber { get; }
        /// <summary>The number of instance-level services.</summary>
        public int NumberInstanceServices { get; }
        /// <summary>The number of instances to create for this class.</summary>
        public uint NumberInstances { get; }
        /// <summary>The class name.</summary>
        public string Name { get; }
        /// <summary>The class revision number.</summary>
        public ushort Revision { get; }
        /// <summary>Whether the class is created with the OpENer default initialization.</summary>
        public bool DefaultInitialize { get; } = true;

        /// <summary>Creates an empty CIP class wrapper.</summary>
        public CIPClass() {
        }
        /// <summary>Creates a CIP class with the given class code, attribute/service counts, name, and revision.</summary>
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
