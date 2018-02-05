using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using GHIElectronics.TinyCLR.Networking;

namespace System.Security.Authentication {
    [Flags]
    public enum SslProtocols {
        None = 0,
        Ssl2 = 1,
        Ssl3 = 2,
        Tls = 4,
        Tls11 = 8,
        Tls12 = 16,
        Default = Ssl3 | Tls
    }
}

namespace System.Net.Security {
    public class SslStream : NetworkStream
    {
        // Internal flags
        private int _sslContext;
        private bool _isServer;

        //--//

        public SslStream(Socket socket)
            : base(socket, false)
        {
            if (SocketType.Stream != (SocketType)this._socketType)
            {
                throw new NotSupportedException();
            }

            this._sslContext = -1;
            this._isServer = false;
        }

        public void AuthenticateAsClient(string targetHost, params SslProtocols[] sslProtocols) => AuthenticateAsClient(targetHost, null, null, sslProtocols);

        public void AuthenticateAsClient(string targetHost, X509Certificate cert, params SslProtocols[] sslProtocols) => AuthenticateAsClient(targetHost, cert, null, sslProtocols);

        public void AuthenticateAsClient(string targetHost, X509Certificate cert, X509Certificate[] ca, params SslProtocols[] sslProtocols) => Authenticate(false, targetHost, cert, ca, sslProtocols);

        public void AuthenticateAsServer(X509Certificate cert, params SslProtocols[] sslProtocols) => AuthenticateAsServer(cert, null, sslProtocols);

        public void AuthenticateAsServer(X509Certificate cert, X509Certificate[] ca, params SslProtocols[] sslProtocols) => Authenticate(true, "", cert, ca, sslProtocols);

        public void UpdateCertificates(X509Certificate cert, X509Certificate[] ca)
        {
            if(this._sslContext == -1) throw new InvalidOperationException();

            SslNative.UpdateCertificates(this._sslContext, cert, ca);
        }

        internal void Authenticate(bool isServer, string targetHost, X509Certificate certificate, X509Certificate[] ca, params SslProtocols[] sslProtocols)
        {
            var vers = (SslProtocols)0;

            if (-1 != this._sslContext) throw new InvalidOperationException();

            for (var i = sslProtocols.Length - 1; i >= 0; i--)
            {
                vers |= sslProtocols[i];
            }

            this._isServer = isServer;

            try
            {
                if (isServer)
                {
                    this._sslContext = SslNative.SecureServerInit((int)vers, certificate, ca);
                    SslNative.SecureAccept(this._sslContext, this._socket);
                }
                else
                {
                    this._sslContext = SslNative.SecureClientInit((int)vers, certificate, ca);
                    SslNative.SecureConnect(this._sslContext, targetHost, this._socket);
                }
            }
            catch
            {
                if (this._sslContext != -1)
                {
                    SslNative.ExitSecureContext(this._sslContext);
                    this._sslContext = -1;
                }

                throw;
            }
        }

        public bool IsServer => this._isServer;

        public override long Length
        {
            get
            {
                if (this._disposed == true) throw new ObjectDisposedException();
                if (this._socket == null) throw new IOException();

                return SslNative.DataAvailable(this._socket);
            }
        }

        public override bool DataAvailable
        {
            get
            {
                if (this._disposed == true) throw new ObjectDisposedException();
                if (this._socket == null) throw new IOException();

                return (SslNative.DataAvailable(this._socket) > 0);
            }
        }

        ~SslStream()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                this._disposed = true;

                if(this._socket.m_Handle != -1)
                {
                    SslNative.SecureCloseSocket(this._socket);
                    this._socket.m_Handle = -1;
                }

                if (this._sslContext != -1)
                {
                    SslNative.ExitSecureContext(this._sslContext);
                    this._sslContext = -1;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (this._disposed)
            {
                throw new ObjectDisposedException();
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (size < 0 || size > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }

            return SslNative.SecureRead(this._socket, buffer, offset, size, this._socket.ReceiveTimeout);
        }

        public override void Write(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            if (this._disposed)
            {
                throw new ObjectDisposedException();
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (size < 0 || size > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException();
            }

            SslNative.SecureWrite(this._socket, buffer, offset, size, this._socket.SendTimeout);
        }
    }
}


