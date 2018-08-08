using System;
using System.ComponentModel;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class LightBulb {
        private PwmChannel red;
        private PwmChannel green;
        private PwmChannel blue;
        //private bool started;

        public LightBulb() {
            //started = false;

            var PWM = PwmController.FromName(Board.BoardType == BoardType.BP2 ? FEZCLR.PwmChannel.Controller3.Id : G30.PwmChannel.Controller3.Id);
            PWM.SetDesiredFrequency(10000);
            this.red = PWM.OpenChannel(Board.BoardType == BoardType.BP2 ? FEZCLR.PwmChannel.Controller3.PC9 : G30.PwmChannel.Controller3.PC9);
            this.green = PWM.OpenChannel(Board.BoardType == BoardType.BP2 ? FEZCLR.PwmChannel.Controller3.PC8 : G30.PwmChannel.Controller3.PC8);
            this.blue = PWM.OpenChannel(Board.BoardType == BoardType.BP2 ? FEZCLR.PwmChannel.Controller3.PC6 : G30.PwmChannel.Controller3.PC7);
            // red = new PWM(Peripherals.LightBulb.Red, 10000, 1, false);
            // green = new PWM(Peripherals.LightBulb.Green, 10000, 1, false);
            //blue = new PWM(Peripherals.LightBulb.Blue, 10000, 1, false);
            this.red.Start();
            this.green.Start();
            this.blue.Start();

            TurnColor(0, 0, 0);
        }

        /// <summary>
        /// Sets the color of the light bulb.
        /// </summary>
        /// <param name="r">The red value of the color between 0 (fully off) and 100 (fully on).</param>
        /// <param name="g">The green value of the color between 0 (fully off) and 100 (fully on).</param>
        /// <param name="blue">The blue value of the color between 0 (fully off) and 100 (fully on).</param>
        public void TurnColor(double r, double g, double b) {
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
        public void TurnOff() => TurnColor(0, 0, 0);

        /// <summary>
        /// Turns the light bulb on.
        /// </summary>
        public void TurnWhite() => TurnColor(40, 30, 80);

        /// <summary>
        /// Turns the light bulb Red.
        /// </summary>
        public void TurnRed() => TurnColor(40, 0, 0);

        /// <summary>
        /// Turns the light bulb Green.
        /// </summary>
        public void TurnGreen() => TurnColor(0, 30, 0);

        /// <summary>
        /// Turns the light bulb Blue.
        /// </summary>
        public void TurnBlue() => TurnColor(0, 0, 80);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType() => base.GetType();
    }
}
