using System.Drawing.Imaging;
using System.IO;

namespace System.Drawing {
    [Serializable]
    public abstract class Image : MarshalByRefObject, ICloneable, IDisposable {
        internal Graphics data;
        private bool disposed;

        public int Width => this.data.Width;
        public int Height => this.data.Height;

        public object Clone() => throw new NotImplementedException();

        public static Image FromStream(Stream stream) => new Bitmap(stream);

        public void Save(Stream stream, ImageFormat format) {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (format == null) throw new ArgumentNullException(nameof(format));
            if (format != ImageFormat.MemoryBmp) throw new ArgumentException("Only MemoryBmp supported.");

            var buf = this.data.GetBitmap();

            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(buf, 0, buf.Length);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.data.Dispose();

                this.data.callFromImage = false;

                this.disposed = true;
            }
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Image() => this.Dispose(false);
    }

    public enum BitmapImageType : byte {
        TinyCLRBitmap = 0,
        Gif = 1,
        Jpeg = 2,
        Bmp = 3 // The windows .bmp format
    }

    public sealed class Bitmap : Image {
        private Bitmap(Internal.Bitmap bmp) => this.data = new Graphics(bmp, IntPtr.Zero);
        public Bitmap(int width, int height) => this.data = new Graphics(width, height);

        public Bitmap(Stream stream) {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var buffer = new byte[(int)stream.Length];

            stream.Read(buffer, 0, buffer.Length);

            this.data = new Graphics(buffer);
        }

        public Bitmap(byte[] buffer, BitmapImageType type) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.data = new Graphics(buffer, type);
        }

        public Bitmap(byte[] buffer, int offset, int count, BitmapImageType type) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.data = new Graphics(buffer, offset, count, type);
        }

        public void SetPixel(int x, int y, Color color) => this.data.SetPixel(x, y, (uint)color.ToArgb());
        public Color GetPixel(int x, int y) => Color.FromArgb((int)this.data.GetPixel(x, y));
    }

    namespace Imaging {
        public sealed class ImageFormat {
            private static ImageFormat memoryBMP = new ImageFormat(new Guid(new byte[] { 170, 60, 107, 185, 40, 7, 211, 17, 157, 123, 0, 0, 248, 30, 243, 46 }));
            private static ImageFormat bmp = new ImageFormat(new Guid(new byte[] { 171, 60, 107, 185, 40, 7, 211, 17, 157, 123, 0, 0, 248, 30, 243, 46 }));
            private static ImageFormat emf = new ImageFormat(new Guid(new byte[] { 172, 60, 107, 185, 40, 7, 211, 17, 157, 123, 0, 0, 248, 30, 243, 46 }));
            private static ImageFormat wmf = new ImageFormat(new Guid(new byte[] { 173, 60, 107, 185, 40, 7, 211, 17, 157, 123, 0, 0, 248, 30, 243, 46 }));
            private static ImageFormat jpeg = new ImageFormat(new Guid(new byte[] { 174, 60, 107, 185, 40, 7, 211, 17, 157, 123, 0, 0, 248, 30, 243, 46 }));
            private static ImageFormat png = new ImageFormat(new Guid(new byte[] { 175, 60, 107, 185, 40, 7, 211, 17, 157, 123, 0, 0, 248, 30, 243, 46 }));
            private static ImageFormat gif = new ImageFormat(new Guid(new byte[] { 176, 60, 107, 185, 40, 7, 211, 17, 157, 123, 0, 0, 248, 30, 243, 46 }));
            private static ImageFormat tiff = new ImageFormat(new Guid(new byte[] { 177, 60, 107, 185, 40, 7, 211, 17, 157, 123, 0, 0, 248, 30, 243, 46 }));
            private static ImageFormat exif = new ImageFormat(new Guid(new byte[] { 178, 60, 107, 185, 40, 7, 211, 17, 157, 123, 0, 0, 248, 30, 243, 46 }));
            private static ImageFormat photoCD = new ImageFormat(new Guid(new byte[] { 179, 60, 107, 185, 40, 7, 211, 17, 157, 123, 0, 0, 248, 30, 243, 46 }));
            private static ImageFormat flashPIX = new ImageFormat(new Guid(new byte[] { 180, 60, 107, 185, 40, 7, 211, 17, 157, 123, 0, 0, 248, 30, 243, 46 }));
            private static ImageFormat icon = new ImageFormat(new Guid(new byte[] { 181, 60, 107, 185, 40, 7, 211, 17, 157, 123, 0, 0, 248, 30, 243, 46 }));

            public ImageFormat(Guid guid) => this.Guid = guid;

            public Guid Guid { get; }

            public static ImageFormat MemoryBmp => ImageFormat.memoryBMP;
            public static ImageFormat Bmp => ImageFormat.bmp;
            public static ImageFormat Emf => ImageFormat.emf;
            public static ImageFormat Wmf => ImageFormat.wmf;
            public static ImageFormat Gif => ImageFormat.gif;
            public static ImageFormat Jpeg => ImageFormat.jpeg;
            public static ImageFormat Png => ImageFormat.png;
            public static ImageFormat Tiff => ImageFormat.tiff;
            public static ImageFormat Exif => ImageFormat.exif;
            public static ImageFormat Icon => ImageFormat.icon;

            public override bool Equals(object o) => o is ImageFormat fmt && fmt.Guid == this.Guid;

            public override int GetHashCode() => this.Guid.GetHashCode();

            public override string ToString() {
                if (this == ImageFormat.memoryBMP) return "MemoryBMP";
                if (this == ImageFormat.bmp) return "Bmp";
                if (this == ImageFormat.emf) return "Emf";
                if (this == ImageFormat.wmf) return "Wmf";
                if (this == ImageFormat.gif) return "Gif";
                if (this == ImageFormat.jpeg) return "Jpeg";
                if (this == ImageFormat.png) return "Png";
                if (this == ImageFormat.tiff) return "Tiff";
                if (this == ImageFormat.exif) return "Exif";
                if (this == ImageFormat.icon) return "Icon";
                return "[ImageFormat: " + this.Guid + "]";
            }
        }
    }
}
