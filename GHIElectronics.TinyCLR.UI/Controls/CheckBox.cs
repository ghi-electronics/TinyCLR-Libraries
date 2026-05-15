using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class CheckBox : ContentControl, IDisposable {
        // Cached once per AppDomain so every toggle doesn't allocate fresh RoutedEvent objects.
        private static readonly RoutedEvent ClickRoutedEvent =
            new RoutedEvent("ClickEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
        private static readonly RoutedEvent CheckedRoutedEvent =
            new RoutedEvent("CheckedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
        private static readonly RoutedEvent UncheckedRoutedEvent =
            new RoutedEvent("UncheckedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));

        public event RoutedEventHandler Click;
        public event RoutedEventHandler Checked;
        public event RoutedEventHandler Unchecked;

        private BitmapImage bitmapImageCheckboxOn;
        private BitmapImage bitmapImageCheckboxOff;

        private bool isChecked = false;

        public string Name { get; set; } = string.Empty;
        public ushort Alpha { get; set; } = 0xC8;
        public ushort RadiusBorder { get; set; } = 5;


        private void InitResource() {
            this.bitmapImageCheckboxOn = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.CheckBox_On)));
            this.bitmapImageCheckboxOff = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.CheckBox_Off)));
        }

        public CheckBox() : base() {
            this.InitResource();

            this.Width = this.bitmapImageCheckboxOn.Width;
            this.Height = this.bitmapImageCheckboxOn.Height;
        }

        public override void OnRender(DrawingContext dc) {
            var x = 0;
            var y = 0;

            if (this.isChecked)
                dc.Scale9Image(x, y, this.Width, this.Height, this.bitmapImageCheckboxOn, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.Alpha);
            else
                dc.Scale9Image(x, y, this.Width, this.Height, this.bitmapImageCheckboxOff, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.Alpha);
        }

        protected override void OnTouchUp(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            e.Handled = this.PerformClick();
        }

        protected override void OnTouchDown(TouchEventArgs e) {
            // No-op. Toggling happens on TouchUp so that drag-off-and-release-elsewhere
            // doesn't flip state, and Click fires exactly once per activation.
        }

        protected override void OnButtonUp(ButtonEventArgs e) {
            if (!this.IsEnabled || e.Button != HardwareButton.Select) {
                return;
            }

            e.Handled = this.PerformClick();
        }

        // Toggles checked state and raises Click + Checked/Unchecked.
        // Returns the Click args.Handled flag so callers can forward it.
        private bool PerformClick() {
            var clickArgs = new RoutedEventArgs(ClickRoutedEvent, this);
            try {
                this.Click?.Invoke(this, clickArgs);
            }
            catch { }

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


        public bool IsChecked {
            get => this.isChecked;
            set {
                if (this.isChecked != value) {
                    this.isChecked = value;
                }
            }
        }

        private bool disposed;

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {

                this.bitmapImageCheckboxOn.graphics.Dispose();
                this.bitmapImageCheckboxOff.graphics.Dispose();

                this.disposed = true;
            }
        }

        ~CheckBox() {
            this.Dispose(false);
        }

    }
}
