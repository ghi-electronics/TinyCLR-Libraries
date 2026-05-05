// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

using System;
using System.Collections;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.EthernetIP.Adapter
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

        static bool isEnabled = false;   
        static bool isInitialized = false;   

        private readonly NativeEventDispatcher nedReceivedExplictTcpData;
        public delegate void ReceivedExplictTcpDataHandler(AdapterController adapter, ushort commandCode, IPAddress ipAdrress);
        private ReceivedExplictTcpDataHandler eventReceivedExplictTcpDataHandler;

        private readonly NativeEventDispatcher nedReceivedExplictUdpData;
        public delegate void ReceivedExplictUdpDataHandler(AdapterController adapter, ushort commandCode, IPAddress ipAdrress, bool unicast);
        private ReceivedExplictUdpDataHandler eventReceivedExplictUdpDataHandler;

        private readonly NativeEventDispatcher nedNotifyClass;
        public delegate void NotifyClassHandler(AdapterController adapter, uint classCode, ushort instanceNumbber, ushort attributeNumber, IPAddress ipAdrress);
        private NotifyClassHandler eventNotifyClassHandler;

        private readonly NativeEventDispatcher nedAfterAssemblyDataReceived;
        public delegate void AfterAssemblyDataReceivedHandler(AdapterController adapter, ushort instanceNumbber);
        private AfterAssemblyDataReceivedHandler eventAfterAssemblyDataReceivedHandler;

        private readonly NativeEventDispatcher nedBeforeAssemblyDataSend;
        public delegate void BeforeAssemblyDataSendHandler(AdapterController adapter, ushort instanceNumbber);
        private BeforeAssemblyDataSendHandler eventBeforeAssemblyDataSendHandler;


        public delegate void RegisterSessionHandler(AdapterController adapter, IPAddress ipAdrress);
        private RegisterSessionHandler eventRegisterSessionHandler;
        private RegisterSessionHandler eventUnregisterSessionHandler;

        private readonly NativeEventDispatcher nedForwardOpen;
        public delegate void ForwardOpenHandler(AdapterController adapter, IPAddress ipAdrress, bool large);
        private ForwardOpenHandler eventForwardOpenHandler;

        private readonly NativeEventDispatcher nedForwardClose;
        public delegate void ForwardCloseHandler(AdapterController adapter, IPAddress ipAdrress);
        private ForwardCloseHandler eventForwardCloseHandler;

        public AdapterController(string deviceName, uint deviceVendorID, ushort deviceType, ushort deviceProductCode, uint deviceSerialNumber, byte deviceMajorRevision, byte deviceMinorRevision) {
            if (isInitialized) {
                throw new Exception("The controller is initialized already.");
            }
            

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

            this.nedReceivedExplictTcpData = NativeEventDispatcher.GetDispatcher("EthernetIP.Adapter.ReceivedExplictTcpData");
            this.nedReceivedExplictTcpData.OnInterrupt += this.NedReceivedExplictTcpData_OnInterrupt;

            this.nedReceivedExplictUdpData = NativeEventDispatcher.GetDispatcher("EthernetIP.Adapter.ReceivedExplictUdpData");
            this.nedReceivedExplictUdpData.OnInterrupt += this.NedReceivedExplictUdpData_OnInterrupt;

            this.nedNotifyClass = NativeEventDispatcher.GetDispatcher("EthernetIP.Adapter.HandleNotifyClass");
            this.nedNotifyClass.OnInterrupt += this.NedNotifyClass_OnInterrupt;

            this.nedAfterAssemblyDataReceived = NativeEventDispatcher.GetDispatcher("EthernetIP.Adapter.AfterAssemblyDataReceived");
            this.nedAfterAssemblyDataReceived.OnInterrupt += this.NedAssemblyConnectedDataReceived_OnInterrupt;

            this.nedBeforeAssemblyDataSend = NativeEventDispatcher.GetDispatcher("EthernetIP.Adapter.BeforeAssemblyDataSend");
            this.nedBeforeAssemblyDataSend.OnInterrupt += this.NedBeforeAssemblyDataSend_OnInterrupt;

            this.nedForwardOpen = NativeEventDispatcher.GetDispatcher("EthernetIP.Adapter.ForwardOpen");
            this.nedForwardOpen.OnInterrupt += this.NedForwardOpen_OnInterrupt;

            this.nedForwardClose = NativeEventDispatcher.GetDispatcher("EthernetIP.Adapter.ForwardClose");
            this.nedForwardClose.OnInterrupt += this.NedForwardClose_OnInterrupt;

            this.InitCipStack(false);

            isInitialized = true;
        }

        private void NedForwardOpen_OnInterrupt(string data0, long data1, long data2, long data3, IntPtr data4, DateTime timestamp) {
            var originator_address = IPAddress.None;


            if (data1 != 0)
                originator_address = new IPAddress(data1);

            this.eventForwardOpenHandler?.Invoke(this, originator_address, data2 != 0 ? true: false);
        }
        private void NedForwardClose_OnInterrupt(string data0, long data1, long data2, long data3, IntPtr data4, DateTime timestamp) {
            var originator_address = IPAddress.None;


            if (data1 != 0)
                originator_address = new IPAddress(data1);

            this.eventForwardCloseHandler?.Invoke(this, originator_address);
        }

        private void NedBeforeAssemblyDataSend_OnInterrupt(string data0, long data1, long data2, long data3, IntPtr data4, DateTime timestamp) {
            var instanceNumber = (ushort)(data1);

            this.eventBeforeAssemblyDataSendHandler?.Invoke(this, instanceNumber);
            ;
        }

        private void NedAssemblyConnectedDataReceived_OnInterrupt(string data0, long data1, long data2, long data3, IntPtr data4, DateTime timestamp) {
          
            var instanceNumber = (ushort)(data1);

            this.eventAfterAssemblyDataReceivedHandler?.Invoke(this, instanceNumber);
            ;

        }

        private void NedNotifyClass_OnInterrupt(string data0, long data1, long data2, long data3, IntPtr data4, DateTime timestamp) {
            var originator_address = IPAddress.None;
            var classCode = (uint)data1;
            var instanceNumber = (ushort)(data2 >> 16);
            var attributeNumber = (ushort)(data2 >> 0);

            if (data3 != 0)
                originator_address = new IPAddress(data3);

            this.eventNotifyClassHandler?.Invoke(this, classCode, instanceNumber, attributeNumber, originator_address);
            ;
        }

        private void NedReceivedExplictUdpData_OnInterrupt(string data0, long data1, long data2, long data3, IntPtr data4, DateTime timestamp) {
            var from_address = IPAddress.None;

            if (data2 != 0)
                from_address = new IPAddress(data2);

            this.eventReceivedExplictUdpDataHandler?.Invoke(this, (ushort)data1, from_address, data3 != 0 ? true : false);
            ;
        }

        private void NedReceivedExplictTcpData_OnInterrupt(string data0, long data1, long data2, long data3, IntPtr data4, DateTime timestamp) {
            var originator_address = IPAddress.None;

            if (data2 != 0)
                originator_address = new IPAddress(data2);

            this.eventReceivedExplictTcpDataHandler?.Invoke(this, (ushort)data1, originator_address);

            if (data1 == (long)EncapsulationCommand.RegisterSession) {
                this.eventRegisterSessionHandler?.Invoke(this, originator_address);
            }

            else if (data1 == (long)EncapsulationCommand.UnregisterSession) {
                this.eventUnregisterSessionHandler?.Invoke(this, originator_address);
            }

        }

        public event ReceivedExplictTcpDataHandler ReceivedExplictTcpData {

            add => this.eventReceivedExplictTcpDataHandler += value;
            remove => this.eventReceivedExplictTcpDataHandler -= value;
        }

        public event ReceivedExplictUdpDataHandler ReceivedExplictUdpData {

            add => this.eventReceivedExplictUdpDataHandler += value;
            remove => this.eventReceivedExplictUdpDataHandler -= value;
        }

        public event NotifyClassHandler NotifyClass {

            add => this.eventNotifyClassHandler += value;
            remove => this.eventNotifyClassHandler -= value;
        }

        public event AfterAssemblyDataReceivedHandler AfterAssemblyDataReceived {

            add => this.eventAfterAssemblyDataReceivedHandler += value;
            remove => this.eventAfterAssemblyDataReceivedHandler -= value;
        }

        public event BeforeAssemblyDataSendHandler BeforeAssemblyDataSend {

            add => this.eventBeforeAssemblyDataSendHandler += value;
            remove => this.eventBeforeAssemblyDataSendHandler -= value;
        }

        public event RegisterSessionHandler RegisterSessionDetected {

            add => this.eventRegisterSessionHandler += value;
            remove => this.eventRegisterSessionHandler -= value;
        }

        public event RegisterSessionHandler UnregisterSessionDetected {

            add => this.eventUnregisterSessionHandler += value;
            remove => this.eventUnregisterSessionHandler -= value;
        }

        public event ForwardOpenHandler ForwardOpenDetected {

            add => this.eventForwardOpenHandler += value;
            remove => this.eventForwardOpenHandler -= value;
        }

        public event ForwardCloseHandler ForwardCloseDetected {

            add => this.eventForwardCloseHandler += value;
            remove => this.eventForwardCloseHandler -= value;
        }

        public void AddCipClass(CIPClass cipClass) {

            //var cip = cipClass;

            cipClass.Impl = this.NativeCreateCipClass((uint)cipClass.ClassCode, cipClass.NumberClassAttributes, cipClass.HighestClassAttributeNumber, cipClass.NumberClassServices, cipClass.NumberInstanceAttributes, cipClass.HighestInstanceAttributeNumber, cipClass.NumberInstanceServices, cipClass.NumberInstances, cipClass.Name, cipClass.Revision, cipClass.DefaultInitialize);
            ;
            //this.cipClassesList.Add(cip);            
        }

        public void AddAssemblyObject(AssemblyObject asmObject) {

            //var obj = asmObject;

            asmObject.Impl = this.NativeCreateAssemblyObject(asmObject.InstanceId, asmObject.Data, asmObject.Size); ;

            //this.assemblyObjectsList.Add(obj);
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

        public CIPClass CreateAssemblyClass(int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision) {

            var cipClass = new CIPClass(ClassId.Assembly, numberClassAttributes, highestClassAttributeNumber, numberClassServices, numberInstanceAttributes, highestInstanceAttributeNumber, numberInstanceServices, numberInstances, name, revision) {
                Impl = this.NativeCreateAssemblyClass((uint)ClassId.Assembly, numberClassAttributes, highestClassAttributeNumber, numberClassServices, numberInstanceAttributes, highestInstanceAttributeNumber, numberInstanceServices, numberInstances, name, revision, true)
            };

            return cipClass;
        }

        private void InitCipStackDefault() => this.NativeInitCipStackDefault(); // for testing only

        private void Open() => this.NativeOpen();// for testing only


        public void Enable() {
            if (isEnabled) {
                throw new Exception("The controller is enabled already.");
            }
            
            isEnabled = true;

            this.NativeEnable();
        }

        public void Disable() {
            // TODO not implemented
            throw new NotImplementedException(); ;
        }

        public void AddCipInstance(CIPClass cipClass, uint instanceId) {
            //var instance = new CipInstance {
            //    Impl = this.NativeAddCipInstance(cipClass.Impl, instanceId)
            //};

            //return instance;

            this.NativeAddCipInstance(cipClass.Impl, instanceId); ;
        }

        public void AddCipInstances(CIPClass cipClass, uint instanceId) {
            //var instance = new CipInstance {
            //    Impl = this.NativeAddCipInstances(cipClass.Impl, instanceId)
            //};

            //return instance;

            this.NativeAddCipInstances(cipClass.Impl, instanceId); ;
        }

        public void AllocateAttributeMasks(CIPClass targetClass) => this.NativeAllocateAttributeMasks(targetClass.Impl);
        public void CalculateIndex(ushort attributeNumber) => this.NativeCalculateIndex(attributeNumber);

        public CipAttribute GetCipAttribute(CipInstance cipInstance, ushort attributeNumber) {

            var attribute = new CipAttribute {
                Impl = this.NativeGetCipAttribute(cipInstance.Impl, attributeNumber)
            };

            return attribute;
        }

        public CIPClass GetCipClass(ushort classId) {

            var cipclass = new CIPClass { Impl = this.NativeGetCipClass(classId) };

            return cipclass;
        }

        public CipInstance GetCipInstance(CIPClass cipClass, uint instanceNumber) {

            var cipinstance = new CipInstance { Impl = this.NativeGetCipInstance(cipClass.Impl, instanceNumber) };

            if (((uint)cipinstance.Impl) == 0) {
                return null;
            }

            return cipinstance;
        }

        private void InitCipStack(bool useDefault) => this.NativeInitCipStack(useDefault);

        private void MessageRouterInit(CIPClass cipClass) {
            this.AddCipClass(cipClass);
            this.NativeMessageRouterInit(cipClass.Impl, cipClass.NumberClassAttributes, cipClass.HighestClassAttributeNumber, cipClass.NumberClassServices, cipClass.NumberInstanceAttributes, cipClass.HighestInstanceAttributeNumber, cipClass.NumberInstanceServices, cipClass.NumberInstances, cipClass.Name, cipClass.Revision, false);

        }

        private void IdentityInit(CIPClass cipClass) {
            this.AddCipClass(cipClass);
            this.NativeIdentityInit(cipClass.Impl, cipClass.NumberClassAttributes, cipClass.HighestClassAttributeNumber, cipClass.NumberClassServices, cipClass.NumberInstanceAttributes, cipClass.HighestInstanceAttributeNumber, cipClass.NumberInstanceServices, cipClass.NumberInstances, cipClass.Name, cipClass.Revision, false);

        }

        private void ConnectionManagerInit(CIPClass cipClass) {
            this.AddCipClass(cipClass);
            this.NativeConnectionManagerInit(cipClass.Impl, cipClass.NumberClassAttributes, cipClass.HighestClassAttributeNumber, cipClass.NumberClassServices, cipClass.NumberInstanceAttributes, cipClass.HighestInstanceAttributeNumber, cipClass.NumberInstanceServices, cipClass.NumberInstances, cipClass.Name, cipClass.Revision, false);

        }

        public void EnableHeaderO2T(bool on) => this.NativeRunIdleHeaderSetO2T(on);
        public void EnableHeaderT2O(bool on) => this.NativeRunIdleHeaderSetT2O(on);

        //////////////////////////////// Native code //////////////////////////////

        private void Acquire(string deviceName, uint deviceVendorID, uint deviceType, uint deviceProductCode, uint deviceSerialNumber, uint deviceMajorRevision, uint deviceMinorRevision) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeSetDeviceSerialNumber(uint serialNumber) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeConfigureExclusiveOwnerConnectionPoint(uint connectionNumber, uint outputAssemblyId, uint inputAssemblyId, uint configurationAssemblyId) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeConfigureInputOnlyConnectionPoint(uint connectionNumber, uint outputAssemblyId, uint inputAssemblyId, uint configurationAssemblyId) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeConfigureListenOnlyConnectionPoint(uint connectionNumber, uint outputAssemblyId, uint inputAssemblyId, uint configurationAssemblyId) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeOpen() => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeInsertService(IntPtr cipClassPtr, uint serviceCode, uint functionCode, string serviceName) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeInsertAttribute(IntPtr cipInstancePtr, ushort attributeNumber, byte cipType, uint encodeFunctionCode, uint decodeFunctionCode, byte[] data, uint cipFlags) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeSetDeviceRevision(byte major, byte minor) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeSetDeviceType(ushort type) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeSetDeviceProductCode(ushort code) => throw new System.NotSupportedException("TODO - Not supported");
         private void NativeSetDeviceStatus(uint status) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeSetDeviceVendorId(uint vendorId) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeInitCipStackDefault() => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeEnable() => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeDisable() => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeSetDeviceProductName(string name) => throw new System.NotSupportedException("TODO - Not supported");
        private IntPtr NativeCreateCipClass(uint classCode, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize) => throw new System.NotSupportedException("TODO - Not supported");
        private IntPtr NativeCreateAssemblyObject(int instanceId, byte[] data, ushort size) => throw new System.NotSupportedException("TODO - Not supported");
        private IntPtr NativeAddCipInstance(IntPtr cipClassImpl, uint instanceId) => throw new System.NotSupportedException("TODO - Not supported");
        private IntPtr NativeAddCipInstances(IntPtr cipClassImpl, uint instanceId) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeAllocateAttributeMasks(IntPtr cipClassImpl) => throw new System.NotSupportedException("TODO - Not supported");
        private IntPtr NativeCalculateIndex(ushort attributeNumber) => throw new System.NotSupportedException("TODO - Not supported");
        private IntPtr NativeGetCipAttribute(IntPtr cipinstance, uint attributeNumber) => throw new System.NotSupportedException("TODO - Not supported");
        private IntPtr NativeGetCipClass(ushort classId) => throw new System.NotSupportedException("TODO - Not supported");
        private IntPtr NativeGetCipInstance(IntPtr cipClass, uint instanceNumber) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeInitCipStack(bool useDefault) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeMessageRouterInit(IntPtr cipClass, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeIdentityInit(IntPtr cipClass, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeConnectionManagerInit(IntPtr cipClass, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize) => throw new System.NotSupportedException("TODO - Not supported");
        private IntPtr NativeCreateAssemblyClass(uint classCode, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeRunIdleHeaderSetO2T(bool on) => throw new System.NotSupportedException("TODO - Not supported");
        private void NativeRunIdleHeaderSetT2O(bool on) => throw new System.NotSupportedException("TODO - Not supported");
        //////////////////////////////// Test code //////////////////////////////
        public void DoTest() {
            this.DoNativeTest(); ;
        }

        private void DoNativeTest() => throw new System.NotSupportedException("TODO - Not supported");
    }
}
