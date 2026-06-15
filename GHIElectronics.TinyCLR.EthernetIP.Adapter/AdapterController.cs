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
    
    /// <summary>
    /// Runs the device as an EtherNet/IP <b>Adapter</b> (the server side of EIP — what
    /// a PLC scanner connects to). Wraps the native OpENer stack with a managed-C# API.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Singleton</b>: only one instance can exist at a time. Disposing or calling
    /// <see cref="Disable"/> resets the singleton flag so a fresh controller can be
    /// constructed.
    /// </para>
    /// <para>
    /// <b>Typical bring-up</b> (full example in <c>README.md</c> and
    /// <c>Test\TinyCLRApplication_EthernetIP\Program.cs</c>):
    /// <code>
    /// using (var adapter = new AdapterController("MyDev", 0x1234, 12, 100, 0x01020304, 1, 0)) {
    ///     adapter.EnableHeaderO2T(true);                                  // for AB scanners
    ///     adapter.AddAssemblyObject(new AssemblyObject(100, input,  32)); // T->O
    ///     adapter.AddAssemblyObject(new AssemblyObject(150, output, 32)); // O->T
    ///     adapter.AddAssemblyObject(new AssemblyObject(151, config, 10));
    ///     adapter.ConfigureExclusiveOwnerConnectionPoint(0, 150, 100, 151);
    ///     adapter.Enable();
    ///     while (true) Thread.Sleep(10);   // your app produces / consumes IO
    /// }   // Dispose() shuts down cleanly
    /// </code>
    /// </para>
    /// <para>
    /// <b>Identity, Message Router, Connection Manager, Assembly, and QoS classes are
    /// auto-initialized</b> in <see cref="Enable"/> if user code didn't already register
    /// them. So a minimal adapter that only adds Assembly objects works out-of-the-box.
    /// </para>
    /// </remarks>
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
        /// <summary>Handles an explicit TCP encapsulation command received from a scanner.</summary>
        public delegate void ReceivedExplicitTcpDataHandler(AdapterController adapter, ushort commandCode, IPAddress ipAddress);
        private ReceivedExplicitTcpDataHandler eventReceivedExplicitTcpDataHandler;

        private readonly NativeEventDispatcher nedReceivedExplicitUdpData;
        /// <summary>Handles an explicit UDP encapsulation command received from a scanner.</summary>
        public delegate void ReceivedExplicitUdpDataHandler(AdapterController adapter, ushort commandCode, IPAddress ipAddress, bool unicast);
        private ReceivedExplicitUdpDataHandler eventReceivedExplicitUdpDataHandler;

        private readonly NativeEventDispatcher nedNotifyClass;
        /// <summary>Handles notification that a scanner accessed a CIP class, instance, and attribute.</summary>
        public delegate void NotifyClassHandler(AdapterController adapter, uint classCode, ushort instanceNumber, ushort attributeNumber, IPAddress ipAddress);
        private NotifyClassHandler eventNotifyClassHandler;

        private readonly NativeEventDispatcher nedAfterAssemblyDataReceived;
        /// <summary>Handles assembly data having been received into an assembly instance.</summary>
        public delegate void AfterAssemblyDataReceivedHandler(AdapterController adapter, ushort instanceNumber);
        private AfterAssemblyDataReceivedHandler eventAfterAssemblyDataReceivedHandler;

        private readonly NativeEventDispatcher nedBeforeAssemblyDataSend;
        /// <summary>Handles the moment just before assembly data is sent from an assembly instance.</summary>
        public delegate void BeforeAssemblyDataSendHandler(AdapterController adapter, ushort instanceNumber);
        private BeforeAssemblyDataSendHandler eventBeforeAssemblyDataSendHandler;


        /// <summary>Handles a scanner registering or unregistering an encapsulation session.</summary>
        public delegate void RegisterSessionHandler(AdapterController adapter, IPAddress ipAddress);
        private RegisterSessionHandler eventRegisterSessionHandler;
        private RegisterSessionHandler eventUnregisterSessionHandler;

        private readonly NativeEventDispatcher nedForwardOpen;
        /// <summary>Handles a successful Forward Open opening a CIP connection.</summary>
        public delegate void ForwardOpenHandler(AdapterController adapter, IPAddress ipAddress, bool large);
        private ForwardOpenHandler eventForwardOpenHandler;

        private readonly NativeEventDispatcher nedForwardClose;
        /// <summary>Handles a Forward Close tearing down a CIP connection.</summary>
        public delegate void ForwardCloseHandler(AdapterController adapter, IPAddress ipAddress);
        private ForwardCloseHandler eventForwardCloseHandler;

        /// <summary>Construct the adapter. Identity values are set on the underlying CIP
        /// Identity object but the network stack isn't started yet — call <see cref="Enable"/>
        /// after wiring up assemblies and connection points.</summary>
        /// <param name="deviceName">Human-readable product name (Identity attr 7). Must
        /// match the <c>ProdName</c> field in your EDS file.</param>
        /// <param name="deviceVendorID">ODVA-assigned vendor ID (CIP UINT). Must match
        /// the <c>VendCode</c> field in your EDS file. ODVA values 1–99 are reserved for
        /// specific companies.</param>
        /// <param name="deviceType">CIP device type per Vol 1 Appendix A (e.g. 0x000C =
        /// Generic Device).</param>
        /// <param name="deviceProductCode">Vendor-assigned product code (Identity attr 3).</param>
        /// <param name="deviceSerialNumber">32-bit serial number (Identity attr 6). Should
        /// be unique per physical device — typically read from non-volatile storage.</param>
        /// <param name="deviceMajorRevision">Major revision (USINT, 0–255).</param>
        /// <param name="deviceMinorRevision">Minor revision (USINT, 0–255).</param>
        /// <exception cref="System.Exception">Thrown if another <c>AdapterController</c> is
        /// already constructed and hasn't been disposed (singleton enforcement).</exception>
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

        /// <summary>Fires for every explicit TCP encapsulation command received
        /// (RegisterSession, UnregisterSession, SendRRData, SendUnitData, etc.).
        /// Argument <c>commandCode</c> is an <see cref="EncapsulationCommand"/> value.
        /// Mostly diagnostic; for real connection state, use the lifecycle-specific
        /// events below.</summary>
        public event ReceivedExplicitTcpDataHandler ReceivedExplicitTcpData {
            add => this.eventReceivedExplicitTcpDataHandler += value;
            remove => this.eventReceivedExplicitTcpDataHandler -= value;
        }

        /// <summary>Fires for every explicit UDP encapsulation command received
        /// (ListIdentity, ListServices, ListInterfaces). <c>unicast</c> is true if
        /// the request came via UDP unicast vs broadcast.</summary>
        public event ReceivedExplicitUdpDataHandler ReceivedExplicitUdpData {
            add => this.eventReceivedExplicitUdpDataHandler += value;
            remove => this.eventReceivedExplicitUdpDataHandler -= value;
        }

        /// <summary>Fires when any CIP class on this device is accessed by a scanner.
        /// Diagnostic — lets you log which class/instance/attribute the scanner
        /// touched.</summary>
        public event NotifyClassHandler NotifyClass {
            add => this.eventNotifyClassHandler += value;
            remove => this.eventNotifyClassHandler -= value;
        }

        /// <summary>Fires after a Class-1 implicit packet has been received and copied
        /// into the assembly's data buffer. Use this hook to react to scanner-written
        /// output values.</summary>
        public event AfterAssemblyDataReceivedHandler AfterAssemblyDataReceived {
            add => this.eventAfterAssemblyDataReceivedHandler += value;
            remove => this.eventAfterAssemblyDataReceivedHandler -= value;
        }

        /// <summary>Fires immediately before a Class-1 implicit packet is sent.
        /// Hook to refresh input-assembly values from your application state just
        /// before the wire goes out.</summary>
        public event BeforeAssemblyDataSendHandler BeforeAssemblyDataSend {
            add => this.eventBeforeAssemblyDataSendHandler += value;
            remove => this.eventBeforeAssemblyDataSendHandler -= value;
        }

        /// <summary>Fires when a scanner successfully registers an encapsulation
        /// session (TCP RegisterSession command). Argument is the originator's IP.</summary>
        public event RegisterSessionHandler RegisterSessionDetected {
            add => this.eventRegisterSessionHandler += value;
            remove => this.eventRegisterSessionHandler -= value;
        }

        /// <summary>Fires when a scanner closes its encapsulation session
        /// (UnregisterSession command). Argument is the originator's IP.</summary>
        public event RegisterSessionHandler UnregisterSessionDetected {
            add => this.eventUnregisterSessionHandler += value;
            remove => this.eventUnregisterSessionHandler -= value;
        }

        /// <summary>Fires when a Forward Open (regular or large) succeeds — a CIP
        /// connection is now open. <c>large</c> argument is true for Large Forward
        /// Open (service 0x5B), false for regular (service 0x54).
        /// <para>
        /// As of Phase 3.5 this only fires on <i>successful</i> Forward Opens; failed
        /// attempts (resource exhaustion, electronic key mismatch, etc.) no longer
        /// trigger a phantom event.
        /// </para></summary>
        public event ForwardOpenHandler ForwardOpenDetected {
            add => this.eventForwardOpenHandler += value;
            remove => this.eventForwardOpenHandler -= value;
        }

        /// <summary>Fires when a Forward Close completes — the named CIP connection
        /// is being torn down.</summary>
        public event ForwardCloseHandler ForwardCloseDetected {
            add => this.eventForwardCloseHandler += value;
            remove => this.eventForwardCloseHandler -= value;
        }

        /// <summary>Registers a CIP object class with the adapter, creating it on the native stack.</summary>
        public void AddCipClass(CIPClass cipClass) {

            //var cip = cipClass;

            cipClass.Impl = this.NativeCreateCipClass((uint)cipClass.ClassCode, cipClass.NumberClassAttributes, cipClass.HighestClassAttributeNumber, cipClass.NumberClassServices, cipClass.NumberInstanceAttributes, cipClass.HighestInstanceAttributeNumber, cipClass.NumberInstanceServices, cipClass.NumberInstances, cipClass.Name, cipClass.Revision, cipClass.DefaultInitialize);
            ;
            //this.cipClassesList.Add(cip);            
        }

        /// <summary>Register a Class 4 (Assembly) instance the scanner can read or write
        /// via implicit (Class 1) or explicit messaging. Auto-creates the Assembly class
        /// itself on first call if not already present.</summary>
        /// <param name="asmObject">Assembly description. The backing <c>Data</c> byte[]
        /// is held by raw pointer on the native side — keep it alive (static or long-lived
        /// field) for the controller's lifetime, otherwise the GC may free it and the
        /// next Class-1 send reads garbage.</param>
        public void AddAssemblyObject(AssemblyObject asmObject) {

            //var obj = asmObject;

            asmObject.Impl = this.NativeCreateAssemblyObject(asmObject.InstanceId, asmObject.Data, asmObject.Size); ;

            //this.assemblyObjectsList.Add(obj);
        }

        //public void SetDeviceSerialNumber(uint serialNumber) => this.NativeSetDeviceSerialNumber(serialNumber);
        /// <summary>Register an <i>Exclusive Owner</i> connection point — the standard
        /// bidirectional Class-1 I/O connection. Scanner writes the output assembly,
        /// reads the input assembly, configures via the configuration assembly.</summary>
        /// <param name="connectionNumber">0-based slot index. Up to
        /// <c>OPENER_CIP_NUM_EXLUSIVE_OWNER_CONNS</c> exclusive-owner connections can
        /// be defined (currently 1).</param>
        /// <param name="outputAssemblyId">Instance ID of the O→T (output) assembly (scanner writes here).</param>
        /// <param name="inputAssemblyId">Instance ID of the T→O (input) assembly (target produces here).</param>
        /// <param name="configurationAssemblyId">Instance ID of the configuration assembly,
        /// sent during Forward Open.</param>
        public void ConfigureExclusiveOwnerConnectionPoint(uint connectionNumber, uint outputAssemblyId, uint inputAssemblyId, uint configurationAssemblyId) => this.NativeConfigureExclusiveOwnerConnectionPoint(connectionNumber, outputAssemblyId, inputAssemblyId, configurationAssemblyId);

        /// <summary>Register an <i>Input Only</i> connection point — scanner just reads
        /// the input assembly with no outputs (a "heartbeat" connection sized 0 for
        /// O→T).</summary>
        public void ConfigureInputOnlyConnectionPoint(uint connectionNumber, uint outputAssemblyId, uint inputAssemblyId, uint configurationAssemblyId) => this.NativeConfigureInputOnlyConnectionPoint(connectionNumber, outputAssemblyId, inputAssemblyId, configurationAssemblyId);

        /// <summary>Register a <i>Listen Only</i> connection point — a secondary
        /// scanner subscribes to the same multicast input data the Exclusive Owner is
        /// receiving, without owning the connection.</summary>
        public void ConfigureListenOnlyConnectionPoint(uint connectionNumber, uint outputAssemblyId, uint inputAssemblyId, uint configurationAssemblyId) => this.NativeConfigureListenOnlyConnectionPoint(connectionNumber, outputAssemblyId, inputAssemblyId, configurationAssemblyId);

        /// <summary>Adds a service to a CIP class, binding the given handler to the service slot.</summary>
        // serviceCode: the CIP service number recorded on the class's service slot;
        // returned to the scanner in the reply-service byte.
        // handlerCode: selects which native handler function (ForwardOpen, GetAttributeAll,
        // ResetService, etc.) is bound to that slot. Usually equal to serviceCode but
        // can differ when redirecting a service to a custom handler.
        public void InsertService(CIPClass cipClass, CIPServiceCode serviceCode, CIPServiceCode handlerCode, string serviceName) {
            var ptr = cipClass.Impl;

            this.NativeInsertService(ptr, (uint)serviceCode, (uint)handlerCode, serviceName);
        }

        /// <summary>Adds an attribute to a CIP instance, with its data type, encode/decode functions, data, and access flags.</summary>
        public void InsertAttribute(CipInstance cipInstance, ushort attributeNumber, CIPDataType cipType, CipAttributeEncodeInMessage encodeFunctionCode, CipAttributeDecodeFromMessage decodeFunctionCode, byte[] data, CIPAttributeFlag cipFlags) {
            var ptr = cipInstance.Impl;

            this.NativeInsertAttribute(ptr, attributeNumber, (byte)cipType, (uint)encodeFunctionCode, (uint)decodeFunctionCode, data, (byte)cipFlags);
        }
        
        //public void SetDeviceRevision(byte major, byte minor) => this.NativeSetDeviceRevision(major, minor);

        //public void SetDeviceType(ushort type) => this.NativeSetDeviceType(type);

        //public void SetDeviceProductCode(ushort code) => this.NativeSetDeviceProductCode(code);

        //public void SetDeviceStatus(uint status) => this.NativeSetDeviceStatus(status);

        //public void SetDeviceVendorId(uint vendorId) => this.NativeSetDeviceVendorId(vendorId);

        /// <summary>Creates and registers the Assembly (Class 4) object class with the given attribute/service counts.</summary>
        public CIPClass CreateAssemblyClass(int numberClassAttributes, uint highestClassAttributeNumber, int numberClassServices, int numberInstanceAttributes, uint highestInstanceAttributeNumber, int numberInstanceServices, uint numberInstances, string name, ushort revision) {

            var cipClass = new CIPClass(ClassId.Assembly, numberClassAttributes, highestClassAttributeNumber, numberClassServices, numberInstanceAttributes, highestInstanceAttributeNumber, numberInstanceServices, numberInstances, name, revision) {
                Impl = this.NativeCreateAssemblyClass((uint)ClassId.Assembly, numberClassAttributes, highestClassAttributeNumber, numberClassServices, numberInstanceAttributes, highestInstanceAttributeNumber, numberInstanceServices, numberInstances, name, revision, true)
            };

            return cipClass;
        }

        private void InitCipStackDefault() => this.NativeInitCipStackDefault(); // for testing only

        private void Open() => this.NativeOpen();// for testing only


        /// <summary>Start the EtherNet/IP stack: opens the four CIP sockets (TCP 44818,
        /// UDP 44818 unicast + broadcast, UDP 2222 for Class 1), spawns the OpENer
        /// processing thread, and auto-creates any standard CIP class (Identity,
        /// Message Router, Connection Manager, Assembly, QoS) that user code didn't
        /// already register via <c>AddCipClass</c>.</summary>
        /// <remarks>Idempotent guard: throws if already enabled. Use
        /// <see cref="Dispose"/> (or <see cref="Disable"/>) to tear down before
        /// re-enabling.</remarks>
        /// <exception cref="System.Exception">Thrown if the controller is already
        /// enabled.</exception>
        public void Enable() {
            if (isEnabled) {
                throw new Exception("The controller is enabled already.");
            }

            isEnabled = true;

            this.NativeEnable();
        }

        /// <summary>Stop the EtherNet/IP stack. Equivalent to <see cref="Dispose"/>.
        /// Idempotent — safe to call multiple times. Resets the singleton flag so a
        /// fresh controller can be constructed afterwards.</summary>
        public void Disable() => this.Dispose();

        private bool disposed;

        /// <summary>Tear down all native state cleanly: signals <c>g_end_stack=1</c>,
        /// polls up to 1 s for the OpENer thread to terminate (during which it calls
        /// <c>ShutdownCipStack</c> to close all CIP connections and delete every
        /// registered class), frees the 8 KB thread stack, and resets the singleton
        /// flags so a fresh <see cref="AdapterController"/> can be constructed.
        /// Idempotent. Suitable for use in a <c>using</c> block.</summary>
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

        /// <summary>Adds a single instance with the given instance ID to a CIP class.</summary>
        public void AddCipInstance(CIPClass cipClass, uint instanceId) {
            //var instance = new CipInstance {
            //    Impl = this.NativeAddCipInstance(cipClass.Impl, instanceId)
            //};

            //return instance;

            this.NativeAddCipInstance(cipClass.Impl, instanceId); ;
        }

        /// <summary>Adds the configured number of instances starting at the given instance ID to a CIP class.</summary>
        public void AddCipInstances(CIPClass cipClass, uint instanceId) {
            //var instance = new CipInstance {
            //    Impl = this.NativeAddCipInstances(cipClass.Impl, instanceId)
            //};

            //return instance;

            this.NativeAddCipInstances(cipClass.Impl, instanceId); ;
        }

        /// <summary>Allocates the get/set attribute bit masks for the given CIP class.</summary>
        public void AllocateAttributeMasks(CIPClass targetClass) => this.NativeAllocateAttributeMasks(targetClass.Impl);
        /// <summary>Calculates the internal index for the given attribute number.</summary>
        public void CalculateIndex(ushort attributeNumber) => this.NativeCalculateIndex(attributeNumber);

        /// <summary>Gets the attribute with the given number from a CIP instance.</summary>
        public CipAttribute GetCipAttribute(CipInstance cipInstance, ushort attributeNumber) {

            var attribute = new CipAttribute {
                Impl = this.NativeGetCipAttribute(cipInstance.Impl, attributeNumber)
            };

            return attribute;
        }

        /// <summary>Gets the registered CIP class with the given class ID.</summary>
        public CIPClass GetCipClass(ushort classId) {

            var cipclass = new CIPClass { Impl = this.NativeGetCipClass(classId) };

            return cipclass;
        }

        /// <summary>Gets the instance with the given number from a CIP class, or null if it does not exist.</summary>
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

        /// <summary>Toggle the 32-bit Run/Idle header on O→T (output) Class-1 data.
        /// <b>Must be true</b> when talking to Allen-Bradley ControlLogix/CompactLogix
        /// scanners — they always prepend a Run/Idle header. False for most other
        /// scanner brands (HMS Anybus, Codesys). Default: false.
        /// <para>
        /// Symptom of wrong setting: the first 4 bytes of your output assembly
        /// oscillate between 0x00000000 and 0x00000001 every cycle instead of holding
        /// real scanner-written data.
        /// </para></summary>
        public void EnableHeaderO2T(bool on) => this.NativeRunIdleHeaderSetO2T(on);

        /// <summary>Toggle the 32-bit Run/Idle header on T→O (input) Class-1 data.
        /// Less commonly required than O2T; defaults to false. Enable if your scanner's
        /// configuration expects it.</summary>
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
