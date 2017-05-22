using System.Collections;
using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.Devices.Gpio.Provider {
    internal class GpioPinEventListener : NativeEventDispatcher {
        private IDictionary pinMap = new Hashtable();

        public GpioPinEventListener() : base("GHIElectronics.TinyCLR.NativeEventNames.Gpio.ValueChanged") => this.OnInterrupt += (e1, e2, ts) => {
            var pin = default(DefaultGpioPinProvider);
            var num = (int)e1;
            var edge = e2 != 0 ? ProviderGpioPinEdge.RisingEdge : ProviderGpioPinEdge.FallingEdge;

            lock (this.pinMap)
                if (this.pinMap.Contains(num))
                    pin = (DefaultGpioPinProvider)this.pinMap[num];

            if (pin != null)
                pin.OnPinChangedInternal(edge);
        };

        public void AddPin(int pinNumber, DefaultGpioPinProvider pin) {
            lock (this.pinMap)
                this.pinMap[pin.PinNumber] = pin;
        }

        public void RemovePin(int pinNumber) {
            lock (this.pinMap)
                if (this.pinMap.Contains(pinNumber))
                    this.pinMap.Remove(pinNumber);
        }
    }
}
