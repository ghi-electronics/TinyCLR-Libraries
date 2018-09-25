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
        private readonly IntPtr gpioApi;
        private readonly int pulsePinNumber;
        private readonly int echoPinNumber;
        private readonly GpioPin pulsePin;
        private readonly GpioPin echoPin;

        public bool DisableInterrupts { get; set; }
        public TimeSpan Timeout { get; set; }
        public TimeSpan PulseLength { get; set; }
        public GpioPinValue PulsePinValue { get; set; }
        public GpioPinValue EchoPinValue { get; set; }
        public GpioPinDriveMode PulsePinDriveMode { get; set; }
        public GpioPinDriveMode EchoPinDriveMode { get; set; }

        public PulseFeedback(int pinNumber, PulseFeedbackMode mode)
            : this(GpioController.GetDefault(), pinNumber, mode) {

        }

        public PulseFeedback(int pulsePinNumber, int echoPinNumber, PulseFeedbackMode mode)
            : this(GpioController.GetDefault(), pulsePinNumber, echoPinNumber, mode) {

        }

        public PulseFeedback(GpioController gpioController, int pinNumber, PulseFeedbackMode mode)
            : this(gpioController, pinNumber, pinNumber, mode) {

        }

        public PulseFeedback(GpioController gpioController, int pulsePinNumber, int echoPinNumber, PulseFeedbackMode mode) {
            if (!(gpioController.Provider is Gpio.Provider.GpioControllerApiWrapper p)) throw new NotSupportedException();

            this.DisableInterrupts = false;
            this.Timeout = TimeSpan.FromMilliseconds(100);
            this.PulseLength = TimeSpan.FromMilliseconds(20);
            this.PulsePinValue = GpioPinValue.High;
            this.EchoPinValue = GpioPinValue.High;
            this.PulsePinDriveMode = GpioPinDriveMode.Input;
            this.EchoPinDriveMode = GpioPinDriveMode.Input;

            this.mode = mode;
            this.gpioApi = p.Api.Implementation;
            this.pulsePinNumber = pulsePinNumber;
            this.echoPinNumber = echoPinNumber;

            this.pulsePin = gpioController.OpenPin(pulsePinNumber);

            if (this.pulsePinNumber != this.echoPinNumber)
                this.echoPin = gpioController.OpenPin(echoPinNumber);

            this.pulsePin.SetDriveMode(GpioPinDriveMode.Input);
            this.echoPin?.SetDriveMode(GpioPinDriveMode.Input);
        }

        public void Dispose() {
            this.pulsePin.Dispose();
            this.echoPin?.Dispose();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern TimeSpan GeneratePulse();
    }

    public sealed class SignalGenerator : IDisposable {
        private readonly IntPtr gpioApi;
        private readonly int pinNumber;
        private readonly GpioPin pin;
        private GpioPinValue idleValue;

        public bool DisableInterrupts { get; set; } = false;
        public bool GenerateCarrierFrequency { get; set; } = false;
        public long CarrierFrequency { get; set; } = 0;
        public GpioPinValue IdleValue { get => this.idleValue; set => this.pin.Write(this.idleValue = value); }

        public SignalGenerator(int pinNumber) : this(GpioController.GetDefault(), pinNumber) {

        }

        public SignalGenerator(GpioController gpioController, int pinNumber) {
            if (!(gpioController.Provider is Gpio.Provider.GpioControllerApiWrapper p)) throw new NotSupportedException();

            this.gpioApi = p.Api.Implementation;
            this.pinNumber = pinNumber;
            this.pin = gpioController.OpenPin(pinNumber);

            this.pin.SetDriveMode(GpioPinDriveMode.Output);

            this.IdleValue = GpioPinValue.Low;
        }

        public void Dispose() => this.pin.Dispose();

        public void Write(GpioPinValue value) => this.pin.Write(value);

        public void Write(TimeSpan[] buffer) => this.Write(buffer, 0, buffer.Length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern void Write(TimeSpan[] buffer, int offset, int count);
    }

    public sealed class SignalCapture : IDisposable {
        private readonly IntPtr gpioApi;
        private readonly int pinNumber;
        private readonly GpioPin pin;

        public bool DisableInterrupts { get; set; } = false;
        public TimeSpan Timeout { get; set; } = TimeSpan.MaxValue;
        public GpioPinDriveMode DriveMode { get => this.pin.GetDriveMode(); set => this.pin.SetDriveMode(value); }

        public SignalCapture(int pinNumber) : this(GpioController.GetDefault(), pinNumber) {

        }

        public SignalCapture(GpioController gpioController, int pinNumber) {
            if (!(gpioController.Provider is Gpio.Provider.GpioControllerApiWrapper p)) throw new NotSupportedException();

            this.gpioApi = p.Api.Implementation;
            this.pinNumber = pinNumber;
            this.pin = gpioController.OpenPin(pinNumber);

            this.DriveMode = GpioPinDriveMode.Input;
        }

        public void Dispose() => this.pin.Dispose();

        public GpioPinValue Read() => this.pin.Read();

        public int Read(out GpioPinValue initialState, TimeSpan[] buffer) => this.Read(out initialState, buffer, 0, buffer.Length);

        public int Read(GpioPinValue waitForState, TimeSpan[] buffer) => this.Read(waitForState, buffer, 0, buffer.Length);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int Read(out GpioPinValue initialState, TimeSpan[] buffer, int offset, int count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int Read(GpioPinValue waitForState, TimeSpan[] buffer, int offset, int count);
    }
}