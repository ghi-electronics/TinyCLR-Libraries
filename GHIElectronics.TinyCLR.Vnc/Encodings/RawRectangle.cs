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

namespace GHIElectronics.TinyCLR.Vnc {

    internal sealed class RawRectangle : EncodedRectangle {

        public RawRectangle(FrameBuffer framebuffer, int x, int y, int width, int height) : base(framebuffer, x, y, width, height) {
            switch (this.framebuffer.BitsPerPixel) {
                case 32:
                    this.pixels = new byte[this.Width * this.Height * 4];

                    break;

                case 16:

                    if (x == 0 && y == 0 && width == framebuffer.Width && height == framebuffer.Height) {
                        this.pixels = this.framebuffer.Screen.GetBitmap();
                    }
                    else {
                        this.pixels = this.framebuffer.Screen.GetBitmap(x, y, width, height);
                    }

                    break;

                case 8:
                    this.pixels = new byte[this.Width * this.Height];
                    break;
            }
        }

        public override void Encode() {
            var rgbFormat = Color.RgbFormat.Rgb8888;
            var bytesPerBit = 4;


            switch (this.framebuffer.BitsPerPixel) {
                case 32:

                    break;

                case 16:
                    rgbFormat = Color.RgbFormat.Rgb565;
                    bytesPerBit = 2;
                    break;

                case 8:
                    rgbFormat = Color.RgbFormat.Rgb323;
                    bytesPerBit = 1;
                    break;
            }

            if (this.framebuffer.BitsPerPixel != 16) {
                if (this.framebuffer.Width * this.framebuffer.Height == this.pixels.Length / bytesPerBit) {
                    var data = this.framebuffer.Screen.GetBitmap();
                    Color.Convert(data, Color.RgbFormat.Rgb565, this.pixels, rgbFormat);
                }
                else {
                    var data = this.framebuffer.Screen.GetBitmap(this.X, this.Y, this.Width, this.Height);
                    Color.Convert(data, Color.RgbFormat.Rgb565, this.pixels, rgbFormat);
                }
            }


            BitConverter.SwapEndianness(this.pixels, bytesPerBit);

        }
    }
}
