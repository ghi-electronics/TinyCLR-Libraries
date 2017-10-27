using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.SerialCommunication;
using GHIElectronics.TinyCLR.Storage.Streams;

namespace GHIElectronics.TinyCLR.Networking.SPWF01Sx {
    public class SPWF01SxInterface {

        private DataWriter serWriter;
        public DataReader serReader;
        private SerialDevice serial;
        private string passtype;
        private string radio;
        private string smode;
        private string socket;
        private string serialBuffer;
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
            public static readonly string Reset = "at+cfun=1";

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
            Write("AT+S.SCFG=blink_led,1");

            Write(Command.SaveConfig);
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
            Write(Command.SetValue + "wifi_wep_keys[0]," + ToHex(password));
            Write(Command.SetValue + "wifi_wep_key_lens," + this.passtype);
            Write("AT+S.SCFG=wifi_auth_type,0");
            SetRadioMode("3");  // set MiniAP
            SetSecurityMode(this.smode);
            Write(Command.SaveConfig);
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

        public void InitWiFi(string serialId, int resetPin) {
            //Create a serial port connection
            var WiFiReset = GpioController.GetDefault().OpenPin(resetPin);
            WiFiReset.SetDriveMode(GpioPinDriveMode.Output);
            WiFiReset.Write(GpioPinValue.Low);

            this.serial = SerialDevice.FromId(serialId);
            this.serial.BaudRate = 115200;
            this.serial.ReadTimeout = TimeSpan.Zero;

            this.serReader = new DataReader(this.serial.InputStream);
            this.serWriter = new DataWriter(this.serial.OutputStream);
            Thread.Sleep(100);

            var reader = new Thread(this.Read);
            reader.Start();
            Thread.Sleep(100);

            WiFiReset.Write(GpioPinValue.High);

            var m = typeof(DataWriter).GetMethod("EnsureSpace", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            m.Invoke(this.serWriter, new object[] { 1536U });
            m.Invoke(this.serWriter, new object[] { 1536U });
            m.Invoke(this.serWriter, new object[] { 1536U });
            m.Invoke(this.serWriter, new object[] { 1536U });

        }

        public void Write(string command) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            if (command.Length > 127 + 2) //2 for AT prefix
            {
                //AT + 127 chars according to the documentation
                throw new ArgumentException("Maximum command length is 127 chars!", "command");
            }

            this.serWriter.WriteString(command + "\r");
            this.serWriter.Store();
            Debug.WriteLine("Sent: " + command);
        }

        public void WriteData(string command, byte[] data) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            if (command.Length > 127 + 2) //2 for AT prefix
            {
                //AT + 127 chars according to the documentation
                throw new ArgumentException("Maximum command length is 127 chars!", "command");
            }


            this.serWriter.WriteString(command + "\r");
            this.serWriter.WriteBytes(data);

            var written = 0U;

            while (this.serWriter.UnstoredBufferLength > 0)
                written = this.serWriter.Store();

            Debug.WriteLine("Sent: " + command);
        }

        public void WriteData(string command, string data) {
            if (command == null) throw new ArgumentNullException("command");


            if (command.Length > 127 + 2) //2 for AT prefix
            {
                //AT + 127 chars according to the documentation
                throw new ArgumentException("Maximum command length is 127 chars!", "command");
            }


            this.serWriter.WriteString(command + "\r");
            this.serWriter.WriteString(data);

            var written = 0U;

            while (this.serWriter.UnstoredBufferLength > 0)
                written = this.serWriter.Store();

            Debug.WriteLine("Sent: " + command);
        }

        public void WiFiOn() => Write("AT+S.WIFI=1");

        public void WiFiOff() => Write("AT+S.WIFI=0");

        public void Test() => Write(Command.Test);


        public void CLoseServerSocket() => Write(Command.ServerSocket + "0");

        public void Statistics() => Write(Command.Status);

        public void ScanNetworks() => Write(Command.ScanNetworks);

        public void FWUpdate(string IP, string filepath, string port) => Write(Command.FWUpdate + IP + "," + filepath + "," + port);


        public void WriteToSocket(string id, string message) {
            WriteData("AT+S.SOCKW=" + id + "," + message.Length.ToString(), message);


            while (this.connected == true) {
                Write("AT+S.SOCKQ=" + id);
                Thread.Sleep(3000);

                Debug.WriteLine(this.readline);


                Write("AT+S.SOCKR=" + id + "," + this.readline);

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
            Write(Command.Clean);
            Thread.Sleep(100);

            Write(Command.SETTIME + "=" + time);
            Thread.Sleep(100);

            Write(Command.SETTIME);
            Thread.Sleep(100);

            Write(Command.OpenSocket + domain + "," + socket.ToString() + ",s");
            Thread.Sleep(100);
        }

        public void TLSOneWayAuth(string time, byte[] cert, string domain, string hostname, int socket) {

            if (cert.Length < 700 || cert.Length > 2000) {
                throw new ArgumentException("Use certificate according RSA-2048 authentication");
            }

            Write(Command.Clean);
            Thread.Sleep(100);

            Write(Command.SETTIME + "=" + time);
            Thread.Sleep(100);

            Write(Command.SETTIME);
            Thread.Sleep(100);

            WriteData(Command.CertCA + cert.Length.ToString(), cert);
            Thread.Sleep(100);

            Write(Command.SetDomain + domain);
            Thread.Sleep(100);

            Write(Command.CheckCerts);
            Thread.Sleep(100);

            Write(Command.OpenSocket + hostname + "," + socket.ToString() + ",s,ind");
            Thread.Sleep(100);

        }

        public void TLSMutualAuth(string time, byte[] cert, byte[] client, byte[] key, string domain, string hostname, int socket) {


            Write(Command.Clean);
            Thread.Sleep(100);

            Write(Command.SETTIME + "=" + time);
            Thread.Sleep(100);

            Write(Command.SETTIME);
            Thread.Sleep(100);

            WriteData(Command.CertCA + cert.Length.ToString(), cert);
            Thread.Sleep(100);

            WriteData(Command.CertClient + client.Length.ToString(), client);
            Thread.Sleep(100);

            WriteData(Command.ClientKey + key.Length.ToString(), key);
            Thread.Sleep(100);

            Write(Command.SetDomain + domain);
            Thread.Sleep(100);

            Write(Command.CheckCerts);
            Thread.Sleep(100);

            Write(Command.OpenSocket + hostname + "," + socket.ToString() + ",s,ind");
            Thread.Sleep(1000);

        }

        public void SetSecurityMode(string mode) => Write(Command.SetValue + "wifi_priv_mode," + mode);

        public void SetRadioMode(string radio) => Write(Command.SetValue + "wifi_mode," + radio);

        public void ChooseNetwork(string network) => Write(Command.SSID + network);

        public void Password(string passtype, string password) => Write(Command.SetValue + passtype + password);

        public void HTTPGet(string host, string path) => Write(Command.HttpGet + host + ",/" + path);

        public void HTTPPOST(string host, string form) => Write(Command.HttpPost + host + ",/" + form);

        public void Ping(string host) => Write(Command.Ping + host);

        public void ReadSocket(string id, string length) => Write(Command.ReadSocket + id + "," + length);

        public void CloseSocket(string id) {

            Write("AT+S.SOCKQ=" + id);
            Thread.Sleep(3000);

            Write("AT+S.SOCKR=" + id + "," + this.readline.ToString());
            Thread.Sleep(5000);

            Write(Command.CloseSocket + id);

        }

        public void Erase() => Write(Command.Erase);

        public void Reset() => Write(Command.Reset);

        public void ServerSocket(int number) => Write(Command.Socket + number);

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

            Write(Command.OpenSocket + host + "," + port + "," + this.socket);
        }



        public void Help() => Write("AT+S.HELP");

        public void Config() => Write("AT&V");

        public void FileList() => Write("AT+S.FSL");

        public void FileContent(string filepath) => Write("AT+S.FSP=" + filepath);


        public void Read() {
            this.connected = true;
            this.wait = 0;
            this.error = false;
            var zero = 0;
            while (true) {
                Thread.Sleep(10);
                var i = this.serReader.Load(512);
                if (i == 0) continue;
                var response = this.serReader.ReadString(i);
                response.ToString();
                Debug.WriteLine(response);

                if (response.IndexOf("200 OK") != -1) {
                    this.connected = false;
                }

                if (response.IndexOf("+WIND:55:") != -1) {
                    this.wait++;
                }

                if (response.IndexOf("DATALEN: ") != -1) {
                    this.readline = response.Substring(12);
                    if (this.readline.IndexOf("ALEN: ") != -1) {
                        this.readline = zero.ToString();
                    }
                    Debug.WriteLine(this.readline);
                }


                if (response.IndexOf("+WIND:8:") != -1 || response.IndexOf("IND:8:") != -1 || response.IndexOf(":Hard Fault:") != -1) {
                    this.error = true;
                }
            }
        }


        public void ReadBytes() {

            var builder = new StringBuilder();
            const int length = 512;

            var buffer = new byte[length];

            var i = this.serReader.Load(length);

            for (var j = 0; j < i; j++) {

                buffer[j] = this.serReader.ReadByte();
                if (buffer[j] != 0) {
                    var result = (char)buffer[j];
                    builder.Append(result);
                    Array.Clear(buffer, 0, j);
                }

            }

            Debug.WriteLine(builder.ToString());
        }




    }
}
