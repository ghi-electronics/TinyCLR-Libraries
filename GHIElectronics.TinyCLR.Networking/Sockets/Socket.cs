[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("GHIElectronics.TinyCLR.Devices.Network")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("GHIElectronics.TinyCLR.Networking.Http")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("GHIElectronics.TinyCLR.Networking.Ftp")]

namespace System.Net.Sockets {
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using GHIElectronics.TinyCLR.Networking;

    /// <summary>Implements the Berkeley sockets interface for network communication.</summary>
    public class Socket : IDisposable {
        /* WARNING!!!!
* The m_Handle field MUST be the first field in the Socket class; it is expected by
* the SPOT.NET.this.ni class.
*/
        // Provider used by every new Socket() to dispatch its InternalCalls.
        // With multiple network controllers (Ethernet + WiFi) Enable()d
        // simultaneously, all providers ultimately share the same lwIP
        // socket pool — the underlying socket is routed by destination
        // netmask, not by which provider's wrapper invoked Create().
        // Switching DefaultProvider therefore only changes which native
        // API table new sockets dispatch through; existing sockets keep
        // their captured provider and continue working unaffected.
        internal static INetworkProvider DefaultProvider { get; set; }

        internal int m_Handle = -1;

        /// <summary>The delay, in milliseconds, between successive send attempts.</summary>
        public int DelayBetweenSend { get; set; } = 1;
        /// <summary>The delay, in milliseconds, between successive receive attempts.</summary>
        public int DelayBetweenReceive { get; set; } = 1;

        // Mirrors the native default in DEFAULT_*_TIMEOUT_IN_MILLISECOND. Used as
        // the native blocking-poll quantum; the user-visible Send/ReceiveTimeout
        // (m_sendTimeout / m_recvTimeout) is layered on top in managed code.
        private int nativeSendTimeout = 250;
        private int nativeReceiveTimeout = 250;

        // Sentinel values returned by the native Send/Receive layer.
        // Kept in sync with TINYCLR_LWIP_*_SENTINEL in tinyclr_lwip.h.
        //   0  : graceful close (peer FIN) - Receive only
        //   -2 : SO_RCVTIMEO/SO_SNDTIMEO expired with no data - retry
        //   -1 : real socket error - throw with SO_ERROR
        private const int NativeTimeoutSentinel = -2;
        private const int NativeErrorSentinel = -1;

        // Per-syscall native poll quantum (how long one underlying poll()
        // blocks before the managed Send/Receive loop wakes up to check the
        // user-visible SendTimeout/ReceiveTimeout deadline). 250 ms is fine
        // for the typical SC20260 workload; user code shouldn't need this.
        //
        // Hidden (internal) for two reasons:
        //   1. The BCL Socket has no equivalent — exposing it as public
        //      breaks dual-mode source: code that sets it compiles against
        //      TinyCLR.Networking but throws MissingMethodException when the
        //      same assembly loads under TinyCLR.Networking.Desktop.
        //   2. Users coming from .NET reach for "SendTimeout" expecting
        //      total operation timeout, not native poll quantum, and
        //      autocompleting on Native* leads them wrong.
        // The public knob for total timeout is SendTimeout / ReceiveTimeout.
        internal int NativeSendTimeout {
            get => this.nativeSendTimeout;

            set {
                if (value < System.Threading.Timeout.Infinite) throw new ArgumentOutOfRangeException();

                this.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);

                this.nativeSendTimeout = value;
            }
        }

        internal int NativeReceiveTimeout {
            get => this.nativeReceiveTimeout;

            set {
                if (value < System.Threading.Timeout.Infinite) throw new ArgumentOutOfRangeException();

                this.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value);

                this.nativeReceiveTimeout = value;
            }
        }

        private readonly INetworkProvider ni;
        private bool m_fBlocking = true;
        private EndPoint m_localEndPoint = null;

        // timeout values are stored in uSecs since the Poll method requires it.
        private int m_recvTimeout = System.Threading.Timeout.Infinite;
        private int m_sendTimeout = System.Threading.Timeout.Infinite;

        static int socketInUsedCount = 0;

        static object socketCountObject = new object();

        /// <summary>The number of sockets currently in use.</summary>
        public static int SocketInUsed => socketInUsedCount;

        // .NET-parity: track the address family / socket type / protocol type
        // the Socket was created with so callers don't need to remember them.
        private readonly AddressFamily m_AddressFamily;
        private readonly SocketType m_SocketType;
        private readonly ProtocolType m_ProtocolType;
        private bool m_isConnected;

        /// <summary>The address family of the socket.</summary>
        public AddressFamily AddressFamily => this.m_AddressFamily;
        /// <summary>The type of the socket.</summary>
        public SocketType SocketType => this.m_SocketType;
        /// <summary>The protocol type of the socket.</summary>
        public ProtocolType ProtocolType => this.m_ProtocolType;

        /// <summary>Whether the socket is connected to a remote host.</summary>
        // True once Connect() / Accept() succeeded; false after Dispose/Close
        // or before connection. Matches full .NET semantics for a TCP socket;
        // for UDP this stays false unless Connect() (associate a remote) was
        // called, also matching .NET.
        public bool Connected => this.m_Handle != -1 && this.m_isConnected;

        /// <summary>Initializes a new socket with the specified address family, type, and protocol.</summary>
        public Socket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
            this.ni = Socket.DefaultProvider;
            this.m_Handle = this.ni.Create(addressFamily, socketType, protocolType);
            this.m_AddressFamily = addressFamily;
            this.m_SocketType = socketType;
            this.m_ProtocolType = protocolType;

            lock(socketCountObject) {

                socketInUsedCount++;
            }
        }

        private Socket(int handle) {
            this.ni = Socket.DefaultProvider;
            this.m_Handle = handle;
            // Accepted sockets inherit the family/type/protocol of the listener.
            // We don't have native getters yet, so initialize to plausible TCP
            // defaults — accepted sockets are Connected by definition.
            this.m_AddressFamily = AddressFamily.InterNetwork;
            this.m_SocketType = SocketType.Stream;
            this.m_ProtocolType = ProtocolType.Tcp;
            this.m_isConnected = true;

            lock(socketCountObject) {

                socketInUsedCount++;
            }
        }

        /// <summary>The number of bytes available to be read from the socket.</summary>
        public int Available {
            get {
                if (this.m_Handle == -1) {
                    throw new ObjectDisposedException();
                }

                var cBytes = this.ni.Available(this.m_Handle);

                return cBytes;
            }
        }

        private EndPoint GetEndPoint(bool fLocal) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            EndPoint ep = null;

            if (this.m_localEndPoint == null) {
                this.m_localEndPoint = new IPEndPoint(IPAddress.Any, 0);
            }

            SocketAddress socketAddress;

            if (fLocal) {
                this.ni.GetLocalAddress(this.m_Handle, out socketAddress);
            }
            else {
                this.ni.GetRemoteAddress(this.m_Handle, out socketAddress);
            }

            ep = this.m_localEndPoint.Create(socketAddress);

            if (fLocal) {
                this.m_localEndPoint = ep;
            }

            return ep;
        }

        /// <summary>The local endpoint the socket is bound to.</summary>
        public EndPoint LocalEndPoint => this.GetEndPoint(true);

        /// <summary>The remote endpoint the socket is connected to.</summary>
        public EndPoint RemoteEndPoint => this.GetEndPoint(false);

        /// <summary>The amount of time, in milliseconds, that a receive operation waits before timing out.</summary>
        public int ReceiveTimeout {
            get => this.m_recvTimeout;

            set {
                if (value < System.Threading.Timeout.Infinite) throw new ArgumentOutOfRangeException();

                this.m_recvTimeout = value;
            }
        }

        /// <summary>The amount of time, in milliseconds, that a send operation waits before timing out.</summary>
        public int SendTimeout {
            get => this.m_sendTimeout;

            set {
                if (value < System.Threading.Timeout.Infinite) throw new ArgumentOutOfRangeException();

                this.m_sendTimeout = value;
            }
        }

        /// <summary>Binds the socket to the specified local endpoint.</summary>
        public void Bind(EndPoint localEP) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            this.ni.Bind(this.m_Handle, localEP.Serialize());

            this.m_localEndPoint = localEP;
        }

        /// <summary>Connects the socket to the specified remote endpoint.</summary>
        public void Connect(EndPoint remoteEP) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            this.ni.Connect(this.m_Handle, remoteEP.Serialize());

            // .NET's blocking Connect waits indefinitely for the handshake;
            // Poll's argument is microseconds — passing m_sendTimeout (which
            // holds milliseconds via the SendTimeout property) would cap a
            // 5000ms connect at 5ms. Use -1 (infinite) to match .NET / NETMF.
            if (this.m_fBlocking) {
                if (this.Poll(-1, SelectMode.SelectWrite) == false) {
                    throw new SocketException(SocketError.SocketError);
                }
            }

            this.m_isConnected = true;
        }

        /// <summary>Closes the socket and releases its resources.</summary>
        public void Close() => ((IDisposable)this).Dispose();

        /// <summary>Disables sending and/or receiving on the socket.</summary>
        // Disables sends and/or receives on this Socket without closing it.
        // Mirrors System.Net.Sockets.Socket.Shutdown.
        // - Send  : peer sees a graceful FIN on TCP; subsequent local sends fail.
        // - Receive: subsequent local Receives return 0.
        // - Both  : full half-close in both directions.
        public void Shutdown(SocketShutdown how) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            this.ni.Shutdown(this.m_Handle, how);
        }

        /// <summary>Places the socket in a listening state with the specified backlog.</summary>
        public void Listen(int backlog) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            this.ni.Listen(this.m_Handle, backlog);
        }

        /// <summary>Accepts a pending connection request and returns a new connected socket.</summary>
        public Socket Accept() {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            // The firmware's TinyCLR_Lwip_SocketAccept inherits the listen
            // socket's SO_RCVTIMEO (default 50ms) and returns -1 (sentinel)
            // on each timeout, mirroring how Receive returns 0 on timeout.
            // Real errors still surface as InvalidOperationException via the
            // extern. Loop until a real client arrives or the listener is
            // closed (Stop()/Dispose() sets m_Handle = -1).
            int socketHandle;
            while (true) {
                if (this.m_Handle == -1) throw new ObjectDisposedException();
                socketHandle = this.ni.Accept(this.m_Handle);
                if (socketHandle != -1) break;
                Thread.Sleep(1);
            }

            var socket = new Socket(socketHandle) {
                m_localEndPoint = this.m_localEndPoint
            };

            return socket;
        }

        /// <summary>Sends the specified number of bytes from the buffer and returns the number of bytes sent.</summary>
        public int Send(byte[] buffer, int size, SocketFlags socketFlags) => this.Send(buffer, 0, size, socketFlags);

        /// <summary>Sends the entire buffer and returns the number of bytes sent.</summary>
        public int Send(byte[] buffer, SocketFlags socketFlags) => this.Send(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags);

        /// <summary>Sends the entire buffer and returns the number of bytes sent.</summary>
        public int Send(byte[] buffer) => this.Send(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None);

        /// <summary>Sends data from the buffer starting at the given offset and returns the number of bytes sent.</summary>
        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags) {
            if (this.m_Handle == -1) throw new ObjectDisposedException();

            var expired = DateTime.MaxValue.Ticks;
            if (this.SendTimeout != System.Threading.Timeout.Infinite) {
                expired = DateTime.Now.Ticks + (this.SendTimeout * 10000L);
            }

            var totalSend = 0;

            while (totalSend < size) {
                if (this.m_Handle == -1) throw new ObjectDisposedException();

                var sent = this.ni.Send(this.m_Handle, buffer, offset + totalSend, size - totalSend, socketFlags);

                if (sent > 0) {
                    totalSend += sent;
                    if (this.SendTimeout != System.Threading.Timeout.Infinite) {
                        expired = DateTime.Now.Ticks + (this.SendTimeout * 10000L);
                    }
                    if (totalSend < size && this.DelayBetweenSend > 0)
                        Thread.Sleep(this.DelayBetweenSend);
                    continue;
                }
                if (sent == NativeTimeoutSentinel) {
                    // Send buffer was full (EAGAIN). Honour SendTimeout.
                    if (DateTime.Now.Ticks >= expired)
                        throw new SocketException(SocketError.TimedOut);
                    if (this.DelayBetweenSend > 0)
                        Thread.Sleep(this.DelayBetweenSend);
                    continue;
                }
                // sent == 0 is not normally reachable for a non-zero-byte send
                // on stream/dgram sockets. If it happens, treat as error.
                throw new SocketException(ReadSocketErrorOrGeneric());
            }

            return totalSend;
        }

        /// <summary>Sends data to the specified endpoint and returns the number of bytes sent.</summary>
        public int SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP) {
            if (this.m_Handle == -1) throw new ObjectDisposedException();

            var address = remoteEP.Serialize();

            var expired = DateTime.MaxValue.Ticks;
            if (this.SendTimeout != System.Threading.Timeout.Infinite) {
                expired = DateTime.Now.Ticks + (this.SendTimeout * 10000L);
            }

            var totalSend = 0;

            while (totalSend < size) {
                if (this.m_Handle == -1) throw new ObjectDisposedException();

                var sent = this.ni.SendTo(this.m_Handle, buffer, offset + totalSend, size - totalSend, socketFlags, address);

                if (sent > 0) {
                    totalSend += sent;
                    if (this.SendTimeout != System.Threading.Timeout.Infinite) {
                        expired = DateTime.Now.Ticks + (this.SendTimeout * 10000L);
                    }
                    if (totalSend < size && this.DelayBetweenSend > 0)
                        Thread.Sleep(this.DelayBetweenSend);
                    continue;
                }
                if (sent == NativeTimeoutSentinel) {
                    if (DateTime.Now.Ticks >= expired)
                        throw new SocketException(SocketError.TimedOut);
                    if (this.DelayBetweenSend > 0)
                        Thread.Sleep(this.DelayBetweenSend);
                    continue;
                }
                throw new SocketException(ReadSocketErrorOrGeneric());
            }

            return totalSend;
        }

        /// <summary>Sends the specified number of bytes to the given endpoint and returns the number of bytes sent.</summary>
        public int SendTo(byte[] buffer, int size, SocketFlags socketFlags, EndPoint remoteEP) => this.SendTo(buffer, 0, size, socketFlags, remoteEP);

        /// <summary>Sends the entire buffer to the given endpoint and returns the number of bytes sent.</summary>
        public int SendTo(byte[] buffer, SocketFlags socketFlags, EndPoint remoteEP) => this.SendTo(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags, remoteEP);

        /// <summary>Sends the entire buffer to the given endpoint and returns the number of bytes sent.</summary>
        public int SendTo(byte[] buffer, EndPoint remoteEP) => this.SendTo(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None, remoteEP);

        /// <summary>Receives the specified number of bytes into the buffer and returns the number of bytes read.</summary>
        public int Receive(byte[] buffer, int size, SocketFlags socketFlags) => this.Receive(buffer, 0, size, socketFlags);

        /// <summary>Receives data into the entire buffer and returns the number of bytes read.</summary>
        public int Receive(byte[] buffer, SocketFlags socketFlags) => this.Receive(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags);

        /// <summary>Receives data into the entire buffer and returns the number of bytes read.</summary>
        public int Receive(byte[] buffer) => this.Receive(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None);

        /// <summary>Receives data into the buffer starting at the given offset and returns the number of bytes read.</summary>
        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags) {
            if (this.m_Handle == -1) throw new ObjectDisposedException();

            var expired = DateTime.MaxValue.Ticks;
            if (this.ReceiveTimeout != System.Threading.Timeout.Infinite) {
                expired = DateTime.Now.Ticks + (this.ReceiveTimeout * 10000L);
            }

            while (true) {
                if (this.m_Handle == -1) throw new ObjectDisposedException();

                var read = this.ni.Receive(this.m_Handle, buffer, offset, size, socketFlags);

                if (read > 0) {
                    return read; // .NET parity: return as soon as any data arrives
                }
                if (read == 0) {
                    // Peer FIN-closed gracefully. .NET returns 0 immediately.
                    this.m_isConnected = false;
                    return 0;
                }
                if (read == NativeTimeoutSentinel) {
                    if (DateTime.Now.Ticks >= expired)
                        throw new SocketException(SocketError.TimedOut);
                    if (this.DelayBetweenReceive > 0)
                        Thread.Sleep(this.DelayBetweenReceive);
                    continue;
                }
                // Real error. Pull SO_ERROR for a specific code if available.
                throw new SocketException(ReadSocketErrorOrGeneric());
            }
        }

        // Best-effort: read SO_ERROR after a native -1 sentinel. lwIP errno
        // values are POSIX-style integers (not Winsock); callers can still
        // catch SocketException, but SocketErrorCode won't always map cleanly.
        // Falls back to the generic SocketError code if the option read fails.
        private int ReadSocketErrorOrGeneric() {
            try {
                return (int)this.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Error);
            }
            catch {
                return (int)SocketError.SocketError;
            }
        }

        /// <summary>Receives data and the sender's endpoint, returning the number of bytes read.</summary>
        public int ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP) {
            if (this.m_Handle == -1) throw new ObjectDisposedException();

            var address = remoteEP.Serialize();

            var expired = DateTime.MaxValue.Ticks;
            if (this.ReceiveTimeout != System.Threading.Timeout.Infinite) {
                expired = DateTime.Now.Ticks + (this.ReceiveTimeout * 10000L);
            }

            while (true) {
                if (this.m_Handle == -1) throw new ObjectDisposedException();

                var read = this.ni.ReceiveFrom(this.m_Handle, buffer, offset, size, socketFlags, ref address);

                if (read > 0) {
                    remoteEP = remoteEP.Create(address);
                    return read;
                }
                if (read == 0) {
                    // For SOCK_DGRAM this is a legitimate zero-length datagram
                    // (not a close, since UDP has no FIN). Surface as 0 bytes
                    // with the source address.
                    remoteEP = remoteEP.Create(address);
                    return 0;
                }
                if (read == NativeTimeoutSentinel) {
                    if (DateTime.Now.Ticks >= expired)
                        throw new SocketException(SocketError.TimedOut);
                    if (this.DelayBetweenReceive > 0)
                        Thread.Sleep(this.DelayBetweenReceive);
                    continue;
                }
                throw new SocketException(ReadSocketErrorOrGeneric());
            }
        }

        /// <summary>Receives the specified number of bytes and the sender's endpoint, returning the number of bytes read.</summary>
        public int ReceiveFrom(byte[] buffer, int size, SocketFlags socketFlags, ref EndPoint remoteEP) => this.ReceiveFrom(buffer, 0, size, socketFlags, ref remoteEP);

        /// <summary>Receives data into the entire buffer and the sender's endpoint, returning the number of bytes read.</summary>
        public int ReceiveFrom(byte[] buffer, SocketFlags socketFlags, ref EndPoint remoteEP) => this.ReceiveFrom(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags, ref remoteEP);

        /// <summary>Receives data into the entire buffer and the sender's endpoint, returning the number of bytes read.</summary>
        public int ReceiveFrom(byte[] buffer, ref EndPoint remoteEP) => this.ReceiveFrom(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None, ref remoteEP);

        /// <summary>Sets an integer-valued socket option.</summary>
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            //BitConverter.GetBytes(int). Or else deal with endianness here?
            byte[] val;
            if (SystemInfo.IsBigEndian)
                val = new byte[4] { (byte)(optionValue >> 24), (byte)(optionValue >> 16), (byte)(optionValue >> 8), (byte)(optionValue >> 0) };
            else
                val = new byte[4] { (byte)(optionValue >> 0), (byte)(optionValue >> 8), (byte)(optionValue >> 16), (byte)(optionValue >> 24) };

            switch (optionName) {
                case SocketOptionName.SendTimeout:
                    // desktop implementation treats 0 as infinite
                    this.m_sendTimeout = ((optionValue == 0) ? System.Threading.Timeout.Infinite : optionValue);
                    break;
                case SocketOptionName.ReceiveTimeout:
                    // desktop implementation treats 0 as infinite
                    this.m_recvTimeout = ((optionValue == 0) ? System.Threading.Timeout.Infinite : optionValue);
                    break;
            }

            this.ni.SetOption(this.m_Handle, optionLevel, optionName, val);
        }

        /// <summary>Sets a boolean-valued socket option.</summary>
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue) => this.SetSocketOption(optionLevel, optionName, (optionValue ? 1 : 0));

        /// <summary>Sets a byte-array-valued socket option.</summary>
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            this.ni.SetOption(this.m_Handle, optionLevel, optionName, optionValue);
        }

        /// <summary>Returns the value of the specified socket option as an integer.</summary>
        public object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName) {
            if (optionName == SocketOptionName.DontLinger ||
                optionName == SocketOptionName.AddMembership ||
                optionName == SocketOptionName.DropMembership) {
                //special case linger?
                throw new NotSupportedException();
            }

            var val = new byte[4];

            this.GetSocketOption(optionLevel, optionName, val);

            //Use BitConverter.ToInt32
            //endianness?
            int iVal;

            if (SystemInfo.IsBigEndian)
                iVal = (val[3] << 0 | val[2] << 8 | val[1] << 16 | val[0] << 24);
            else
                iVal = (val[0] << 0 | val[1] << 8 | val[2] << 16 | val[3] << 24);


            return (object)iVal;
        }

        /// <summary>Reads the value of the specified socket option into the given byte array.</summary>
        public void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] val) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            this.ni.GetOption(this.m_Handle, optionLevel, optionName, val);
        }

        /// <summary>Determines the status of the socket within the specified timeout.</summary>
        public bool Poll(int microSeconds, SelectMode mode) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            // microSeconds == 0 is a non-blocking probe (matches .NET).
            // Special-case it: the polling loop below would otherwise compute
            // expired == Now and skip the firmware call entirely, since the
            // first 'Now < expired' check trips immediately.
            if (microSeconds == 0) {
                return this.ni.Poll(this.m_Handle, 0, mode);
            }

            var expired = (microSeconds == -1) ? DateTime.MaxValue.Ticks : (DateTime.Now.Ticks + microSeconds * 10);

            while (DateTime.Now.Ticks < expired) {
                if (this.ni.Poll(this.m_Handle, microSeconds, mode))
                    return true;

                if (this.m_Handle == -1) { // socket closed - stop
                    break;
                }

                Thread.Sleep(1);
            }

            return false;
        }

        /// <summary>Releases the resources used by the socket.</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected virtual void Dispose(bool disposing) {
            if (this.m_Handle != -1) {

                lock (socketCountObject) {
                    if (socketInUsedCount > 0)
                        socketInUsedCount--;
                }

                this.ni.Close(this.m_Handle);
                this.m_Handle = -1;
                this.m_isConnected = false;
            }
        }

        void IDisposable.Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases resources when the socket is finalized.</summary>
        ~Socket() {
            this.Dispose(false);
        }
    }
}


