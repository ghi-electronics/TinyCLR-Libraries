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
using System.Drawing;
using System.Threading;

namespace GHIElectronics.TinyCLR.Vnc {
    public class VncServer {

        public delegate void PointerChangedEventHandler(int x, int y, bool pressed);
        public delegate void KeyChangedEventHandler(uint key, bool pressed);

        public event PointerChangedEventHandler PointerChangedEvent;
        public event KeyChangedEventHandler KeyChangedEvent;

        public int Port { get; private set; }
        internal string Password { get; private set; }
        public string ServerName { get; private set; } = "Default";

        private VncHost host;
        private FrameBuffer fb;
        private Graphics screen;

        public Graphics Screen {
            get => this.screen;
            set {
                this.screen = value;

                this.fb = new FrameBuffer(this.screen.Width, this.screen.Height) {
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
            }
        }

        public VncServer(string serverName, int port) {
            this.Password = null;
            this.Port = port;
            this.ServerName = serverName;
        }

        private void Run() {
            while ((this.host.isRunning)) {
                Thread.Sleep(1);
                try {
                    switch (this.host.ReadServerMessageType()) {
                        case VncHost.ClientMessages.SetPixelFormat:

                            var f = this.host.ReadSetPixelFormat(this.fb.Width, this.fb.Height);

                            if (f != null) {
                                this.fb = f;

                                this.fb.Screen = this.Screen;
                                this.fb.ServerName = this.ServerName;
                            }
                            break;
                        case VncHost.ClientMessages.ReadColorMapEntries:
                            this.host.Close();
                            throw new NotSupportedException("Read ReadColorMapEntry");

                        case VncHost.ClientMessages.SetEncodings:
                            this.host.ReadSetEncodings();
                            break;

                        case VncHost.ClientMessages.FramebufferUpdateRequest:
                            this.host.ReadFrameBufferUpdateRequest(this.fb);
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
                catch {

                }
            }
        }

        public void Start() {

            if (string.IsNullOrEmpty(this.ServerName))
                throw new ArgumentNullException("Name", "The VNC Server Name cannot be empty.");
            if (this.Port == 0)
                throw new ArgumentNullException("Port", "The VNC Server port cannot be zero.");

            this.host = new VncHost(this.Port);

            this.host.KeyChangedEvent += (a, b) => KeyChangedEvent?.Invoke(a, b);
            this.host.PointerChangedEvent += (a, b, c) => PointerChangedEvent?.Invoke(a, b, c);

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

                this.host.WriteServerInit(this.fb);

                new Thread(this.Run).Start();
            }
        }

        public void Stop() => this.host.Close();

        public void Pause() => this.host.Pause = true;

        public void Resume() => this.host.Pause = false;

        public void SetUpdateWindow(int x, int y, int width, int height) {
            var w = this.fb.Width;
            var h = this.fb.Height;

            if ((x < 0) || (y < 0) || (width <= 0) || (height <= 0)) {
                throw new ArgumentException();
            }

            if (x + width > w) {
                throw new ArgumentException();
            }

            if (y + height > h) {
                throw new ArgumentException();
            }

            this.host.SetUpdateWindow(x, y, width, height);
            //throw new NotSupportedException();


        }

    }
}
