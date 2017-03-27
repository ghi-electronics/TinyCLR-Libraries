using System;

namespace GHIElectronics.TinyCLR.Drawing {
    [Serializable]
    public abstract class Image : MarshalByRefObject, ICloneable, IDisposable {
        internal readonly Graphics data;
        private bool disposed;

        public int Height { get; }
        public int Width { get; }

        protected Image(int width, int height, Graphics data) {
            this.Width = width;
            this.Height = height;
            this.data = data;
        }

        public abstract object Clone();

        protected virtual void Dispose(bool disposing) {
            if (disposing && !this.disposed) {
                this.data.Dispose();

                this.disposed = true;
            }
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Image() => this.Dispose(false);
    }

    public sealed class Bitmap : Image {
        public Bitmap(int width, int height) : base(width, height, Graphics.CreateGraphics(width, height)) { }

        public override object Clone() => throw new NotSupportedException();
    }
}
