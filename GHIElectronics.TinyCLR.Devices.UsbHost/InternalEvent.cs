
using System;
using System.Collections;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.UsbHost {
    internal static class InternalEvent {
        private static NativeEventDispatcher dispatcher;

        internal delegate void InternalEventEventHandler(object sender, InternalEventEventArgs e);

        internal static event InternalEventEventHandler ControllerAreaNetworkActivity;

        internal static event InternalEventEventHandler RuntimeLoadableProceduresEvent;

        internal static event InternalEventEventHandler UsbDeviceConnected;

        internal static event InternalEventEventHandler UsbDeviceConnectionFailed;

        internal static event InternalEventEventHandler UsbDeviceDisconnected;

        private enum EventType : byte {
            UsbDeviceDisconnected = 2,
            UsbDeviceConnectionFailed = 3,
            ControllerAreaNetworkActivity = 4,
            RuntimeLoadableProceduresEvent = 5,
            UsbDeviceConnected = 100
        }

        static InternalEvent() {
            

            InternalEvent.dispatcher = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.UsbHost.Event");
            //InternalEvent.dispatcher.OnInterrupt += InternalEvent.OnEvent;

            InternalEvent.dispatcher.OnInterrupt += Dispatcher_OnInterrupt;
        }

        private static void Dispatcher_OnInterrupt(string data0, long data1, long data2, long data3, IntPtr data4, DateTime timestamp) {
            //throw new NotImplementedException();
            var eventId = (uint)data1;

            switch ((EventType)eventId) {
                case EventType.ControllerAreaNetworkActivity: InternalEvent.RaiseEvent(InternalEvent.ControllerAreaNetworkActivity, eventId, (uint)data2); break;
                case EventType.RuntimeLoadableProceduresEvent: InternalEvent.RaiseEvent(InternalEvent.RuntimeLoadableProceduresEvent, eventId, (uint)data2); break;
                case EventType.UsbDeviceConnectionFailed: InternalEvent.RaiseEvent(InternalEvent.UsbDeviceConnectionFailed, eventId, (uint)data2); break;
                case EventType.UsbDeviceDisconnected: InternalEvent.RaiseEvent(InternalEvent.UsbDeviceDisconnected, eventId, (uint)data2); break;
                default: InternalEvent.RaiseEvent(InternalEvent.UsbDeviceConnected, eventId, (uint)data2); break;
            }
        }

        private static void OnEvent(uint eventId, uint data2, DateTime time) {
            switch ((EventType)eventId) {
                case EventType.ControllerAreaNetworkActivity: InternalEvent.RaiseEvent(InternalEvent.ControllerAreaNetworkActivity, eventId, data2); break;
                case EventType.RuntimeLoadableProceduresEvent: InternalEvent.RaiseEvent(InternalEvent.RuntimeLoadableProceduresEvent, eventId, data2); break;
                case EventType.UsbDeviceConnectionFailed: InternalEvent.RaiseEvent(InternalEvent.UsbDeviceConnectionFailed, eventId, data2); break;
                case EventType.UsbDeviceDisconnected: InternalEvent.RaiseEvent(InternalEvent.UsbDeviceDisconnected, eventId, data2); break;
                default: InternalEvent.RaiseEvent(InternalEvent.UsbDeviceConnected, eventId, data2); break;
            }
        }

        private static void RaiseEvent(InternalEventEventHandler e, uint eventId, uint data) => e?.Invoke(null, new InternalEventEventArgs(eventId, data));

        internal class InternalEventEventArgs : EventArgs {
            private uint eventId;
            private uint data;

            internal uint EventId => this.eventId;

            internal uint Data => this.data;

            internal InternalEventEventArgs(uint eventId, uint data) {
                this.eventId = eventId;
                this.data = data;
            }
        }
    }
}
