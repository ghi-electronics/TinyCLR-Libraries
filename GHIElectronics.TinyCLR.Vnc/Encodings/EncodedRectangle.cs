namespace GHIElectronics.TinyCLR.Vnc {

    internal abstract class EncodedRectangle {
        protected FrameBuffer framebuffer;

        protected byte[] pixels;
        protected int x;
        protected int y;
        protected int width;
        protected int height;
        protected int bitsPerPixel;

        public EncodedRectangle(FrameBuffer framebuffer, int x, int y, int width, int height) {
            this.framebuffer = framebuffer;

            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.bitsPerPixel = this.framebuffer.BitsPerPixel;
        }

        public byte[] Pixels {
            get => this.pixels;
            set => this.pixels = value;
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
