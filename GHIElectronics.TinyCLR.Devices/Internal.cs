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
}
