using System;
using System.Runtime.CompilerServices;

namespace GHIElectronics.TinyCLR.Native {
    public static class SystemTime {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SetSystemTime(ulong utcTicks, int timezoneOffsetMinutes);
    }
}
