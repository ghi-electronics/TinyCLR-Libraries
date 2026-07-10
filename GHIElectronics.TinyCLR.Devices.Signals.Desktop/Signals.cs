using System;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Native;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.Signals\Signals.cs.
// Bodies on Desktop are safe no-ops; Trigger/Read/Write return zero/empty.
namespace GHIElectronics.TinyCLR.Devices.Signals {
    public enum PulseFeedbackMode {
        DrainDuration,
        EchoDuration,
        DurationUntilEcho
    }

    public class PulseFeedback : IDisposable {
        private readonly GpioPin pulsePin;
        private readonly GpioPin echoPin;

        public bool DisableInterrupts { get; set; }
        public TimeSpan Timeout { get; set; }
        public TimeSpan PulseLength { get; set; }
        public GpioPinValue PulseValue { get; set; }
        public GpioPinValue EchoValue { get; set; }

        public PulseFeedback(GpioPin pin, PulseFeedbackMode mode) : this(pin, null, mode) { }

        public PulseFeedback(GpioPin pulsePin, GpioPin echoPin, PulseFeedbackMode mode) {
            this.DisableInterrupts = false;
            this.Timeout = TimeSpan.FromMilliseconds(100);
            this.PulseLength = TimeSpan.FromMilliseconds(20);
            this.PulseValue = GpioPinValue.High;
            this.EchoValue = GpioPinValue.High;

            this.pulsePin = pulsePin;
            this.echoPin = echoPin;

            if (mode == PulseFeedbackMode.DrainDuration) {
                if (this.echoPin != null || this.pulsePin == null) throw new ArgumentException();
            }
            else {
                if (this.echoPin == null || this.pulsePin == null) throw new ArgumentException();
            }

            this.pulsePin.SetDriveMode(GpioPinDriveMode.Input);
            this.echoPin?.SetDriveMode(GpioPinDriveMode.Input);
        }

        public void Dispose() {
            this.pulsePin.SetDriveMode(GpioPinDriveMode.Input);
            this.echoPin?.SetDriveMode(GpioPinDriveMode.Input);
        }

        public TimeSpan Trigger() => TimeSpan.Zero;
    }

    public class SignalGenerator : IDisposable {
        private readonly GpioPin pin;
        private GpioPinValue idleValue;

        public bool DisableInterrupts { get; set; } = false;
        public bool GenerateCarrierFrequency { get; set; } = false;
        public long CarrierFrequency { get; } = 38000;
        public GpioPinValue IdleValue { get => this.idleValue; set => this.pin.Write(this.idleValue = value); }

        public SignalGenerator(GpioPin pin) {
            this.pin = pin;
            this.pin.SetDriveMode(GpioPinDriveMode.Output);
            this.IdleValue = GpioPinValue.Low;
        }

        public void Dispose() => this.pin.SetDriveMode(GpioPinDriveMode.Input);

        public void Write(GpioPinValue value) => this.pin.Write(value);
        public void Write(TimeSpan[] buffer) => this.Write(buffer, 0, buffer.Length);
        public void Write(TimeSpan[] buffer, int offset, int count) { }
    }

    public class SignalCapture : IDisposable {
        private readonly GpioPin pin;

        public bool DisableInterrupts { get; set; } = false;
        public TimeSpan Timeout { get; set; } = TimeSpan.MaxValue;
        public GpioPinDriveMode DriveMode { get => this.pin.GetDriveMode(); set => this.pin.SetDriveMode(value); }

        public SignalCapture(GpioPin pin) {
            this.pin = pin;
            this.DriveMode = GpioPinDriveMode.Input;
        }

        public void Dispose() => this.pin.SetDriveMode(GpioPinDriveMode.Input);

        public GpioPinValue Read() => this.pin.Read();

        public int Read(out GpioPinValue initialState, TimeSpan[] buffer) => this.Read(out initialState, buffer, 0, buffer.Length);
        public int Read(GpioPinValue waitForState, TimeSpan[] buffer) => this.Read(waitForState, buffer, 0, buffer.Length);

        public int Read(out GpioPinValue initialState, TimeSpan[] buffer, int offset, int count) {
            initialState = GpioPinValue.Low;
            return 0;
        }

        public int Read(GpioPinValue waitForState, TimeSpan[] buffer, int offset, int count) => 0;
    }

    public class DigitalSignal : IDisposable {
        public delegate void PulseReadEventHandler(DigitalSignal sender, TimeSpan duration, uint count, GpioPinValue initialState);
        public delegate void PulseCaptureEventHandler(DigitalSignal sender, double[] buffer, uint count, GpioPinValue initialState);
        public delegate void PulseGenerateEventHandler(DigitalSignal sender, GpioPinValue endState);

        private bool disposed;

        public bool CanReadPulse => !this.disposed;
        public bool CanCapture => !this.disposed;
        public bool CanGenerate => !this.disposed;

        public DigitalSignal(GpioPin pin) { }

        public void Dispose() {
            if (!this.disposed) this.disposed = true;
            GC.SuppressFinalize(this);
        }

        ~DigitalSignal() => this.Dispose();

        public void ReadPulse(uint pulseNum, GpioPinEdge edge, bool waitForEdge) { }
        public void Capture(uint bufferSize, GpioPinEdge edge, bool waitForEdge) { }
        public void Capture(uint count, GpioPinEdge edge, bool waitForEdge, TimeSpan timeout) { }
        public void Generate(uint[] data, uint offset, uint count) { }
        public void Generate(uint[] data, uint offset, uint count, uint multiplier) { }
        public void Generate(uint[] data, uint offset, uint count, uint multiplier, GpioPinValue startingPolarity) { }
        public void Generate(uint[] data, uint offset, uint count, uint multiplier, GpioPinValue startingPolarity, uint repeatCount) { }
        public void Abort() { }

        public event PulseReadEventHandler OnReadPulseFinished { add { } remove { } }
        public event PulseCaptureEventHandler OnCaptureFinished { add { } remove { } }
        public event PulseGenerateEventHandler OnGenerateFinished { add { } remove { } }
    }
}
