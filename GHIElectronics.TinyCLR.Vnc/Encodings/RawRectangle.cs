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
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Vnc {

    internal sealed class RawRectangle : EncodedRectangle, IDisposable {
        private UnmanagedBuffer unmanagedBuffer;

        public RawRectangle(FrameBuffer framebuffer) : base(framebuffer) {
            if (Memory.UnmanagedMemory.FreeBytes > 0) { // Unmanaged memory ready

                this.unmanagedBuffer = new UnmanagedBuffer(this.Width * this.Height * (this.framebuffer.BitsPerPixel / 8));
                this.data = this.unmanagedBuffer.Bytes;

            }
            else {
                this.data = new byte[this.Width * this.Height * (this.framebuffer.BitsPerPixel / 8)];
            }
        }

        public void Dispose() {
            if (this.unmanagedBuffer != null ) {
                this.data = null;
                this.unmanagedBuffer.Dispose();
                this.unmanagedBuffer = null;
            }
        }

        public override void Encode() {
            var colorFormat = Color.ColorFormat.Rgb8888;
            var bytesPerBit = 4;


            switch (this.framebuffer.BitsPerPixel) {
                case 32:

                    break;

                case 16:
                    colorFormat = Color.ColorFormat.Rgb565;
                    bytesPerBit = 2;
                    break;

                case 8:
                    colorFormat = Color.ColorFormat.Rgb332;
                    bytesPerBit = 1;
                    break;
            }

            Color.Convert(this.framebuffer.Data, this.data, colorFormat);
            BitConverter.SwapEndianness(this.data, bytesPerBit);

        }

        ~RawRectangle() {
            this.Dispose();
        }
    }
}
