using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Gpio.Provider;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.BrainPad {
    /// <summary>
    /// The avilable buttons.
    /// </summary>
    public enum Button {
        Left = 0,
        Select = 1,
        Right = 2
    }

    /// <summary>
    /// The signature of all button events.
    /// </summary>
    /// <param name="button">The button in question.</param>
    public delegate void ButtonEventHandler(Button button);
}

namespace GHIElectronics.TinyCLR.BrainPad.Internal {
    public class Buttons {
        //private InterruptPort[] ports;
        private GpioPin[] buttons;

        public Buttons() {
            var GPIO = GpioController.GetDefault();
            this.buttons = new GpioPin[]
            {
#if true
                // new BrainPad
                GPIO.OpenPin(G30.GpioPin.PA15),
                GPIO.OpenPin(G30.GpioPin.PB10),
                GPIO.OpenPin(G30.GpioPin.PC13)
#else
                // old
                GPIO.OpenPin(G30.GpioPin.PB10),
                GPIO.OpenPin(G30.GpioPin.PC13),
                GPIO.OpenPin(G30.GpioPin.PA5)
#endif
            };
            foreach (var button in this.buttons) {
                button.SetDriveMode(GpioPinDriveMode.InputPullUp);
                //button.ValueChanged += Button_ValueChanged;
            }
        }
        /// <summary>
        /// Returns a friendly name (string) for a specific button.
        /// </summary>
        /// <param name="button">The button needed.</param>
        /// <returns>The name of that button.</returns>
        public string GetFriendlyName(Button button) {
            string str;
            switch (button) {
                case Button.Left:
                    str = "Left";
                    break;
                case Button.Right:
                    str = "Right";
                    break;
                case Button.Select:
                    str = "Select";
                    break;
                default:
                    str = "Unknown!";
                    break;
            }
            return str;
        }

        /// <summary>
        /// The event raised when a button is released.
        /// </summary>
        public event ButtonEventHandler ButtonReleased;
        public event ButtonEventHandler ButtonPressed;
        private void Button_ValueChanged(object sender, GpioPinValueChangedEventArgs e) {

            for (var i = 0; i < 3; i++) {
                if (((IGpioPinProvider)sender).PinNumber == this.buttons[i].PinNumber) {
                    if (e.Edge == GpioPinEdge.FallingEdge)
                        ButtonPressed?.Invoke((Button)i);
                    else
                        ButtonReleased?.Invoke((Button)i);
                }
            }
        }

        /// <summary>
        /// Is the select button pressed.
        /// </summary>
        /// <returns>Whether or not it is pressed.</returns>
        public bool IsSelectPressed() => IsPressed(Button.Select);

        /// <summary>
        /// Is the left button pressed.
        /// </summary>
        /// <returns>Whether or not it is pressed.</returns>
        public bool IsLeftPressed() => IsPressed(Button.Left);

        /// <summary>
        /// Is the right button pressed.
        /// </summary>
        /// <returns>Whether or not it is pressed.</returns>
        public bool IsRightPressed() => IsPressed(Button.Right);

        /// <summary>
        /// Checks if a button is pressed.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>Whether or not it was pressed.</returns>
        public bool IsPressed(Button button) => this.buttons[(int)button].Read() == GpioPinValue.Low;// it is low when it is pressed
    }
}
