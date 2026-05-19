using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class Dropdown : ListBox, IDisposable {
        private bool isOpened;
        private int originalHeight;

        private BitmapImage dropdownTextUp;
        private BitmapImage dropdownTextDown;
        private BitmapImage dropdownButtonUp;
        private BitmapImage dropdownButtonDown;

        public ushort Alpha { get; set; } = Theme.DefaultAlpha;
        public int RadiusBorder { get; set; } = Theme.DefaultRadiusBorder;

        /// <summary>
        /// Optional cap on the expanded list height. When set to a positive value
        /// the open dropdown is clamped to this height and overflow scrolls through
        /// the inherited ScrollViewer. Default 0 = no clamp (legacy behavior).
        /// </summary>
        public int MaxOpenHeight { get; set; }

        private int Margin { get; set; } = 2;

        private void InitResource() {
            this.dropdownTextUp = Resources.LoadBitmapImage(Resources.BitmapResources.DropdownText_Up);
            this.dropdownTextDown = Resources.LoadBitmapImage(Resources.BitmapResources.DropdownText_Down);
            this.dropdownButtonUp = Resources.LoadBitmapImage(Resources.BitmapResources.DropdownButton_Up);
            this.dropdownButtonDown = Resources.LoadBitmapImage(Resources.BitmapResources.DropdownButton_Down);
        }

        public Dropdown() : base() {
            this.InitResource();

            this.TouchUp += this.Dropdown_TouchUp;
            this.SelectionChanged += this.Dropdown_SelectionChanged;
        }

        private Media.Color GetForeColor() =>
            (this.Foreground is SolidColorBrush scb) ? scb.Color : Colors.Black;

        private Text CreateOptionText(string content) {
            var text = new Text(this.Font, content) {
                ForeColor = this.GetForeColor()
            };
            text.SetMargin(this.Margin);
            return text;
        }

        private void Dropdown_SelectionChanged(object sender, SelectionChangedEventArgs args) {
            if (this.options == null || this.SelectedIndex < 0 || this.SelectedIndex >= this.options.Count)
                return;

            this.Items.Clear();
            this.Items.Add(this.CreateOptionText(this.options[this.SelectedIndex].ToString()));

            if (this.Parent != null)
                this.Invalidate();
        }

        private void Dropdown_TouchUp(object sender, TouchEventArgs e) => this.ToggleOpen();

        /// <summary>
        /// Hardware button support: <see cref="HardwareButton.Select"/> toggles the
        /// dropdown open/closed (parity with touch). <see cref="HardwareButton.Back"/>
        /// closes an open dropdown. Up/Down navigation comes free from <see cref="ListBox"/>.
        /// </summary>
        protected override void OnButtonDown(ButtonEventArgs e) {
            if (e.Button == HardwareButton.Select) {
                this.ToggleOpen();
                e.Handled = true;
                return;
            }

            if (e.Button == HardwareButton.Back && this.isOpened) {
                this.ToggleOpen();
                e.Handled = true;
                return;
            }

            // Delegate Up/Down to the ListBox base only while open; collapsed dropdowns
            // shouldn't eat navigation keys.
            if (this.isOpened) {
                base.OnButtonDown(e);
            }
        }

        private void ToggleOpen() {
            this.EnsureOriginalHeight();

            if (!this.isOpened) {
                if (this.options != null && this.options.Count > 0) {
                    var full = this.options.Count * this.originalHeight;
                    this.Height = (this.MaxOpenHeight > 0 && full > this.MaxOpenHeight)
                        ? this.MaxOpenHeight
                        : full;

                    this.Items.Clear();
                    for (var i = 0; i < this.options.Count; i++) {
                        this.Items.Add(this.CreateOptionText(this.options[i].ToString()));
                    }
                }
            }
            else {
                if (this.options != null && this.options.Count > 0) {
                    this.Height = this.originalHeight;
                }
            }

            this.isOpened = !this.isOpened;

            if (this.Parent != null)
                this.Invalidate();
        }

        // Capture the collapsed row height lazily so callers aren't forced to
        // set Height before Options. Only refresh while closed — once opened,
        // Height represents the expanded list.
        private void EnsureOriginalHeight() {
            if (this.isOpened) return;

            try {
                var h = this.Height;
                if (h > 0) this.originalHeight = h;
            }
            catch (InvalidOperationException) {
                // Height was never set; leave originalHeight at 0 and let OnRender
                // guard the paint pass.
            }
        }

        private ArrayList options;
        public ArrayList Options {
            get => this.options;

            set {
                this.options = value;

                if (this.options != null) {
                    this.EnsureOriginalHeight();

                    this.Items.Clear();
                    for (var i = 0; i < this.options.Count; i++) {
                        this.Items.Add(this.CreateOptionText(this.options[i].ToString()));
                    }
                }
            }
        }

        public override void OnRender(DrawingContext dc) {
            // Honor Background/focus visual from Control base.
            base.OnRender(dc);

            this.EnsureOriginalHeight();

            // Layout may not have run yet (Width/Height never set, or measure pass
            // pending). Bailing here keeps the paint pass alive for sibling
            // controls on the same canvas instead of throwing "width not set"
            // out of the render loop.
            var w = this.ActualWidth;
            var h = this.ActualHeight;
            if (w <= 0 || h <= 0 || this.originalHeight <= 0) return;
            if (this.dropdownButtonDown == null || this.dropdownButtonDown.Height <= 0) return;

            var alpha = this.IsEnabled ? this.Alpha : (ushort)(this.Alpha / 2);

            // Scale the chevron button image proportionally to the collapsed row
            // height. The previous formula divided image.Height by originalHeight
            // (both ints) which inverted the ratio AND truncated to zero whenever
            // the control was taller than the source bitmap, making the chevron
            // vanish.
            var ratio = (double)this.originalHeight / this.dropdownButtonDown.Height;
            var imgWidth = (int)(this.dropdownButtonDown.Width * ratio);
            if (imgWidth < 1) imgWidth = 1;
            if (imgWidth > w) imgWidth = w;

            var buttonImg = this.isOpened ? this.dropdownButtonUp : this.dropdownButtonDown;
            var textImg = this.isOpened ? this.dropdownTextDown : this.dropdownTextUp;

            dc.Scale9Image(w - imgWidth, 0, imgWidth, this.originalHeight, buttonImg, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, alpha);
            dc.Scale9Image(0, 0, w, h, textImg, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, alpha);
        }

        private bool disposed;

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (this.disposed) return;

            if (disposing) {
                this.TouchUp -= this.Dropdown_TouchUp;
                this.SelectionChanged -= this.Dropdown_SelectionChanged;

                this.dropdownTextUp?.graphics?.Dispose();
                this.dropdownTextDown?.graphics?.Dispose();
                this.dropdownButtonUp?.graphics?.Dispose();
                this.dropdownButtonDown?.graphics?.Dispose();
            }

            this.disposed = true;
        }

        ~Dropdown() {
            this.Dispose(false);
        }
    }
}
