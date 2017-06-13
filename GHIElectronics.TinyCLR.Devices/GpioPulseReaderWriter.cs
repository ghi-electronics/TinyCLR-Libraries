using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.Gpio {
    /// <summary>Reads a pulse from an external device. See https://www.ghielectronics.com/docs/326/ for more information.</summary>
    /// <remarks>All managed threads are blocked while reading.</remarks>
    public class GpioPulseReaderWriter : IDisposable {
        private bool disposed;
        private int timeout;
        private int pulseLength;
        private bool pulseState;
        private bool echoState;
        private int pulsePin;
        private int echoPin;
        private Mode mode;
        private int driveMode;

        /// <summary>
        /// The read modes.
        /// </summary>
        public enum Mode {
            /// <summary>
            /// Read how long it takes for a pin to drain to its opposite state after a pulse.
            /// </summary>
            DrainDuration,
            /// <summary>
            /// Read how long an echo pulse is.
            /// </summary>
            EchoDuration,
            /// <summary>
            /// Read how long it takes to receive an echo pulse.
            /// </summary>
            DurationUntilEcho
        }

        /// <summary>How long to let the measure operation wait in milliseconds until returning an error.</summary>
        public int ReadTimeout {
            get => this.timeout == Timeout.Infinite ? this.timeout : this.timeout / 1000;
            set {
                if (value < 0 && value != Timeout.Infinite) throw new ArgumentOutOfRangeException("value", "value must be non-negative or Timeout.Infinite.");

                this.timeout = value == Timeout.Infinite ? value : value * 1000;
            }
        }

        /// <summary>The resistor mode to use when switching to input and reading a pulse on a pin.</summary>
        public GpioPinDriveMode DriveMode {
            get => (GpioPinDriveMode)this.driveMode;
            set {
                if (value != GpioPinDriveMode.Input && value != GpioPinDriveMode.InputPullDown && value != GpioPinDriveMode.InputPullUp) throw new ArgumentException("value");

                this.driveMode = (int)value;
            }
        }

        /// <summary>The length in microseconds of the initial pulse.</summary>
        public int PulseLength {
            get => this.pulseLength;
            set {
                if (value <= 0) throw new ArgumentOutOfRangeException("value", "value must be positive.");

                this.pulseLength = value;
            }
        }

        /// <summary>Constructs a new object that pulses and echos on the same pin with the same state.</summary>
        /// <param name="mode">The mode to use when reading.</param>
        /// <param name="pulseState">The state of the initial pulse.</param>
        /// <param name="pulseLength">The length in microseconds of the initial pulse.</param>
        /// <param name="pulsePin">The pin on which to send the pulse and read.</param>
        public GpioPulseReaderWriter(Mode mode, bool pulseState, int pulseLength, int pulsePin)
            : this(mode, pulseState, pulseLength, pulsePin, pulseState, pulsePin) {

        }

        /// <summary>Constructs a new object to pulse and echo on the given pins.</summary>
        /// <param name="mode">The mode to use when reading.</param>
        /// <param name="pulseState">The state of the initial pulse.</param>
        /// <param name="pulseLength">The length in microseconds of the initial pulse.</param>
        /// <param name="pulsePin">The pin on which to send the pulse.</param>
        /// <param name="echoState">The state of the echo for which to wait.</param>
        /// <param name="echoPin">The pin on which to measure the echo.</param>
        public GpioPulseReaderWriter(Mode mode, bool pulseState, int pulseLength, int pulsePin, bool echoState, int echoPin) {
            if (pulseLength <= 0) throw new ArgumentOutOfRangeException("pulseLength", "pulseLength must be positive.");
            if (mode != Mode.DrainDuration && mode != Mode.DurationUntilEcho && mode != Mode.EchoDuration) throw new ArgumentException("You must specify a valid mode.", "mode");
            if (pulsePin != echoPin && mode == Mode.DrainDuration) throw new ArgumentException("DrainDuration mode is not valid with different pulse and echo pins.", "mode");

            this.pulseState = pulseState;
            this.pulseLength = pulseLength;
            this.pulsePin = pulsePin;
            this.echoState = echoState;
            this.echoPin = echoPin;
            this.disposed = false;
            this.mode = mode;

            this.ReadTimeout = 100;
            this.DriveMode = GpioPinDriveMode.Input;
        }

        /// <summary>Performs a read operation based on the mode.</summary>
        /// <returns>The length of the time in microseconds, -1 if it timed out.</returns>
        public long Read() {
            if (this.disposed) throw new ObjectDisposedException();

            switch (this.mode) {
                case Mode.DurationUntilEcho: return this.NativeReadEcho(true);
                case Mode.EchoDuration: return this.NativeReadEcho(false);
                case Mode.DrainDuration: return this.NativeReadDrainTime();
                default: return 0;
            }
        }

        /// <summary>The finalizer.</summary>
        ~GpioPulseReaderWriter() {
            this.Dispose(false);
        }

        /// <summary>Disposes the object.</summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Disposes the object.</summary>
        /// <param name="disposing">Whether or not this is called from Dispose.</param>
        protected virtual void Dispose(bool disposing) {
            if (this.disposed)
                return;

            this.NativeFinalize();

            this.disposed = true;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private long NativeReadDrainTime();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private long NativeReadEcho(bool readUntil);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void NativeFinalize();
    }
}