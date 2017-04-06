using GHIElectronics.TinyCLR.Devices.Internal;
using System;
using System.Collections;

namespace GHIElectronics.TinyCLR.Devices.Gpio.Provider {
    internal class GpioPinEvent : BaseEvent {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        public int PinNumber;
        public ProviderGpioPinEdge Edge;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    }

    internal class GpioPinEventListener : IEventProcessor, IEventListener {
        // Map of pin numbers to GpioPin objects.
        private IDictionary m_pinMap = new Hashtable();

        public GpioPinEventListener() {
            EventSink.AddEventProcessor(EventCategory.Gpio, this);
            EventSink.AddEventListener(EventCategory.Gpio, this);
        }

        public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time) => new GpioPinEvent {
            // Data1 is packed by PostManagedEvent, so we need to unpack the high word.
            PinNumber = (int)(data1 >> 16),
            Edge = (data2 == 0) ? ProviderGpioPinEdge.FallingEdge : ProviderGpioPinEdge.RisingEdge,
        };

        public void InitializeForEventSource() {
        }

        public bool OnEvent(BaseEvent ev) {
            var pinEvent = (GpioPinEvent)ev;
            DefaultGpioPinProvider pin = null;

            lock (this.m_pinMap) {
                if (this.m_pinMap.Contains(pinEvent.PinNumber)) {
                    pin = (DefaultGpioPinProvider)this.m_pinMap[pinEvent.PinNumber];
                }
            }

            // Avoid calling this under a lock to prevent a potential lock inversion.
            if (pin != null) {
                pin.OnPinChangedInternal(pinEvent.Edge);
            }

            return true;
        }

        public void AddPin(int pinNumber, DefaultGpioPinProvider pin) {
            lock (this.m_pinMap) {
                this.m_pinMap[pin.PinNumber] = pin;
            }
        }

        public void RemovePin(int pinNumber) {
            lock (this.m_pinMap) {
                if (this.m_pinMap.Contains(pinNumber)) {
                    this.m_pinMap.Remove(pinNumber);
                }
            }
        }
    }
}
