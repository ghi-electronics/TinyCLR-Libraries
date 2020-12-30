// TinyCLS OS VNC Server Library
// Copyright (C) 2020 GHI Electronics
//
// This file is a heavy modified version from T1T4N, based on VncSharp project.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.IO;
using System.Net;

using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

namespace GHIElectronics.TinyCLR.Vnc {

    internal class VncHost {

        internal delegate void PointerChangedEventHandler(int x, int y, bool pressed);
        internal delegate void KeyChangedEventHandler(uint key, bool pressed);

        internal event PointerChangedEventHandler PointerChangedEvent;
        internal event KeyChangedEventHandler KeyChangedEvent;

        public enum Encoding : int {
            RawEncoding = 0,
        }

        public enum ServerMessages {
            FramebufferUpdate = 0,
            SetColorMapEntries = 1,
            Bell = 2,
            ServerCutText = 3,
        }

        public enum ClientMessages : byte {
            SetPixelFormat = 0,
            ReadColorMapEntries = 1,
            SetEncodings = 2,
            FramebufferUpdateRequest = 3,
            KeyEvent = 4,
            PointerEvent = 5,
            ClientCutText = 6,
        }

        //Version numbers
        protected int verMajor = 3;
        protected int verMinor = 8;

        //Shared flag
        public bool Shared { get; set; }

        public string cutText;

        protected Socket localClient;		// Network object used to communicate with host
        protected Socket serverSocket;

        protected NetworkStream stream;	        // Stream object used to send/receive data
        protected StreamReader reader;	// Integral rather than Byte values are typically
        protected StreamWriter writer; // sent and received, so these handle this.                                               

        public bool isRunning;
        public bool Pause { get; set; }

        public int CurrentX { get; set; }
        public int CurrentY { get; set; }
        public int CurrentWidth { get; set; }
        public int CurrentHeight { get; set; }

        public int Port { get; set; }

        private EncodedRectangle endcodedRectangle;

        //Supported encodings
        public uint[] Encodings { get; private set; }

        public Encoding GetPreferredEncoding() => Encoding.RawEncoding;

        public VncHost(int port) => this.Port = port;

        public float ServerVersion => (float)this.verMajor + (this.verMinor * 0.1f);

        public void Start() {
            this.isRunning = true;
            try {

                var serverSocketEP = new IPEndPoint(IPAddress.Any, this.Port);
                this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.serverSocket.Bind(serverSocketEP);
                this.serverSocket.Listen(1);

            }
            //The port is being used, and serverSocket cannot start
            catch (Exception ex) {
                Debug.WriteLine(ex.ToString());

                return;
            }
            try {

                this.localClient = this.serverSocket.Accept();

                this.localClient.SendTimeout = 0;

                this.localClient.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);

                var localIP = IPAddress.Parse(((IPEndPoint)this.localClient.RemoteEndPoint).Address.ToString());

                this.stream = new NetworkStream(this.localClient, true);
                this.reader = new StreamReader(this.stream);
                this.writer = new StreamWriter(this.stream);

            }
            catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
            }

        }

        public void ReadProtocolVersion() {
            try {
                var b = this.reader.ReadBytes(12);

                if (b[0] == 0x52 &&					// R
                    b[1] == 0x46 &&					// F
                    b[2] == 0x42 &&					// B
                    b[3] == 0x20 &&					// (space)
                    b[4] == 0x30 &&					// 0
                    b[5] == 0x30 &&					// 0
                    b[6] == 0x33 &&					// 3
                    b[7] == 0x2e &&					// .
                   (b[8] == 0x30 ||                 // 0
                    b[8] == 0x38) &&				// BUG FIX: Apple reports 8 
                   (b[9] == 0x30 ||                 // 0
                    b[9] == 0x38) &&				// BUG FIX: Apple reports 8 
                   (b[10] == 0x33 ||				// 3, 7, OR 8 are all valid and possible
                    b[10] == 0x36 ||				// BUG FIX: UltraVNC reports protocol version 3.6!
                    b[10] == 0x37 ||
                    b[10] == 0x38 ||
                    b[10] == 0x39) &&               // BUG FIX: Apple reports 9					
                    b[11] == 0x0a)					// \n
                {

                    this.verMajor = 3;

                    switch (b[10]) {
                        case 0x33:
                        case 0x36:
                            this.verMinor = 3;
                            break;
                        case 0x37:
                            this.verMinor = 7;
                            break;
                        case 0x38:
                            this.verMinor = 8;
                            break;
                        case 0x39:
                            this.verMinor = 8;
                            break;
                    }
                }
                else {
                    throw new NotSupportedException("Only versions 3.3, 3.7, and 3.8 of the RFB Protocol are supported.");
                }
            }
            catch (IOException ex) {
                Debug.WriteLine(ex.Message);
                this.Close();
            }
        }


        public void WriteProtocolVersion() {
            try {
                // We will use which ever version the server understands, be it 3.3, 3.7, or 3.8.
                Debug.Assert(this.verMinor == 3 || this.verMinor == 7 || this.verMinor == 8, "Wrong Protocol Version!",
                             string.Format("Protocol Version should be 3.3, 3.7, or 3.8 but is {0}.{1}", this.verMajor.ToString(), this.verMinor.ToString()));

                this.writer.Write(GetBytes(string.Format("RFB 003.00{0}\n", this.verMinor.ToString())));
                this.writer.Flush();
            }
            catch (IOException ex) {
                Debug.WriteLine(ex.Message);
                this.Close();

            }
        }

        public void Close() {
            this.isRunning = false;

            this.serverSocket.Close();
            this.localClient.Close();
        }


        public bool WriteAuthentication(string password) {
            if (string.IsNullOrEmpty(password)) {
                if (this.verMinor == 3) {
                    this.WriteUint32(1);
                }
                else {
                    byte[] types = { 1 };
                    this.writer.Write((byte)types.Length);

                    for (var i = 0; i < types.Length; i++)
                        this.writer.Write(types[i]);
                }
                if (this.verMinor >= 7)
                    this.reader.ReadByte();

                if (this.verMinor == 8)
                    this.WriteSecurityResult(0);
                return true;
            }
            return false;
        }


        public void WriteSecurityResult(uint sr) => this.writer.Write(sr);

        public bool ReadClientInit() {
            var sh = false;
            try {
                var read = this.reader.ReadByte();
                this.Shared = (read == 1);
                sh = this.Shared;
                return this.Shared;
            }
            catch (IOException ex) {
                Debug.WriteLine(ex.Message);
                this.Close();
            }
            return sh;
        }

        public void WriteServerInit(FrameBuffer fb) {
            try {
                this.writer.Write((ushort)(fb.Width));
                this.writer.Write((ushort)(fb.Height));
                this.writer.Write(fb.ToPixelFormat());

                this.writer.Write((uint)(fb.ServerName.Length));
                this.writer.Write(GetBytes(fb.ServerName));
                this.writer.Flush();
            }
            catch (IOException ex) {
                Debug.WriteLine(ex.Message);
                this.Close();

            }
        }

        public FrameBuffer ReadSetPixelFormat(int w, int h) {
            this.CurrentX = 0;
            this.CurrentY = 0;
            this.CurrentWidth = w;
            this.CurrentHeight = h;

            FrameBuffer ret = null;
            try {
                this.ReadPadding(3);
                var pf = this.ReadBytes(16);
                ret = FrameBuffer.FromPixelFormat(pf, w, h);
                return ret;
            }
            catch (IOException ex) {
                Debug.WriteLine(ex.Message);
                this.Close();
            }
            return ret;
        }

        public void ReadSetEncodings() {
            try {
                this.ReadPadding(1);
                var len = this.reader.ReadUInt16();
                var enc = new uint[len];

                for (var i = 0; i < (int)len; i++)
                    enc[i] = this.reader.ReadUInt32();
                this.Encodings = enc;
            }
            catch (IOException ex) {
                Debug.WriteLine(ex.Message);
                this.Close();

            }
        }

        public void ReadFrameBufferUpdateRequest(FrameBuffer fb) {

            try {
                var incremental = this.reader.ReadByte() == 0 ? false : true;
                var x = this.reader.ReadUInt16();
                var y = this.reader.ReadUInt16();
                var width = this.reader.ReadUInt16();
                var height = this.reader.ReadUInt16();

                this.DoFrameBufferUpdate(fb, incremental, x, y, width, height);

            }
            catch (IOException ex) {
                Debug.WriteLine(ex.Message);
                this.Close();

            }
        }

        private void DoFrameBufferUpdate(FrameBuffer fb, bool incremental, int x, int y, int width, int height) {
            var w = fb.Width;
            var h = fb.Height;
            if ((x < 0) || (y < 0) || (width <= 0) || (height <= 0)) {
                return;
            }
            if (x + width > w) {
                return;
            }
            if (y + height > h) {
                return;
            }

            this.WriteFrameBufferUpdate(fb, incremental, x, y, width, height);
        }


        public void WriteFrameBufferUpdate(FrameBuffer fb, bool incremental, int x, int y, int width, int height) {

#if DEBUG
            var start = DateTime.Now;
#endif

            var frameCountUpdate = 1;

            Thread.Sleep(1); //Note: Note that there may be an indefinite period
                             //between the FramebufferUpdateRequest and the FramebufferUpdate.

            while (this.Pause) {
                Thread.Sleep(1);
            }

            // Any change below require cache rectangle again
            if (this.endcodedRectangle == null || this.endcodedRectangle.BitsPerPixel != fb.BitsPerPixel || this.endcodedRectangle.X != this.CurrentX || this.endcodedRectangle.Y != this.CurrentY || this.endcodedRectangle.Width != this.CurrentWidth || this.endcodedRectangle.Height != this.CurrentHeight) {
                if (incremental == true) {
                    this.endcodedRectangle = new RawRectangle(fb, this.CurrentX, this.CurrentY, this.CurrentWidth, this.CurrentHeight);
                }
                else {
                    this.endcodedRectangle = new RawRectangle(fb, fb.X, fb.Y, fb.Width, fb.Height);
                }
            }

            this.endcodedRectangle.Encode();

#if DEBUG
            var endcodeTime = DateTime.Now - start;

            Debug.WriteLine("Encode time " + endcodeTime.TotalMilliseconds);

#endif

            var data = this.endcodedRectangle.Pixels;

            var header = new byte[16];

            header[0] = (byte)ServerMessages.FramebufferUpdate;
            header[1] = 0; //pad

            header[2] = (byte)(frameCountUpdate >> 8);  // frameCountUpdate
            header[3] = (byte)(frameCountUpdate >> 0); // frameCountUpdate

            header[4] = (byte)(this.endcodedRectangle.X >> 8);
            header[5] = (byte)(this.endcodedRectangle.X >> 0);

            header[6] = (byte)(this.endcodedRectangle.Y >> 8);
            header[7] = (byte)(this.endcodedRectangle.Y >> 0);

            header[8] = (byte)(this.endcodedRectangle.Width >> 8);
            header[9] = (byte)(this.endcodedRectangle.Width >> 0);

            header[10] = (byte)(this.endcodedRectangle.Height >> 8); ;
            header[11] = (byte)(this.endcodedRectangle.Height >> 0); ;


            header[12] = 0; // VncHost.Encoding.RawEncoding
            header[13] = 0; // VncHost.Encoding.RawEncoding
            header[14] = 0; // VncHost.Encoding.RawEncoding
            header[15] = 0; // VncHost.Encoding.RawEncoding


            this.Write(header);
#if DEBUG
            start = DateTime.Now;
#endif

            var blockSize = 1024;
            var block = data.Length / blockSize;

            var offset = 0;
            var total = 0;

            while (block > 0) {

                var count = (data.Length - total) > blockSize ? blockSize : (data.Length - total);

                this.Write(data, offset, count);

                offset += count;
                total += count;

                block--;

                //System.Threading.Thread.Sleep(1);
            }
#if DEBUG
            var sendingTime = DateTime.Now - start;

            Debug.WriteLine("Sending time " + sendingTime.TotalMilliseconds);
#endif

        }


        public void ReadKeyEvent() {
            try {
                var pressed = (this.reader.ReadByte() == 1);
                this.ReadPadding(2);
                var keysym = this.reader.ReadUInt32();

                KeyChangedEvent?.Invoke(keysym, pressed);

            }
            catch (IOException ex) {
                Debug.WriteLine(ex.Message);
                this.Close();
            }
        }

        public void ReadPointerEvent() {
            try {
                var buttonMask = (byte)this.reader.ReadByte();
                var x = this.reader.ReadUInt16();
                var y = this.reader.ReadUInt16();

                PointerChangedEvent?.Invoke(x, y, buttonMask != 0);
            }
            catch (IOException ex) {
                Debug.WriteLine(ex.Message);
                this.Close();

            }
        }

        public void ReadClientCutText() {
            try {
                this.ReadPadding(3);

                var len = (int)(this.reader.ReadUInt32());
                var text = GetString(this.reader.ReadBytes(len));
                this.cutText = text;
            }
            catch (IOException ex) {
                Debug.WriteLine(ex.Message);
                this.Close();

            }
        }

        public ClientMessages ReadServerMessageType() {
            byte x = 0;
            try {
                x = (byte)this.reader.ReadByte();
                return (ClientMessages)x;
            }
            catch (IOException ex) {
                Debug.WriteLine(ex.Message);
                this.Close();
            }
            return (ClientMessages)x;
        }

        /// <summary>
        /// Writes the type of message being sent to the client--all messages are prefixed with a message type.
        /// </summary>
        private void WriteServerMessageType(ServerMessages message) {
            try { this.writer.Write((byte)(message)); }
            catch (IOException ex) {
                Debug.WriteLine(ex.Message);
                this.Close();

            }
        }


        public void WriteServerCutText(string text) {
            try {
                this.WriteServerMessageType(ServerMessages.ServerCutText);
                this.WritePadding(3);

                this.writer.Write((uint)text.Length);
                this.writer.Write(GetBytes(text));
                this.writer.Flush();
            }
            catch (IOException ex) {
                Debug.WriteLine(ex.Message);
                this.Close();
            }
        }

        public void SetUpdateWindow(int x, int y, int width, int height) {
            this.CurrentX = x;
            this.CurrentY = y;
            this.CurrentWidth = width;
            this.CurrentHeight = height;
        }

        public uint ReadUint32() => this.reader.ReadUInt32();

        public ushort ReadUInt16() => this.reader.ReadUInt16();

        public byte ReadByte() => (byte)this.reader.ReadByte();

        public byte[] ReadBytes(int count) => this.reader.ReadBytes(count);

        public void WriteUint32(uint value) => this.writer.Write(value);

        public void WriteUInt16(ushort value) => this.writer.Write(value);

        public void WriteUInt32(uint value) => this.writer.Write(value);

        public void WriteByte(byte value) => this.writer.Write(value);

        public void Write(byte[] buffer) => this.writer.Write(buffer);

        public void Write(byte[] buffer, int offset, int count) => this.writer.Write(buffer, offset, count);

        public void Flush() => this.writer.Flush();

        public void ReadPadding(int length) => this.ReadBytes(length);

        public void WritePadding(int length) {
            var padding = new byte[length];
            this.writer.Write(padding, 0, padding.Length);
        }

        protected static byte[] GetBytes(string text) => System.Text.Encoding.UTF8.GetBytes(text);
        protected static string GetString(byte[] bytes) => System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
    }
}
