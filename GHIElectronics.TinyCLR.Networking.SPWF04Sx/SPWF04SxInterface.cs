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
    internal delegate object PoolObjectCreator();

    internal class Pool {
        private readonly ArrayList all = new ArrayList();
        private readonly Stack available = new Stack();
        private readonly PoolObjectCreator creator;

        public Pool(PoolObjectCreator creator) => this.creator = creator;

        public object Acquire() {
            lock (this.available) {
                if (this.available.Count == 0) {
                    var obj = this.creator();

                    this.all.Add(obj);

                    return obj;
                }

                return this.available.Pop();
            }
        }

        public void Release(object obj) {
            lock (this.available) {
                if (!this.all.Contains(obj)) throw new ArgumentException();

                this.available.Push(obj);
            }
        }

        public void ResetAll() {
            lock (this.available) {
                this.available.Clear();

                foreach (var obj in this.all)
                    this.available.Push(obj);
            }
        }
    }

    internal class GrowableBuffer {
        private byte[] buffer;

        public byte[] Data => this.buffer;
        public int CurrentSize => this.buffer.Length;
        public int RemainingSize => this.MaxSize - this.CurrentSize;
        public int MaxSize { get; }

        public GrowableBuffer(int startSize) : this(startSize, int.MaxValue) { }

        public GrowableBuffer(int startSize, int maxSize) {
            this.MaxSize = maxSize;

            if (!this.TryGrow(false, startSize))
                throw new ArgumentException();
        }

        public void EnsureSize(int size, bool copy) {
            if (size > this.CurrentSize && !this.TryGrow(copy, size + Math.Min(this.RemainingSize, 100)))
                throw new Exception("Buffer size exceeded max.");
        }

        public bool TryGrow(bool copy) => this.RemainingSize > 0 && this.TryGrow(copy, this.CurrentSize + Math.Min(this.RemainingSize, 100));

        public bool TryGrow(bool copy, int size) {
            if (size >= this.MaxSize)
                return false;

            var newBuffer = new byte[size];

            if (copy)
                Array.Copy(this.buffer, newBuffer, this.CurrentSize);

            this.buffer = newBuffer;

            return true;
        }
    }

    internal class ReadWriteBuffer {
        private readonly ManualResetEvent writeWaiter = new ManualResetEvent(false);
        private readonly GrowableBuffer buffer;
        private int nextRead;
        private int nextWrite;

        public byte[] Data => this.buffer.Data;

        public int AvailableWrite { get { lock (this) return this.buffer.CurrentSize - this.nextWrite; } }
        public int AvailableRead { get { lock (this) return this.nextWrite - this.nextRead; } }

        public int WriteOffset => this.nextWrite;
        public int ReadOffset => this.nextRead;

        public ReadWriteBuffer(int size, int maxSize) => this.buffer = new GrowableBuffer(size, maxSize);

        public void WaitForWriteSpace(int desired) {
            while (true) {
                lock (this) {
                    if (this.AvailableWrite < desired)
                        this.buffer.TryGrow(this.nextRead != 0 || this.nextWrite != 0, desired + this.nextWrite);

                    if (this.AvailableWrite != 0) {
                        this.writeWaiter.Reset();

                        return;
                    }

                    if (this.nextRead != 0) {
                        Array.Copy(this.buffer.Data, this.nextRead, this.buffer.Data, 0, this.nextWrite - this.nextRead);

                        this.nextWrite -= this.nextRead;
                        this.nextRead = 0;

                        continue;
                    }
                }

                this.writeWaiter.WaitOne();
            }
        }

        public void Write(int count) {
            lock (this)
                this.nextWrite += count;
        }

        public void Read(int count) {
            lock (this) {
                this.nextRead += count;
                this.writeWaiter.Set();
            }
        }

        public void Reset() {
            this.writeWaiter.Reset();
            this.nextWrite = 0;
            this.nextRead = 0;
        }
    }

    internal class Semaphore : WaitHandle {
        private readonly object lck = new object();
        private readonly ManualResetEvent evt = new ManualResetEvent(false);
        private int count;

        public override bool WaitOne() {
            while (true) {
                this.evt.WaitOne();

                lock (this.lck) {
                    if (this.count > 0) {
                        if (--this.count == 0)
                            this.evt.Reset();

                        return true;
                    }
                }
            }
        }

        public int Release() {
            lock (this.lck) {
                var cnt = this.count;

                this.count++;

                this.evt.Set();

                return cnt;
            }
        }
    }

    public sealed class SPWF04SxCommand {
        private readonly string[] parameters = new string[16];
        private readonly Queue pendingReads = new Queue();
        private readonly Semaphore pendingReadsSemaphore = new Semaphore();
        private readonly GrowableBuffer writeHeader = new GrowableBuffer(4, 512);
        private ReadWriteBuffer readPayload;
        private int parameterCount;
        private int partialRead;
        private int writeHeaderLength;
        private byte[] writePayload;
        private int writePayloadOffset;
        private int writePayloadLength;

        internal delegate void DataReaderWriter(byte[] buffer, int offset, int count);

        internal SPWF04SxCommand() { }

        internal bool Sent { get; set; }
        internal bool HasWritePayload => this.writePayloadLength > 0;

        public SPWF04SxCommand AddParameter(string parameter) {
            this.parameters[this.parameterCount++] = parameter;

            return this;
        }

        public SPWF04SxCommand Finalize(SPWF04SxCommandIds cmdId) => this.Finalize(cmdId, null, 0, 0);

        public SPWF04SxCommand Finalize(SPWF04SxCommandIds cmdId, byte[] rawData, int rawDataOffset, int rawDataCount) {
            if (rawData == null && rawDataCount != 0) throw new ArgumentException();
            if (rawDataOffset < 0) throw new ArgumentOutOfRangeException();
            if (rawDataCount < 0) throw new ArgumentOutOfRangeException();
            if (rawData != null && rawDataOffset + rawDataCount > rawData.Length) throw new ArgumentOutOfRangeException();

            var required = 4 + this.parameterCount;

            for (var i = 0; i < this.parameterCount; i++) {
                var p = this.parameters[i];

                required += p != null ? p.Length : 0;
            }

            this.writeHeader.EnsureSize(required, false);

            var idx = 0;
            var buf = this.writeHeader.Data;

            buf[idx++] = 0x00;
            buf[idx++] = 0x00;

            buf[idx++] = (byte)cmdId;
            buf[idx++] = (byte)this.parameterCount;

            for (var i = 0; i < this.parameterCount; i++) {
                var p = this.parameters[i];
                var pLen = p != null ? p.Length : 0;

                buf[idx++] = (byte)pLen;

                if (!string.IsNullOrEmpty(p))
                    Encoding.UTF8.GetBytes(p, 0, pLen, buf, idx);

                idx += pLen;
            }

            var len = idx + rawDataCount - 2;
            buf[0] = (byte)((len >> 8) & 0xFF);
            buf[1] = (byte)((len >> 0) & 0xFF);

            this.writePayload = rawData;
            this.writePayloadOffset = rawDataOffset;
            this.writePayloadLength = rawDataCount;
            this.writeHeaderLength = idx;

            return this;
        }

        public string ReadString() {
            this.pendingReadsSemaphore.WaitOne();

            lock (this.pendingReads) {
                var start = this.readPayload.ReadOffset;
                var len = (int)this.pendingReads.Dequeue();
                var res = Encoding.UTF8.GetString(this.readPayload.Data, start, len);

                this.readPayload.Read(len);

                return res;
            }
        }

        public int ReadBuffer() => this.ReadBuffer(null, 0, 0);

        public int ReadBuffer(byte[] buffer, int offset, int count) {
            this.pendingReadsSemaphore.WaitOne();

            lock (this.pendingReads) {
                var len = 0;

                if (buffer != null) {
                    len = (int)this.pendingReads.Peek() - this.partialRead;

                    if (len <= count) {
                        Array.Copy(this.readPayload.Data, this.readPayload.ReadOffset, buffer, offset, len);

                        this.partialRead = 0;

                        this.pendingReads.Dequeue();
                    }
                    else {
                        len = count;

                        Array.Copy(this.readPayload.Data, this.readPayload.ReadOffset, buffer, offset, count);

                        this.partialRead += count;
                    }
                }
                else {
                    len = (int)this.pendingReads.Dequeue();
                }

                this.readPayload.Read(len);

                return len;
            }
        }

        internal void ReadPayload(DataReaderWriter reader, int count) {
            var remaining = count;

            while (remaining > 0) {
                this.readPayload.WaitForWriteSpace(remaining);

                var actual = Math.Min(remaining, this.readPayload.AvailableWrite);

                reader(this.readPayload.Data, this.readPayload.WriteOffset, actual);

                this.readPayload.Write(actual);

                remaining -= actual;
            }

            lock (this.pendingReads) {
                this.pendingReads.Enqueue(count);
                this.pendingReadsSemaphore.Release();
            }
        }

        internal void WriteHeader(DataReaderWriter reader) => reader(this.writeHeader.Data, 0, this.writeHeaderLength);
        internal void WritePayload(DataReaderWriter reader) => reader(this.writePayload, this.writePayloadOffset, this.writePayloadLength);

        internal void Reset() {
            lock (this.pendingReads)
                if (!this.Sent || this.pendingReads.Count != 0)
                    throw new Exception("Not complete");

            this.Sent = false;
            this.parameterCount = 0;
            this.writeHeaderLength = 0;
            this.writePayloadOffset = 0;
            this.writePayloadLength = 0;

            this.readPayload.Reset();
            this.readPayload = null;
        }

        internal void SetPayloadBuffer(ReadWriteBuffer buffer) => this.readPayload = buffer;
    }

    public class SPWF04SxInterface : NetworkInterface, ISocket, IDns, IDisposable {
        private readonly Pool commandPool;
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

        public static SpiConnectionSettings GetConnectionSettings(int chipSelectLine) => new SpiConnectionSettings(chipSelectLine) {
            ClockFrequency = 4000000,
            Mode = SpiMode.Mode0,
            SharingMode = SpiSharingMode.Exclusive,
            DataBitLength = 8
        };

        public SPWF04SxInterface(SpiDevice spi, GpioPin irq, GpioPin reset) {
            this.commandPool = new Pool(() => new SPWF04SxCommand());
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

        int ISocket.Create(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) {
            if (addressFamily != AddressFamily.InterNetwork || socketType != SocketType.Stream || protocolType != ProtocolType.Tcp) throw new ArgumentException();

            var id = this.nextSocketId++;

            this.netifSockets.Add(id, 0);

            return id;
        }

        int ISocket.Available(int socket) => this.QuerySocket(this.GetInternalSocketId(socket));

        void ISocket.Close(int socket) {
            this.CloseSocket(this.GetInternalSocketId(socket));

            this.netifSockets.Remove(socket);
        }

        void ISocket.Connect(int socket, SocketAddress address) {
            if (!this.netifSockets.Contains(socket)) throw new ArgumentException();
            if (address.Family != AddressFamily.InterNetwork) throw new ArgumentException();

            this.GetAddress(address, out var host, out var port);

            this.netifSockets[socket] = this.OpenSocket(host, port, SPWF04SxConnectionyType.Tcp, this.ForceSocketsTls ? SPWF04SxConnectionSecurityType.Tls : SPWF04SxConnectionSecurityType.None, this.ForceSocketsTls ? this.ForceSocketsTlsCommonName : null);
        }

        int ISocket.Send(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout) {
            if (flags != SocketFlags.None) throw new ArgumentException();

            this.WriteSocket(this.GetInternalSocketId(socket), buffer, offset, count);

            return count;
        }

        int ISocket.Receive(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout) {
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

        bool ISocket.Poll(int socket, int microSeconds, SelectMode mode) {
            switch (mode) {
                default: throw new ArgumentException();
                case SelectMode.SelectError: return false;
                case SelectMode.SelectWrite: return true;
                case SelectMode.SelectRead: return this.QuerySocket(this.GetInternalSocketId(socket)) != 0;
            }
        }

        void ISocket.Bind(int socket, SocketAddress address) => throw new NotImplementedException();
        void ISocket.Listen(int socket, int backlog) => throw new NotImplementedException();
        int ISocket.Accept(int socket) => throw new NotImplementedException();
        int ISocket.SendTo(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, SocketAddress address) => throw new NotImplementedException();
        int ISocket.ReceiveFrom(int socket, byte[] buffer, int offset, int count, SocketFlags flags, int timeout, ref SocketAddress address) => throw new NotImplementedException();

        void ISocket.GetRemoteAddress(int socket, out SocketAddress address) => address = new SocketAddress(AddressFamily.InterNetwork, 16);
        void ISocket.GetLocalAddress(int socket, out SocketAddress address) => address = new SocketAddress(AddressFamily.InterNetwork, 16);

        void ISocket.GetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) {
            if (optionLevel == SocketOptionLevel.Socket && optionName == SocketOptionName.Type)
                Array.Copy(BitConverter.GetBytes((int)SocketType.Stream), optionValue, 4);
        }

        void ISocket.SetOption(int socket, SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue) {

        }

        void IDns.GetHostByName(string name, out string canonicalName, out SocketAddress[] addresses) {
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
