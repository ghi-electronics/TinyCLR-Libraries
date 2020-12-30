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
namespace GHIElectronics.TinyCLR.Vnc {

    internal class FrameBuffer {
        private string name;

        private int bpp;
        private int depth;
        private bool bigEndian;
        private bool trueColor;
        private int redMax;
        private int greenMax;
        private int blueMax;
        private int redShift;
        private int greenShift;
        private int blueShift;

        private readonly int x;
        private readonly int y;
        private readonly int width;
        private readonly int height;

        private Graphics graphic;

        public FrameBuffer(int width, int height) {
            this.x = 0;
            this.y = 0;
            this.width = width;
            this.height = height;
        }

        public Graphics Screen {
            get => this.graphic;
            set => this.graphic = value;
        }

        public int X => this.x;

        public int Y => this.y;

        public int Width => this.width;

        public int Height => this.height;

        public int BitsPerPixel {
            get => this.bpp;
            set {
                if (value == 32 || value == 16 || value == 8)
                    this.bpp = value;
                else throw new ArgumentException("Wrong value for BitsPerPixel");
            }
        }

        public int Depth {
            get => this.depth;
            set => this.depth = value;
        }

        public bool BigEndian {
            get => this.bigEndian;
            set => this.bigEndian = value;
        }

        public bool TrueColor {
            get => this.trueColor;
            set => this.trueColor = value;
        }

        public int RedMax {
            get => this.redMax;
            set => this.redMax = value;
        }

        public int GreenMax {
            get => this.greenMax;
            set => this.greenMax = value;
        }

        public int BlueMax {
            get => this.blueMax;
            set => this.blueMax = value;
        }

        public int RedShift {
            get => this.redShift;
            set => this.redShift = value;
        }

        public int GreenShift {
            get => this.greenShift;
            set => this.greenShift = value;
        }

        public int BlueShift {
            get => this.blueShift;
            set => this.blueShift = value;
        }

        public string ServerName {
            get => this.name;
            set => this.name = value ?? throw new ArgumentNullException("ServerName");
        }

        public byte[] ToPixelFormat() {
            var b = new byte[16];

            b[0] = (byte)(this.bpp);
            b[1] = (byte)(this.depth);
            b[2] = (byte)(this.bigEndian ? 1 : 0);
            b[3] = (byte)(this.trueColor ? 1 : 0);
            b[4] = (byte)((this.redMax >> 8) & 0xff);
            b[5] = (byte)(this.redMax & 0xff);
            b[6] = (byte)((this.greenMax >> 8) & 0xff);
            b[7] = (byte)(this.greenMax & 0xff);
            b[8] = (byte)((this.blueMax >> 8) & 0xff);
            b[9] = (byte)(this.blueMax & 0xff);
            b[10] = (byte)(this.redShift);
            b[11] = (byte)(this.greenShift);
            b[12] = (byte)(this.blueShift);
            // plus 3 bytes padding = 16 bytes

            return b;
        }

        public static FrameBuffer FromPixelFormat(byte[] b, int width, int height) {
            if (b.Length != 16)
                throw new ArgumentException("Length of b must be 16 bytes.");

            var buffer = new FrameBuffer(width, height) {
                BitsPerPixel = (int)(b[0]),
                Depth = (int)(b[1]),
                BigEndian = (b[2] != 0),
                TrueColor = (b[3] != 0),
                RedMax = (int)(b[5] | b[4] << 8),
                GreenMax = (int)(b[7] | b[6] << 8),
                BlueMax = (int)(b[9] | b[8] << 8),
                RedShift = (int)(b[10]),
                GreenShift = (int)(b[11]),
                BlueShift = (int)(b[12])
            };
            // Last 3 bytes are padding, ignore									

            return buffer;
        }
    }
}
