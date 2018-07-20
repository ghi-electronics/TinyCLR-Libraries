using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GHIElectronics.TinyCLR.Devices.Gpio.Provider;

namespace GHIElectronics.TinyCLR.Devices.Gpio {
    public delegate void GpioPinValueChangedEventHandler(GpioPin sender, GpioPinValueChangedEventArgs e);

    public sealed class GpioPinValueChangedEventArgs : EventArgs {
        public GpioPinEdge Edge { get; }

        internal GpioPinValueChangedEventArgs(GpioPinEdge edge) => this.Edge = edge;
    }

    public enum GpioPinDriveMode {
        Input = 0,
        Output = 1,
        InputPullUp = 2,
        InputPullDown = 3,
        OutputOpenDrain = 4,
        OutputOpenDrainPullUp = 5,
        OutputOpenSource = 6,
        OutputOpenSourcePullDown = 7,
    }

    public enum GpioPinValue {
        Low = 0,
        High = 1,
    }

    [Flags]
    public enum GpioPinEdge {
        FallingEdge = 0x01,
        RisingEdge = 0x02,
    }

    public sealed class GpioController : IDisposable {
        public IGpioControllerProvider Provider { get; }

        private GpioController(IGpioControllerProvider provider) => this.Provider = provider;

        public static GpioController GetDefault() => Api.GetDefaultFromCreator(ApiType.GpioController) is GpioController c ? c : GpioController.FromName(Api.GetDefaultName(ApiType.GpioController));
        public static GpioController FromName(string name) => GpioController.FromProvider(new GpioControllerApiWrapper(Api.Find(name, ApiType.GpioController)));
        public static GpioController FromProvider(IGpioControllerProvider provider) => new GpioController(provider);

        public void Dispose() => this.Provider.Dispose();


        public GpioPin OpenPin(int pinNumber) => this.OpenPin((uint)pinNumber);

        public GpioPin OpenPin(uint pinNumber) => new GpioPin(this, pinNumber);

        public GpioPin[] OpenPins(params uint[] pinNumbers) {
            var res = new GpioPin[pinNumbers.Length];
            var i = 0U;

            for (; i < pinNumbers.Length; i++) {
                try {
                    res[i] = this.OpenPin(i);
                }
                catch {
                    for (var ii = 0; ii < i; ii++)
                        res[ii].Dispose();

                    throw;
                }
            }

            return res;
        }

        public bool TryOpenPin(uint pinNumber, out GpioPin pin) {
            try {
                pin = this.OpenPin(pinNumber);
                return true;
            }
            catch {
                pin = null;
                return false;
            }
        }

        public bool TryOpenPins(out GpioPin[] pins, params uint[] pinNumbers) {
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

        public uint PinNumber { get; }
        public GpioController Controller { get; }

        internal GpioPin(GpioController controller, uint pinNumber) {
            this.PinNumber = pinNumber;
            this.Controller = controller;

            this.Controller.Provider.OpenPin(pinNumber);
        }

        public void Dispose() => this.Controller.Provider.ClosePin(this.PinNumber);

        public bool IsDriveModeSupported(uint pin, GpioPinDriveMode mode) => this.Controller.Provider.IsDriveModeSupported(pin, mode);

        public TimeSpan DebounceTimeout { get => this.Controller.Provider.GetDebounceTimeout(this.PinNumber); set => this.Controller.Provider.SetDebounceTimeout(this.PinNumber, value); }
        public GpioPinDriveMode DriveMode { get => this.Controller.Provider.GetDriveMode(this.PinNumber); set => this.Controller.Provider.SetDriveMode(this.PinNumber, value); }

        public GpioPinDriveMode GetDriveMode() => this.DriveMode;
        public void SetDriveMode(GpioPinDriveMode value) => this.DriveMode = value;

        public GpioPinValue Read() => this.Controller.Provider.Read(this.PinNumber);

        public void Write(GpioPinValue value) {
            var init = this.Read();

            this.Controller.Provider.Write(this.PinNumber, value);

            if (init != value)
                this.OnValueChanged(this, new GpioPinValueChangedEventArgs(value == GpioPinValue.High ? GpioPinEdge.RisingEdge : GpioPinEdge.FallingEdge));
        }

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
            uint PinCount { get; }

            void OpenPin(uint pin);
            void ClosePin(uint pin);

            bool IsDriveModeSupported(uint pin, GpioPinDriveMode mode);
            void SetPinChangedHandler(uint pin, GpioPinEdge edge, GpioPinValueChangedEventHandler value);
            void ClearPinChangedHandler(uint pin);

            TimeSpan GetDebounceTimeout(uint pin);
            void SetDebounceTimeout(uint pin, TimeSpan value);
            GpioPinDriveMode GetDriveMode(uint pin);
            void SetDriveMode(uint pin, GpioPinDriveMode value);
            GpioPinValue Read(uint pin);
            void Write(uint pin, GpioPinValue value);
        }

        public sealed class GpioControllerApiWrapper : IGpioControllerProvider {
            private readonly IntPtr impl;
            private IDictionary pinMap;
            private NativeEventDispatcher dispatcher;

            public Api Api { get; }

            public GpioControllerApiWrapper(Api api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            public extern uint PinCount { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void OpenPin(uint pin);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void ClosePin(uint pin);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern TimeSpan GetDebounceTimeout(uint pin);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetDebounceTimeout(uint pin, TimeSpan value);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern GpioPinDriveMode GetDriveMode(uint pin);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void SetDriveMode(uint pin, GpioPinDriveMode value);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern GpioPinValue Read(uint pin);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Write(uint pin, GpioPinValue value);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern bool IsDriveModeSupported(uint pin, GpioPinDriveMode mode);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void SetPinChangedEdge(uint pin, GpioPinEdge edge);

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void ClearPinChangedEdge(uint pin);

            private static string GetEventKey(string apiName, ulong pin) => $"{apiName}\\{pin}";

            public void SetPinChangedHandler(uint pin, GpioPinEdge edge, GpioPinValueChangedEventHandler value) {
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

            public void ClearPinChangedHandler(uint pin) {
                var key = GpioControllerApiWrapper.GetEventKey(this.Api.Name, pin);

                lock (this.pinMap)
                    if (this.pinMap.Contains(key))
                        this.pinMap.Remove(key);

                this.ClearPinChangedEdge(pin);
            }

            private void OnDispatcher(string api, ulong d0, ulong d1, ulong d2, IntPtr d3, DateTime ts) {
                var handler = default(GpioPinValueChangedEventHandler);
                var key = GpioControllerApiWrapper.GetEventKey(api, d0);
                var edge = d1 != 0 ? GpioPinEdge.RisingEdge : GpioPinEdge.FallingEdge;

                lock (this.pinMap)
                    if (this.pinMap.Contains(key))
                        handler = (GpioPinValueChangedEventHandler)this.pinMap[key];

                if (handler != null)
                    handler?.Invoke(null, new GpioPinValueChangedEventArgs(edge));
            }
        }
    }
}
