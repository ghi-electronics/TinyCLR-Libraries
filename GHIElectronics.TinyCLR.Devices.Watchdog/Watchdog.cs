using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Watchdog.Provider;
using GHIElectronics.TinyCLR.Native;

namespace GHIElectronics.TinyCLR.Devices.Watchdog
{
    /// <summary>
    /// Independent watchdog timer. <see cref="Enable(uint)"/> with a timeout and
    /// call <see cref="Reset"/> periodically — if the timer ever expires without
    /// being reset, the chip reboots. Useful as a failsafe against firmware lockups.
    /// </summary>
    public class WatchdogController : IDisposable {
        /// <summary>The low-level provider backing this controller.</summary>
        public IWatchdogControllerProvider Provider { get; }

        private WatchdogController(IWatchdogControllerProvider provider) => this.Provider = provider;

        /// <summary>Returns the default watchdog for this device.</summary>
        public static WatchdogController GetDefault() => NativeApi.GetDefaultFromCreator(NativeApiType.WatchdogController) is WatchdogController c ? c : WatchdogController.FromName(NativeApi.GetDefaultName(NativeApiType.WatchdogController));
        /// <summary>Returns a watchdog identified by its native API name.</summary>
        public static WatchdogController FromName(string name) => WatchdogController.FromProvider(new WatchdogControllerApiWrapper(NativeApi.Find(name, NativeApiType.WatchdogController)));
        /// <summary>Creates a controller from a custom <see cref="IWatchdogControllerProvider"/>.</summary>
        public static WatchdogController FromProvider(IWatchdogControllerProvider provider) => new WatchdogController(provider);

        /// <summary>Releases the underlying provider.</summary>
        public void Dispose() => this.Provider.Dispose();
        /// <summary>Largest legal value (in milliseconds) for the <see cref="Enable(uint)"/> timeout argument.</summary>
        public uint GetMaxTimeout => this.Provider.GetMaxTimeout;
        /// <summary>True once <see cref="Enable(uint)"/> has been called.</summary>
        public bool IsEnabled => this.Provider.IsEnabled;
        /// <summary>
        /// Starts the watchdog. From this point on, <see cref="Reset"/> must be
        /// called more often than <paramref name="timeout"/> or the device will reboot.
        /// On many chips the watchdog cannot be disabled once enabled.
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds (must be &gt; 0 and ≤ <see cref="GetMaxTimeout"/>).</param>
        public void Enable(uint timeout) {
            if (timeout == 0 || timeout > this.GetMaxTimeout)
                throw new ArgumentOutOfRangeException(nameof(timeout));

            this.Provider.Enable(timeout);
        }
        /// <summary>Disables the watchdog (only supported on hardware that allows it).</summary>
        public void Disable() => this.Provider.Disable();
        /// <summary>Re-arms the watchdog. Must be called before the timeout elapses.</summary>
        public void Reset() => this.Provider.Reset();
    }

    namespace Provider {
        /// <summary>Provider contract for a watchdog controller.</summary>
        public interface IWatchdogControllerProvider : IDisposable {
            /// <summary>Largest legal timeout in milliseconds.</summary>
            uint GetMaxTimeout { get; }
            /// <summary>True once the watchdog is running.</summary>
            bool IsEnabled { get; }
            /// <summary>Starts the watchdog with the given timeout in milliseconds.</summary>
            void Enable(uint timeout);
            /// <summary>Disables the watchdog where the hardware permits it.</summary>
            void Disable();
            /// <summary>Re-arms the watchdog.</summary>
            void Reset();
        }

        /// <summary>Concrete <see cref="IWatchdogControllerProvider"/> backed by the native TinyCLR HAL.</summary>
        public sealed class WatchdogControllerApiWrapper : IWatchdogControllerProvider {
            private readonly IntPtr impl;

            /// <summary>The underlying native API descriptor.</summary>
            public NativeApi Api { get; }

            /// <summary>Wraps the given native API as a provider.</summary>
            public WatchdogControllerApiWrapper(NativeApi api) {
                this.Api = api;

                this.impl = api.Implementation;

                this.Acquire();
            }

            /// <summary>Releases the native controller.</summary>
            public void Dispose() => this.Release();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Acquire();

            [MethodImpl(MethodImplOptions.InternalCall)]
            private extern void Release();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Enable(uint timeout);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Disable();

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.InternalCall)]
            public extern void Reset();

            /// <inheritdoc/>
            public extern uint GetMaxTimeout { [MethodImpl(MethodImplOptions.InternalCall)] get; }

            /// <inheritdoc/>
            public extern bool IsEnabled { [MethodImpl(MethodImplOptions.InternalCall)] get; }
        }
    }
}
