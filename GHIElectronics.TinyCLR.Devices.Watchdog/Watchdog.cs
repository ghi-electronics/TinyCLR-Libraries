using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Watchdog.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Watchdog
{
    public sealed class WatchdogController : IDisposable {
        public IWatchdogControllerProvider Provider { get; }

        private WatchdogController(IWatchdogControllerProvider provider) => this.Provider = provider;

        public static WatchdogController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.WatchdogController) is WatchdogController c ? c : WatchdogController.FromName(NativeApi.GetDefaultName(NativeApiType.WatchdogController));
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
            private readonly IntPtr impl;

            public NativeApi Api { get; }

            public WatchdogControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();           

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable(uint timeout);

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Reset();

            public extern uint GetMaxTimeout { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            public extern bool IsEnabled { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        }
    }
}
