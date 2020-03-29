using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class CheckBox : ContentControl {
        public event RoutedEventHandler Click;

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

            var evt = new RoutedEvent("TouchUpEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
            var args = new RoutedEventArgs(evt, this);

            this.Click?.Invoke(this, args);

            e.Handled = args.Handled;

            this.Checked = !this.Checked;

            if (this.Parent != null)
                this.Invalidate();
        }

        protected override void OnTouchDown(TouchEventArgs e) {
            if (!this.IsEnabled) {
                return;
            }

            var evt = new RoutedEvent("TouchDownEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
            var args = new RoutedEventArgs(evt, this);

            this.Click?.Invoke(this, args);

            e.Handled = args.Handled;

            if (this.Parent != null)
                this.Invalidate();
        }

       
        public bool Checked {
            get => this.isChecked;
            set {
                if (this.isChecked != value) {
                    this.isChecked = value;
                }
            }
        }

        public void Dispose() {
            this.bitmapImageCheckboxOn.graphics.Dispose();
            this.bitmapImageCheckboxOff.graphics.Dispose();
        }

    }
}
