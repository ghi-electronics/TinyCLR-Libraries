#define SSL


using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace GHIElectronics.TinyCLR.Networking.Mqtt {
    /// <summary>
    /// Channel to communicate over the network
    /// </summary>
    public class MqttNetworkChannel : IMqttNetworkChannel {
        // remote host information
        private string remoteHostName;
        private IPAddress remoteIpAddress;
        private int remotePort;

        // socket for communication
        private Socket socket;
        // using SSL
        private bool secure;

        // CA certificate (on client)
        private X509Certificate caCert;
        // Server certificate (on broker)
        private X509Certificate serverCert;
        // client certificate (on client)
        private X509Certificate clientCert;

        // SSL/TLS protocol version
        private SslProtocols sslProtocol;

        /// <summary>
        /// Remote host name
        /// </summary>
        public string RemoteHostName => this.remoteHostName;

        /// <summary>
        /// Remote IP address
        /// </summary>
        public IPAddress RemoteIpAddress => this.remoteIpAddress;

        /// <summary>
        /// Remote port
        /// </summary>
        public int RemotePort => this.remotePort;

        // SSL stream
        private SslStream sslStream;

        //private NetworkStream netStream;


        /// <summary>
        /// Data available on the channel
        /// </summary>
        public bool DataAvailable {
            get {

                if (this.secure)
                    return this.sslStream.DataAvailable;
                else
                    return (this.socket.Available > 0);

            }
        }


        public MqttNetworkChannel(Socket socket) : this(socket, false, null, SslProtocols.None, null, null) {

        }


        public MqttNetworkChannel(Socket socket, bool secure, X509Certificate serverCert, SslProtocols sslProtocol) {
            this.socket = socket;
            this.secure = secure;
            this.serverCert = serverCert;
            this.sslProtocol = sslProtocol;

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="remoteHostName">Remote Host name</param>
        /// <param name="remotePort">Remote port</param>
        public MqttNetworkChannel(string remoteHostName, int remotePort)
            : this(remoteHostName, remotePort, false, null, null, SslProtocols.None, null, null) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="remoteHostName">Remote Host name</param>
        /// <param name="remotePort">Remote port</param>
        /// <param name="secure">Using SSL</param>
        /// <param name="caCert">CA certificate</param>
        /// <param name="clientCert">Client certificate</param>
        /// <param name="sslProtocol">SSL/TLS protocol version</param>
        public MqttNetworkChannel(string remoteHostName, int remotePort, bool secure, X509Certificate caCert, X509Certificate clientCert, SslProtocols sslProtocol) {
            IPAddress remoteIpAddress = null;
            try {
                // check if remoteHostName is a valid IP address and get it
                remoteIpAddress = IPAddress.Parse(remoteHostName);
            }
            catch {
            }

            // in this case the parameter remoteHostName isn't a valid IP address
            if (remoteIpAddress == null) {
                var hostEntry = Dns.GetHostEntry(remoteHostName);
                if ((hostEntry != null) && (hostEntry.AddressList.Length > 0)) {
                    // check for the first address not null
                    // it seems that with .Net Micro Framework, the IPV6 addresses aren't supported and return "null"
                    var i = 0;
                    while (hostEntry.AddressList[i] == null) i++;
                    remoteIpAddress = hostEntry.AddressList[i];
                }
                else {
                    throw new Exception("No address found for the remote host name");
                }
            }

            this.remoteHostName = remoteHostName;
            this.remoteIpAddress = remoteIpAddress;
            this.remotePort = remotePort;
            this.secure = secure;
            this.caCert = caCert;
            this.clientCert = clientCert;
            this.sslProtocol = sslProtocol;

        }

        public MqttNetworkChannel(Socket socket, bool v, object p1, SslProtocols none, object p2, object p3) => this.socket = socket;

        public MqttNetworkChannel(string remoteHostName, int remotePort, bool v, object p1, object p2, SslProtocols none, object p3, object p4) {
            this.remoteHostName = remoteHostName;
            this.remotePort = remotePort;
        }

        /// <summary>
        /// Connect to remote server
        /// </summary>
        public void Connect() {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // try connection to the broker
            this.socket.Connect(new IPEndPoint(this.remoteIpAddress, this.remotePort));


            // secure channel requested
            if (this.secure) {
                // create SSL stream
                this.sslStream = new SslStream(this.socket);

                this.sslStream.AuthenticateAsClient(this.remoteHostName, this.caCert, this.clientCert, this.sslProtocol);


            }

        }

        /// <summary>
        /// Send data on the network channel
        /// </summary>
        /// <param name="buffer">Data buffer to send</param>
        /// <returns>Number of byte sent</returns>
        public int Send(byte[] buffer) {

            if (this.secure) {
                this.sslStream.Write(buffer, 0, buffer.Length);
                this.sslStream.Flush();
                return buffer.Length;
            }
            else
                return this.socket.Send(buffer, 0, buffer.Length, SocketFlags.None);

        }

        /// <summary>
        /// Receive data from the network
        /// </summary>
        /// <param name="buffer">Data buffer for receiving data</param>
        /// <returns>Number of bytes received</returns>
        public int Receive(byte[] buffer) {

            if (this.secure) {
                // read all data needed (until fill buffer)
                int idx = 0, read = 0;
                while (idx < buffer.Length) {
                    // fixed scenario with socket closed gracefully by peer/broker and
                    // Read return 0. Avoid infinite loop.
                    read = this.sslStream.Read(buffer, idx, buffer.Length - idx);
                    if (read == 0)
                        return 0;
                    idx += read;
                }
                return buffer.Length;
            }
            else {
                // read all data needed (until fill buffer)
                int idx = 0, read = 0;
                while (idx < buffer.Length) {
                    // fixed scenario with socket closed gracefully by peer/broker and
                    // Read return 0. Avoid infinite loop.
                    read = this.socket.Receive(buffer, idx, buffer.Length - idx, SocketFlags.None);
                    if (read == 0)
                        return 0;
                    idx += read;
                }
                return buffer.Length;
            }

        }

        /// <summary>
        /// Receive data from the network channel with a specified timeout
        /// </summary>
        /// <param name="buffer">Data buffer for receiving data</param>
        /// <param name="timeout">Timeout on receiving (in milliseconds)</param>
        /// <returns>Number of bytes received</returns>
        public int Receive(byte[] buffer, int timeout) {
            // check data availability (timeout is in microseconds)
            if (this.socket.Poll(timeout * 1000, SelectMode.SelectRead)) {
                return this.Receive(buffer);
            }
            else {
                return 0;
            }
        }

        /// <summary>
        /// Close the network channel
        /// </summary>
        public void Close() {

            if (this.secure) {
                this.sslStream.Close();
            }
            this.socket.Close();

        }

        /// <summary>
        /// Accept connection from a remote client
        /// </summary>
        public void Accept() {

            // secure channel requested
            if (this.secure) {

                this.sslStream = new SslStream(this.socket);

                this.sslStream.AuthenticateAsServer(this.serverCert, this.sslProtocol);

            }

            return;

        }
    }



}
