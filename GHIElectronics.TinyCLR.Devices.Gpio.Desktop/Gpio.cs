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
    public delegate void GpioPinValueChangedEventHandler(GpioPin sender, GpioPinValueChangedEventArgs e);

    public class GpioPinValueChangedEventArgs : EventArgs {
        public GpioPinEdge Edge { get; }
        public DateTime Timestamp { get; }

        public GpioPinValueChangedEventArgs(GpioPinEdge edge, DateTime timestamp) {
            this.Edge = edge;
            this.Timestamp = timestamp;
        }
    }

    public enum GpioPinDriveMode {
        Input = 0,
        Output = 1,
        InputPullUp = 2,
        InputPullDown = 3,
        OutputOpenDrain = 4,
    }

    public enum GpioPinValue {
        Low = 0,
        High = 1,
    }

    [Flags]
    public enum GpioPinEdge {
        FallingEdge = 1,
        RisingEdge = 2,
    }

    public class GpioController : IDisposable {
        public IGpioControllerProvider Provider { get; }

        private GpioController(IGpioControllerProvider provider) => this.Provider = provider;

        // GetDefault() routes through FromName("Simulator") on Desktop. Reads
        // honestly in user code/debugger — and gives us a future Phase 2 hook
        // (users could register a real simulator under that name).
        public static GpioController GetDefault() => GpioController.FromName("Simulator");
        public static GpioController FromName(string name) => GpioController.FromProvider(new GpioControllerApiWrapper(NativeApi.Find(name, NativeApiType.GpioController)));
        public static GpioController FromProvider(IGpioControllerProvider provider) => new GpioController(provider);

        public void Dispose() => this.Provider.Dispose();

        public GpioPin OpenPin(int pinNumber) => new GpioPin(this, pinNumber);

        public GpioPin[] OpenPins(params int[] pinNumbers) {
            var res = new GpioPin[pinNumbers.Length];
            for (var i = 0; i < pinNumbers.Length; i++)
                res[i] = this.OpenPin(pinNumbers[i]);
            return res;
        }

        public bool TryOpenPin(int pinNumber, out GpioPin pin) {
            pin = this.OpenPin(pinNumber);
            return true;
        }

        public bool TryOpenPins(out GpioPin[] pins, params int[] pinNumbers) {
            pins = this.OpenPins(pinNumbers);
            return true;
        }
    }

    public class GpioPin : IDisposable {
        private GpioPinValueChangedEventHandler callbacks;
        private GpioPinEdge valueChangedEdge = GpioPinEdge.FallingEdge | GpioPinEdge.RisingEdge;

        public int PinNumber { get; }
        public GpioController Controller { get; }

        internal GpioPin(GpioController controller, int pinNumber) {
            this.PinNumber = pinNumber;
            this.Controller = controller;
            this.Controller.Provider.OpenPin(pinNumber);
        }

        public void Dispose() => this.Controller.Provider.ClosePin(this.PinNumber);

        public bool IsDriveModeSupported(GpioPinDriveMode mode) => this.Controller.Provider.IsDriveModeSupported(this.PinNumber, mode);

        [Obsolete("Use IsDriveModeSupported(GpioPinDriveMode mode) instead; the pin parameter is ignored and the GpioPin's own PinNumber is used.")]
        public bool IsDriveModeSupported(int pin, GpioPinDriveMode mode) => this.Controller.Provider.IsDriveModeSupported(this.PinNumber, mode);

        public TimeSpan DebounceTimeout {
            get => this.Controller.Provider.GetDebounceTimeout(this.PinNumber);
            set => this.Controller.Provider.SetDebounceTimeout(this.PinNumber, value);
        }

        public GpioPinDriveMode GetDriveMode() => this.Controller.Provider.GetDriveMode(this.PinNumber);
        public void SetDriveMode(GpioPinDriveMode value) => this.Controller.Provider.SetDriveMode(this.PinNumber, value);

        public GpioPinValue Read() => this.Controller.Provider.Read(this.PinNumber);
        public void Write(GpioPinValue value) => this.Controller.Provider.Write(this.PinNumber, value);
        public void Toggle() => this.Controller.Provider.Write(this.PinNumber, this.Controller.Provider.Read(this.PinNumber) == GpioPinValue.Low ? GpioPinValue.High : GpioPinValue.Low);

        public GpioPinEdge ValueChangedEdge {
            get => this.valueChangedEdge;
            set {
                this.valueChangedEdge = value;
                if (this.callbacks != null)
                    this.Controller.Provider.SetPinChangedHandler(this.PinNumber, this.valueChangedEdge, this.OnValueChanged);
            }
        }

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
        public interface IGpioControllerProvider : IDisposable {
            int PinCount { get; }

            void OpenPin(int pin);
            void ClosePin(int pin);

            bool IsDriveModeSupported(int pin, GpioPinDriveMode mode);
            void SetPinChangedHandler(int pin, GpioPinEdge edge, GpioPinValueChangedEventHandler value);
            void ClearPinChangedHandler(int pin);

            TimeSpan GetDebounceTimeout(int pin);
            void SetDebounceTimeout(int pin, TimeSpan value);
            GpioPinDriveMode GetDriveMode(int pin);
            void SetDriveMode(int pin, GpioPinDriveMode value);
            GpioPinValue Read(int pin);
            void Write(int pin, GpioPinValue value);
            void TransferFeature(int pinSource, int pinDestination, uint mode, uint type, uint direction, uint speed, uint alternate);
        }

        // Public surface mirrors the impl's GpioControllerApiWrapper. Same
        // public ctor signature (NativeApi), same Api property, same methods.
        // Bodies are no-ops (no [MethodImpl(InternalCall)]). Drive mode and
        // debounce timeout per-pin are stored in dictionaries so consumer
        // code that round-trips Get/Set sees consistent values.
        public sealed class GpioControllerApiWrapper : IGpioControllerProvider {
            private readonly System.Collections.Hashtable driveModes = new System.Collections.Hashtable();
            private readonly System.Collections.Hashtable debounces = new System.Collections.Hashtable();
            private readonly System.Collections.Hashtable pinHandlers = new System.Collections.Hashtable();

            public NativeApi Api { get; }

            public GpioControllerApiWrapper(NativeApi api) => this.Api = api;

            public void Dispose() { }

            public int PinCount => int.MaxValue;

            public void OpenPin(int pin) { }
            public void ClosePin(int pin) { }

            public TimeSpan GetDebounceTimeout(int pin) => this.debounces.Contains(pin) ? (TimeSpan)this.debounces[pin] : TimeSpan.Zero;
            public void SetDebounceTimeout(int pin, TimeSpan value) => this.debounces[pin] = value;

            public GpioPinDriveMode GetDriveMode(int pin) => this.driveModes.Contains(pin) ? (GpioPinDriveMode)this.driveModes[pin] : GpioPinDriveMode.Input;
            public void SetDriveMode(int pin, GpioPinDriveMode value) => this.driveModes[pin] = value;

            public GpioPinValue Read(int pin) => GpioPinValue.Low;
            public void Write(int pin, GpioPinValue value) { }

            public bool IsDriveModeSupported(int pin, GpioPinDriveMode mode) => true;

            public void SetPinChangedHandler(int pin, GpioPinEdge edge, GpioPinValueChangedEventHandler value) => this.pinHandlers[pin] = value;
            public void ClearPinChangedHandler(int pin) => this.pinHandlers.Remove(pin);

            public void TransferFeature(int pinSource, int pinDestination, uint mode, uint type, uint direction, uint speed, uint alternate) { }
        }
    }
}
