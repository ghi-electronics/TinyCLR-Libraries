using System;
using System.Collections;

namespace GHIElectronics.TinyCLR.Native {
    public delegate void NativeEventHandler(string data0, long data1, long data2, long data3, IntPtr data4, DateTime timestamp);

    // Public surface mirrors the impl. The impl's private ctor and
    // Enable/DisableInterrupt/Dispose(bool) are all extern InternalCall —
    // here they're plain no-ops. Event handlers get stored via add/remove
    // but are never invoked (no native interrupt source on Desktop).
    public sealed class NativeEventDispatcher : IDisposable {
        private static Hashtable instances = new Hashtable();

        private NativeEventHandler m_callbacks = null;
        private bool m_disposed = false;
        private string name;

        private NativeEventDispatcher(string name) { }

        public void EnableInterrupt() { }
        public void DisableInterrupt() { }
        private void Dispose(bool disposing) { }

        ~NativeEventDispatcher() => this.Dispose(false);

        public void Dispose() {
            if (!this.m_disposed) {
                NativeEventDispatcher.instances.Remove(this.name);
                this.Dispose(true);
                GC.SuppressFinalize(this);
                this.m_disposed = true;
            }
        }

        public static NativeEventDispatcher GetDispatcher(string name) {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (NativeEventDispatcher.instances.Contains(name))
                return (NativeEventDispatcher)NativeEventDispatcher.instances[name];

            var inst = new NativeEventDispatcher(name) { name = name };
            NativeEventDispatcher.instances[name] = inst;
            return inst;
        }

        public event NativeEventHandler OnInterrupt {
            add { this.m_callbacks = (NativeEventHandler)Delegate.Combine(this.m_callbacks, value); }
            remove { this.m_callbacks = (NativeEventHandler)Delegate.Remove(this.m_callbacks, value); }
        }
    }
}
