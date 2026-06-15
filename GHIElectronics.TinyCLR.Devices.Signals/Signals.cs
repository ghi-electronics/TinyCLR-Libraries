using System;
using System.Runtime.CompilerServices;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Signals {
    /// <summary>How a <see cref="PulseFeedback"/> session is driven.</summary>
    public enum PulseFeedbackMode {
        /// <summary>Measure the time the pin takes to be pulled back to its idle state on a single shared pin.</summary>
        DrainDuration,
        /// <summary>Drive a pulse on one pin, then measure the duration of the echo pulse on a separate pin (e.g. ultrasonic ECHO).</summary>
        EchoDuration,
        /// <summary>Drive a pulse on one pin, then measure how long it takes the echo pin to respond (e.g. distance-sensor TOF).</summary>
        DurationUntilEcho
    }

    /// <summary>
    /// Generates a digital pulse on one pin and measures a pulse-related duration on
    /// the same pin or a separate echo pin. Common applications: ultrasonic distance
    /// sensors (HC-SR04), 1-wire interrogation, capacitive touch.
    /// </summary>
    public class PulseFeedback : IDisposable {
        private readonly PulseFeedbackMode mode;

        private readonly GpioPin pulsePin;
        private readonly GpioPin echoPin;

        private readonly int pulsePinNumber;
        private readonly int echoPinNumber;

        /// <summary>When true, the trigger runs with interrupts disabled for jitter-free timing.</summary>
        public bool DisableInterrupts { get; set; }
        /// <summary>Maximum time to wait for the echo before giving up.</summary>
        public TimeSpan Timeout { get; set; }
        /// <summary>How long to drive the stimulus pulse before measuring.</summary>
        public TimeSpan PulseLength { get; set; }
        /// <summary>Active level of the stimulus pulse on <c>pulsePin</c>.</summary>
        public GpioPinValue PulseValue { get; set; }
        /// <summary>Active level of the echo on <c>echoPin</c>.</summary>
        public GpioPinValue EchoValue { get; set; }

        /// <summary>Single-pin (drain-duration) constructor.</summary>
        public PulseFeedback(GpioPin pin, PulseFeedbackMode mode)
            : this(pin, null, mode) {
        }

        /// <summary>Two-pin (echo / duration-until-echo) constructor.</summary>
        public PulseFeedback(GpioPin pulsePin, GpioPin echoPin, PulseFeedbackMode mode) {

            this.DisableInterrupts = false;
            this.Timeout = TimeSpan.FromMilliseconds(100);
            this.PulseLength = TimeSpan.FromMilliseconds(20);
            this.PulseValue = GpioPinValue.High;
            this.EchoValue = GpioPinValue.High;

            this.mode = mode;

            this.pulsePin = pulsePin;
            this.echoPin = echoPin;

            this.pulsePinNumber = pulsePin.PinNumber;
            this.echoPinNumber = echoPin != null ? echoPin.PinNumber : -1;

            if (mode == PulseFeedbackMode.DrainDuration) {
                if (this.echoPin != null || this.pulsePin == null)
                    throw new ArgumentException();
            }
            else {
                if (this.echoPin == null || this.pulsePin == null) {
                    throw new ArgumentException();
                }
            }

            this.pulsePin.SetDriveMode(GpioPinDriveMode.Input);
            this.echoPin?.SetDriveMode(GpioPinDriveMode.Input);
        }

        /// <summary>Returns the pin(s) to high-impedance input.</summary>
        public void Dispose() {
            this.pulsePin.SetDriveMode(GpioPinDriveMode.Input);
            this.echoPin?.SetDriveMode(GpioPinDriveMode.Input);
        }

        /// <summary>
        /// Runs one stimulate-and-measure cycle. Returns the measured duration,
        /// or <see cref="Timeout"/> when no echo arrives in time.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern TimeSpan Trigger();
    }

    /// <summary>
    /// Drives a pin through a precise sequence of high/low transitions. Suitable
    /// for generating IR-remote waveforms, addressable-LED streams (WS281x), and
    /// other strict-timing protocols. Optional carrier-frequency modulation is
    /// available for IR.
    /// </summary>
    public class SignalGenerator : IDisposable {
        private readonly GpioPin pin;
        private readonly int pinNumber;

        private GpioPinValue idleValue;

        /// <summary>When true, the write runs with interrupts disabled for sub-microsecond accuracy.</summary>
        public bool DisableInterrupts { get; set; } = false;
        /// <summary>When true, the output is modulated with the carrier frequency.</summary>
        public bool GenerateCarrierFrequency { get; set; } = false;
        /// <summary>Carrier frequency in Hz used when <see cref="GenerateCarrierFrequency"/> is true.</summary>
        public long CarrierFrequency { get; } = 38000;
        /// <summary>The level the pin returns to between transitions.</summary>
        public GpioPinValue IdleValue { get => this.idleValue; set => this.pin.Write(this.idleValue = value); }

        /// <summary>Opens a signal generator on the given pin (drives it as a push-pull output).</summary>
        public SignalGenerator(GpioPin pin) {

            this.pin = pin;

            this.pinNumber = pin.PinNumber;

            this.pin.SetDriveMode(GpioPinDriveMode.Output);

            this.IdleValue = GpioPinValue.Low;
        }

        /// <summary>Returns the pin to high-impedance input.</summary>
        public void Dispose() => this.pin.SetDriveMode(GpioPinDriveMode.Input);

        /// <summary>Sets the pin to a static level.</summary>
        public void Write(GpioPinValue value) => this.pin.Write(value);

        /// <summary>Drives the pin through a sequence of timed transitions.</summary>
        /// <param name="buffer">Durations of each segment, alternating active/inactive starting from the current <see cref="IdleValue"/>'s opposite.</param>
        public void Write(TimeSpan[] buffer) => this.Write(buffer, 0, buffer.Length);

        /// <summary>Drives a slice of the timed transitions.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Write(TimeSpan[] buffer, int offset, int count);
    }

    /// <summary>
    /// Samples a digital input and records the durations between successive edges
    /// — the inverse of <see cref="SignalGenerator"/>. Useful for capturing IR-remote
    /// codes, decoding bit-banged protocols, or measuring pulse widths.
    /// </summary>
    public class SignalCapture : IDisposable {
        private readonly GpioPin pin;
        private readonly int pinNumber;

        /// <summary>When true, capture runs with interrupts disabled.</summary>
        public bool DisableInterrupts { get; set; } = false;
        /// <summary>Maximum total time the capture will wait before returning.</summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.MaxValue;
        /// <summary>Drive mode applied to the capture pin (typically input or input-pull-up).</summary>
        public GpioPinDriveMode DriveMode { get => this.pin.GetDriveMode(); set => this.pin.SetDriveMode(value); }

        /// <summary>Opens a capture session on the given pin.</summary>
        public SignalCapture(GpioPin pin) {

            this.pin = pin;
            this.pinNumber = pin.PinNumber;

            this.DriveMode = GpioPinDriveMode.Input;
        }

        /// <summary>Returns the pin to high-impedance input.</summary>
        public void Dispose() => this.pin.SetDriveMode(GpioPinDriveMode.Input);

        /// <summary>Samples the current level of the pin without waiting.</summary>
        public GpioPinValue Read() => this.pin.Read();

        /// <summary>Captures up to <paramref name="buffer"/>.Length inter-edge intervals starting now.</summary>
        /// <param name="initialState">Receives the level of the pin at the moment capture began.</param>
        /// <param name="buffer">Receives inter-edge intervals.</param>
        /// <returns>Number of intervals captured.</returns>
        public int Read(out GpioPinValue initialState, TimeSpan[] buffer) => this.Read(out initialState, buffer, 0, buffer.Length);

        /// <summary>Waits for the pin to reach <paramref name="waitForState"/>, then captures inter-edge intervals.</summary>
        /// <param name="waitForState">Level the capture should be armed by.</param>
        /// <param name="buffer">Receives inter-edge intervals.</param>
        /// <returns>Number of intervals captured.</returns>
        public int Read(GpioPinValue waitForState, TimeSpan[] buffer) => this.Read(waitForState, buffer, 0, buffer.Length);

        /// <summary>Captures a slice of inter-edge intervals.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int Read(out GpioPinValue initialState, TimeSpan[] buffer, int offset, int count);

        /// <summary>Captures a slice of inter-edge intervals after waiting for <paramref name="waitForState"/>.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int Read(GpioPinValue waitForState, TimeSpan[] buffer, int offset, int count);
    }

    /// <summary>
    /// DMA/timer-backed pulse-train I/O on a small set of pins. Capable of pulse
    /// counting (<see cref="ReadPulse"/>), high-resolution edge capture
    /// (<see cref="Capture"/>), and emitting pulse-width-modulated sequences
    /// (<see cref="Generate"/>). All three operations are mutually exclusive and
    /// run asynchronously — completion is reported via <see cref="OnReadPulseFinished"/>
    /// / <see cref="OnCaptureFinished"/> / <see cref="OnGenerateFinished"/>.
    /// Only specific pins are supported (currently 0, 1, and 19).
    /// </summary>
    public class DigitalSignal : IDisposable {

        private int pinNumber;
        private readonly NativeEventDispatcher nativeEventDispatcher;

        /// <summary>Handler for <see cref="OnReadPulseFinished"/>.</summary>
        public delegate void PulseReadEventHandler(DigitalSignal sender, TimeSpan duration, uint count, GpioPinValue initialState);
        /// <summary>Handler for <see cref="OnCaptureFinished"/>.</summary>
        public delegate void PulseCaptureEventHandler(DigitalSignal sender, double[] buffer, uint count, GpioPinValue initialState);
        /// <summary>Handler for <see cref="OnGenerateFinished"/>.</summary>
        public delegate void PulseGenerateEventHandler(DigitalSignal sender, GpioPinValue endState);

        private PulseReadEventHandler pulseReadCallback;
        private PulseCaptureEventHandler pulseCaptureCallback;
        private PulseGenerateEventHandler pulseGenerateCallback;

        // Single state field replacing the previous isBusy / isCaptureMode /
        // isWriteMode trio. Transitions go through Interlocked.CompareExchange
        // so the busy-check and mode-set can't tear and the IRQ handler can't
        // observe a half-set mode pair.
        private const int StateIdle = 0;
        private const int StateRead = 1;
        private const int StateCapture = 2;
        private const int StateGenerate = 3;
        private int state;

        /// <summary>True when no operation is in progress and <see cref="ReadPulse"/> may be called.</summary>
        public bool CanReadPulse => this.state == StateIdle;
        /// <summary>True when no operation is in progress and <see cref="Capture"/> may be called.</summary>
        public bool CanCapture => this.state == StateIdle;
        /// <summary>True when no operation is in progress and <see cref="Generate"/> may be called.</summary>
        public bool CanGenerate => this.state == StateIdle;

        /// <summary>Opens a digital-signal session on a supported pin (0, 1, or 19).</summary>
        /// <exception cref="NotSupportedException">Thrown when the pin is not one of the supported ones.</exception>
        public DigitalSignal(GpioPin pin) {
            if (pin == null)
                throw new ArgumentNullException(nameof(pin));

            this.pinNumber = pin.PinNumber;

            if (this.pinNumber == 0) {
                this.nativeEventDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.DigitalSignal.Event0");
            }
            else if (this.pinNumber == 1) {
                this.nativeEventDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.DigitalSignal.Event1");
            }
            else if (this.pinNumber == 19) {
                this.nativeEventDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.DigitalSignal.Event19");
            }
            else {
                // Native driver only raises events for pins 0, 1, 19. Reject
                // other pins explicitly instead of NRE'ing on the OnInterrupt
                // subscribe below.
                throw new NotSupportedException("DigitalSignal pin not supported on this target.");
            }

            this.nativeEventDispatcher.OnInterrupt += this.OnInterruptEventHandler;

            this.NativeAcquire();

            this.state = StateIdle;
        }

        void OnInterruptEventHandler(string apiName, long d0, long d1, long d2, IntPtr d3, DateTime ts) {
            // Snapshot the active mode atomically and reset to idle in one
            // step, then dispatch from the snapshot. This avoids a window
            // where the IRQ could see (busy=true, mode=stale).
            var currentState = Interlocked.Exchange(ref this.state, StateIdle);

            if (this.disposed || currentState == StateIdle || d0 != this.pinNumber || apiName.CompareTo("DigitalSignal") != 0)
                return;

            if (currentState == StateCapture) {
                if (d2 > 0) {
                    var data = new double[(int)d2];

                    if (this.NativeGetBuffer(data))
                        this.pulseCaptureCallback?.Invoke(this, data, (uint)data.Length, ((int)d3 != 0) ? GpioPinValue.High : GpioPinValue.Low);
                }
                else
                    // Native still reads the pin level when zero edges
                    // were captured; pass it through instead of hardcoding Low.
                    this.pulseCaptureCallback?.Invoke(this, null, 0, ((int)d3 != 0) ? GpioPinValue.High : GpioPinValue.Low);
            }
            else if (currentState == StateGenerate) {
                this.pulseGenerateCallback?.Invoke(this, ((int)d3 != 0) ? GpioPinValue.High : GpioPinValue.Low);
            }
            else {
                this.pulseReadCallback?.Invoke(this, new TimeSpan(d1), (uint)d2, ((int)d3 != 0) ? GpioPinValue.High : GpioPinValue.Low);
            }
        }

        private bool disposed;

        /// <summary>Releases the native pulse-train resources.</summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Dispose implementation.</summary>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {

                this.nativeEventDispatcher.OnInterrupt -= this.OnInterruptEventHandler;
                this.NativeRelease();

                this.state = StateIdle;

                this.disposed = true;
            }
        }

        /// <summary>Finalizer.</summary>
        ~DigitalSignal() {
            this.Dispose(false);
        }

        /// <summary>
        /// Counts up to <paramref name="pulseNum"/> edges matching <paramref name="edge"/>
        /// and measures the total elapsed time. Result is delivered via <see cref="OnReadPulseFinished"/>.
        /// </summary>
        /// <param name="pulseNum">Number of edges to count.</param>
        /// <param name="edge">Which edge(s) to count.</param>
        /// <param name="waitForEdge">When true, the timer starts on the first matching edge instead of immediately.</param>
        public void ReadPulse(uint pulseNum, GpioPinEdge edge, bool waitForEdge) {
            // Atomic transition idle -> Read. If we lose the race (another
            // op already armed), CompareExchange returns the prior state.
            if (Interlocked.CompareExchange(ref this.state, StateRead, StateIdle) != StateIdle)
                throw new InvalidOperationException();

            this.NativeRead(pulseNum, edge, waitForEdge);
        }

        /// <summary>Captures inter-edge intervals. See full-parameter overload for buffer-layout details.</summary>
        public void Capture(uint bufferSize, GpioPinEdge edge, bool waitForEdge) => this.Capture(bufferSize, edge, waitForEdge, TimeSpan.Zero);

        /// <summary>
        /// Capture timestamps of `count` edges on the pin.
        /// </summary>
        /// <remarks>
        /// The returned buffer holds nanosecond values that are inter-edge
        /// intervals EXCEPT for <c>buffer[0]</c>, which in both modes is a
        /// DMA-arm-to-first-captured-edge artifact:
        /// - When <paramref name="waitForEdge"/> is false the timer starts
        ///   immediately. <c>buffer[0]</c> is the time from timer-start to
        ///   the first edge (~150 ns of counter-vs-DMA startup latency
        ///   included). Returned length is <c>count</c>.
        /// - When <paramref name="waitForEdge"/> is true the FIRST signal
        ///   edge fires a GPIO ISR which arms the capture-DMA; the timer
        ///   then counts from zero and the next <c>count - 1</c> edges are
        ///   captured. Returned length is <c>count - 1</c>. <c>buffer[0]</c>
        ///   is the phase between DMA-arm and the next signal edge — a
        ///   random fraction of one period, NOT an edge1→edge2 interval.
        /// Callers performing jitter/period analysis should DISCARD
        /// <c>buffer[0]</c> in both modes; valid inter-edge intervals start
        /// at <c>buffer[1]</c>.
        /// </remarks>
        public void Capture(uint count, GpioPinEdge edge, bool waitForEdge, TimeSpan timeout) {
            if (Interlocked.CompareExchange(ref this.state, StateCapture, StateIdle) != StateIdle)
                throw new InvalidOperationException();

            this.NativeCapture(count, edge, waitForEdge, timeout);
        }

        /// <summary>Emits a pulse train described by <paramref name="data"/> with default multiplier and starting high.</summary>
        public void Generate(uint[] data, uint offset, uint count) => this.Generate(data, offset, count, 100, GpioPinValue.High);

        /// <summary>Emits a pulse train with explicit multiplier and starting high.</summary>
        public void Generate(uint[] data, uint offset, uint count, uint multiplier) => this.Generate(data, offset, count, multiplier, GpioPinValue.High);

        /// <summary>
        /// Emits a pulse train: each <paramref name="data"/> entry is a duration in
        /// timer ticks, alternating polarity starting from <paramref name="startingPolarity"/>.
        /// </summary>
        /// <param name="data">Buffer of segment durations.</param>
        /// <param name="offset">Starting index in <paramref name="data"/>.</param>
        /// <param name="count">Number of entries to emit.</param>
        /// <param name="multiplier">Multiplier applied to each duration entry (timer-tick scaling).</param>
        /// <param name="startingPolarity">Polarity of the first segment.</param>
        public void Generate(uint[] data, uint offset, uint count, uint multiplier, GpioPinValue startingPolarity) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Compare as long to avoid uint+uint wrap (offset+count could
            // overflow back to a small value and pass a naive uint compare).
            if ((long)offset + (long)count > data.Length)
                throw new ArgumentOutOfRangeException();

            if (Interlocked.CompareExchange(ref this.state, StateGenerate, StateIdle) != StateIdle)
                throw new InvalidOperationException();

            this.NativeWrite(data, offset, count, multiplier, startingPolarity);
        }

        /// <summary>Aborts the running operation, if any.</summary>
        public void Abort() => this.NativeAbort();

        /// <summary>Raised when <see cref="ReadPulse"/> completes.</summary>
        public event PulseReadEventHandler OnReadPulseFinished {
            add => this.pulseReadCallback += value;
            remove => this.pulseReadCallback -= value;
        }

        /// <summary>Raised when <see cref="Capture"/> completes.</summary>
        public event PulseCaptureEventHandler OnCaptureFinished {
            add => this.pulseCaptureCallback += value;
            remove => this.pulseCaptureCallback -= value;
        }

        /// <summary>Raised when <see cref="Generate"/> completes.</summary>
        public event PulseGenerateEventHandler OnGenerateFinished {
            add => this.pulseGenerateCallback += value;
            remove => this.pulseGenerateCallback -= value;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeAcquire();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeRelease();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeRead(uint count, GpioPinEdge edge, bool waitForEdge);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeCapture(uint count, GpioPinEdge edge, bool waitForEdge, TimeSpan timeout);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeAbort();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern bool NativeGetBuffer(double[] data);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeWrite(uint[] data, uint offset, uint count, uint multiplier, GpioPinValue polarity);

    }
}
