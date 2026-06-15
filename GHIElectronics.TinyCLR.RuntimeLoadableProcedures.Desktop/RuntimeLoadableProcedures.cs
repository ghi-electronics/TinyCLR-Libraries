using System;

namespace GHIElectronics.TinyCLR.RuntimeLoadableProcedures {
    // Public surface mirrors the impl. All operations are safe no-ops on
    // Desktop: ElfImage stores the byte[] but performs no real load; symbol
    // lookups return 0; NativeFunction.Invoke returns 0; NativeEvent never
    // fires. Lets dual-mode app code construct and use the API without
    // throwing on PC — including for null/invalid arguments that would
    // throw on device. Dual-mode contract: PC runs to completion.
    public static class RuntimeLoadableProcedures {

        public delegate void NativeEventHandler(uint data);

#pragma warning disable 0067
        public static event NativeEventHandler NativeEvent;
#pragma warning restore 0067


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

            public ElfImage(byte[] elfImageData) => this.imageData = elfImageData;

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

            public uint FindSymbolAddress(string name, SymbolType type) => 0;

            public NativeFunction FindFunction(string name) => new NativeFunction(0);

            public void InitializeBssRegion() { }

            public void InitializeBssRegion(string startSymbolName, string endSymbolName) { }

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
