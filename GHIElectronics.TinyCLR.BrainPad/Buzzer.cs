using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.ComponentModel;
using System.Threading;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class Buzzer {
        private PwmController controller;
        private PwmPin buzz;

        public Buzzer() {
            this.controller = PwmController.FromId(Board.BoardType == BoardType.BP2 ? FEZChip.PwmPin.Controller4.Id : G30.PwmPin.Controller4.Id);
            this.buzz = this.controller.OpenPin(Board.BoardType == BoardType.BP2 ? FEZChip.PwmPin.Controller4.PB8 : G30.PwmPin.Controller4.PB8);
        }

        /// <summary>
        /// Starts a given frequency.
        /// </summary>
        /// <param name="frequency">The frequency to play.</param>
        public void StartBuzzing(double frequency) {
            StopBuzzing();
            if (frequency > 0) {
                this.controller.SetDesiredFrequency(frequency);
                this.buzz.Start();
                this.buzz.SetActiveDutyCyclePercentage(0.5);
            }
        }

        /// <summary>
        /// Makes a short beep sound.
        /// </summary>
        public void Beep() {
            StartBuzzing(2000);
            Thread.Sleep(5);
            StopBuzzing();
        }

        /// <summary>
        /// Stops any note or frequency currently playing.
        /// </summary>
        public void StopBuzzing() => this.buzz.Stop();

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
