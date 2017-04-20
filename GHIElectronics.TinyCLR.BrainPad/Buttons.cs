using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class Buttons {
        /// <summary>
        /// The signature of all button events.
        /// </summary>
        /// <param name="button">The button in question.</param>
        public delegate void ButtonEventHandler();

        private enum Button {
            Left = 0,
            Down = 1,
            Right = 2,
            Up = 3
        }

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

        public event ButtonEventHandler WhenLeftButtonReleased;
        public event ButtonEventHandler WhenLeftButtonPressed;
        public event ButtonEventHandler WhenRightButtonReleased;
        public event ButtonEventHandler WhenRightButtonPressed;
        public event ButtonEventHandler WhenUpButtonReleased;
        public event ButtonEventHandler WhenUpButtonPressed;
        public event ButtonEventHandler WhenDownButtonReleased;
        public event ButtonEventHandler WhenDownButtonPressed;

        private void Button_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) {

            for (var i = 0; i < 3; i++) {
                if (sender.PinNumber == this.buttons[i].PinNumber) {
                    var pressed = e.Edge == GpioPinEdge.FallingEdge;

                    switch ((Button)i) {
                        case Button.Left: (pressed ? this.WhenLeftButtonPressed : this.WhenLeftButtonReleased)?.Invoke(); break;
                        case Button.Right: (pressed ? this.WhenRightButtonPressed : this.WhenRightButtonReleased)?.Invoke(); break;
                        case Button.Up: (pressed ? this.WhenUpButtonPressed : this.WhenUpButtonReleased)?.Invoke(); break;
                        case Button.Down: (pressed ? this.WhenDownButtonPressed : this.WhenDownButtonReleased)?.Invoke(); break;
                    }
                }
            }
        }

        /// <summary>
        /// Is the down button pressed.
        /// </summary>
        /// <returns>Whether or not it is pressed.</returns>
        public bool IsDownPressed() => IsPressed(Button.Down);

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

        private bool IsPressed(Button button) => this.buttons[(int)button].Read() == GpioPinValue.Low;// it is low when it is pressed
    }
}
