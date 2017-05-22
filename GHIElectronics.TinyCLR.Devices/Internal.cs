using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Devices.Internal {
    internal class Port {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public bool ReservePin(Cpu.Pin pin, bool fReserve);
    }

    internal static class Cpu {
        public enum Pin : int {
        }
    }

    internal delegate void NativeEventHandler(uint data1, uint data2, DateTime time);

    internal class NativeEventDispatcher : IDisposable {
        protected NativeEventHandler m_threadSpawn = null;
        protected NativeEventHandler m_callbacks = null;
        protected bool m_disposed = false;
        private object m_NativeEventDispatcher;

        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public NativeEventDispatcher(string strDriverName, ulong drvData);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public virtual void EnableInterrupt();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern public virtual void DisableInterrupt();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern protected virtual void Dispose(bool disposing);

        //--//

        ~NativeEventDispatcher() {
            Dispose(false);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public virtual void Dispose() {
            if (!this.m_disposed) {
                Dispose(true);

                GC.SuppressFinalize(this);

                this.m_disposed = true;
            }
        }

        public event NativeEventHandler OnInterrupt {
            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            add {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                NativeEventHandler callbacksOld = this.m_callbacks;
                NativeEventHandler callbacksNew = (NativeEventHandler)Delegate.Combine(callbacksOld, value);

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

            [MethodImplAttribute(MethodImplOptions.Synchronized)]
            remove {
                if (this.m_disposed) {
                    throw new ObjectDisposedException();
                }

                NativeEventHandler callbacksOld = this.m_callbacks;
                NativeEventHandler callbacksNew = (NativeEventHandler)Delegate.Remove(callbacksOld, value);

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

        private void MultiCastCase(uint port, uint state, DateTime time) => this.m_callbacks?.Invoke(port, state, time);
    }
}
