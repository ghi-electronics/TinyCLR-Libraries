using System.Drawing.Imaging;
using System.IO;

namespace System.Drawing
{
    /// <summary>Abstract base for raster images. Concrete subclass: <see cref="Bitmap"/>.</summary>
    [Serializable]
    public abstract class Image : MarshalByRefObject, ICloneable, IDisposable
    {
        internal Graphics data;
        private bool disposed;

        /// <summary>Gets the width of this image in pixels.</summary>
        public int Width => this.data.Width;
        /// <summary>Gets the height of this image in pixels.</summary>
        public int Height => this.data.Height;

        /// <summary>Creates a copy of this image.</summary>
        public object Clone() => throw new NotImplementedException();

        /// <summary>Creates an image from data in the given stream.</summary>
        public static Image FromStream(Stream stream) => new Bitmap(stream);

        /// <summary>Saves this image to the given stream in the specified format (only RawBitmap and Bmp are supported).</summary>
        public void Save(Stream stream, ImageFormat format)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (format == null) throw new ArgumentNullException(nameof(format));
            if (format != ImageFormat.RawBitmap && format != ImageFormat.Bmp) throw new ArgumentException("Only MemoryBmp and Bmp supported.");

            var buf = this.data.GetBitmap();

            if (format != ImageFormat.Bmp)
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Write(buf, 0, buf.Length);
            }
            else
            {
                var header = new byte[] {
                    0x42, 0x4D,             // BM
                    0x00, 0x00, 0x00, 0x00, // size
                    0x00, 0x00, 0x00, 0x00, // should be zero
                    0x36, 0x00, 0x00, 0x00, // Image start after header 54
                    0x28, 0x00, 0x00, 0x00, // must be 0x28
                    0x00, 0x00, 0x00, 0x00, // Width
                    0x00, 0x00, 0x00, 0x00, // Height
                    0x01, 0x00,             // Must be 1
                    0x18, 0x00,             // Bits per pixel 24
                    0x00, 0x00, 0x00, 0x00, // Compression - 0 
                    0x00, 0x00, 0x00, 0x00, // Width * Height * 2
                    0x00, 0x00, 0x00, 0x00, // 0 Not used
                    0x00, 0x00, 0x00, 0x00, // 0 Not used
                    0x00, 0x00, 0x00, 0x00, // 0 Not used
                    0x00, 0x00, 0x00, 0x00  // 0 Not used
                };

                var width16bit = new byte[this.Width * 2];
                var width24bit = new byte[this.Width * 3];

                var dataSize = (buf.Length / 2) * 3;

                var streamSizeInBytes = BitConverter.GetBytes(header.Length + dataSize);

                Array.Copy(streamSizeInBytes, 0, header, 2, 4);

                var width = BitConverter.GetBytes(this.Width);
                var heigh = BitConverter.GetBytes(this.Height);

                Array.Copy(width, 0, header, 18, 4);
                Array.Copy(heigh, 0, header, 22, 4);

                var rawSizeInBytes = BitConverter.GetBytes(dataSize);

                Array.Copy(rawSizeInBytes, 0, header, 34, 4);

                stream.Seek(0, SeekOrigin.Begin);

                stream.Write(header, 0, header.Length);

                for (var i = (buf.Length - this.Width * 2); i >= 0; i -= this.Width * 2)
                {

                    Array.Copy(buf, i, width16bit, 0, this.Width * 2);

                    Color.Convert(width16bit, width24bit, Color.ColorFormat.Rgb888, Color.RgbFormat.Bgr);

                    stream.Write(width24bit, 0, this.Width * 3);
                }
            }
        }

        /// <summary>Releases the resources used by this image.</summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.data.Dispose();

                this.data.callFromImage = false;

                this.disposed = true;
            }
        }

        /// <summary>Releases the resources used by this image.</summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Sets the color of the pixel at the given coordinates.</summary>
        public virtual void SetPixel(int x, int y, Color color) => this.data.SetPixel(x, y, color);
        /// <summary>Gets the color of the pixel at the given coordinates.</summary>
        public virtual Color GetPixel(int x, int y) => Color.FromArgb((int)this.data.GetPixel(x, y));
        /// <summary>Gets the raw pixel data of this image.</summary>
        public byte[] GetBitmap() => this.data.GetBitmap();
        /// <summary>Gets the raw pixel data for a rectangular region of this image.</summary>
        public byte[] GetBitmap(int x, int y, int width, int height) => this.data.GetBitmap(x, y, width, height);
        /// <summary>Makes the given color transparent in this image.</summary>
        public void MakeTransparent(Color color) => this.data.MakeTransparent(color);

        ~Image() => this.Dispose(false);
    }

    /// <summary>Identifies the encoded format of bitmap data.</summary>
    public enum BitmapImageType : byte
    {
        /// <summary>The native TinyCLR bitmap format.</summary>
        TinyCLRBitmap = 0,
        /// <summary>The GIF image format.</summary>
        Gif = 1,
        /// <summary>The JPEG image format.</summary>
        Jpeg = 2,
        /// <summary>The Windows .bmp format.</summary>
        Bmp = 3 // The windows .bmp format
    }

    /// <summary>A raster bitmap loaded from a resource or stream (BMP/JPEG/GIF; PNG and TIFF are not supported).</summary>
    public class Bitmap : Image
    {
        private Bitmap(Internal.Bitmap bmp) => this.data = new Graphics(bmp, IntPtr.Zero);
        /// <summary>Initializes a new blank bitmap of the given pixel size.</summary>
        public Bitmap(int width, int height) => this.data = new Graphics(width, height);
        /// <summary>Initializes a new bitmap from raw pixel data of the given pixel size.</summary>
        public Bitmap(byte[] data, int width, int height) => this.data = new Graphics(data, width, height);

        /// <summary>Initializes a new bitmap by decoding image data from the given stream.</summary>
        public Bitmap(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var buffer = new byte[(int)stream.Length];

            stream.Read(buffer, 0, buffer.Length);

            this.data = new Graphics(buffer);
        }

        /// <summary>Initializes a new bitmap by decoding image data of the given type.</summary>
        public Bitmap(byte[] buffer, BitmapImageType type)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.data = new Graphics(buffer, type);
        }

        /// <summary>Initializes a new bitmap by decoding a range of image data of the given type.</summary>
        public Bitmap(byte[] buffer, int offset, int count, BitmapImageType type)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            this.data = new Graphics(buffer, offset, count, type);
        }

        /// <summary>Sets the color of the pixel at the given coordinates.</summary>
        public override void SetPixel(int x, int y, Color color) => this.data.SetPixel(x, y, color);
        /// <summary>Gets the color of the pixel at the given coordinates.</summary>
        public override Color GetPixel(int x, int y) => Color.FromArgb((int)this.data.GetPixel(x, y));
    }

    namespace Imaging
    {
        /// <summary>Identifies the file format of an image by a unique GUID.</summary>
        public sealed class ImageFormat
        {
            private static ImageFormat rawBitmap = new ImageFormat(new Guid(new byte[] { 170, 60, 107, 185, 40, 7, 211, 17, 157, 123, 0, 0, 248, 30, 243, 46 }));
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

            /// <summary>Initializes a new image format identified by the given GUID.</summary>
            public ImageFormat(Guid guid) => this.Guid = guid;

            /// <summary>Gets the GUID that identifies this image format.</summary>
            public Guid Guid { get; }

            /// <summary>Gets the raw (uncompressed) bitmap format.</summary>
            public static ImageFormat RawBitmap => ImageFormat.rawBitmap;
            /// <summary>Gets the Windows bitmap (BMP) format.</summary>
            public static ImageFormat Bmp => ImageFormat.bmp;
            /// <summary>Gets the enhanced metafile (EMF) format.</summary>
            public static ImageFormat Emf => ImageFormat.emf;
            /// <summary>Gets the Windows metafile (WMF) format.</summary>
            public static ImageFormat Wmf => ImageFormat.wmf;
            /// <summary>Gets the GIF format.</summary>
            public static ImageFormat Gif => ImageFormat.gif;
            /// <summary>Gets the JPEG format.</summary>
            public static ImageFormat Jpeg => ImageFormat.jpeg;
            /// <summary>Gets the PNG format.</summary>
            public static ImageFormat Png => ImageFormat.png;
            /// <summary>Gets the TIFF format.</summary>
            public static ImageFormat Tiff => ImageFormat.tiff;
            /// <summary>Gets the EXIF format.</summary>
            public static ImageFormat Exif => ImageFormat.exif;
            /// <summary>Gets the icon format.</summary>
            public static ImageFormat Icon => ImageFormat.icon;

            /// <summary>Determines whether the specified object is an image format with the same GUID.</summary>
            public override bool Equals(object o) => o is ImageFormat fmt && fmt.Guid == this.Guid;

            /// <summary>Returns a hash code for this image format.</summary>
            public override int GetHashCode() => this.Guid.GetHashCode();

            /// <summary>Returns the name of this image format.</summary>
            public override string ToString()
            {
                if (this == ImageFormat.rawBitmap) return "RawBitmap";
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
