using System;

namespace GHIElectronics.TinyCLR.RuntimeLoadableProcedures {
    // Public surface mirrors the impl. All operations are safe no-ops on
    // Desktop: ElfImage stores the byte[] but performs no real load; symbol
    // lookups return 0; NativeFunction.Invoke returns 0; NativeEvent never
    // fires. Lets dual-mode app code construct and use the API without
    // throwing on PC.
    public static class RuntimeLoadableProcedures {

        public delegate void NativeEventHandler(uint data);

        public static event NativeEventHandler NativeEvent;

        // Reference the field once so the compiler doesn't strip it.
        // Never invoked because Desktop has no native event source.
        private static void Touch() => RuntimeLoadableProcedures.NativeEvent?.Invoke(0);


        public sealed class ElfImage : IDisposable {

            public enum SymbolType {
                None = 0,
                Object = 1,
                Function = 2,
                Section = 3,
            }

            private byte[] imageData;
            private bool disposed;

            public uint Address => 0;
            public uint Size => 0;
            public uint RegionCount => 0;

            public ElfImage(byte[] elfImageData) {
                if (elfImageData == null) throw new ArgumentNullException(nameof(elfImageData));
                this.imageData = elfImageData;
            }

            ~ElfImage() => this.Dispose(false);

            public void Dispose() {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing) {
                if (this.disposed) return;
                this.imageData = null;
                this.disposed = true;
            }

            public uint FindSymbolAddress(string name, SymbolType type) {
                if (name == null) throw new ArgumentNullException(nameof(name));
                return 0;
            }

            public NativeFunction FindFunction(string name) {
                if (name == null) throw new ArgumentNullException(nameof(name));
                return new NativeFunction(0);
            }

            public void InitializeBssRegion() { }

            public void InitializeBssRegion(string startSymbolName, string endSymbolName) {
                if (startSymbolName == null) throw new ArgumentNullException(nameof(startSymbolName));
                if (endSymbolName == null) throw new ArgumentNullException(nameof(endSymbolName));
            }

            public void ZeroRegion(uint address, uint length) { }
        }


        public sealed class NativeFunction : IDisposable {
            private uint address;
            private bool disposed;

            public uint Address => this.address;

            public NativeFunction(uint address) => this.address = address;

            ~NativeFunction() => this.Dispose(false);

            public void Dispose() {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing) {
                if (this.disposed) return;
                this.disposed = true;
            }

            public int Invoke(params object[] arguments) => 0;
        }
    }
}
