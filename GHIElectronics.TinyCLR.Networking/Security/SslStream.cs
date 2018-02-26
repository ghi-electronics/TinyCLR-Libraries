using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using GHIElectronics.TinyCLR.Net.NetworkInterface;
using NI = System.Net.NetworkInterface.NetworkInterface;

namespace System.Net.Security {
    public class SslStream : NetworkStream {
        // Internal flags
        private int sslHandle;
        private bool _isServer;
        private readonly ISslStreamProvider ni;

        //--//

        public SslStream(Socket socket)
            : base(socket, false) {
            if (SocketType.Stream != (SocketType)this._socketType) {
                throw new NotSupportedException();
            }

            this._isServer = false;
            this.sslHandle = -1;

            this.ni = NI.GetActiveForSslStream();
        }

        public void AuthenticateAsClient(string targetHost, params SslProtocols[] sslProtocols) => AuthenticateAsClient(targetHost, default(X509Certificate));

        public void AuthenticateAsClient(string targetHost, X509Certificate cert, params SslProtocols[] sslProtocols) => this.sslHandle = this.ni.AuthenticateAsClient(this._socket.m_Handle, targetHost, cert, sslProtocols);

        public void AuthenticateAsServer(X509Certificate cert, params SslProtocols[] sslProtocols) => this.sslHandle = this.ni.AuthenticateAsServer(this._socket.m_Handle, cert, sslProtocols);


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
            Dispose(false);
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

            if (this._disposed) {
                throw new ObjectDisposedException();
            }

            if (offset < 0 || offset > buffer.Length) {
                throw new ArgumentOutOfRangeException();
            }

            if (size < 0 || size > buffer.Length - offset) {
                throw new ArgumentOutOfRangeException();
            }

            return this.ni.Read(this.sslHandle, buffer, offset, size, this._socket.ReceiveTimeout);
        }

        public override void Write(byte[] buffer, int offset, int size) {
            if (buffer == null) {
                throw new ArgumentNullException();
            }

            if (this._disposed) {
                throw new ObjectDisposedException();
            }

            if (offset < 0 || offset > buffer.Length) {
                throw new ArgumentOutOfRangeException();
            }

            if (size < 0 || size > buffer.Length - offset) {
                throw new ArgumentOutOfRangeException();
            }

            this.ni.Write(this.sslHandle, buffer, offset, size, this._socket.SendTimeout);
        }
    }
}


