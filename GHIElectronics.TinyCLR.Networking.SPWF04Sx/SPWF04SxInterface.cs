using System;
using System.Collections;
using System.Net;
using System.Net.NetworkInterface;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;

namespace GHIElectronics.TinyCLR.Networking.SPWF04Sx {
    public class SPWF04SxInterface : NetworkInterface, ISocket, IDns, IDisposable {
        private readonly Hashtable sockets;
        private readonly Queue pendingReads;
        private readonly Queue pendingEvents;
        private readonly byte[] writeCommandBuffer;
        private readonly byte[] readHeaderBuffer;
        private readonly byte[] readPayloadBuffer;
        private readonly string[] parameters;
        private readonly SpiDevice spi;
        private readonly GpioPin irq;
        private readonly GpioPin reset;
        private Thread worker;
        private bool running;
        private int validParameters;
        private int nextRead;
        private int pendingWriteLength;
        private byte[] pendingRawData;
        private int pendingRawDataOffset;
        private int pendingRawDataLength;
        private int nextSocketId;

        public event SPWF04SxIndicationReceivedEventHandler IndicationReceived;
        public event SPWF04SxErrorReceivedEventHandler ErrorReceived;

        public SPWF04SxWiFiState State { get; private set; }
        public bool ForceSocketsTls { get; set; }
        public string ForceSocketsTlsCommonName { get; set; }

        public static SpiConnectionSettings GetConnectionSettings(int chipSelectLine) => new SpiConnectionSettings(chipSelectLine) {
            ClockFrequency = 4000000,
            Mode = SpiMode.Mode0,
            SharingMode = SpiSharingMode.Exclusive,
            DataBitLength = 8
        };

        public SPWF04SxInterface(SpiDevice spi, GpioPin irq, GpioPin reset) {
            this.sockets = new Hashtable();
            this.pendingReads = new Queue();
            this.pendingEvents = new Queue();
            this.writeCommandBuffer = new byte[512];
            this.readHeaderBuffer = new byte[4];
            this.readPayloadBuffer = new byte[1500 + 500]; //Longest payload, set by the socket heap variable, plus overhead for other result codes and WINDs
            this.parameters = new string[16];
            this.spi = spi;
            this.irq = irq;
            this.reset = reset;

            this.reset.SetDriveMode(GpioPinDriveMode.Output);
            this.reset.Write(GpioPinValue.Low);

            this.irq.SetDriveMode(GpioPinDriveMode.Input);
        }

        ~SPWF04SxInterface() => this.Dispose(false);

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.TurnOff();

                this.spi.Dispose();
                this.irq.Dispose();
                this.reset.Dispose();
            }
        }

        public void TurnOn() {
            if (this.running) return;

            this.running = true;
            this.worker = new Thread(this.Process);
            this.worker.Start();

            this.reset.SetDriveMode(GpioPinDriveMode.Input);
        }

        public void TurnOff() {
            if (!this.running) return;

            this.reset.SetDriveMode(GpioPinDriveMode.Output);
            this.reset.Write(GpioPinValue.Low);

            this.running = false;
            this.worker.Join();
            this.worker = null;

            this.validParameters = 0;

            this.sockets.Clear();
            this.nextSocketId = 0;

            this.pendingReads.Clear();
            this.pendingEvents.Clear();
            this.nextRead = 0;

            this.pendingWriteLength = 0;
            this.pendingRawData = null;
            this.pendingRawDataOffset = 0;
            this.pendingRawDataLength = 0;
        }

        public void ClearTlsServerRootCertificate() {
            this.AddParameterToCommand("content");
            this.AddParameterToCommand("2");
            this.SendCommand(SPWF04SxCommandIds.TLSCERT);

            this.ReadBuffer();
            this.ReadBuffer();
        }

        public string SetTlsServerRootCertificate(byte[] certificate) {
            if (certificate == null) throw new ArgumentNullException();

            this.AddParameterToCommand("ca");
            this.AddParameterToCommand(certificate.Length.ToString());
            this.SendCommand(SPWF04SxCommandIds.TLSCERT, certificate, 0, certificate.Length);

            var result = this.ReadString();

            this.ReadBuffer();

            return result.Substring(result.IndexOf(':') + 1);
        }

        public int HttpGet(string host, string path, int port, SPWF04SxConnectionSecurityType connectionSecurity, byte[] buffer, int offset, int count, out int responseCode) {
            this.AddParameterToCommand(host);
            this.AddParameterToCommand(path);
            this.AddParameterToCommand(port.ToString());
            this.AddParameterToCommand(connectionSecurity == SPWF04SxConnectionSecurityType.None ? "0" : "2");
            this.AddParameterToCommand(null);
            this.AddParameterToCommand(null);
            this.AddParameterToCommand(null);
            this.AddParameterToCommand(null);
            this.SendCommand(SPWF04SxCommandIds.HTTPGET);

            if (connectionSecurity == SPWF04SxConnectionSecurityType.Tls) {
                this.ReadBuffer();
                this.ReadBuffer();
            }

            var result = this.ReadString();
            var parts = result.Split(':');
            var current = 0;
            var read = 0;

            do {
                current = this.ReadBuffer(buffer, offset + read, count - read);
                read += current;
            } while (current != 0);

            responseCode = parts[0] == "Http Server Status Code" ? int.Parse(parts[1]) : throw new Exception($"Request failed: {result}");

            return read;
        }

        //TODO Need to test on an actual server
        public int HttpPost(string host, string path, int port, SPWF04SxConnectionSecurityType connectionSecurity, byte[] buffer, int offset, int count, out int responseCode) {
            this.AddParameterToCommand(host);
            this.AddParameterToCommand(path);
            this.AddParameterToCommand(port.ToString());
            this.AddParameterToCommand(connectionSecurity == SPWF04SxConnectionSecurityType.None ? "0" : "2");
            this.AddParameterToCommand(null);
            this.AddParameterToCommand(null);
            this.AddParameterToCommand(null);
            this.AddParameterToCommand(null);
            this.SendCommand(SPWF04SxCommandIds.HTTPPOST);

            if (connectionSecurity == SPWF04SxConnectionSecurityType.Tls) {
                this.ReadBuffer();
                this.ReadBuffer();
            }

            var result = this.ReadString();
            var parts = result.Split(':');
            var current = 0;
            var read = 0;

            do {
                current = this.ReadBuffer(buffer, offset + read, count - read);
                read += current;
            } while (current != 0);

            responseCode = parts[0] == "Http Server Status Code" ? int.Parse(parts[1]) : throw new Exception($"Request failed: {result}");

            return read;
        }

        public int OpenSocket(string host, int port, SPWF04SxConnectionyType connectionType, SPWF04SxConnectionSecurityType connectionSecurity, string commonName = null) {
            this.AddParameterToCommand(host);
            this.AddParameterToCommand(port.ToString());
            this.AddParameterToCommand(null);
            this.AddParameterToCommand(commonName ?? (connectionType == SPWF04SxConnectionyType.Tcp ? (connectionSecurity == SPWF04SxConnectionSecurityType.Tls ? "s" : "t") : "u"));
            this.SendCommand(SPWF04SxCommandIds.SOCKON);

            if (connectionSecurity == SPWF04SxConnectionSecurityType.Tls) {
                this.ReadBuffer();
                this.ReadBuffer();
            }

            var result = this.ReadString().Split(':');

            this.ReadBuffer();

            return result[0] == "On" ? int.Parse(result[2]) : throw new Exception("Request failed");
        }

        public void CloseSocket(int socket) {
            this.AddParameterToCommand(socket.ToString());
            this.SendCommand(SPWF04SxCommandIds.SOCKC);
            this.ReadBuffer();
        }

        public void WriteSocket(int socket, byte[] data) => this.WriteSocket(socket, data, 0, data != null ? data.Length : throw new ArgumentNullException(nameof(data)));

        public void WriteSocket(int socket, byte[] data, int offset, int count) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset < 0) throw new ArgumentOutOfRangeException();
            if (count < 0) throw new ArgumentOutOfRangeException();
            if (offset + count > data.Length) throw new ArgumentOutOfRangeException();

            this.AddParameterToCommand(socket.ToString());
            this.AddParameterToCommand(count.ToString());
            this.SendCommand(SPWF04SxCommandIds.SOCKW, data, offset, count);
            this.ReadBuffer();
        }

        public int ReadSocket(int socket, byte[] buffer, int offset, int count) {
            this.AddParameterToCommand(socket.ToString());
            this.AddParameterToCommand(count.ToString());
            this.SendCommand(SPWF04SxCommandIds.SOCKR);

            this.ReadBuffer();

            var res = this.ReadBuffer(buffer, offset, count);

            this.ReadBuffer();

            return res;
        }

        public int QuerySocket(int socket) {
            this.AddParameterToCommand(socket.ToString());
            this.SendCommand(SPWF04SxCommandIds.SOCKQ);

            var result = this.ReadString().Split(':');

            this.ReadBuffer();

            return result[0] == "Query" ? int.Parse(result[1]) : throw new Exception("Request failed");
        }

        public void EnableRadio() {
            this.AddParameterToCommand("1");
            this.SendCommand(SPWF04SxCommandIds.WIFI);
            this.ReadBuffer();
        }

        public void DisableRadio() {
            this.AddParameterToCommand("0");
            this.SendCommand(SPWF04SxCommandIds.WIFI);
            this.ReadBuffer();
        }

        public void JoinNetwork(string ssid, string password) {
            this.DisableRadio();

            this.AddParameterToCommand("wifi_mode");
            this.AddParameterToCommand("1");
            this.SendCommand(SPWF04SxCommandIds.SCFG);
            this.ReadBuffer();

            this.AddParameterToCommand("wifi_priv_mode");
            this.AddParameterToCommand("2");
            this.SendCommand(SPWF04SxCommandIds.SCFG);
            this.ReadBuffer();

            this.AddParameterToCommand("wifi_wpa_psk_text");
            this.AddParameterToCommand(password);
            this.SendCommand(SPWF04SxCommandIds.SCFG);
            this.ReadBuffer();

            this.AddParameterToCommand(ssid);
            this.SendCommand(SPWF04SxCommandIds.SSIDTXT);
            this.ReadBuffer();

            this.EnableRadio();

            this.SendCommand(SPWF04SxCommandIds.WCFG);
            this.ReadBuffer();
        }

        protected void AddParameterToCommand(string parameter) => this.parameters[this.validParameters++] = parameter;

        protected void SendCommand(SPWF04SxCommandIds cmdId) => this.SendCommand(cmdId, null, 0, 0);

        protected void SendCommand(SPWF04SxCommandIds cmdId, byte[] rawData, int rawDataOffset, int rawDataCount) {
            if (rawData == null && rawDataCount != 0) throw new ArgumentException();
            if (rawDataOffset < 0) throw new ArgumentOutOfRangeException();
            if (rawDataCount < 0) throw new ArgumentOutOfRangeException();
            if (rawData != null && rawDataOffset + rawDataCount > rawData.Length) throw new ArgumentOutOfRangeException();

            if (this.pendingWriteLength != 0)
                throw new InvalidOperationException("Previous write not finished");

            var idx = 0;

            this.writeCommandBuffer[idx++] = 0x02;
            this.writeCommandBuffer[idx++] = 0x00;
            this.writeCommandBuffer[idx++] = 0x00;

            this.writeCommandBuffer[idx++] = (byte)cmdId;
            this.writeCommandBuffer[idx++] = (byte)this.validParameters;

            for (var i = 0; i < this.validParameters; i++) {
                var p = this.parameters[i];
                var pLen = p != null ? p.Length : 0;

                this.writeCommandBuffer[idx++] = (byte)pLen;

                if (!string.IsNullOrEmpty(p))
                    Encoding.UTF8.GetBytes(p, 0, pLen, this.writeCommandBuffer, idx);

                idx += pLen;
            }

            var len = idx + rawDataCount - 3;
            this.writeCommandBuffer[1] = (byte)((len >> 8) & 0xFF);
            this.writeCommandBuffer[2] = (byte)((len >> 0) & 0xFF);

            this.validParameters = 0;

            this.pendingRawData = rawData;
            this.pendingRawDataOffset = rawDataOffset;
            this.pendingRawDataLength = rawDataCount;
            this.pendingWriteLength = idx;
        }

        protected string ReadString() {
            while (true) {
                lock (this.pendingReads) {
                    if (this.pendingReads.Count != 0) {
                        var start = (int)this.pendingReads.Dequeue();
                        var end = (int)this.pendingReads.Dequeue();
                        var res = this.PayloadToString(start, end - start);

                        if (this.pendingReads.Count == 0)
                            this.nextRead = 0;

                        return res;
                    }
                }

                Thread.Sleep(10);
            }
        }

        protected int ReadBuffer() => this.ReadBuffer(null, 0, 0);

        protected int ReadBuffer(byte[] buffer, int offset, int count) {
            while (true) {
                lock (this.pendingReads) {
                    if (this.pendingReads.Count != 0) {
                        var start = (int)this.pendingReads.Dequeue();
                        var len = (int)this.pendingReads.Dequeue() - start;

                        if (this.pendingReads.Count == 0)
                            this.nextRead = 0;

                        if (buffer != null) {
                            if (len > count)
                                throw new SPWF04SxBufferOverflowException("Read buffer too small for response.");

                            Array.Copy(this.readPayloadBuffer, start, buffer, offset, len);
                        }

                        return len;
                    }
                }

                Thread.Sleep(10);
            }
        }

        private void Process() {
            while (this.running) {
                if (this.irq.Read() == GpioPinValue.High && this.pendingWriteLength > 0) {
                    this.spi.Write(this.writeCommandBuffer, 0, this.pendingWriteLength);

                    if (this.pendingRawDataLength > 0) {
                        while (this.irq.Read() == GpioPinValue.High)
                            Thread.Sleep(1);

                        this.spi.Write(this.pendingRawData, this.pendingRawDataOffset, this.pendingRawDataLength);

                        this.pendingRawData = null;
                        this.pendingRawDataOffset = 0;
                        this.pendingRawDataLength = 0;
                    }

                    this.pendingWriteLength = 0;
                }

                while (this.irq.Read() == GpioPinValue.Low) {
                    do {
                        Thread.Sleep(10);

                        this.spi.Read(this.readHeaderBuffer, 0, 1);
                    } while (this.readHeaderBuffer[0] != 0x02);

                    this.spi.Read(this.readHeaderBuffer);

                    var status = this.readHeaderBuffer[0];
                    var ind = this.readHeaderBuffer[1];
                    var payloadLength = (this.readHeaderBuffer[3] << 8) | this.readHeaderBuffer[2];

                    lock (this.pendingReads) {
                        if (payloadLength > this.readPayloadBuffer.Length - this.nextRead)
                            throw new SPWF04SxBufferOverflowException("Internal read buffer overflowed.");

                        if (payloadLength > 0)
                            this.spi.Read(this.readPayloadBuffer, this.nextRead, payloadLength);

                        this.State = (SPWF04SxWiFiState)(status & 0b0000_1111);

                        switch ((status & 0b1111_0000) >> 4) {
                            case 0x01:
                                this.pendingEvents.Enqueue(new SPWF04SxIndicationReceivedEventArgs((SPWF04SxIndication)ind, this.PayloadToString(this.nextRead, payloadLength)));
                                break;

                            case 0x02:
                                this.pendingEvents.Enqueue(new SPWF04SxErrorReceivedEventArgs(ind, this.PayloadToString(this.nextRead, payloadLength)));
                                break;

                            case 0x03:
                                this.pendingReads.Enqueue(this.nextRead);
                                this.nextRead += payloadLength;
                                this.pendingReads.Enqueue(this.nextRead);
                                break;

                            default:
                                throw new SPWF04SxException("Unexpected message kind");
                        }
                    }
                }

                while (this.pendingEvents.Count != 0) {
                    switch (this.pendingEvents.Dequeue()) {
                        case SPWF04SxIndicationReceivedEventArgs e: this.IndicationReceived?.Invoke(this, e); break;
                        case SPWF04SxErrorReceivedEventArgs e: this.ErrorReceived?.Invoke(this, e); break;
                    }
                }

                Thread.Sleep(1);
            }
        }

        private string PayloadToString(int start, int length) => Encoding.UTF8.GetString(this.readPayloadBuffer, start, length);

        private int GetInternalSocketId(int socket) => this.sockets.Contains(socket) ? (int)this.sockets[socket] : throw new ArgumentException();

        private void GetAddress(SocketAddress address, out string host, out int port) {
            port = 0;
            port |= (byte)(address[2] << 8);
            port |= (byte)(address[3] << 0);

            host = "";
            host += (address[4] << 00) + ".";
            host += (address[5] << 08) + ".";
            host += (address[6] << 16) + ".";
            host += (address[7] << 24);
        }

        int ISocket.Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
            if (addressFamily != AddressFamily.InterNetwork || socketType != SocketType.Stream || protocolType != ProtocolType.Tcp) throw new ArgumentException();

            var id = this.nextSocketId++;

            this.sockets.Add(id, 0);

            return id;
        }

        void ISocket.Close(int socket) => this.CloseSocket(this.GetInternalSocketId(socket));
        int ISocket.Available(int socket) => this.QuerySocket(this.GetInternalSocketId(socket));

        void ISocket.Connect(int socket, SocketAddress address) {
            if (!this.sockets.Contains(socket)) throw new ArgumentException();
            if (address.Family != AddressFamily.InterNetwork) throw new ArgumentException();

            this.GetAddress(address, out var host, out var port);

            this.sockets[socket] = this.OpenSocket(host, port, SPWF04SxConnectionyType.Tcp, this.ForceSocketsTls ? SPWF04SxConnectionSecurityType.Tls : SPWF04SxConnectionSecurityType.None, this.ForceSocketsTls ? this.ForceSocketsTlsCommonName : null);
        }


        int ISocket.Send(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout) {
            if (flags != SocketFlags.None) throw new ArgumentException();
            if (timeout != Timeout.Infinite) throw new ArgumentException();

            this.WriteSocket(this.GetInternalSocketId(socket), buffer, offset, count);

            return count;
        }

        int ISocket.Receive(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout) {
            if (flags != SocketFlags.None) throw new ArgumentException();
            if (timeout != Timeout.Infinite) throw new ArgumentException();

            return this.ReadSocket(this.GetInternalSocketId(socket), buffer, offset, count);
        }

        bool ISocket.Poll(int socket, int microSeconds, SelectMode mode) => throw new NotImplementedException();
        void ISocket.Bind(int socket, SocketAddress address) => throw new NotImplementedException();
        void ISocket.Listen(int socket, int backlog) => throw new NotImplementedException();
        int ISocket.Accept(int socket) => throw new NotImplementedException();
        int ISocket.SendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, SocketAddress address) => throw new NotImplementedException();
        int ISocket.ReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, ref SocketAddress address) => throw new NotImplementedException();
        void ISocket.GetRemoteAddress(int socket, out SocketAddress address) => throw new NotImplementedException();
        void ISocket.GetLocalAddress(int socket, out SocketAddress address) => throw new NotImplementedException();
        void ISocket.GetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) => throw new NotImplementedException();
        void ISocket.SetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) => throw new NotImplementedException();

        void IDns.GetHostByName(string name, out string canonicalName, out SocketAddress[] addresses) {
            this.AddParameterToCommand(name);
            this.AddParameterToCommand("80");
            this.AddParameterToCommand(null);
            this.AddParameterToCommand("t");
            this.SendCommand(SPWF04SxCommandIds.SOCKON);

            var result = this.ReadString().Split(':');

            this.ReadBuffer();

            var socket = result[0] == "On" ? int.Parse(result[2]) : throw new Exception("Request failed");

            this.CloseSocket(socket);

            canonicalName = "";
            addresses = new[] { new IPEndPoint(IPAddress.Parse(result[1]), 80).Serialize() };
        }

        public override string Id => nameof(SPWF04Sx);
        public override string Name => this.Id;
        public override string Description => string.Empty;
        public override OperationalStatus OperationalStatus => this.State == SPWF04SxWiFiState.ReadyToTransmit ? OperationalStatus.Up : OperationalStatus.Down;
        public override bool IsReceiveOnly => false;
        public override bool SupportsMulticast => false;
        public override NetworkInterfaceType NetworkInterfaceType => NetworkInterfaceType.Wireless80211;

        public override bool Supports(NetworkInterfaceComponent networkInterfaceComponent) => networkInterfaceComponent == NetworkInterfaceComponent.IPv4;
    }
}
