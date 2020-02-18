using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using GHIElectronics.TinyCLR.Networking;

namespace System.Net.Security {
    public class SslStream : NetworkStream {
        // Internal flags
        private int sslHandle;
        private bool _isServer;
        private readonly INetworkProvider ni;

        //--//

        public SslStream(Socket socket)
            : base(socket, false) {
            if (SocketType.Stream != (SocketType)this._socketType) {
                throw new NotSupportedException();
            }

            this._isServer = false;
            this.sslHandle = -1;

            this.ni = Socket.DefaultProvider;
        }

        public void AuthenticateAsClient(string targetHost) => this.AuthenticateAsClient(targetHost, default(X509Certificate));

        public void AuthenticateAsClient(string targetHost, X509Certificate caCertificate) => this.AuthenticateAsClient(targetHost, caCertificate, null, SslProtocols.None);

        public void AuthenticateAsClient(string targetHost, X509Certificate caCertificate, X509Certificate clientCertificate) => this.AuthenticateAsClient(targetHost, caCertificate, clientCertificate, SslProtocols.None);

        public void AuthenticateAsClient(string targetHost, X509Certificate caCertificate, X509Certificate clientCertificate, SslProtocols sslProtocols) => this.AuthenticateAsClient(targetHost, caCertificate, clientCertificate, SslProtocols.None, SslVerification.Optional);

        public void AuthenticateAsClient(string targetHost, X509Certificate caCertificate, X509Certificate clientCertificate, SslProtocols sslProtocols, SslVerification sslVerification) => this.sslHandle = this.ni.AuthenticateAsClient(this._socket.m_Handle, targetHost, caCertificate, clientCertificate, sslProtocols, sslVerification);

        public void AuthenticateAsServer(X509Certificate caCertificate, SslProtocols sslProtocols) => this.sslHandle = this.ni.AuthenticateAsServer(this._socket.m_Handle, caCertificate, sslProtocols);

        public bool IsServer => this._isServer;

        public override long Length {
            get {
                if (this._disposed) throw new ObjectDisposedException();
                if (this._socket == null) throw new IOException();

                return this.ni.Available(this.sslHandle);
            }
        }

        public override bool DataAvailable {
            get {
                if (this._disposed) throw new ObjectDisposedException();
                if (this._socket == null) throw new IOException();

                return (this.ni.Available(this.sslHandle) > 0);
            }
        }

        ~SslStream() {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            this.Dispose(false);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void Dispose(bool disposing) {
            if (!this._disposed) {
                this._disposed = true;

                if (this.sslHandle != -1) {
                    this.ni.Close(this.sslHandle);
                    this.sslHandle = -1;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int size) {
            if (buffer == null) {
                throw new ArgumentNullException();
            }

            if (this.sslHandle == -1 || this._disposed) {
                throw new ObjectDisposedException();
            }

            if (offset < 0 || offset > buffer.Length) {
                throw new ArgumentOutOfRangeException();
            }

            if (size < 0 || size > buffer.Length - offset) {
                throw new ArgumentOutOfRangeException();
            }

            //var expired = DateTime.MaxValue.Ticks;
            var totalBytesReceive = 0;

            //if (this._socket.ReceiveTimeout != System.Threading.Timeout.Infinite) {
            //    expired = DateTime.Now.Ticks + (this._socket.ReceiveTimeout * 10000L);
            //}

            //while (DateTime.Now.Ticks < expired && totalBytesReceive < size)
            totalBytesReceive += this.ni.SecureRead(this.sslHandle, buffer, offset + totalBytesReceive, size - totalBytesReceive);

            return totalBytesReceive;
        }

        public override void Write(byte[] buffer, int offset, int size) {
            if (buffer == null) {
                throw new ArgumentNullException();
            }

            if (this.sslHandle == -1 || this._disposed) {
                throw new ObjectDisposedException();
            }

            if (offset < 0 || offset > buffer.Length) {
                throw new ArgumentOutOfRangeException();
            }

            if (size < 0 || size > buffer.Length - offset) {
                throw new ArgumentOutOfRangeException();
            }

            //var expired = DateTime.MaxValue.Ticks;
            var totalBytesSent = 0;

            //if (this._socket.SendTimeout != System.Threading.Timeout.Infinite) {
            //    expired = DateTime.Now.Ticks + (this._socket.SendTimeout * 10000L);
            //}

            //while (DateTime.Now.Ticks < expired && totalBytesSent < size)
            totalBytesSent += this.ni.SecureWrite(this.sslHandle, buffer, offset + totalBytesSent, size - totalBytesSent);

        }
    }

}


