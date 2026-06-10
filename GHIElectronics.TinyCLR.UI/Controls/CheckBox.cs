using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    /// <summary>A two-state check box that toggles when clicked.</summary>
    public class CheckBox : ContentControl, IDisposable {
        // Cached once per AppDomain so every toggle doesn't allocate fresh RoutedEvent objects.
        private static readonly RoutedEvent ClickRoutedEvent =
            new RoutedEvent("ClickEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
        private static readonly RoutedEvent CheckedRoutedEvent =
            new RoutedEvent("CheckedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
        private static readonly RoutedEvent UncheckedRoutedEvent =
            new RoutedEvent("UncheckedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));

        /// <summary>Raised when the check box is clicked.</summary>
        public event RoutedEventHandler Click;
        /// <summary>Raised when the check box becomes checked.</summary>
        public event RoutedEventHandler Checked;
        /// <summary>Raised when the check box becomes unchecked.</summary>
        public event RoutedEventHandler Unchecked;

        private BitmapImage bitmapImageCheckboxOn;
        private BitmapImage bitmapImageCheckboxOff;

        private bool isChecked = false;

        /// <summary>Optional name identifying the check box.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>Opacity (0-255) used when drawing the check box image.</summary>
        public ushort Alpha { get; set; } = Theme.DefaultAlpha;
        /// <summary>Corner radius used by the nine-slice check box image.</summary>
        public ushort RadiusBorder { get; set; } = (ushort)Theme.DefaultRadiusBorder;


        private void InitResource() {
            this.bitmapImageCheckboxOn = Resources.LoadBitmapImage(Resources.BitmapResources.CheckBox_On);
            this.bitmapImageCheckboxOff = Resources.LoadBitmapImage(Resources.BitmapResources.CheckBox_Off);
        }

        /// <summary>Creates a new CheckBox.</summary>
        public CheckBox() : base() {
            this.InitResource();

            this.Width = this.bitmapImageCheckboxOn.Width;
            this.Height = this.bitmapImageCheckboxOn.Height;
        }

        /// <summary>Draws the check box in its checked or unchecked state.</summary>
        public override void OnRender(DrawingContext dc) {
            // The ctor pre-sets Width/Height from the bitmap, so this normally
            // wouldn't throw — but a caller can still override Width/Height
            // back to unset via reflection or future API changes. Using
            // ActualWidth/ActualHeight keeps the paint pass alive regardless.
            var w = this.ActualWidth;
            var h = this.ActualHeight;
            if (w <= 0 || h <= 0) return;

            var img = this.isChecked ? this.bitmapImageCheckboxOn : this.bitmapImageCheckboxOff;
            dc.Scale9Image(0, 0, w, h, img, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.Alpha);
        }

        /// <summary>Handles touch release; toggles the check box.</summary>
        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            e.Handled = this.PerformClick();
        }

        /// <summary>Handles touch press. Toggling is deferred to touch release.</summary>
        protected override void OnTouchDown(TouchEventArgs e) {
            // No-op. Toggling happens on TouchUp so that drag-off-and-release-elsewhere
            // doesn't flip state, and Click fires exactly once per activation.
        }

        /// <summary>Handles the Select hardware button release; toggles the check box.</summary>
        protected override void OnButtonUp(ButtonEventArgs e) {
            if (!this.IsEnabled || e.Button != HardwareButton.Select) {
                return;
            }

            e.Handled = this.PerformClick();
        }

        // Toggles checked state and raises Click + Checked/Unchecked.
        // Returns the Click args.Handled flag so callers can forward it.
        // Exceptions from user handlers propagate — the framework should not
        // silently swallow them.
        private bool PerformClick() {
            var clickArgs = new RoutedEventArgs(ClickRoutedEvent, this);
            this.Click?.Invoke(this, clickArgs);

            this.IsChecked = !this.IsChecked;

            var stateArgs = new RoutedEventArgs(this.isChecked ? CheckedRoutedEvent : UncheckedRoutedEvent, this);
            if (this.isChecked) {
                this.Checked?.Invoke(this, stateArgs);
            }
            else {
                this.Unchecked?.Invoke(this, stateArgs);
            }

            if (this.Parent != null)
                this.Invalidate();

            return clickArgs.Handled;
        }


        /// <summary>Whether the check box is currently checked.</summary>
        public bool IsChecked {
            get => this.isChecked;
            set {
                if (this.isChecked == value) return;
                this.isChecked = value;

                // Programmatic state changes must repaint — PerformClick()
                // already invalidates on its own path, but direct property
                // assignment used to silently leave the visual stale.
                if (this.Parent != null) this.Invalidate();
            }
        }

        private bool disposed;

        /// <summary>Releases the resources used by the check box.</summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the check box's bitmap resources.</summary>
        protected virtual void Dispose(bool disposing) {
            if (this.disposed) return;

            if (disposing) {
                this.bitmapImageCheckboxOn?.graphics?.Dispose();
                this.bitmapImageCheckboxOff?.graphics?.Dispose();
            }

            this.disposed = true;
        }

        ~CheckBox() {
            this.Dispose(false);
        }

    }
}
