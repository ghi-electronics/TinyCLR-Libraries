using System;
using System.Threading;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class Wait {
        /// <summary>
        /// Tells the BrainPad to wait for the given number of seconds.
        /// </summary>
        /// <param name="seconds">The number of seconds to wait.</param>
        public void Seconds(double seconds) {
            if (seconds < 0.0) throw new ArgumentOutOfRangeException("seconds", "seconds must not be negative.");

            Thread.Sleep((int)(seconds * 1000));
        }

        /// <summary>
        /// Tells the BrainPad to wait for the given number of milliseconds.
        /// </summary>
        /// <param name="milliseconds">The number of milliseconds to wait.</param>
        public void Milliseconds(double milliseconds) {
            if (milliseconds < 0) throw new ArgumentOutOfRangeException("milliseconds", "milliseconds must not be negative.");

            Thread.Sleep((int)milliseconds);
        }
        public void Minimum() => Seconds(0.02);
    }
}
