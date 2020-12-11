using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Signals {
    public enum PulseFeedbackMode {
        DrainDuration,
        EchoDuration,
        DurationUntilEcho
    }

    public sealed class PulseFeedback : IDisposable {
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

    public sealed class SignalGenerator : IDisposable {
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

    public sealed class SignalCapture : IDisposable {
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

        private PulseReadEventHandler pulseReadCallback;
        private PulseCaptureEventHandler pulseCaptureCallback;

        private bool isReading;
        private bool isCapture;

        public bool CanReadPulse => !this.isReading;
        public bool CanCapture => !this.isReading;

        public DigitalSignal(GpioPin pin) {
            this.pinNumber = pin.PinNumber;

            this.nativeEventDispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.DigitalSignal.Event");

            this.nativeEventDispatcher.OnInterrupt += (apiName, d0, d1, d2, d3, ts) => {
                if (!this.disposed && this.isReading && d0 == this.pinNumber && apiName.CompareTo("DigitalSignal") == 0) {
                    if (this.isCapture == true) {
                        if (d2 > 0) {
                            var data = new double[(int)d2];

                            if (this.NativeGetBuffer(data))
                                this.pulseCaptureCallback?.Invoke(this, data, (uint)data.Length, ((int)d3 != 0) ? GpioPinValue.High : GpioPinValue.Low);
                        }
                        else
                            this.pulseCaptureCallback?.Invoke(this, null, 0, GpioPinValue.Low);
                    }
                    else {
                        this.pulseReadCallback?.Invoke(this, new TimeSpan(d1), (uint)d2, ((int)d3 != 0) ? GpioPinValue.High : GpioPinValue.Low);
                    }

                }

                this.isReading = false;
                this.isCapture = false;
            };

            this.NativeAcquire();

            this.isReading = false;
        }

        private bool disposed;

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {

                this.NativeRelease();

                this.isReading = false;

                this.disposed = true;
            }
        }

        ~DigitalSignal() {
            this.Dispose(false);
        }

        public void ReadPulse(uint pulseNum, GpioPinEdge edge, bool waitForEdge) {
            if (this.isReading)
                new InvalidOperationException();

            this.isReading = true;
            this.isCapture = false;

            this.NativeRead(pulseNum, edge, waitForEdge);
        }

        public void Capture(uint count, GpioPinEdge edge, bool waitForEdge, TimeSpan timeout) {
            if (this.isReading)
                new InvalidOperationException();

            this.isReading = true;
            this.isCapture = true;

            this.NativeCapture(count, edge, waitForEdge, timeout);
        }

        public void Capture(uint bufferSize, GpioPinEdge edge, bool waitForEdge) => this.Capture(bufferSize, edge, waitForEdge, TimeSpan.Zero);

        public void Abort() => this.NativeAbort();

        public event PulseReadEventHandler OnReadPulseReady {
            add {
                this.pulseReadCallback += value;
            }
            remove {
                this.pulseReadCallback -= value;
            }
        }

        public event PulseCaptureEventHandler OnCaptureReady {
            add {
                this.pulseCaptureCallback += value;
            }
            remove {
                this.pulseCaptureCallback -= value;
            }
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

    }
}
