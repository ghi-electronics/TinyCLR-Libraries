// Copyright (c) 2020 Rossmann Engineering
// Modified by GHI Electronics, LLC
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Network;


namespace GHIElectronics.TinyCLR.EthernetIP.Scanner
{
    /// <summary>
    /// Runs the device as an EtherNet/IP <b>Scanner</b> (the client/originator side —
    /// the role that talks to PLCs, motor drives, or other EIP adapters). Pure C#
    /// implementation, no native interop.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Property-bag API</b>: configure the connection by setting properties
    /// (<see cref="IPAddress"/>, <see cref="O_T_InstanceID"/>, <see cref="T_O_InstanceID"/>,
    /// <see cref="RequestedPacketRate_O_T"/>, <see cref="O_T_RealTimeFormat"/>, etc.)
    /// <i>before</i> calling <see cref="ForwardOpen()"/>. Changing properties after
    /// the connection is up has no effect on the live session.
    /// </para>
    /// <para>
    /// <b>Typical flow</b> (full example in <c>README.md</c> and
    /// <c>Test\TinyCLRApplication_EthernetIP\Program.cs</c>):
    /// <code>
    /// using (var scanner = new ScannerController()) {
    ///     var devices = scanner.ListIdentity(networkController, TimeSpan.FromSeconds(2));
    ///     scanner.IPAddress = "192.168.1.100";
    ///     scanner.O_T_InstanceID = 150; scanner.T_O_InstanceID = 100;
    ///     scanner.O_T_IOData = new byte[32]; scanner.T_O_IOData = new byte[32];
    ///     scanner.ImplicitDataReceived += (s, snapshot) =&gt; { /* use snapshot */ };
    ///     scanner.RegisterSession();
    ///     scanner.ForwardOpen();
    ///     while (true) Thread.Sleep(10);   // implicit I/O runs on background threads
    /// }   // Dispose() runs ForwardClose + UnRegisterSession automatically
    /// </code>
    /// </para>
    /// <para>
    /// <b>Thread safety</b>: <see cref="T_O_IOData"/> is written by the receive thread
    /// while your application reads it (torn-read risk). New code should subscribe to
    /// <see cref="ImplicitDataReceived"/> instead, which delivers a race-free byte[]
    /// snapshot. <see cref="T_O_IOData"/> is kept for backwards compatibility.
    /// </para>
    /// </remarks>
    public class ScannerController : IDisposable
    {
        TcpClient client;
        NetworkStream stream;
        uint sessionHandle;
        uint connectionID_O_T;
        uint connectionID_T_O;
        uint multicastAddress;
        ushort connectionSerialNumber;
        // One Random per controller. Upstream EEIP.NET (and the original port) called
        // `new Random()` three times back-to-back in ForwardOpen, which on platforms
        // that seed Random from a low-resolution tick counter produced identical seeds
        // and therefore identical "random" connection IDs and serial numbers.
        readonly Random rng = new Random();
		const int BUFFER_SIZE = 1024;
        /// <summary>
        /// TCP-Port of the Server
        /// </summary>
        public ushort TCPPort { get; set; } = 0xAF12;
        /// <summary>
        /// UDP-Port of the IO-Adapter - Standard is 0xAF12
        /// </summary>
        public ushort TargetUDPPort { get; set; } = 0x08AE;
        /// <summary>
        /// UDP-Port of the Scanner - Standard is 0xAF12
        /// </summary>
        public ushort OriginatorUDPPort { get; set; } = 0x08AE;
        /// <summary>
        /// IPAddress of the Ethernet/IP Device
        /// </summary>
        /// 
        public string IPAddress { get; set; } = "172.0.0.1";
        /// <summary>
        /// Requested Packet Rate (RPI) in Microseconds Originator -> Target for Implicit-Messaging (Default 0x7A120 -> 500ms)
        /// </summary>
        public uint RequestedPacketRate_O_T { get; set; } = 0x7A120;      //500ms
        /// <summary>
        /// Requested Packet Rate (RPI) in Microseconds Target -> Originator for Implicit-Messaging (Default 0x7A120 -> 500ms)
        /// </summary>
        public uint RequestedPacketRate_T_O { get; set; } = 0x7A120;      //500ms
        /// <summary>
        /// "1" Indicates that multiple connections are allowed Originator -> Target for Implicit-Messaging (Default: TRUE) 
        /// </summary>
        public bool O_T_OwnerRedundant { get; set; } = true;                //For Forward Open
        /// <summary>
        /// "1" Indicates that multiple connections are allowed Target -> Originator for Implicit-Messaging (Default: TRUE) 
        /// </summary>
        public bool T_O_OwnerRedundant { get; set; } = true;                //For Forward Open
        /// <summary>
        /// With a fixed size connection, the amount of data shall be the size of specified in the "Connection Size" Parameter.
        /// With a variable size, the amount of data could be up to the size specified in the "Connection Size" Parameter
        /// Originator -> Target for Implicit Messaging (Default: True (Variable length))
        /// </summary>
        public bool O_T_VariableLength { get; set; } = true;                //For Forward Open
        /// <summary>
        /// With a fixed size connection, the amount of data shall be the size of specified in the "Connection Size" Parameter.
        /// With a variable size, the amount of data could be up to the size specified in the "Connection Size" Parameter
        /// Target -> Originator for Implicit Messaging (Default: True (Variable length))
        /// </summary>
        public bool T_O_VariableLength { get; set; } = true;                //For Forward Open
        /// <summary>
        /// The maximum size in bytes (only pure data without sequence count and 32-Bit Real Time Header (if present)) from Originator -> Target for Implicit Messaging (Default: 505)
        /// </summary>
        public ushort O_T_Length {
            get  {
                if (this.O_T_IOData == null)
                    return 0;

                return (ushort)this.O_T_IOData.Length;
            }
        }                  //For Forward Open - Max 505
        /// <summary>
        /// The maximum size in bytes (only pure data woithout sequence count and 32-Bit Real Time Header (if present)) from Target -> Originator for Implicit Messaging (Default: 505)
        /// </summary>
        public ushort T_O_Length {
            get {
                if (this.T_O_IOData == null)
                    return 0;
                return (ushort)this.T_O_IOData.Length;
            }
        }                 //For Forward Open - Max 505
        /// <summary>
        /// Connection Type Originator -> Target for Implicit Messaging (Default: ConnectionType.Point_to_Point)
        /// </summary>
        public ConnectionType O_T_ConnectionType { get; set; } = ConnectionType.Point_to_Point;
        /// <summary>
        /// Connection Type Target -> Originator for Implicit Messaging (Default: ConnectionType.Multicast)
        /// </summary>
        public ConnectionType T_O_ConnectionType { get; set; } = ConnectionType.Multicast;
        /// <summary>
        /// Priority Originator -> Target for Implicit Messaging (Default: Priority.Scheduled)
        /// Could be: Priority.Scheduled; Priority.High; Priority.Low; Priority.Urgent
        /// </summary>
        public Priority O_T_Priority { get; set; } = Priority.Scheduled;
        /// <summary>
        /// Priority Target -> Originator for Implicit Messaging (Default: Priority.Scheduled)
        /// Could be: Priority.Scheduled; Priority.High; Priority.Low; Priority.Urgent
        /// </summary>
        public Priority T_O_Priority { get; set; } = Priority.Scheduled;
        /// <summary>
        /// Class Assembly (Consuming IO-Path - Outputs) Originator -> Target for Implicit Messaging 
        /// </summary>
        public byte O_T_InstanceID { get; set; } 
        /// <summary>
        /// Class Assembly (Producing IO-Path - Inputs) Target -> Originator for Implicit Messaging 
        /// </summary>
        public byte T_O_InstanceID { get; set; } 
        /// <summary>
        /// Provides Access to the Class 1 Real-Time IO-Data Originator -> Target for Implicit Messaging    
        /// </summary>
        public byte[] O_T_IOData { get; set; }   //Class 1 Real-Time IO-Data O->T   
        /// <summary>
        /// Provides Access to the Class 1 Real-Time IO-Data Target -> Originator for Implicit Messaging
        /// </summary>
        public byte[] T_O_IOData { get; set; }    //Class 1 Real-Time IO-Data T->O  
        /// <summary>
        /// Used Real-Time Format Originator -> Target for Implicit Messaging (Default: RealTimeFormat.Header32Bit)
        /// Possible Values: RealTimeFormat.Header32Bit; RealTimeFormat.Heartbeat; RealTimeFormat.ZeroLength; RealTimeFormat.Modeless
        /// </summary>
        public RealTimeFormat O_T_RealTimeFormat { get; set; } = RealTimeFormat.Header32Bit;
        /// <summary>
        /// Used Real-Time Format Target -> Originator for Implicit Messaging (Default: RealTimeFormat.Modeless)
        /// Possible Values: RealTimeFormat.Header32Bit; RealTimeFormat.Heartbeat; RealTimeFormat.ZeroLength; RealTimeFormat.Modeless
        /// </summary>
        public RealTimeFormat T_O_RealTimeFormat { get; set; } = RealTimeFormat.Modeless;
        /// <summary>
        /// AssemblyObject for the Configuration Path in case of Implicit Messaging (Standard: 0x04)
        /// </summary>
        public byte AssemblyObjectClass { get; set; } = 0x04;
        /// <summary>
        /// ConfigurationAssemblyInstanceID is the InstanceID of the configuration Instance in the Assembly Object Class (Standard: 0x01)
        /// </summary>
        public byte ConfigurationAssemblyInstanceID { get; set; } = 0x01;
        /// <summary>
        /// ConfigurationAssemblyInstanceID is the InstanceID of the configuration Instance in the Assembly Object Class (Standard: 0x01)
        /// </summary>
        public byte[] ConfigurationAssembly_Data { get; set; } 
        /// <summary>
        /// ConfigurationAssemblyDataLength max 500
        /// </summary>
        public ushort ConfigurationAssemblyData_Length {
            get {
                if (this.ConfigurationAssembly_Data == null)
                    return 0;

                if (this.ConfigurationAssembly_Data.Length > 500)
                    throw new Exception("Configuration max 500");

                return (ushort)this.ConfigurationAssembly_Data.Length;
            }
        }
        /// <summary>
        /// ConfigurationAssemblyDataLength max 500
        /// </summary>
        public bool WriteConfiguration { get; set; } = false;
        
        /// <summary>
        /// Returns the Date and Time when the last Implicit Message has been received fŕom The Target Device
        /// Could be used to determine a Timeout.
        /// <para>
        /// NOTE: this is a wall-clock timestamp (DateTime.Now) and breaks if the system
        /// clock jumps (NTP sync, DST, manual set). For timeout detection that must
        /// survive clock changes, read <see cref="LastReceivedImplicitMessageTickCount"/>
        /// instead and compare against <c>Environment.TickCount</c>.
        /// </para>
        /// </summary>
        public DateTime LastReceivedImplicitMessage { get; set; }

        /// <summary>
        /// Monotonic (clock-jump-safe) counterpart of <see cref="LastReceivedImplicitMessage"/>.
        /// Stores <c>Environment.TickCount</c> at the moment the last Class-1 packet was
        /// received from the target. Compare against a fresh <c>Environment.TickCount</c>
        /// read to compute elapsed-since-last-implicit, e.g.
        /// <c>(Environment.TickCount - scanner.LastReceivedImplicitMessageTickCount) > 5000</c>
        /// for a 5-second watchdog. Unlike <see cref="LastReceivedImplicitMessage"/>, this
        /// value is not affected by NTP sync, DST changes, or manual clock adjustment.
        /// </summary>
        public int LastReceivedImplicitMessageTickCount { get; private set; }

        // ===========================================================================
        // Phase 3.5 — Event surface
        //
        // Replaces the previous poll-only model where users had to watch
        // LastReceivedImplicitMessage and T_O_IOData directly. The events fire on
        // the same thread that read or processed the underlying network packet —
        // handlers should be quick and non-blocking. To do heavy work, marshal to
        // a worker thread.
        //
        // T_O_IOData remains public+mutable for now (backwards compat). The
        // ImplicitDataReceived event delivers a fresh snapshot byte[] each time,
        // so user code can avoid the torn-read race entirely by reading the event
        // argument instead of the field. T_O_IOData becomes [Obsolete] in a future
        // phase.
        // ===========================================================================

        // Stand-in for System.EventHandler — TinyCLR's mscorlib doesn't define
        // the non-generic delegate, so the Scanner declares its own with the
        // same signature.
        /// <summary>Handler for scanner lifecycle events (connection established/lost, RPI violated).</summary>
        public delegate void EipEventHandler(object sender, EventArgs e);

        /// <summary>Fired once after a successful ForwardOpen / LargeForwardOpen.</summary>
        public event EipEventHandler ConnectionEstablished;

        /// <summary>
        /// Fired when the implicit producer (sendUDP) repeatedly fails to send, or when
        /// the target stops producing for longer than the RPI watchdog. Not fired on a
        /// user-initiated ForwardClose / Dispose.
        /// </summary>
        public event EipEventHandler ConnectionLost;

        /// <summary>
        /// Fired on every Class-1 packet received from the target. The byte[] argument
        /// is a freshly-allocated snapshot of the payload — safe to retain, safe to
        /// read off-thread. T_O_IOData is also updated, but the snapshot is the
        /// race-free way to consume implicit input.
        /// </summary>
        public event ImplicitDataReceivedHandler ImplicitDataReceived;
        /// <summary>Handler for <see cref="ImplicitDataReceived"/>; receives a race-free snapshot of each Class-1 payload.</summary>
        public delegate void ImplicitDataReceivedHandler(ScannerController scanner, byte[] snapshot);

        /// <summary>
        /// Fired when an implicit packet arrives later than the negotiated RPI tolerance
        /// (currently fires when the inter-arrival gap exceeds 4 * RPI). Diagnostic only;
        /// the connection is not automatically closed.
        /// </summary>
        public event EipEventHandler RpiViolated;

        // Track inter-arrival timing for RpiViolated detection.
        private int lastImplicitTickCount;

        // Substitute for Environment.TickCount (missing in TinyCLR mscorlib).
        // Truncating DateTime.UtcNow.Ticks/TicksPerMillisecond to int matches
        // .NET's Environment.TickCount semantics — it wraps every ~24.8 days
        // and unsigned-delta arithmetic on the wrapped values stays correct.
        private static int MonotonicMs() => (int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond);

        private void ReceiveCallback(UdpState state)
        {
            lock (this) {
                

                var u = (UdpClient)(state).u;
                
                IPEndPoint recEndpoint = null;

                var receiveBytes = u.Receive(ref recEndpoint);

                if (receiveBytes.Length > 0) {


                    var command = receiveBytes[0] | (receiveBytes[1] << 8);
                    if (command == 0x63) {

                        //return Encapsulation.CIPIdentityItem.GetCIPIdentityItem(24, receiveBytes);

                        this.returnList.Add(Encapsulation.CIPIdentityItem.GetCIPIdentityItem(24, receiveBytes));
                    }
                }


            }

        }
        /// <summary>Holds the UDP client and endpoint used by an asynchronous receive operation.</summary>
        public class UdpState
        {
            /// <summary>The remote endpoint associated with the UDP operation.</summary>
            public System.Net.IPEndPoint e;
            /// <summary>The UDP client used to send or receive.</summary>
            public UdpClient u;

        }

        ArrayList returnList;

        /// <summary>
        /// Broadcasts an EIP List Identity command and collects responses from every
        /// EtherNet/IP device on the local subnet during the timeout window.
        /// </summary>
        /// <param name="networkController">The TinyCLR <c>NetworkController</c> whose
        /// IP/mask is used to compute the broadcast address.</param>
        /// <param name="timeout">How long to listen for responses. Spec says devices
        /// reply within a random 0–2 s delay window, so use ≥ 2 s for reliable discovery.
        /// Pass <c>TimeSpan.Zero</c> to wait indefinitely (not recommended).</param>
        /// <returns>One <see cref="Encapsulation.CIPIdentityItem"/> per responding device,
        /// or <c>null</c> if no device responded.</returns>
        /// <remarks>Current implementation broadcasts to the <i>directed</i> subnet
        /// address (e.g. 192.168.1.255 for a /24). Devices behind a router won't see
        /// the request unless directed-broadcast forwarding is enabled.</remarks>
        public Encapsulation.CIPIdentityItem[] ListIdentity(NetworkController networkController, TimeSpan timeout) {

            this.returnList = new ArrayList();

            this.returnList.Clear();

            var mask = networkController.GetIPProperties().SubnetMask;
            var address = networkController.GetIPProperties().Address;

            var multicastAddress = (address.GetAddressBytes()[0] | (~(mask.GetAddressBytes()[0])) & 0xFF).ToString() + "." + (address.GetAddressBytes()[1] | (~(mask.GetAddressBytes()[1])) & 0xFF).ToString() + "." + (address.GetAddressBytes()[2] | (~(mask.GetAddressBytes()[2])) & 0xFF).ToString() + "." + (address.GetAddressBytes()[3] | (~(mask.GetAddressBytes()[3])) & 0xFF).ToString();

            var sendData = new byte[24];
            sendData[0] = 0x63;               //Command for "ListIdentity"
            var udpClient = new System.Net.Sockets.UdpClient();

            var endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(multicastAddress), 44818);
      
            udpClient.Send(sendData, sendData.Length, endPoint);

            var s = new UdpState {
                e = endPoint,
                u = udpClient
            };

          
            // List Identity is a broadcast — multiple devices respond, each at a
            // random delay (per CIP spec 2 s default max). Loop over the full
            // timeout window and collect every responder, not just the first.
            var expired = DateTime.Now + timeout;
            while (true) {
                if (udpClient.Available > 0) {
                    this.ReceiveCallback(s);
                }
                else if (timeout != TimeSpan.Zero && DateTime.Now >= expired) {
                    break;
                }
                else {
                    Thread.Sleep(1);
                }
            }

            if (this.returnList.Count > 0) {
                var devices = new Encapsulation.CIPIdentityItem[this.returnList.Count];
                for ( var i = 0; i < this.returnList.Count;i++) {
                    devices[i] = (Encapsulation.CIPIdentityItem)this.returnList.ToArray()[i];
                }
                return devices;
            }

            return null;






            //foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces()) {
            //    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet) {

            //        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses) {
            //            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
            //                System.Net.IPAddress mask = ip.IPv4Mask;
            //                System.Net.IPAddress address = ip.Address;

            //                String multicastAddress = (address.GetAddressbytes()[0] | (~(mask.GetAddressbytes()[0])) & 0xFF).ToString() + "." + (address.GetAddressbytes()[1] | (~(mask.GetAddressbytes()[1])) & 0xFF).ToString() + "." + (address.GetAddressbytes()[2] | (~(mask.GetAddressbytes()[2])) & 0xFF).ToString() + "." + (address.GetAddressbytes()[3] | (~(mask.GetAddressbytes()[3])) & 0xFF).ToString();

            //                byte[] sendData = new byte[24];
            //                sendData[0] = 0x63;               //Command for "ListIdentity"
            //                System.Net.Sockets.UdpClient udpClient = new System.Net.Sockets.UdpClient();
            //                System.Net.IPEndPoint endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(multicastAddress), 44818);
            //                udpClient.Send(sendData, sendData.Length, endPoint);

            //                UdpState s = new UdpState();
            //                s.e = endPoint;
            //                s.u = udpClient;

            //                var asyncResult = udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), s);

            //                System.Threading.Thread.Sleep(1000);
            //            }
            //        }
            //    }
            //}
            //return returnList;
        }

        /// <summary>
        /// Opens a TCP connection to the target on port 44818 (0xAF12) and sends an EIP
        /// RegisterSession command to establish an encapsulation session. Required before
        /// any explicit-messaging service (GetAttributeSingle, ForwardOpen, etc.) can be sent.
        /// </summary>
        /// <param name="address">Target IP as a packed 32-bit value: top byte is the
        /// first IPv4 octet (e.g. 192.168.1.1 = 0xC0A80101). Use the string overload
        /// if you have a dotted-quad string.</param>
        /// <param name="port">TCP port — typically 0xAF12 (44818, EIP standard). Pass
        /// a non-standard port only if the target listens elsewhere.</param>
        /// <returns>The 32-bit session handle assigned by the target. Stored internally;
        /// you typically don't need to inspect it.</returns>
        /// <remarks><b>This call blocks</b> until the TCP connect succeeds, fails, or the
        /// OS-level timeout fires (typically 60–120 s for unreachable hosts). There is
        /// no per-call timeout knob — if you might point at unreachable IPs, run this
        /// on a thread you can abandon.</remarks>
        public uint RegisterSession(uint address, ushort port)
        {
            if (this.sessionHandle != 0)
                return this.sessionHandle;
            var encapsulation = new Encapsulation();
            encapsulation.Command = Encapsulation.CommandsEnum.RegisterSession;
            encapsulation.Length = 4;
            encapsulation.CommandSpecificData.Add(1);       //Protocol version (should be set to 1)
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);       //Session options shall be set to "0"
            encapsulation.CommandSpecificData.Add(0);


            var ipAddress = Encapsulation.CIPIdentityItem.GetIPAddress(address);
            this.IPAddress = ipAddress;
            this.client = new TcpClient(ipAddress, port);
            this.stream = this.client.GetStream();

            this.stream.Write(encapsulation.Tobytes(), 0, encapsulation.Tobytes().Length);
            var data = new byte[256];

            var bytes = this.stream.Read(data, 0, data.Length);

            var returnvalue = (uint)data[4] + (((uint)data[5]) << 8) + (((uint)data[6]) << 16) + (((uint)data[7]) << 24);
            this.sessionHandle = returnvalue;
            return returnvalue;
        }

        /// <summary>
        /// Sends an EIP UnregisterSession command to gracefully close the encapsulation
        /// session, then closes the TCP connection. Idempotent — safe to call even if
        /// the target has already dropped. <see cref="Dispose"/> calls this for you.
        /// </summary>
        public void UnRegisterSession()
        {
            var encapsulation = new Encapsulation();
            encapsulation.Command = Encapsulation.CommandsEnum.UnRegisterSession;
            encapsulation.Length = 0;
            encapsulation.SessionHandle = this.sessionHandle;
 
            // Best-effort courtesy notification. If the target already tore down the
            // TCP session (common when it disposes first — its sockets close right
            // after the I/O exchange), skip the write entirely. Otherwise Socket.Send
            // throws SocketException/IOException; the catch handles it, but it still
            // surfaces as noisy first-chance exceptions. A peer-closed socket reads as
            // Poll(SelectRead)==true with zero bytes available.
            if (this.IsSessionSocketAlive())
            {
                try
                {
                    var bytes = encapsulation.Tobytes();
                    this.stream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception)
                {
                    // Peer dropped between the poll and the write — closing anyway.
                }
            }

            try { this.client.Close(); } catch { }
            try { this.stream.Close(); } catch { }
            this.sessionHandle = 0;
        }

        // True if the encapsulation TCP socket still looks usable for a final write.
        // Mirrors the closed-socket check used elsewhere in the stack (e.g. Modbus):
        // Poll(SelectRead) is true when the socket is readable OR closed; a closed peer
        // has zero bytes available, so that combination means "gone — don't write".
        private bool IsSessionSocketAlive()
        {
            try
            {
                var socket = this.client?.Client;
                if (socket == null || !socket.Connected) return false;
                return !(socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch
            {
                return false;
            }
        }

        private bool disposed;

        /// <summary>Tears down the scanner cleanly: closes the Class-1 connection
        /// (ForwardClose), unregisters the encapsulation session, closes all sockets.
        /// Each sub-step swallows its own exceptions so Dispose itself never throws
        /// (per the <see cref="IDisposable"/> contract). Idempotent and safe to call
        /// from a <c>using</c> block on any code path including before <see cref="ForwardOpen()"/>.</summary>
        public void Dispose()
        {
            if (this.disposed) return;
            this.disposed = true;

            // 1. If a Class-1 connection is up, close it. ForwardClose signals stopUDP,
            // sleeps long enough for the producer thread to exit, then sends a
            // ForwardClose to the target. We only call it if ForwardOpen actually
            // ran (udpClientReceive set then).
            if (this.udpClientReceive != null && !this.udpClientReceiveClosed) {
                try { this.ForwardClose(); }
                catch { /* peer may have already dropped — best-effort teardown */ }
            }

            // 2. If an encapsulation session is registered, terminate it.
            if (this.sessionHandle != 0) {
                try { this.UnRegisterSession(); }
                catch { /* see above */ }
            }

            // 3. Belt-and-suspenders: explicitly close anything still open. Both
            // ForwardClose and UnRegisterSession close their own sockets, but if
            // they threw before getting there, these calls plug the leak.
            try { this.udpClientReceive?.Close(); } catch { }
            try { this.stream?.Close(); }          catch { }
            try { this.client?.Close(); }          catch { }
        }

        /// <summary>Opens a Class-1 (implicit / cyclic I/O) connection to the target
        /// using regular Forward Open (service 0x54). Configure all connection-related
        /// properties (<see cref="IPAddress"/>, <see cref="O_T_InstanceID"/>,
        /// <see cref="T_O_InstanceID"/>, <see cref="RequestedPacketRate_O_T"/>,
        /// <see cref="O_T_RealTimeFormat"/>, etc.) before calling.</summary>
        /// <remarks>Spawns two background threads — <c>sendUDP</c> producing O→T data
        /// at the negotiated RPI, and a receive loop that fires
        /// <see cref="ImplicitDataReceived"/> per T→O packet. Use Large Forward Open
        /// (the overload below) for connection sizes > ~500 bytes.</remarks>
        public void ForwardOpen() => this.ForwardOpen(false);

        System.Net.Sockets.UdpClient udpClientReceive;
        bool udpClientReceiveClosed = false;

        /// <summary>Opens a Class-1 implicit connection, choosing between regular Forward
        /// Open (service 0x54) and Large Forward Open (service 0x5B) based on the
        /// <paramref name="largeForwardOpen"/> flag. Large form is required when either
        /// direction's payload exceeds ~500 bytes.</summary>
        /// <param name="largeForwardOpen">true → use Large Forward Open (32-bit
        /// connection parameters, allows up to ~65 KB per direction);
        /// false → use regular Forward Open.</param>
        /// <exception cref="System.Exception">Connection size exceeds buffer, missing
        /// configuration, etc. See exception message.</exception>
        /// <exception cref="CIPException">Target returned a CIP error status. Use
        /// <c>data[42]</c> + extended status to diagnose.</exception>
        public void ForwardOpen(bool largeForwardOpen)
        {
            if (this.O_T_Length > BUFFER_SIZE) {
                throw new Exception(string.Format("O_T_Length is larger than {0}", BUFFER_SIZE));
            }

            if (this.T_O_Length > BUFFER_SIZE) {
                throw new Exception(string.Format("T_O_Length is larger than {0}", BUFFER_SIZE));
            }

            if (!largeForwardOpen && (this.O_T_Length > 511 - 2  || this.T_O_Length > 511 - 6)) {
                throw new Exception(string.Format("Data too larger for ForwardOpen. Try to use Large ForwardOpen."));
            }

            if (this.ConfigurationAssemblyData_Length > 500) {
                throw new Exception(string.Format("Max ConfigurationAssemblyDataLength is {0}", this.ConfigurationAssembly_Data.Length));
            }

            this.udpClientReceiveClosed = false;
            ushort o_t_headerOffset = 2;                    
            if (this.O_T_RealTimeFormat == RealTimeFormat.Header32Bit)
                o_t_headerOffset = 6;
            if (this.O_T_RealTimeFormat == RealTimeFormat.Heartbeat)
                o_t_headerOffset = 0;

            ushort t_o_headerOffset = 2;                    
            if (this.T_O_RealTimeFormat == RealTimeFormat.Header32Bit)
                t_o_headerOffset = 6;
            if (this.T_O_RealTimeFormat == RealTimeFormat.Heartbeat)
                t_o_headerOffset = 0;

            var lengthOffset = (5 + (this.O_T_ConnectionType == ConnectionType.Null ? 0 : 2) + (this.T_O_ConnectionType == ConnectionType.Null ? 0 : 2));

            var encapsulation = new Encapsulation();
            encapsulation.SessionHandle = this.sessionHandle;
            encapsulation.Command = Encapsulation.CommandsEnum.SendRRData;
            //!!!!!!-----Length Field at the end!!!!!!!!!!!!!

            //---------------Interface Handle CIP
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            //----------------Interface Handle CIP

            //----------------Timeout
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            //----------------Timeout

            //Common Packet Format (Table 2-6.1)
            var commonPacketFormat = new Encapsulation.CommonPacketFormat();
            commonPacketFormat.ItemCount = 0x02;

            commonPacketFormat.AddressItem = 0x0000;        //NULL (used for UCMM Messages)
            commonPacketFormat.AddressLength = 0x0000;


            commonPacketFormat.DataItem = 0xB2;

            if (this.ConfigurationAssemblyData_Length > 0)
                commonPacketFormat.DataLength = (ushort)(41 + (ushort)lengthOffset + (this.ConfigurationAssemblyData_Length + 2));
            else
                commonPacketFormat.DataLength = (ushort)(41 + (ushort)lengthOffset); 

            if (largeForwardOpen)
                commonPacketFormat.DataLength = (ushort)(commonPacketFormat.DataLength + 4);



            //----------------CIP Command "Forward Open" (Service Code 0x54)
            if (!largeForwardOpen)
                commonPacketFormat.Data.Add(0x54);
            //----------------CIP Command "Forward Open"  (Service Code 0x54)

            //----------------CIP Command "large Forward Open" (Service Code 0x5B)
            else
                commonPacketFormat.Data.Add(0x5B);
            //----------------CIP Command "large Forward Open"  (Service Code 0x5B)

            //----------------Requested Path size
            commonPacketFormat.Data.Add(2);
            //----------------Requested Path size

            //----------------Path segment for Class ID
            commonPacketFormat.Data.Add(0x20);
            commonPacketFormat.Data.Add((byte)6);
            //----------------Path segment for Class ID

            //----------------Path segment for Instance ID
            commonPacketFormat.Data.Add(0x24);
            commonPacketFormat.Data.Add((byte)1);
            //----------------Path segment for Instace ID

            //----------------Priority and Time/Tick - Table 3-5.16 (Vol. 1)
            commonPacketFormat.Data.Add(0x03);
            //----------------Priority and Time/Tick

            //----------------Timeout Ticks - Table 3-5.16 (Vol. 1)
            commonPacketFormat.Data.Add(0xfa);
            //----------------Timeout Ticks

            this.connectionID_O_T = (uint)(this.rng.Next(0xfffffff));
            this.connectionID_T_O = (uint)(this.rng.Next(0xfffffff) + 1);
            commonPacketFormat.Data.Add((byte)this.connectionID_O_T);
            commonPacketFormat.Data.Add((byte)(this.connectionID_O_T >> 8));
            commonPacketFormat.Data.Add((byte)(this.connectionID_O_T >> 16));
            commonPacketFormat.Data.Add((byte)(this.connectionID_O_T >> 24));


            commonPacketFormat.Data.Add((byte)this.connectionID_T_O);
            commonPacketFormat.Data.Add((byte)(this.connectionID_T_O >> 8));
            commonPacketFormat.Data.Add((byte)(this.connectionID_T_O >> 16));
            commonPacketFormat.Data.Add((byte)(this.connectionID_T_O >> 24));

            this.connectionSerialNumber = (ushort)(this.rng.Next(0xFFFF) + 2);
            commonPacketFormat.Data.Add((byte)this.connectionSerialNumber);
            commonPacketFormat.Data.Add((byte)(this.connectionSerialNumber >> 8));

            //----------------Originator Vendor ID
            commonPacketFormat.Data.Add(0xFF);
            commonPacketFormat.Data.Add(0);
            //----------------Originaator Vendor ID

            //----------------Originator Serial Number
            commonPacketFormat.Data.Add(0xFF);
            commonPacketFormat.Data.Add(0xFF);
            commonPacketFormat.Data.Add(0xFF);
            commonPacketFormat.Data.Add(0xFF);
            //----------------Originator Serial Number

            //----------------Timeout Multiplier
            commonPacketFormat.Data.Add(3);
            //----------------Timeout Multiplier

            //----------------Reserved
            commonPacketFormat.Data.Add(0);
            commonPacketFormat.Data.Add(0);
            commonPacketFormat.Data.Add(0);
            //----------------Reserved

            //----------------Requested Packet Rate O->T in Microseconds
            commonPacketFormat.Data.Add((byte)this.RequestedPacketRate_O_T);
            commonPacketFormat.Data.Add((byte)(this.RequestedPacketRate_O_T >> 8));
            commonPacketFormat.Data.Add((byte)(this.RequestedPacketRate_O_T >> 16));
            commonPacketFormat.Data.Add((byte)(this.RequestedPacketRate_O_T >> 24));
            //----------------Requested Packet Rate O->T in Microseconds

            //----------------O->T Network Connection Parameters
            var redundantOwner = (bool)this.O_T_OwnerRedundant == false ? 0 : 1;
            var connectionType = (byte)this.O_T_ConnectionType; //1=Multicast, 2=P2P
            var priority = (byte)this.O_T_Priority;         //00=low; 01=High; 10=Scheduled; 11=Urgent
            var variableLength = this.O_T_VariableLength == false ? 0 : 1;       //0=fixed; 1=variable
            var connectionSize = (ushort)(this.O_T_Length + o_t_headerOffset);      //The maximum size in bytes og the data for each direction (were applicable) of the connection. For a variable -> maximum
            uint NetworkConnectionParameters = (ushort)((ushort)(connectionSize & 0x1FF) | (((ushort)(variableLength)) << 9) | ((priority & 0x03) << 10) | ((connectionType & 0x03) << 13) | (((ushort)(redundantOwner)) << 15));
            if (largeForwardOpen)
                NetworkConnectionParameters = (uint)((uint)(connectionSize & 0xFFFF) | (((uint)(variableLength)) << 25) | (uint)((priority & 0x03) << 26) | (uint)((connectionType & 0x03) << 29) | (((uint)(redundantOwner)) << 31));
            commonPacketFormat.Data.Add((byte)NetworkConnectionParameters);
            commonPacketFormat.Data.Add((byte)(NetworkConnectionParameters >> 8));
            if (largeForwardOpen) {
                commonPacketFormat.Data.Add((byte)(NetworkConnectionParameters >> 16));
                commonPacketFormat.Data.Add((byte)(NetworkConnectionParameters >> 24));
            }
            //----------------O->T Network Connection Parameters

            //----------------Requested Packet Rate T->O in Microseconds
            commonPacketFormat.Data.Add((byte)this.RequestedPacketRate_T_O);
            commonPacketFormat.Data.Add((byte)(this.RequestedPacketRate_T_O >> 8));
            commonPacketFormat.Data.Add((byte)(this.RequestedPacketRate_T_O >> 16));
            commonPacketFormat.Data.Add((byte)(this.RequestedPacketRate_T_O >> 24));
            //----------------Requested Packet Rate T->O in Microseconds

            //----------------T->O Network Connection Parameters


            redundantOwner = (bool)this.T_O_OwnerRedundant == false ? 0 : 1;
            connectionType = (byte)this.T_O_ConnectionType; //1=Multicast, 2=P2P
            priority = (byte)this.T_O_Priority;
            variableLength = this.T_O_VariableLength == false ? 0 : 1;
            connectionSize = (ushort)(this.T_O_Length + t_o_headerOffset);
            NetworkConnectionParameters = (ushort)((ushort)(connectionSize & 0x1FF) | (((ushort)(variableLength)) << 9) | ((priority & 0x03) << 10) | ((connectionType & 0x03) << 13) | (((ushort)(redundantOwner)) << 15));
            if (largeForwardOpen)
                NetworkConnectionParameters = (uint)((uint)(connectionSize & 0xFFFF) | (((uint)(variableLength)) << 25) | (uint)((priority & 0x03) << 26) | (uint)((connectionType & 0x03) << 29) | (((uint)(redundantOwner)) << 31));
            commonPacketFormat.Data.Add((byte)NetworkConnectionParameters);
            commonPacketFormat.Data.Add((byte)(NetworkConnectionParameters >> 8));
            if (largeForwardOpen) {
                commonPacketFormat.Data.Add((byte)(NetworkConnectionParameters >> 16));
                commonPacketFormat.Data.Add((byte)(NetworkConnectionParameters >> 24));
            }
            //----------------T->O Network Connection Parameters

            //----------------Transport Type/Trigger
            commonPacketFormat.Data.Add(0x01);
            //X------- = 0= Client; 1= Server
            //-XXX---- = Production Trigger, 0 = Cyclic, 1 = CoS, 2 = Application Object
            //----XXXX = Transport class, 0 = Class 0, 1 = Class 1, 2 = Class 2, 3 = Class 3
            //----------------Transport Type Trigger
            //Connection Path size 
            //commonPacketFormat.Data.Add((byte)((0x2) + (O_T_ConnectionType == ConnectionType.Null ? 0 : 1) + (T_O_ConnectionType == ConnectionType.Null ? 0 : 1) ));

            var connectionPathSize = (byte)((0x2) + (O_T_ConnectionType == ConnectionType.Null ? 0 : 1) + (T_O_ConnectionType == ConnectionType.Null ? 0 : 1));
            
            if (this.ConfigurationAssemblyData_Length > 0)
            {
                connectionPathSize += (byte)((this.ConfigurationAssemblyData_Length + 2) / 2) ;    // +2 = below            
            }

            commonPacketFormat.Data.Add(connectionPathSize);

            //Verbindugspfad
            commonPacketFormat.Data.Add((byte)(0x20));
            commonPacketFormat.Data.Add((byte)(this.AssemblyObjectClass));
            commonPacketFormat.Data.Add((byte)(0x24));
            commonPacketFormat.Data.Add((byte)(this.ConfigurationAssemblyInstanceID));
            if (this.O_T_ConnectionType != ConnectionType.Null) {
                commonPacketFormat.Data.Add((byte)(0x2C));
                commonPacketFormat.Data.Add((byte)(this.O_T_InstanceID));
            }
            if (this.T_O_ConnectionType != ConnectionType.Null) {
                commonPacketFormat.Data.Add((byte)(0x2C));
                commonPacketFormat.Data.Add((byte)(this.T_O_InstanceID));
            }
            
            // GHI add config
            if (this.ConfigurationAssemblyData_Length > 0)
            {
                commonPacketFormat.Data.Add((byte)(0x80));
                commonPacketFormat.Data.Add((byte)(this.ConfigurationAssemblyData_Length / 2 + this.ConfigurationAssemblyData_Length % 2));

                for (var i = 0; i < this.ConfigurationAssemblyData_Length; i++)
                {
                    commonPacketFormat.Data.Add((byte)(this.ConfigurationAssembly_Data[i]));
                }
            }

            //AddSocket Addrress Item O->T

            commonPacketFormat.SocketaddrInfo_O_T = new Encapsulation.SocketAddress();
            commonPacketFormat.SocketaddrInfo_O_T.SIN_port = this.OriginatorUDPPort;
            commonPacketFormat.SocketaddrInfo_O_T.SIN_family = 2;
            if (this.O_T_ConnectionType == ConnectionType.Multicast) {
                var addressInbytes = System.Net.IPAddress.Parse(this.IPAddress).GetAddressBytes();
                var address = (uint)(addressInbytes[3] << 24 | addressInbytes[2] << 16 | addressInbytes[1] << 8 | addressInbytes[0]);

                var multicastResponseAddress = ScannerController.GetMulticastAddress(address);

                commonPacketFormat.SocketaddrInfo_O_T.SIN_Address = (multicastResponseAddress);

                this.multicastAddress = commonPacketFormat.SocketaddrInfo_O_T.SIN_Address;
            }
            else
                commonPacketFormat.SocketaddrInfo_O_T.SIN_Address = 0;

            encapsulation.Length = (ushort)(commonPacketFormat.Tobytes().Length + 6);


            var dataToWrite = new byte[encapsulation.Tobytes().Length + commonPacketFormat.Tobytes().Length];
            Array.Copy(encapsulation.Tobytes(), 0, dataToWrite, 0, encapsulation.Tobytes().Length);
            Array.Copy(commonPacketFormat.Tobytes(), 0, dataToWrite, encapsulation.Tobytes().Length, commonPacketFormat.Tobytes().Length);
            //encapsulation.tobytes();

            this.stream.Write(dataToWrite, 0, dataToWrite.Length);
            var data = new byte[BUFFER_SIZE + 64];
            // GHI changed
            data[42] = 0xFF; // set error 

            // wait a bit for data ready
            var to = 0;
            while (!this.stream.DataAvailable) {
                to++;
                Thread.Sleep(1);

                if (to >= 10)
                    break;
            }

            var bytes = this.stream.Read(data, 0, data.Length);

            //--------------------------BEGIN Error?
            if (data[42] != 0)      //Exception codes see "Table B-1.1 CIP General Status Codes"
            {
                if (data[42] == 0x1)
                    if (data[43] == 0)
                        throw new CIPException("Connection failure, General Status Code: " + data[42]);
                    else
                        throw new CIPException("Connection failure, General Status Code: " + data[42] + " Additional Status Code: " + ((data[45] << 8) | data[44]) + " " + ObjectLibrary.ConnectionManagerObject.GetExtendedStatus((uint)((data[45] << 8) | data[44])));
                else
                    throw new CIPException(GeneralStatusCodes.GetStatusCode(data[42]));
            }
            //--------------------------END Error?
            //Read the Network ID from the Reply (see 3-3.7.1.1)
            var itemCount = data[30] + (data[31] << 8);
            var lengthUnconectedDataItem = data[38] + (data[39] << 8);
            this.connectionID_O_T = data[44] + (uint)(data[45] << 8) + (uint)(data[46] << 16) + (uint)(data[47] << 24);
            this.connectionID_T_O = data[48] + (uint)(data[49] << 8) + (uint)(data[50] << 16) + (uint)(data[51] << 24);

            //Is a SocketInfoItem present?
            var numberOfCurrentItem = 0;
            Encapsulation.SocketAddress socketInfoItem;
            while (itemCount > 2) {
                var typeID = data[40 + lengthUnconectedDataItem + 20 * numberOfCurrentItem] + (data[40 + lengthUnconectedDataItem + 1 + 20 * numberOfCurrentItem] << 8);
                if (typeID == 0x8001) {
                    socketInfoItem = new Encapsulation.SocketAddress();
                    socketInfoItem.SIN_Address = (uint)(data[40 + lengthUnconectedDataItem + 8 + 20 * numberOfCurrentItem]) + (uint)(data[40 + lengthUnconectedDataItem + 9 + 20 * numberOfCurrentItem] << 8) + (uint)(data[40 + lengthUnconectedDataItem + 10 + 20 * numberOfCurrentItem] << 16) + (uint)(data[40 + lengthUnconectedDataItem + 11 + 20 * numberOfCurrentItem] << 24);
                    socketInfoItem.SIN_port = (ushort)((ushort)(data[40 + lengthUnconectedDataItem + 7 + 20 * numberOfCurrentItem]) + (ushort)(data[40 + lengthUnconectedDataItem + 6 + 20 * numberOfCurrentItem] << 8));
                    if (this.T_O_ConnectionType == ConnectionType.Multicast)
                        this.multicastAddress = socketInfoItem.SIN_Address;
                    this.TargetUDPPort = socketInfoItem.SIN_port;
                }
                numberOfCurrentItem++;
                itemCount--;
            }
            //Open UDP-Port



            var endPointReceive = new System.Net.IPEndPoint(System.Net.IPAddress.Any, this.OriginatorUDPPort);
            this.udpClientReceive = new System.Net.Sockets.UdpClient(endPointReceive);
            var s = new UdpState();
            s.e = endPointReceive;
            s.u = this.udpClientReceive;
            if (this.multicastAddress != 0) {
                var multicast = (new System.Net.IPAddress(this.multicastAddress));
                this.udpClientReceive.JoinMulticastGroup(multicast);

            }

            var sendThread = new System.Threading.Thread(this.sendUDP);
            sendThread.Start();

            this.receiveUdpState = s;
            var receiveThread = new System.Threading.Thread(this.ReceiveCallbackClass1);
            receiveThread.Start();
            //new System.Threading.Thread(() => {
            //    this.ReceiveCallbackClass1(s);
            //}).Start();

            //var asyncResult = this.udpClientReceive.BeginReceive(new AsyncCallback(this.ReceiveCallbackClass1), s);

            // Phase 3.5: signal that the implicit producer + consumer threads are up
            // and the target accepted our Forward Open. Fired after the threads start
            // so handlers can safely read T_O_IOData / subscribe to ImplicitDataReceived.
            this.lastImplicitTickCount = MonotonicMs();
            this.ConnectionEstablished?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Opens a Class-1 implicit connection using Large Forward Open (service 0x5B), for payloads larger than ~500 bytes.</summary>
        public void LargeForwardOpen() => this.ForwardOpen(true);

        private ushort o_t_detectedLength;
        /// <summary>
        /// Detects the Length of the data Originator -> Target.
        /// The Method uses an Explicit Message to detect the length.
        /// The IP-Address, Port and the Instance ID has to be defined before
        /// </summary>
        public ushort Detect_O_T_Length ()
        {
            if (this.o_t_detectedLength == 0)
            {
                if (this.sessionHandle == 0)
                    this.RegisterSession();
                this.o_t_detectedLength = (ushort)(this.GetAttributeSingle(0x04, this.O_T_InstanceID, 3)).Length;
                return this.o_t_detectedLength;
            }
            else
                return this.o_t_detectedLength;
        }

        private ushort t_o_detectedLength;
        /// <summary>
        /// Detects the Length of the data Target -> Originator.
        /// The Method uses an Explicit Message to detect the length.
        /// The IP-Address, Port and the Instance ID has to be defined before
        /// </summary>
        public ushort Detect_T_O_Length()
        {
            if (this.t_o_detectedLength == 0)
            {
                if (this.sessionHandle == 0)
                    this.RegisterSession();
                this.t_o_detectedLength = (ushort)(this.GetAttributeSingle(0x04, this.T_O_InstanceID, 3)).Length;
                return this.t_o_detectedLength;
            }
            else
                return this.t_o_detectedLength;
        }

        private static uint GetMulticastAddress(uint deviceIPAddress)
        {
            var cip_Mcast_Base_Addr = 0xEFC00100;
            uint cip_Host_Mask = 0x3FF;
            uint netmask = 0;

            //Class A Network?
            if (deviceIPAddress <= 0x7FFFFFFF)
                netmask = 0xFF000000;
            //Class B Network?
            if (deviceIPAddress >= 0x80000000 && deviceIPAddress <= 0xBFFFFFFF)
                netmask = 0xFFFF0000;
            //Class C Network?
            if (deviceIPAddress >= 0xC0000000 && deviceIPAddress <= 0xDFFFFFFF)
                netmask = 0xFFFFFF00;

            var hostID = deviceIPAddress & ~netmask;
            var mcastIndex = hostID - 1;
            mcastIndex = mcastIndex & cip_Host_Mask;

            return (uint) (cip_Mcast_Base_Addr + mcastIndex * (uint)32);

        }

        /// <summary>Sends a Forward Close to tear down the Class-1 implicit connection,
        /// stops the producer thread, waits for the producer to exit, then sends the
        /// close request and closes the UDP receive socket. Safe to call from a
        /// <c>using</c> block via <see cref="Dispose"/>.</summary>
        public void ForwardClose()
        {
            //First stop the Thread which send data

            this.stopUDP = true;

            var max_delay = (this.RequestedPacketRate_O_T > this.RequestedPacketRate_T_O) ? this.RequestedPacketRate_O_T : this.RequestedPacketRate_T_O;
            max_delay /= 1000;
            Thread.Sleep((int)max_delay + 1); // wait to make sure thread stopUDP stop


            var lengthOffset = (5 + (this.O_T_ConnectionType == ConnectionType.Null ? 0 : 2) + (this.T_O_ConnectionType == ConnectionType.Null ? 0 : 2));

            var encapsulation = new Encapsulation();
            encapsulation.SessionHandle = this.sessionHandle;
            encapsulation.Command = Encapsulation.CommandsEnum.SendRRData;
            encapsulation.Length = (ushort)(16 + 17 + (ushort)lengthOffset);
            //---------------Interface Handle CIP
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            //----------------Interface Handle CIP

            //----------------Timeout
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            //----------------Timeout

            //Common Packet Format (Table 2-6.1)
            var commonPacketFormat = new Encapsulation.CommonPacketFormat();
            commonPacketFormat.ItemCount = 0x02;

            commonPacketFormat.AddressItem = 0x0000;        //NULL (used for UCMM Messages)
            commonPacketFormat.AddressLength = 0x0000;


            commonPacketFormat.DataItem = 0xB2;
            commonPacketFormat.DataLength = (ushort)(17 + (ushort)lengthOffset);



            //----------------CIP Command "Forward Close"
            commonPacketFormat.Data.Add(0x4E);
            //----------------CIP Command "Forward Close"

            //----------------Requested Path size
            commonPacketFormat.Data.Add(2);
            //----------------Requested Path size

            //----------------Path segment for Class ID
            commonPacketFormat.Data.Add(0x20);
            commonPacketFormat.Data.Add((byte)6);
            //----------------Path segment for Class ID

            //----------------Path segment for Instance ID
            commonPacketFormat.Data.Add(0x24);
            commonPacketFormat.Data.Add((byte)1);
            //----------------Path segment for Instace ID

            //----------------Priority and Time/Tick - Table 3-5.16 (Vol. 1)
            commonPacketFormat.Data.Add(0x03);
            //----------------Priority and Time/Tick

            //----------------Timeout Ticks - Table 3-5.16 (Vol. 1)
            commonPacketFormat.Data.Add(0xfa);
            //----------------Timeout Ticks

            //Connection serial number
            commonPacketFormat.Data.Add((byte)this.connectionSerialNumber);
            commonPacketFormat.Data.Add((byte)(this.connectionSerialNumber >> 8));
            //connection seruial number

            //----------------Originator Vendor ID
            commonPacketFormat.Data.Add(0xFF);
            commonPacketFormat.Data.Add(0);
            //----------------Originaator Vendor ID

            //----------------Originator Serial Number
            commonPacketFormat.Data.Add(0xFF);
            commonPacketFormat.Data.Add(0xFF);
            commonPacketFormat.Data.Add(0xFF);
            commonPacketFormat.Data.Add(0xFF);
            //----------------Originator Serial Number

            //Connection Path size 
            commonPacketFormat.Data.Add((byte)((0x2) + (this.O_T_ConnectionType == ConnectionType.Null ? 0 : 1) + (this.T_O_ConnectionType == ConnectionType.Null ? 0 : 1)));
            //Reserved
            commonPacketFormat.Data.Add(0);
            //Reserved

            commonPacketFormat.Data.Add((byte)(0x20));
            commonPacketFormat.Data.Add(this.AssemblyObjectClass);
            commonPacketFormat.Data.Add((byte)(0x24));
            commonPacketFormat.Data.Add((byte)(this.ConfigurationAssemblyInstanceID));
            if (this.O_T_ConnectionType != ConnectionType.Null) {
                commonPacketFormat.Data.Add((byte)(0x2C));
                commonPacketFormat.Data.Add((byte)(this.O_T_InstanceID));
            }
            if (this.T_O_ConnectionType != ConnectionType.Null) {
                commonPacketFormat.Data.Add((byte)(0x2C));
                commonPacketFormat.Data.Add((byte)(this.T_O_InstanceID));
            }

            var dataToWrite = new byte[encapsulation.Tobytes().Length + commonPacketFormat.Tobytes().Length];
            Array.Copy(encapsulation.Tobytes(), 0, dataToWrite, 0, encapsulation.Tobytes().Length);
            Array.Copy(commonPacketFormat.Tobytes(), 0, dataToWrite, encapsulation.Tobytes().Length, commonPacketFormat.Tobytes().Length);
            encapsulation.Tobytes();
            try {
                this.stream.Write(dataToWrite, 0, dataToWrite.Length);
            }
            catch  {
                //Handle Exception  to allow Forward close if the connection was closed by the Remote Device before
            }
            var data = new byte[BUFFER_SIZE + 64];

            try {
                var bytes = this.stream.Read(data, 0, data.Length);
            }
            catch (Exception) {
                //Handle Exception  to allow Forward close if the connection was closed by the Remote Device before
            }


            //--------------------------BEGIN Error?
            if (data[42] != 0)      //Exception codes see "Table B-1.1 CIP General Status Codes"
            {
                throw new CIPException(GeneralStatusCodes.GetStatusCode(data[42]));
            }


            //Close the Socket for Receive
            this.udpClientReceiveClosed = true;
            Thread.Sleep(1); // Wait for the thread using udpClientReceive close
            this.udpClientReceive.Close();




        }

        private bool stopUDP;
        int sequence = 0;
        private void sendUDP()
        {
            var udpClientsend = new System.Net.Sockets.UdpClient();
            this.stopUDP = false;
            uint sequenceCount = 0;

            // Phase 3.5: ConnectionLost detection — count consecutive send failures.
            // Reset to 0 on a successful Send. The connectionLostFired flag ensures the
            // event raises once per connection, not on every subsequent failed tick.
            int sendErrorStreak = 0;
            bool connectionLostFired = false;

            while (!this.stopUDP)
            {
                var o_t_IOData = new byte[BUFFER_SIZE + 64];
                var endPointsend = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(this.IPAddress), this.TargetUDPPort);
               
                var send = new UdpState();
                 
                //---------------Item count
                o_t_IOData[0] = 2;
                o_t_IOData[1] = 0;
                //---------------Item count

                //---------------Type ID
                o_t_IOData[2] = 0x02;
                o_t_IOData[3] = 0x80;
                //---------------Type ID

                //---------------Length
                o_t_IOData[4] = 0x08;
                o_t_IOData[5] = 0x00;
                //---------------Length

                //---------------connection ID
                sequenceCount++;
                o_t_IOData[6] = (byte)(this.connectionID_O_T);
                o_t_IOData[7] = (byte)(this.connectionID_O_T >> 8); 
                o_t_IOData[8] = (byte)(this.connectionID_O_T >> 16); 
                o_t_IOData[9] = (byte)(this.connectionID_O_T >> 24);
                //---------------connection ID     

                //---------------sequence count
                o_t_IOData[10] = (byte)(sequenceCount);
                o_t_IOData[11] = (byte)(sequenceCount >> 8);
                o_t_IOData[12] = (byte)(sequenceCount >> 16);
                o_t_IOData[13] = (byte)(sequenceCount >> 24);
                //---------------sequence count            

                //---------------Type ID
                o_t_IOData[14] = 0xB1;
                o_t_IOData[15] = 0x00;
                //---------------Type ID

                ushort headerOffset = 0;
                if (this.O_T_RealTimeFormat == RealTimeFormat.Header32Bit)
                    headerOffset = 4;
                if (this.O_T_RealTimeFormat == RealTimeFormat.Heartbeat)
                    headerOffset = 0;
                var o_t_Length = (ushort)(this.O_T_Length + headerOffset+2);   //Modeless and zero Length

                //---------------Length
                o_t_IOData[16] = (byte)o_t_Length;
                o_t_IOData[17] = (byte)(o_t_Length >> 8);
                //---------------Length

                //---------------Sequence count
                this.sequence++;
                if (this.O_T_RealTimeFormat != RealTimeFormat.Heartbeat)
                {
                    o_t_IOData[18] = (byte)this.sequence;
                    o_t_IOData[19] = (byte)(this.sequence >> 8);
                }
                //---------------Sequence count

                if (this.O_T_RealTimeFormat == RealTimeFormat.Header32Bit)
                {
                    o_t_IOData[20] = (byte)1;
                    o_t_IOData[21] = (byte)0;
                    o_t_IOData[22] = (byte)0;
                    o_t_IOData[23] = (byte)0;

                }

                    //---------------Write data
                    for ( var i = 0; i < this.O_T_Length; i++)
                        o_t_IOData[20+headerOffset+i] = (byte)this.O_T_IOData[i];
                //---------------Write data


                try {
                    udpClientsend.Send(o_t_IOData, this.O_T_Length + 20 + headerOffset, endPointsend);
                    sendErrorStreak = 0;
                }
                catch {
                    // Transient send failure (e.g. link briefly down, ARP miss, sendto EAGAIN).
                    // Swallow and retry on next RPI tick — letting the exception escape would
                    // kill the producer thread silently and leave ForwardOpen reporting
                    // "established" while no data is being produced.
                    //
                    // Phase 3.5: count consecutive failures. If we miss >= 4 sends in a row
                    // (which exceeds typical scanner watchdog of 4 * RPI), the target will
                    // close the connection on its end anyway — surface ConnectionLost so
                    // the application can react instead of polling LastReceivedImplicitMessage.
                    if (++sendErrorStreak == 4 && !connectionLostFired) {
                        connectionLostFired = true;
                        this.ConnectionLost?.Invoke(this, EventArgs.Empty);
                    }
                }
                System.Threading.Thread.Sleep((int)this.RequestedPacketRate_O_T /1000);

            }

            udpClientsend.Close();

        }

        UdpState receiveUdpState;
        private void ReceiveCallbackClass1()
        {
            var u = this.receiveUdpState.u;
            var e = this.receiveUdpState.e;

           


            while (!this.udpClientReceiveClosed) {

                try {
                    while (u.Available == 0 && !this.udpClientReceiveClosed) {
                        Thread.Sleep(1);
                    }

                    if (this.udpClientReceiveClosed)
                        return;

                    lock (this) {
                        IPEndPoint recEndpoint = null;

                        var receivebytes = u.Receive(ref recEndpoint);


                        // EndReceive worked and we have received data and remote endpoint

                        if (receivebytes.Length > 20) {
                            //Get the connection ID
                            var connectionID = (uint)(receivebytes[6] | receivebytes[7] << 8 | receivebytes[8] << 16 | receivebytes[9] << 24);


                            if (connectionID == this.connectionID_T_O) {
                                ushort headerOffset = 0;
                                if (this.T_O_RealTimeFormat == RealTimeFormat.Header32Bit)
                                    headerOffset = 4;
                                if (this.T_O_RealTimeFormat == RealTimeFormat.Heartbeat)
                                    headerOffset = 0;

                                // Phase 3.5: build the snapshot byte[] FIRST, then copy into
                                // T_O_IOData (still public+mutable for backwards compat). The
                                // snapshot is what the event delivers — handlers that use it
                                // avoid the torn-read race entirely.
                                var payloadLength = receivebytes.Length - 20 - headerOffset;
                                var snapshot = new byte[payloadLength];
                                for (var i = 0; i < payloadLength; i++) {
                                    snapshot[i] = receivebytes[20 + i + headerOffset];
                                    this.T_O_IOData[i] = snapshot[i];
                                }

                                // RpiViolated detection: millisecond delta from a monotonically
                                // increasing source. TinyCLR's mscorlib lacks Environment.TickCount,
                                // so we derive ms from DateTime.UtcNow.Ticks (truncated to int — wraps
                                // every ~24 days but the delta math still works for RPI tolerances).
                                var nowTicks = MonotonicMs();
                                var rpiMs = (int)(this.RequestedPacketRate_T_O / 1000);
                                if (this.lastImplicitTickCount != 0 && rpiMs > 0
                                    && (nowTicks - this.lastImplicitTickCount) > rpiMs * 4) {
                                    this.RpiViolated?.Invoke(this, EventArgs.Empty);
                                }
                                this.lastImplicitTickCount = nowTicks;
                                // Phase 3.5 (Item 3): expose the same monotonic value as a
                                // public property so user code can poll without subscribing
                                // to the RpiViolated event. Set after the violation check so
                                // we don't observe "0 elapsed" the first time.
                                this.LastReceivedImplicitMessageTickCount = nowTicks;

                                this.ImplicitDataReceived?.Invoke(this, snapshot);
                            }
                        }
                    }
                    this.LastReceivedImplicitMessage = DateTime.Now;
                }
                catch {

                }
            }
        }



        /// <summary>
        /// Sends a RegisterSession command to a target to initiate session
        /// </summary>
        /// <param name="address">IP-Address of the target device</param> 
        /// <param name="port">Port of the target device (default should be 0xAF12)</param> 
        /// <returns>Session Handle</returns>	
        public uint RegisterSession(string address, ushort port)
        {
            var addressSubstring = address.Split('.');
            var ipAddress = uint.Parse(addressSubstring[3]) + (uint.Parse(addressSubstring[2]) << 8) + (uint.Parse(addressSubstring[1]) << 16) + (uint.Parse(addressSubstring[0]) << 24);
            return this.RegisterSession(ipAddress, port);
        }

        /// <summary>
        /// Sends a RegisterSession command to a target to initiate session with the Standard or predefined Port (Standard: 0xAF12)
        /// </summary>
        /// <param name="address">IP-Address of the target device</param> 
        /// <returns>Session Handle</returns>	
        public uint RegisterSession(string address)
        {
            var addressSubstring = address.Split('.');
            var ipAddress = uint.Parse(addressSubstring[3]) + (uint.Parse(addressSubstring[2]) << 8) + (uint.Parse(addressSubstring[1]) << 16) + (uint.Parse(addressSubstring[0]) << 24);
            return this.RegisterSession(ipAddress, this.TCPPort);
        }

        /// <summary>
        /// Sends a RegisterSession command to a target to initiate session with the Standard or predefined Port and Predefined IPAddress (Standard-Port: 0xAF12)
        /// </summary>
        /// <returns>Session Handle</returns>	
        public uint RegisterSession() => this.RegisterSession(this.IPAddress, this.TCPPort);

        /// <summary>Sends a CIP Get_Attribute_Single (service 0x0E) to read one
        /// attribute from the target. Auto-registers a session if not already open.
        /// Blocks until the target replies or the underlying TCP read times out.</summary>
        /// <param name="classID">CIP class code (e.g. 0x01 = Identity).</param>
        /// <param name="instanceID">Instance number, 1-based.</param>
        /// <param name="attributeID">Attribute ID within the instance.</param>
        /// <returns>Raw attribute bytes. Endianness and structure depend on the
        /// attribute's CIP type — use the <c>ToUshort</c>/<c>ToUint</c> helpers or
        /// the strongly-typed accessors on <see cref="ObjectLibrary.IdentityObject"/>
        /// etc. for common cases.</returns>
        /// <exception cref="CIPException">Target returned a non-success CIP general
        /// status (e.g. 0x14 Attribute Not Supported, 0x05 Path Destination Unknown).</exception>
        public byte[] GetAttributeSingle(int classID, int instanceID, int attributeID)
        {
            var requestedPath = this.GetEPath(classID, instanceID, attributeID);
            if (this.sessionHandle == 0)             //If a Session is not Registers, Try to Registers a Session with the predefined IP-Address and Port
                this.RegisterSession();
            var dataToSend = new byte[42+ requestedPath.Length];
            var encapsulation = new Encapsulation();
            encapsulation.SessionHandle = this.sessionHandle;
            encapsulation.Command = Encapsulation.CommandsEnum.SendRRData;
            encapsulation.Length = (ushort)(18 + requestedPath.Length);
            //---------------Interface Handle CIP
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            //----------------Interface Handle CIP

            //----------------Timeout
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            //----------------Timeout

            //Common Packet Format (Table 2-6.1)
            var commonPacketFormat = new Encapsulation.CommonPacketFormat();
            commonPacketFormat.ItemCount = 0x02;

            commonPacketFormat.AddressItem = 0x0000;        //NULL (used for UCMM Messages)
            commonPacketFormat.AddressLength = 0x0000;

            commonPacketFormat.DataItem = 0xB2;
            commonPacketFormat.DataLength = (ushort)(2 + requestedPath.Length);



            //----------------CIP Command "Get Attribute Single"
            commonPacketFormat.Data.Add((byte)GHIElectronics.TinyCLR.EthernetIP.Scanner.CIPCommonServices.Get_Attribute_Single);
            //----------------CIP Command "Get Attribute Single"

            //----------------Requested Path size (number of 16 bit words)
            commonPacketFormat.Data.Add((byte)(requestedPath.Length / 2));
            //----------------Requested Path size (number of 16 bit words)

            //----------------Path segment for Class ID
            //----------------Path segment for Class ID

            //----------------Path segment for Instance ID
            //----------------Path segment for Instace ID

            //----------------Path segment for Attribute ID
            //----------------Path segment for Attribute ID

            for (var i = 0; i < requestedPath.Length; i++)
            {
                commonPacketFormat.Data.Add(requestedPath[i]);
            }

            var dataToWrite = new byte[encapsulation.Tobytes().Length + commonPacketFormat.Tobytes().Length];
            Array.Copy(encapsulation.Tobytes(), 0, dataToWrite, 0, encapsulation.Tobytes().Length);
            Array.Copy(commonPacketFormat.Tobytes(), 0, dataToWrite, encapsulation.Tobytes().Length, commonPacketFormat.Tobytes().Length);
            encapsulation.Tobytes();

            this.stream.Write(dataToWrite, 0, dataToWrite.Length);
            var data = new byte[BUFFER_SIZE + 64];

            var bytes = this.stream.Read(data, 0, data.Length);

            //--------------------------BEGIN Error?
            if (data[42] != 0)      //Exception codes see "Table B-1.1 CIP General Status Codes"
            {
                throw new CIPException(GeneralStatusCodes.GetStatusCode(data[42]));
            }
            //--------------------------END Error?

            var returnData = new byte[bytes - 44];
            Array.Copy(data, 44, returnData, 0, bytes-44);

            return returnData;
        }

        /// <summary>
        /// Implementation of Common Service "Get_Attribute_All" - Service Code: 0x01
        /// </summary>
        /// <param name="classID">Class id of requested Attributes</param> 
        /// <param name="instanceID">Instance of Requested Attributes (0 for class Attributes)</param> 
        /// <returns>Session Handle</returns>	
        public byte[] GetAttributeAll(int classID, int instanceID)
        {
            var requestedPath = this.GetEPath(classID, instanceID, 0);
            if (this.sessionHandle == 0)             //If a Session is not Registered, Try to Registers a Session with the predefined IP-Address and Port
                this.RegisterSession();
            var dataToSend = new byte[42 + requestedPath.Length];
            var encapsulation = new Encapsulation();
            encapsulation.SessionHandle = this.sessionHandle;
            encapsulation.Command = Encapsulation.CommandsEnum.SendRRData;
            encapsulation.Length = (ushort)(18 + requestedPath.Length);
            //---------------Interface Handle CIP
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            //----------------Interface Handle CIP

            //----------------Timeout
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            //----------------Timeout

            //Common Packet Format (Table 2-6.1)
            var commonPacketFormat = new Encapsulation.CommonPacketFormat();
            commonPacketFormat.ItemCount = 0x02;

            commonPacketFormat.AddressItem = 0x0000;        //NULL (used for UCMM Messages)
            commonPacketFormat.AddressLength = 0x0000;

            commonPacketFormat.DataItem = 0xB2;
            commonPacketFormat.DataLength = (ushort)(2 + requestedPath.Length); //WAS 6



            //----------------CIP Command "Get Attribute All"
            commonPacketFormat.Data.Add((byte)GHIElectronics.TinyCLR.EthernetIP.Scanner.CIPCommonServices.Get_Attributes_All);
            //----------------CIP Command "Get Attribute All"

            //----------------Requested Path size
            commonPacketFormat.Data.Add((byte)(requestedPath.Length / 2));
            //----------------Requested Path size

            //----------------Path segment for Class ID
            //----------------Path segment for Class ID

            //----------------Path segment for Instance ID
            //----------------Path segment for Instace ID
            for (var i = 0; i < requestedPath.Length; i++)
            {
                commonPacketFormat.Data.Add(requestedPath[i]);
            }

            var dataToWrite = new byte[encapsulation.Tobytes().Length + commonPacketFormat.Tobytes().Length];
            Array.Copy(encapsulation.Tobytes(), 0, dataToWrite, 0, encapsulation.Tobytes().Length);
            Array.Copy(commonPacketFormat.Tobytes(), 0, dataToWrite, encapsulation.Tobytes().Length, commonPacketFormat.Tobytes().Length);


            this.stream.Write(dataToWrite, 0, dataToWrite.Length);
            var data = new byte[BUFFER_SIZE + 64];

            var bytes = this.stream.Read(data, 0, data.Length);
            //--------------------------BEGIN Error?
            if (data[42] != 0)      //Exception codes see "Table B-1.1 CIP General Status Codes"
            {
                throw new CIPException(GeneralStatusCodes.GetStatusCode(data[42]));
            }
            //--------------------------END Error?

            var returnData = new byte[bytes - 44];
            Array.Copy(data, 44, returnData, 0, bytes - 44);

            return returnData;
        }

        /// <summary>Sends a CIP Set_Attribute_Single (service 0x10) to write one
        /// attribute on the target. Auto-registers a session if not already open.</summary>
        /// <param name="classID">CIP class code.</param>
        /// <param name="instanceID">Instance number, 1-based.</param>
        /// <param name="attributeID">Attribute ID within the instance.</param>
        /// <param name="value">Raw bytes to write. Must match the attribute's CIP type
        /// width and endianness — most types are little-endian on the wire.</param>
        /// <returns>Any reply bytes the target included. Usually empty.</returns>
        /// <exception cref="CIPException">Target returned a non-success status (e.g.
        /// 0x0E Attribute Not Settable, 0x09 Invalid Attribute Value).</exception>
        public byte[] SetAttributeSingle(int classID, int instanceID, int attributeID, byte[] value)
        {
            var requestedPath = this.GetEPath(classID, instanceID, attributeID);
            if (this.sessionHandle == 0)             //If a Session is not Registers, Try to Registers a Session with the predefined IP-Address and Port
                this.RegisterSession();
            var dataToSend = new byte[42 + value.Length + requestedPath.Length];
            var encapsulation = new Encapsulation();
            encapsulation.SessionHandle = this.sessionHandle;
            encapsulation.Command = Encapsulation.CommandsEnum.SendRRData;
            encapsulation.Length = (ushort)(18+value.Length + requestedPath.Length);
            //---------------Interface Handle CIP
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            //----------------Interface Handle CIP

            //----------------Timeout
            encapsulation.CommandSpecificData.Add(0);
            encapsulation.CommandSpecificData.Add(0);
            //----------------Timeout

            //Common Packet Format (Table 2-6.1)
            var commonPacketFormat = new Encapsulation.CommonPacketFormat();
            commonPacketFormat.ItemCount = 0x02;

            commonPacketFormat.AddressItem = 0x0000;        //NULL (used for UCMM Messages)
            commonPacketFormat.AddressLength = 0x0000;

            commonPacketFormat.DataItem = 0xB2;
            commonPacketFormat.DataLength = (ushort)(2 + value.Length+requestedPath.Length);



            //----------------CIP Command "Set Attribute Single"
            commonPacketFormat.Data.Add((byte)GHIElectronics.TinyCLR.EthernetIP.Scanner.CIPCommonServices.Set_Attribute_Single);
            //----------------CIP Command "Set Attribute Single"

            //----------------Requested Path size (number of 16 bit words)
            commonPacketFormat.Data.Add((byte)(requestedPath.Length/2));
            //----------------Requested Path size (number of 16 bit words)

            //----------------Path segment for Class ID
            //----------------Path segment for Class ID

            //----------------Path segment for Instance ID
            //----------------Path segment for Instace ID

            //----------------Path segment for Attribute ID
            //----------------Path segment for Attribute ID
            for (var i = 0; i < requestedPath.Length; i++)
            {
                commonPacketFormat.Data.Add(requestedPath[i]);
            }

                //----------------Data
            for (var i = 0; i < value.Length; i++)
            {
                commonPacketFormat.Data.Add(value[i]);
            }
            //----------------Data

            var dataToWrite = new byte[encapsulation.Tobytes().Length + commonPacketFormat.Tobytes().Length];
            Array.Copy(encapsulation.Tobytes(), 0, dataToWrite, 0, encapsulation.Tobytes().Length);
            Array.Copy(commonPacketFormat.Tobytes(), 0, dataToWrite, encapsulation.Tobytes().Length, commonPacketFormat.Tobytes().Length);
            encapsulation.Tobytes();

            this.stream.Write(dataToWrite, 0, dataToWrite.Length);
            var data = new byte[BUFFER_SIZE + 64];

            var bytes = this.stream.Read(data, 0, data.Length);

            //--------------------------BEGIN Error?
            if (data[42] != 0)      //Exception codes see "Table B-1.1 CIP General Status Codes"
            {
                throw new CIPException(GeneralStatusCodes.GetStatusCode(data[42]));
            }
            //--------------------------END Error?

            var returnData = new byte[bytes - 44];
            Array.Copy(data, 44, returnData, 0, bytes - 44);

            return returnData;
        }

        /// <summary>
        /// Get the Encrypted Request Path - See Volume 1 Appendix C (C9)
        /// e.g. for 8 Bit: 20 05 24 02 30 01
        /// for 16 Bit: 21 00 05 00 24 02 30 01
        /// </summary>
        /// <param name="classID">Requested Class ID</param>
        /// <param name="instanceID">Requested Instance ID</param>
        /// <param name="attributeID">Requested Attribute ID - if "0" the attribute will be ignored</param>
        /// <returns>Encrypted Request Path</returns>
        private byte[] GetEPath(int classID, int instanceID, int attributeID)
        {
            var byteCount = 0;
            if (classID < 0xff)
                byteCount = byteCount + 2;
            else
                byteCount = byteCount + 4;
           
            if (instanceID < 0xff)
                byteCount = byteCount + 2;
            else
                byteCount = byteCount + 4;
            if (attributeID != 0)
                if (attributeID < 0xff)
                    byteCount = byteCount + 2;
                else
                    byteCount = byteCount + 4;

            var returnValue = new byte[byteCount];
            byteCount = 0;
            if (classID < 0xff)
            {
                returnValue[byteCount] = 0x20;
                returnValue[byteCount+1] = (byte)classID;
                byteCount = byteCount + 2;
            }
            else
            {
                returnValue[byteCount] = 0x21;
                returnValue[byteCount + 1] = 0;                             //Padded byte
                returnValue[byteCount + 2] = (byte)classID;                 //LSB
                returnValue[byteCount + 3] = (byte)(classID>>8);            //MSB
                byteCount = byteCount + 4;
            }


            if (instanceID < 0xff)
            {
                returnValue[byteCount] = 0x24;
                returnValue[byteCount + 1] = (byte)instanceID;
                byteCount = byteCount + 2;
            }
            else
            {
                returnValue[byteCount] = 0x25;
                returnValue[byteCount + 1] = 0;                                //Padded byte
                returnValue[byteCount + 2] = ((byte)instanceID);                 //LSB
                returnValue[byteCount + 3] = (byte)(instanceID >> 8);          //MSB
                byteCount = byteCount + 4;
            }
            if (attributeID != 0)
                if (attributeID < 0xff)
                {
                    returnValue[byteCount] = 0x30;
                    returnValue[byteCount + 1] = (byte)attributeID;
                    byteCount = byteCount + 2;
                }
                else
                {
                    returnValue[byteCount] = 0x31;
                    returnValue[byteCount + 1] = 0;                                 //Padded byte
                    returnValue[byteCount + 2] = (byte)attributeID;                 //LSB
                    returnValue[byteCount + 3] = (byte)(attributeID >> 8);          //MSB
                    byteCount = byteCount + 4;
                }

            return returnValue;

        }

        /// <summary>
        /// Implementation of Common Service "Get_Attribute_All" - Service Code: 0x01
        /// </summary>
        /// <param name="classID">Class id of requested Attributes</param> 
        public byte[] GetAttributeAll(int classID) => this.GetAttributeAll(classID, 0);

        ObjectLibrary.IdentityObject identityObject;
        /// <summary>
        /// Implementation of the identity Object (Class Code: 0x01) - Required Object according to CIP-Specification
        /// </summary>
        public ObjectLibrary.IdentityObject IdentityObject
        {
            get
            {
                if (this.identityObject == null)
                    this.identityObject = new ObjectLibrary.IdentityObject(this);
                return this.identityObject;

            }
        }

        ObjectLibrary.MessageRouterObject messageRouterObject;
        /// <summary>
        /// Implementation of the Message Router Object (Class Code: 0x02) - Required Object according to CIP-Specification
        /// </summary>
        public ObjectLibrary.MessageRouterObject MessageRouterObject
        {
            get
            {
                if (this.messageRouterObject == null)
                    this.messageRouterObject = new ObjectLibrary.MessageRouterObject(this);
                return this.messageRouterObject;

            }
        }

        ObjectLibrary.AssemblyObject assemblyObject;
        /// <summary>
        /// Implementation of the Assembly Object (Class Code: 0x04)
        /// </summary>
        public ObjectLibrary.AssemblyObject AssemblyObject
        {
            get
            {
                if (this.assemblyObject == null)
                    this.assemblyObject = new ObjectLibrary.AssemblyObject(this);
                return this.assemblyObject;
            }
        }

        ObjectLibrary.TcpIpInterfaceObject tcpIpInterfaceObject;
        /// <summary>
        /// Implementation of the TCP/IP Object (Class Code: 0xF5) - Required Object according to CIP-Specification
        /// </summary>
        public ObjectLibrary.TcpIpInterfaceObject TcpIpInterfaceObject
        {
            get
            {
                if (this.tcpIpInterfaceObject == null)
                    this.tcpIpInterfaceObject = new ObjectLibrary.TcpIpInterfaceObject(this);
                return this.tcpIpInterfaceObject;

            }
        }

        /// <summary>
        /// Converts a bytearray (received e.g. via getAttributeSingle) to ushort
        /// </summary>
        /// <param name="byteArray">bytearray to convert</param> 
        public static ushort ToUshort(byte[] byteArray)
        {
            ushort returnValue;
            returnValue = (ushort)(byteArray[1] << 8 | byteArray[0]);
            return returnValue;
        }

        /// <summary>
        /// Converts a bytearray (received e.g. via getAttributeSingle) to uint
        /// </summary>
        /// <param name="byteArray">bytearray to convert</param> 
        public static uint ToUint(byte[] byteArray)
        {
            var returnValue = ((uint)byteArray[3] << 24 | (uint)byteArray[2] << 16 | (uint)byteArray[1] << 8 | (uint)byteArray[0]);
            return returnValue;
        }

        /// <summary>
        /// Returns the "Bool" State of a byte Received via getAttributeSingle
        /// </summary>
        /// <param name="inputbyte">byte to convert</param> 
        /// <param name="bitposition">bitposition to convert (First bit = bitposition 0)</param> 
        /// <returns>Converted bool value</returns>
        public static bool ToBool(byte inputbyte, int bitposition) => (((inputbyte >> bitposition) & 0x01) != 0) ? true : false;
    }

    /// <summary>The connection type for one direction of a Class-1 implicit connection.</summary>
    public enum ConnectionType : byte
    {
        /// <summary>No connection in this direction.</summary>
        Null = 0,
        /// <summary>Multicast connection.</summary>
        Multicast = 1,
        /// <summary>Point-to-point (unicast) connection.</summary>
        Point_to_Point = 2
    }

    /// <summary>The transport priority for one direction of a Class-1 implicit connection.</summary>
    public enum Priority : byte
    {
        /// <summary>Low priority.</summary>
        Low = 0,
        /// <summary>High priority.</summary>
        High = 1,
        /// <summary>Scheduled priority.</summary>
        Scheduled = 2,
        /// <summary>Urgent priority.</summary>
        Urgent = 3
    }

    /// <summary>The real-time data format used for one direction of a Class-1 implicit connection.</summary>
    public enum RealTimeFormat : byte
    {
        /// <summary>Pure data with no run/idle header.</summary>
        Modeless = 0,
        /// <summary>Zero-length data (idle indication only).</summary>
        ZeroLength = 1,
        /// <summary>Heartbeat with no data payload.</summary>
        Heartbeat = 2,
        /// <summary>32-bit run/idle real-time header preceding the data.</summary>
        Header32Bit = 3


    }
}
