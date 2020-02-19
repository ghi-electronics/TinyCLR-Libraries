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

        private SslStream sslStream;

        public bool DataAvailable {
            get {

                if (this.sslProtocol != SslProtocols.None)
                    return this.sslStream.DataAvailable;
                else
                    return (this.socket.Available > 0);

            }
        }

        public MqttStream(string hostName, int port, X509Certificate caCert, X509Certificate clientCert, SslProtocols sslProtocol) {
            IPAddress remoteIpAddress = null;

            if (remoteIpAddress == null) {
                var hostEntry = Dns.GetHostEntry(hostName);
                if ((hostEntry != null) && (hostEntry.AddressList.Length > 0)) {
                    var i = 0;
                    while (hostEntry.AddressList[i] == null) i++;
                    remoteIpAddress = hostEntry.AddressList[i];
                }
                else {
                    throw new Exception("Server not found."); ;
                }
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

                this.sslStream = new SslStream(this.socket);

                this.sslStream.AuthenticateAsClient(this.hostName, this.caCert, this.clientCert, this.sslProtocol);

            }

        }

        public int Send(byte[] buffer) {
            if (this.sslProtocol != SslProtocols.None) {
                this.sslStream.Write(buffer, 0, buffer.Length);
                this.sslStream.Flush();
                return buffer.Length;
            }
            else
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
                    idx += this.sslStream.Read(buffer, idx, buffer.Length - idx);
                }
            }
            else {
                if (this.socket.ReceiveTimeout != System.Threading.Timeout.Infinite) {
                    expired = DateTime.Now.Ticks + (this.socket.ReceiveTimeout * 10000L);
                }

                while (idx < buffer.Length && DateTime.Now.Ticks < expired) {
                    idx += this.socket.Receive(buffer, idx, buffer.Length - idx, SocketFlags.None);
                }
            }

            return idx;
        }

        public void Close() {
            if (this.sslProtocol != SslProtocols.None) {
                this.sslStream.Close();
            }
            this.socket.Close();
        }
    }
}
