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
        private KeyboardView active;
        private double scaleX;
        private double scaleY;
        private int offsetX;
        private int offsetY;

        public static new Font Font { get; set; }

        internal OnScreenKeyboard() {
            this.views = new Hashtable();

            this.Width = WindowManager.Instance.ActualWidth;
            this.Height = WindowManager.Instance.ActualHeight;
            this.Background = new SolidColorBrush(Colors.Red);

            var holder = new StackPanel();

            this.input = new TextBox {
                ForOnScreenKeyboard = true,
                Font = OnScreenKeyboard.Font,
                Height = 2 * Font.Height
            };

            holder.Children.Add(this.input);

            this.image = new Controls.Image {
                Source = null,
                Width = WindowManager.Instance.ActualWidth,
                Height = WindowManager.Instance.ActualHeight - this.input.Height,
                Stretch = Stretch.Fill
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
            var x = (int)(e.Touches[0].X * this.scaleX - this.offsetX);
            var y = (int)(e.Touches[0].Y * this.scaleY - this.offsetY);
            var row = y / this.active.RowHeight;
            var column = 0;
            var columnWidth = this.active.ColumnWidth[row];
            var total = this.active.RowColumnOffset[row];

            while (column < columnWidth.Length && total < x)
                total += columnWidth[column++];

            if (--column < 0 || total < x)
                return;

            if (this.active.SpecialKeys?[row]?[column] is Action a) {
                a();
            }
            else {
                this.Append(this.active.Keys[row][column]);
            }
        }

        private void Backspace() { if (this.input.Text.Length > 0) this.input.Text = this.input.Text.Substring(0, this.input.Text.Length - 1); }
        private void Append(char c) => this.input.Text += c;

        private new void Close() {
            this.source.Text = this.input.Text;

            Application.Current.CloseOnScreenKeyboard();
        }

        private void ShowView(KeyboardViewId id) {
            if (!this.views.Contains(id))
                this.CreateView(id);

            this.active = (KeyboardView)this.views[id];

            this.image.Source = this.active.Image;

            this.image.InvalidateMeasure();

            this.scaleX = this.active.Image.Width / (double)this.image.Width;
            this.scaleY = this.active.Image.Height / (double)this.image.Height;
            this.offsetX = 0;
            this.offsetY = this.input.Height;
        }

        private void CreateView(KeyboardViewId id) {
            var hf = 40;
            var sz = 80;
            var szh = 120;
            var full = new[] { sz, sz, sz, sz, sz, sz, sz, sz, sz, sz };
            var image = default(System.Drawing.Bitmap);
            var view = new KeyboardView { RowHeight = sz };

            switch (id) {
                case KeyboardViewId.Lowercase:
                    image = Resources.GetBitmap(Resources.BitmapResources.Keyboard_Lowercase);
                    view.RowColumnOffset = new[] { 0, hf, 0, 0 };
                    view.ColumnWidth = new[] {
                        full,
                        new[] { sz, sz, sz, sz, sz, sz, sz, sz, sz },
                        new[] { szh, sz, sz, sz, sz, sz, sz, sz, szh },
                        new[] { szh, sz, sz * 5, sz, szh }
                    };
                    view.Keys = new[] {
                        new[] { 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p' },
                        new[] { 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l' },
                        new[] { '\0', 'z', 'x', 'c', 'v', 'b', 'n', 'm', '\0' },
                        new[] { '\0', ',', ' ', '.', '\0' }
                    };
                    view.SpecialKeys = new[] {
                        null,
                        null,
                        new Action[] { () => this.ShowView(KeyboardViewId.Uppercase), null, null, null, null, null, null, null, () => this.Backspace() },
                        new Action[] { () => this.ShowView(KeyboardViewId.Numbers), null, null, null, () => this.Close() }
                    };

                    break;

                case KeyboardViewId.Uppercase:
                    image = Resources.GetBitmap(Resources.BitmapResources.Keyboard_Uppercase);
                    view.RowColumnOffset = new[] { 0, hf, 0, 0 };
                    view.ColumnWidth = new[] {
                        full,
                        new[] { sz, sz, sz, sz, sz, sz, sz, sz, sz },
                        new[] { szh, sz, sz, sz, sz, sz, sz, sz, szh },
                        new[] { szh, sz, sz * 5, sz, szh }
                    };
                    view.Keys = new[] {
                        new[] { 'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P' },
                        new[] { 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L' },
                        new[] { '\0', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', '\0' },
                        new[] { '\0', ',', ' ', '.', '\0' }
                    };
                    view.SpecialKeys = new[] {
                        null,
                        null,
                        new Action[] { () => this.ShowView(KeyboardViewId.Lowercase), null, null, null, null, null, null, null, () => this.Backspace() },
                        new Action[] { () => this.ShowView(KeyboardViewId.Numbers), null, null, null, () => this.Close() }
                    };

                    break;

                case KeyboardViewId.Numbers:
                    image = Resources.GetBitmap(Resources.BitmapResources.Keyboard_Numbers);
                    view.RowColumnOffset = new[] { 0, 0, 0, 0 };
                    view.ColumnWidth = new[] {
                        full,
                        full,
                        new[] { szh, sz, sz, sz, sz, sz, sz, sz, szh },
                        new[] { szh, sz, sz * 5, sz, szh }
                    };
                    view.Keys = new[] {
                        new[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' },
                        new[] { '@', '#', '$', '%', '&', '*', '-', '+', '(', ')' },
                        new[] { '\0', '!', '"', '\'', ':', ';', '/', '?', '\0' },
                        new[] { '\0', ',', ' ', '.', '\0' }
                    };
                    view.SpecialKeys = new[] {
                        null,
                        null,
                        new Action[] { () => this.ShowView(KeyboardViewId.Symbols), null, null, null, null, null, null, null, () => this.Backspace() },
                        new Action[] { () => this.ShowView(KeyboardViewId.Lowercase), null, null, null, () => this.Close() }
                    };

                    break;

                case KeyboardViewId.Symbols:
                    image = Resources.GetBitmap(Resources.BitmapResources.Keyboard_Symbols);
                    view.RowColumnOffset = new[] { 0, 0, 0, 0 };
                    view.ColumnWidth = new[] {
                        full,
                        full,
                        new[] { szh, sz, sz, sz, sz, sz, sz, sz, szh },
                        new[] { szh, sz, sz * 5, sz, szh }
                    };
                    view.Keys = new[] {
                        new[] { '~', '`', '|', '•', '√', 'π', '÷', '×', '{', '}' },
                        new[] { '\t', '£', '¢', '€', 'º', '^', '_', '=', '[', ']' },
                        new[] { '\0', '™', '®', '©', '¶', '\\', '<', '>', '\0' },
                        new[] { '\0', ',', ' ', '.', '\0' }
                    };
                    view.SpecialKeys = new[] {
                        null,
                        null,
                        new Action[] { () => this.ShowView(KeyboardViewId.Numbers), null, null, null, null, null, null, null, () => this.Backspace() },
                        new Action[] { () => this.ShowView(KeyboardViewId.Lowercase), null, null, null, () => this.Close() }
                    };

                    break;
            }

            view.Image = BitmapImage.FromGraphics(Graphics.FromImage(image));

            this.views.Add(id, view);
        }

        private enum KeyboardViewId {
            Lowercase,
            Uppercase,
            Numbers,
            Symbols
        }

        private class KeyboardView {
            public BitmapImage Image { get; set; }
            public int RowHeight { get; set; }
            public int[] RowColumnOffset { get; set; }
            public int[][] ColumnWidth { get; set; }
            public char[][] Keys { get; set; }
            public Action[][] SpecialKeys { get; set; }
        }
    }
}
