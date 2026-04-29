using System;
using System.Collections;

namespace System.Device.Gpio {
    [Flags]
    public enum PinEventTypes {
        Rising = 1,
        Falling = 2,
    }

    public enum PinMode {
        Input = 0,
        Output = 1,
        InputPullUp = 2,
        InputPullDown = 3,
        OutputOpenDrain = 4,
    }

    public enum PinValue {
        Low = 0,
        High = 1,
    }

    public enum PinNumberingScheme {
        Logical = 0,
        Board = 1,
        Bcm = 2,
    }

    public sealed class PinValueChangedEventArgs : EventArgs {
        public PinEventTypes ChangeType { get; }
        public int PinNumber { get; }

        public PinValueChangedEventArgs(PinEventTypes changeType, int pinNumber) {
            this.ChangeType = changeType;
            this.PinNumber = pinNumber;
        }
    }

    public delegate void PinChangeEventHandler(object sender, PinValueChangedEventArgs pinValueChangedEventArgs);

    public abstract class GpioDriver : IDisposable {
        public abstract int PinCount { get; }

        protected internal abstract void OpenPin(int pinNumber);
        protected internal abstract void ClosePin(int pinNumber);
        protected internal abstract bool IsPinModeSupported(int pinNumber, PinMode mode);
        protected internal abstract void SetPinMode(int pinNumber, PinMode mode);
        protected internal abstract PinMode GetPinMode(int pinNumber);
        protected internal abstract PinValue Read(int pinNumber);
        protected internal abstract void Write(int pinNumber, PinValue value);
        protected internal abstract void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback);
        protected internal abstract void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback);

        public abstract void Dispose();
    }

    public class TinyClrGpioDriver : GpioDriver {
        private readonly GHIElectronics.TinyCLR.Devices.Gpio.GpioController controller;
        private readonly Hashtable pinToTinyClrPin;
        private readonly Hashtable callbackMap;
        private readonly int pinBase;

        private sealed class CallbackRegistration {
            public GHIElectronics.TinyCLR.Devices.Gpio.GpioPinValueChangedEventHandler Handler;
        }

        public TinyClrGpioDriver() : this(GHIElectronics.TinyCLR.Devices.Gpio.GpioController.GetDefault(), 0) {
        }

        public TinyClrGpioDriver(int pinBase) : this(GHIElectronics.TinyCLR.Devices.Gpio.GpioController.GetDefault(), pinBase) {
        }

        public TinyClrGpioDriver(GHIElectronics.TinyCLR.Devices.Gpio.GpioController tinyClrController, int pinBase = 0) {
            this.controller = tinyClrController ?? throw new ArgumentNullException(nameof(tinyClrController));
            this.pinToTinyClrPin = new Hashtable();
            this.callbackMap = new Hashtable();
            this.pinBase = pinBase;
        }

        public override int PinCount => this.controller.Provider.PinCount;

        public override void Dispose() {
            lock (this.pinToTinyClrPin) {
                foreach (DictionaryEntry entry in this.pinToTinyClrPin)
                    ((GHIElectronics.TinyCLR.Devices.Gpio.GpioPin)entry.Value).Dispose();

                this.pinToTinyClrPin.Clear();
            }

            this.callbackMap.Clear();
            this.controller.Dispose();
        }

        protected internal override void OpenPin(int pinNumber) {
            var mappedPin = this.MapPinNumber(pinNumber);

            lock (this.pinToTinyClrPin) {
                if (this.pinToTinyClrPin.Contains(pinNumber))
                    return;

                this.pinToTinyClrPin[pinNumber] = this.controller.OpenPin(mappedPin);
            }
        }

        protected internal override void ClosePin(int pinNumber) {
            lock (this.pinToTinyClrPin) {
                if (!this.pinToTinyClrPin.Contains(pinNumber))
                    return;

                var tinyPin = (GHIElectronics.TinyCLR.Devices.Gpio.GpioPin)this.pinToTinyClrPin[pinNumber];
                tinyPin.Dispose();
                this.pinToTinyClrPin.Remove(pinNumber);
            }
        }

        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode) =>
            this.GetTinyClrPin(pinNumber).IsDriveModeSupported(this.MapPinNumber(pinNumber), ToTinyClrDriveMode(mode));

        protected internal override void SetPinMode(int pinNumber, PinMode mode) =>
            this.GetTinyClrPin(pinNumber).SetDriveMode(ToTinyClrDriveMode(mode));

        protected internal override PinMode GetPinMode(int pinNumber) => ToPinMode(this.GetTinyClrPin(pinNumber).GetDriveMode());

        protected internal override PinValue Read(int pinNumber) =>
            this.GetTinyClrPin(pinNumber).Read() == GHIElectronics.TinyCLR.Devices.Gpio.GpioPinValue.High ? PinValue.High : PinValue.Low;

        protected internal override void Write(int pinNumber, PinValue value) =>
            this.GetTinyClrPin(pinNumber).Write(value == PinValue.High ? GHIElectronics.TinyCLR.Devices.Gpio.GpioPinValue.High : GHIElectronics.TinyCLR.Devices.Gpio.GpioPinValue.Low);

        protected internal override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback) {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            var tinyPin = this.GetTinyClrPin(pinNumber);
            tinyPin.ValueChangedEdge = ToTinyClrEdge(eventTypes);

            var key = GetCallbackKey(pinNumber, callback);
            lock (this.callbackMap) {
                if (this.callbackMap.Contains(key))
                    return;

                GHIElectronics.TinyCLR.Devices.Gpio.GpioPinValueChangedEventHandler handler = (s, e) =>
                    callback(this, new PinValueChangedEventArgs(ToPinEventTypes(e.Edge), pinNumber));

                tinyPin.ValueChanged += handler;
                this.callbackMap[key] = new CallbackRegistration { Handler = handler };
            }
        }

        protected internal override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback) {
            if (callback == null)
                return;

            var key = GetCallbackKey(pinNumber, callback);
            var tinyPin = this.GetTinyClrPin(pinNumber);

            lock (this.callbackMap) {
                if (!this.callbackMap.Contains(key))
                    return;

                var registration = (CallbackRegistration)this.callbackMap[key];
                tinyPin.ValueChanged -= registration.Handler;
                this.callbackMap.Remove(key);
            }
        }

        protected virtual int MapPinNumber(int pinNumber) => checked(this.pinBase + pinNumber);

        private GHIElectronics.TinyCLR.Devices.Gpio.GpioPin GetTinyClrPin(int pinNumber) {
            lock (this.pinToTinyClrPin) {
                if (!this.pinToTinyClrPin.Contains(pinNumber))
                    throw new InvalidOperationException("Pin has not been opened.");

                return (GHIElectronics.TinyCLR.Devices.Gpio.GpioPin)this.pinToTinyClrPin[pinNumber];
            }
        }

        private static long GetCallbackKey(int pinNumber, PinChangeEventHandler callback) =>
            ((long)pinNumber << 32) | (uint)callback.GetHashCode();

        private static GHIElectronics.TinyCLR.Devices.Gpio.GpioPinDriveMode ToTinyClrDriveMode(PinMode mode) {
            switch (mode) {
                case PinMode.Input: return GHIElectronics.TinyCLR.Devices.Gpio.GpioPinDriveMode.Input;
                case PinMode.Output: return GHIElectronics.TinyCLR.Devices.Gpio.GpioPinDriveMode.Output;
                case PinMode.InputPullUp: return GHIElectronics.TinyCLR.Devices.Gpio.GpioPinDriveMode.InputPullUp;
                case PinMode.InputPullDown: return GHIElectronics.TinyCLR.Devices.Gpio.GpioPinDriveMode.InputPullDown;
                case PinMode.OutputOpenDrain: return GHIElectronics.TinyCLR.Devices.Gpio.GpioPinDriveMode.OutputOpenDrain;
                default: throw new NotSupportedException("Unsupported pin mode.");
            }
        }

        private static PinMode ToPinMode(GHIElectronics.TinyCLR.Devices.Gpio.GpioPinDriveMode mode) {
            switch (mode) {
                case GHIElectronics.TinyCLR.Devices.Gpio.GpioPinDriveMode.Input: return PinMode.Input;
                case GHIElectronics.TinyCLR.Devices.Gpio.GpioPinDriveMode.Output: return PinMode.Output;
                case GHIElectronics.TinyCLR.Devices.Gpio.GpioPinDriveMode.InputPullUp: return PinMode.InputPullUp;
                case GHIElectronics.TinyCLR.Devices.Gpio.GpioPinDriveMode.InputPullDown: return PinMode.InputPullDown;
                case GHIElectronics.TinyCLR.Devices.Gpio.GpioPinDriveMode.OutputOpenDrain: return PinMode.OutputOpenDrain;
                default: throw new NotSupportedException("Unsupported drive mode.");
            }
        }

        private static GHIElectronics.TinyCLR.Devices.Gpio.GpioPinEdge ToTinyClrEdge(PinEventTypes eventTypes) {
            var edge = (GHIElectronics.TinyCLR.Devices.Gpio.GpioPinEdge)0;
            if ((eventTypes & PinEventTypes.Rising) != 0) edge |= GHIElectronics.TinyCLR.Devices.Gpio.GpioPinEdge.RisingEdge;
            if ((eventTypes & PinEventTypes.Falling) != 0) edge |= GHIElectronics.TinyCLR.Devices.Gpio.GpioPinEdge.FallingEdge;
            return edge;
        }

        private static PinEventTypes ToPinEventTypes(GHIElectronics.TinyCLR.Devices.Gpio.GpioPinEdge edge) {
            var types = (PinEventTypes)0;
            if ((edge & GHIElectronics.TinyCLR.Devices.Gpio.GpioPinEdge.RisingEdge) != 0) types |= PinEventTypes.Rising;
            if ((edge & GHIElectronics.TinyCLR.Devices.Gpio.GpioPinEdge.FallingEdge) != 0) types |= PinEventTypes.Falling;
            return types;
        }
    }

    public sealed class GpioController : IDisposable {
        private readonly Hashtable openedPins;

        public GpioDriver Driver { get; }
        public PinNumberingScheme NumberingScheme { get; }

        public int PinCount => this.Driver.PinCount;

        public GpioController() : this(PinNumberingScheme.Logical, new TinyClrGpioDriver()) {
        }

        public GpioController(PinNumberingScheme numberingScheme) : this(numberingScheme, new TinyClrGpioDriver()) {
        }

        public GpioController(PinNumberingScheme numberingScheme, GpioDriver driver) {
            this.NumberingScheme = numberingScheme;
            this.Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            this.openedPins = new Hashtable();
        }

        public void Dispose() {
            lock (this.openedPins) {
                foreach (DictionaryEntry entry in this.openedPins)
                    this.Driver.ClosePin((int)entry.Key);

                this.openedPins.Clear();
            }

            this.Driver.Dispose();
        }

        public void OpenPin(int pinNumber) => this.OpenPin(pinNumber, PinMode.Input);

        public void OpenPin(int pinNumber, PinMode mode) {
            this.OpenPin(pinNumber, mode, PinValue.Low);
        }

        public void OpenPin(int pinNumber, PinMode mode, PinValue initialValue) {
            lock (this.openedPins) {
                if (this.openedPins.Contains(pinNumber))
                    return;

                this.Driver.OpenPin(pinNumber);
                this.openedPins[pinNumber] = pinNumber;
            }

            this.SetPinMode(pinNumber, mode);

            if (mode == PinMode.Output || mode == PinMode.OutputOpenDrain)
                this.Write(pinNumber, initialValue);
        }

        public bool IsPinOpen(int pinNumber) {
            lock (this.openedPins)
                return this.openedPins.Contains(pinNumber);
        }

        public void ClosePin(int pinNumber) {
            lock (this.openedPins) {
                if (!this.openedPins.Contains(pinNumber))
                    return;

                this.Driver.ClosePin(pinNumber);
                this.openedPins.Remove(pinNumber);
            }
        }

        public PinMode GetPinMode(int pinNumber) => this.Driver.GetPinMode(pinNumber);
        public void SetPinMode(int pinNumber, PinMode mode) => this.Driver.SetPinMode(pinNumber, mode);
        public bool IsPinModeSupported(int pinNumber, PinMode mode) => this.Driver.IsPinModeSupported(pinNumber, mode);

        public PinValue Read(int pinNumber) => this.Driver.Read(pinNumber);
        public void Write(int pinNumber, PinValue value) => this.Driver.Write(pinNumber, value);

        public void RegisterCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback) =>
            this.Driver.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);

        public void UnregisterCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback) =>
            this.Driver.RemoveCallbackForPinValueChangedEvent(pinNumber, callback);
    }
}

namespace System.Device.Gpio.Drivers {
    public sealed class LibGpiodDriver : System.Device.Gpio.TinyClrGpioDriver {
        public int ChipNumber { get; }

        public LibGpiodDriver(int chipNumber) : base(CalculatePinBase(chipNumber)) {
            this.ChipNumber = chipNumber;
        }

        private static int CalculatePinBase(int chipNumber) {
            if (chipNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(chipNumber));

            if (chipNumber > int.MaxValue / 16)
                throw new ArgumentOutOfRangeException(nameof(chipNumber));

            return chipNumber * 16;
        }
    }
}
