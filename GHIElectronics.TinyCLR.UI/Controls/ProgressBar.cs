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

    public class ProgressBar : Image, IDisposable {
        private BitmapImage bitmapImageProgressBar;
        private BitmapImage bitmapImageProgressBarFill;

        public Direction Direction { get; set; } = Direction.Right;
        public int MaxValue { get; set; } = 100;
        public int Value { get; set; } = 0;
        public ushort Alpha { get; set; } = Theme.DefaultAlpha;

        /// <summary>Corner radius in pixels for the Scale9Image-rendered bar.</summary>
        public int RadiusBorder { get; set; } = Theme.DefaultRadiusBorder;

        /// <summary>Legacy alias for <see cref="RadiusBorder"/>. Will be removed.</summary>
        [Obsolete("Renamed to RadiusBorder for parity with the other Scale9 controls.")]
        public int Border {
            get => this.RadiusBorder;
            set => this.RadiusBorder = value;
        }


        private void InitResource() {
            this.bitmapImageProgressBar = Resources.LoadBitmapImage(Resources.BitmapResources.ProgressBar);
            this.bitmapImageProgressBarFill = Resources.LoadBitmapImage(Resources.BitmapResources.ProgressBar_Fill);
        }

        public ProgressBar() : base() => this.InitResource();

        public override void OnRender(DrawingContext dc) {
            var fullW = this.ActualWidth;
            var fullH = this.ActualHeight;
            if (fullW <= 0 || fullH <= 0) return;

            // Avoid divide-by-zero if MaxValue was zero'd by a caller.
            var max = this.MaxValue == 0 ? 1 : this.MaxValue;

            dc.Scale9Image(0, 0, fullW, fullH, this.bitmapImageProgressBar, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.Alpha);

            var x = 1;
            var y = 1;

            var width = fullW;
            var height = fullH;
            var size = (float)this.Value / (float)max;

            if (this.Direction == Direction.Right || this.Direction == Direction.Left) {
                width = (int)((fullW - 2) * size);
                height = fullH - 2;
            }
            else if (this.Direction == Direction.Up || this.Direction == Direction.Down) {
                width = fullW - 2;
                height = (int)((fullH - 2) * size);
            }

            if (this.Direction == Direction.Left) {
                x += (fullW - 2) - width;
            }
            else if (this.Direction == Direction.Up) {
                y += (fullH - 2) - height;
            }

            if (width > 0 && height > 0)
                dc.Scale9Image(x, y, width, height, this.bitmapImageProgressBarFill, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.Alpha);
        }

        private bool disposed;

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (this.disposed) return;

            if (disposing) {
                this.bitmapImageProgressBar?.graphics?.Dispose();
                this.bitmapImageProgressBarFill?.graphics?.Dispose();
            }

            this.disposed = true;
        }

        ~ProgressBar() {
            this.Dispose(false);
        }

    }
}
