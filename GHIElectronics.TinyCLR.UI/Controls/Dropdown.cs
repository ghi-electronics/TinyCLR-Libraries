using System;
using System.Collections;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class Dropdown : ListBox {
        private bool isOpened;
        private int originalHeight;

        private BitmapImage dropdownTextUp;
        private BitmapImage dropdownTextDown;
        private BitmapImage dropdownButtonUp;
        private BitmapImage dropdownButtonDown;

        public ushort Alpha { get; set; } = 0xC8;
        public int RadiusBorder { get; set; } = 5;

        private int Margin { get; set; } = 2;

        private void InitResource() {
            this.dropdownTextUp = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.DropdownText_Up)));
            this.dropdownTextDown = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.DropdownText_Down)));
            this.dropdownButtonUp = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.DropdownButton_Up)));
            this.dropdownButtonDown = BitmapImage.FromGraphics(Graphics.FromImage(Resources.GetBitmap(Resources.BitmapResources.DropdownButton_Down)));
        }

        public Dropdown() : base() {
            this.InitResource();

            this.TouchUp += this.Dropdown_TouchUp;
            this.SelectionChanged += this.Dropdown_SelectionChanged;
        }

        private void Dropdown_SelectionChanged(object sender, SelectionChangedEventArgs args) {
            this.Items.Clear();

            var text = new Text(this.Font, this.options[this.SelectedIndex].ToString());

            text.SetMargin(this.Margin);

            this.Items.Add(text);

            if (this.Parent != null)
                this.Invalidate();
        }

        private void Dropdown_TouchUp(object sender, TouchEventArgs e) {
            if (!this.isOpened) {
                if (this.options != null && this.options.Count > 0) {
                    this.Height = this.options.Count * this.originalHeight;

                    this.Items.Clear();

                    for (var i = 0; i < this.options.Count; i++) {
                        var text = new Text(this.Font, this.options[i].ToString());

                        text.SetMargin(this.Margin);

                        this.Items.Add(text);
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

        private ArrayList options;
        public ArrayList Options {
            get => this.options;

            set {
                this.options = value;

                if (this.options != null) {
                    this.Items.Clear();

                    this.originalHeight = this.Height;

                    for (var i = 0; i < this.options.Count; i++) {
                        var text = new Text(this.Font, this.options[i].ToString());

                        text.SetMargin(this.Margin);

                        this.Items.Add(text);
                    }
                }

            }
        }

        public override void OnRender(DrawingContext dc) {
            var x = 0;
            var y = 0;

            var alpha = this.IsEnabled ? this.Alpha : (ushort)(this.Alpha / 2);

            double ratio = this.dropdownButtonDown.Height / this.originalHeight;

            var imgWidth = (int)(this.dropdownButtonDown.Width * ratio);

            if (this.isOpened) {
                dc.Scale9Image(this.Width - imgWidth, y, imgWidth, this.originalHeight, this.dropdownButtonUp, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, alpha);
                dc.Scale9Image(x, y, this.Width, this.Height, this.dropdownTextDown, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, alpha);
            }
            else {
                dc.Scale9Image(this.Width - imgWidth, y, imgWidth, this.originalHeight, this.dropdownButtonDown, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, alpha);
                dc.Scale9Image(x, y, this.Width, this.Height, this.dropdownTextUp, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, this.RadiusBorder, alpha);
            }
        }

        public void Dispose() {
            this.dropdownTextUp.graphics.Dispose();
            this.dropdownTextDown.graphics.Dispose();
            this.dropdownButtonUp.graphics.Dispose();
            this.dropdownButtonDown.graphics.Dispose();
        }
    }
}
