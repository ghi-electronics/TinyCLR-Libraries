using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace GHIElectronics.TinyCLR.Networking.Mqtt {
    internal class MqttStream {

        private string hostName;
        private IPAddress ipAddress;
        private int port;

        private Socket socket;

        private X509Certificate caCert;
        private X509Certificate clientCert;
        private SslProtocols sslProtocol;

        // BCL SslStream wraps a Stream (not a Socket directly the way TinyCLR's
        // SslStream does), so on Desktop we keep both the NetworkStream and the
        // SslStream around for proper lifecycle management.
        private NetworkStream networkStream;
        private SslStream sslStream;

        public MqttStream(string hostName, int port, X509Certificate caCert, X509Certificate clientCert, SslProtocols sslProtocol) {
            var hostEntry = Dns.GetHostEntry(hostName);
            if (hostEntry == null || hostEntry.AddressList.Length == 0) {
                throw new Exception("Server not found.");
            }

            IPAddress remoteIpAddress = null;
            for (var i = 0; i < hostEntry.AddressList.Length; i++) {
                if (hostEntry.AddressList[i] != null) {
                    remoteIpAddress = hostEntry.AddressList[i];
                    break;
                }
            }
            if (remoteIpAddress == null) {
                throw new Exception("Server not found.");
            }

            this.hostName = hostName;
            this.ipAddress = remoteIpAddress;
            this.port = port;
            this.caCert = caCert;
            this.clientCert = clientCert;
            this.sslProtocol = sslProtocol;
        }

        public void Connect() {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(new IPEndPoint(this.ipAddress, this.port));

            if (this.sslProtocol != SslProtocols.None) {
                // BCL SslStream(Stream, leaveInnerStreamOpen, validationCallback).
                // We own the socket lifecycle, so NetworkStream is created with
                // ownsSocket=false and SslStream with leaveInnerStreamOpen=false.
                this.networkStream = new NetworkStream(this.socket, ownsSocket: false);
                this.sslStream = new SslStream(
                    this.networkStream,
                    leaveInnerStreamOpen: false,
                    userCertificateValidationCallback: this.ValidateServerCertificate);

                var clientCerts = this.clientCert != null
                    ? new X509CertificateCollection(new[] { this.clientCert })
                    : null;

                this.sslStream.AuthenticateAsClient(
                    this.hostName,
                    clientCerts,
                    this.sslProtocol,
                    checkCertificateRevocation: false);
            }
        }

        public int Send(byte[] buffer) {
            if (this.sslProtocol != SslProtocols.None) {
                this.sslStream.Write(buffer, 0, buffer.Length);
                this.sslStream.Flush();
                return buffer.Length;
            }
            return this.socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        public int Receive(byte[] buffer) {
            var expired = DateTime.MaxValue.Ticks;
            var idx = 0;

            if (this.sslProtocol != SslProtocols.None) {
                if (this.sslStream.ReadTimeout != System.Threading.Timeout.Infinite) {
                    expired = DateTime.Now.Ticks + (this.sslStream.ReadTimeout * 10000L);
                }
                while (idx < buffer.Length && DateTime.Now.Ticks < expired) {
                    var n = this.sslStream.Read(buffer, idx, buffer.Length - idx);
                    if (n <= 0) break; // broker closed connection; surface partial read
                    idx += n;
                }
            }
            else {
                if (this.socket.ReceiveTimeout != System.Threading.Timeout.Infinite) {
                    expired = DateTime.Now.Ticks + (this.socket.ReceiveTimeout * 10000L);
                }
                while (idx < buffer.Length && DateTime.Now.Ticks < expired) {
                    var n = this.socket.Receive(buffer, idx, buffer.Length - idx, SocketFlags.None);
                    if (n <= 0) break;
                    idx += n;
                }
            }
            return idx;
        }

        public void Close() {
            if (this.sslStream != null) {
                try { this.sslStream.Close(); } catch { }
                this.sslStream = null;
            }
            if (this.networkStream != null) {
                try { this.networkStream.Close(); } catch { }
                this.networkStream = null;
            }
            this.socket?.Close();
        }

        // Server certificate validation. If the user supplied a custom CA cert
        // (typical for self-signed brokers), accept the chain when any element's
        // thumbprint matches that CA — covers both root and intermediate pinning.
        // Otherwise defer to BCL's standard chain validation.
        private bool ValidateServerCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors) {
            if (errors == SslPolicyErrors.None) {
                return true;
            }
            if (this.caCert == null) {
                return false;
            }
            if (chain != null) {
                var caHash = this.caCert.GetCertHashString();
                foreach (var element in chain.ChainElements) {
                    if (element.Certificate.GetCertHashString() == caHash) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
