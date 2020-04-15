[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("GHIElectronics.TinyCLR.Devices.Network")]

namespace System.Net.Sockets {
    using System.Net;
    using System.Runtime.CompilerServices;
    using GHIElectronics.TinyCLR.Networking;

    public class Socket : IDisposable {
        /* WARNING!!!!
* The m_Handle field MUST be the first field in the Socket class; it is expected by
* the SPOT.NET.this.ni class.
*/
        internal static INetworkProvider DefaultProvider { get; set; }

        internal int m_Handle = -1;

        private readonly INetworkProvider ni;
        private bool m_fBlocking = true;
        private EndPoint m_localEndPoint = null;

        // timeout values are stored in uSecs since the Poll method requires it.
        private int m_recvTimeout = System.Threading.Timeout.Infinite;
        private int m_sendTimeout = System.Threading.Timeout.Infinite;

        public Socket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
            this.ni = Socket.DefaultProvider;
            this.m_Handle = this.ni.Create(addressFamily, socketType, protocolType);
        }

        private Socket(int handle) {
            this.ni = Socket.DefaultProvider;
            this.m_Handle = handle;
        }

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

        public EndPoint LocalEndPoint => this.GetEndPoint(true);

        public EndPoint RemoteEndPoint => this.GetEndPoint(false);

        public int ReceiveTimeout {
            get => this.m_recvTimeout;

            set {
                if (value < System.Threading.Timeout.Infinite) throw new ArgumentOutOfRangeException();

                this.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value);
            }
        }

        public int SendTimeout {
            get => this.m_sendTimeout;

            set {
                if (value < System.Threading.Timeout.Infinite) throw new ArgumentOutOfRangeException();

                this.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);
            }
        }

        public void Bind(EndPoint localEP) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            this.ni.Bind(this.m_Handle, localEP.Serialize());

            this.m_localEndPoint = localEP;
        }

        public void Connect(EndPoint remoteEP) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            this.ni.Connect(this.m_Handle, remoteEP.Serialize());

            if (this.m_fBlocking) {
                this.Poll(-1, SelectMode.SelectWrite);
            }
        }

        public void Close() => ((IDisposable)this).Dispose();

        public void Listen(int backlog) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            this.ni.Listen(this.m_Handle, backlog);
        }

        public Socket Accept() {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            int socketHandle;

            if (this.m_fBlocking) {
                this.Poll(-1, SelectMode.SelectRead);
            }

            socketHandle = this.ni.Accept(this.m_Handle);

            var socket = new Socket(socketHandle) {
                m_localEndPoint = this.m_localEndPoint
            };

            return socket;
        }

        public int Send(byte[] buffer, int size, SocketFlags socketFlags) => this.Send(buffer, 0, size, socketFlags);

        public int Send(byte[] buffer, SocketFlags socketFlags) => this.Send(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags);

        public int Send(byte[] buffer) => this.Send(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None);

        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            //var expired = DateTime.MaxValue.Ticks;

            //if (this.SendTimeout != System.Threading.Timeout.Infinite) {
            //    expired = DateTime.Now.Ticks + (this.SendTimeout * 10000L);
            //}

            var totalSend = 0;

            //while (DateTime.Now.Ticks < expired && totalSend < size)
            totalSend += this.ni.Send(this.m_Handle, buffer, offset + totalSend, size - totalSend, socketFlags);

            return totalSend;
        }

        public int SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            var address = remoteEP.Serialize();

            //var expired = DateTime.MaxValue.Ticks;

            //if (this.SendTimeout != System.Threading.Timeout.Infinite) {
            //    expired = DateTime.Now.Ticks + (this.SendTimeout * 10000L);
            //}

            var totalSend = 0;

            //while (DateTime.Now.Ticks < expired && totalSend < size)
            totalSend += this.ni.SendTo(this.m_Handle, buffer, offset + offset + totalSend, size - totalSend, socketFlags, address);

            return totalSend;
        }

        public int SendTo(byte[] buffer, int size, SocketFlags socketFlags, EndPoint remoteEP) => this.SendTo(buffer, 0, size, socketFlags, remoteEP);

        public int SendTo(byte[] buffer, SocketFlags socketFlags, EndPoint remoteEP) => this.SendTo(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags, remoteEP);

        public int SendTo(byte[] buffer, EndPoint remoteEP) => this.SendTo(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None, remoteEP);

        public int Receive(byte[] buffer, int size, SocketFlags socketFlags) => this.Receive(buffer, 0, size, socketFlags);

        public int Receive(byte[] buffer, SocketFlags socketFlags) => this.Receive(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags);

        public int Receive(byte[] buffer) => this.Receive(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None);

        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            //var expired = DateTime.MaxValue.Ticks;

            //if (this.ReceiveTimeout != System.Threading.Timeout.Infinite) {
            //    expired = DateTime.Now.Ticks + (this.ReceiveTimeout * 10000L);
            //}

            var totalBytesReceive = 0;

            //while (DateTime.Now.Ticks < expired && totalBytesReceive < size)
            totalBytesReceive += this.ni.Receive(this.m_Handle, buffer, offset + totalBytesReceive, size - totalBytesReceive, socketFlags);

            return totalBytesReceive;
        }

        public int ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            var address = remoteEP.Serialize();
            var totalBytesReceive = 0;

            //var expired = DateTime.MaxValue.Ticks;

            //if (this.ReceiveTimeout != System.Threading.Timeout.Infinite) {
            //    expired = DateTime.Now.Ticks + (this.ReceiveTimeout * 10000L);
            //}

            //while (DateTime.Now.Ticks < expired && totalBytesReceive < size)
            totalBytesReceive += this.ni.ReceiveFrom(this.m_Handle, buffer, offset + totalBytesReceive, size - totalBytesReceive, socketFlags, ref address);

            remoteEP = remoteEP.Create(address);

            return totalBytesReceive;
        }

        public int ReceiveFrom(byte[] buffer, int size, SocketFlags socketFlags, ref EndPoint remoteEP) => this.ReceiveFrom(buffer, 0, size, socketFlags, ref remoteEP);

        public int ReceiveFrom(byte[] buffer, SocketFlags socketFlags, ref EndPoint remoteEP) => this.ReceiveFrom(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags, ref remoteEP);

        public int ReceiveFrom(byte[] buffer, ref EndPoint remoteEP) => this.ReceiveFrom(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None, ref remoteEP);

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

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue) => this.SetSocketOption(optionLevel, optionName, (optionValue ? 1 : 0));

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            this.ni.SetOption(this.m_Handle, optionLevel, optionName, optionValue);
        }

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

        public void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] val) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            this.ni.GetOption(this.m_Handle, optionLevel, optionName, val);
        }

        public bool Poll(int microSeconds, SelectMode mode) {
            if (this.m_Handle == -1) {
                throw new ObjectDisposedException();
            }

            var expired = (microSeconds == -1) ? DateTime.MaxValue.Ticks : (DateTime.Now.Ticks + microSeconds * 10);

            var poll = false;

            while ((DateTime.Now.Ticks < expired) && !poll) {
                poll = this.ni.Poll(this.m_Handle, microSeconds, mode);
            }

            return poll;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected virtual void Dispose(bool disposing) {
            if (this.m_Handle != -1) {
                this.ni.Close(this.m_Handle);
                this.m_Handle = -1;
            }
        }

        void IDisposable.Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Socket() {
            this.Dispose(false);
        }
    }
}


