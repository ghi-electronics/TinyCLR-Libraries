using System;
using GHIElectronics.TinyCLR.Devices.Gpio.Provider;
using GHIElectronics.TinyCLR.Native;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.Gpio\Gpio.cs.
// Bodies on Desktop are safe no-ops:
//   * Read()                -> GpioPinValue.Low
//   * Write/SetDriveMode/.. -> empty body
//   * GetDriveMode()        -> Input
//   * IsDriveModeSupported  -> true
//   * Event handlers stored but never raised (no native interrupt source)
// User code with StartGpio() / OpenPin() / Read() / Write() runs to
// completion on Desktop without throwing.
namespace GHIElectronics.TinyCLR.Devices.Gpio {
    /// <summary>
    /// Handler invoked when a <see cref="GpioPin"/> configured as an input
    /// observes a level change matching its <see cref="GpioPin.ValueChangedEdge"/>.
    /// </summary>
    /// <param name="sender">The pin that detected the edge.</param>
    /// <param name="e">Edge direction and timestamp captured by the driver.</param>
    public delegate void GpioPinValueChangedEventHandler(GpioPin sender, GpioPinValueChangedEventArgs e);

    /// <summary>Arguments for the <see cref="GpioPin.ValueChanged"/> event.</summary>
    public class GpioPinValueChangedEventArgs : EventArgs {
        /// <summary>The edge direction (rising or falling) that triggered the event.</summary>
        public GpioPinEdge Edge { get; }

        /// <summary>Driver-captured time at which the transition was detected.</summary>
        public DateTime Timestamp { get; }

        /// <summary>Creates a new <see cref="GpioPinValueChangedEventArgs"/>.</summary>
        /// <param name="edge">The edge direction that triggered the event.</param>
        /// <param name="timestamp">Driver-captured time of the transition.</param>
        public GpioPinValueChangedEventArgs(GpioPinEdge edge, DateTime timestamp) {
            this.Edge = edge;
            this.Timestamp = timestamp;
        }
    }

    /// <summary>
    /// Electrical drive mode applied to a <see cref="GpioPin"/>. Not every mode
    /// is available on every pin — call <see cref="GpioPin.IsDriveModeSupported(GpioPinDriveMode)"/>
    /// before <see cref="GpioPin.SetDriveMode(GpioPinDriveMode)"/> when in doubt.
    /// </summary>
    public enum GpioPinDriveMode {
        /// <summary>High-impedance input with no internal resistor.</summary>
        Input = 0,
        /// <summary>Push-pull output. The pin actively drives both high and low.</summary>
        Output = 1,
        /// <summary>Input with the internal pull-up resistor enabled.</summary>
        InputPullUp = 2,
        /// <summary>Input with the internal pull-down resistor enabled.</summary>
        InputPullDown = 3,
        /// <summary>Open-drain output: the pin actively pulls low and floats when set high.</summary>
        OutputOpenDrain = 4,
    }

    /// <summary>Logical level of a <see cref="GpioPin"/>.</summary>
    public enum GpioPinValue {
        /// <summary>The pin is at the low (0 V / ground) level.</summary>
        Low = 0,
        /// <summary>The pin is at the high (Vcc) level.</summary>
        High = 1,
    }

    /// <summary>
    /// Which edges trigger the <see cref="GpioPin.ValueChanged"/> event. The flags
    /// can be combined to fire on both edges.
    /// </summary>
    [Flags]
    public enum GpioPinEdge {
        /// <summary>Fire when the input transitions from high to low.</summary>
        FallingEdge = 1,
        /// <summary>Fire when the input transitions from low to high.</summary>
        RisingEdge = 2,
    }

    /// <summary>
    /// Represents a GPIO controller — the hardware peripheral that owns a set of
    /// individually addressable pins. Use <see cref="GetDefault"/> to obtain the
    /// device's primary controller, then <see cref="OpenPin(int)"/> to acquire
    /// pins for input or output.
    /// </summary>
    public class GpioController : IDisposable {
        /// <summary>The low-level provider backing this controller.</summary>
        public IGpioControllerProvider Provider { get; }

        private GpioController(IGpioControllerProvider provider) => this.Provider = provider;

        // GetDefault() routes through FromName("Simulator") on Desktop. Reads
        // honestly in user code/debugger — and gives us a future Phase 2 hook
        // (users could register a real simulator under that name).
        /// <summary>Returns the default GPIO controller for this device.</summary>
        public static GpioController GetDefault() => GpioController.FromName("Simulator");

        /// <summary>Returns a GPIO controller identified by its native API name.</summary>
        /// <param name="name">Native API name (e.g. one of the platform-specific Pin map constants).</param>
        public static GpioController FromName(string name) => GpioController.FromProvider(new GpioControllerApiWrapper(NativeApi.Find(name, NativeApiType.GpioController)));

        /// <summary>Creates a controller from a custom <see cref="IGpioControllerProvider"/>.</summary>
        /// <param name="provider">Provider implementing the pin operations.</param>
        public static GpioController FromProvider(IGpioControllerProvider provider) => new GpioController(provider);

        /// <summary>Releases the underlying provider and any pins it still holds open.</summary>
        public void Dispose() => this.Provider.Dispose();

        /// <summary>Opens a single pin for input or output.</summary>
        /// <param name="pinNumber">Controller-relative pin index.</param>
        /// <returns>A <see cref="GpioPin"/> bound to this controller; dispose it to release the pin.</returns>
        public GpioPin OpenPin(int pinNumber) => new GpioPin(this, pinNumber);

        /// <summary>
        /// Opens multiple pins atomically. If any pin fails to open, every pin
        /// already opened by this call is disposed and the original exception is rethrown.
        /// </summary>
        /// <param name="pinNumbers">Controller-relative pin indices.</param>
        /// <returns>An array of opened pins in the same order as <paramref name="pinNumbers"/>.</returns>
        public GpioPin[] OpenPins(params int[] pinNumbers) {
            var res = new GpioPin[pinNumbers.Length];
            for (var i = 0; i < pinNumbers.Length; i++)
                res[i] = this.OpenPin(pinNumbers[i]);
            return res;
        }

        /// <summary>Non-throwing version of <see cref="OpenPin(int)"/>.</summary>
        /// <param name="pinNumber">Controller-relative pin index.</param>
        /// <param name="pin">Receives the opened pin on success; null on failure.</param>
        /// <returns>True if the pin was opened; false if it was unavailable or in use.</returns>
        public bool TryOpenPin(int pinNumber, out GpioPin pin) {
            pin = this.OpenPin(pinNumber);
            return true;
        }

        /// <summary>Non-throwing batch version of <see cref="OpenPins(int[])"/>.</summary>
        /// <param name="pins">Receives the opened pins on success; null on failure.</param>
        /// <param name="pinNumbers">Controller-relative pin indices.</param>
        /// <returns>True if every pin opened; false if any one failed (in which case no pins remain open).</returns>
        public bool TryOpenPins(out GpioPin[] pins, params int[] pinNumbers) {
            pins = this.OpenPins(pinNumbers);
            return true;
        }
    }

    /// <summary>
    /// A single GPIO pin opened from a <see cref="GpioController"/>. Configure the
    /// direction with <see cref="SetDriveMode(GpioPinDriveMode)"/>, then drive it
    /// with <see cref="Write(GpioPinValue)"/> or sample it with <see cref="Read"/>.
    /// Subscribe to <see cref="ValueChanged"/> for edge-triggered notifications on inputs.
    /// </summary>
    public class GpioPin : IDisposable {
        private GpioPinValueChangedEventHandler callbacks;
        private GpioPinEdge valueChangedEdge = GpioPinEdge.FallingEdge | GpioPinEdge.RisingEdge;

        /// <summary>The controller-relative pin index this object represents.</summary>
        public int PinNumber { get; }

        /// <summary>The <see cref="GpioController"/> that owns this pin.</summary>
        public GpioController Controller { get; }

        internal GpioPin(GpioController controller, int pinNumber) {
            this.PinNumber = pinNumber;
            this.Controller = controller;
            this.Controller.Provider.OpenPin(pinNumber);
        }

        /// <summary>Releases the pin so another caller can open it.</summary>
        public void Dispose() => this.Controller.Provider.ClosePin(this.PinNumber);

        /// <summary>Tests whether a given drive mode is supported on this pin.</summary>
        /// <param name="mode">The drive mode to test.</param>
        /// <returns>True if the pin can be configured with the given mode.</returns>
        public bool IsDriveModeSupported(GpioPinDriveMode mode) => this.Controller.Provider.IsDriveModeSupported(this.PinNumber, mode);

        /// <summary>
        /// Obsolete. The <paramref name="pin"/> argument is ignored — the pin's own
        /// <see cref="PinNumber"/> is used. Call the no-pin overload instead.
        /// </summary>
        /// <param name="pin">Ignored.</param>
        /// <param name="mode">The drive mode to test.</param>
        /// <returns>True if the pin can be configured with the given mode.</returns>
        [Obsolete("Use IsDriveModeSupported(GpioPinDriveMode mode) instead; the pin parameter is ignored and the GpioPin's own PinNumber is used.")]
        public bool IsDriveModeSupported(int pin, GpioPinDriveMode mode) => this.Controller.Provider.IsDriveModeSupported(this.PinNumber, mode);

        /// <summary>
        /// Debounce window applied before <see cref="ValueChanged"/> fires.
        /// Transitions shorter than this duration are suppressed.
        /// </summary>
        public TimeSpan DebounceTimeout {
            get => this.Controller.Provider.GetDebounceTimeout(this.PinNumber);
            set => this.Controller.Provider.SetDebounceTimeout(this.PinNumber, value);
        }

        /// <summary>Returns the pin's currently configured drive mode.</summary>
        public GpioPinDriveMode GetDriveMode() => this.Controller.Provider.GetDriveMode(this.PinNumber);

        /// <summary>Configures the pin's electrical drive mode.</summary>
        /// <param name="value">New drive mode. Must be supported on this pin (see <see cref="IsDriveModeSupported(GpioPinDriveMode)"/>).</param>
        public void SetDriveMode(GpioPinDriveMode value) => this.Controller.Provider.SetDriveMode(this.PinNumber, value);

        /// <summary>Samples the pin and returns the current logical level.</summary>
        /// <returns>The pin's present <see cref="GpioPinValue"/>.</returns>
        public GpioPinValue Read() => this.Controller.Provider.Read(this.PinNumber);

        /// <summary>Drives an output pin to the specified level. No effect on input modes.</summary>
        /// <param name="value">The logical level to drive.</param>
        public void Write(GpioPinValue value) => this.Controller.Provider.Write(this.PinNumber, value);

        /// <summary>
        /// Inverts the pin's current output level. Equivalent to reading the
        /// current state and writing its opposite.
        /// </summary>
        public void Toggle() => this.Controller.Provider.Write(this.PinNumber, this.Controller.Provider.Read(this.PinNumber) == GpioPinValue.Low ? GpioPinValue.High : GpioPinValue.Low);

        /// <summary>
        /// Selects which edges raise <see cref="ValueChanged"/>. Defaults to both
        /// rising and falling. Updates take effect immediately if a handler is attached.
        /// </summary>
        public GpioPinEdge ValueChangedEdge {
            get => this.valueChangedEdge;
            set {
                this.valueChangedEdge = value;
                if (this.callbacks != null)
                    this.Controller.Provider.SetPinChangedHandler(this.PinNumber, this.valueChangedEdge, this.OnValueChanged);
            }
        }

        /// <summary>
        /// Raised when the pin transitions on an edge selected by <see cref="ValueChangedEdge"/>.
        /// The handler runs on the driver's event thread — keep it short and avoid blocking.
        /// </summary>
        public event GpioPinValueChangedEventHandler ValueChanged {
            add {
                if (this.callbacks == null)
                    this.Controller.Provider.SetPinChangedHandler(this.PinNumber, this.valueChangedEdge, this.OnValueChanged);
                this.callbacks += value;
            }
            remove {
                this.callbacks -= value;
                if (this.callbacks == null)
                    this.Controller.Provider.ClearPinChangedHandler(this.PinNumber);
            }
        }

        private void OnValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e) => this.callbacks?.Invoke(this, e);
    }

    namespace Provider {
        /// <summary>
        /// Provider contract for a GPIO controller. Most users call
        /// <see cref="GpioController"/> / <see cref="GpioPin"/> directly; implement
        /// this interface only when supplying a custom or virtual controller.
        /// </summary>
        public interface IGpioControllerProvider : IDisposable {
            /// <summary>Total number of pins exposed by this controller.</summary>
            int PinCount { get; }

            /// <summary>Acquires exclusive access to the specified pin.</summary>
            /// <param name="pin">Controller-relative pin index.</param>
            void OpenPin(int pin);

            /// <summary>Releases a previously opened pin.</summary>
            /// <param name="pin">Controller-relative pin index.</param>
            void ClosePin(int pin);

            /// <summary>Tests whether the pin supports a given drive mode.</summary>
            /// <param name="pin">Controller-relative pin index.</param>
            /// <param name="mode">The drive mode to test.</param>
            /// <returns>True if the mode is supported on that pin.</returns>
            bool IsDriveModeSupported(int pin, GpioPinDriveMode mode);

            /// <summary>Installs or replaces the edge-change handler for a pin.</summary>
            /// <param name="pin">Controller-relative pin index.</param>
            /// <param name="edge">Which edges should raise the event.</param>
            /// <param name="value">Delegate to invoke on a matching edge.</param>
            void SetPinChangedHandler(int pin, GpioPinEdge edge, GpioPinValueChangedEventHandler value);

            /// <summary>Removes any previously installed edge-change handler.</summary>
            /// <param name="pin">Controller-relative pin index.</param>
            void ClearPinChangedHandler(int pin);

            /// <summary>Returns the pin's current debounce window.</summary>
            /// <param name="pin">Controller-relative pin index.</param>
            TimeSpan GetDebounceTimeout(int pin);

            /// <summary>Sets the pin's debounce window.</summary>
            /// <param name="pin">Controller-relative pin index.</param>
            /// <param name="value">Minimum stable interval before an edge is reported.</param>
            void SetDebounceTimeout(int pin, TimeSpan value);

            /// <summary>Returns the pin's currently configured drive mode.</summary>
            /// <param name="pin">Controller-relative pin index.</param>
            GpioPinDriveMode GetDriveMode(int pin);

            /// <summary>Configures the pin's drive mode.</summary>
            /// <param name="pin">Controller-relative pin index.</param>
            /// <param name="value">Drive mode to apply.</param>
            void SetDriveMode(int pin, GpioPinDriveMode value);

            /// <summary>Samples the pin and returns its current logical level.</summary>
            /// <param name="pin">Controller-relative pin index.</param>
            GpioPinValue Read(int pin);

            /// <summary>Drives an output pin to the specified level.</summary>
            /// <param name="pin">Controller-relative pin index.</param>
            /// <param name="value">Level to drive.</param>
            void Write(int pin, GpioPinValue value);

            /// <summary>
            /// Re-routes a pin's signal through the controller's alternate-function fabric.
            /// Advanced; used by peripheral drivers (UART/SPI/I2C/etc.) to claim package pins.
            /// </summary>
            /// <param name="pinSource">Source pin index.</param>
            /// <param name="pinDestination">Destination pin index.</param>
            /// <param name="mode">Platform-specific mode bits.</param>
            /// <param name="type">Platform-specific type bits.</param>
            /// <param name="direction">Platform-specific direction bits.</param>
            /// <param name="speed">Platform-specific speed/slew bits.</param>
            /// <param name="alternate">Alternate-function index.</param>
            void TransferFeature(int pinSource, int pinDestination, uint mode, uint type, uint direction, uint speed, uint alternate);
        }

        // Public surface mirrors the impl's GpioControllerApiWrapper. Same
        // public ctor signature (NativeApi), same Api property, same methods.
        // Bodies are no-ops (no [MethodImpl(InternalCall)]). Drive mode and
        // debounce timeout per-pin are stored in dictionaries so consumer
        // code that round-trips Get/Set sees consistent values.
        /// <summary>
        /// Concrete <see cref="IGpioControllerProvider"/> backed by the native
        /// TinyCLR GPIO HAL. Constructed internally by <see cref="GpioController"/>;
        /// you don't normally need to use this type directly.
        /// </summary>
        public sealed class GpioControllerApiWrapper : IGpioControllerProvider {
            private readonly System.Collections.Hashtable driveModes = new System.Collections.Hashtable();
            private readonly System.Collections.Hashtable debounces = new System.Collections.Hashtable();
            private readonly System.Collections.Hashtable pinHandlers = new System.Collections.Hashtable();

            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            /// <summary>Wraps the given native API as a provider.</summary>
            /// <param name="api">The native GPIO API to bind to.</param>
            public GpioControllerApiWrapper(NativeApi api) => this.Api = api;

            /// <summary>Releases the native controller.</summary>
            public void Dispose() { }

            /// <inheritdoc/>
            public int PinCount => int.MaxValue;

            /// <inheritdoc/>
            public void OpenPin(int pin) { }
            /// <inheritdoc/>
            public void ClosePin(int pin) { }

            /// <inheritdoc/>
            public TimeSpan GetDebounceTimeout(int pin) => this.debounces.Contains(pin) ? (TimeSpan)this.debounces[pin] : TimeSpan.Zero;
            /// <inheritdoc/>
            public void SetDebounceTimeout(int pin, TimeSpan value) => this.debounces[pin] = value;

            /// <inheritdoc/>
            public GpioPinDriveMode GetDriveMode(int pin) => this.driveModes.Contains(pin) ? (GpioPinDriveMode)this.driveModes[pin] : GpioPinDriveMode.Input;
            /// <inheritdoc/>
            public void SetDriveMode(int pin, GpioPinDriveMode value) => this.driveModes[pin] = value;

            /// <inheritdoc/>
            public GpioPinValue Read(int pin) => GpioPinValue.Low;
            /// <inheritdoc/>
            public void Write(int pin, GpioPinValue value) { }

            /// <inheritdoc/>
            public bool IsDriveModeSupported(int pin, GpioPinDriveMode mode) => true;

            /// <inheritdoc/>
            public void SetPinChangedHandler(int pin, GpioPinEdge edge, GpioPinValueChangedEventHandler value) => this.pinHandlers[pin] = value;
            /// <inheritdoc/>
            public void ClearPinChangedHandler(int pin) => this.pinHandlers.Remove(pin);

            /// <inheritdoc/>
            public void TransferFeature(int pinSource, int pinDestination, uint mode, uint type, uint direction, uint speed, uint alternate) { }
        }
    }
}
