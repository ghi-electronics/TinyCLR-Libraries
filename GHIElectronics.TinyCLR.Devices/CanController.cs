namespace GHIElectronics.TinyCLR.Devices {
    public sealed class CanController {
        private ICanControllerProvider provider;

        public delegate void MessageReceivedEventHandler(CanController sender, int count);
        public delegate void ErrorReceivedEventHandler(CanController sender, CanError error);

        private CanEventListener canEventListener;

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


        public void SetTiming(CanTimings timing) {
            this.provider.Acquire();
            this.provider.SetTiming(timing);
        }

        public bool Reset() => this.provider.Reset();

        public int ReadMessages(CanMessage[] messages, int offset, int count) => this.provider.ReadMessages(messages, offset, count);

        public int SendMessages(CanMessage[] messages, int offset, int count) => this.provider.SendMessages(messages, offset, count);

        public int ReceivedMessageCount() => this.provider.ReceivedMessageCount();

        void SetExplicitFilters(uint[] filters) => this.provider.SetExplicitFilters(filters);

        void SetGroupFilters(uint[] lowerBounds, uint[] upperBounds) => this.provider.SetGroupFilters(lowerBounds, upperBounds);

        void DiscardIncomingMessages() => this.provider.DiscardIncomingMessages();

        bool CanSend() => this.provider.CanSend();

        public event MessageReceivedEventHandler MessageAvailable;
        public event ErrorReceivedEventHandler ErrorReceived;

        internal void OnMessageReceived(int count) => MessageAvailable?.Invoke(this, count);
        internal void OnErrorReceived(CanError error) => ErrorReceived?.Invoke(this, error);

    }
}
