using GHIElectronics.TinyCLR.UI.Input;
using GHIElectronics.TinyCLR.UI.Media;

namespace GHIElectronics.TinyCLR.UI.Controls {
    public class Button : ContentControl {
        public Button() => this.Background = new SolidColorBrush(Colors.Gray);

        public event RoutedEventHandler Click;

        protected override void OnTouchUp(TouchEventArgs e) {
            var evt = new RoutedEvent("ClickEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler));
            var args = new RoutedEventArgs(evt, this);

            this.Click?.Invoke(this, args);

            e.Handled = args.Handled;
        }
    }
}
