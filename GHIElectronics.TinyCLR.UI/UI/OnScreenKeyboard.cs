using System.Drawing;
using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI {
    public class OnScreenKeyboard : Window {
        private TextBox source;
        private TextBox input;

        public static Font Font { get; set; }

        internal OnScreenKeyboard() {
            this.Width = WindowManager.Instance.ActualWidth;
            this.Height = WindowManager.Instance.ActualHeight;
            this.Background = new SolidColorBrush(Colors.Red);

            var holder = new StackPanel();

            this.input = new TextBox {
                ForOnScreenKeyboard = true,
                Font = OnScreenKeyboard.Font
            };

            holder.Children.Add(this.input);

            this.Child = holder;
        }

        internal void ShowFor(TextBox textBox) {
            this.Width = WindowManager.Instance.ActualWidth;
            this.Height = WindowManager.Instance.ActualHeight;

            this.source = textBox;
            this.input.Text = this.source.Text;
        }

        protected override void OnTouchUp(TouchEventArgs e) {
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
    }
}
