using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GHIElectronics.TinyCLR.Devices.Gpio {
    /// <summary>Captures a pin's digital waveform into a buffer. See https://www.ghielectronics.com/docs/106/ for more information.</summary>
    /// <remarks>
    /// All managed threads are blocked while capturing. When the pin's state changes, the time from the last change until the new change is recorded.
    /// </remarks>
    public class GpioChangeReader : IDisposable {
        private int timeout;
        private bool disposed;
        private GpioPin port;
        private int pin;

        /// <summary>How long to let the read operations wait until returning with incomplete data.</summary>
        public int ReadTimeout {
            get => this.timeout;

            set {
                if (value < 0 && value != Timeout.Infinite) throw new ArgumentOutOfRangeException("value", "value must be non-negative or Timeout.Infinite.");

                this.timeout = value;
            }
        }

        /// <summary>Constructs a new object using an InputPort on the given pin.</summary>
        /// <param name="pin">The pin on which to create the port.</param>
        /// <param name="driveMode">The resistor mode for the pin.</param>
        public GpioChangeReader(int pin, GpioPinDriveMode driveMode) {
            this.timeout = Timeout.Infinite;
            this.disposed = false;
            this.pin = pin;

            switch (driveMode) {
                case GpioPinDriveMode.Input:
                case GpioPinDriveMode.InputPullUp:
                case GpioPinDriveMode.InputPullDown: break;
                default: throw new ArgumentException("Invalid drive mode.");
            }

            this.port = GpioController.GetNativeDefault().OpenPin(pin);
            this.port.SetDriveMode(driveMode);
        }

        /// <summary>The finalizer.</summary>
        ~GpioChangeReader() {
            this.Dispose(false);
        }

        /// <summary>Disposes the object.</summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Reads the pin's waveform and its initial state.</summary>
        /// <param name="initialState">The initial state of the pin.</param>
        /// <param name="buffer">The buffer to into which to read the pin transition times in microseconds.</param>
        /// <returns>The number of pin transitions.</returns>
        public int Read(out bool initialState, uint[] buffer) {
            if (buffer == null) throw new ArgumentNullException("buffer");

            return this.Read(out initialState, buffer, 0, buffer.Length);
        }

        /// <summary>Reads the pin's waveform after waiting for an initial state.</summary>
        /// <param name="waitForState">The pin state to wait for before starting the capture.</param>
        /// <param name="buffer">The buffer to into which to read the pin transition times in microseconds.</param>
        /// <returns>The number of pin transitions.</returns>
        public int Read(bool waitForState, uint[] buffer) {
            if (buffer == null) throw new ArgumentNullException("buffer");

            return this.Read(waitForState, buffer, 0, buffer.Length);
        }

        /// <summary>Reads the pin's waveform and its initial state.</summary>
        /// <param name="initialState">The initial state of the pin.</param>
        /// <param name="buffer">The buffer to into which to read the pin transition times in microseconds.</param>
        /// <param name="offset">To offset into the buffer at which to begin reading.</param>
        /// <param name="count">The number of transitions to read.</param>
        /// <returns>The number of pin transitions.</returns>
        public int Read(out bool initialState, uint[] buffer, int offset, int count) {
            if (this.disposed) throw new ObjectDisposedException();
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", "offset must be non-negative.");
            if (count < 0) throw new ArgumentOutOfRangeException("count", "count must be non-negative.");
            if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException("buffer", "offset + count must be no more than buffer.Length.");

            return GpioChangeReader.NativeRead((uint)this.pin, out initialState, buffer, offset, count, this.timeout);
        }

        /// <summary>Reads the pin's waveform after waiting for an initial state.</summary>
        /// <param name="waitForState">The pin state to wait for before starting the capture.</param>
        /// <param name="buffer">The buffer to into which to read the pin transition times in microseconds.</param>
        /// <param name="offset">To offset into the buffer at which to begin reading.</param>
        /// <param name="count">The number of transitions to read.</param>
        /// <returns>The number of pin transitions.</returns>
        public int Read(bool waitForState, uint[] buffer, int offset, int count) {
            if (this.disposed) throw new ObjectDisposedException();
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", "offset must be non-negative.");
            if (count < 0) throw new ArgumentOutOfRangeException("count", "count must be non-negative.");
            if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException("buffer", "offset + count must be no more than buffer.Length.");

            return GpioChangeReader.NativeRead((uint)this.pin, waitForState, buffer, offset, count, this.timeout);
        }

        /// <summary>Disposes the object.</summary>
        /// <param name="disposing">Whether or not this is called from Dispose.</param>
        protected virtual void Dispose(bool disposing) {
            if (this.disposed)
                return;

            if (disposing) {
                if (this.port != null) {
                    this.port.Dispose();
                    this.port = null;
                }
            }

            this.disposed = true;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static int NativeRead(uint pin, out bool initialState, uint[] buffer, int offset, int count, int timeout);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private static int NativeRead(uint pin, bool waitforInitialState, uint[] buffer, int offset, int count, int timeout);
    }
}