using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public enum Direction {
        Left,
        Right,
        Up,
        Down
    }

    public class ProgressBar : Image {
        private BitmapImage bitmapImageProgressBar;
        private BitmapImage bitmapImageProgressBarFill;

        public Direction Direction { get; set; } = Direction.Right;
        public int MaxValue { get; set; } = 100;
        public int Value { get; set; } = 0;
        public ushort Alpha { get; set; } = 0xC8;
        public int Border { get; set; } = 5;


        private void InitResource() {
            this.bitmapImageProgressBar = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.ProgressBar)));
            this.bitmapImageProgressBarFill = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.ProgressBar_Fill)));
        }

        public ProgressBar() : base() => this.InitResource();

        public override void OnRender(DrawingContext dc) {
            var x = 0;
            var y = 0;

            dc.Scale9Image(0, 0, this.Width, this.Height, this.bitmapImageProgressBar, this.Border, this.Border, this.Border, this.Border, this.Alpha);

            x += 1;
            y += 1;

            var width = this.Width;
            var height = this.Height;
            var size = (float)this.Value / (float)this.MaxValue;

            if (this.Direction == Direction.Right || this.Direction == Direction.Left) {
                width = (int)((this.Width - 2) * size);
                height = this.Height - 2;
            }
            else if (this.Direction == Direction.Up || this.Direction == Direction.Down) {
                width = this.Width - 2;
                height = (int)((this.Height - 2) * size);
            }

            if (this.Direction == Direction.Left) {
                x += (this.Width - 2) - width;
            }
            else if (this.Direction == Direction.Up) {
                y += (this.Height - 2) - height;
            }

            dc.Scale9Image(x, y, width, height, this.bitmapImageProgressBarFill, this.Border, this.Border, this.Border, this.Border, this.Alpha);
        }

        public void Dispose() {
            this.bitmapImageProgressBar.graphics.Dispose();
            this.bitmapImageProgressBarFill.graphics.Dispose();
        }

    }
}
