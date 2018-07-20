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
        private readonly uint pulsePinNumber;
        private readonly uint echoPinNumber;
        private readonly GpioPin pulsePin;
        private readonly GpioPin echoPin;

        public bool DisableInterrupts { get; set; }
        public TimeSpan Timeout { get; set; }
        public TimeSpan PulseLength { get; set; }
        public GpioPinValue PulsePinValue { get; set; }
        public GpioPinValue EchoPinValue { get; set; }
        public GpioPinDriveMode EchoPinDriveMode { get; set; }

        public PulseFeedback(uint pinNumber, PulseFeedbackMode mode)
            : this(GpioController.GetDefault(), pinNumber, mode) {

        }

        public PulseFeedback(uint pulsePinNumber, uint echoPinNumber, PulseFeedbackMode mode)
            : this(GpioController.GetDefault(), pulsePinNumber, echoPinNumber, mode) {

        }

        public PulseFeedback(GpioController gpioController, uint pinNumber, PulseFeedbackMode mode)
            : this(gpioController, pinNumber, pinNumber, mode) {

        }

        public PulseFeedback(GpioController gpioController, uint pulsePinNumber, uint echoPinNumber, PulseFeedbackMode mode) {
            if (!(gpioController.Provider is Gpio.Provider.GpioControllerApiWrapper p)) throw new NotSupportedException();

            this.DisableInterrupts = false;
            this.Timeout = TimeSpan.FromMilliseconds(100);
            this.PulseLength = TimeSpan.FromMilliseconds(20);
            this.PulsePinValue = GpioPinValue.High;
            this.EchoPinValue = GpioPinValue.High;
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
        public extern ulong GeneratePulse();
    }

    public sealed class SignalGenerator : IDisposable {
        private readonly IntPtr gpioApi;
        private readonly uint pinNumber;
        private readonly GpioPin pin;

        public bool DisableInterrupts { get; set; }
        public bool GeneratecarrierFrequency { get; set; }
        public uint CarrierFrequency { get; set; }

        public SignalGenerator(uint pinNumber, GpioPinValue initialValue) : this(GpioController.GetDefault(), pinNumber, initialValue) {

        }

        public SignalGenerator(GpioController gpioController, uint pinNumber, GpioPinValue initialValue) {
            if (!(gpioController.Provider is Gpio.Provider.GpioControllerApiWrapper p)) throw new NotSupportedException();

            this.gpioApi = p.Api.Implementation;
            this.pinNumber = pinNumber;

            this.pin = gpioController.OpenPin(pinNumber);
            this.pin.SetDriveMode(GpioPinDriveMode.Output);
            this.pin.Write(initialValue);
        }

        public void Dispose() => this.pin.Dispose();

        public void Write(GpioPinValue value) => this.pin.Write(value);

        public void Write(uint[] buffer) => this.Write(buffer, 0, buffer.Length);
        public void Write(uint[] buffer, int offset, int count) => this.Write(buffer, (uint)offset, (uint)count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void Write(uint[] buffer, uint offset, uint count);
    }

    public sealed class SignalCapture : IDisposable {
        private readonly IntPtr gpioApi;
        private readonly uint pinNumber;
        private readonly GpioPin pin;

        public bool DisableInterrupts { get; set; }
        public TimeSpan Timeout { get; set; }

        public SignalCapture(uint pinNumber, GpioPinDriveMode driveMode) : this(GpioController.GetDefault(), pinNumber, driveMode) {

        }

        public SignalCapture(GpioController gpioController, uint pinNumber, GpioPinDriveMode driveMode) {
            if (!(gpioController.Provider is Gpio.Provider.GpioControllerApiWrapper p)) throw new NotSupportedException();

            switch (driveMode) {
                case GpioPinDriveMode.Input:
                case GpioPinDriveMode.InputPullUp:
                case GpioPinDriveMode.InputPullDown: break;
                default: throw new ArgumentException("Invalid drive mode.");
            }

            this.Timeout = TimeSpan.MaxValue;

            this.gpioApi = p.Api.Implementation;
            this.pinNumber = pinNumber;

            this.pin = gpioController.OpenPin(pinNumber);
            this.pin.SetDriveMode(driveMode);
        }

        public void Dispose() => this.pin.Dispose();

        public GpioPinValue Read() => this.pin.Read();

        public int Read(out bool initialState, uint[] buffer) => this.Read(out initialState, buffer, 0, buffer.Length);
        public int Read(out bool initialState, uint[] buffer, int offset, int count) => (int)this.Read(out initialState, buffer, (uint)offset, (uint)count);

        public int Read(bool waitForState, uint[] buffer) => this.Read(waitForState, buffer, 0, buffer.Length);
        public int Read(bool waitForState, uint[] buffer, int offset, int count) => (int)this.Read(waitForState, buffer, (uint)offset, (uint)count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern uint Read(out bool initialState, uint[] buffer, uint offset, uint count);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern uint Read(bool waitforInitialState, uint[] buffer, uint offset, uint count);
    }
}