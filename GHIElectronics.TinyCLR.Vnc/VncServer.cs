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
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace GHIElectronics.TinyCLR.Vnc {
    public class VncServer {

        public delegate void PointerChangedEventHandler(int x, int y, bool pressed);
        public delegate void KeyChangedEventHandler(uint key, bool pressed);
        public delegate void FrameSentEventHandler(bool success);
        public delegate void ClientRequestUpdateEventHandler(int x, int y, int width, int height);
        public delegate void ConnectionChanged(bool connected);

        public event PointerChangedEventHandler PointerChangedEvent;
        public event KeyChangedEventHandler KeyChangedEvent;
        public event FrameSentEventHandler FrameSentEvent;
        public event ClientRequestUpdateEventHandler ClientRequestUpdateEvent;
        public event ConnectionChanged ConnectionChangedEvent;


        public int Port { get; private set; }
        internal string Password { get; private set; }
        public string ServerName { get; private set; } = "Default";
        public TimeSpan DelayBetweenFrame { get; set; } = TimeSpan.FromMilliseconds(10);
        public bool Connected { get; private set; }

        private VncHost host;
        private readonly FrameBuffer frameBuffer;

        private int Width { get; set; }
        private int Height { get; set; }

        public VncServer(string serverName, int port, int width, int height) {
            this.Password = null;
            this.Port = port;
            this.ServerName = serverName;

            this.frameBuffer = new FrameBuffer(width, height) {
                BitsPerPixel = 16,
                Depth = 16,
                BigEndian = false,
                TrueColor = false,
                RedShift = 11,
                GreenShift = 5,
                BlueShift = 0,
                BlueMax = 0x1F,
                GreenMax = 0x3F,
                RedMax = 0x1F,
                ServerName = string.IsNullOrEmpty(this.ServerName) ? "Default" : this.ServerName
            };

            if (string.IsNullOrEmpty(this.ServerName))
                throw new ArgumentNullException("Name", "The VNC Server Name cannot be empty.");
            if (this.Port == 0)
                throw new ArgumentNullException("Port", "The VNC Server port cannot be zero.");

            this.host = new VncHost(this.Port);

            this.host.KeyChangedEvent += (a, b) => KeyChangedEvent?.Invoke(a, b);
            this.host.PointerChangedEvent += (a, b, c) => PointerChangedEvent?.Invoke(a, b, c);
            this.host.FrameSentEvent += (a) => FrameSentEvent?.Invoke(a);
            this.host.ClientRequestUpdateEvent += (x, y, w, h) => ClientRequestUpdateEvent?.Invoke(x, y, w, h);
            this.host.ConnectionChangedEvent += (a) => ConnectionChangedEvent?.Invoke(a);

            this.Width = width;
            this.Height = height;

        }

        private bool serverRunning;
        private void Run() {

            while (this.serverRunning) {
                try {
                    this.host.Start();

                    this.host.WriteProtocolVersion();

                    this.host.ReadProtocolVersion();

                    if (!this.host.WriteAuthentication(this.Password)) {
                        this.host.Close();
                    }
                    else {

                        var share = this.host.ReadClientInit();

                        if (share == false) {
                            this.host.Close();

                            throw new InvalidOperationException("Read client init failed.");
                        }

                        this.host.WriteServerInit(this.frameBuffer);

                        this.Connected = true;

                        while (this.host.hostRunning) {
                            var timeStart = DateTime.Now;

                            Thread.Sleep(1);

                            switch (this.host.ReadServerMessageType()) {
                                case VncHost.ClientMessages.SetPixelFormat:

                                    var fb = this.host.ReadSetPixelFormat(this.frameBuffer.Width, this.frameBuffer.Height);

                                    if (fb != null) {
                                        this.frameBuffer.BitsPerPixel = fb.BitsPerPixel;
                                        this.frameBuffer.Depth = fb.Depth;
                                        this.frameBuffer.BigEndian = fb.BigEndian;
                                        this.frameBuffer.TrueColor = fb.TrueColor;
                                        this.frameBuffer.RedMax = fb.RedMax;
                                        this.frameBuffer.GreenMax = fb.GreenMax;
                                        this.frameBuffer.BlueMax = fb.BlueMax;
                                        this.frameBuffer.RedShift = fb.RedShift;
                                        this.frameBuffer.GreenShift = fb.GreenShift;
                                        this.frameBuffer.BlueShift = fb.BlueShift;
                                    }
                                    break;
                                case VncHost.ClientMessages.ReadColorMapEntries:
                                    this.host.Close();
                                    throw new NotSupportedException("Read ReadColorMapEntry");

                                case VncHost.ClientMessages.SetEncodings:
                                    this.host.ReadSetEncodings();
                                    break;

                                case VncHost.ClientMessages.FramebufferUpdateRequest:
                                    this.host.ReadFrameBufferUpdateRequest(this.frameBuffer);

                                    var now = (DateTime.Now - timeStart).TotalMilliseconds;

                                    if (now < this.DelayBetweenFrame.TotalMilliseconds) {
                                        Thread.Sleep((int)(this.DelayBetweenFrame.TotalMilliseconds - now));
                                    }

                                    break;

                                case VncHost.ClientMessages.KeyEvent:
                                    this.host.ReadKeyEvent();
                                    break;

                                case VncHost.ClientMessages.PointerEvent:
                                    this.host.ReadPointerEvent();
                                    break;

                                case VncHost.ClientMessages.ClientCutText:
                                    this.host.ReadClientCutText();
                                    break;
                            }
                        }

                    }
                }
                catch {
                    this.Connected = false;
                    this.host.Close();
                }

                Thread.Sleep(1);
            }

        }

        public void Start() {
            this.Connected = false;
            this.serverRunning = true;
            new Thread(this.Run).Start();
        }

        public void Stop() {
            this.Connected = false;
            this.serverRunning = false;
            this.host.Close();
        }

        public void Send(byte[] data, int x, int y, int width, int height) {
            if (this.frameBuffer == null)
                return;

            if ((x != 0) || (y != 0) || ((x + width) != this.Width) || ((y + height) != this.Height)) {
                throw new ArgumentException("Only full screen update is supported");
            }

            this.frameBuffer.Data = data;

            this.frameBuffer.X = x;
            this.frameBuffer.Y = y;
            this.frameBuffer.Width = width;
            this.frameBuffer.Height = height;
        }

    }
}
