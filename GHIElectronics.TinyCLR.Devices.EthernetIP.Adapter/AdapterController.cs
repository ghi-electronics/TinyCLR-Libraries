using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.EthernetIP.Adapter
{
    
    public partial class AdapterController
    {
        private uint deviceVendorID;
        private uint deviceType;
        private uint deviceProductCode;
        private uint deviceMajorRevision;
        private uint deviceMinorRevision;
        private string deviceName;
        private uint deviceSerialNumber;

        private ArrayList cipClassesList;
        private ArrayList assemblyObjectsList;
        public AdapterController(string deviceName, uint deviceVendorID, ushort deviceType, ushort deviceProductCode, uint deviceSerialNumber, byte deviceMajorRevision, byte deviceMinorRevision) {
            this.deviceName = deviceName;
            this.deviceVendorID = deviceVendorID;
            this.deviceType = deviceType;
            this.deviceProductCode = deviceProductCode;
            this.deviceSerialNumber = deviceSerialNumber;
            this.deviceMajorRevision = deviceMajorRevision;
            this.deviceMinorRevision = deviceMinorRevision;

            this.cipClassesList = new ArrayList();
            this.assemblyObjectsList = new ArrayList();

            this.NativeSetDeviceProductName(deviceName);
            this.NativeSetDeviceVendorId(deviceVendorID);
            this.NativeSetDeviceType(deviceType);
            this.NativeSetDeviceProductCode(deviceProductCode);
            this.NativeSetDeviceSerialNumber(deviceSerialNumber);
            this.NativeSetDeviceRevision(deviceMajorRevision, deviceMinorRevision);
        }

        public void AddCipClass(CIPClass cipClass) {

            var cip = cipClass;

            cip.Impl = this.NativeCreateCipClass((uint)cipClass.ClassCode, cipClass.NumberClassAttributes, cipClass.HighestClassAttributeNumber, cipClass.NumberClassServices, cipClass.NumberInstanceAttributes, cipClass.HighestInstanceAttributeNumber, cipClass.NumberInstanceServices, cipClass.NumberInstances, cipClass.Name, cipClass.Revision, cipClass.DefaultInitialize);

            this.cipClassesList.Add(cip);            
        }

        public void AddAssemblyObject(AssemblyObject asmObject) {

            var obj = asmObject;

            obj.Impl = this.NativeCreateAssemblyObject(obj.InstanceId, obj.Data, obj.Size);

            this.assemblyObjectsList.Add(obj);
        }

        //public void SetDeviceSerialNumber(uint serialNumber) => this.NativeSetDeviceSerialNumber(serialNumber);
        public void ConfigureExclusiveOwnerConnectionPoint(uint connectionNumber, uint outputAssemblyId, uint inputAssemblyId, uint configurationAssemblyId) => this.NativeConfigureExclusiveOwnerConnectionPoint(connectionNumber, outputAssemblyId, inputAssemblyId, configurationAssemblyId);
        public void ConfigureInputOnlyConnectionPoint(uint connectionNumber, uint outputAssemblyId, uint inputAssemblyId, uint configurationAssemblyId) => this.NativeConfigureInputOnlyConnectionPoint(connectionNumber, outputAssemblyId, inputAssemblyId, configurationAssemblyId);
        public void ConfigureListenOnlyConnectionPoint(uint connectionNumber, uint outputAssemblyId, uint inputAssemblyId, uint configurationAssemblyId) => this.NativeConfigureListenOnlyConnectionPoint(connectionNumber, outputAssemblyId, inputAssemblyId, configurationAssemblyId);

        public void InsertService(CIPClass cipClass, CIPServiceCode serviceCode, CipServiceFunctionCode funcCode, string serviceName) {
            var ptr = cipClass.Impl;

            this.NativeInsertService(ptr, (uint)serviceCode, (uint)funcCode, serviceName);
        }

        public void InsertAttribute(CipInstance cipInstance, ushort attributeNumber, CIPDataType cipType, CipAttributeEncodeInMessage encodeFunctionCode, CipAttributeDecodeFromMessage decodeFunctionCode, byte[] data, CIPAttributeFlag cipFlags) {
            var ptr = cipInstance.Impl;

            this.NativeInsertAttribute(ptr, attributeNumber, (byte)cipType, (uint)encodeFunctionCode, (uint)decodeFunctionCode, data, (byte)cipFlags);
        }

        //public void SetDeviceRevision(byte major, byte minor) => this.NativeSetDeviceRevision(major, minor);

        //public void SetDeviceType(ushort type) => this.NativeSetDeviceType(type);

        //public void SetDeviceProductCode(ushort code) => this.NativeSetDeviceProductCode(code);

        //public void SetDeviceStatus(uint status) => this.NativeSetDeviceStatus(status);

        //public void SetDeviceVendorId(uint vendorId) => this.NativeSetDeviceVendorId(vendorId);

        

        public void InitCipStackDefault() => this.NativeInitCipStackDefault();

        public void Open() => this.NativeOpen();

        public void Start() => this.NativeStart();

        //////////////////////////////// Native code //////////////////////////////

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void Acquire(string deviceName, uint deviceVendorID, uint deviceType, uint deviceProductCode, uint deviceSerialNumber, uint deviceMajorRevision, uint deviceMinorRevision);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetDeviceSerialNumber(uint serialNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeConfigureExclusiveOwnerConnectionPoint(uint connectionNumber, uint outputAssemblyId, uint inputAssemblyId, uint configurationAssemblyId);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeConfigureInputOnlyConnectionPoint(uint connectionNumber, uint outputAssemblyId, uint inputAssemblyId, uint configurationAssemblyId);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeConfigureListenOnlyConnectionPoint(uint connectionNumber, uint outputAssemblyId, uint inputAssemblyId, uint configurationAssemblyId);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeOpen();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInsertService(IntPtr cipClassPtr, uint serviceCode, uint functionCode, string serviceName);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInsertAttribute(IntPtr cipInstancePtr, ushort attributeNumber, byte cipType, uint encodeFunctionCode, uint decodeFunctionCode, byte[] data, uint cipFlags);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetDeviceRevision(byte major, byte minor);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetDeviceType(ushort type);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetDeviceProductCode(ushort code);

         [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetDeviceStatus(uint status);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetDeviceVendorId(uint vendorId);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInitCipStackDefault();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeStart();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetDeviceProductName(string name);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr NativeCreateCipClass(uint classCode, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr NativeCreateAssemblyObject(int instanceId, byte[] data, ushort size);

        //////////////////////////////// Test code //////////////////////////////
        public void DoTest() {
            this.DoNativeTest(); ;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void DoNativeTest();
    }
}
