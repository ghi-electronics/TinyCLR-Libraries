using System;
using System.Runtime.CompilerServices;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Signals {
    public enum PulseFeedbackMode {
        DrainDuration,
        EchoDuration,
        DurationUntilEcho
    }

    public class PulseFeedback : IDisposable {
        private readonly PulseFeedbackMode mode;

        private readonly GpioPin pulsePin;
        private readonly GpioPin echoPin;

        private readonly int pulsePinNumber;
        private readonly int echoPinNumber;

        public bool DisableInterrupts { get; set; }
        public TimeSpan Timeout { get; set; }
        public TimeSpan PulseLength { get; set; }
        public GpioPinValue PulseValue { get; set; }
        public GpioPinValue EchoValue { get; set; }

        public PulseFeedback(GpioPin pin, PulseFeedbackMode mode)
            : this(pin, null, mode) {
        }

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

        public void Dispose() {
            this.pulsePin.SetDriveMode(GpioPinDriveMode.Input);
            this.echoPin?.SetDriveMode(GpioPinDriveMode.Input);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern TimeSpan Trigger();
    }

    public class SignalGenerator : IDisposable {
        private readonly GpioPin pin;
        private readonly int pinNumber;

        private GpioPinValue idleValue;

        public bool DisableInterrupts { get; set; } = false;
        public bool GenerateCarrierFrequency { get; set; } = false;
        public long CarrierFrequency { get; } = 38000;
        public GpioPinValue IdleValue { get => this.idleValue; set => this.pin.Write(this.idleValue = value); }

        public SignalGenerator(GpioPin pin) {

            this.pin = pin;

            this.pinNumber = pin.PinNumber;

            this.pin.SetDriveMode(GpioPinDriveMode.Output);

            this.IdleValue = GpioPinValue.Low;
        }

        public void Dispose() => this.pin.SetDriveMode(GpioPinDriveMode.Input);

        public void Write(GpioPinValue value) => this.pin.Write(value);

        public void Write(TimeSpan[] buffer) => this.Write(buffer, 0, buffer.Length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Write(TimeSpan[] buffer, int offset, int count);
    }

    public class SignalCapture : IDisposable {
        private readonly GpioPin pin;
        private readonly int pinNumber;

        public bool DisableInterrupts { get; set; } = false;
        public TimeSpan Timeout { get; set; } = TimeSpan.MaxValue;
        public GpioPinDriveMode DriveMode { get => this.pin.GetDriveMode(); set => this.pin.SetDriveMode(value); }

        public SignalCapture(GpioPin pin) {

            this.pin = pin;
            this.pinNumber = pin.PinNumber;

            this.DriveMode = GpioPinDriveMode.Input;
        }

        public void Dispose() => this.pin.SetDriveMode(GpioPinDriveMode.Input);

        public GpioPinValue Read() => this.pin.Read();

        public int Read(out GpioPinValue initialState, TimeSpan[] buffer) => this.Read(out initialState, buffer, 0, buffer.Length);

        public int Read(GpioPinValue waitForState, TimeSpan[] buffer) => this.Read(waitForState, buffer, 0, buffer.Length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int Read(out GpioPinValue initialState, TimeSpan[] buffer, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int Read(GpioPinValue waitForState, TimeSpan[] buffer, int offset, int count);
    }

    public class DigitalSignal : IDisposable {

        private int pinNumber;
        private readonly NativeEventDispatcher nativeEventDispatcher;

        public delegate void PulseReadEventHandler(DigitalSignal sender, TimeSpan duration, uint count, GpioPinValue initialState);
        public delegate void PulseCaptureEventHandler(DigitalSignal sender, double[] buffer, uint count, GpioPinValue initialState);
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

        public bool CanReadPulse => this.state == StateIdle;
        public bool CanCapture => this.state == StateIdle;
        public bool CanGenerate => this.state == StateIdle;

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

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {

                this.nativeEventDispatcher.OnInterrupt -= this.OnInterruptEventHandler;
                this.NativeRelease();

                this.state = StateIdle;

                this.disposed = true;
            }
        }

        ~DigitalSignal() {
            this.Dispose(false);
        }

        public void ReadPulse(uint pulseNum, GpioPinEdge edge, bool waitForEdge) {
            // Atomic transition idle -> Read. If we lose the race (another
            // op already armed), CompareExchange returns the prior state.
            if (Interlocked.CompareExchange(ref this.state, StateRead, StateIdle) != StateIdle)
                throw new InvalidOperationException();

            this.NativeRead(pulseNum, edge, waitForEdge);
        }

        public void Capture(uint bufferSize, GpioPinEdge edge, bool waitForEdge) => this.Capture(bufferSize, edge, waitForEdge, TimeSpan.Zero);

        /// <summary>
        /// Capture timestamps of `count` edges on the pin.
        /// </summary>
        /// <remarks>
        /// The returned buffer holds inter-edge intervals in nanoseconds:
        /// - When <paramref name="waitForEdge"/> is true the timer starts on the
        ///   first edge, so buffer[0] is the interval between the first and
        ///   second edges, buffer[i] the interval from edge i+1 to edge i+2,
        ///   and the returned length is <c>count - 1</c>.
        /// - When <paramref name="waitForEdge"/> is false the timer starts
        ///   immediately. buffer[0] is the time from timer-start to the first
        ///   edge (and includes ~150 ns of counter-vs-DMA startup latency);
        ///   for high-frequency signals discard buffer[0].
        /// </remarks>
        public void Capture(uint count, GpioPinEdge edge, bool waitForEdge, TimeSpan timeout) {
            if (Interlocked.CompareExchange(ref this.state, StateCapture, StateIdle) != StateIdle)
                throw new InvalidOperationException();

            this.NativeCapture(count, edge, waitForEdge, timeout);
        }

        public void Generate(uint[] data, uint offset, uint count) => this.Generate(data, offset, count, 100, GpioPinValue.High);

        public void Generate(uint[] data, uint offset, uint count, uint multiplier) => this.Generate(data, offset, count, multiplier, GpioPinValue.High);

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

        public void Abort() => this.NativeAbort();

        public event PulseReadEventHandler OnReadPulseFinished {
            add => this.pulseReadCallback += value;
            remove => this.pulseReadCallback -= value;
        }

        public event PulseCaptureEventHandler OnCaptureFinished {
            add => this.pulseCaptureCallback += value;
            remove => this.pulseCaptureCallback -= value;
        }

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
