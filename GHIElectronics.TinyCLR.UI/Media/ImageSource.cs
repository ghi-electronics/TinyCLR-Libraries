using System.Drawing;

namespace GHIElectronics.TinyCLR.UI.Media {
    public abstract class ImageSource {
        internal readonly Graphics graphics;

        public virtual int Width => this.graphics.Width;
        public virtual int Height => this.graphics.Height;

        protected ImageSource(Graphics g) => this.graphics = g;
    }

    namespace Imaging {
        public abstract class BitmapSource : ImageSource {
            protected BitmapSource(Graphics g) : base(g) {

            }
        }

        public class BitmapImage : BitmapSource {
            private BitmapImage(Graphics g) : base(g) {

            }

            public static BitmapImage FromGraphics(Graphics g) => new BitmapImage(g);
        }
    }
}
