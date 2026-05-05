using System;
using GHIElectronics.TinyCLR.Devices.Watchdog.Provider;
using GHIElectronics.TinyCLR.Native;

// Public surface mirrors GHIElectronics.TinyCLR.Devices.Watchdog\Watchdog.cs.
// On Desktop, Enable/Disable/Reset are no-ops — there is no hardware watchdog
// to feed; user code calling Reset() in a loop just runs.
namespace GHIElectronics.TinyCLR.Devices.Watchdog {
    public class WatchdogController : IDisposable {
        public IWatchdogControllerProvider Provider { get; }

        private WatchdogController(IWatchdogControllerProvider provider) => this.Provider = provider;

        public static WatchdogController GetDefault() => WatchdogController.FromName("Simulator");
        public static WatchdogController FromName(string name) => WatchdogController.FromProvider(new WatchdogControllerApiWrapper(NativeApi.Find(name, NativeApiType.WatchdogController)));
        public static WatchdogController FromProvider(IWatchdogControllerProvider provider) => new WatchdogController(provider);

        public void Dispose() => this.Provider.Dispose();
        public uint GetMaxTimeout => this.Provider.GetMaxTimeout;
        public bool IsEnabled => this.Provider.IsEnabled;

        public void Enable(uint timeout) {
            if (timeout == 0 || timeout > this.GetMaxTimeout)
                throw new ArgumentOutOfRangeException(nameof(timeout));

            this.Provider.Enable(timeout);
        }

        public void Disable() => this.Provider.Disable();
        public void Reset() => this.Provider.Reset();
    }

    namespace Provider {
        public interface IWatchdogControllerProvider : IDisposable {
            uint GetMaxTimeout { get; }
            bool IsEnabled { get; }
            void Enable(uint timeout);
            void Disable();
            void Reset();
        }

        public sealed class WatchdogControllerApiWrapper : IWatchdogControllerProvider {
            private bool enabled;

            public NativeApi Api { get; }

            public WatchdogControllerApiWrapper(NativeApi api) => this.Api = api;

            public void Dispose() { }

            // Generic max ~32s (32_000_000 ms — well above any sensible value
            // user code passes, so Enable's range check accepts everything).
            public uint GetMaxTimeout => 32_000_000u;
            public bool IsEnabled => this.enabled;

            public void Enable(uint timeout) => this.enabled = true;
            public void Disable() => this.enabled = false;
            public void Reset() { }
        }
    }
}
