using System.Collections;
using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.Devices.Gpio.Provider {
    internal class GpioPinEventListener {
        private IDictionary pinMap = new Hashtable();
        private NativeEventDispatcher dispatcher;

        private static string GetKey(string providerName, uint controllerIndex, ulong pin) => $"{providerName}\\{controllerIndex}\\{pin}";

        public GpioPinEventListener() {
            this.dispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Gpio.ValueChanged");
            this.dispatcher.OnInterrupt += (pn, ci, d0, d1, d2, ts) => {
                var pin = default(DefaultGpioPinProvider);
                var key = GpioPinEventListener.GetKey(pn, ci, d0);
                var edge = d1 != 0 ? ProviderGpioPinEdge.RisingEdge : ProviderGpioPinEdge.FallingEdge;

                lock (this.pinMap)
                    if (this.pinMap.Contains(key))
                        pin = (DefaultGpioPinProvider)this.pinMap[key];

                if (pin != null)
                    pin.OnPinChangedInternal(edge);
            };
        }

        public void AddPin(string providerName, uint controllerIndex, DefaultGpioPinProvider pin) {
            var key = GpioPinEventListener.GetKey(providerName, controllerIndex, (ulong)pin.PinNumber);

            lock (this.pinMap)
                this.pinMap[key] = pin;
        }

        public void RemovePin(string providerName, uint controllerIndex, DefaultGpioPinProvider pin) {
            var key = GpioPinEventListener.GetKey(providerName, controllerIndex, (ulong)pin.PinNumber);

            lock (this.pinMap)
                if (this.pinMap.Contains(key))
                    this.pinMap.Remove(key);
        }
    }
}
