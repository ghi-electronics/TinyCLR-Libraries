using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Spi;

namespace GHIElectronics.TinyCLR.Networking.SPWF04Sx {
    public class SPWF04SxInterface : IDisposable {
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

        public event SPWF04SxIndicationReceivedEventHandler IndicationReceived;
        public event SPWF04SxErrorReceivedEventHandler ErrorReceived;

        public SPWF04SxWiFiState State { get; private set; }

        public static SpiConnectionSettings GetConnectionSettings(int chipSelectLine) => new SpiConnectionSettings(chipSelectLine) {
            ClockFrequency = 4000000,
            Mode = SpiMode.Mode0,
            SharingMode = SpiSharingMode.Exclusive,
            DataBitLength = 8
        };

        public SPWF04SxInterface(SpiDevice spi, GpioPin irq, GpioPin reset) {
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
            this.AddParameterToCommand("ca");
            this.AddParameterToCommand(certificate.Length.ToString());
            this.SendCommand(SPWF04SxCommandIds.TLSCERT, certificate);

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

        public void WriteSocket(int socket, byte[] data) {
            this.AddParameterToCommand(socket.ToString());
            this.AddParameterToCommand(data.Length.ToString());
            this.SendCommand(SPWF04SxCommandIds.SOCKW, data);
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

        protected void SendCommand(SPWF04SxCommandIds cmdId) => this.SendCommand(cmdId, null);

        protected void SendCommand(SPWF04SxCommandIds cmdId, byte[] rawData) {
            if (this.pendingWriteLength != 0)
                throw new InvalidOperationException("Previous write not finished");

            var rawDataLength = rawData != null ? rawData.Length : 0;
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

            var len = idx + rawDataLength - 3;
            this.writeCommandBuffer[1] = (byte)((len >> 8) & 0xFF);
            this.writeCommandBuffer[2] = (byte)((len >> 0) & 0xFF);

            this.validParameters = 0;

            this.pendingRawData = rawData;
            this.pendingRawDataOffset = 0;
            this.pendingRawDataLength = rawDataLength;
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

        private string PayloadToString(int start, int length) => new string(Encoding.UTF8.GetChars(this.readPayloadBuffer, start, length));
    }
}
