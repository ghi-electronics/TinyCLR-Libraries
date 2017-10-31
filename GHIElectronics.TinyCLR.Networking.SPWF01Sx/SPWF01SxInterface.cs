using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.SerialCommunication;
using GHIElectronics.TinyCLR.Storage.Streams;

namespace GHIElectronics.TinyCLR.Networking.SPWF01Sx {
    public class SPWF01SxInterface {

        private string passtype;
        private string radio;
        private string smode;
        private string socket;
        private bool connected;
        private bool error;
        private int wait;
        private string readline;

        public object StringSplitOptions { get; private set; }

        public enum SecurityMode {
            none, // 0
            WEP, // 1
            WPA2Personal, // 2

        }


        public enum SocketType {
            TCP, // t
            UDP, // u
            Secure, // s
        }


        public enum RadioMode {
            IDLE,
            STA,
            IBSS,
            MiniAP
        }

        public enum PasswordType {
            OPEN,
            WEP64,
            WEP128,
            WPA2,
            WPA_TEXT
        }



        public class Command {
            private Command() { }

            public static readonly string Test = "AT";
            public static readonly string Erase = "AT&F";
            public static readonly string SetNetwork = "AT+S.SSIDTXT";
            public static readonly string SetValue = "AT+S.SCFG=";
            public static readonly string SaveConfig = "AT&W";
            public static readonly string Status = "AT+S.STS";
            public static readonly string Socket = "AT+S.SOCKD=";
            public static readonly string HttpGet = "AT+S.HTTPGET=";
            public static readonly string HttpPost = "AT+S.HTTPPOST=";
            public static readonly string CloseSocket = "AT+S.SOCKC=";
            public static readonly string ReadSocket = "AT+S.SOCKR=";
            public static readonly string Ping = "AT+S.PING=";
            public static readonly string SSID = "AT+S.SSIDTXT=";
            public static readonly string AllowSSL = "AT+S.TLSCERT=";
            public static readonly string TLSDOMAIN = "AT+S.TLSDOMAIN=";
            public static readonly string SETTIME = "AT+S.SETTIME";
            public static readonly string Clean = "AT+S.TLSCERT2=clean,all";
            public static readonly string CertCA = "AT+S.TLSCERT=f_ca,";
            public static readonly string CertClient = "AT+S.TLSCERT=f_cert,";
            public static readonly string ClientKey = "AT+S.TLSCERT=f_key,";
            public static readonly string OpenSocket = "AT+S.SOCKON=";
            public static readonly string SetDomain = "AT+S.TLSDOMAIN=f_domain,";
            public static readonly string CheckCerts = "AT+S.TLSCERT=f_content,0";
            public static readonly string FWUpdate = "AT+S.HTTPDFSUPDATE=";
            public static readonly string ScanNetworks = "AT+S.SCAN";
            public static readonly string ServerSocket = "AT+S.SOCKD=";
            public static readonly string Reset = "AT+cfun=1";

        }


        public void ConnectWiFi(string network, PasswordType ptype, string password, RadioMode mode, SecurityMode mode1) {

            // set Password Type
            if (ptype == PasswordType.WEP64) {

                this.passtype = "AT+S.SCFG=wifi_wep_key_lens,05";
            }

            else if (ptype == PasswordType.WEP128) {
                this.passtype = "AT+S.SCFG=wifi_wep_key_lens,0D";
            }

            else if (ptype == PasswordType.WPA2) {
                this.passtype = "wifi_wpa_psk_raw,";
            }

            else if (ptype == PasswordType.WPA_TEXT) {
                this.passtype = "wifi_wpa_psk_text,";
            }

            // set Security Mode
            if (mode1 == SecurityMode.none) {
                this.smode = "0";
            }

            else if (mode1 == SecurityMode.WEP) {
                this.smode = "1";
            }

            else if (mode1 == SecurityMode.WPA2Personal) {
                this.smode = "2";
            }

            // set Radio mode

            if (mode == RadioMode.IDLE) {
                this.radio = "0";
            }

            else if (mode == RadioMode.STA) {
                this.radio = "1";
            }

            else if (mode == RadioMode.IBSS) {
                this.radio = "2";
            }

            else if (mode == RadioMode.MiniAP) {
                this.radio = "3";
            }

            Erase();
            ChooseNetwork(network);
            SetRadioMode(this.radio);
            SetSecurityMode(this.smode);
            Password(this.passtype, password);
            SendATCommand("AT+S.SCFG=blink_led,1");

            SendATCommand(Command.SaveConfig);
            Reset();
            Thread.Sleep(100);


        }

        public void ConnectMiniAP(string network, PasswordType ptype, string password, SecurityMode mode1) {

            Erase();
            Thread.Sleep(100);
            // set Password Type
            if (ptype == PasswordType.WEP64) {
                this.passtype = "05";
            }

            else if (ptype == PasswordType.WEP128) {
                this.passtype = "0D";
            }

            else if (ptype == PasswordType.OPEN) {
                this.passtype = "";
            }

            else if (ptype == PasswordType.WPA2) {
                throw new ArgumentException("Only WEP64 and WEP128 are supported", "command");
            }

            else if (ptype == PasswordType.WPA_TEXT) {
                throw new ArgumentException("Only WEP64 and WEP128 are supported", "command");
            }

            // set Security Mode
            if (mode1 == SecurityMode.none) {
                this.smode = "0";
                password = "";
            }

            else if (mode1 == SecurityMode.WEP) {
                this.smode = "1";

            }

            else if (mode1 == SecurityMode.WPA2Personal) {
                throw new ArgumentException("Only OPEN or WEP are supported", "command");
            }

            if (ptype == PasswordType.WEP64 && password.Length > 5 || ptype == PasswordType.WEP128 && password.Length > 13) {
                throw new ArgumentException("A maximum of5 text characters can be entered for 64 bit keys, and a maximum of 13 characters for 128 bit keys");
            }



            ChooseNetwork(network);
            SendATCommand(Command.SetValue + "wifi_wep_keys[0]," + ToHex(password));
            SendATCommand(Command.SetValue + "wifi_wep_key_lens," + this.passtype);
            SendATCommand("AT+S.SCFG=wifi_auth_type,0");
            SetRadioMode("3");  // set MiniAP
            SetSecurityMode(this.smode);
            SendATCommand(Command.SaveConfig);
            Reset();

            Thread.Sleep(100);


        }
        private static string[] decToHex = new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
        public static string ToHex(string hex) {
            var builder = new StringBuilder();
            //var res = "";

            for (var i = 0; i < hex.Length; i++) {
                var c = hex[i];
                var n = (int)c;
                var low = n & 0x0F;
                var high = (n & 0xF0) >> 4;
                //res = decToHex[high] + decToHex[low] + ":";
                builder.Append(high);
                builder.Append(low);
            }

            return builder.ToString();
        }

        public void WiFiOn() => SendATCommand("AT+S.WIFI=1");

        public void WiFiOff() => SendATCommand("AT+S.WIFI=0");

        public void Test() => SendATCommand(Command.Test);


        public void CLoseServerSocket() => SendATCommand(Command.ServerSocket + "0");

        public void Statistics() => SendATCommand(Command.Status);

        public void ScanNetworks() => SendATCommand(Command.ScanNetworks);

        public void FWUpdate(string IP, string filepath, string port) => SendATCommand(Command.FWUpdate + IP + "," + filepath + "," + port);


        public void WriteToSocket(string id, string message) {
            WriteData("AT+S.SOCKW=" + id + "," + message.Length.ToString(), message);


            while (this.connected == true) {
                SendATCommand("AT+S.SOCKQ=" + id);
                Thread.Sleep(3000);

                Debug.WriteLine(this.readline);


                SendATCommand("AT+S.SOCKR=" + id + "," + this.readline);

                Thread.Sleep(5000);

                if (this.wait > 20) {
                    Debug.WriteLine("Something wrong with connection.");

                    break;
                }

                if (this.error == true) {
                    Debug.WriteLine("Something wrong with connection.");

                    break;
                }


            }



        }



        public void TLSAnonymous(string time, string domain, int socket) {
            SendATCommand(Command.Clean);
            Thread.Sleep(100);

            SendATCommand(Command.SETTIME + "=" + time);
            Thread.Sleep(100);

            SendATCommand(Command.SETTIME);
            Thread.Sleep(100);

            SendATCommand(Command.OpenSocket + domain + "," + socket.ToString() + ",s");
            Thread.Sleep(100);
        }

        public void TLSOneWayAuth(string time, byte[] cert, string domain, string hostname, int socket) {

            if (cert.Length < 700 || cert.Length > 2000) {
                throw new ArgumentException("Use certificate according RSA-2048 authentication");
            }

            SendATCommand(Command.Clean);
            Thread.Sleep(100);

            SendATCommand(Command.SETTIME + "=" + time);
            Thread.Sleep(100);

            SendATCommand(Command.SETTIME);
            Thread.Sleep(100);

            WriteData(Command.CertCA + cert.Length.ToString(), cert);
            Thread.Sleep(100);

            SendATCommand(Command.SetDomain + domain);
            Thread.Sleep(100);

            SendATCommand(Command.CheckCerts);
            Thread.Sleep(100);

            SendATCommand(Command.OpenSocket + hostname + "," + socket.ToString() + ",s,ind");
            Thread.Sleep(100);

        }

        public void TLSMutualAuth(string time, byte[] cert, byte[] client, byte[] key, string domain, string hostname, int socket) {


            SendATCommand(Command.Clean);
            Thread.Sleep(100);

            SendATCommand(Command.SETTIME + "=" + time);
            Thread.Sleep(100);

            SendATCommand(Command.SETTIME);
            Thread.Sleep(100);

            WriteData(Command.CertCA + cert.Length.ToString(), cert);
            Thread.Sleep(100);

            WriteData(Command.CertClient + client.Length.ToString(), client);
            Thread.Sleep(100);

            WriteData(Command.ClientKey + key.Length.ToString(), key);
            Thread.Sleep(100);

            SendATCommand(Command.SetDomain + domain);
            Thread.Sleep(100);

            SendATCommand(Command.CheckCerts);
            Thread.Sleep(100);

            SendATCommand(Command.OpenSocket + hostname + "," + socket.ToString() + ",s,ind");
            Thread.Sleep(1000);

        }

        public void SetSecurityMode(string mode) => SendATCommand(Command.SetValue + "wifi_priv_mode," + mode);

        public void SetRadioMode(string radio) => SendATCommand(Command.SetValue + "wifi_mode," + radio);

        public void ChooseNetwork(string network) => SendATCommand(Command.SSID + network);

        public void Password(string passtype, string password) => SendATCommand(Command.SetValue + passtype + password);

        public void HTTPPOST(string host, string form) => SendATCommand(Command.HttpPost + host + ",/" + form);

        public void Ping(string host) => SendATCommand(Command.Ping + host);

        public void ReadSocket(string id, string length) => SendATCommand(Command.ReadSocket + id + "," + length);

        public void CloseSocket(string id) {

            SendATCommand("AT+S.SOCKQ=" + id);
            Thread.Sleep(3000);

            SendATCommand("AT+S.SOCKR=" + id + "," + this.readline.ToString());
            Thread.Sleep(5000);

            SendATCommand(Command.CloseSocket + id);

        }

        public void Erase() => SendATCommand(Command.Erase);

        public void Reset() => SendATCommand(Command.Reset);

        public void ServerSocket(int number) => SendATCommand(Command.Socket + number);

        public void OpenSocket(string host, int port, SocketType sock) {
            if (sock == SocketType.Secure) {

                this.socket = "s";
            }

            else if (sock == SocketType.TCP) {
                this.socket = "t";
            }

            else if (sock == SocketType.UDP) {
                this.socket = "u";
            }

            SendATCommand(Command.OpenSocket + host + "," + port + "," + this.socket);
        }

        public void Help() => SendATCommand("AT+S.HELP");

        public void Config() => SendATCommand("AT&V");

        public void FileList() => SendATCommand("AT+S.FSL");

        public void FileContent(string filepath) => SendATCommand("AT+S.FSP=" + filepath);

        public void WriteData(string command, byte[] data) {
            this.SendATCommand(command);
            this.serWriter.WriteBytes(data);
            this.Flush();
        }

        public void WriteData(string command, string data) {
            this.SendATCommand(command);
            this.serWriter.WriteString(data);
            this.Flush();
        }




        private void StopWorker() {
            if (this.worker == null) throw new InvalidOperationException("Already stopped.");

            this.stopping = true;
            this.running = false;
            this.worker.Join();
            this.stopping = false;
            this.worker = null;
        }

        private void StartWorker() {
            if (this.worker != null) throw new InvalidOperationException("Already started.");

            this.running = true;
            this.worker = new Thread(this.DoWork);
            this.worker.Start();
        }

        public bool HttpGet(string host, string path) {
            //TODO There's a race condition here. The device manual says async indications are only withheld once the first 'A' character of an AT command is received. We could potentially receive one after stopping the work and before sending the command. See page 5 of UM1695, Rev 7.
            //Can possibly fix with a method like 'SendATCommandAndTakeOver' that will send the first 'A' character, pump the serial reader until empty, then continue on.
            //HTTP post has the same issue

            //TODO GET, POST, and Custom end with <CR><LF><SUB><SUB><SUB><CR><LF><CR><LF>OK<CR><LF>, not just <CR><LF>OK<CR><LF> as the manual implies for GET and POST. Custom does mention the <SUB>.

            this.StopWorker();

            this.SendATCommand($"AT+S.HTTPGET={host},{path}");

            var line = string.Empty;
            while (line == null || (line != "OK" && line.IndexOf("ERROR:") != 0))
                while (this.ExtractLine(out line) && line != "OK" && line.IndexOf("ERROR:") != 0)
                    this.HttpDataReceived?.Invoke(this, line + "\r\n");

            this.StartWorker();

            return line == "OK";
        }

        public bool HttpPost(string host, string path, string[][] formData) {
            var form = "";
            var combined = new string[formData.Length];
            var i = 0;

            for (i = 0; i < formData.Length; i++)
                combined[i] = formData[i][0] + "=" + formData[i][1];

            for (i = 0; i < formData.Length - 1; i++)
                form += combined[i] + "&";

            if (i < formData.Length)
                form += combined[i];

            this.StopWorker();

            this.SendATCommand($"AT+S.HTTPPOST={host},{path},{form}");

            var line = string.Empty;
            while (line == null || (line != "OK" && line.IndexOf("ERROR:") != 0))
                while (this.ExtractLine(out line) && line != "OK" && line.IndexOf("ERROR:") != 0)
                    this.HttpDataReceived?.Invoke(this, line + "\r\n");

            this.StartWorker();

            return line == "OK";
        }

        public class AsynchronousIndicationEventArgs : EventArgs {
            public int Code { get; }
            public string Description { get; }

            public AsynchronousIndicationEventArgs(int code, string description) {
                this.Code = code;
                this.Description = description;
            }
        }

        public delegate void StringEventHandler(SPWF01SxInterface sender, string e);
        public delegate void AsynchronousIndicationEventHandler(SPWF01SxInterface sender, AsynchronousIndicationEventArgs e);

        public event StringEventHandler LineSent;
        public event StringEventHandler LineReceived;
        public event StringEventHandler HttpDataReceived;
        public event AsynchronousIndicationEventHandler AsynchronousIndicationReceived;

        private Thread worker;
        private char[] buffer;
        private string responseBuffer;
        private AutoResetEvent atExpectedEvent;
        private string atExpectedResponse;
        private bool running;
        private bool stopping;
        private DataWriter serWriter;
        private DataReader serReader;
        private SerialDevice serial;
        private GpioPin resetPin;

        public SPWF01SxInterface(string serialId, int resetPin) {
            this.atExpectedEvent = new AutoResetEvent(false);
            this.atExpectedResponse = string.Empty;
            this.responseBuffer = string.Empty;
            this.buffer = new char[1024];
            this.running = false;
            this.stopping = false;

            this.resetPin = GpioController.GetDefault().OpenPin(resetPin);
            this.resetPin.SetDriveMode(GpioPinDriveMode.Output);
            this.resetPin.Write(GpioPinValue.Low);

            this.serial = SerialDevice.FromId(serialId);
            this.serial.BaudRate = 115200;
            this.serial.ReadTimeout = TimeSpan.FromMilliseconds(100);

            this.serReader = new DataReader(this.serial.InputStream);
            this.serWriter = new DataWriter(this.serial.OutputStream);
        }

        public void TurnOn() {
            this.StartWorker();

            this.resetPin.Write(GpioPinValue.High);
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
                while (this.ExtractLine(out var response)) {
                    if (this.atExpectedResponse != string.Empty && response.IndexOf(this.atExpectedResponse) == 0)
                        this.atExpectedEvent.Set();

                    if (response[0] == '+' && response.IndexOf(":") != -1) {
                        this.ParseIndication(response, out var code, out var desc);

                        this.AsynchronousIndicationReceived?.Invoke(this, new AsynchronousIndicationEventArgs(code, desc));
                    }
                    else {

                    }
                }

                Thread.Sleep(250);
            }
        }

        private void ReadIn() {
            var asdf = this.serReader.Load(1024);

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

            //If taken over above in things like HTTP get, don't swallow these
            if (this.running && (line == "\r\n" || line == ""))
                return this.ExtractLine(out line);

            this.LineReceived?.Invoke(this, line);

            return true;
        }

        private void ParseIndication(string indication, out int code, out string description) {
            var first = indication.IndexOf(":") + 1;
            var second = indication.IndexOf(":", first);

            code = int.Parse(indication.Substring(first, second - first));
            description = indication.Substring(second + 1);
        }
    }
}
