using System;

namespace GHIElectronics.TinyCLR.Native {
    public sealed class Interop {
        private Interop() { }

        public static Interop[] FindAll() => new Interop[0];

        public string Name => string.Empty;
        public uint Checksum => 0;
        public IntPtr Methods => IntPtr.Zero;
    }
}
