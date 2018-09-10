using System;
using System.Collections;
using System.Net;
using System.Net.NetworkInterface;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Net.NetworkInterface;
using GHIElectronics.TinyCLR.Networking.SPWF04Sx.Helpers;

namespace GHIElectronics.TinyCLR.Networking.SPWF04Sx {
    public class SPWF04SxInterface : NetworkInterface, ISocketProvider, ISslStreamProvider, IDnsProvider, IDisposable {
        private readonly ObjectPool commandPool;
        private readonly Hashtable netifSockets;
        private readonly Queue pendingCommands;
        private readonly ReadWriteBuffer readPayloadBuffer;
        private readonly SpiDevice spi;
        private readonly GpioPin irq;
        private readonly GpioPin reset;
        private SPWF04SxCommand activeCommand;
        private SPWF04SxCommand activeHttpCommand;
        private Thread worker;
        private bool running;
        private int nextSocketId;

        public event SPWF04SxIndicationReceivedEventHandler IndicationReceived;
        public event SPWF04SxErrorReceivedEventHandler ErrorReceived;

        public SPWF04SxWiFiState State { get; private set; }
        public bool ForceSocketsTls { get; set; }
        public string ForceSocketsTlsCommonName { get; set; }

        public static SpiConnectionSettings GetConnectionSettings(SpiChipSelectType chipSelectType, int chipSelectLine) => new SpiConnectionSettings {
            ClockFrequency = 4000000,
            Mode = SpiMode.Mode0,
            DataBitLength = 8,
            ChipSelectType = chipSelectType,
            ChipSelectLine = chipSelectLine
        };

        public SPWF04SxInterface(SpiDevice spi, GpioPin irq, GpioPin reset) {
            this.commandPool = new ObjectPool(() => new SPWF04SxCommand());
            this.netifSockets = new Hashtable();
            this.pendingCommands = new Queue();
            this.readPayloadBuffer = new ReadWriteBuffer(32, 1500 + 512);
            this.spi = spi;
            this.irq = irq;
            this.reset = reset;

            this.State = SPWF04SxWiFiState.RadioTerminatedByUser;

            this.reset.SetDriveMode(GpioPinDriveMode.Output);
            this.reset.Write(GpioPinValue.Low);

            this.irq.SetDriveMode(GpioPinDriveMode.Input);

            NetworkInterface.RegisterNetworkInterface(this);
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

                NetworkInterface.DeregisterNetworkInterface(this);
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

            this.pendingCommands.Clear();
            this.readPayloadBuffer.Reset();

            this.netifSockets.Clear();
            this.nextSocketId = 0;
            this.activeCommand = null;
            this.activeHttpCommand = null;

            this.commandPool.ResetAll();
        }

        protected SPWF04SxCommand GetCommand() => (SPWF04SxCommand)this.commandPool.Acquire();

        protected void EnqueueCommand(SPWF04SxCommand cmd) {
            lock (this.pendingCommands) {
                this.pendingCommands.Enqueue(cmd);

                this.ReadyNextCommand();
            }
        }

        protected void FinishCommand(SPWF04SxCommand cmd) {
            if (this.activeCommand != cmd) throw new ArgumentException();

            lock (this.pendingCommands) {
                cmd.Reset();

                this.commandPool.Release(cmd);

                this.ReadyNextCommand();
            }
        }

        private void ReadyNextCommand() {
            lock (this.pendingCommands) {
                if (this.pendingCommands.Count != 0) {
                    var cmd = (SPWF04SxCommand)this.pendingCommands.Dequeue();
                    cmd.SetPayloadBuffer(this.readPayloadBuffer);
                    this.activeCommand = cmd;
                }
                else {
                    this.activeCommand = null;
                }
            }
        }

        public void ClearTlsServerRootCertificate() {
            var cmd = this.GetCommand()
                .AddParameter("content")
                .AddParameter("2")
                .Finalize(SPWF04SxCommandIds.TLSCERT);

            this.EnqueueCommand(cmd);

            cmd.ReadBuffer();
            cmd.ReadBuffer();
            this.FinishCommand(cmd);
        }

        public string SetTlsServerRootCertificate(byte[] certificate) {
            if (certificate == null) throw new ArgumentNullException();

            var cmd = this.GetCommand()
                .AddParameter("ca")
                .AddParameter(certificate.Length.ToString())
                .Finalize(SPWF04SxCommandIds.TLSCERT, certificate, 0, certificate.Length);

            this.EnqueueCommand(cmd);

            var result = cmd.ReadString();

            cmd.ReadBuffer();

            this.FinishCommand(cmd);

            return result.Substring(result.IndexOf(':') + 1);
        }

        public int SendHttpGet(string host, string path, int port, SPWF04SxConnectionSecurityType connectionSecurity) {
            if (this.activeHttpCommand != null) throw new InvalidOperationException();

            this.activeHttpCommand = this.GetCommand()
                .AddParameter(host)
                .AddParameter(path)
                .AddParameter(port.ToString())
                .AddParameter(connectionSecurity == SPWF04SxConnectionSecurityType.None ? "0" : "2")
                .AddParameter(null)
                .AddParameter(null)
                .AddParameter(null)
                .AddParameter(null)
                .Finalize(SPWF04SxCommandIds.HTTPGET);

            this.EnqueueCommand(this.activeHttpCommand);

            var result = this.activeHttpCommand.ReadString();
            if (connectionSecurity == SPWF04SxConnectionSecurityType.Tls && result == string.Empty) {
                result = this.activeHttpCommand.ReadString();

                if (result.IndexOf("Loading:") == 0)
                    result = this.activeHttpCommand.ReadString();
            }

            return result.Split(':') is var parts && parts[0] == "Http Server Status Code" ? int.Parse(parts[1]) : throw new Exception($"Request failed: {result}");
        }

        //TODO Need to test on an actual server
        public int SendHttpPost(string host, string path, int port, SPWF04SxConnectionSecurityType connectionSecurity) {
            if (this.activeHttpCommand != null) throw new InvalidOperationException();

            this.activeHttpCommand = this.GetCommand()
                .AddParameter(host)
                .AddParameter(path)
                .AddParameter(port.ToString())
                .AddParameter(connectionSecurity == SPWF04SxConnectionSecurityType.None ? "0" : "2")
                .AddParameter(null)
                .AddParameter(null)
                .AddParameter(null)
                .AddParameter(null)
                .Finalize(SPWF04SxCommandIds.HTTPPOST);

            this.EnqueueCommand(this.activeHttpCommand);

            var result = this.activeHttpCommand.ReadString();
            if (connectionSecurity == SPWF04SxConnectionSecurityType.Tls && result == string.Empty) {
                result = this.activeHttpCommand.ReadString();

                if (result.IndexOf("Loading:") == 0)
                    result = this.activeHttpCommand.ReadString();
            }

            return result.Split(':') is var parts && parts[0] == "Http Server Status Code" ? int.Parse(parts[1]) : throw new Exception($"Request failed: {result}");
        }

        public int ReadHttpResponse(byte[] buffer, int offset, int count) {
            if (this.activeHttpCommand == null) throw new InvalidOperationException();

            var len = this.activeHttpCommand.ReadBuffer(buffer, offset, count);

            if (len == 0) {
                this.FinishCommand(this.activeHttpCommand);

                this.activeHttpCommand = null;
            }

            return len;
        }

        public int OpenSocket(string host, int port, SPWF04SxConnectionyType connectionType, SPWF04SxConnectionSecurityType connectionSecurity, string commonName = null) {
            var cmd = this.GetCommand()
                .AddParameter(host)
                .AddParameter(port.ToString())
                .AddParameter(null)
                .AddParameter(commonName ?? (connectionType == SPWF04SxConnectionyType.Tcp ? (connectionSecurity == SPWF04SxConnectionSecurityType.Tls ? "s" : "t") : "u"))
                .Finalize(SPWF04SxCommandIds.SOCKON);

            this.EnqueueCommand(cmd);

            var a = cmd.ReadString();
            var b = cmd.ReadString();

            if (connectionSecurity == SPWF04SxConnectionSecurityType.Tls && b.IndexOf("Loading:") == 0) {
                a = cmd.ReadString();
                b = cmd.ReadString();
            }

            this.FinishCommand(cmd);

            return a.Split(':') is var result && result[0] == "On" ? int.Parse(result[2]) : throw new Exception("Request failed");
        }

        public void CloseSocket(int socket) {
            var cmd = this.GetCommand()
                .AddParameter(socket.ToString())
                .Finalize(SPWF04SxCommandIds.SOCKC);

            this.EnqueueCommand(cmd);

            cmd.ReadBuffer();

            this.FinishCommand(cmd);
        }

        public void WriteSocket(int socket, byte[] data) => this.WriteSocket(socket, data, 0, data != null ? data.Length : throw new ArgumentNullException(nameof(data)));

        public void WriteSocket(int socket, byte[] data, int offset, int count) {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (offset < 0) throw new ArgumentOutOfRangeException();
            if (count < 0) throw new ArgumentOutOfRangeException();
            if (offset + count > data.Length) throw new ArgumentOutOfRangeException();

            var cmd = this.GetCommand()
                .AddParameter(socket.ToString())
                .AddParameter(count.ToString())
                .Finalize(SPWF04SxCommandIds.SOCKW, data, offset, count);

            this.EnqueueCommand(cmd);

            cmd.ReadBuffer();

            this.FinishCommand(cmd);
        }

        public int ReadSocket(int socket, byte[] buffer, int offset, int count) {
            var cmd = this.GetCommand()
                .AddParameter(socket.ToString())
                .AddParameter(count.ToString())
                .Finalize(SPWF04SxCommandIds.SOCKR);

            this.EnqueueCommand(cmd);

            cmd.ReadBuffer();

            var current = 0;
            var total = 0;
            do {
                current = cmd.ReadBuffer(buffer, offset + total, count - total);
                total += current;
            } while (current != 0);

            this.FinishCommand(cmd);

            return total;
        }

        public int QuerySocket(int socket) {
            var cmd = this.GetCommand()
                .AddParameter(socket.ToString())
                .Finalize(SPWF04SxCommandIds.SOCKQ);

            this.EnqueueCommand(cmd);

            var result = cmd.ReadString().Split(':');

            cmd.ReadBuffer();

            this.FinishCommand(cmd);

            return result[0] == "Query" ? int.Parse(result[1]) : throw new Exception("Request failed");
        }

        public string ListSocket() {
            var cmd = this.GetCommand()
                .Finalize(SPWF04SxCommandIds.SOCKL);

            this.EnqueueCommand(cmd);

            var str = string.Empty;
            while (cmd.ReadString() is var s && s != string.Empty)
                str += s + Environment.NewLine;

            cmd.ReadBuffer();

            this.FinishCommand(cmd);

            return str;
        }

        public void EnableRadio() {
            var cmd = this.GetCommand()
                .AddParameter("1")
                .Finalize(SPWF04SxCommandIds.WIFI);

            this.EnqueueCommand(cmd);

            cmd.ReadBuffer();

            this.FinishCommand(cmd);
        }

        public void DisableRadio() {
            var cmd = this.GetCommand()
                .AddParameter("0")
                .Finalize(SPWF04SxCommandIds.WIFI);

            this.EnqueueCommand(cmd);

            cmd.ReadBuffer();

            this.FinishCommand(cmd);
        }

        public void JoinNetwork(string ssid, string password) {
            this.DisableRadio();

            var cmd = this.GetCommand()
                .AddParameter("wifi_mode")
                .AddParameter("1")
                .Finalize(SPWF04SxCommandIds.SCFG);
            this.EnqueueCommand(cmd);
            cmd.ReadBuffer();
            this.FinishCommand(cmd);

            cmd = this.GetCommand()
                .AddParameter("wifi_priv_mode")
                .AddParameter("2")
                .Finalize(SPWF04SxCommandIds.SCFG);
            this.EnqueueCommand(cmd);
            cmd.ReadBuffer();
            this.FinishCommand(cmd);

            cmd = this.GetCommand()
                .AddParameter("wifi_wpa_psk_text")
                .AddParameter(password)
                .Finalize(SPWF04SxCommandIds.SCFG);
            this.EnqueueCommand(cmd);
            cmd.ReadBuffer();
            this.FinishCommand(cmd);

            cmd = this.GetCommand()
                .AddParameter(ssid)
                .Finalize(SPWF04SxCommandIds.SSIDTXT);
            this.EnqueueCommand(cmd);
            cmd.ReadBuffer();
            this.FinishCommand(cmd);

            this.EnableRadio();

            cmd = this.GetCommand()
                .Finalize(SPWF04SxCommandIds.WCFG);
            this.EnqueueCommand(cmd);
            cmd.ReadBuffer();
            this.FinishCommand(cmd);
        }

        private void Process() {
            var pendingEvents = new Queue();
            var windPayloadBuffer = new GrowableBuffer(32, 1500 + 512);
            var readHeaderBuffer = new byte[4];
            var syncRead = new byte[1];
            var syncWrite = new byte[1];

            while (this.running) {
                var hasWrite = this.activeCommand != null && !this.activeCommand.Sent;
                var hasIrq = this.irq.Read() == GpioPinValue.Low;

                if (hasIrq || hasWrite) {
                    syncWrite[0] = (byte)(!hasIrq && hasWrite ? 0x02 : 0x00);

                    this.spi.TransferFullDuplex(syncWrite, syncRead);

                    if (!hasIrq && hasWrite && syncRead[0] != 0x02) {
                        this.activeCommand.WriteHeader(this.spi.Write);

                        if (this.activeCommand.HasWritePayload) {
                            while (this.irq.Read() == GpioPinValue.High)
                                Thread.Sleep(0);

                            this.activeCommand.WritePayload(this.spi.Write);

                            while (this.irq.Read() == GpioPinValue.Low)
                                Thread.Sleep(0);
                        }

                        this.activeCommand.Sent = true;
                    }
                    else if (syncRead[0] == 0x02) {
                        this.spi.Read(readHeaderBuffer);

                        var status = readHeaderBuffer[0];
                        var ind = readHeaderBuffer[1];
                        var payloadLength = (readHeaderBuffer[3] << 8) | readHeaderBuffer[2];
                        var type = (status & 0b1111_0000) >> 4;

                        this.State = (SPWF04SxWiFiState)(status & 0b0000_1111);

                        if (type == 0x01 || type == 0x02) {
                            if (payloadLength > 0) {
                                windPayloadBuffer.EnsureSize(payloadLength, false);

                                this.spi.Read(windPayloadBuffer.Data, 0, payloadLength);
                            }

                            var str = Encoding.UTF8.GetString(windPayloadBuffer.Data, 0, payloadLength);

                            pendingEvents.Enqueue(type == 0x01 ? new SPWF04SxIndicationReceivedEventArgs((SPWF04SxIndication)ind, str) : (object)new SPWF04SxErrorReceivedEventArgs(ind, str));
                        }
                        else {
                            if (this.activeCommand == null || !this.activeCommand.Sent) throw new InvalidOperationException("Unexpected payload.");

                            this.activeCommand.ReadPayload(this.spi.Read, payloadLength);
                        }
                    }
                }
                else {
                    while (pendingEvents.Count != 0) {
                        switch (pendingEvents.Dequeue()) {
                            case SPWF04SxIndicationReceivedEventArgs e: this.IndicationReceived?.Invoke(this, e); break;
                            case SPWF04SxErrorReceivedEventArgs e: this.ErrorReceived?.Invoke(this, e); break;
                        }
                    }
                }

                Thread.Sleep(0);
            }
        }

        private int GetInternalSocketId(int socket) => this.netifSockets.Contains(socket) ? (int)this.netifSockets[socket] : throw new ArgumentException();

        private void GetAddress(SocketAddress address, out string host, out int port) {
            port = 0;
            port |= address[2] << 8;
            port |= address[3] << 0;

            host = "";
            host += address[4] + ".";
            host += address[5] + ".";
            host += address[6] + ".";
            host += address[7];
        }

        int ISocketProvider.Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
            if (addressFamily != AddressFamily.InterNetwork || socketType != SocketType.Stream || protocolType != ProtocolType.Tcp) throw new ArgumentException();

            var id = this.nextSocketId++;

            this.netifSockets.Add(id, 0);

            return id;
        }

        int ISocketProvider.Available(int socket) => this.QuerySocket(this.GetInternalSocketId(socket));

        void ISocketProvider.Close(int socket) {
            this.CloseSocket(this.GetInternalSocketId(socket));

            this.netifSockets.Remove(socket);
        }

        void ISocketProvider.Connect(int socket, SocketAddress address) {
            if (!this.netifSockets.Contains(socket)) throw new ArgumentException();
            if (address.Family != AddressFamily.InterNetwork) throw new ArgumentException();

            this.GetAddress(address, out var host, out var port);

            this.netifSockets[socket] = this.OpenSocket(host, port, SPWF04SxConnectionyType.Tcp, this.ForceSocketsTls ? SPWF04SxConnectionSecurityType.Tls : SPWF04SxConnectionSecurityType.None, this.ForceSocketsTls ? this.ForceSocketsTlsCommonName : null);
        }

        int ISocketProvider.Send(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout) {
            if (flags != SocketFlags.None) throw new ArgumentException();

            this.WriteSocket(this.GetInternalSocketId(socket), buffer, offset, count);

            return count;
        }

        int ISocketProvider.Receive(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout) {
            if (flags != SocketFlags.None) throw new ArgumentException();
            if (timeout != Timeout.Infinite && timeout < 0) throw new ArgumentException();

            var end = (timeout != Timeout.Infinite ? DateTime.UtcNow.AddMilliseconds(timeout) : DateTime.MaxValue).Ticks;
            var sock = this.GetInternalSocketId(socket);
            var avail = 0;

            do {
                avail = this.QuerySocket(sock);

                Thread.Sleep(1);
            } while (avail == 0 && DateTime.UtcNow.Ticks < end);

            return avail > 0 ? this.ReadSocket(sock, buffer, offset, Math.Min(avail, count)) : 0;
        }

        bool ISocketProvider.Poll(int socket, int microSeconds, SelectMode mode) {
            switch (mode) {
                default: throw new ArgumentException();
                case SelectMode.SelectError: return false;
                case SelectMode.SelectWrite: return true;
                case SelectMode.SelectRead: return this.QuerySocket(this.GetInternalSocketId(socket)) != 0;
            }
        }

        void ISocketProvider.Bind(int socket, SocketAddress address) => throw new NotImplementedException();
        void ISocketProvider.Listen(int socket, int backlog) => throw new NotImplementedException();
        int ISocketProvider.Accept(int socket) => throw new NotImplementedException();
        int ISocketProvider.SendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, SocketAddress address) => throw new NotImplementedException();
        int ISocketProvider.ReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, ref SocketAddress address) => throw new NotImplementedException();

        void ISocketProvider.GetRemoteAddress(int socket, out SocketAddress address) => address = new SocketAddress(AddressFamily.InterNetwork, 16);
        void ISocketProvider.GetLocalAddress(int socket, out SocketAddress address) => address = new SocketAddress(AddressFamily.InterNetwork, 16);

        void ISocketProvider.GetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) {
            if (optionLevel == SocketOptionLevel.Socket && optionName == SocketOptionName.Type)
                Array.Copy(BitConverter.GetBytes((int)SocketType.Stream), optionValue, 4);
        }

        void ISocketProvider.SetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) {

        }

        int ISslStreamProvider.AuthenticateAsClient(int socketHandle, string targetHost, X509Certificate certificate, SslProtocols[] sslProtocols) => socketHandle;
        int ISslStreamProvider.AuthenticateAsServer(int socketHandle, X509Certificate certificate, SslProtocols[] sslProtocols) => throw new NotImplementedException();
        void ISslStreamProvider.Close(int handle) => ((ISocketProvider)this).Close(handle);
        int ISslStreamProvider.Read(int handle, byte[] buffer, int offset, int count, int timeout) => ((ISocketProvider)this).Receive(handle, buffer, offset, count, SocketFlags.None, timeout);
        int ISslStreamProvider.Write(int handle, byte[] buffer, int offset, int count, int timeout) => ((ISocketProvider)this).Send(handle, buffer, offset, count, SocketFlags.None, timeout);
        int ISslStreamProvider.Available(int handle) => ((ISocketProvider)this).Available(handle);

        void IDnsProvider.GetHostByName(string name, out string canonicalName, out SocketAddress[] addresses) {
            var cmd = this.GetCommand()
                .AddParameter(name)
                .AddParameter("80")
                .AddParameter(null)
                .AddParameter("t")
                .Finalize(SPWF04SxCommandIds.SOCKON);

            this.EnqueueCommand(cmd);

            var result = cmd.ReadString().Split(':');

            cmd.ReadBuffer();

            this.FinishCommand(cmd);

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
