using System;
using System.Drawing;
using GHIElectronics.TinyCLR.UI;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A clickable push button that raises <see cref="Click"/> when activated.</summary>
    public class Button : ContentControl, IDisposable {
        /// <summary>Opacity (0-255) used when drawing the button image.</summary>
        public ushort Alpha { get; set; } = Theme.DefaultAlpha;
        /// <summary>Corner radius used by the nine-slice button image.</summary>
        public int RadiusBorder { get; set; } = Theme.DefaultRadiusBorder;
        /// <summary>Optional custom face image, nine-slice scaled with <see cref="RadiusBorder"/>. When set it replaces
        /// the default themed button skin (and takes priority over <see cref="Control.Background"/>).</summary>
        public ImageSource BackgroundImage { get; set; }

        /// <summary>Determines whether <see cref="Click"/> fires on touch press or release. Default is
        /// <see cref="ClickMode.Release"/> (WPF convention); set to <see cref="ClickMode.Press"/> for the
        /// instant TinyCLR 2.x response (fires the moment the button is touched).</summary>
        public ClickMode ClickMode { get; set; } = ClickMode.Release;

        // Cached events - allocated once per AppDomain, not per click. Each
        // click still needs a fresh RoutedEventArgs, but the event identity
        // is constant.
        private static readonly RoutedEvent ClickRoutedEvent =
            new RoutedEvent("ClickEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));

        // Track the parent we actually subscribed to (not just a bool) so that
        // if the button is re-parented at runtime we unsubscribe from the
        // ORIGINAL parent, not whatever this.Parent happens to return now.
        // Without this, moving a button between containers leaks the handler
        // on the old parent.
        private UIElement subscribedParent;

        /// <summary>Creates a new Button.</summary>
        public Button() {
            this.InitResource();

            // Background is left null so a default button paints the themed nine-slice skin (see OnRender). Set
            // Background to a SolidColorBrush for a flat coloured button, or BackgroundImage for a custom face.

            // A button centers its content by default (WPF/WinForms convention), so a
            // Text child lands in the middle instead of the top-left of the button face.
            this.HorizontalContentAlignment = HorizontalAlignment.Center;
            this.VerticalContentAlignment = VerticalAlignment.Center;
        }

        /// <summary>Places the content (<see cref="ContentControl.Child"/>) inside the button face according to
        /// <see cref="ContentControl.HorizontalContentAlignment"/> / <see cref="ContentControl.VerticalContentAlignment"/>.
        /// For the default Center/Center this sizes the child to its desired size and centers it; Stretch fills the
        /// face (the pre-existing behavior).</summary>
        protected override void ArrangeOverride(int arrangeWidth, int arrangeHeight) {
            var child = this.Child;
            if (child == null) {
                return;
            }

            child.GetDesiredSize(out var desiredWidth, out var desiredHeight);
            ArrangeContentAxis((int)this.HorizontalContentAlignment, arrangeWidth, desiredWidth, out var x, out var w);
            ArrangeContentAxis((int)this.VerticalContentAlignment, arrangeHeight, desiredHeight, out var y, out var h);
            child.Arrange(x, y, w, h);
        }

        // Shared by both axes (HorizontalAlignment and VerticalAlignment share Stretch/near/center/far ordinals).
        private static void ArrangeContentAxis(int alignment, int container, int desired, out int offset, out int size) {
            // Stretch (0): fill the face; the child's own alignment then applies inside.
            if (alignment == (int)HorizontalAlignment.Stretch) {
                offset = 0;
                size = container;
                return;
            }

            size = (desired < container) ? desired : container;

            // Left/Top (1) = start, Center (2), Right/Bottom (3) = end.
            if (alignment == (int)HorizontalAlignment.Center) {
                offset = (container - size) / 2;
            }
            else if (alignment == (int)HorizontalAlignment.Right) {
                offset = container - size;
            }
            else {
                offset = 0;
            }
        }

        /// <summary>Raised when the button is clicked.</summary>
        public event RoutedEventHandler Click;

        private BitmapImage bitmapImageButtonDown;
        private BitmapImage bitmapImageButtonUp;
        private bool isPressed;

        /// <summary>True while the button is held down.</summary>
        public bool IsPressed => this.isPressed;

        private void InitResource() {
            this.bitmapImageButtonDown = Resources.LoadBitmapImage(Resources.BitmapResources.Button_Down);
            this.bitmapImageButtonUp = Resources.LoadBitmapImage(Resources.BitmapResources.Button_Up);
        }

        private void OnParentTouchUp(object sender, TouchEventArgs e) {
            // Handles the drag-off-and-release-elsewhere case: the touch went
            // down on us (isPressed=true) but the user released on a different
            // element, so our own OnTouchUp never fires. The release bubbles
            // through a common ancestor we're subscribed to, which lets us
            // clear isPressed and repaint as unpressed.
            //
            // Bubble routing means our own OnTouchUp has already fired first
            // for on-button releases, so isPressed is already false when this
            // runs in that case — no special guard needed.
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

        /// <summary>Handles touch release; fires Click if the press started on this button.</summary>
        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            // Only fire Click if the press actually started on this Button.
            // OnParentTouchUp clears isPressed when the user releases off-button,
            // so a press-then-drag-away-then-release-elsewhere yields no Click.
            var wasPressed = this.isPressed;
            this.isPressed = false;

            if (this.Parent != null)
                this.Invalidate();

            // In Press mode the click already fired on TouchDown; don't fire it twice.
            if (wasPressed && this.ClickMode != ClickMode.Press) {
                e.Handled = this.PerformClick();
            }
        }

        /// <summary>Handles touch press; marks the button as pressed (and, in <see cref="ClickMode.Press"/>
        /// mode, fires <see cref="Click"/> immediately).</summary>
        protected override void OnTouchDown(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            this.EnsureParentSubscription();

            this.isPressed = true;
            e.Handled = true;

            if (this.Parent != null)
                this.Invalidate();

            // ClickMode.Press: activate on press (the TinyCLR 2.x behavior). The default
            // Release mode fires in OnTouchUp instead.
            if (this.ClickMode == ClickMode.Press) {
                this.PerformClick();
            }
        }

        /// <summary>Handles the Select hardware button press; marks the button as pressed.</summary>
        protected override void OnButtonDown(ButtonEventArgs e) {
            if (!this.IsEnabled || e.Button != HardwareButton.Select) {
                return;
            }

            this.isPressed = true;
            e.Handled = true;

            if (this.Parent != null)
                this.Invalidate();
        }

        /// <summary>Handles the Select hardware button release; fires Click if it was pressed.</summary>
        protected override void OnButtonUp(ButtonEventArgs e) {
            if (!this.IsEnabled || e.Button != HardwareButton.Select) {
                return;
            }

            var wasPressed = this.isPressed;
            this.isPressed = false;

            if (this.Parent != null)
                this.Invalidate();

            if (wasPressed) {
                e.Handled = this.PerformClick();
            }
        }

        // Fires Click. Returns the args.Handled flag so callers can propagate
        // it to TouchEventArgs/ButtonEventArgs.Handled. Exceptions from user
        // handlers propagate — the framework should not silently swallow them.
        private bool PerformClick() {
            var args = new RoutedEventArgs(ClickRoutedEvent, this);
            this.Click?.Invoke(this, args);
            return args.Handled;
        }

        /// <summary>Draws the button in its pressed or unpressed state.</summary>
        public override void OnRender(DrawingContext dc) {
            // ActualWidth/ActualHeight are populated by Arrange and never throw,
            // unlike this.Width/Height which fire "width not set" if the caller
            // forgot to assign them — and a single exception out of OnRender
            // aborts paint for every sibling on the same canvas.
            var w = this.ActualWidth;
            var h = this.ActualHeight;
            if (w <= 0 || h <= 0) return;

            var alpha = (this.IsEnabled) ? this.Alpha : (ushort)(this.Alpha / 2);
            var pressed = this.isPressed && this.IsEnabled;

            // 1) Custom face image (nine-slice) — highest priority.
            if (this.BackgroundImage != null) {
                var a = pressed ? (ushort)(alpha * 3 / 4) : alpha; // dim slightly when pressed
                dc.Scale9Image(0, 0, w, h, this.BackgroundImage, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, a);
                return;
            }

            // 2) Flat colour — when the caller set a SolidColorBrush Background. Fill + a darker border, darkened on press.
            if (this.Background is SolidColorBrush scb) {
                // Media.-qualified: the dual-mode Desktop build also has System.Drawing.Color/Pen in scope.
                var c = scb.Color;
                if (pressed) {
                    c = Media.Color.FromArgb(c.A, (byte)(c.R * 4 / 5), (byte)(c.G * 4 / 5), (byte)(c.B * 4 / 5));
                }

                var border = Media.Color.FromArgb(0xFF, (byte)(c.R * 3 / 5), (byte)(c.G * 3 / 5), (byte)(c.B * 3 / 5));
                dc.DrawRectangle(new SolidColorBrush(c), new Media.Pen(border, 1), 0, 0, w, h);
                return;
            }

            // 3) Default themed skin.
            var img = pressed ? this.bitmapImageButtonDown : this.bitmapImageButtonUp;
            dc.Scale9Image(0, 0, w, h, img, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, alpha);
        }

        private bool disposed;

        /// <summary>Releases the resources used by the button.</summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the button's bitmap resources and event subscriptions.</summary>
        protected virtual void Dispose(bool disposing) {
            if (this.disposed) return;

            if (disposing) {
                // Managed cleanup — only safe when called from explicit Dispose(),
                // not from the finalizer (where dependent managed objects may
                // already be gone or about to finalize themselves).
                this.bitmapImageButtonDown?.graphics?.Dispose();
                this.bitmapImageButtonUp?.graphics?.Dispose();

                if (this.subscribedParent != null) {
                    this.subscribedParent.TouchUp -= this.OnParentTouchUp;
                    this.subscribedParent = null;
                }
            }

            this.disposed = true;
        }

        ~Button() {
            this.Dispose(false);
        }
    }
}
