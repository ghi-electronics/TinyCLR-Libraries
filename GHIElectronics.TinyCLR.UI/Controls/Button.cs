using System;
using System.Drawing;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class Button : ContentControl, IDisposable {
        public ushort Alpha { get; set; } = 0xC8;
        public int RadiusBorder { get; set; } = 5;
        private bool isTouchParentAssigned = false;

        public Button() {
            this.InitResource();

            this.Background = new SolidColorBrush(Colors.Gray);
        }

        public event RoutedEventHandler Click;

        private BitmapImage bitmapImageButtonDown;
        private BitmapImage bitmapImageButtonUp;
        private bool isPressed;

        private void InitResource() {
            this.bitmapImageButtonDown = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.Button_Down)));
            this.bitmapImageButtonUp = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.Button_Up)));
        }

        private void OnParentTouchUp(object sender, TouchEventArgs e) {
            if (this.isPressed) {
                this.isPressed = false;
                this.Invalidate();
            }
        }

        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            var evt = new RoutedEvent("TouchUpEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
            var args = new RoutedEventArgs(evt, this);

            this.Click?.Invoke(this, args);

            e.Handled = args.Handled;

            this.isPressed = false;

            if (this.Parent != null)
                this.Invalidate();
        }

        protected override void OnTouchDown(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            if (!this.isTouchParentAssigned) {
                if (this.Parent != null) {
                    this.Parent.TouchUp += this.OnParentTouchUp;
                    this.isTouchParentAssigned = true;
                }
            }

            var evt = new RoutedEvent("TouchDownEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
            var args = new RoutedEventArgs(evt, this);

            this.Click?.Invoke(this, args);

            e.Handled = args.Handled;

            this.isPressed = true;

            if (this.Parent != null)
                this.Invalidate();

        }

        public override void OnRender(DrawingContext dc) {
            var alpha = (this.IsEnabled) ? this.Alpha : (ushort)(this.Alpha / 2);

            if (this.isPressed && this.IsEnabled)
                dc.Scale9Image(0, 0, this.Width, this.Height, this.bitmapImageButtonDown, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, alpha);
            else
                dc.Scale9Image(0, 0, this.Width, this.Height, this.bitmapImageButtonUp, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, alpha);
        }

        private bool disposed;

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {

                this.bitmapImageButtonDown.graphics.Dispose();
                this.bitmapImageButtonUp.graphics.Dispose();

                if (this.Parent != null && this.isTouchParentAssigned) {
                    this.Parent.TouchUp -= this.OnParentTouchUp;
                }

                this.disposed = true;
            }
        }

        ~Button() {
            this.Dispose(false);
        }
    }
}
