using GHIElectronics.TinyCLR.UI.Controls;
using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI {
    public class OnScreenKeyboard : Window {
        internal OnScreenKeyboard() {
            this.Width = 200;
            this.Height = 200;
            this.Background = new SolidColorBrush(Colors.Red);
        }

        internal void ShowFor(TextBox textBox) {

        }

        protected override void OnTouchUp(TouchEventArgs e) => Application.Current.CloseOnScreenKeyboard();
    }
}
