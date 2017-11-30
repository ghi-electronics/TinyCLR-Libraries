namespace GHIElectronics.TinyCLR.ControllerAreaNetwork {
    public sealed class CanController {
        private ICanControllerProvider provider;

        internal CanController(ICanControllerProvider provider) => this.provider = provider;


        public static CanController[] GetControllers(ICanProvider provider) {
            var providers = provider.GetControllers();
            var controllers = new CanController[providers.Length];

            for (var i = 0; i < providers.Length; i++)
                controllers[i] = new CanController(providers[i]);

            return controllers;
        }

        public void Enable() => this.provider.Enable();

        public void SetSpeed(int propagation, int phase1, int phase2, int brp, int synchronizationJumpWidth, int useMultiBitSampling) => this.provider.SetSpeed(propagation, phase1, phase2, brp, synchronizationJumpWidth, useMultiBitSampling);

        public bool Reset() => this.provider.Reset();

        public int ReadMessages(Message[] messages, int offset, int count) => this.provider.ReadMessages(messages, offset, count);

        public int SendMessages(Message[] messages, int offset, int count) => this.provider.SendMessages(messages, offset, count);

        public int ReceivedMessageCount() => this.provider.ReceivedMessageCount();
    }
}
