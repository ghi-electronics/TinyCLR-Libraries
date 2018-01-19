using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.SerialCommunication;

namespace GHIElectronics.TinyCLR.Networking.SPWF01Sx {
    public class SPWF04SxInterface {
        public void Reset() => this.SendATCommand("AT+S.RESET");

        public void ConnectWiFi(string network, string password) {
            this.StopWorker();

            this.SendATCommand("AT.S+WIFI=0");
            this.SendATCommand("AT+S.SCFG=wifi_mode,1");
            this.SendATCommand("AT+S.SCFG=wifi_priv_mode,2");
            this.SendATCommand("AT+S.SSIDTXT=" + network);
            this.SendATCommand("AT+S.SCFG=wifi_wpa_psk_text," + password);
            this.SendATCommand("AT.S+WIFI=1");
            this.SendATCommand("AT.S+WCFG");
            this.Reset();

            this.ExtractLine(out var status1);
            this.ExtractLine(out var status2);
            this.ExtractLine(out var status3);
            this.ExtractLine(out var status4);
            this.ExtractLine(out var status5);
            this.ExtractLine(out var status6);
            this.ExtractLine(out var status7);
            this.ExtractLine(out var status8);
            this.ExtractLine(out var status9);
            this.ExtractLine(out var status10);
            this.ExtractLine(out var status11);
            this.ExtractLine(out var status12);
            this.ExtractLine(out var status13);
            this.ExtractLine(out var status14);

            this.StartWorker();
        }

        public void ConfigureTls(byte[] certificate) {
            this.StopWorker();

            this.SendATCommand("AT+S.TLSCERT=content,1");
            this.SendATCommand($"AT+S.TLSCERT=ca," + certificate.Length);
            this.Write(certificate);

            this.ExtractLine(out var status1);
            this.ExtractLine(out var status2);
            this.ExtractLine(out var status3);
            this.ExtractLine(out var status4);

            this.StartWorker();
        }

        public string OpenSocket(string host, int port, char type) {
            this.StopWorker();

            this.SendATCommand($"AT+S.SOCKON=" + host + "," + port + ",," + type);

            this.ExtractLine(out _);
            this.ExtractLine(out var id);
            this.ExtractLine(out _);
            this.ExtractLine(out var status);

            this.StartWorker();

            if (status != "AT-S.OK")
                throw new Exception("Couldn't open socket.");

            return id.Substring(5);
        }

        public string OpenSocket(string host, int port, string tlsCn) {
            this.StopWorker();

            this.SendATCommand($"AT+S.SOCKON=" + host + "," + port + ",," + tlsCn);

            this.ExtractLine(out _);
            this.ExtractLine(out _);
            this.ExtractLine(out var id);
            this.ExtractLine(out var status);

            this.StartWorker();

            if (status != "AT-S.OK")
                throw new Exception("Couldn't open socket.");

            return id.Substring(id.LastIndexOf(":") + 1);
        }

        public int AvailableSocket(string socket) {
            this.StopWorker();

            this.SendATCommand($"AT+S.SOCKQ=" + socket);

            this.ExtractLine(out var length);
            this.ExtractLine(out var status);

            this.StartWorker();

            if (status != "AT-S.OK")
                throw new Exception("Couldn't open socket.");

            return int.Parse(length.Substring(length.LastIndexOf(":") + 1));
        }

        public bool WriteSocket(string socket, byte[] data) {
            this.StopWorker();

            this.SendATCommand($"AT+S.SOCKW=" + socket + "," + data.Length);

            this.Write(data);

            this.ExtractLine(out var status);

            this.StartWorker();

            return status == "AT-S.OK";
        }

        public bool WriteSocket(string socket, byte[] data, int index, int length) {
            this.StopWorker();

            this.SendATCommand($"AT+S.SOCKW=" + socket + "," + length);

            this.Write(data, index, length);

            this.ExtractLine(out var status);

            this.StartWorker();

            return status == "AT-S.OK";
        }

        public byte[] ReadSocket(string socket, int length) {
            this.StopWorker();

            this.SendATCommand($"AT+S.SOCKR=" + socket + "," + length);

            var data = new byte[length];
            var read = 0;
            var loaded = 0U;

            do {
                if (loaded < length)
                    loaded += this.serReader.Load((uint)(length - loaded));

                var avail = (int)this.serReader.UnconsumedBufferLength;

                for (var i = 0; i < avail; i++)
                    data[i + read] = this.serReader.ReadByte();

                read += avail;
            } while (read < length);

            this.ExtractLine(out var status);

            this.StartWorker();

            if (status != "AT-S.OK")
                throw new Exception("Couldn't read socket.");

            return data;
        }

        public int ReadSocket(string socket, int length, char[] buffer, int index) {
            this.StopWorker();

            this.SendATCommand($"AT+S.SOCKR=" + socket + "," + length);

            this.ExtractLine(out var status1);

            //var read = 0;
            //var loaded = 0U;
            //
            //do {
            //    if (loaded < length)
            //        loaded += this.serReader.Load((uint)(length - loaded));
            //
            //    var avail = (int)this.serReader.UnconsumedBufferLength;
            //
            //    for (var i = 0; i < avail; i++)
            //        buffer[i + read + index] = this.serReader.ReadByte();
            //
            //    read += avail;
            //} while (read < length);

            this.ExtractData(length, buffer, index);

            this.ExtractLine(out var status);

            this.StartWorker();

            if (status != "AT-S.OK")
                throw new Exception("Couldn't read socket.");

            return length;
        }

        public bool CloseSocket(string socket) {
            this.StopWorker();

            this.SendATCommand($"AT+S.SOCKC=" + socket);

            this.ExtractLine(out var status);

            this.StartWorker();

            return status == "AT-S.OK";
        }

        private void StopWorker() {
            if (this.paused) throw new InvalidOperationException("Already stopped.");

            this.stopping = true;

            while (this.stopping)
                Thread.Sleep(10);

            this.paused = true;
        }

        private void StartWorker() {
            if (!this.paused) throw new InvalidOperationException("Already started.");

            this.paused = false;
            this.workerWait.Set();
        }

        private const string HttpEnd = "\u001A\u001A\u001A";

        public bool HttpGet(string host, string path) => this.DoHttp($"AT+S.HTTPGET=" + host + "," + path, null);

        public bool HttpPost(string host, string path, string[][] formData) => this.DoHttp($"AT+S.HTTPPOST=" + host + "," + path + "," + SPWF04SxInterface.HttpFormEncode(formData), null);

        public bool HttpCustom(string host, int port, string data) => this.DoHttp($"AT+S.HTTPREQ=" + host + "," + port + "," + data.Length, data);

        private static string HttpFormEncode(string[][] formData) {
            var form = "";
            var combined = new string[formData.Length];
            var i = 0;

            for (i = 0; i < formData.Length; i++)
                combined[i] = formData[i][0] + "=" + formData[i][1];

            for (i = 0; i < formData.Length - 1; i++)
                form += combined[i] + "&";

            if (i < formData.Length)
                form += combined[i];

            return form;
        }

        private bool DoHttp(string request, string body) {
            //TODO There's a race condition here. The device manual says async indications are only withheld once the first 'A' character of an AT command is received. We could potentially receive one after stopping the work and before sending the command. See page 5 of UM1695, Rev 7.
            //Can possibly fix with a method like 'SendATCommandAndTakeOver' that will send the first 'A' character, pump the serial reader until empty, then continue on.

            //TODO Since this is based around ExtractLine, we may run out of memory or miss data if the line is particularly long. This is usually not an issue for the headers which are short (except cookies?), but the body, if it doesn't have any CRLF in it, we'll try to read the entire body in one line which may fail.

            //TODO GET, POST, and Custom end with <CR><LF><SUB><SUB><SUB><CR><LF><CR><LF>OK<CR><LF>, not just <CR><LF>OK<CR><LF> as the manual implies for GET and POST. Custom does mention the <SUB>.

            this.StopWorker();

            this.SendATCommand(request);

            if (body != null)
                this.Write(Encoding.UTF8.GetBytes(body));

            var a = string.Empty;
            var b = string.Empty;

            this.ExtractLine(out a);

            while (true) {
                this.ExtractLine(out b);

                if (b != SPWF04SxInterface.HttpEnd) {
                    this.HttpDataReceived?.Invoke(this, a + "\r\n");

                    a = b;
                }
                else {
                    this.HttpDataReceived?.Invoke(this, a);

                    break;
                }
            }

            this.ExtractLine(out _);
            this.ExtractLine(out var status);

            this.StartWorker();

            return status == "AT-S.OK";
        }

        public string HttpCustomSsl(string host, string commonName, int port, string data) {
            var s = this.OpenSocket(host, port, commonName);

            this.WriteSocket(s, Encoding.UTF8.GetBytes(data));

            while (this.AvailableSocket(s) == 0)
                ;

            var lst = new ArrayList();
            var len = 0;
            while ((len = this.AvailableSocket(s)) != 0) {
                var d = this.ReadSocket(s, len);

                lst.Add(d);
            }

            this.CloseSocket(s);

            len = 0;
            foreach (byte[] l in lst)
                len += l.Length;

            var res = new char[len];
            var i = 0;

            foreach (byte[] l in lst) {
                for (var j = 0; j < l.Length; j++)
                    res[j + i] = (char)l[j];

                i += l.Length;
            }

            return new string(res);
        }

        public string HttpCustomSsl(string host, string commonName, int port, string data, byte[] bytes, char[] chars) {
            var s = this.OpenSocket(host, port, commonName);

            var len = Encoding.UTF8.GetBytes(data, 0, data.Length, bytes, 0);

            this.WriteSocket(s, bytes, 0, len);

            while (this.AvailableSocket(s) == 0)
                ;

            len = 0;

            var i = 0;
            while ((len = this.AvailableSocket(s)) != 0)
                i += this.ReadSocket(s, len, chars, i);

            this.CloseSocket(s);

            //for (var j = 0; j < i; j++)
            //    chars[j] = (char)bytes[j];

            return new string(chars, 0, i);
        }

        public class AsynchronousIndicationEventArgs : EventArgs {
            public int Code { get; }
            public string Description { get; }

            public AsynchronousIndicationEventArgs(int code, string description) {
                this.Code = code;
                this.Description = description;
            }
        }

        public delegate void StringEventHandler(SPWF04SxInterface sender, string e);
        public delegate void AsynchronousIndicationEventHandler(SPWF04SxInterface sender, AsynchronousIndicationEventArgs e);

        public event StringEventHandler LineSent;
        public event StringEventHandler LineReceived;
        public event StringEventHandler HttpDataReceived;
        public event AsynchronousIndicationEventHandler AsynchronousIndicationReceived;

        private Thread worker;
        private char[] buffer;
        private string responseBuffer;
        private AutoResetEvent atExpectedEvent;
        private AutoResetEvent workerWait;
        private string atExpectedResponse;
        private bool running;
        private bool stopping;
        private bool paused;
        private DataWriter serWriter;
        private DataReader serReader;
        private SerialDevice serial;
        private GpioPin resetPin;

        public SPWF04SxInterface(string serialId, int resetPin) {
            this.atExpectedEvent = new AutoResetEvent(false);
            this.workerWait = new AutoResetEvent(false);
            this.atExpectedResponse = string.Empty;
            this.responseBuffer = string.Empty;
            this.buffer = new char[1024];
            this.running = true;
            this.stopping = false;
            this.paused = true;

            this.resetPin = GpioController.GetDefault().OpenPin(resetPin);
            this.resetPin.SetDriveMode(GpioPinDriveMode.Output);
            this.resetPin.Write(GpioPinValue.Low);

            this.serial = SerialDevice.FromId(serialId);
            this.serial.BaudRate = 115200;
            this.serial.ReadTimeout = TimeSpan.FromMilliseconds(100);
            this.serial.Handshake = SerialHandshake.None;

            this.serReader = new DataReader(this.serial.InputStream);
            this.serWriter = new DataWriter(this.serial.OutputStream);

            this.worker = new Thread(this.DoWork);
            this.worker.Start();
        }

        public void TurnOn() {
            this.StartWorker();

            this.resetPin.Write(GpioPinValue.High);
        }

        public void EnableHandshaking() {
            this.StopWorker();

            this.SendATCommand("AT+S.SCFG=console1_hwfc,1");
            this.SendATCommand("AT+S.WCFG");

            this.Reset();

            this.ExtractLine(out var status1);
            this.ExtractLine(out var status2);
            this.ExtractLine(out var status3);
            this.ExtractLine(out var status4);
            this.ExtractLine(out var status5);
            this.ExtractLine(out var status6);

            this.StartWorker();
        }

        public void DisableEcho() {
            this.StopWorker();

            this.SendATCommand("AT+S.SCFG=console_echo,0");
            this.SendATCommand("AT+S.WCFG");

            this.ExtractLine(out var status1);
            this.ExtractLine(out var status2);
            this.ExtractLine(out var status3);
            this.ExtractLine(out var status4);
            this.ExtractLine(out var status5);
            this.ExtractLine(out var status6);

            this.StartWorker();
        }

        public void SendATCommand(string atCommand) => this.SendATCommand(atCommand, string.Empty);

        public void SendATCommand(string atCommand, string expectedResponse) => this.SendATCommand(atCommand, expectedResponse, Timeout.Infinite);

        public bool SendATCommand(string atCommand, string expectedResponse, int timeout) {
            if (atCommand.IndexOf("AT") == -1) throw new ArgumentException("atCommand", "The command must begin with AT.");
            if (timeout == 0) throw new ArgumentException("timeout", "timeout cannot be 0.");

            if (atCommand.IndexOf("\r") < 0)
                atCommand += "\r";

            this.atExpectedEvent.Reset();
            this.atExpectedResponse = expectedResponse;

            this.Write(atCommand);

            if (expectedResponse != string.Empty && !this.atExpectedEvent.WaitOne(timeout, false))
                return false;

            this.atExpectedResponse = string.Empty;

            return true;
        }

        private void DoWork() {
            while (this.running) {
                this.stopping = false;
                this.workerWait.WaitOne();

                while (this.ExtractLine(out var response)) {

                }

                Thread.Sleep(250);
            }
        }

        private void ReadIn() {
            var loaded = this.serReader.Load(1024);
            var avail = (int)this.serReader.UnconsumedBufferLength;

            while (avail > 0) {
                for (var i = 0; i < avail && i < this.buffer.Length; i++)
                    this.buffer[i] = (char)this.serReader.ReadByte(); //TODO This should read an actual char, need to figure out how the device transmits higher code points

                var idx = 0;

                while (this.buffer[idx] == 0 && idx < avail)
                    idx++;

                var str = new string(this.buffer, idx, avail - idx);

                this.responseBuffer += str;

                avail -= idx + str.Length;
            }
        }

        private void Write(byte[] data) {
            this.serWriter.WriteBytes(data);

            this.Flush();
        }

        private void Write(byte[] data, int index, int length) {
            this.serWriter.WriteBuffer(data.AsBuffer(index, length));

            this.Flush();
        }

        private void Write(string line) {
            this.serWriter.WriteString(line);

            this.Flush();

            this.LineSent?.Invoke(this, line);
        }

        private void Flush() {
            while (this.serWriter.UnstoredBufferLength > 0) {
                this.serWriter.Store();

                Thread.Sleep(10);
            }
        }

        private bool ExtractLine(out string line) {
            line = null;

            var index = this.responseBuffer.IndexOf("\r\n");

            if (index == -1) {
                do {
                    this.ReadIn();

                    Thread.Sleep(10);

                    if (this.stopping)
                        return false;
                } while ((index = this.responseBuffer.IndexOf("\r\n")) == -1);
            }

            line = this.responseBuffer.Substring(0, index);

            this.responseBuffer = this.responseBuffer.Substring(index + 2);

            Array.Copy(this.buffer, index + 2, this.buffer, 0, this.buffer.Length - index - 2);

            //If taken over above in things like HTTP get, don't swallow these
            if (!this.paused && (line == "\r\n" || line == ""))
                return this.ExtractLine(out line);

            this.LineReceived?.Invoke(this, line);

            if (this.atExpectedResponse != string.Empty && line.IndexOf(this.atExpectedResponse) == 0)
                this.atExpectedEvent.Set();

            if (line[0] == '+' && line.IndexOf(":") != -1) {
                this.ParseIndication(line, out var code, out var desc);

                this.AsynchronousIndicationReceived?.Invoke(this, new AsynchronousIndicationEventArgs(code, desc));

                if (code == 2) {
                    this.resetPin.Write(GpioPinValue.Low);
                    Thread.Sleep(100);
                    this.resetPin.Write(GpioPinValue.High);
                }

                return this.ExtractLine(out line);
            }

            return true;
        }

        //TODO buffer may not have all the data or we may have already missed some
        private void ExtractData(int length, char[] buffer, int index) {
            Array.Copy(this.buffer, 0, buffer, index, length);
            Array.Copy(this.buffer, length, this.buffer, 0, this.buffer.Length - length);

            this.responseBuffer = this.responseBuffer.Substring(length);
        }

        private void ParseIndication(string indication, out int code, out string description) {
            var first = indication.IndexOf(":") + 1;
            var second = indication.IndexOf(":", first);

            code = int.Parse(indication.Substring(first, second - first));
            description = indication.Substring(second + 1);
        }
    }
}
