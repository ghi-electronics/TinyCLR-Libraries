using System;
using System.Collections;
using System.Runtime.CompilerServices;
using GHIElectronics.TinyCLR.Devices.Gpio.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Gpio {
    public delegate void GpioPinValueChangedEventHandler(GpioPin sender, GpioPinValueChangedEventArgs e);

    public sealed class GpioPinValueChangedEventArgs : EventArgs {
        public GpioPinEdge Edge { get; }
        public DateTime Timestamp { get; }

        internal GpioPinValueChangedEventArgs(GpioPinEdge edge, DateTime timestamp) {
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

    public sealed class GpioController : IDisposable {
        public IGpioControllerProvider Provider { get; }

        private GpioController(IGpioControllerProvider provider) => this.Provider = provider;

        public static GpioController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.GpioController) is GpioController c ? c : GpioController.FromName(NativeApi.GetDefaultName(NativeApiType.GpioController));
        public static GpioController FromName(string name) => GpioController.FromProvider(new GpioControllerApiWrapper(NativeApi.Find(name, NativeApiType.GpioController)));
        public static GpioController FromProvider(IGpioControllerProvider provider) => new GpioController(provider);

        public void Dispose() => this.Provider.Dispose();

        public GpioPin OpenPin(int pinNumber) => new GpioPin(this, pinNumber);

        public GpioPin[] OpenPins(params int[] pinNumbers) {
            var res = new GpioPin[pinNumbers.Length];
            var i = 0;

            for (; i < pinNumbers.Length; i++) {
                try {
                    res[i] = this.OpenPin(pinNumbers[i]);
                }
                catch {
                    for (var ii = 0; ii < i; ii++)
                        res[ii].Dispose();

                    throw;
                }
            }

            return res;
        }

        public bool TryOpenPin(int pinNumber, out GpioPin pin) {
            try {
                pin = this.OpenPin(pinNumber);
                return true;
            }
            catch {
                pin = null;
                return false;
            }
        }

        public bool TryOpenPins(out GpioPin[] pins, params int[] pinNumbers) {
            try {
                pins = this.OpenPins(pinNumbers);
                return true;
            }
            catch {
                pins = null;
                return false;
            }
        }
    }

    public sealed class GpioPin : IDisposable {
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

        public bool IsDriveModeSupported(int pin, GpioPinDriveMode mode) => this.Controller.Provider.IsDriveModeSupported(pin, mode);

        public TimeSpan DebounceTimeout { get => this.Controller.Provider.GetDebounceTimeout(this.PinNumber); set => this.Controller.Provider.SetDebounceTimeout(this.PinNumber, value); }

        public GpioPinDriveMode GetDriveMode() => this.Controller.Provider.GetDriveMode(this.PinNumber);
        public void SetDriveMode(GpioPinDriveMode value) => this.Controller.Provider.SetDriveMode(this.PinNumber, value);

        public GpioPinValue Read() => this.Controller.Provider.Read(this.PinNumber);

        public void Write(GpioPinValue value) => this.Controller.Provider.Write(this.PinNumber, value);

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
        }

        public sealed class GpioControllerApiWrapper : IGpioControllerProvider {
            private readonly IntPtr impl;
            private IDictionary pinMap;
            private NativeEventDispatcher dispatcher;

            public NativeApi Api { get; }

            public GpioControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            public extern int PinCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void OpenPin(int pin);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClosePin(int pin);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern TimeSpan GetDebounceTimeout(int pin);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetDebounceTimeout(int pin, TimeSpan value);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern GpioPinDriveMode GetDriveMode(int pin);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetDriveMode(int pin, GpioPinDriveMode value);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern GpioPinValue Read(int pin);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Write(int pin, GpioPinValue value);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool IsDriveModeSupported(int pin, GpioPinDriveMode mode);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetPinChangedEdge(int pin, GpioPinEdge edge);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void ClearPinChangedEdge(int pin);

            private static string GetEventKey(string apiName, long pin) => $"{apiName}\\{pin}";

            public void SetPinChangedHandler(int pin, GpioPinEdge edge, GpioPinValueChangedEventHandler value) {
                if (this.dispatcher == null) {
                    this.dispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Gpio.PinChanged");
                    this.pinMap = new Hashtable();

                    this.dispatcher.OnInterrupt += this.OnDispatcher;
                }

                var key = GpioControllerApiWrapper.GetEventKey(this.Api.Name, pin);

                lock (this.pinMap)
                    this.pinMap[key] = value;

                this.SetPinChangedEdge(pin, edge);
            }

            public void ClearPinChangedHandler(int pin) {
                var key = GpioControllerApiWrapper.GetEventKey(this.Api.Name, pin);

                lock (this.pinMap)
                    if (this.pinMap.Contains(key))
                        this.pinMap.Remove(key);

                this.ClearPinChangedEdge(pin);
            }

            private void OnDispatcher(string api, long d0, long d1, long d2, IntPtr d3, DateTime ts) {
                var handler = default(GpioPinValueChangedEventHandler);
                var key = GpioControllerApiWrapper.GetEventKey(api, d0);
                var edge = d1 != 0 ? GpioPinEdge.RisingEdge : GpioPinEdge.FallingEdge;

                lock (this.pinMap)
                    if (this.pinMap.Contains(key))
                        handler = (GpioPinValueChangedEventHandler)this.pinMap[key];

                if (handler != null)
                    handler?.Invoke(null, new GpioPinValueChangedEventArgs(edge, ts));
            }
        }
    }
}
