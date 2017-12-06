using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.Devices {
    public sealed class CanController {
        private ICanControllerProvider provider;

        public delegate void MessageReceivedEventHandler(CanController sender, int count);
        public delegate void ErrorReceivedEventHandler(CanController sender, CanError error);

        private NativeEventDispatcher nativeMessageAvailableEvent;
        private NativeEventDispatcher nativeErrorEvent;

        internal int receiveBufferSize = 128;

        internal int controllerId;

        internal bool enable = false;

        internal CanController(ICanControllerProvider provider) => this.provider = provider;

        public static CanController[] GetControllers(ICanProvider provider) {
            var providers = provider.GetControllers();
            var controllers = new CanController[providers.Length];

            for (var i = 0; i < providers.Length; i++) {
                controllers[i] = new CanController(providers[i]) {
                    controllerId = i
                };

            }

            return controllers;
        }

        public void ApplySettings(CanTimings timing) {
            if (!this.enable) {
                this.provider.Acquire();
                this.nativeMessageAvailableEvent = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.MessageReceived");
                this.nativeErrorEvent = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.ErrorReceived");

                this.nativeMessageAvailableEvent.OnInterrupt += (pn, ci, d0, d1, d2, ts) => { if (this.controllerId == ci) MessageAvailable?.Invoke(this, (int)d0); };
                this.nativeErrorEvent.OnInterrupt += (pn, ci, d0, d1, d2, ts) => { if (this.controllerId == ci) ErrorReceived?.Invoke(this, (CanError)d0); };

                this.enable = true;
            }

            this.provider.SetTiming(timing);



        }

        public bool Reset() => this.provider.Reset();

        public int ReadMessages(CanMessage[] messages, int offset, int count) => this.provider.ReadMessages(messages, offset, count);

        public CanMessage ReadMessage() {
            if (this.ReceivedMessageCount != 0) {

                var message = new CanMessage[1] { new CanMessage() };

                ReadMessages(message, 0, 1);

                return message[0];
            }

            return null;
        }

        public int SendMessages(CanMessage[] messages, int offset, int count) => this.provider.SendMessages(messages, offset, count);

        public bool SendMessage(CanMessage message) {
            if (this.CanSend) {
                var messages = new CanMessage[1];

                messages[0] = message;

                this.provider.SendMessages(messages, 0, 1);

                return true;
            }

            return false;
        }

        public int ReceivedMessageCount => this.provider.ReceivedMessageCount();

        public void SetExplicitFilters(uint[] filters) => this.provider.SetExplicitFilters(filters);

        public void SetGroupFilters(uint[] lowerBounds, uint[] upperBounds) => this.provider.SetGroupFilters(lowerBounds, upperBounds);

        public void DiscardIncomingMessages() => this.provider.DiscardIncomingMessages();

        public bool CanSend => this.provider.CanSend();

        public int ReceiveErrorCount => this.provider.ReceiveErrorCount();

        public int TransmitErrorCount => this.provider.TransmitErrorCount();

        public uint GetSourceClock => this.provider.GetSourceClock();

        public int ReceiveBufferSize {
            get => this.receiveBufferSize;
            set {
                this.receiveBufferSize = value;
                this.provider.SetReceiveBufferSize(this.receiveBufferSize);
            }
        }

        public event MessageReceivedEventHandler MessageAvailable;
        public event ErrorReceivedEventHandler ErrorReceived;
    }
}
