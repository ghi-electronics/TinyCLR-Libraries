using System;
using System.Collections;
using System.Drawing;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;
using GHIElectronics.TinyCLR.UI.Media.Imaging;

namespace GHIElectronics.TinyCLR.UI {
    public class OnScreenKeyboard : Window {
        private Hashtable views;
        private TextBox source;
        private TextBox input;
        private Controls.Image image;

        public static Font Font { get; set; }

        internal OnScreenKeyboard() {
            this.views = new Hashtable();

            this.Width = WindowManager.Instance.ActualWidth;
            this.Height = WindowManager.Instance.ActualHeight;
            this.Background = new SolidColorBrush(Colors.Red);

            var holder = new StackPanel();

            this.input = new TextBox {
                ForOnScreenKeyboard = true,
                Font = OnScreenKeyboard.Font
            };

            holder.Children.Add(this.input);

            this.image = new Controls.Image {
                Source = null
            };

            this.image.TouchUp += this.OnTouchUp;

            holder.Children.Add(this.image);

            this.Child = holder;
        }

        internal void ShowFor(TextBox textBox) {
            this.Width = WindowManager.Instance.ActualWidth;
            this.Height = WindowManager.Instance.ActualHeight;

            this.source = textBox;
            this.input.Text = this.source.Text;

            this.ShowView(KeyboardViewId.Lowercase);
        }

        private void OnTouchUp(object sender, TouchEventArgs e) {
            if (e.Touches[0].Y < 100)
                this.Close();
            else
                this.Append('a');
        }

        private void Append(char c) => this.input.Text += c;

        private void Close() {
            this.source.Text = this.input.Text;

            Application.Current.CloseOnScreenKeyboard();
        }

        private void ShowView(KeyboardViewId id) {
            if (!this.views.Contains(id))
                this.CreateView(id);

            var view = (KeyboardView)this.views[id];

            this.image.Source = view.Image;

            this.image.InvalidateMeasure();
        }

        private void CreateView(KeyboardViewId id) {
            System.Drawing.Bitmap bmp = default;

            switch (id) {
                case KeyboardViewId.Lowercase: bmp = Resources.GetBitmap(Resources.BitmapResources.Keyboard_Lowercase); break;
                case KeyboardViewId.Uppercase: bmp = Resources.GetBitmap(Resources.BitmapResources.Keyboard_Uppercase); break;
                case KeyboardViewId.Numbers: bmp = Resources.GetBitmap(Resources.BitmapResources.Keyboard_Numbers); break;
                case KeyboardViewId.Symbols: throw new NotImplementedException(); // bmp = Resources.GetBitmap(Resources.BitmapResources.Keyboard_Symbols); break;
            }

            this.views.Add(id, new KeyboardView {
                Image = BitmapImage.FromGraphics(Graphics.FromImage(bmp))
            });
        }

        private enum KeyboardViewId {
            Lowercase,
            Uppercase,
            Numbers,
            Symbols
        }

        private class KeyboardView {
            public BitmapImage Image { get; set; }
        }
    }
}
