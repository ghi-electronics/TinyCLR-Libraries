////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("System.Net.Security")]

namespace System.Net.Sockets {
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using GHIElectronics.TinyCLR.Networking;

    public class Socket : IDisposable
    {
        /* WARNING!!!!
         * The m_Handle field MUST be the first field in the Socket class; it is expected by
         * the SPOT.NET.SocketNative class.
         */
        internal int m_Handle = -1;

        private bool m_fBlocking = true;
        private EndPoint m_localEndPoint = null;

        // timeout values are stored in uSecs since the Poll method requires it.
        private int m_recvTimeout = System.Threading.Timeout.Infinite;
        private int m_sendTimeout = System.Threading.Timeout.Infinite;

        public Socket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) => this.m_Handle = SocketNative.socket((int)addressFamily, (int)socketType, (int)protocolType);

        private Socket(int handle) => this.m_Handle = handle;

        public int Available
        {
            get
            {
                if (this.m_Handle == -1)
                {
                    throw new ObjectDisposedException();
                }

                uint cBytes = 0;

                SocketNative.ioctl(this, SocketNative.FIONREAD, ref cBytes);

                return (int)cBytes;
            }
        }

        private EndPoint GetEndPoint(bool fLocal)
        {
            if (this.m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            EndPoint ep = null;

            if (this.m_localEndPoint == null)
            {
                this.m_localEndPoint = new IPEndPoint(IPAddress.Any, 0);
            }

            byte[] address = null;

            if (fLocal)
            {
                SocketNative.getsockname(this, out address);
            }
            else
            {
                SocketNative.getpeername(this, out address);
            }

            var socketAddress = new SocketAddress(address);
            ep = this.m_localEndPoint.Create(socketAddress);

            if (fLocal)
            {
                this.m_localEndPoint = ep;
            }

            return ep;
        }

        public EndPoint LocalEndPoint => GetEndPoint(true);

        public EndPoint RemoteEndPoint => GetEndPoint(false);

        public int ReceiveTimeout {
            get => this.m_recvTimeout;

            set {
                if (value < Timeout.Infinite) throw new ArgumentOutOfRangeException();

                // desktop implementation treats 0 as infinite
                this.m_recvTimeout = ((value == 0) ? Timeout.Infinite : value);
            }
        }

        public int SendTimeout {
            get => this.m_sendTimeout;

            set {
                if (value < Timeout.Infinite) throw new ArgumentOutOfRangeException();

                // desktop implementation treats 0 as infinite
                this.m_sendTimeout = ((value == 0) ? Timeout.Infinite : value);
            }
        }

        public void Bind(EndPoint localEP)
        {
            if (this.m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            SocketNative.bind(this, localEP.Serialize().m_Buffer);

            this.m_localEndPoint = localEP;
        }

        public void Connect(EndPoint remoteEP)
        {
            if (this.m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            SocketNative.connect(this, remoteEP.Serialize().m_Buffer, !this.m_fBlocking);

            if (this.m_fBlocking)
            {
                Poll(-1, SelectMode.SelectWrite);
            }
        }

        public void Close() => ((IDisposable)this).Dispose();

        public void Listen(int backlog)
        {
            if (this.m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            SocketNative.listen(this, backlog);
        }

        public Socket Accept()
        {
            if (this.m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            int socketHandle;

            if (this.m_fBlocking)
            {
                Poll(-1, SelectMode.SelectRead);
            }

            socketHandle = SocketNative.accept(this);

            var socket = new Socket(socketHandle) {
                m_localEndPoint = this.m_localEndPoint
            };

            return socket;
        }

        public int Send(byte[] buffer, int size, SocketFlags socketFlags) => Send(buffer, 0, size, socketFlags);

        public int Send(byte[] buffer, SocketFlags socketFlags) => Send(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags);

        public int Send(byte[] buffer) => Send(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None);

        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            if (this.m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            return SocketNative.send(this, buffer, offset, size, (int)socketFlags, this.m_sendTimeout);
        }

        public int SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            if (this.m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            var address = remoteEP.Serialize().m_Buffer;

            return SocketNative.sendto(this, buffer, offset, size, (int)socketFlags, this.m_sendTimeout, address);
        }

        public int SendTo(byte[] buffer, int size, SocketFlags socketFlags, EndPoint remoteEP) => SendTo(buffer, 0, size, socketFlags, remoteEP);

        public int SendTo(byte[] buffer, SocketFlags socketFlags, EndPoint remoteEP) => SendTo(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags, remoteEP);

        public int SendTo(byte[] buffer, EndPoint remoteEP) => SendTo(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None, remoteEP);

        public int Receive(byte[] buffer, int size, SocketFlags socketFlags) => Receive(buffer, 0, size, socketFlags);

        public int Receive(byte[] buffer, SocketFlags socketFlags) => Receive(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags);

        public int Receive(byte[] buffer) => Receive(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None);

        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            if (this.m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            return SocketNative.recv(this, buffer, offset, size, (int)socketFlags, this.m_recvTimeout);
        }

        public int ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            if (this.m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            var address = remoteEP.Serialize().m_Buffer;
            var len = 0;

            len = SocketNative.recvfrom(this, buffer, offset, size, (int)socketFlags, this.m_recvTimeout, ref address);

            var socketAddress = new SocketAddress(address);
            remoteEP = remoteEP.Create(socketAddress);

            return len;
        }

        public int ReceiveFrom(byte[] buffer, int size, SocketFlags socketFlags, ref EndPoint remoteEP) => ReceiveFrom(buffer, 0, size, socketFlags, ref remoteEP);

        public int ReceiveFrom(byte[] buffer, SocketFlags socketFlags, ref EndPoint remoteEP) => ReceiveFrom(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags, ref remoteEP);

        public int ReceiveFrom(byte[] buffer, ref EndPoint remoteEP) => ReceiveFrom(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None, ref remoteEP);

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            if (this.m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            //BitConverter.GetBytes(int). Or else deal with endianness here?
            byte[] val;
            if(SystemInfo.IsBigEndian)
                val = new byte[4] { (byte)(optionValue >> 24), (byte)(optionValue >> 16), (byte)(optionValue >> 8), (byte)(optionValue >> 0) };
            else
                val = new byte[4] { (byte)(optionValue >> 0), (byte)(optionValue >> 8), (byte)(optionValue >> 16), (byte)(optionValue >> 24) };

            switch (optionName)
            {
                case SocketOptionName.SendTimeout:
                    this.m_sendTimeout = optionValue;
                    break;
                case SocketOptionName.ReceiveTimeout:
                    this.m_recvTimeout = optionValue;
                    break;
            }

            SocketNative.setsockopt(this, (int)optionLevel, (int)optionName, val);
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue) => SetSocketOption(optionLevel, optionName, (optionValue ? 1 : 0));

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            if (this.m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            SocketNative.setsockopt(this, (int)optionLevel, (int)optionName, optionValue);
        }

        public object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
        {
            if (optionName == SocketOptionName.DontLinger ||
                optionName == SocketOptionName.AddMembership ||
                optionName == SocketOptionName.DropMembership)
            {
                //special case linger?
                throw new NotSupportedException();
            }

            var val = new byte[4];

            GetSocketOption(optionLevel, optionName, val);

            //Use BitConverter.ToInt32
            //endianness?
            int iVal;

            if(SystemInfo.IsBigEndian)
                iVal = (val[3] << 0 | val[2] << 8 | val[1] << 16 | val[0] << 24);
            else
                iVal = (val[0] << 0 | val[1] << 8 | val[2] << 16 | val[3] << 24);


            return (object)iVal;
        }

        public void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] val)
        {
            if (this.m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            SocketNative.getsockopt(this, (int)optionLevel, (int)optionName, val);
        }

        public bool Poll(int microSeconds, SelectMode mode)
        {
            if (this.m_Handle == -1)
            {
                throw new ObjectDisposedException();
            }

            return SocketNative.poll(this, (int)mode, microSeconds);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        protected virtual void Dispose(bool disposing)
        {
            if (this.m_Handle != -1)
            {
                SocketNative.close(this);
                this.m_Handle = -1;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Socket()
        {
            Dispose(false);
        }
    }
}


