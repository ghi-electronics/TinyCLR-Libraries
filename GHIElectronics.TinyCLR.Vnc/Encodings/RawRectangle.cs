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
                    // TODO
                    break;

                case 8:
                    this.pixels = new byte[this.Width * this.Height];
                    break;
            }
        }

        public override void Encode() {

            switch (this.framebuffer.BitsPerPixel) {
                case 32:
                    Color.Convert(this.framebuffer.Screen.GetBitmap(), Color.RgbFormat.Rgb565, this.pixels, Color.RgbFormat.Rgb8888);
                    BitConverter.SwapEndianness(this.pixels, 4);

                    break;

                case 16:
                    // TODO
                    break;

                case 8:
                    Color.Convert(this.framebuffer.Screen.GetBitmap(), Color.RgbFormat.Rgb565, this.pixels, Color.RgbFormat.Rgb323);
                    break;
            }

        }
    }
}
