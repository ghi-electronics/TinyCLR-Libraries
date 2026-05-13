using System;
using System.Drawing;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class Button : ContentControl, IDisposable {
        public ushort Alpha { get; set; } = 0xC8;
        public int RadiusBorder { get; set; } = 5;

        // Cached events - allocated once per AppDomain, not per click. Each
        // click still needs a fresh RoutedEventArgs, but the event identity
        // is constant.
        private static readonly RoutedEvent TouchDownRoutedEvent =
            new RoutedEvent("TouchDownEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
        private static readonly RoutedEvent TouchUpRoutedEvent =
            new RoutedEvent("TouchUpEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));

        // Track the parent we actually subscribed to (not just a bool) so that
        // if the button is re-parented at runtime we unsubscribe from the
        // ORIGINAL parent, not whatever this.Parent happens to return now.
        // Without this, moving a button between containers leaks the handler
        // on the old parent.
        private UIElement subscribedParent;

        public Button() {
            this.InitResource();

            this.Background = Theme.ControlSurfaceBrush;
        }

        public event RoutedEventHandler Click;

        private BitmapImage bitmapImageButtonDown;
        private BitmapImage bitmapImageButtonUp;
        private bool isPressed;

        public bool IsPressed => this.isPressed;

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

        // Re-syncs the parent TouchUp subscription if the button has been
        // re-parented since the last touch. Called from OnTouchDown.
        private void EnsureParentSubscription() {
            var current = this.Parent;
            if (this.subscribedParent == current) return;

            if (this.subscribedParent != null)
                this.subscribedParent.TouchUp -= this.OnParentTouchUp;

            this.subscribedParent = current;

            if (this.subscribedParent != null)
                this.subscribedParent.TouchUp += this.OnParentTouchUp;
        }

        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            var args = new RoutedEventArgs(TouchUpRoutedEvent, this);

            try {
                this.Click?.Invoke(this, args);
            }
            catch {

            }

            e.Handled = args.Handled;

            this.isPressed = false;

            if (this.Parent != null)
                this.Invalidate();
        }

        protected override void OnTouchDown(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            this.EnsureParentSubscription();

            var args = new RoutedEventArgs(TouchDownRoutedEvent, this);

            try {
                this.Click?.Invoke(this, args);
            }
            catch {

            }



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

                this.bitmapImageButtonDown?.graphics?.Dispose();
                this.bitmapImageButtonUp?.graphics?.Dispose();

                // Unsubscribe from the parent we ACTUALLY subscribed to, not
                // this.Parent (which may have changed via re-parenting).
                if (this.subscribedParent != null) {
                    this.subscribedParent.TouchUp -= this.OnParentTouchUp;
                    this.subscribedParent = null;
                }

                this.disposed = true;
            }
        }

        ~Button() {
            this.Dispose(false);
        }
    }
}
