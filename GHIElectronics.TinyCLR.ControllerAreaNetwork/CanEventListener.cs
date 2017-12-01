using System;
using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.ControllerAreaNetwork {
    public class CanEventListener {
        private NativeEventDispatcher dispatcherMessageReceived;
        private NativeEventDispatcher dispatcherErrorReceived;

        public CanController controller;

        public CanEventListener() {
            this.dispatcherMessageReceived = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.MessageReceived");
            this.dispatcherErrorReceived = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.ErrorReceived");

            this.dispatcherMessageReceived.OnInterrupt += this.Dispatcher_OnMessageReceivedInterrupt;
            this.dispatcherErrorReceived.OnInterrupt += this.Dispatcher_OnErrorReceivedInterrupt;
        }

        private void Dispatcher_OnMessageReceivedInterrupt(string apiName, uint implementationIndex, ulong data0, ulong data1, IntPtr data2, DateTime timestamp) => this.controller.MessageReceived((int)data0);
        private void Dispatcher_OnErrorReceivedInterrupt(string apiName, uint implementationIndex, ulong data0, ulong data1, IntPtr data2, DateTime timestamp) => this.controller.ErrorReceived((Error)data0);

    }
}
