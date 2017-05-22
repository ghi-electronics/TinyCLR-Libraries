using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.Gpio {
    /// <summary>Allows a high frequency signal to be generated on a given digital pin. See https://www.ghielectronics.com/docs/24/ for more information.</summary>
    /// <remarks>Software generation is used so accuracy may suffer and is platform dependent.</remarks>
    public class GpioChangeWriter : IDisposable {
        private uint pin;
        private bool disposed;

#pragma warning disable 0169
        private uint nativePointer;
#pragma warning restore 0169

        /// <summary>Whether or not the pin is currently being toggled.</summary>
        public bool Active {
            get {
                if (this.disposed) throw new ObjectDisposedException();

                return this.NativeIsActive();
            }
        }

        /// <summary>The pin the signal generator is using.</summary>
        public int Pin {
            get {
                if (this.disposed) throw new ObjectDisposedException();

                return (int)this.pin;
            }
        }

        /// <summary>Constructs a new object.</summary>
        /// <param name="pin">The pin on which signals will be generated.</param>
        /// <param name="initialValue">The initial value of the pin.</param>
        public GpioChangeWriter(int pin, bool initialValue) {
            this.pin = (uint)pin;

            if (!this.NativeConstructor(initialValue)) throw new ArgumentException();
        }

        /// <summary>The finalizer.</summary>
        ~GpioChangeWriter() {
            this.Dispose(false);
        }

        /// <summary>Disposes the pin and marks it as available again.</summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Sets the current pin state.</summary>
        /// <param name="pinState">Pin state.</param>
        public void Set(bool pinState) {
            if (this.disposed) throw new ObjectDisposedException();

            this.NativeSet(pinState);
        }

        /// <summary>Begins to generate a signal on the pin using the given timings buffer.</summary>
        /// <param name="initialValue">The initial value of the pin.</param>
        /// <param name="timingsBuffer">The timings buffer. Each entry determines how long in microseconds the pin is held in the state before transitioning.</param>
        public void Set(bool initialValue, uint[] timingsBuffer) {
            if (timingsBuffer == null) throw new ArgumentNullException("timingsBuffer");

            this.Set(initialValue, timingsBuffer, 0, timingsBuffer.Length, false);
        }

        /// <summary>Begins to generate a signal on the pin using the given timings buffer.</summary>
        /// <param name="initialValue">The initial value of the pin.</param>
        /// <param name="timingsBuffer">The timings buffer. Each entry determines how long in microseconds the pin is held in the state before transitioning.</param>
        /// <param name="repeat">Whether or not to repeat the buffer continually.</param>
        public void Set(bool initialValue, uint[] timingsBuffer, bool repeat) {
            if (timingsBuffer == null) throw new ArgumentNullException("timingsBuffer");

            this.Set(initialValue, timingsBuffer, 0, timingsBuffer.Length, repeat);
        }

        /// <summary>Begins to generate a signal on the pin using the given timings buffer.</summary>
        /// <param name="initialValue">The initial value of the pin.</param>
        /// <param name="timingsBuffer">The timings buffer. Each entry determines how long in microseconds the pin is held in the state before transitioning.</param>
        /// <param name="bufferOffset">The offset into the buffer at which to begin.</param>
        /// <param name="bufferCount">The number of transitions in the buffer to make.</param>
        public void Set(bool initialValue, uint[] timingsBuffer, int bufferOffset, int bufferCount) => this.Set(initialValue, timingsBuffer, bufferOffset, bufferCount, false);

        /// <summary>Begins to generate a signal on the pin using the given timings buffer.</summary>
        /// <param name="initialValue">The initial value of the pin.</param>
        /// <param name="timingsBuffer">The timings buffer. Each entry determines how long in microseconds the pin is held in the state before transitioning.</param>
        /// <param name="bufferOffset">The offset into the buffer at which to begin.</param>
        /// <param name="bufferCount">The number of transitions in the buffer to make.</param>
        /// <param name="repeat">Whether or not to repeat the buffer continually.</param>
        public void Set(bool initialValue, uint[] timingsBuffer, int bufferOffset, int bufferCount, bool repeat) {
            if (this.disposed) throw new ObjectDisposedException();
            if (timingsBuffer == null) throw new ArgumentNullException("timingsBuffer");
            if (bufferOffset < 0) throw new ArgumentOutOfRangeException("offset", "offset may not be negative.");
            if (bufferCount <= 0) throw new ArgumentOutOfRangeException("count", "count must be positive.");
            if (bufferOffset + bufferCount > timingsBuffer.Length) throw new ArgumentOutOfRangeException("timingsBuffer", "bufferOffset + bufferCount must be no more than timingsBuffer.Length.");

            if (!this.NativeSet(initialValue, timingsBuffer, bufferOffset, bufferCount, repeat))
                throw new OutOfMemoryException("There is not enough memory available to create an internal buffer to hold all the given timings.");
        }

        /// <summary>
        /// Generates a signal on the pin using the given timings buffer and blocks until all timings have been processed with interrupts disabled.
        /// </summary>
        /// <param name="initialValue">The initial value of the pin.</param>
        /// <param name="timingsBuffer">The timings buffer. Each entry determines how long in microseconds the pin is held in the state before transitioning.</param>
        public void SetBlocking(bool initialValue, uint[] timingsBuffer) {
            if (this.disposed) throw new ObjectDisposedException();
            if (timingsBuffer == null) throw new ArgumentNullException("timingsBuffer");
            if (timingsBuffer.Length == 0) throw new ArgumentOutOfRangeException("timingsBuffer", "timingsBuffer.Length must be positive.");

            this.NativeSet(initialValue, timingsBuffer, 0, timingsBuffer.Length, 0U, true, 0U);
        }

        /// <summary>
        /// Generates a signal on the pin using the given timings buffer and blocks until all timings have been processed with interrupts disabled.
        /// </summary>
        /// <param name="initialValue">The initial value of the pin.</param>
        /// <param name="timingsBuffer">The timings buffer. Each entry determines how long in microseconds the pin is held in the state before transitioning.</param>
        /// <param name="bufferOffset">The offset into the buffer at which to begin.</param>
        /// <param name="bufferCount">The number of transitions in the buffer to make.</param>
        public void SetBlocking(bool initialValue, uint[] timingsBuffer, int bufferOffset, int bufferCount) {
            if (this.disposed) throw new ObjectDisposedException();
            if (timingsBuffer == null) throw new ArgumentNullException("timingsBuffer");
            if (bufferOffset < 0) throw new ArgumentOutOfRangeException("offset", "offset may not be negative.");
            if (bufferCount <= 0) throw new ArgumentOutOfRangeException("count", "count must be positive.");
            if (bufferOffset + bufferCount > timingsBuffer.Length) throw new ArgumentOutOfRangeException("timingsBuffer", "bufferOffset + bufferCount must be no more than timingsBuffer.Length.");

            this.NativeSet(initialValue, timingsBuffer, bufferOffset, bufferCount, 0U, true, 0U);
        }

        /// <summary>Generates a signal on the pin using the given timings buffer and blocks until all timings have been processed.</summary>
        /// <param name="initialValue">The initial value of the pin.</param>
        /// <param name="timingsBuffer">The timings buffer. Each entry determines how long in microseconds the pin is held in the state before transitioning.</param>
        /// <param name="disableInterrupts">Whether or not to disable interrupts while processing the timings.</param>
        /// <param name="lastBitHoldTime">How long to hold the pin in its last state before returning.</param>
        /// <param name="carrierFrequency">The carrier frequency of the signal in hertz. This is generated using software and may not be fully accurate.</param>
        public void SetBlocking(bool initialValue, uint[] timingsBuffer, bool disableInterrupts, int lastBitHoldTime, int carrierFrequency) {
            if (this.disposed) throw new ObjectDisposedException();
            if (timingsBuffer == null) throw new ArgumentNullException("timingsBuffer");
            if (timingsBuffer.Length == 0) throw new ArgumentOutOfRangeException("timingsBuffer", "timingsBuffer.Length must be positive.");
            if (lastBitHoldTime < 0) throw new ArgumentOutOfRangeException("lastBitHoldTime", "lastBitHoldTime may not be negative.");
            if (carrierFrequency < 0) throw new ArgumentOutOfRangeException("carrierFrequency", "carrierFrequency may not be negative.");

            this.NativeSet(initialValue, timingsBuffer, 0, timingsBuffer.Length, (uint)lastBitHoldTime, disableInterrupts, (uint)carrierFrequency);
        }

        /// <summary>Generates a signal on the pin using the given timings buffer and blocks until all timings have been processed.</summary>
        /// <param name="initialValue">The initial value of the pin.</param>
        /// <param name="timingsBuffer">The timings buffer. Each entry determines how long in microseconds the pin is held in the state before transitioning.</param>
        /// <param name="bufferOffset">The offset into the buffer at which to begin.</param>
        /// <param name="bufferCount">The number of transitions in the buffer to make.</param>
        /// <param name="disableInterrupts">Whether or not to disable interrupts while processing the timings.</param>
        /// <param name="lastBitHoldTime">How long to hold the pin in its last state before returning.</param>
        /// <param name="carrierFrequency">The carrier frequency of the signal in hertz. This is generated using software and may not be fully accurate.</param>
        public void SetBlocking(bool initialValue, uint[] timingsBuffer, int bufferOffset, int bufferCount, bool disableInterrupts, int lastBitHoldTime, int carrierFrequency) {
            if (this.disposed) throw new ObjectDisposedException();
            if (timingsBuffer == null) throw new ArgumentNullException("timingsBuffer");
            if (bufferOffset < 0) throw new ArgumentOutOfRangeException("offset", "offset may not be negative.");
            if (bufferCount <= 0) throw new ArgumentOutOfRangeException("count", "count must be positive.");
            if (bufferOffset + bufferCount > timingsBuffer.Length) throw new ArgumentOutOfRangeException("timingsBuffer", "bufferOffset + bufferCount must be no more than timingsBuffer.Length.");
            if (lastBitHoldTime < 0) throw new ArgumentOutOfRangeException("lastBitHoldTime", "lastBitHoldTime may not be negative.");
            if (carrierFrequency < 0) throw new ArgumentOutOfRangeException("carrierFrequency", "carrierFrequency may not be negative.");

            this.NativeSet(initialValue, timingsBuffer, bufferOffset, bufferCount, (uint)lastBitHoldTime, disableInterrupts, (uint)carrierFrequency);
        }

        /// <summary>Disposes the pin and marks it as available again.</summary>
        /// <param name="disposing">Whether or not this is called from Dispose.</param>
        protected virtual void Dispose(bool disposing) {
            if (this.disposed)
                return;

            this.NativeDispose();

            this.disposed = true;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private bool NativeConstructor(bool initialValue);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void NativeDispose();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private bool NativeIsActive();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void NativeSet(bool state);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private bool NativeSet(bool initialValue, uint[] timingsBuffer, int bufferOffset, int bufferCount, bool repeat);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern private void NativeSet(bool initialValue, uint[] timingsBuffer, int bufferOffset, int bufferCount, uint lastBitHoldTime, bool disableInterrupts, uint carrierFrequency);
    }
}