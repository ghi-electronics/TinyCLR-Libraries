using System.Drawing;

namespace GHIElectronics.TinyCLR.UI.Media {
    /// <summary>Base class for an image that can be drawn.</summary>
    public abstract class ImageSource {
        internal readonly Graphics graphics;

        /// <summary>The width of the image in pixels.</summary>
        public virtual int Width => this.graphics.Width;
        /// <summary>The height of the image in pixels.</summary>
        public virtual int Height => this.graphics.Height;

        /// <summary>Creates an image source backed by the given graphics.</summary>
        protected ImageSource(Graphics g) => this.graphics = g;
    }

    namespace Imaging {
        /// <summary>Base class for image sources backed by a bitmap.</summary>
        public abstract class BitmapSource : ImageSource {
            /// <summary>Creates a bitmap source backed by the given graphics.</summary>
            protected BitmapSource(Graphics g) : base(g) {

            }
        }

        /// <summary>An image source created from a bitmap.</summary>
        public class BitmapImage : BitmapSource {
            private BitmapImage(Graphics g) : base(g) {

            }

            /// <summary>Creates a bitmap image from the given graphics.</summary>
            public static BitmapImage FromGraphics(Graphics g) => new BitmapImage(g);
        }
    }
}
