using System;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Gpio;

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
}
