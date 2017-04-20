using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.BrainPad {
    /// <summary>
    /// The avilable buttons.
    /// </summary>
    public enum Button {
        Left = 0,
        DownSelect = 1,
        Right = 2,
        Up = 3
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
            var PC1 = GpioController.GetDefault().OpenPin(G30.GpioPin.PC1);
            PC1.SetDriveMode(GpioPinDriveMode.InputPullDown);
            if (PC1.Read() == GpioPinValue.High) {
                // new brainpad
                this.buttons = new GpioPin[]
                  {
                        // new BrainPad
                        GPIO.OpenPin(G30.GpioPin.PA15),
                        GPIO.OpenPin(G30.GpioPin.PB10),
                        GPIO.OpenPin(G30.GpioPin.PC13),
                        GPIO.OpenPin(G30.GpioPin.PA5)

                  };
            }
            else {
                // old brainpad
                this.buttons = new GpioPin[]
                  {


                        // old
                        GPIO.OpenPin(G30.GpioPin.PB10),
                        GPIO.OpenPin(G30.GpioPin.PC13),
                        GPIO.OpenPin(G30.GpioPin.PA5),
                        GPIO.OpenPin(G30.GpioPin.PA15)
                  };
            }
            PC1.Dispose();
            PC1 = null;


            foreach (var button in this.buttons) {
                button.SetDriveMode(GpioPinDriveMode.InputPullUp);
                button.ValueChanged += this.Button_ValueChanged;
            }
        }

        /// <summary>
        /// The event raised when a button is released.
        /// </summary>
        public event ButtonEventHandler ButtonReleased;
        public event ButtonEventHandler ButtonPressed;
        private void Button_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) {

            for (var i = 0; i < 3; i++) {
                if (sender.PinNumber == this.buttons[i].PinNumber) {
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
        public bool IsDownSelectPressed() => IsPressed(Button.DownSelect);

        /// <summary>
        /// Is the select button pressed.
        /// </summary>
        /// <returns>Whether or not it is pressed.</returns>
        public bool IsUpPressed() => IsPressed(Button.Up);

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
