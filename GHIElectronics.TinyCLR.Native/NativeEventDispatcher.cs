using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    /// <summary>Handler signature for <see cref="NativeEventDispatcher.OnInterrupt"/>.</summary>
    public delegate void NativeEventHandler(string data0, long data1, long data2, long data3, IntPtr data4, DateTime timestamp);

    /// <summary>
    /// Marshals native ISR events to managed handlers. One dispatcher exists per
    /// well-known event name (e.g. <c>GHIElectronics.TinyCLR.NativeEventNames.Gpio.PinChanged</c>);
    /// retrieve the singleton with <see cref="GetDispatcher(string)"/> and subscribe
    /// to <see cref="OnInterrupt"/>. The first subscription enables the native
    /// interrupt; removing the last one disables it.
    /// </summary>
    public sealed class NativeEventDispatcher : IDisposable {
        private static Hashtable instances = new Hashtable();

        private NativeEventHandler m_threadSpawn = null;
        private NativeEventHandler m_callbacks = null;
        private bool m_disposed = false;
#pragma warning disable CS0169
        private object m_NativeEventDispatcher;
#pragma warning restore CS0169
        private string name;

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private NativeEventDispatcher(string name);

        /// <summary>Manually arms the native interrupt source. Usually unnecessary — <see cref="OnInterrupt"/> handles it.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        extern public void EnableInterrupt();

        /// <summary>Manually disarms the native interrupt source.</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        extern public void DisableInterrupt();

        [MethodImpl(MethodImplOptions.InternalCall)]
        extern private void Dispose(bool disposing);

        /// <summary>Finalizer; ensures the native dispatcher is released.</summary>
        ~NativeEventDispatcher() {
            Dispose(false);
        }

        /// <summary>Releases the native dispatcher and removes it from the per-name registry.</summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Dispose() {
            if (!this.m_disposed) {
                NativeEventDispatcher.instances.Remove(this.name);

                Dispose(true);

                GC.SuppressFinalize(this);

                this.m_disposed = true;
            }
        }

        /// <summary>
        /// Returns the singleton dispatcher for a given event name, creating it on
        /// first request. All managed subscribers to the same name share one dispatcher.
        /// </summary>
        public static NativeEventDispatcher GetDispatcher(string name) {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (NativeEventDispatcher.instances.Contains(name))
                return (NativeEventDispatcher)NativeEventDispatcher.instances[name];

            var inst = new NativeEventDispatcher(name) { name = name };

            NativeEventDispatcher.instances[name] = inst;

            return inst;
        }

        /// <summary>
        /// Raised by the firmware when the underlying native event fires. The first
        /// subscription enables the native interrupt; removing the last unsubscribes
        /// and disables it. Multi-cast subscribers are dispatched on the same thread.
        /// </summary>
        public event NativeEventHandler OnInterrupt {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                var callbacksOld = this.m_callbacks;
                var callbacksNew = (NativeEventHandler)Delegate.Combine(callbacksOld, value);

                try {
                    this.m_callbacks = callbacksNew;

                    if (callbacksNew != null) {
                        if (callbacksOld == null) {
                            EnableInterrupt();
                        }

                        if (callbacksNew.Equals(value) == false) {
                            callbacksNew = new NativeEventHandler(this.MultiCastCase);
                        }
                    }

                    this.m_threadSpawn = callbacksNew;
                }
                catch {
                    this.m_callbacks = callbacksOld;

                    if (callbacksOld == null) {
                        DisableInterrupt();
                    }

                    throw;
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            remove {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                var callbacksOld = this.m_callbacks;
                var callbacksNew = (NativeEventHandler)Delegate.Remove(callbacksOld, value);

                try {
                    this.m_callbacks = (NativeEventHandler)callbacksNew;

                    if (callbacksNew == null && callbacksOld != null) {
                        DisableInterrupt();
                    }
                }
                catch {
                    this.m_callbacks = callbacksOld;

                    throw;
                }
            }
        }

        private void MultiCastCase(string providerName, long data0, long data1, long data2, IntPtr data3, DateTime timestamp) => this.m_callbacks?.Invoke(providerName, data0, data1, data2, data3, timestamp);
    }
}
