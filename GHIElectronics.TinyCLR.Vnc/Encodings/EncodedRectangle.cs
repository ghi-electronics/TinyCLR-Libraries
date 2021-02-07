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

namespace GHIElectronics.TinyCLR.Vnc {

    internal abstract class EncodedRectangle {
        protected FrameBuffer framebuffer;

        protected byte[] data;
        protected int x;
        protected int y;
        protected int width;
        protected int height;
        protected int bitsPerPixel;

        public EncodedRectangle(FrameBuffer framebuffer) {
            this.framebuffer = framebuffer;

            this.x = framebuffer.X;
            this.y = framebuffer.Y;
            this.width = framebuffer.Width;
            this.height = framebuffer.Height;
            this.bitsPerPixel = this.framebuffer.BitsPerPixel;
        }

        public byte[] Data {
            get => this.data;
            set => this.data = value;
        }

        public int BitsPerPixel {
            get => this.bitsPerPixel;
            set => this.bitsPerPixel = value;
        }

        public int X {
            get => this.x;
            set => this.x = value;
        }

        public int Y {
            get => this.y;
            set => this.y = value;
        }

        public int Width {
            get => this.width;
            set => this.width = value;
        }

        public int Height {
            get => this.height;
            set => this.height = value;
        }

        public abstract void Encode();
    }
}
