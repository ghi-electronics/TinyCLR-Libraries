using System.Runtime.InteropServices;

namespace GHIElectronics.TinyCLR.Devices {
    public sealed class CanController {
        private ICanControllerProvider provider;

        public delegate void MessageReceivedEventHandler(CanController sender, int count);
        public delegate void ErrorReceivedEventHandler(CanController sender, CanError error);

        private NativeEventDispatcher nativeMessageAvailableEvent;
        private NativeEventDispatcher nativeErrorEvent;

        public int ReadBufferSize {
            get => this.ReadBufferSize;
            set {
                this.ReadBufferSize = value;
                this.provider.SetReadBufferSize(value);
            }
        }

        private int controllerId;

        private bool enable = false;

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

        public void SetTimings(CanTimings timing) {
            if (!this.enable) {
                this.provider.Acquire();
                this.nativeMessageAvailableEvent = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.MessageReceived");
                this.nativeErrorEvent = NativeEventDispatcher.GetDispatcher("GHIElectronics.TinyCLR.NativeEventNames.Can.ErrorReceived");

                this.nativeMessageAvailableEvent.OnInterrupt += (pn, ci, d0, d1, d2, ts) => { if (this.controllerId == ci) MessageAvailable?.Invoke(this, (int)d0); };
                this.nativeErrorEvent.OnInterrupt += (pn, ci, d0, d1, d2, ts) => { if (this.controllerId == ci) ErrorReceived?.Invoke(this, (CanError)d0); };

                this.enable = true;
            }

            this.provider.SetTimings(timing);
        }

        public bool Reset() => this.provider.Reset();

        public int ReadMessages(CanMessage[] messages, int offset, int count) => this.provider.ReadMessages(messages, offset, count);

        public CanMessage ReadMessage() {
            if (this.GetUnreadMessageCount != 0) {

                var message = new CanMessage[1] { new CanMessage() };

                ReadMessages(message, 0, 1);

                return message[0];
            }

            return null;
        }

        public int WriteMessages(CanMessage[] messages, int offset, int count) => this.provider.WriteMessages(messages, offset, count);

        public bool WriteMessage(CanMessage message) {
            if (this.IsSendingAllowed) {
                var messages = new CanMessage[1];

                messages[0] = message;

                this.provider.WriteMessages(messages, 0, 1);

                return true;
            }

            return false;
        }

        public int GetUnreadMessageCount => this.provider.GetUnreadMessageCount();

        public void SetExplicitFilters(uint[] filters) => this.provider.SetExplicitFilters(filters);

        public void SetGroupFilters(uint[] lowerBounds, uint[] upperBounds) => this.provider.SetGroupFilters(lowerBounds, upperBounds);

        public void DiscardUnreadMessages() => this.provider.DiscardUnreadMessages();

        public bool IsSendingAllowed => this.provider.IsSendingAllowed();

        public int GetReadErrorCount => this.provider.GetReadErrorCount();

        public int GetWriteErrorCount => this.provider.GetWriteErrorCount();

        public uint GetSourceClock => this.provider.GetSourceClock();

        public event MessageReceivedEventHandler MessageAvailable;
        public event ErrorReceivedEventHandler ErrorReceived;
    }
}
