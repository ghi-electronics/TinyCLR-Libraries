using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;
using System;

namespace GHIElectronics.TinyCLR.BrainPad.Internal {
    /// <summary>
    /// Provides access to the light bulb on the BrainPad.
    /// </summary>
    public class LightBulb {
        private PwmPin red;
        private PwmPin green;
        private PwmPin blue;
        //private bool started;

        public LightBulb() {
            //started = false;

            var PWM = PwmController.FromId(G30.PwmPin.Controller3.Id);
            PWM.SetDesiredFrequency(10000);
            this.red = PWM.OpenPin(G30.PwmPin.Controller3.PC9);
            this.green = PWM.OpenPin(G30.PwmPin.Controller3.PC8);
            this.blue = PWM.OpenPin(G30.PwmPin.Controller3.PC7);
            // red = new PWM(Peripherals.LightBulb.Red, 10000, 1, false);
            // green = new PWM(Peripherals.LightBulb.Green, 10000, 1, false);
            //blue = new PWM(Peripherals.LightBulb.Blue, 10000, 1, false);
            this.red.Start();
            this.green.Start();
            this.blue.Start();

            SetRgbColor(0, 0, 0);
        }

        /// <summary>
        /// Sets the color of the light bulb.
        /// </summary>
        /// <param name="r">The red value of the color between 0 (fully off) and 100 (fully on).</param>
        /// <param name="g">The green value of the color between 0 (fully off) and 100 (fully on).</param>
        /// <param name="blue">The blue value of the color between 0 (fully off) and 100 (fully on).</param>
        public void SetRgbColor(double r, double g, double b) {
            if (r < 0 || r > 100) throw new ArgumentOutOfRangeException("red", "red must be between zero and one hundred.");
            if (g < 0 || g > 100) throw new ArgumentOutOfRangeException("green", "green must be between zero and one hundred.");
            if (b < 0 || b > 100) throw new ArgumentOutOfRangeException("blue", "blue must be between zero and one hundred.");

            this.red.SetActiveDutyCyclePercentage(r / 100);
            this.green.SetActiveDutyCyclePercentage(g / 100);
            this.blue.SetActiveDutyCyclePercentage(b / 100);
        }

        /// <summary>
        /// Turns off the light bulb.
        /// </summary>
        public void TurnOff() => SetRgbColor(0, 0, 0);

        /// <summary>
        /// Turns the light bulb on.
        /// </summary>
        public void TurnWhite() => SetRgbColor(100, 100, 100);

        /// <summary>
        /// Turns the light bulb Red.
        /// </summary>
        public void TurnRed() => SetRgbColor(100, 0, 0);

        /// <summary>
        /// Turns the light bulb Green.
        /// </summary>
        public void TurnGreen() => SetRgbColor(0, 100, 0);

        /// <summary>
        /// Turns the light bulb Blue.
        /// </summary>
        public void TurnBlue() => SetRgbColor(0, 0, 100);
    }
}
