using System;
using System.Collections;
using System.Threading;

namespace System.Device.Gpio {
    /// <summary>Edge selector for <see cref="GpioController"/> change notifications. Same shape as .NET IoT.</summary>
    [Flags]
    public enum PinEventTypes {
        /// <summary>No edge — returned by <see cref="GpioController.WaitForEvent(int, PinEventTypes, TimeSpan)"/> when it times out or is cancelled.</summary>
        None = 0,
        /// <summary>Rising edge (low → high).</summary>
        Rising = 1,
        /// <summary>Falling edge (high → low).</summary>
        Falling = 2,
    }

    /// <summary>Drive mode applied to a GPIO pin. Mirrors .NET IoT's <c>System.Device.Gpio.PinMode</c>.</summary>
    public enum PinMode {
        /// <summary>High-impedance input.</summary>
        Input = 0,
        /// <summary>Push-pull output.</summary>
        Output = 1,
        /// <summary>Input with internal pull-up.</summary>
        InputPullUp = 2,
        /// <summary>Input with internal pull-down.</summary>
        InputPullDown = 3,
        /// <summary>Open-drain output.</summary>
        OutputOpenDrain = 4,
    }

    /// <summary>Logical pin level.</summary>
    public enum PinValue {
        /// <summary>Low (0 V).</summary>
        Low = 0,
        /// <summary>High (Vcc).</summary>
        High = 1,
    }

    /// <summary>How <see cref="GpioController"/> interprets pin numbers — TinyCLR uses <see cref="Logical"/>.</summary>
    public enum PinNumberingScheme {
        /// <summary>Driver-relative logical pin index.</summary>
        Logical = 0,
        /// <summary>Physical board header pin number.</summary>
        Board = 1,
        /// <summary>Broadcom SoC pin number (Raspberry Pi convention).</summary>
        Bcm = 2,
    }

    /// <summary>Arguments for the .NET IoT pin-change callback.</summary>
    public sealed class PinValueChangedEventArgs : EventArgs {
        /// <summary>The edge that occurred.</summary>
        public PinEventTypes ChangeType { get; }
        /// <summary>The pin that changed.</summary>
        public int PinNumber { get; }

        /// <summary>Creates the event arguments.</summary>
        public PinValueChangedEventArgs(PinEventTypes changeType, int pinNumber) {
            this.ChangeType = changeType;
            this.PinNumber = pinNumber;
        }
    }

    /// <summary>Callback signature for .NET IoT pin-change notifications.</summary>
    public delegate void PinChangeEventHandler(object sender, PinValueChangedEventArgs pinValueChangedEventArgs);

    /// <summary>Abstract GPIO driver per .NET IoT. Implemented by <see cref="TinyClrGpioDriver"/> for TinyCLR hardware.</summary>
    public abstract class GpioDriver : IDisposable {
        /// <summary>Number of pins the driver exposes.</summary>
        public abstract int PinCount { get; }

        /// <summary>Opens the pin for use.</summary>
        protected internal abstract void OpenPin(int pinNumber);
        /// <summary>Closes the pin.</summary>
        protected internal abstract void ClosePin(int pinNumber);
        /// <summary>Returns true if the pin supports the given mode.</summary>
        protected internal abstract bool IsPinModeSupported(int pinNumber, PinMode mode);
        /// <summary>Sets the pin's drive mode.</summary>
        protected internal abstract void SetPinMode(int pinNumber, PinMode mode);
        /// <summary>Gets the pin's drive mode.</summary>
        protected internal abstract PinMode GetPinMode(int pinNumber);
        /// <summary>Reads the pin level.</summary>
        protected internal abstract PinValue Read(int pinNumber);
        /// <summary>Writes the pin level.</summary>
        protected internal abstract void Write(int pinNumber, PinValue value);
        /// <summary>Registers a callback for edge changes on the pin.</summary>
        protected internal abstract void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback);
        /// <summary>Removes a previously registered callback.</summary>
        protected internal abstract void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback);

        /// <summary>Releases the driver and closes all pins.</summary>
        public abstract void Dispose();
    }

    /// <summary>TinyCLR-backed implementation of <see cref="GpioDriver"/>. Routes <see cref="GpioController"/> calls to <see cref="GHIElectronics.TinyCLR.Devices.Gpio.GpioController"/>.</summary>
    public class TinyClrGpioDriver : GpioDriver {
        private readonly GHIElectronics.TinyCLR.Devices.Gpio.GpioController controller;
        private readonly Hashtable pinToTinyClrPin;
        private readonly Hashtable callbackMap;
        private readonly int pinBase;

        private sealed class CallbackRegistration {
            public GHIElectronics.TinyCLR.Devices.Gpio.GpioPinValueChangedEventHandler Handler;
        }

        /// <summary>Creates a driver over the default GPIO controller.</summary>
        public TinyClrGpioDriver() : this(GHIElectronics.TinyCLR.Devices.Gpio.GpioController.GetDefault(), 0) {
        }

        /// <summary>Creates a driver over the default controller with a pin-number offset.</summary>
        public TinyClrGpioDriver(int pinBase) : this(GHIElectronics.TinyCLR.Devices.Gpio.GpioController.GetDefault(), pinBase) {
        }

        /// <summary>Creates a driver over the given controller with an optional pin-number offset.</summary>
        public TinyClrGpioDriver(GHIElectronics.TinyCLR.Devices.Gpio.GpioController tinyClrController, int pinBase = 0) {
            this.controller = tinyClrController ?? throw new ArgumentNullException(nameof(tinyClrController));
            this.pinToTinyClrPin = new Hashtable();
            this.callbackMap = new Hashtable();
            this.pinBase = pinBase;
        }

        /// <summary>Number of pins on the controller.</summary>
        public override int PinCount => this.controller.Provider.PinCount;

        /// <summary>Closes all open pins and releases the controller.</summary>
        public override void Dispose() {
            lock (this.pinToTinyClrPin) {
                foreach (DictionaryEntry entry in this.pinToTinyClrPin)
                    ((GHIElectronics.TinyCLR.Devices.Gpio.GpioPin)entry.Value).Dispose();

                this.pinToTinyClrPin.Clear();
            }

            this.callbackMap.Clear();
            this.controller.Dispose();
        }

        /// <summary>Opens the pin on the underlying controller.</summary>
        protected internal override void OpenPin(int pinNumber) {
            var mappedPin = this.MapPinNumber(pinNumber);

            lock (this.pinToTinyClrPin) {
                if (this.pinToTinyClrPin.Contains(pinNumber))
                    return;

                this.pinToTinyClrPin[pinNumber] = this.controller.OpenPin(mappedPin);
            }
        }

        /// <summary>Closes the pin on the underlying controller.</summary>
        protected internal override void ClosePin(int pinNumber) {
            lock (this.pinToTinyClrPin) {
                if (!this.pinToTinyClrPin.Contains(pinNumber))
                    return;

                var tinyPin = (GHIElectronics.TinyCLR.Devices.Gpio.GpioPin)this.pinToTinyClrPin[pinNumber];
                tinyPin.Dispose();
                this.pinToTinyClrPin.Remove(pinNumber);
            }
        }

        /// <summary>Returns true if the pin supports the given mode.</summary>
        protected internal override bool IsPinModeSupported(int pinNumber, PinMode mode) =>
            this.GetTinyClrPin(pinNumber).IsDriveModeSupported(ToTinyClrDriveMode(mode));

        /// <summary>Sets the pin's drive mode.</summary>
        protected internal override void SetPinMode(int pinNumber, PinMode mode) =>
            this.GetTinyClrPin(pinNumber).SetDriveMode(ToTinyClrDriveMode(mode));

        /// <summary>Gets the pin's drive mode.</summary>
        protected internal override PinMode GetPinMode(int pinNumber) => ToPinMode(this.GetTinyClrPin(pinNumber).GetDriveMode());

        /// <summary>Reads the pin level.</summary>
        protected internal override PinValue Read(int pinNumber) =>
            this.GetTinyClrPin(pinNumber).Read() == GHIElectronics.TinyCLR.Devices.Gpio.GpioPinValue.High ? PinValue.High : PinValue.Low;

        /// <summary>Writes the pin level.</summary>
        protected internal override void Write(int pinNumber, PinValue value) =>
            this.GetTinyClrPin(pinNumber).Write(value == PinValue.High ? GHIElectronics.TinyCLR.Devices.Gpio.GpioPinValue.High : GHIElectronics.TinyCLR.Devices.Gpio.GpioPinValue.Low);

        /// <summary>Registers a callback for edge changes on the pin.</summary>
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

        /// <summary>Removes a previously registered callback.</summary>
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

        /// <summary>Maps a controller pin number to the underlying hardware pin, applying the pin-number offset.</summary>
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

    /// <summary>Result of a <see cref="GpioController.WaitForEvent(int, PinEventTypes, TimeSpan)"/> call. Same shape as .NET IoT.</summary>
    public struct WaitForEventResult {
        /// <summary>The edge that occurred, or <see cref="PinEventTypes.None"/> if the wait timed out or was cancelled.</summary>
        public PinEventTypes EventTypes;
        /// <summary>True if the wait elapsed (or was cancelled) before the event occurred.</summary>
        public bool TimedOut;
    }

    /// <summary>
    /// .NET IoT-style GPIO controller. Same surface as <c>System.Device.Gpio.GpioController</c>;
    /// internally routes through TinyCLR's GPIO HAL via <see cref="TinyClrGpioDriver"/>.
    /// </summary>
    public sealed class GpioController : IDisposable {
        private readonly Hashtable openedPins;

        /// <summary>The underlying driver.</summary>
        public GpioDriver Driver { get; }
        /// <summary>How pin numbers are interpreted.</summary>
        public PinNumberingScheme NumberingScheme { get; }

        /// <summary>Number of pins on the controller.</summary>
        public int PinCount => this.Driver.PinCount;

        /// <summary>Creates a controller over the default TinyCLR GPIO driver.</summary>
        public GpioController() : this(PinNumberingScheme.Logical, new TinyClrGpioDriver()) {
        }

        /// <summary>Creates a controller with the given numbering scheme.</summary>
        public GpioController(PinNumberingScheme numberingScheme) : this(numberingScheme, new TinyClrGpioDriver()) {
        }

        /// <summary>Creates a controller with the given numbering scheme and driver.</summary>
        public GpioController(PinNumberingScheme numberingScheme, GpioDriver driver) {
            this.NumberingScheme = numberingScheme;
            this.Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            this.openedPins = new Hashtable();
        }

        /// <summary>Closes all open pins and releases the driver.</summary>
        public void Dispose() {
            lock (this.openedPins) {
                foreach (DictionaryEntry entry in this.openedPins)
                    this.Driver.ClosePin((int)entry.Key);

                this.openedPins.Clear();
            }

            this.Driver.Dispose();
        }

        /// <summary>Opens the pin as an input.</summary>
        public void OpenPin(int pinNumber) => this.OpenPin(pinNumber, PinMode.Input);

        /// <summary>Opens the pin in the given mode.</summary>
        public void OpenPin(int pinNumber, PinMode mode) {
            this.OpenPin(pinNumber, mode, PinValue.Low);
        }

        /// <summary>Opens the pin in the given mode and, for outputs, sets its initial level.</summary>
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

        /// <summary>Returns true if the pin is open.</summary>
        public bool IsPinOpen(int pinNumber) {
            lock (this.openedPins)
                return this.openedPins.Contains(pinNumber);
        }

        /// <summary>Closes the pin.</summary>
        public void ClosePin(int pinNumber) {
            lock (this.openedPins) {
                if (!this.openedPins.Contains(pinNumber))
                    return;

                this.Driver.ClosePin(pinNumber);
                this.openedPins.Remove(pinNumber);
            }
        }

        /// <summary>Gets the pin's drive mode.</summary>
        public PinMode GetPinMode(int pinNumber) => this.Driver.GetPinMode(pinNumber);
        /// <summary>Sets the pin's drive mode.</summary>
        public void SetPinMode(int pinNumber, PinMode mode) => this.Driver.SetPinMode(pinNumber, mode);
        /// <summary>Returns true if the pin supports the given mode.</summary>
        public bool IsPinModeSupported(int pinNumber, PinMode mode) => this.Driver.IsPinModeSupported(pinNumber, mode);

        /// <summary>Reads the pin level.</summary>
        public PinValue Read(int pinNumber) => this.Driver.Read(pinNumber);
        /// <summary>Writes the pin level.</summary>
        public void Write(int pinNumber, PinValue value) => this.Driver.Write(pinNumber, value);

        /// <summary>Registers a callback for edge changes on the pin.</summary>
        public void RegisterCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback) =>
            this.Driver.AddCallbackForPinValueChangedEvent(pinNumber, eventTypes, callback);

        /// <summary>Removes a previously registered callback.</summary>
        public void UnregisterCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback) =>
            this.Driver.RemoveCallbackForPinValueChangedEvent(pinNumber, callback);

        /// <summary>
        /// Blocks until an edge of type <paramref name="eventTypes"/> occurs on <paramref name="pinNumber"/>
        /// or <paramref name="timeout"/> elapses. The pin must already be open. Pass
        /// <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
        /// </summary>
        public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, TimeSpan timeout) {
            var totalMs = (long)timeout.TotalMilliseconds;
            var millisecondsTimeout = totalMs < Timeout.Infinite ? Timeout.Infinite
                : (totalMs > int.MaxValue ? int.MaxValue : (int)totalMs);

            return this.WaitForEventCore(pinNumber, eventTypes, millisecondsTimeout, CancellationToken.None);
        }

        /// <summary>
        /// Blocks until an edge of type <paramref name="eventTypes"/> occurs on <paramref name="pinNumber"/>
        /// or <paramref name="cancellationToken"/> is cancelled. The pin must already be open. On
        /// cancellation the result has <see cref="WaitForEventResult.TimedOut"/> set to true.
        /// </summary>
        public WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken) =>
            this.WaitForEventCore(pinNumber, eventTypes, Timeout.Infinite, cancellationToken);

        private WaitForEventResult WaitForEventCore(int pinNumber, PinEventTypes eventTypes, int millisecondsTimeout, CancellationToken cancellationToken) {
            var signal = new AutoResetEvent(false);
            var captured = PinEventTypes.None;

            // AutoResetEvent latches a Set that arrives before WaitOne, so an edge that fires
            // between registering and waiting is not lost.
            PinChangeEventHandler handler = (s, e) => {
                captured = e.ChangeType;
                signal.Set();
            };

            this.RegisterCallbackForPinValueChangedEvent(pinNumber, eventTypes, handler);

            try {
                // TinyCLR's CancellationToken has no callback registration (poll-only), so when a
                // cancellable token is supplied we wait in short chunks and re-check it, instead of
                // blocking for the whole timeout. With an uncancellable token we just block once.
                if (!cancellationToken.CanBeCanceled) {
                    return signal.WaitOne(millisecondsTimeout, false)
                        ? new WaitForEventResult { EventTypes = captured, TimedOut = false }
                        : new WaitForEventResult { EventTypes = PinEventTypes.None, TimedOut = true };
                }

                const int PollMilliseconds = 50;
                var remaining = (long)millisecondsTimeout;   // negative == infinite

                while (true) {
                    if (cancellationToken.IsCancellationRequested)
                        return new WaitForEventResult { EventTypes = PinEventTypes.None, TimedOut = true };

                    var chunk = PollMilliseconds;
                    if (remaining >= 0) {
                        if (remaining == 0)
                            return new WaitForEventResult { EventTypes = PinEventTypes.None, TimedOut = true };
                        if (remaining < chunk)
                            chunk = (int)remaining;
                    }

                    if (signal.WaitOne(chunk, false))
                        return new WaitForEventResult { EventTypes = captured, TimedOut = false };

                    if (remaining >= 0)
                        remaining -= chunk;
                }
            }
            finally {
                this.UnregisterCallbackForPinValueChangedEvent(pinNumber, handler);
            }
        }
    }
}

namespace System.Device.Gpio.Drivers {
    /// <summary>Alias of <see cref="TinyClrGpioDriver"/> for source-compatibility with Linux .NET IoT samples that reference <c>LibGpiodDriver</c>.</summary>
    public sealed class LibGpiodDriver : System.Device.Gpio.TinyClrGpioDriver {
        /// <summary>The GPIO chip number this driver maps onto.</summary>
        public int ChipNumber { get; }

        /// <summary>Creates a driver for the given GPIO chip number.</summary>
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
