using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
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

        public void AuthenticateAsClient(string targetHost, X509Certificate caCertificate, X509Certificate clientCertificate, SslProtocols sslProtocols) => this.AuthenticateAsClient(targetHost, caCertificate, clientCertificate, sslProtocols, SslVerification.Optional);

        public void AuthenticateAsClient(string targetHost, X509Certificate caCertificate, X509Certificate clientCertificate, SslProtocols sslProtocols, SslVerification sslVerification) => this.sslHandle = this.ni.AuthenticateAsClient(this._socket.m_Handle, targetHost, caCertificate, clientCertificate, sslProtocols, sslVerification);

        public void AuthenticateAsServer(X509Certificate caCertificate, SslProtocols sslProtocols) => this.sslHandle = this.ni.AuthenticateAsServer(this._socket.m_Handle, caCertificate, sslProtocols);

        public bool IsServer => this._isServer;

        // Standard .NET behavior: SslStream is not seekable and Length throws
        // NotSupportedException. The previous override returned `Available`
        // (decrypted plaintext bytes ready to read), which silently broke
        // callers that did `new byte[stream.Length]` — they'd get a buffer
        // sized to whatever happened to be buffered at that instant rather
        // than the full content. For "is there data ready" use DataAvailable
        // (bool, matches BCL NetworkStream.DataAvailable). For a content
        // length, read the HTTP/protocol header — never the stream's Length.
        public override long Length => throw new NotSupportedException();

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

            var expired = DateTime.MaxValue.Ticks;

            if (this._socket.ReceiveTimeout != System.Threading.Timeout.Infinite) {
                expired = DateTime.Now.Ticks + (this._socket.ReceiveTimeout * 10000L);
            }

            // Sentinels from native SecureRead (see Socket.NativeTimeoutSentinel).
            while (true) {
                var read = this.ni.SecureRead(this.sslHandle, buffer, offset, size);

                if (read > 0) return read;
                if (read == 0) return 0; // TLS close_notify / peer close — match .NET
                if (read == -2) {
                    // WANT_READ / WANT_WRITE: handshake or record fragment in flight.
                    if (DateTime.Now.Ticks >= expired)
                        throw new IOException("SSL read timed out.");
                    if (this._socket.DelayBetweenReceive > 0)
                        Thread.Sleep(this._socket.DelayBetweenReceive);
                    continue;
                }
                // Real TLS / transport error.
                throw new IOException("SSL read failed.");
            }
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

            var expired = DateTime.MaxValue.Ticks;
            var totalSent = 0;

            if (this._socket.SendTimeout != System.Threading.Timeout.Infinite) {
                expired = DateTime.Now.Ticks + (this._socket.SendTimeout * 10000L);
            }

            while (totalSent < size) {
                var sent = this.ni.SecureWrite(this.sslHandle, buffer, offset + totalSent, size - totalSent);

                if (sent > 0) {
                    totalSent += sent;
                    if (this._socket.SendTimeout != System.Threading.Timeout.Infinite) {
                        expired = DateTime.Now.Ticks + (this._socket.SendTimeout * 10000L);
                    }
                    if (totalSent < size && this._socket.DelayBetweenSend > 0)
                        Thread.Sleep(this._socket.DelayBetweenSend);
                    continue;
                }
                if (sent == -2) {
                    // WANT_READ / WANT_WRITE — transient, retry.
                    if (DateTime.Now.Ticks >= expired)
                        throw new IOException("SSL write timed out.");
                    if (this._socket.DelayBetweenSend > 0)
                        Thread.Sleep(this._socket.DelayBetweenSend);
                    continue;
                }
                // sent == 0 or other negative: TLS error or peer close mid-write.
                throw new IOException("SSL write failed.");
            }
        }
    }

}


