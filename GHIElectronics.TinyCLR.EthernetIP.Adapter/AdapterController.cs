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
    
    public partial class AdapterController : IDisposable
    {
        // Phase 3.5: storage widths now match CIP Identity attribute widths per ODVA Vol 1.
        // VendorID, DeviceType, ProductCode are UINT (16-bit). Major/Minor Revision are
        // USINT (8-bit). SerialNumber stays UDINT (32-bit). Previously these were all
        // `uint` storage, which silently allowed out-of-range values that the wire layer
        // truncated.
        private ushort deviceVendorID;
        private ushort deviceType;
        private ushort deviceProductCode;
        private byte deviceMajorRevision;
        private byte deviceMinorRevision;
        private string deviceName;
        private uint deviceSerialNumber;

        private ArrayList cipClassesList;
        private ArrayList assemblyObjectsList;

        static bool isEnabled = false;   
        static bool isInitialized = false;   

        private readonly NativeEventDispatcher nedReceivedExplicitTcpData;
        public delegate void ReceivedExplicitTcpDataHandler(AdapterController adapter, ushort commandCode, IPAddress ipAddress);
        private ReceivedExplicitTcpDataHandler eventReceivedExplicitTcpDataHandler;

        private readonly NativeEventDispatcher nedReceivedExplicitUdpData;
        public delegate void ReceivedExplicitUdpDataHandler(AdapterController adapter, ushort commandCode, IPAddress ipAddress, bool unicast);
        private ReceivedExplicitUdpDataHandler eventReceivedExplicitUdpDataHandler;

        private readonly NativeEventDispatcher nedNotifyClass;
        public delegate void NotifyClassHandler(AdapterController adapter, uint classCode, ushort instanceNumber, ushort attributeNumber, IPAddress ipAddress);
        private NotifyClassHandler eventNotifyClassHandler;

        private readonly NativeEventDispatcher nedAfterAssemblyDataReceived;
        public delegate void AfterAssemblyDataReceivedHandler(AdapterController adapter, ushort instanceNumber);
        private AfterAssemblyDataReceivedHandler eventAfterAssemblyDataReceivedHandler;

        private readonly NativeEventDispatcher nedBeforeAssemblyDataSend;
        public delegate void BeforeAssemblyDataSendHandler(AdapterController adapter, ushort instanceNumber);
        private BeforeAssemblyDataSendHandler eventBeforeAssemblyDataSendHandler;


        public delegate void RegisterSessionHandler(AdapterController adapter, IPAddress ipAddress);
        private RegisterSessionHandler eventRegisterSessionHandler;
        private RegisterSessionHandler eventUnregisterSessionHandler;

        private readonly NativeEventDispatcher nedForwardOpen;
        public delegate void ForwardOpenHandler(AdapterController adapter, IPAddress ipAddress, bool large);
        private ForwardOpenHandler eventForwardOpenHandler;

        private readonly NativeEventDispatcher nedForwardClose;
        public delegate void ForwardCloseHandler(AdapterController adapter, IPAddress ipAddress);
        private ForwardCloseHandler eventForwardCloseHandler;

        public AdapterController(string deviceName, ushort deviceVendorID, ushort deviceType, ushort deviceProductCode, uint deviceSerialNumber, byte deviceMajorRevision, byte deviceMinorRevision) {
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

            this.nedReceivedExplicitTcpData = NativeEventDispatcher.GetDispatcher("EthernetIP.Adapter.ReceivedExplicitTcpData");
            this.nedReceivedExplicitTcpData.OnInterrupt += this.NedReceivedExplicitTcpData_OnInterrupt;

            this.nedReceivedExplicitUdpData = NativeEventDispatcher.GetDispatcher("EthernetIP.Adapter.ReceivedExplicitUdpData");
            this.nedReceivedExplicitUdpData.OnInterrupt += this.NedReceivedExplicitUdpData_OnInterrupt;

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

        private void NedReceivedExplicitUdpData_OnInterrupt(string data0, long data1, long data2, long data3, IntPtr data4, DateTime timestamp) {
            var from_address = IPAddress.None;

            if (data2 != 0)
                from_address = new IPAddress(data2);

            this.eventReceivedExplicitUdpDataHandler?.Invoke(this, (ushort)data1, from_address, data3 != 0 ? true : false);
            ;
        }

        private void NedReceivedExplicitTcpData_OnInterrupt(string data0, long data1, long data2, long data3, IntPtr data4, DateTime timestamp) {
            var originator_address = IPAddress.None;

            if (data2 != 0)
                originator_address = new IPAddress(data2);

            this.eventReceivedExplicitTcpDataHandler?.Invoke(this, (ushort)data1, originator_address);

            if (data1 == (long)EncapsulationCommand.RegisterSession) {
                this.eventRegisterSessionHandler?.Invoke(this, originator_address);
            }

            else if (data1 == (long)EncapsulationCommand.UnregisterSession) {
                this.eventUnregisterSessionHandler?.Invoke(this, originator_address);
            }

        }

        public event ReceivedExplicitTcpDataHandler ReceivedExplicitTcpData {

            add => this.eventReceivedExplicitTcpDataHandler += value;
            remove => this.eventReceivedExplicitTcpDataHandler -= value;
        }

        public event ReceivedExplicitUdpDataHandler ReceivedExplicitUdpData {

            add => this.eventReceivedExplicitUdpDataHandler += value;
            remove => this.eventReceivedExplicitUdpDataHandler -= value;
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

        // serviceCode: the CIP service number recorded on the class's service slot;
        // returned to the scanner in the reply-service byte.
        // handlerCode: selects which native handler function (ForwardOpen, GetAttributeAll,
        // ResetService, etc.) is bound to that slot. Usually equal to serviceCode but
        // can differ when redirecting a service to a custom handler.
        public void InsertService(CIPClass cipClass, CIPServiceCode serviceCode, CIPServiceCode handlerCode, string serviceName) {
            var ptr = cipClass.Impl;

            this.NativeInsertService(ptr, (uint)serviceCode, (uint)handlerCode, serviceName);
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

        // Phase 3.5: Disable() and Dispose() now do real teardown. Previously Disable
        // threw NotImplementedException and there was no Dispose at all; the opener
        // thread + its 8 KB stack stayed alive for the process lifetime.
        //
        // Disable() is kept as a synonym of Dispose() for API symmetry with Enable.
        // Either one is safe to call multiple times.
        public void Disable() => this.Dispose();

        private bool disposed;
        public void Dispose() {
            if (this.disposed) return;
            this.disposed = true;

            // Native side signals g_end_stack=1, waits up to 1 s for the opener
            // thread to terminate (during which the thread calls ShutdownCipStack
            // to close connections + delete every registered CipClass), then
            // frees the 8 KB thread stack.
            this.NativeShutdown();

            // Allow a fresh AdapterController to be constructed after this one is
            // disposed. Without resetting these, the constructor's isInitialized
            // guard would block the next `new AdapterController(...)`.
            isEnabled = false;
            isInitialized = false;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeShutdown();

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
        private extern void NativeSetDeviceVendorId(ushort vendorId);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInitCipStackDefault();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeEnable();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeDisable();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeSetDeviceProductName(string name);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr NativeCreateCipClass(uint classCode, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr NativeCreateAssemblyObject(int instanceId, byte[] data, ushort size);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr NativeAddCipInstance(IntPtr cipClassImpl, uint instanceId);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr NativeAddCipInstances(IntPtr cipClassImpl, uint instanceId);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeAllocateAttributeMasks(IntPtr cipClassImpl);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr NativeCalculateIndex(ushort attributeNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr NativeGetCipAttribute(IntPtr cipinstance, uint attributeNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr NativeGetCipClass(ushort classId);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr NativeGetCipInstance(IntPtr cipClass, uint instanceNumber);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInitCipStack(bool useDefault);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeMessageRouterInit(IntPtr cipClass, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeIdentityInit(IntPtr cipClass, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeConnectionManagerInit(IntPtr cipClass, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern IntPtr NativeCreateAssemblyClass(uint classCode, int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision, bool defaultInitialize);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeRunIdleHeaderSetO2T(bool on);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeRunIdleHeaderSetT2O(bool on);
    }
}
