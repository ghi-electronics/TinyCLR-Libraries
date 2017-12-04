namespace GHIElectronics.TinyCLR.Devices {
    public sealed class CanController {
        private ICanControllerProvider provider;

        public delegate void MessageReceivedEventHandler(CanController sender, int count);
        public delegate void ErrorReceivedEventHandler(CanController sender, CanError error);

        private CanEventListener canEventListener;

        int receiveBufferSize = 128;

        internal CanController(ICanControllerProvider provider) => this.provider = provider;

        public static CanController[] GetControllers(ICanProvider provider) {
            var providers = provider.GetControllers();
            var controllers = new CanController[providers.Length];

            for (var i = 0; i < providers.Length; i++) {
                controllers[i] = new CanController(providers[i]) {
                    canEventListener = new CanEventListener()
                };
                controllers[i].canEventListener.controller = controllers[i];
            }

            return controllers;
        }

        public void ApplySettings(CanTimings timing, int receivingBufferSize) {
            this.provider.Acquire();
            this.ReceiveBufferSize = receivingBufferSize;
            this.provider.SetTiming(timing);
        }

        public void ApplySettings(CanTimings timing) => ApplySettings(timing, this.receiveBufferSize);

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

        internal void OnMessageReceived(int count) => MessageAvailable?.Invoke(this, count);
        internal void OnErrorReceived(CanError error) => ErrorReceived?.Invoke(this, error);

    }
}
